using SoulFab.Core.Base;
using SoulFab.Core.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenBao.MES.Lib
{
    public class XCOMMessageCodec : TextMessageCodec<string>
    {
        public XCOMMessageCodec(MessageScheme scheme)
            : base(scheme)
        {
            this.Scheme = scheme;
        }

        protected override object DecodeItem(FieldDef fd, TextMessageParser parser)
        {
            object ret = null;

            switch (fd.Type)
            {
                case CommonType.String:
                    ret = parser.GetString(fd.Length);
                    break;

                case CommonType.Integer:
                    ret = parser.GetInt(fd.Length);
                    if (fd.Precision > 0)
                    {
                        ret = Convert.ToDouble(ret) / Math.Pow(10, fd.Precision);
                    }

                    break;
            }

            return ret;
        }

        protected override void EncodeItem(FieldDef fd, TextMessageBuilder builder, object value)
        {
            switch (fd.Type)
            {
                case CommonType.String:
                    builder.Add((string)value, fd.Length);
                    break;

                case CommonType.DataTime:
                    builder.Add(((DateTime)value).ToLocalTime().ToString("yyyyMMddHHmmss"), 14);
                    break;

                case CommonType.Integer:
                    if (fd.Precision > 0)
                    {
                        value = (int)(Math.Round((double)value * Math.Pow(10, fd.Precision)));
                    }

                    builder.Add(Convert.ToInt32(value), fd.Length);
                    break;
            }
        }
    }
}
