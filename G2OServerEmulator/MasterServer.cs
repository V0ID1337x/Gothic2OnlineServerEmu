using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using static G2OServerEmulator.Server;

namespace G2OServerEmulator
{
    struct PacketServerRequest
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
        public string Header;
        public byte Query;
        public ushort Port;
        public byte Major;
        public byte Minor;
        public byte Patch;
        public byte Players;
        public byte MaxSlots;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
        public string HostName;
    }
    class MasterServer
    {
        private string address;
        private int port;
        private long timerAnnounce;
        public MasterServer(in string Address)
        {
            timerAnnounce = 0;
            var host = Address.Split(':');
            if (host.Length != 2) throw new Exception("[MasterServer] Remote host error! Type {address:port}");
            address = host[0];
            if (!Int32.TryParse(host[1], out port)) throw new Exception("[MasterServer] Remote host error! Type {address:port}");
        }
        private void announce()
        {
            var packet = new PacketServerRequest();
            packet.Header = "GOm";
            packet.Query = 0x1;
            packet.Port = (ushort)ServerInstance.Config.Data.port;
            packet.Major = (byte)Version.Major;
            packet.Minor = (byte)Version.Minor;
            packet.Patch = (byte)Version.Patch;
            packet.Players = (byte)ServerInstance.PlayerManager.players.Count();
            packet.MaxSlots = (byte)ServerInstance.Config.Data.max_slots;
            packet.HostName = ServerInstance.Config.Data.hostname;

            var client = new UdpClient(address, port);
            var bytes = ByteSerializer.getBytes(packet);
            client.Send(bytes, bytes.Length);
            Console.WriteLine("[MasterServer] Announced successfully!");
        }
        public void Process()
        {
            if(Ticks > timerAnnounce)
            {
                announce();
                timerAnnounce = Ticks + 60000;
            }
        }
    }
}
