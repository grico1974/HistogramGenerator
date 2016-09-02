using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace HistogramGenerator
{
    public interface IStreamPreprocessor<T>
    {
        IEnumerable<IEnumerable<T>> Process(IEnumerable<T> stream);
    }

    public static class StreamPreprocessors
    {
        public static readonly IStreamPreprocessor<char> GetTextPreprocessor = new TextPreprocessor();

        private class TextPreprocessor: IStreamPreprocessor<char>
        {
            private const char whitespace = '\u0020';
            private static readonly char apostrophe = '\'';

            public TextPreprocessor()
            {
            }

            public IEnumerable<IEnumerable<char>> Process(IEnumerable<char> stream) =>
                removePunctuationSymbols(normalizeWhitespaces(stream)).Split(new[] { '\u0020','\u1680','\u2000','\u2001','\u2002','\u2003',
                                                                                     '\u2004','\u2005','\u2006','\u2007','\u2008','\u2009',
                                                                                     '\u200A','\u202F','\u205F','\u3000','\u2028','\u2029',
                                                                                     '\u0009','\u000A','\u000B','\u000C','\u000D','\u0085',
                                                                                     '\u00A0' }, StringSplitOptions.RemoveEmptyEntries);

            private  IEnumerable<char> normalizeWhitespaces(IEnumerable<char> stream)
            {
                using (var enumerator = LookAheadEnumerator<char>.Create(stream))
                {
                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;

                        if (char.IsWhiteSpace(current))
                        {
                            char peeked;

                            if (enumerator.Peek(out peeked) &&
                                !char.IsWhiteSpace(peeked))
                            {
                                yield return whitespace;
                            }
                        }
                        else
                        {
                            yield return char.ToLowerInvariant(current);
                        }
                    }
                }
            }

            private string removePunctuationSymbols(IEnumerable<char> stream)
            {
                var builder = new StringBuilder();

                using (var enumerator = LookAheadEnumerator<char>.Create(stream))
                {
                    var apostropheSuffix = false;

                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;
                        char peeked;

                        if (char.IsWhiteSpace(current))
                        {
                            apostropheSuffix = false;

                            if (enumerator.Peek(out peeked) &&
                                !char.IsWhiteSpace(peeked))
                            {

                                builder.Append(current);
                            }
                        }
                        else if (!apostropheSuffix)
                        { 
                            if (char.IsPunctuation(current))
                            {
                                if (current == apostrophe)
                                {
                                    apostropheSuffix = true;
                                }
                                else
                                {
                                    if (enumerator.Peek(out peeked) &&
                                        !char.IsWhiteSpace(peeked) &&
                                        !char.IsPunctuation(peeked))
                                    {
                                        builder.Append(whitespace);
                                    }

                                    apostropheSuffix = false;
                                }
                            }
                            else if (char.IsDigit(current))
                            {
                                if (enumerator.Peek(out peeked))
                                {
                                    if (!char.IsDigit(peeked) &&
                                        !char.IsWhiteSpace(peeked))
                                    {
                                        builder.Append(current);
                                        builder.Append(whitespace);
                                    }
                                }
                                else
                                {
                                    builder.Append(current);
                                }
                            }
                            else
                            {
                                builder.Append(current);
                            }
                        }
                    }
                }

                return builder.ToString();
            }
        }

        private class LookAheadEnumerator<T>: IDisposable, IEnumerator<T>
        {
            public static LookAheadEnumerator<T> Create(IEnumerable<T> enumeration) => new LookAheadEnumerator<T>(enumeration);

            private readonly IEnumerator<T> enumerator;
            private bool disposed;
            private bool peeked;
            private T current;

            private LookAheadEnumerator(IEnumerable<T> enumeration)
            {
                Debug.Assert(enumeration != null);
                this.enumerator = enumeration.GetEnumerator();
            }

            public void Dispose()
            {
                if (disposed)
                    return;

                Debug.Assert(enumerator != null);
                enumerator.Dispose();
                disposed = true;
            }

            public T Current => peeked ? current : enumerator.Current;
            object IEnumerator.Current => enumerator.Current;

            public bool MoveNext()
            {
                if (peeked)
                {
                    peeked = false;
                    current = enumerator.Current;
                    return true;
                }

                bool hasMoved = enumerator.MoveNext();

                if (hasMoved)
                {
                    current = enumerator.Current;
                }

                return hasMoved;
            }

            public bool Peek(out T c)
            {
                if (peeked)
                {
                    c = enumerator.Current;
                    return true;
                }

                if (enumerator.MoveNext())
                {
                    peeked = true;
                    c = enumerator.Current;
                }
                else
                {
                    c = default(T);
                }

                return peeked;
            }

            public void Reset()
            {
                peeked = false;
                enumerator.Reset();
            }
        }
    }
}
