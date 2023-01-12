using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
namespace L2JServer.Net.IO
{
    internal class PacketReader : BinaryReader

    {
        private NetworkStream _ns;
        public PacketReader(NetworkStream ns) : base(ns)
        {
            _ns = ns;
        }

        public string ReadMessage()
        {
            byte[] msgBuffer;
            var length = ReadInt32();
            msgBuffer = new byte[length];
            _ns.Read(msgBuffer, 0, length);
            var msg = Encoding.ASCII.GetString(msgBuffer);

            return msg;
        }

        public string ReadJPMessage()
        {
            byte[] msgBuffer;
            var length = ReadInt32() * 2;
            msgBuffer = new byte[length];
            _ns.Read(msgBuffer, 0, length);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var jpEnc = Encoding.GetEncoding(932);
            var msg = jpEnc.GetString(msgBuffer);

            return msg;
        }
    }
}
