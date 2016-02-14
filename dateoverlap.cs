using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Hub.Rocks.Helpers
{
    public class DateOverlap
    {
        private DateTime _start = new DateTime();
        private DateTime _end = new DateTime();
        /// <summary>
        /// Start of the date range
        /// </summary>
        public DateTime Start
        {
            get { return _start; }
            set
            {
                if (value > End)
                    _end = value;
                _start = value;
            }
        }

        /// <summary>
        /// The end of the date range
        /// </summary>
        public DateTime End
        {
            get { return _end; }
            set
            {
                if (value < Start)
                    _start = value;
                _end = value;
            }
        }

        /// <summary>
        /// Gets or sets the data item associated with the range.
        /// Not used in any calculations but is useful when determining 
        /// which objects conflict without have to compare dates after
        /// the retrun of the overlap functions
        /// </summary>
        public object DataItem
        {
            get;
            set;
        }

        public DateOverlap(DateTime Start, DateTime End)
        {
            this.Start = Start;
            this.End = End;
        }

        public DateOverlap()
        {

        }

        public TimeSpan Span
        {
            get { return End - Start; }
        }

        /// <summary>
        /// Tests for an overlap of given date ranges
        /// </summary>
        /// <param name="TestEndPoints">Set to true if you want to count the date range end points as an overlap and false if you don't care if the end points overlap.</param>
        /// <param name="Ranges">The set of date ranges that you want to test.</param>
        /// <returns>True if there is an overlap of the ranges. False if there is no overlap.</returns>
        public static bool HasOverlap(bool TestEndPoints, params DateOverlap[] Ranges)
        {
            if (Ranges.Count() < 2)
                throw new Exception("Ranges count must be greater than 1");
            List<DateOverlap> Dates = new List<DateOverlap>();
            List<DateOverlap> WhiteSpace = new List<DateOverlap>();
            Dates.AddRange(Ranges);
            Dates = Dates.OrderBy(r => r.Start).ToList();
            Parallel.For(0, Dates.Count - 1, i =>
            {
                if (Dates[i].End < Dates[i + 1].Start)
                    WhiteSpace.Add(new DateOverlap(Dates[i].End, Dates[i + 1].Start));
            });
            TimeSpan total = Ranges.Max(r => r.End) - Ranges.Min(r => r.Start);
            TimeSpan sum = new TimeSpan();
            foreach (DateOverlap r in Ranges)
                sum += r.Span;
            foreach (DateOverlap r in WhiteSpace)
                sum += r.Span;
            if (sum < total)
                return false;
            if (sum > total)
                return true;
            if (!TestEndPoints)
                return false;
            var query = from a in Dates
                        join b in Dates on a.Start equals b.End
                        select a;
            return query.Count() > 0;
        }

        /// <summary>
        /// Tests for an overlap of given date ranges
        /// </summary>
        /// <param name="TestEndPoints">Set to true if you want to count the date range end points as an overlap and false if you don't care if the end points overlap.</param>
        /// <param name="Ranges">The set of date ranges that you want to test.</param>
        /// <returns>A list of pairs of date ranges that overlap each other. The list will be empty if there is no overlaps.</returns>
        public static IEnumerable<OverlapPairs> OverlapingRanges(bool TestEndPoints, params DateOverlap[] Ranges)
        {
            if (Ranges.Count() < 2)
                throw new Exception("Ranges count must be greater than 1");
            List<OverlapPairs> ret = new List<OverlapPairs>();
            if (!HasOverlap(TestEndPoints, Ranges))
                return ret;
            Parallel.For(0, Ranges.Count() - 1, i =>
            {
                Parallel.For(i + 1, Ranges.Count(), j =>
                {
                    if (HasOverlap(TestEndPoints, Ranges[i], Ranges[j]))
                        ret.Add(new OverlapPairs(Ranges[i], Ranges[j]));
                });
            });
            return ret;
        }
    }

    public class OverlapPairs
    {
        public DateOverlap First { get; private set; }
        public DateOverlap Second { get; private set; }

        public OverlapPairs(DateOverlap First, DateOverlap Second)
        {
            this.First = First;
            this.Second = Second;
        }
    }
}
