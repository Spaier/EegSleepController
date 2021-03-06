﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static SleepController.Domain.Constants;

namespace SleepController.Domain.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var eegStreamReader = new StreamReader(@$"..\..\..\..\Data\kazakov_eeg_4channels.edf");
            var eegIndex = 0;

            using var armStreamReader = new StreamReader(@$"..\..\..\..\Data\buhanov_arm.dat");
            var armIndex = 22;

            var parser = new SignalParser();
            var signal = await parser.Parse(armStreamReader, armIndex).ConfigureAwait(false);
            var detector = new SuperDetector()
            {
                Threshold = ARM_THRESHOLD,
            };
            var result = new List<(short Entry, bool IsDetected, int Average, int FloatingAverage)>();
            var batchSize = 24;
            {
                var i = 0;
                while (true)
                {
                    var batch = signal.Data
                        .Skip(batchSize * i++)
                        .Take(batchSize);

                    if (!batch.Any())
                    {
                        break;
                    }

                    var isDetected = detector.Detect(batch);
                    result.AddRange(batch.Select(entry => (entry, isDetected, detector.Average, detector.FloatingAverage)));
                }
            }

            await File.WriteAllLinesAsync("results.csv", result
                .Select(it => $"{(it.IsDetected ? "1" : "0")},{it.Average},{it.FloatingAverage},{it.Entry}")
                .Prepend("IsDetected,Average,FloatingAverage,Value")
                .ToArray())
                .ConfigureAwait(false);

            await File.WriteAllLinesAsync("is_detected.txt", result
                .Select(it => $"{(it.IsDetected ? "1" : "0")}")
                .ToArray())
                .ConfigureAwait(false);

            await File.WriteAllLinesAsync("average.txt", result
                .Select(it => $"{it.Average}")
                .ToArray())
                .ConfigureAwait(false);

            await File.WriteAllLinesAsync("floating_average.txt", result
                .Select(it => $"{it.FloatingAverage}")
                .ToArray())
                .ConfigureAwait(false);

            Console.WriteLine("The End.");
            Console.ReadKey();
        }
    }
}
