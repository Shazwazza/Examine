using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine.Scoring
{
    public class DecayingNumber
    {
        private double _lambda;
        public DecayingNumber(double halflifeInDays = 2)
        {
            _lambda = Math.Log(2) / (halflifeInDays);
        }

        /// <summary>
        /// The total number of events
        /// </summary>
        public int Count { get; internal set; }

        /// <summary>
        /// The decayed "mass" of events
        /// </summary>
        internal double Mass { get; set; }

        /// <summary>
        /// Last change date in ticks
        /// </summary>
        public long? LastChange { get; internal set; }

        public void Increment(DateTime? refDate = null)
        {
            lock (this)
            {
                Mass = GetCurrentMass(DateTime.Now.Ticks);

                if (refDate == null)
                {
                    LastChange = DateTime.Now.Ticks;
                    ++Mass;
                }
                else
                {
                    LastChange = LastChange.HasValue && LastChange.Value > refDate.Value.Ticks ? LastChange.Value : refDate.Value.Ticks;
                    var age = (DateTime.Now - refDate.Value).TotalDays;
                    Mass += Math.Exp(-age * _lambda);
                }
                ++Count;
            }
        }

        public void Decrement(DateTime refDate)
        {
            lock (this)
            {
                --Count;
                var age = (DateTime.Now - refDate).TotalDays;
                Mass -= Math.Exp(-age * _lambda);
            }
        }


        public double GetCurrentMass(long now)
        {            
            if (!LastChange.HasValue)
            {
                return 0d;
            }

            var age = (now - LastChange.Value) / (double)TimeSpan.TicksPerDay;
            return Mass * Math.Exp(-age * _lambda);
        }

        public override string ToString()
        {
            return Count.ToString();
        }
    }
}
