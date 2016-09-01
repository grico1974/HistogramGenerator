using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace HistogramGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.BufferHeight = 15000;
            Console.WriteLine("Reading text...");
            /*var str = @"The hen named the fox chicken, and thereafter there were more chickens and hens laughing than foxes smiling
                        and Andrew was happy and so was I and therefore I immeadiately smiled.";*/ 
            var str = File.ReadAllText(@"../../sometext.txt");
            var words = SplitAndFilter(str).ToList();
            Console.WriteLine("Generating histograms...");
            Console.WriteLine();
            Benchmark(words, 1, false); //warmup
            var time = Benchmark(words, 10, true);
            Console.WriteLine($"Text is {str.Length:#,###,##0} characters / {words.Count:#,###,##0} words long.");
            Console.WriteLine($"Word histogram built in an average time of {time} ms");
            Console.ReadLine();
        }

        private static IEnumerable<string> SplitAndFilter(string str)
        {
            var words = str.Split(new[] { ' ', '\n', '\r', '\t', }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                var builder = new StringBuilder(word.Length);

                foreach (var c in word)
                {
                    if (!char.IsSymbol(c) &&
                        !char.IsPunctuation(c) &&
                        !char.IsWhiteSpace(c))
                    {
                        builder.Append(char.ToLowerInvariant(c));
                    }
                    else if (builder.Length > 0)
                    {
                        break;
                    }
                }

                yield return builder.ToString();
            }
        }

        public static int Benchmark(IEnumerable<string> words, int repetitions, bool output = true)
        {
            var histogram = Enumerable.Empty<HistogramGenerator.HistogramItem<char>>();
            var time = 0L;

            for (int i = 0; i < repetitions; i++)
            {
                var watch = Stopwatch.StartNew();
                histogram = HistogramGenerator.GetHistogram(words);
                watch.Stop();
                time += watch.ElapsedMilliseconds;
            }

            if (output)
            {
                foreach (var item in histogram)
                {
                    Console.WriteLine($"{new string(item.Path.ToArray())}: x{item.Count:#,###,##0}");
                }

                Console.WriteLine();
                Console.WriteLine($"Total words in histogram: {histogram.Sum(h => h.Count)::#,###,##0}");
            }

            return (int)Math.Round(time / (double)repetitions, 0);
        }
    }
}
