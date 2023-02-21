using Microsoft.Extensions.Logging;
using S7.Net;
using S7.Net.Protocol.S7;
using SoulFab.Core.Base;
using SoulFab.Core.Logger;
using SoulFab.Core.System;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GenBao.MES.Service
{
    public class S7DataAcquire
    {
        private ILogger Logger;
        private Plc PLC;
        private DataScheme Scheme;
        private ConcurrentLinkedQueue<AcruiredData> Quere;

        public S7DataAcquire(ISystem system)
        {
            this.Logger = system.Get<ILogger>();

            this.Quere = new ConcurrentLinkedQueue<AcruiredData>();
        }

        public void SetConfig()
        { 
        
        }

        public void Start()
        { 
        }

        public void Stop()
        { 
        
        }

        private IList<DataItemAddress> MakeAddressItem()
        {
            var ret = new List<DataItemAddress>();

            return ret;
        }

        private void AcquireProc()
        {
            byte[] RequestData;

            while (true)
            {
                try
                {
                    if (!this.PLC.IsConnected)
                    {
                        this.PLC.Open();
                    }

                    try
                    {
                        var items = this.MakeAddressItem();
                        RequestData = this.PLC.Build(items);

                        this.DoAcquire(RequestData).Wait();
                    }
                    catch (Exception ex)
                    {
                        this.PLC.Close();
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex);
                }

                Thread.Sleep(1000);
            }
        }

        private async Task DoAcquire(byte[] req_data)
        {
            var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(20));
            while (await timer.WaitForNextTickAsync())
            {
                var ret = await this.PLC.Read(req_data);

                this.Quere.Enqueue(new AcruiredData(ret));
            }
        }
    }
}
