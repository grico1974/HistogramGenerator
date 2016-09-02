using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace HistogramGenerator
{
    internal enum OutPutSettings
    {
        HideAll,
        Short,
        Full
    }

    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.BufferHeight = 15000;
            runTests(OutPutSettings.Full);    
#if DEBUG
            Console.ReadLine();
#endif
        }

        private static void runTests(OutPutSettings output)
        {
            Console.WriteLine("Reading text...");
            var text = File.ReadAllText(@"../../sometext.txt");
            /*var text = @"The hen named the fox chicken, and thereafter there were more chickens and hens laughing than foxes smiling
                        and Andrew was happy and so was I and therefore I immeadiately smiled.";*/
            Benchmark(text, 1, OutPutSettings.HideAll); //warmup
            var time = Benchmark(text, 5, output);
            Console.WriteLine($"Histogram was generated in {time} ms.");
        }

        public static int Benchmark(string text, int repetitions, OutPutSettings output)
        {
            if (output != OutPutSettings.HideAll)
            {
                Console.WriteLine("Generating histograms...");
                Console.WriteLine();
            }

            var histogram = Enumerable.Empty<HistogramGenerator.HistogramItem<char>>();
            var time = 0L;

            for (int i = 0; i < repetitions; i++)
            {
                var watch = Stopwatch.StartNew();
                histogram = HistogramGenerator.GetHistogram(text, StreamPreprocessors.GetTextPreprocessor);
                watch.Stop();
                time += watch.ElapsedMilliseconds;
            }

            if (output != OutPutSettings.HideAll)
            {
                if (output == OutPutSettings.Full)
                {
                    foreach (var item in histogram)
                    {
                        Console.WriteLine($"{new string(item.Path.ToArray())}: x{item.Count:#,###,##0}");
                    }

                    Console.WriteLine();
                }

                Console.WriteLine($"Text is {text.Length:#,###,##0} characters / {StreamPreprocessors.GetTextPreprocessor.Process(text).Count() :#,###,##0} long.");
                Console.WriteLine($"Total words in histogram: {histogram.Sum(h => h.Count)::#,###,##0}");
            }

            return (int)Math.Round(time / (double)repetitions, 0);
        }
    }
}
