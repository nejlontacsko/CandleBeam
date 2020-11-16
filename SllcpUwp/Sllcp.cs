using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace NejlonTacsko.Network
{
    public static class Sllcp
    {
        private static byte version = (byte)0x10;
        private static byte[] header = Encoding.ASCII.GetBytes("SLLCPv" + (char)version + '\0');
        private static int strLength = 16;
        private static uint sequence = 0;

        public enum OpCodes
        {
            TestMsg = 0x00,
            Restart = 0x01,
            Shutdown = 0x02,
            Disconn = 0x03,
            Poll = 0x10,
            PollReply = 0x11,
            PollResults = 0x12,
            OutLaser = 0x21,
            OutClose = 0x20,
            OutStrip = 0x22,
            OutAck = 0x2f,
            GetIpConf = 0x30,
            GetApList = 0x31,
            SetMode = 0x40,
            SetIpAdd = 0x41,
            SetWiFiAp = 0x42,
            SetAck = 0x4f,
            ApReply = 0x52,
            OutDmx256 = 0xc0,
            OutDmx512 = 0xd0,
            OutDmx1K = 0xe0,
            OutDmx2K = 0xf0
        }

        public enum DeviceCode : byte
        {
            Controller,
            Server,
            Node,
            Visualizer
        }
        public enum DmxMode
        {
            Dmx256 = 0xc0,
            Dmx512 = 0xd0,
            Dmx1024 = 0xe0,
            Dmx2048 = 0xf0
        };

        public static DmxMode Length { get; set; }

        public static bool IsNetworkCentralized { get; set; }

        public static uint SequenceId => sequence;

        public static void ResetSequence(uint id = 0)
        {
            sequence = id;
            //TODO: implement sequence reset packet
            //TODO: send it immediately
        }

        public static byte[] GetPacket(IPacket p)
        {
            Byte[] bytes = new byte[Marshal.SizeOf(p.GetType())];
            GCHandle s = GCHandle.Alloc(p, GCHandleType.Pinned);

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

        static async void Send(IPacket p, HostName ip, int port)
        {
            using (var serverDatagramSocket = new DatagramSocket())
            {
                using (Stream outputStream = (await serverDatagramSocket.GetOutputStreamAsync(ip, port.ToString())).AsStreamForWrite())
                {
                    byte[] packet = GetPacket(p);
                    await outputStream.WriteAsync(packet, 0, packet.Length);
                    await outputStream.FlushAsync();
                }
            }
        }

        public interface IPacket
        {
            /*public byte[] GetPacket(IPacket p)
            {
                Byte[] bytes = new byte[Marshal.SizeOf(p.GetType())];
                GCHandle s = GCHandle.Alloc(p, GCHandleType.Pinned);

                try
                {
                    Marshal.Copy(s.AddrOfPinnedObject(), bytes, 0, bytes.Length);
                    return bytes;
                }
                finally
                {
                    s.Free();
                }
            }*/
        }

        public static class Factory
        {
            public static Poll CreatePollRequest() => new Poll(header);

            public static PollReply CreatePollReply(
                string manufacture, string modelName, DmxMode length,
                bool hasWiFiIf, bool hasEthIf, DeviceCode dc,
                byte dmxIn, byte dmxOut, byte midiIn,
                byte midiOut, byte laserOut, byte stripOut
            ) => new PollReply(
                header,
                manufacture, modelName, length,
                hasWiFiIf, hasEthIf, dc,
                dmxIn, dmxOut, midiIn,
                midiOut, laserOut, stripOut);

            public static List<PollResults> CreatePollResults(List<PollReply> list)
            {
                List<PollResults> results = new List<PollResults>();
                PollReply[] replies;

                int i = 0;
                for (i = 0; i < list.Count / 23; i++)
                {
                    replies = new PollReply[23];
                    for (int j = 0; j < 23; j++)
                        replies[j] = list[i * 23 + j];
                    results.Add(new PollResults(replies, (byte)(list.Count / 23 - i)));
                }

                int mod = list.Count % 23;
                if (mod > 0)
                {
                    replies = new PollReply[mod];
                    for (int j = 0; j < mod; j++)
                        replies[j] = list[i * 23 + j];
                    results.Add(new PollResults(replies, 0));
                }

                return results;
            }

            public static OutDmx CreateOutDmx(IPAddress target, byte intf) =>
                new OutDmx(Length, sequence, IsNetworkCentralized ? target : new IPAddress(0), intf);
        }

        /*public static class Output
        {
            public static void SendDmxDataToServer() { }
            public static void SendDmxDataToNode() { }
        }*/

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Poll : IPacket
        {
            byte[] ID;
            byte OpCode;

            internal Poll(byte[] header)
            {
                ID = header;
                OpCode = (byte)OpCodes.Poll;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct PollReply : IPacket
        {
            byte[] ID;
            byte OpCode;

            byte[] Manufacture;
            byte[] ModelName;

            //Bit 7-6: Reserved (should be 1),
            //Bit 5-4: DmxLength,
            //Bit 3: HasWiFi,
            //Bit 2: HasEthernet,
            //Bit 1-0: DeviceCode
            byte Flags;

            //Byte 0: Bit 7-4: DmxIn, Bit 3-0: DmxOut;
            //Byte 1: Bit 7-4: MidiIn, Bit 3-0: MidiOut;
            //Byte 2: Bit 7-4: LaserOut, Bit 3-0: StripOut;
            byte[] InterfaceCnt;

            internal PollReply(
                byte[] header,
                string manufacture,
                string modelName,
                DmxMode length,
                bool hasWiFiIf,
                bool hasEthIf,
                DeviceCode dc,
                byte dmxIn,
                byte dmxOut,
                byte midiIn,
                byte midiOut,
                byte laserOut,
                byte stripOut)
            {
                ID = header;
                OpCode = (byte)OpCodes.PollReply;
                Manufacture = Encoding.ASCII.GetBytes(manufacture, 0, manufacture.Length < strLength - 1 ? manufacture.Length : strLength - 1);
                ModelName = Encoding.ASCII.GetBytes(modelName, 0, modelName.Length < strLength ? modelName.Length : strLength);
                Flags = (byte)(0xc0 | 256 << ((length - DmxMode.Dmx256) >> 4) | (hasWiFiIf ? 8 : 0) | (hasEthIf ? 4 : 0) | (byte)dc);
                InterfaceCnt = new byte[3] { (byte)(dmxOut | dmxIn << 4), (byte)(midiOut | midiIn << 4), (byte)(stripOut | laserOut << 4) };
            }
            public override string ToString() =>
                base.ToString() + Environment.NewLine +
                    "Protocol:\t" + Encoding.ASCII.GetString(header, 0, header.Length - 2) + BitConverter.ToString(new byte[] { header[6] }) + Environment.NewLine +
                    "Operation:\t" + OpCode + Environment.NewLine +
                    "Manufacture:\t" + Encoding.ASCII.GetString(Manufacture, 0, Manufacture.Length) + Environment.NewLine +
                    "Model name:\t" + Encoding.ASCII.GetString(ModelName, 0, ModelName.Length) + Environment.NewLine +
                    "Flags:\t" + Flags + Environment.NewLine +
                    "Interfaces:\t" + InterfaceCnt[0] + " " + InterfaceCnt[1] + " " + InterfaceCnt[2] + Environment.NewLine;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct PollResults : IPacket
        {
            byte[] ID;
            byte OpCode;
            byte Length; //Max. 23
            byte PacketsLeft;

            PollReply[] List; //23

            byte EndOfPacket;

            internal PollResults(PollReply[] list, byte left)
            {
                if (list.Length > 23)
                    throw new ArgumentOutOfRangeException();

                ID = header;
                OpCode = (byte)OpCodes.PollResults;
                Length = (byte)list.Length;
                PacketsLeft = left;
                List = list;
                EndOfPacket = 0xff;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct OutDmx : IPacket
        {
            byte[] ID;
            byte OpCode;
            byte[] padding;
            uint SeqId;
            byte[] ForwardIp;
            byte[] Data;

            internal OutDmx(DmxMode dm, uint SeqId, IPAddress forward, byte intf)
            {
                this.ID = header;
                this.OpCode = (byte)((byte)dm | intf);
                this.padding = new byte[3] { 0xaa, 0xbb, 0xcc };
                this.SeqId = SeqId;
                this.ForwardIp = forward.GetAddressBytes();

                this.Data = new byte[dm switch
                {
                    DmxMode.Dmx256 => 256,
                    DmxMode.Dmx512 => 512,
                    DmxMode.Dmx1024 => 1024,
                    DmxMode.Dmx2048 => 2048,
                    _ => throw new Exception("Please set the Length first!")
                }]; 
            }

            public void SetChannel(ushort channel, byte val)
            {
                fixed (byte* p = this.Data)
                {
                    p[channel] = val;
                }
            }
        }
    }
}