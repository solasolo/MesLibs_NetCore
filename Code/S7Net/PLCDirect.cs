using S7.Net.Protocol;
using S7.Net.Protocol.S7;
using S7.Net.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S7.Net
{
    public partial class Plc
    {
        public async Task<byte[]> Read(byte[] send_data, CancellationToken cancel_oken = default)
        {
            var s7data = await RequestTsduAsync(send_data, cancel_oken);

            ValidateResponseCode((ReadWriteErrorCode)s7data[14]);

            return Parse(s7data);
        }

        public async Task Write()
        {
        }

        public byte[] Build(IList<DataItemAddress> items)
        {
            byte[] ret = null;

            if (Accert(items))
            {
                ret = BuildReadRequestPackage(items);
            }


            return ret;
        }

        private bool Accert(IList<DataItemAddress> items)
        {
            return true;
        }

        private byte[] Parse(byte[] recv_data)
        {
            byte[] ret = null;

            int offset = 14;
            int item_count = recv_data[13];
            var ranges = new int[item_count * 2];
            var total_size = 0;
            int data_len = recv_data.Length;

            for (int i = 0; i < item_count; i++)
            {
                // check for Return Code = Success
                if (recv_data[offset] != 0xff)
                    throw new PlcException(ErrorCode.WrongNumberReceivedBytes);

                int len = (recv_data[offset + 2] * 256 + recv_data[offset + 3]) / 8;

                // to Data bytes
                offset += 4;

                ranges[i * 2] = offset;
                ranges[i * 2 + 1] = len;
                total_size += len;

                // next Item
                offset += len;

                // Always align to even offset
                if (offset % 2 != 0)
                    offset++;
            }

            if (total_size > 0)
            {
                ret = new byte[total_size];
                int des_pos = 0;
                for (int i = 0; i < item_count; i++)
                {
                    var src_pos = ranges[i * 2];
                    var len = ranges[i * 2 + 1];
                    Array.Copy(recv_data, src_pos, ret, des_pos, len);
                    des_pos += len;
                }
            }

            return ret;
        }
    }
}
