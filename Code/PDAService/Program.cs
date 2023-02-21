using SoulFab.Core.Logger;
using SoulFab.Core.System;
using SoulFab.Core.Web;

namespace GenBao.MES.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var world = new World(args);

            var logger = world.Logger;

            using (world)
            {
                WebAPIService.Run(args, world);
            }

        }
    }
}
