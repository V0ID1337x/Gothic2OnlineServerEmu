using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RakNet;
using static G2OServerEmulator.Server;
namespace G2OServerEmulator
{
    class ScriptPacket : IDisposable
    {
        private BitStream bitStream;
        public ScriptPacket(BitStream bS)
        {
            bitStream = new BitStream();

            bS.ResetReadPointer();
            bitStream.Write(bS);
            bitStream.IgnoreBytes(2); // Lub 8 (w oryginalnej implementacji jest tu 8 podczas gdy w bs znajdują się 2 bajty)
        }

        public ScriptPacket()
        {
            bitStream = new BitStream();
        }

        public void Dispose()
        {
            bitStream.Reset();
            bitStream.Dispose();
        }

        public void Reset()
        {
            bitStream.Reset();
            bitStream.Write((byte)eNetworkMessage.SCRIPT_RPC);
            bitStream.Write((byte)eScriptRPC.SCRIPT_PACKET);
            bitStream.IgnoreBytes(2); // Lub 8
        }

        public void Begin()
        {
            bitStream.SetReadOffset(0);
            bitStream.IgnoreBytes(2); // Lub 8
        }

        public void Send(in int playerID, in PacketReliability reliability)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                ServerInstance.Network.Send(ref bitStream, PacketPriority.MEDIUM_PRIORITY, reliability, player.SystemAddress);
        }

        public void SendToAll(in PacketReliability reliability)
        {
            ServerInstance.Network.SendToAll(ref bitStream, PacketPriority.MEDIUM_PRIORITY, reliability);
        }

        public void WriteBool(bool val)
        {
            bitStream.Write(val);
        }

        public void WriteChar(char val)
        {
            bitStream.Write(val);
        }

        public void WriteUInt8(int val)
        {
            bitStream.Write((byte)val);
        }

        public void WriteInt16(int val)
        {
            bitStream.Write((Int16)val);
        }

        public void WriteUInt16(int val)
        {
            bitStream.Write((UInt16)val);
        }

        public void WriteInt32(int val)
        {
            bitStream.Write(val);
        }

        public void WriteUInt32(int val)
        {
            bitStream.Write(val);
        }
        
        public void WriteFloat(float val)
        {
            bitStream.Write(val);
        }

        public void WriteString(string val)
        {
            bitStream.Write(val);
        }

        public bool ReadBool()
        {
            bool result;
            if (!bitStream.Read(out result)) throw new Exception("Cannot read data from packet!");
            return result;
        }

        public char ReadChar()
        {
            char result;
            if (!bitStream.Read(out result)) throw new Exception("Cannot read data from packet!");
            return result;
        }

        public char ReadInt8()
        {
            char result;
            if (!bitStream.Read(out result)) throw new Exception("Cannot read data from packet!");
            return result;
        }

        public byte ReadUInt8()
        {
            byte result;
            if (!bitStream.Read(out result)) throw new Exception("Cannot read data from packet!");
            return result;
        }

        public Int16 ReadInt16()
        {
            Int16 result;
            if (!bitStream.Read(out result)) throw new Exception("Cannot read data from packet!");
            return result;
        }

        public UInt16 ReadUInt16()
        {
            Int16 result;
            if (!bitStream.Read(out result)) throw new Exception("Cannot read data from packet!");
            return (UInt16)result;
        }

        public int ReadInt32()
        {
            int result;
            if (!bitStream.Read(out result)) throw new Exception("Cannot read data from packet!");
            return result;
        }

        public uint ReadUInt32()
        {
            int result;
            if (!bitStream.Read(out result)) throw new Exception("Cannot read data from packet!");
            return (uint)result;
        }

        public float ReadFloat()
        {
            float result;
            if (!bitStream.Read(out result)) throw new Exception("Cannot read data from packet!");
            return result;
        }

        public string ReadString()
        {
            string result;
            if (!bitStream.Read(out result)) throw new Exception("Cannot read data from packet!");
            return result;
        }
    }
}
