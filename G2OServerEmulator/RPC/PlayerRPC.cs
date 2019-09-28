using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using RakNet;
using static System.Console;
using static G2OServerEmulator.Server;

namespace G2OServerEmulator
{
    internal class PlayerRPC
    {
        public static void Incoming(ref Packet packet, ref BitStream bitStream, in Network network)
        {
            WriteLine("[net] Incoming connection from: {0}...", packet.systemAddress.ToString(true, ':'));
        }

        public static void Disconnect(ref Packet packet, ref BitStream bitStream, in Network network)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(packet.systemAddress.systemIndex, out player))
            {
                //onPlayerDisconnect
                ServerInstance.EventManager.CallEvent("onPlayerDisconnect", player.Id);
                bitStream.Reset();

                bitStream.Write((byte)eNetworkMessage.PLAYER_DESTROY);
                NetStream.WritePedId(ref bitStream, packet.systemAddress.systemIndex);
                network.SendToAll(ref bitStream, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE);

                WriteLine($"[net] Player {player.Name} disconnected from server.");
                player.Connected = false;
                player.Joined = false;

                ServerInstance.PlayerManager.Destroy(packet.systemAddress.systemIndex);
            }
        }

        public static void Connect(ref Packet packet, ref BitStream bitStream, in Network network)
        {
            var systemAddress = packet.systemAddress;

            Int32 version;
            byte uBuild;
            string serial;
            string macAddr;
            string name;

            bitStream.Read(out version);
            bitStream.Read(out uBuild);
            bitStream.ReadCompressed(out serial);
            bitStream.ReadCompressed(out macAddr);
            bitStream.ReadCompressed(out name);

            //WriteLine($"Name: {name} Serial: {serial} Mac: {macAddr}");
            if (uBuild < (byte)ServerInstance.Config.Data.version_build
                || version != ((Version.Major << 16) + (Version.Minor << 8) + Version.Patch))
            {
                WriteLine($"Player {name} using incorrect version!");
                bitStream.Reset();
                bitStream.Write((byte)eNetworkMessage.CONNECT_WRONG_VERSION);

                network.Send(ref bitStream, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, in systemAddress);
                network.Disconnect(in systemAddress);
                return;
            }

            // System banów

            // Sprawdzanie czy playerManager nie jest pełny (max_slots)
            if (ServerInstance.PlayerManager.players.Count() >= ServerInstance.Config.Data.max_slots)
            {
                WriteLine($"Player {name} is trying to join full server.");

                bitStream.Reset();
                bitStream.Write((byte)eNetworkMessage.CONNECT_FULL);

                network.Send(ref bitStream, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, in systemAddress);
                network.Disconnect(in systemAddress);
                return;
            }
            // Czy nazwa gracza już nie istnieje? (w PlayerManager)
            if (ServerInstance.PlayerManager.Exist(name))
            {
                WriteLine($"Nickname {name} is already used!");

                bitStream.Reset();
                bitStream.Write((byte)eNetworkMessage.CONNECT_NICKNAME_USED);

                network.Send(ref bitStream, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, in systemAddress);
                network.Disconnect(in systemAddress);
                return;
            }
            // Tworzenie gracza (playerManager)
            Player player = null;
            if (ServerInstance.PlayerManager.Create(systemAddress.systemIndex, name, out player))
            {
                player.SystemAddress = new SystemAddress(systemAddress.ToString(true));
                player.Serial = serial;
                player.MacAddr = macAddr;

                WriteLine($"{name} has been connected from: {systemAddress.ToString(true, ':')}");
                // TIP: uważać na typy w bitstreamie, każdy typ z C++ ma swój rozmiarowo-zgodny odpowiednik w C#
                bitStream.Reset();
                bitStream.Write((byte)eNetworkMessage.CONNECT_ACCEPT);
                bitStream.WriteCompressed(ServerInstance.Config.Data.hostname);
                bitStream.WriteCompressed(ServerInstance.Config.Data.world_name);
                bitStream.Write((ushort)ServerInstance.Config.Data.max_slots);
                bitStream.Write(systemAddress.systemIndex);
                var time = ServerInstance.TimeController;
                // Time
                bitStream.Write((byte)time.Day); //Day
                bitStream.Write((byte)time.Hour); //Hour
                bitStream.Write((byte)time.Minute); //Min
                bitStream.Write(time.MinuteLength); //Min len
                // Zero reakcji clienta obecnie
                ServerInstance.Network.Send(ref bitStream, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, in systemAddress);
                ServerInstance.Network.Ping(in systemAddress);

                player.Connected = true;
                //onPlayerConnect
                ServerInstance.EventManager.CallEvent("onPlayerConnect", player.Id);
            }
            else
            {
                WriteLine($"[error] Cannot create player {name}");
                ServerInstance.Network.Disconnect(in systemAddress);
            }
        }

        public static void Join(ref Packet packet, ref BitStream bitStream, in Network network)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(packet.systemAddress.systemIndex, out player))
            {
                WriteLine($"[net] Player {player.Name} joined the server!");

                // playerCreate
                bitStream.Reset();

                bitStream.Write((byte)eNetworkMessage.PLAYER_CREATE);
                NetStream.WritePedId(ref bitStream, packet.systemAddress.systemIndex);
                bitStream.WriteCompressed(player.Name);
                bitStream.Write(player.NameColor.r);
                bitStream.Write(player.NameColor.g);
                bitStream.Write(player.NameColor.b);

                network.SendToAll(ref bitStream, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE);

                // Zapomniałby o tworzeniu innych graczy dla tego gracza
                foreach(var other in ServerInstance.PlayerManager.players)
                {
                    if(other.Value != player)
                    {
                        bitStream.Reset();
                        bitStream.Write((byte)eNetworkMessage.PLAYER_CREATE);
                        NetStream.WritePedId(ref bitStream, other.Value.Id);
                        bitStream.WriteCompressed(other.Value.Name);
                        bitStream.Write(other.Value.NameColor.r);
                        bitStream.Write(other.Value.NameColor.g);
                        bitStream.Write(other.Value.NameColor.b);
                        network.Send(ref bitStream, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
                    }
                }
                player.Joined = true;
                // TYMCZASOWO USTAWIAM TUTAJ SPAWNED NA TRUE BO CHYBA NORMALNIE TO SIE W SKRYPTACH DAJE
                player.IsSpawned = true;
                // onPlayerJoin(int playerid)
                ServerInstance.EventManager.CallEvent("onPlayerJoin", player.Id);
            }
        }

        public static void SyncFoot(ref Packet packet, ref BitStream bitStream, in Network network)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(packet.systemAddress.systemIndex, out player))
            {
                // Is player alive?
                if (bitStream.ReadBit())
                {
                    float x, y, z, angle, lookAtX, lookAtY;
                    int hp, maxHp;
                    byte weaponMode;
                    string faceAni;

                    player.StateData.Dead = false;
                    player.StateData.Unconscious = false;

                    if (!NetStream.ReadVec(ref bitStream, out x, out y, out z) ||
                        !NetStream.ReadAngle(ref bitStream, out angle) ||
                        !NetStream.ReadHp(ref bitStream, out hp) ||
                        !NetStream.ReadHp(ref bitStream, out maxHp) ||
                        !NetStream.ReadWm(ref bitStream, out weaponMode) ||
                        !bitStream.ReadFloat16(out lookAtX, 0.0f, 1.0f) ||
                        !bitStream.ReadFloat16(out lookAtY, 0.0f, 1.0f) ||
                        !bitStream.Read(out faceAni))
                        return;

                    player.Position.X = x;
                    player.Position.Y = y;
                    player.Position.Z = z;
                    //WriteLine($"Player {player.Name} X: {x} Y: {y} Z: {z}");
                    player.Angle = angle;

                    if (player.Health != hp) player.Health = hp;
                    if (player.MaxHealth != maxHp) player.UpdateMaxHealth(maxHp);
                    if (player.WeaponMode != weaponMode) player.UpdateWeaponMode(weaponMode);

                    player.FaceAni = faceAni;
                }
                else if (!player.StateData.Dead)
                {
                    if (!player.IsDead) player.Health = 0;
                    player.StateData.Dead = true;
                }

                // Update focus
                if (bitStream.ReadBit())
                {
                    int focusId;
                    if (!NetStream.ReadPedId(ref bitStream, out focusId))
                        return;
                    if (player.Focus != focusId) player.UpdateFocus(focusId);
                }
                else if (player.Focus != -1)
                    player.UpdateFocus(-1);

                // Mana XDDDDDDDDDDDDDDD
                if (bitStream.ReadBit())
                {
                    int mana, maxMana;
                    if (!NetStream.ReadHp(ref bitStream, out mana) ||
                        !NetStream.ReadHp(ref bitStream, out maxMana))
                        return;
                    if (player.Mana != mana) player.Mana = mana;
                    if (player.MaxMana != maxMana) player.MaxMana = maxMana;
                }

                // Body state
                if (bitStream.ReadBit())
                {
                    byte bodyState;
                    if (!bitStream.Read(out bodyState))
                        return;
                    if (player.BodyState != bodyState) player.BodyState = bodyState;
                }
                player.SyncPure(bitStream);
            }
        }
        public static void Anis(ref Packet packet, ref BitStream bitStream, in Network network)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(packet.systemAddress.systemIndex, out player))
            {
                if (!player.IsDead)
                {
                    int aniId;
                    float aniPercent;
                    while (NetStream.ReadAniId(ref bitStream, out aniId))
                    {
                        if (!bitStream.ReadFloat16(out aniPercent, -1.0f, 1.0f)) break;
                        player.AniId = aniId;
                    }
                    player.SyncAnis(bitStream);
                }
            }
        }

        public static void Wear(ref Packet packet, ref BitStream bitStream, in Network network)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(packet.systemAddress.systemIndex, out player))
            {
                UInt16 ucFlags = 0;
                bitStream.Read(ucFlags);
                if ((ucFlags & 1) > 0)
                {
                    uint instance;
                    if (!NetStream.ReadItem(ref bitStream, out instance))
                        return;
                    player.UpdateArmor((int)instance);
                }
                if ((ucFlags & 2) > 0)
                {
                    uint instance;
                    if (!NetStream.ReadItem(ref bitStream, out instance))
                        return;
                    player.UpdateHelmet((int)instance);
                }
                if ((ucFlags & 4) > 0)
                {
                    uint instance;
                    if (!NetStream.ReadItem(ref bitStream, out instance))
                        return;
                    player.UpdateMeleeWeapon((int)instance);
                }
                if ((ucFlags & 8) > 0)
                {
                    uint instance;
                    if (!NetStream.ReadItem(ref bitStream, out instance))
                        return;
                    player.UpdateRangedWeapon((int)instance);
                }
                if ((ucFlags & 16) > 0)
                {
                    uint instance;
                    if (!NetStream.ReadItem(ref bitStream, out instance))
                        return;
                    player.UpdateShield((int)instance);
                }
                if ((ucFlags & 32) > 0)
                {
                    uint instance;
                    if (!NetStream.ReadItem(ref bitStream, out instance))
                        return;
                    player.UpdateMagic((int)instance);
                }
                if ((ucFlags & 64) > 0)
                {
                    uint instance;
                    if (!NetStream.ReadItem(ref bitStream, out instance))
                        return;
                    player.UpdateHand(Hand.LEFT, (int)instance);
                }
                if ((ucFlags & 128) > 0)
                {
                    uint instance;
                    if (!NetStream.ReadItem(ref bitStream, out instance))
                        return;
                    player.UpdateHand(Hand.RIGHT, (int)instance);
                }
            }
        }
        public static void Hit(ref Packet packet, ref BitStream bitStream, in Network network)
        {
            Player killer = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(packet.systemAddress.systemIndex, out killer))
            {
                int id;
                byte ucFlags;
                int deltaHp;
                bool bDontKill;
                if (!NetStream.ReadPedId(ref bitStream, out id) ||
                    !bitStream.Read(out ucFlags) ||
                    !bitStream.Read(out deltaHp) ||
                    !bitStream.Read(out bDontKill))
                    return;

                Player player = null;
                if (ServerInstance.PlayerManager.players.TryGetValue(id, out player))
                    player.Hit(packet.systemAddress.systemIndex, ucFlags, deltaHp, bDontKill);
            }
        }
        public static void MagicCast(ref Packet packet, ref BitStream bitStream, in Network network)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(packet.systemAddress.systemIndex, out player))
                player.MagicCast();
        }
        public static void Overlay(ref Packet packet, ref BitStream bitStream, in Network network)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(packet.systemAddress.systemIndex, out player))
            {
                bool bToggle;
                uint MdsId;
                bool bResult = false;

                if (!bitStream.Read(out bToggle) ||
                !NetStream.ReadMds(ref bitStream, out MdsId))
                    return;

                if (bToggle)
                    bResult = player.ApplyOverlay(MdsId);
                else bResult = player.RemoveOverlay(MdsId);

                if (!bResult) return;

                bitStream.Reset();
                bitStream.Write((byte)eNetworkMessage.PLAYER_OVERLAY);
                NetStream.WritePedId(ref bitStream, packet.systemAddress.systemIndex);
                bitStream.Write(bToggle);
                NetStream.WriteMds(ref bitStream, MdsId);
                ServerInstance.Network.SendToStreamed(ref bitStream, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player);
            }
        }
        public static void ChangeWorld(ref Packet packet, ref BitStream bitStream, in Network network)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(packet.systemAddress.systemIndex, out player))
            {
                string world;
                if (!bitStream.ReadCompressed(out world))
                    return;
                //onPlayerChangeWorld(id, world)
                ServerInstance.EventManager.CallEvent("onPlayerChangeWorld", player.Id, player.WorldName, world);
                player.WorldName = world; //Nic nie wysyłamy, streamer zjamie się resztą ;))
            }
        }
        public static void EnterWorld(ref Packet packet, ref BitStream bitStream, in Network network)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(packet.systemAddress.systemIndex, out player))
            {
                //onPlayerEnterWorld(playerid)
                ServerInstance.EventManager.CallEvent("onPlayerEnterWorld", player.Id);
            }
        }
        public static void MobInteract(ref Packet packet, ref BitStream bitStream, in Network network)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(packet.systemAddress.systemIndex, out player))
            {
                int from, to;
                Vector3 pos;
                if (!bitStream.Read(out from) ||
                    !bitStream.Read(out to) ||
                    !NetStream.ReadVec(ref bitStream, out pos))
                    return;

                //onPlayerMobInteract(playerId, from, to)
                ServerInstance.EventManager.CallEvent("onPlayerMobInteract", player.Id, from, to);
                bitStream.Reset();
                bitStream.Write((byte)eNetworkMessage.PLAYER_MOB);
                NetStream.WritePedId(ref bitStream, packet.systemAddress.systemIndex);
                bitStream.Write(from);
                bitStream.Write(to);
                NetStream.WriteVec(ref bitStream, pos);
                ServerInstance.Network.SendToStreamed(ref bitStream, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player);
            }
        }
    }
}
