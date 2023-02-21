using SoulFab.Core.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenBao.MES.Lib
{
    public struct XCOMHeader
    {
        public int Length;
        public string TelID;
        public DateTime SendTime;
        public string SenderID;
        public string ReceiveID;

        public XCOMHeader(int len, string id)
        {
            this.SendTime = DateTime.Now;
            this.Length = len;
            this.TelID = id;
            this.SenderID = "";
            this.ReceiveID = "";
        }

        public byte[] ToBytes()
        {
            var encoder = Encoding.UTF8;

            byte[] ret = new byte[28];

            string len_str = String.Format("{0:0000}", this.Length);
            string time_str = this.SendTime.ToString("yyyyMMddHHmmss");

            var TelID = encoder.GetBytes(this.TelID);
            var TelLen = encoder.GetBytes(len_str);
            var TelTime = encoder.GetBytes(time_str);
            var TelSenderID = encoder.GetBytes(this.SenderID);
            var TelReceiveID = encoder.GetBytes(this.ReceiveID);

            TelLen.CopyTo(ret, 0);
            TelID.CopyTo(ret, 4);
            TelTime.CopyTo(ret, 10);
            TelSenderID.CopyTo(ret, 24);
            TelReceiveID.CopyTo(ret, 26);

            return ret;
        }

        public void FromBytes(byte[] bs)
        {
            var encoder = Encoding.UTF8;

            var TelLen = encoder.GetString(bs[0..4]);
            var TelTime = encoder.GetString(bs[10..24]);

            this.Length = Int32.Parse(TelLen);
            string TimeStr = $"{TelTime[0..4]}-{TelTime[4..6]}-{TelTime[6..8]} {TelTime[8..10]}:{TelTime[10..12]}:{TelTime[12..14]}";
            this.SendTime = DateTime.Parse(TimeStr);

            this.TelID = encoder.GetString(bs[4..10]);
            this.SenderID = encoder.GetString(bs[24..26]);
            this.ReceiveID = encoder.GetString(bs[26..28]);
        }
    }

    public class XCOMFrameCodec : IFrameCodec<string>
    {
        const byte ETX = 0x0a;

        private string MyID;
        private string OtherID;

        public XCOMFrameCodec(string mid, string oid)
        { 
            this.MyID = mid;
            this.OtherID = oid;
        }

        public bool Decode(StreamBuffer stream, ref MessageEnity<string> entity)
        {
            bool result = false;

            int BufSize = stream.GetSize();

            while (BufSize > 30)
            {
                int Length = 0;

                var len_bytes = new byte[4];
                stream.ReadData(len_bytes, 4);
                if (this.CheckLength(len_bytes, ref Length))
                {
                    byte ender = stream.ReadByte(Length - 1);
                    if (ender == ETX)
                    {
                        var header_bytes = new byte[28];
                        stream.ReadData(header_bytes, 28);
                        var header = new XCOMHeader();
                        header.FromBytes(header_bytes);

                        stream.Pick(28);

                        entity.Code = header.TelID;
                        int payload_size = Length - 29;
                        entity.Data = new byte[payload_size];
                        stream.PickData(entity.Data, payload_size);

                        stream.Pick(1);

                        result = true;
                    }
                    else
                    {
                        this.Clean(stream);
                    }
                }
                else 
                {
                    this.Clean(stream);
                }

                BufSize = stream.GetSize();
            }

            return result;
        }

        public byte[] Encode(string cmd, byte[] data)
        {
            int len = data.Length;
            int tel_len = len + 30;

            byte[] msg = new byte[tel_len];

            XCOMHeader header = new XCOMHeader(tel_len, cmd);
            header.SenderID = this.MyID;
            header.ReceiveID = this.OtherID;

            header.ToBytes().CopyTo(msg, 0);
            data.CopyTo(msg, 28);

            msg[msg.Length - 1] = ETX;

            return msg;
        }

        public byte[] EncodeHeartbeat()
        {
            int len = 30;

            byte[] msg = new byte[len];

            XCOMHeader header = new XCOMHeader(len, "999999");
            header.SenderID = this.MyID;
            header.ReceiveID = this.OtherID;

            header.ToBytes().CopyTo(msg, 0);
            msg[len - 2] = (byte)'C';
            msg[len - 1] = ETX;

            return msg;
        }

        private bool CheckLength(byte[] buf, ref int len)
        {
            bool ret = true;

            for (int i = 0; i < 4; i++)
            {
                byte c = buf[i];
                if (c > '9' || c < '0')
                {
                    ret = false;
                    break;
                }
            }

            if (ret)
            { 
                string str = Encoding.UTF8.GetString(buf);
                len = Int32.Parse(str);
            }

            return ret;
        }

        private void Clean(StreamBuffer stream)
        {
            int size = stream.GetSize();
            int pos = stream.Scan(ETX);
            if (pos > -1)
            {
                stream.Pick(pos + 1);
            }
            else
            {
                stream.Pick(size);
            }
        }
    }
}
