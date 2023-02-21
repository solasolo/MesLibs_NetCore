using SoulFab.Core.Base;
using SoulFab.Core.Communication;
using SoulFab.Core.Helper;

namespace GenBao.MES.Lib
{
    public class PLCMessageCodec<T> : BaseMessageCode<T, InvertBinaryMessageBuilder, InvertBinaryMessageParser>
    {
        public PLCMessageCodec(MessageScheme scheme)
        {
            this.Scheme = scheme;
        }

        protected override object DecodeItem(FieldDef fd, InvertBinaryMessageParser parser)
        {
            object ret = null;

            switch (fd.Type)
            {
                case CommonType.String:
                    ret = parser.GetPLCString();
                    break;

                case CommonType.Byte:
                    ret = parser.GetByte();
                    break;

                case CommonType.Word:
                    ret = parser.GetShort();
                    break;

                case CommonType.Integer:
                    ret = parser.GetInt();
                    break;

                case CommonType.LongInt:
                    ret = parser.GetLong();
                    break;

                case CommonType.Float:
                    ret = parser.GetFloat();
                    break;
            }

            return ret;
        }

        protected override void EncodeItem(FieldDef fd, InvertBinaryMessageBuilder builder, object value)
        {
            switch (fd.Type)
            {
                case CommonType.String:
                    builder.Add((string)value, fd.Length);
                    break;

                case CommonType.Byte:
                    builder.Add((byte)value);
                    break;

                case CommonType.Word:
                    builder.Add((short)value);
                    break;

                case CommonType.Integer:
                    builder.Add((int)value);
                    break;

                case CommonType.LongInt:
                    builder.Add((long)value);
                    break;

                case CommonType.Float:
                    builder.Add(Convert.ToSingle(value));
                    break;
            }
        }
    }
}
