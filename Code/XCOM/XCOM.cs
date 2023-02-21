using Microsoft.Extensions.Logging;
using SoulFab.Core.Communication;
using SoulFab.Core.Config;
using SoulFab.Core.Logger;
using System.Text;

namespace GenBao.MES.Lib
{
    public class XCOMServerChannel : TCPServer<string>
    {
        private XCOM Parent;

        public XCOMServerChannel(XCOM xcom, string ip, int port)
            : base(ip, port)
        {
            this.Parent = xcom;
            this.setLogger(xcom.Logger);
        }

        protected override void HandleConnected(IChannel channel)
        {
            this.Logger.Info("XCOM Server Connected");
        }

        protected override void HandleFrameData(IChannel channel, MessageEnity<string> msg)
        {
            this.Parent.HandleFrameData(channel, msg);
        }
    }

    public class XCOMClientChannel : TCPClient<string>
    {
        private XCOM Parent;

        public XCOMClientChannel(XCOM xcom, string ip, int port)
            : base(ip, port)
        {
            this.Parent = xcom;
            this.setLogger(xcom.Logger);
        }

        protected override void HandleConnected(IChannel channel)
        {
            this.Logger.Info("XCOM Clinet Connected");
        }

        protected override void HandleFrameData(IChannel channel, MessageEnity<string> msg)
        {
            this.Parent.HandleFrameData(this, msg);
        }
    }

    public interface IXCOMHandler
    {
        void Connected(IChannel channel);
        void DataReceived(string cmd, object obj);
    }

    public abstract class XCOM
    {
        public readonly ILogger Logger;
        protected TelegramLogger TelLogger;

        private XCOMServerChannel ServerChannel;
        private XCOMClientChannel ClientChannel;

        private IFrameCodec<string> FrameCodec;
        private IMessageCodec<string> MessageCodec;
        private IXCOMHandler Handler;

        private object SendLocker;
        private ManualResetEventSlim SendResponseEvent;

        public XCOM(ILogger logger, IConfig config, string log_path)
        {
            this.Logger = logger;

            this.TelLogger = new TelegramLogger(log_path, "XCOM");

            this.ServerChannel = new XCOMServerChannel(this, config["LocalIP"], config.GetInt("LocalPort"));
            this.ClientChannel = new XCOMClientChannel(this, config["RemoteIP"], config.GetInt("RemotePort"));

            string sid = config["SID"];
            string rid = config["RID"];
            this.FrameCodec = new XCOMFrameCodec(sid, rid);
            this.ServerChannel.SetFrameCodec(this.FrameCodec);
            this.ClientChannel.SetFrameCodec(this.FrameCodec);

            this.SendLocker = new object();
            this.SendResponseEvent = new ManualResetEventSlim(false);
        }

        public void Startup()
        {
            this.ServerChannel.Startup();
            this.ClientChannel.Startup();
        }

        public void Shutdown()
        {
            this.ServerChannel.Shutdown();
            this.ClientChannel.Shutdown();
        }

        public void Send(string cmd, object data)
        {
            lock (this.SendLocker)
            {
                var buf = this.MessageCodec.Encode(cmd, data);
                var send_buf = new byte[buf.Length + 1];
                send_buf[0] = (byte)'D';
                buf.CopyTo(send_buf, 1);

                this.ClientChannel.Send(cmd, send_buf);
                this.TelLogger.Log(cmd, buf, true);

                this.Logger.Debug($"{cmd} Sent");

                this.SendResponseEvent.Reset();
                bool done = this.SendResponseEvent.Wait(5000);
                if (!done)
                {
                    throw new Exception("XCOM Low Response Timeout");
                }
            }
        }

        public void setMessageCodec(IMessageCodec<string> codec)
        {
            this.MessageCodec = codec;
        }

        private void SendResponse(IChannel channel, string cmd, string msg)
        {
            byte[] res_data = new byte[81];
            for (int i = 1; i < res_data.Length; i++)
            {
                res_data[i] = (byte)' ';
            }

            if (String.IsNullOrEmpty(msg))
            {
                res_data[0] = (byte)'A';
            }
            else
            {
                res_data[0] = (byte)'B';
                var msg_bytes = Encoding.UTF8.GetBytes(msg);
                msg_bytes.CopyTo(res_data, 1);
            }

            (channel as IFrameChannel<string>).Send(cmd, res_data);
        }

        internal void HandleFrameData(IChannel channel, MessageEnity<string> msg)
        {
            string code = msg.Code;
            byte func = msg.Data[0];

            if (func == (byte)'C')
            {
            }
            else
            {
                if (msg.Data.Length > 1)
                {
                    if (channel != this.ClientChannel)
                    {
                        if (func == (byte)'D')
                        {
                            byte[] data = msg.Data[1..];

                            this.TelLogger.Log(code, data, false);

                            object obj = null;
                            try
                            {
                                obj = this.MessageCodec.Decode(code, data);
                                this.SendResponse(channel, msg.Code, "");
                            }
                            catch(Exception ex)
                            {
                                // this.SendResponse(channel, msg.Code, "Message Decode Error");
                                this.SendResponse(channel, msg.Code, "");
                                this.Logger.Error(ex, "Handle XCOM Frame");
                            }

                            if(obj != null)
                            {
                                this.DataArrived(code, obj);
                            }
                        }
                        else
                        { 
                            //
                        }
                    }
                    else
                    {
                        if (func == (byte)'B')
                        {
                            this.Logger.Error($"XCOM NAK Message: {code}");
                        }

                        this.Logger.Debug($"{code} Response: {Convert.ToChar(func)}");
                        this.SendResponseEvent.Set();
                    }
                }
                else
                {
                    this.Logger.Warning("XCOM Got Empty Message");
                }
            }
        }

        protected abstract void DataArrived(string cmd, object obj);
    }
}