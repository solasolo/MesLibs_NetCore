using Microsoft.Extensions.Hosting;
using SoulFab.Core.Config;
using SoulFab.Core.Helper;
using SoulFab.Core.System;
using System.Threading;
using System.Threading.Tasks;

namespace GenBao.MES.Service
{
    public class World : DefaultSystem, IService
    {
        private S7DataAcquire DataAcquire;

        public World(string[] args)
            : base(args)
        { 
        }

        protected override void CreateWorld()
        {
            this.RemoteLogPort = 4445;

            ReflectHelper.LoadAssemblyFromPath(this.RootPath);

            base.CreateWorld();

            IConfig config = new XMLConfig(this.RootPath + "../../Data/pda.conf");
            this.Set(config);

            this.DataAcquire = new S7DataAcquire(this);
        }

        public override void Start()
        {
            this.DataAcquire.Start();
        }

        public override void Stop()
        {
            this.DataAcquire.Start();
        }
    }
}
