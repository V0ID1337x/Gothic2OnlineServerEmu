using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using RakNet;

namespace G2OServerEmulator
{
    class PlayerManager
    {
        public Dictionary<int, Player> players = new Dictionary<int, Player>();
        private long timerStream;
        public PlayerManager()
        {
            timerStream = 0;
        }
        public bool Exist(string name)
        {
            foreach (var i in players)
                if (i.Value.Name.ToUpper() == name.ToUpper()) return true;
            return false;
        }
        /// <summary>
        /// Rozłącza wszystkie aktywne połączenia i czyści kontener z graczami
        /// </summary>
        public void Clear()
        {
            foreach (var i in players)
                Server.ServerInstance.Network.Disconnect(i.Value.SystemAddress);

            players.Clear();
            Network.Network_SetPlayers(0);
        }
        /// <summary>
        /// Funkcja do tworzenia gracza, używać tego zamiast new Player()
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool Create(in int id, in string name, out Player player)
        {
            player = new Player(id, name);
            if (id >= 0 && id < Server.ServerInstance.Config.Data.max_slots)
            {
                players.Add(id, player);
                Network.Network_SetPlayers(players.Count());
                return true;
            }
            player = null;
            return false;
        }

        public bool Destroy(in int id)
        {
            if(players.ContainsKey(id))
            {
                var result = players.Remove(id);
                Network.Network_SetPlayers(players.Count());
                return result;
            }
            return false;
        }
        
        public void Update()
        {
            // Sync
            foreach (var player in players)
                player.Value.Sync();
            // Stream
            Streamer();
        }

        private void Streamer()
        {
            if(timerStream < Server.Ticks)
            {
                if(players.Count() > 0)
                {
                    foreach(var player in players)
                    {
                        StreamPlayers(player.Value);
                    }
                }
                timerStream = Server.Ticks + Server.ServerInstance.Config.Data.stream_rate;
            }
        }

        private void StreamPlayers(Player player)
        {
            uint stream_distance = Server.ServerInstance.Config.Data.stream_distance;
            var streamedPlayers = players.Where(n => { return (Vector3.Distance(n.Value.Position, player.Position) <= (float)stream_distance) &&
                n.Value.IsSpawned && (n.Value.VirtualWorld == player.VirtualWorld) && player.IsSpawned && !player.IsInvisible &&
                !n.Value.IsInvisible && n.Value != player && player.WorldName.Equals(n.Value.WorldName) &&
                player.InvisibleChannel == n.Value.InvisibleChannel; }); 

            var toStream = streamedPlayers.Where(n => !player.StreamedPlayers.Any(n2 => n2.Value.Id == n.Value.Id));
            var toDestream = player.StreamedPlayers.Where(n => !streamedPlayers.Any(n2 => n2.Value.Id == n.Value.Id));

            // Stream
            foreach (var p in toStream)
                StreamPlayerToPlayer(player, p.Value);
            // DeStream
            foreach (var p in toDestream)
                DeStreamPlayerToPlayer(player, p.Value);

            player.StreamedPlayers = streamedPlayers.ToDictionary(n => n.Value.Id, n => n.Value);
        }

        private void StreamPlayerToPlayer(in Player p1, in Player p2)
        {
            //Console.WriteLine($"Streaming {p1.Name} to {p2.Name}");
            var bitStream = new BitStream();
            UInt16 ucFlag = 0;

            if (!p1.Instance.Equals("PC_HERO")) ucFlag |= 1;
            if (!p1.Scale.Equals(new Vector3(1.0f))) ucFlag |= 2;
            if (p1.Fatness != 1.0f) ucFlag |= 4;
            if (!p1.VisualApperance.IsDefault()) ucFlag |= 8;
            if (p1.Armor != -1) ucFlag |= 16;
            if (p1.Helmet != -1) ucFlag |= 32;
            if (p1.Melee != -1) ucFlag |= 64;
            if (p1.Ranged != -1) ucFlag |= 128;
            if (p1.Shield != -1) ucFlag |= 256;
            if (p1.Magic != -1) ucFlag |= 512;
            if (p1.HandItem[0] != -1) ucFlag |= 1024;
            if (p1.HandItem[1] != -1) ucFlag |= 2048;

            bitStream.Write((byte)eNetworkMessage.PLAYER_SPAWN);
            NetStream.WritePedId(ref bitStream, p1.Id);
            bitStream.Write(ucFlag);
            NetStream.WriteVec(ref bitStream, p1.Position);
            NetStream.WriteWm(ref bitStream, (byte)p1.WeaponMode);

            var uSkill = new NetUInt2();
            uSkill.uValue = p1.Skill[0];
            bitStream.WriteBits(ByteSerializer.getBytes(uSkill), 2);
            uSkill.uValue = p1.Skill[1];
            bitStream.WriteBits(ByteSerializer.getBytes(uSkill), 2);
            uSkill.uValue = p1.Skill[2];
            bitStream.WriteBits(ByteSerializer.getBytes(uSkill), 2);

            if ((ucFlag & 1) > 0) bitStream.WriteCompressed(p1.Instance);
            if ((ucFlag & 2) > 0) NetStream.WriteScale(ref bitStream, p1.Scale);
            if ((ucFlag & 4) > 0) NetStream.WriteFatness(ref bitStream, p1.Fatness);
            
            // Visual
            if((ucFlag & 8) > 0)
            {
                bitStream.WriteCompressed(p1.VisualApperance.BodyModel);
                bitStream.WriteCompressed(p1.VisualApperance.HeadModel);
                bitStream.Write(p1.VisualApperance.BodyModel);
                bitStream.Write(p1.VisualApperance.HeadTexture);
            }

            if ((ucFlag & 16) > 0) NetStream.WriteItem(ref bitStream, (uint)p1.Armor);
            if ((ucFlag & 32) > 0) NetStream.WriteItem(ref bitStream, (uint)p1.Helmet);
            if ((ucFlag & 64) > 0) NetStream.WriteItem(ref bitStream, (uint)p1.Melee);
            if ((ucFlag & 128) > 0) NetStream.WriteItem(ref bitStream, (uint)p1.Ranged);
            if ((ucFlag & 256) > 0) NetStream.WriteItem(ref bitStream, (uint)p1.Shield);
            if ((ucFlag & 512) > 0) NetStream.WriteItem(ref bitStream, (uint)p1.Magic);
            if ((ucFlag & 1024) > 0) NetStream.WriteItem(ref bitStream, (uint)p1.HandItem[0]);
            if ((ucFlag & 2048) > 0) NetStream.WriteItem(ref bitStream, (uint)p1.HandItem[1]);

            // Overlay MDS
            foreach (var item in p1.Overlays)
                NetStream.WriteMds(ref bitStream, item);

            Server.ServerInstance.Network.Send(ref bitStream, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, p2.SystemAddress);
        }

        private void DeStreamPlayerToPlayer(in Player p1, in Player p2)
        {
            var bitStream = new BitStream();
            bitStream.Write((byte)eNetworkMessage.PLAYER_UNSPAWN);
            NetStream.WritePedId(ref bitStream, p1.Id);
            Server.ServerInstance.Network.Send(ref bitStream, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, p2.SystemAddress);
        }
    }
}
