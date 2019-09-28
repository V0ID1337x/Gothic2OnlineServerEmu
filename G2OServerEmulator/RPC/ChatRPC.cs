using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RakNet;
using static G2OServerEmulator.Server;

namespace G2OServerEmulator
{
    internal class ChatRPC
    {
        public static void ChatMessage(ref Packet packet, ref BitStream bitStream, in Network network)
        {
            Player player = null;
            if(ServerInstance.PlayerManager.players.TryGetValue(packet.systemAddress.systemIndex, out player))
            {
                string message;
                bitStream.ReadCompressed(out message);

                Console.WriteLine($"[chat] {player.Name}: {message}");

                // onPlayerMessage(int id, string message)
                ServerInstance.EventManager.CallEvent("onPlayerMessage", player.Id, message);
                // TYLKO I WYLACZNIE DLA TESTOW PRZESYLAM TUTAJ WIADOMOSC DALEJ!!! POTEM PRZENIESC TO DO SKRYPTA
                bitStream.Reset();
                bitStream.Write((byte)eNetworkMessage.CHAT_MESSAGE);
                NetStream.WritePedId(ref bitStream, packet.systemAddress.systemIndex);
                bitStream.Write((byte)255);
                bitStream.Write((byte)255);
                bitStream.Write((byte)255);
                bitStream.WriteCompressed(message);
                
                network.SendToAll(ref bitStream, PacketPriority.HIGH_PRIORITY, PacketReliability.RELIABLE);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="bitStream"></param>
        /// <param name="network"></param>
        public static void ChatCommand(ref Packet packet, ref BitStream bitStream, in Network network)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(packet.systemAddress.systemIndex, out player))
            {
                string command;
                string @params = "";
                if (!bitStream.ReadCompressed(out command))
                    return;
                if (bitStream.ReadBit())
                {
                    if (!bitStream.ReadCompressed(out @params))
                        return;
                }
                ServerInstance.EventManager.CallEvent("onPlayerCommand", player.Id, command, @params);
            }
        }
    }
}
