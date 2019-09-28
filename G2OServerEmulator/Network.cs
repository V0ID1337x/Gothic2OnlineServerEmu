using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RakNet;
using static System.Console;

namespace G2OServerEmulator
{
    /// <summary>
    /// Dostęp do klasy ograniczony tylko dla tej aplikacji.
    /// Moduły otrzymają osobną funkcję do obsługi pakietów.
    /// </summary>
    internal class Network
    {
        private RakPeerInterface rakPeerInterface;
        private bool isRunning { get; set; }
        private BitStream receiveBitStream;
        public RPC RPC = new RPC();
        // Tu damy DLLimporty do moich funkcji w C++ uzywanych do obslugi statusa serwera
        [DllImport("RakNet.dll", EntryPoint = "Network_SetData", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern void Network_SetData(int major, int minor, int patch, int players, int maxSlots, char[] hostname, char[] world, char[] desc);
        [DllImport("RakNet.dll", EntryPoint = "Network_SetPlayers", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern void Network_SetPlayers(int playersOnline);
        [DllImport("RakNet.dll", EntryPoint = "Network_SetHostname", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern void Network_SetHostname(char[] hostname);
        [DllImport("RakNet.dll", EntryPoint = "Network_SetWorld", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern void Network_SetWorld(char[] world);
        [DllImport("RakNet.dll", EntryPoint = "Network_SetDescription", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern void Network_SetDescription(char[] desc);

        public Network()
        {
            isRunning = false;
        }
        public void Start(in uint port, in uint maxConnections)
        {
            receiveBitStream = new BitStream();
            rakPeerInterface = RakPeer.GetInstance();

            BindRpcFunctions();

            // InitializeSecurity oraz SetIncomingPassword zostało zmodyfikowane w RakNet.dll
            // Przyjmuje tam wartości dokładnie wymagane przez G2O
            // Zostało to zrobione dla bezpieczeństwa (możliwia opublikowanie kodu źródłowego tego emulatora)
            if (!rakPeerInterface.InitializeSecurity("code", "code", false)) throw new Exception("Failed to initialize security! Use RakNet.dll from emulator's directory");
            var result = rakPeerInterface.Startup(maxConnections, new SocketDescriptor((ushort)port, null), 1);
            if (result != StartupResult.RAKNET_STARTED) throw new Exception($"RakNet startup error!\nCode:{result}");
            char[] password = "VARWAG12312VVE".ToCharArray();
            rakPeerInterface.SetIncomingPassword(password.ToString(), password.Length);
            rakPeerInterface.SetMaximumIncomingConnections((ushort)maxConnections);
            // After startup
            for (uint i = 0; i < rakPeerInterface.GetNumberOfAddresses(); ++i)
                WriteLine($"[RakNet] Listening on {rakPeerInterface.GetLocalIP(i)}:{port}...");
            isRunning = true;
        }
        public void Stop()
        {
            RPC.Event.Clear();
            rakPeerInterface.Shutdown(3000);
            RakPeer.DestroyInstance(rakPeerInterface);
            Thread.Sleep(1000);
            isRunning = false;
        }
        /// <summary>
        /// Przyda się do file patcher'a
        /// </summary>
        /// <returns></returns>
        public List<string> GetLocalAddresses()
        {
            var result = new List<string>();
            for (uint i = 0; i < rakPeerInterface.GetNumberOfAddresses(); ++i)
                result.Add(rakPeerInterface.GetLocalIP(i));
            return result;
        }
        public void Ping(in SystemAddress systemAddress)
        {
            rakPeerInterface.Ping(systemAddress);
        }
        public int GetPing(in SystemAddress systemAddress)
        {
            return rakPeerInterface.GetLastPing(systemAddress);
        }
        public void SendJoined(ref BitStream bitStream, PacketPriority packetPriority, PacketReliability packetReliability)
        {
            foreach (var i in Server.ServerInstance.PlayerManager.players)
                if (i.Value.Joined) Send(ref bitStream, packetPriority, packetReliability, i.Value.SystemAddress);
        }
        public void SendToAll(ref BitStream bitStream, PacketPriority packetPriority, PacketReliability packetReliability)
        {
            foreach (var i in Server.ServerInstance.PlayerManager.players)
                Send(ref bitStream, packetPriority, packetReliability, i.Value.SystemAddress);
        }
        public void SendToStreamed(ref BitStream bitStream, PacketPriority packetPriority, PacketReliability packetReliability, in Player player)
        {
            foreach (var i in player.StreamedPlayers)
                Send(ref bitStream, packetPriority, packetReliability, i.Value.SystemAddress);
        }
        /// <summary>
        /// Funkcja służąca do wysyłania pakietów
        /// </summary>
        /// <param name="bitStream"></param>
        /// <param name="packetPriority"></param>
        /// <param name="packetReliability"></param>
        /// <param name="systemAddress"></param>
        /// <returns></returns>
        public bool Send(ref BitStream bitStream, PacketPriority packetPriority, PacketReliability packetReliability, in SystemAddress systemAddress)
        {
            if(isRunning)
                return rakPeerInterface.Send(bitStream, packetPriority, packetReliability, (char)0, systemAddress, false) != 0;
            return false;
        }
        public void Disconnect(in SystemAddress systemAddress)
        {
            rakPeerInterface.CloseConnection(systemAddress, true);
        }
        /// <summary>
        /// Tu zbindujemy funkcje do identyfikatorów pakietów
        /// </summary>
        private void BindRpcFunctions()
        {
            // Connection
            RPC.Event[(byte)DefaultMessageIDTypes.ID_NEW_INCOMING_CONNECTION] = PlayerRPC.Incoming;
            RPC.Event[(byte)DefaultMessageIDTypes.ID_DISCONNECTION_NOTIFICATION] = PlayerRPC.Disconnect;
            RPC.Event[(byte)DefaultMessageIDTypes.ID_CONNECTION_LOST] = PlayerRPC.Disconnect;
            RPC.Event[(byte)eNetworkMessage.DISCONNECT_CRASH] = PlayerRPC.Disconnect;
            RPC.Event[(byte)eNetworkMessage.CONNECT_REQUEST] = PlayerRPC.Connect;
            RPC.Event[(byte)eNetworkMessage.CONNECT_JOIN] = PlayerRPC.Join;

            // Chat
            RPC.Event[(byte)eNetworkMessage.CHAT_MESSAGE] = ChatRPC.ChatMessage;
            RPC.Event[(byte)eNetworkMessage.CHAT_COMMAND] = ChatRPC.ChatCommand;

            // Player
            RPC.Event[(byte)eNetworkMessage.PLAYER_SYNC] = PlayerRPC.SyncFoot;
            RPC.Event[(byte)eNetworkMessage.PLAYER_ANI] = PlayerRPC.Anis;
            RPC.Event[(byte)eNetworkMessage.PLAYER_WEAR] = PlayerRPC.Wear;
            RPC.Event[(byte)eNetworkMessage.PLAYER_HIT] = PlayerRPC.Hit;
            RPC.Event[(byte)eNetworkMessage.PLAYER_MAGIC] = PlayerRPC.MagicCast;
            RPC.Event[(byte)eNetworkMessage.PLAYER_OVERLAY] = PlayerRPC.Overlay;
            RPC.Event[(byte)eNetworkMessage.PLAYER_WORLD_CHANGE] = PlayerRPC.ChangeWorld;
            RPC.Event[(byte)eNetworkMessage.PLAYER_WORLD_ENTER] = PlayerRPC.EnterWorld;
            RPC.Event[(byte)eNetworkMessage.PLAYER_MOB] = PlayerRPC.MobInteract;

            // Script
            RPC.Event[(byte)eNetworkMessage.SCRIPT_RPC] = ScriptRPC.OnPacket;
        }
        public void Process()
        {
            if(isRunning == true)
            {
                for(var packet = rakPeerInterface.Receive(); packet != null; rakPeerInterface.DeallocatePacket(packet), packet = rakPeerInterface.Receive())
                {
                    receiveBitStream.Reset();
                    receiveBitStream.Write(packet.data, packet.length);
                    receiveBitStream.IgnoreBytes(1);
                    // RPC
                    
                    try
                    {
                        RPC.Event[packet.data[0]](ref packet, ref receiveBitStream, this);
                    }
                    catch (Exception e)
                    {
                        WriteLine("[rpc] Unbound packet ID: {0}\nException:{1}", (eNetworkMessage)packet.data[0], e);
                    }
                }
            }
        }
     }
    /// <summary>
    /// Klasa do wywoływania zdarzeń przy odbieraniu pakietów
    /// </summary>
    internal class RPC
    {
        public delegate void RpcFunc(ref Packet packet, ref BitStream bitStream, in Network network);
        public Dictionary<byte, RpcFunc> Event = new Dictionary<byte, RpcFunc>();
    }
}
