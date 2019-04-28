﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SleepController.Domain
{
    public class ClosedEyesDetector
    {
        public int ClosedEyesMinThreshold => 20;

        public int ClosedEyesMaxThreshold => 40;

        public int NextWeight => 2;

        public int PreviousWeight => 1;

        public int FloatingAverage { get; protected set; }

        public bool IsClosed(IEnumerable<EEGEntry> batch, out int average)
        {
            average = batch.Select(it => it.SignalO1A1)
                .Sum(it => Math.Abs(it)) / batch.Count();

            FloatingAverage = (PreviousWeight * FloatingAverage + NextWeight * average)
                / (NextWeight + PreviousWeight);

            var antialiasing = Math.Abs(FloatingAverage - average);

            return antialiasing >= ClosedEyesMinThreshold
                && antialiasing <= ClosedEyesMaxThreshold;
        }
    }
}
