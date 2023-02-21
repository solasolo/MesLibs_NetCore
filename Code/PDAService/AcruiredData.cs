using System;

namespace GenBao.MES.Service
{
    public class AcruiredData
    {
        public readonly DateTime Time;
        public readonly long Tick;
        public readonly byte[] Data;
            
        public AcruiredData(byte[] data)
        {
            this.Tick = Environment.TickCount64;
            this.Time = DateTime.Now;

            this.Data = data;
        }
    }
}
