using System;
using System.Collections.Generic;
using Unipi.Nancy.Numerics;

#if DO_LOG
using NLog;
#endif

namespace Unipi.Nancy.MinPlusAlgebra;

internal class IntervalTree
{
    #if DO_LOG
    private static Logger logger = LogManager.GetCurrentClassLogger();
    #endif

    public IReadOnlyList<Interval> Intervals { get; }

    public int Count => Intervals.Count;

    public IntervalTree(IReadOnlyList<Interval> intervals, ComputationSettings? settings = null)
    {
        Intervals = intervals
            .SortIntervals(settings);

#if DEBUG
        if (!Intervals.AreInTimeSequence())
            throw new InvalidOperationException();
#endif
    }

    public Interval? Query(Rational time)
    {
        int a = 0;
        int b = Count - 1;

        if (a > b)
            throw new ArgumentException("Start must be lower or equal than end");

        while (true)
        {
            if (a == b)
                break;

            int middle = (a + b) / 2;
            var middleInterval = Intervals[middle];
            if (middleInterval.Start >= time)
                b = middle;
            else
                a = a != middle ? middle : b;
        }

        var interval = Intervals[a];
        return (interval.IsPointInterval && interval.Start == time) ? interval : null;
    }

    public IEnumerable<Interval> Query(Rational start, Rational end)
    {
        int startIndex = findStart();
        int endIndex = findEnd();

        for (int i = startIndex; i <= endIndex; i++)
        {
            var interval = Intervals[i];
            if(interval.IsPointInterval && ( i == startIndex || i == endIndex))
                continue;

            yield return interval;
        }

        int findStart()
        {
            int a = 0;
            int b = Count - 1;

            while (true)
            {
                if (a == b)
                    break;

                int middle = (a + b) / 2;
                if (Intervals[middle].Start >= start)
                    b = middle;
                else
                    a = a != middle ? middle : b;
            }

            return a;
        }

        int findEnd()
        {
            int a = 0;
            int b = Count - 1;

            while (true)
            {
                if(a == b)
                    break;

                int middle = (a + b) % 2 == 1 ? 
                    (a + b) / 2 + 1 :
                    (a + b) / 2;
                if (Intervals[middle].End <= end)
                    a = middle;
                else
                    b = b != middle ? middle : a;
            }

            return a;
        }
    }
}