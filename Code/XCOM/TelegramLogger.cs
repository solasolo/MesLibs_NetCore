using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenBao.MES.Lib
{
    public class TelegramLogger
    {
        private string BasePath = String.Empty;
        private string ModuleName = String.Empty;

        public TelegramLogger(string path, string module)
        {
            this.BasePath = path;
            this.ModuleName = module;
        }

        public void Log(string code, byte[] msg, bool is_send)
        {
            DateTime t = DateTime.Now;
            StreamWriter sr = new StreamWriter(this.BasePath + this.ModuleName + t.ToString(" yyyy-MM-dd") + ".log", true);

            string txt = Encoding.UTF8.GetString(msg);
            string line = String.Format("{0:HH:mm:ss.fff} [{1}] {2} {3}", t, code, is_send? "<=" : "=>", txt);

            sr.WriteLine(line);
            sr.Flush();
            sr.Close();
        }
    }
}
