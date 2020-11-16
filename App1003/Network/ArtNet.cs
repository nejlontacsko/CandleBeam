using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace App1003
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ArtNetDmx
    {
        fixed byte ID[8];
        ushort OpCode;
        ushort Version;
        public byte Seq { get; set; }

        byte Physical;
        byte SubUni;
        byte Net;
        ushort Length;
        fixed byte Data[256];

        public ArtNetDmx(byte uni)
        {
            fixed (byte* p = this.ID)
            {
                *(p + 0) = (byte)'A';
                *(p + 1) = (byte)'r';
                *(p + 2) = (byte)'t';
                *(p + 3) = (byte)'-';
                *(p + 4) = (byte)'N';
                *(p + 5) = (byte)'e';
                *(p + 6) = (byte)'t';
                *(p + 7) = (byte)'\0';
            }

            /*fixed (byte* p = this.ID)
            {
                byte[] b = BitConverter.GetBytes(0x4172742d4e657400);
                for (int i = 0; i < 8; i++)
                    *(p + i) = b[i];
            }*/
            //OpCode = 0x2000;
            OpCode = 0x5000;
            //OpCode = 0x2400;
            //OpCode = 0x6000;
            //OpCode = 0x7000;
            //OpCode = 0xf800;
            Version = (ushort)_bswap(14);
            Seq = 0;
            Physical = 0;
            SubUni = uni;
            Net = 0;
            Length = (ushort)_bswap(256);
        }

        private static uint _bswap(uint val) => (val << 8) | (val >> 8);


        public void BlackOut()
        {
            fixed (byte* p = Data)
                for (int i = 0; i < 256; i++)
                    p[i] = 0;
        }
        public void FullOn()
        {
            fixed (byte* p = Data)
                for (int i = 0; i < 256; i++)
                    p[i] = 255;
        }

        public void SetChannel(ushort channel, byte val)
        {
            fixed (byte* p = this.Data)
            {
                p[channel] = val;
            }
        }

        public byte[] GetPacket()
        {
            Byte[] bytes = new byte[Marshal.SizeOf(typeof(ArtNetDmx))];
            GCHandle s = GCHandle.Alloc(this, GCHandleType.Pinned);

            try
            {
                Marshal.Copy(s.AddrOfPinnedObject(), bytes, 0, bytes.Length);
                return bytes;
            }
            finally
            {
                s.Free();
            }
        }

        public override string ToString()
        {
            string str = string.Empty;
            fixed (byte* d = this.Data)
            {
                for (int i = 0; i < 256; i++)
                {
                    str += d[i].ToString().PadLeft(4, ' ');
                    if (i % 20 == 19)
                        str += Environment.NewLine;
                }
            }
            return str;
        }
    }
}
