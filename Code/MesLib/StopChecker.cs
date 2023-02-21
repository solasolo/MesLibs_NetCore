using System;
using System.Runtime.InteropServices;

namespace GenBao.MES.Lib
{
    public enum LineStopState
    {
        Running,
        Begin,
        End,
    }

    public class StopChecker
    {
        private int Interval;
        private double UpperThreshold;
        private double LowerThreshold;

        public LineStopState State { get; private set;  }
        public DateTime StartTime { get; private set; }

        public StopChecker(int interval, double lower, double upper)
        {
            this.Interval = interval;
            this.LowerThreshold = lower;
            this.UpperThreshold = upper;
        }

        public LineStopState Check(double speed)
        {
            LineStopState ret = this.State;

            switch (ret)
            {
                case LineStopState.Begin:
                    if (speed > this.UpperThreshold)
                    {
                        if ((DateTime.Now - this.StartTime).TotalSeconds > this.Interval)
                        {
                            ret = LineStopState.End;
                        }
                        else
                        {
                            ret = LineStopState.Running;
                        }
                    }

                    break;

                case LineStopState.End:
                    ret = LineStopState.Running;

                    break;

                case LineStopState.Running:
                    if (speed < this.LowerThreshold)
                    {
                        this.StartTime = DateTime.Now;
                        ret = LineStopState.Begin;
                    }

                    break;
            }

            this.State = ret; 

            return ret;
        }

    }
}