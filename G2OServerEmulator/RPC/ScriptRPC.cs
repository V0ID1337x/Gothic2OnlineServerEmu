using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RakNet;
using static G2OServerEmulator.Server;

namespace G2OServerEmulator
{
    internal class ScriptRPC
    {
        public static void OnPacket(ref Packet packet, ref BitStream bitStream, in Network network)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(packet.systemAddress.systemIndex, out player))
                ServerInstance.EventManager.CallEvent("onPacket", player.Id, bitStream);
        }
    }
}
