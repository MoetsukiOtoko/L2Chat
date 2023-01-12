using System;
using System.IO;
using System.Text;

namespace L2JServer.Net.IO
{
    internal class PacketBuilder
    {
        MemoryStream _ms;

        public PacketBuilder()
        {
            _ms = new MemoryStream();

        }

        public void WriteOpCode(byte opcode)
        {
            _ms.WriteByte(opcode);
        }

        public void WriteMessage(string msg)
        {
            
            var msgLength = msg.Length;
            _ms.Write(BitConverter.GetBytes(msgLength));
            _ms.Write(Encoding.ASCII.GetBytes(msg));
        }

        public void WriteJPMessage(string msg)
        {

            var msgLength = msg.Length * 2;
            _ms.Write(BitConverter.GetBytes(msgLength));
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var jpEnc = Encoding.GetEncoding(932);
            _ms.Write(jpEnc.GetBytes(msg));
        }

        public byte[] GetPacketBytes()
        {
            return _ms.ToArray();
        }
    }
}
