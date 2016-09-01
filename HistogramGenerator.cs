using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace HistogramGenerator
{
    static class HistogramGenerator
    {
        public static IEnumerable<HistogramItem<T>> GetHistogram<T>(IEnumerable<IEnumerable<T>> stream)
            => Tree<T>.Build(stream).GenerateHistogram();

        public struct HistogramItem<T>
        {
            private readonly ImmutableStack<T> path;

            public int Count { get; }
            public IEnumerable<T> Path => path;

            public HistogramItem(int count, T item)
                : this(count, ImmutableStack<T>.Empty.Push(item))
            {
            }

            private HistogramItem(int count, ImmutableStack<T> stack)
            {
                Debug.Assert(count > 0);
                path = stack;
                Count = count;
            }

            public HistogramItem<T> AppendToPath(T item)
                => new HistogramItem<T>(Count, path.Push(item));
        }

        private class Tree<T>
        {
            public static Tree<T> Build(IEnumerable<IEnumerable<T>> chunks)
            {
                var tree = new Tree<T>();

                foreach (var chunk in chunks)
                {
                    var currentTree = tree;

                    foreach (var item in chunk)
                    {
                        currentTree = currentTree.add(item);
                    }

                    currentTree.Count++;
                }

                return tree;
            }

            private readonly Dictionary<T, Tree<T>> branches;

            public int Count { get; private set; }
            public IReadOnlyDictionary<T, Tree<T>> Branches => branches;

            public Tree()
            {
                branches = new Dictionary<T, Tree<T>>();
                Count = 0;
            }

            private Tree<T> add(T value)
            {
                Tree<T> branch;

                if (!branches.TryGetValue(value, out branch))
                {
                    branch = new Tree<T>();
                    branches.Add(value, branch);
                }

                return branch;
            }

            public IEnumerable<HistogramItem<T>> GenerateHistogram() =>
                getAllCurrentTreeReversedPaths(this).OrderByDescending(h => h.Count);

            private static IEnumerable<HistogramItem<T>> getAllCurrentTreeReversedPaths(Tree<T> tree)
            {
                foreach (var branch in tree.branches)
                {
                    if (branch.Value.Count > 0)
                    {
                        //Make sure we return a valid path included in a longer one. For example: "the" inside "therefore"
                        yield return new HistogramItem<T>(branch.Value.Count, branch.Key);
                    }

                    foreach (var subBranch in getAllCurrentTreeReversedPaths(branch.Value))
                    {
                        yield return subBranch.AppendToPath(branch.Key);
                    }
                }
            }
        }
    }
}

