using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static G2OServerEmulator.Server;
using RakNet;
using System.Security.Cryptography;
using System.Numerics;
using System.IO;
using System.Globalization;

namespace G2OServerEmulator
{
    class ScriptFunc
    {
        #region ChatFunc
        public static void sendMessageToAll(in int r, in int g, in int b, in string message)
        {
            var bs = new BitStream();
            bs.Write((byte)eNetworkMessage.CHAT_MESSAGE);
            NetStream.WritePedId(ref bs, -1);
            bs.Write((byte)r);
            bs.Write((byte)g);
            bs.Write((byte)b);
            bs.WriteCompressed(message);
            ServerInstance.Network.SendToAll(ref bs, PacketPriority.HIGH_PRIORITY, PacketReliability.RELIABLE);
        }
        public static void sendMessageToPlayer(in int playerID, in int r, in int g, in int b, in string message)
        {
            Player player = null;
            if(ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.CHAT_MESSAGE);
                NetStream.WritePedId(ref bs, -1);
                bs.Write((byte)r);
                bs.Write((byte)g);
                bs.Write((byte)b);
                bs.WriteCompressed(message);
                ServerInstance.Network.Send(ref bs, PacketPriority.HIGH_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static void sendPlayerMessageToPlayer(in int senderID, in int playerID, in int r, in int g, in int b, in string message)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.CHAT_MESSAGE);
                NetStream.WritePedId(ref bs, senderID);
                bs.Write((byte)r);
                bs.Write((byte)g);
                bs.Write((byte)b);
                bs.WriteCompressed(message);
                ServerInstance.Network.Send(ref bs, PacketPriority.HIGH_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static void sendPlayerMessageToAll(in int playerID, in int r, in int g, in int b, in string message)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.CHAT_MESSAGE);
                NetStream.WritePedId(ref bs, playerID);
                bs.Write((byte)r);
                bs.Write((byte)g);
                bs.Write((byte)b);
                bs.WriteCompressed(message);
                ServerInstance.Network.SendToAll(ref bs, PacketPriority.HIGH_PRIORITY, PacketReliability.RELIABLE);
            }
        }
        #endregion
        #region HashFunc
        public static string md5(in string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        public static string sha1(in string input)
        {
            using(var sha1 = SHA1.Create())
            {
                byte[] result = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder();
                foreach (var b in result)
                    sb.Append(b.ToString("X2"));
                return sb.ToString();
            }
        }
        public static string sha256(in string input)
        {
            var Sb = new StringBuilder();

            using (var hash = SHA256.Create())
            { 
                var result = hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                foreach (var b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
        public static string sha384(in string input)
        {
            using (var sha1 = SHA384.Create())
            {
                byte[] result = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder();
                foreach (var b in result)
                    sb.Append(b.ToString("X2"));
                return sb.ToString();
            }
        }
        public static string sha512(in string input)
        {
            using (var sha1 = SHA512.Create())
            {
                byte[] result = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder();
                foreach (var b in result)
                    sb.Append(b.ToString("X2"));
                return sb.ToString();
            }
        }
        #endregion
        #region MathFunc
        public static float getVectorAngle(in float aX, in float aY, in float bX, in float bY)
        {
            if (aX == bX && aY == bY)
                return 0.0f;
            var vec = new Vector3(0.0f);
            vec.X = bX - aX;
            vec.Y = bY - aY;
            vec.Z = (float)Math.Atan(vec.X / vec.Y) * 180.0f / 3.14f;
            if (vec.X < 0)
            {
                if (vec.Z > -180.0f)
                    vec.Z += 180.0f;
                else vec.Z -= 180.0f;
            }
            else if (vec.Z < 0)
                vec.Z = 360.0f - vec.Z;
            return vec.Z;
        }
        #endregion
        #region PlayerFunc
        public static void kick(in int playerID, in string reason)
        {
            Player player = null;
            if(ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.CONNECT_KICK);
                bs.WriteCompressed(reason);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
                ServerInstance.Network.Disconnect(player.SystemAddress);
            }
        }
        public static void ban(in int playerID, in int timestamp, in string reason)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                // TODO: Correct timestamp and ban manager
                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.CONNECT_BANNED);
                bs.WriteCompressed(timestamp);
                bs.WriteCompressed(reason);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
                ServerInstance.Network.Disconnect(player.SystemAddress);
            }
        }
        public static void spawnPlayer(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                player.IsSpawned = true;
        }
        public static void unspawnPlayer(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                player.IsSpawned = false;
        }
        public static bool isPlayerConnected(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Joined;
            return false;
        }
        public static bool isPlayerSpawned(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.IsSpawned;
            return false;
        }
        public static bool isPlayerDead(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.IsDead;
            return false;
        }
        public static bool isPlayerUnconscious(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.IsUnconscious;
            return false;
        }
        public static string getPlayerSerial(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Serial;
            return "NULL";
        }
        public static int getPlayerPing(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Ping;
            return -1;
        }
        public static string getPlayerIP(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.SystemAddress.ToString(false);
            return "NULL";
        }
        public static string getPlayerMacAddr(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.MacAddr;
            return "NULL";
        }
        public static int getPlayerFocus(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Focus;
            return -1;
        }
        public static bool setPlayerName(in int playerID, in string name)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                if (name.Length > 18) return false;
                foreach (var p in ServerInstance.PlayerManager.players)
                    if (p.Value.Name.Equals(name)) return false;

                player.Name = name;
                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_NAME);
                NetStream.WritePedId(ref bs, player.Id);
                bs.WriteCompressed(name);
                ServerInstance.Network.SendJoined(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE);
            }
            return false;
        }
        public static string getPlayerName(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Name;
            return "NULL";
        }
        public static void setPlayerRespawnTime(in int playerID, in int respawnTime)
        {
            if (respawnTime > 1000)
            {
                Player player = null;
                if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                    player.RespawnTime = respawnTime;
            }
        }
        public static int getPlayrRespawnTime(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return (int)player.RespawnTime;
            return -1;
        }
        public static void setPlayerColor(in int playerID, in int R, in int G, in int B)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                player.NameColor = new Color() { r = (byte)R, g = (byte)G, b = (byte)B, a = 255 };

                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_COLOR);
                NetStream.WritePedId(ref bs, playerID);
                bs.Write((byte)R);
                bs.Write((byte)G);
                bs.Write((byte)B);
                ServerInstance.Network.SendJoined(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE);
            }
        }
        public static Color getPlayerColor(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.NameColor;
            return new Color();
        }
        public static void setPlayerPosition(in int playerID, in float x, in float y, in float z)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                player.Position = new Vector3(x, y, z);
                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_POSITION);
                NetStream.WriteVec(ref bs, player.Position);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static void setPlayerPosition(in int playerID, in Vector3 vec)
        {
            setPlayerPosition(playerID, vec.X, vec.Y, vec.Z);
        }
        public static Vector3 getPlayerPosition(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Position;
            return new Vector3(0.0f);
        }
        public static void setPlayerAngle(in int playerID, in float angle)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                player.Angle = angle;

                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_ROTATION);
                NetStream.WriteAngle(ref bs, angle);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static float getPlayerAngle(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Angle;
            return 0.0f;
        }
        public static void setPlayerMaxHealth(in int playerID, in int health)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                player.MaxHealth = health;

                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_MAX_HP);
                NetStream.WriteHp(ref bs, health);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static int getPlayerMaxHealth(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.MaxHealth;
            return 0;
        }
        public static void setPlayerHealth(in int playerID, in int health)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                player.Health = health;

                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_HP);
                NetStream.WriteHp(ref bs, health);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static int getPlayerHealth(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Health;
            return 0;
        }
        public static void setPlayerMaxMana(in int playerID, in int mana)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                player.MaxMana = mana;

                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_MAX_MANA);
                NetStream.WriteHp(ref bs, mana);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static int getPlayerMaxMana(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.MaxMana;
            return 0;
        }
        public static void setPlayerMana(in int playerID, in int mana)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                player.Mana = mana;

                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_MANA);
                NetStream.WriteHp(ref bs, mana);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static int getPlayerMana(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Mana;
            return 0;
        }
        public static int getPlayerMagicLvl(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.MagicLevel;
            return 0;
        }
        public static void setPlayerMagicLevel(in int playerID, in int magicLevel)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                player.MagicLevel = (char)magicLevel;
                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_MAGIC_LVL);
                bs.Write((byte)player.MagicLevel);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static int getPlayerWeaponMode(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.WeaponMode;
            return 0;
        }
        public static void setPlayerWeaponMode(in int playerID, in int weaponMode)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                player.WeaponMode = weaponMode;
                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_WEAPONMODE);
                bs.Write(player.WeaponMode);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static string getPlayerInstance(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Instance;
            return "NULL";
        }
        public static void setPlayerInstance(in int playerID, in string instance)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                player.Instance = instance;

                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_INSTANCE);
                NetStream.WritePedId(ref bs, player.Id);
                bs.WriteCompressed(instance);
                ServerInstance.Network.SendToStreamed(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static string getPlayerWorld(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.WorldName;
            return "NULL";
        }
        public static void setPlayerWorld(in int playerID, in string world)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                player.WorldName = world;
                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_WORLD);
                bs.WriteCompressed(player.WorldName);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static int getPlayerVirtualWorld(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.VirtualWorld;
            return 0;
        }
        public static void setPlayerVirtualWorld(in int playerID, in int world)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                player.VirtualWorld = world;
        }
        public static void setPlayerVisual(in int playerID, in string bodyModel, in int bodyTex, in string headModel, in int headTex)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                player.VisualApperance = new Visual() { BodyModel = bodyModel, BodyTexture = (ushort)bodyTex, HeadModel = headModel, HeadTexture = (ushort)headTex };

                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_VISUAL);
                NetStream.WritePedId(ref bs, player.Id);
                bs.WriteCompressed(bodyModel);
                bs.WriteCompressed(headModel);
                bs.Write((ushort)bodyTex);
                bs.Write((ushort)headTex);
                ServerInstance.Network.SendToStreamed(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static Visual getPlayerVisual(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.VisualApperance;
            return new Visual();
        }
        public static void setPlayerScale(in int playerID, in float x, in float y, in float z)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                player.Scale = new Vector3(x, y, z);

                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_SCALE);
                NetStream.WritePedId(ref bs, playerID);
                NetStream.WriteScale(ref bs, x, y, z);

                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
                ServerInstance.Network.SendToStreamed(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player);
            }
        }
        public static Vector3 getPlayerScale(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Scale;
            return new Vector3(0.0f);
        }
        public static void setPlayerFatness(in int playerID, in float fatness)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                player.Fatness = fatness;

                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_FATNESS);
                NetStream.WritePedId(ref bs, playerID);
                NetStream.WriteFatness(ref bs, fatness);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
                ServerInstance.Network.SendToStreamed(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player);
            }
        }
        public static float getPlayerFatness(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Fatness;
            return 0.0f;
        }
        public static void setPlayerStrength(in int playerID, in int value)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                player.Strength = value;

                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_STR);
                bs.Write((UInt16)value);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static int getPlayerStrength(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Strength;
            return 0;
        }
        public static void setPlayerDexterity(in int playerID, in int value)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                player.Dexterity = value;

                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_DEX);
                bs.Write((UInt16)value);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static int getPlayerDexterity(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Dexterity;
            return 0;
        }
        public static void setPlayerSkillWeapon(in int playerID, in int skill, in int value)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                player.Skill[skill] = (byte)value;

                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_SKILL);
                NetStream.WriteSkill(ref bs, skill, value);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);

                if(player.Skill[skill] != (byte)value)
                {
                    bs.Reset();
                    bs.Write((byte)eNetworkMessage.PLAYER_SKILL);
                    NetStream.WritePedId(ref bs, player.Id);
                    NetStream.WriteSkill(ref bs, skill, value);
                    ServerInstance.Network.SendToStreamed(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player);
                }
            }
        }
        public static int getPlayerSkillWeapon(in int playerID, in int skill)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Skill[skill];
            return 0;
        }
        public static void setPlayerTalent(in int playerID, in int talent, in bool toggle)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                player.Talent[talent] = toggle;

                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_TALENT);
                NetStream.WriteTalent(ref bs, talent, toggle);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static bool getPlayerTalent(in int playerID, in int talent)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Talent[talent];
            return false;
        }
        public static bool applyOverlay(in int playerID, in int overlayID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                bool result = player.ApplyOverlay((uint)overlayID);
                if(result)
                {
                    var bs = new BitStream();
                    bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                    bs.Write((byte)eScriptRPC.SCRIPT_OVERLAY);
                    bs.Write1();
                    NetStream.WriteMds(ref bs, (uint)overlayID);
                    ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);

                    bs.Reset();
                    bs.Write((byte)eNetworkMessage.PLAYER_OVERLAY);
                    NetStream.WritePedId(ref bs, playerID);
                    bs.Write1();
                    NetStream.WriteMds(ref bs, (uint)overlayID);
                    ServerInstance.Network.SendToStreamed(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player);
                }
                return result;
            }
            return false;
        }
        public static bool removeOverlay(in int playerID, in int overlayID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                bool result = player.ApplyOverlay((uint)overlayID);
                if (result)
                {
                    var bs = new BitStream();
                    bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                    bs.Write((byte)eScriptRPC.SCRIPT_OVERLAY);
                    bs.Write0();
                    NetStream.WriteMds(ref bs, (uint)overlayID);
                    ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);

                    bs.Reset();
                    bs.Write((byte)eNetworkMessage.PLAYER_OVERLAY);
                    NetStream.WritePedId(ref bs, playerID);
                    bs.Write0();
                    NetStream.WriteMds(ref bs, (uint)overlayID);
                    ServerInstance.Network.SendToStreamed(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player);
                }
                return result;
            }
            return false;
        }
        public static void playAni(in int playerID, in int aniID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_ANI_ID);
                NetStream.WriteAniId(ref bs, aniID);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static void stopAni(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_ANI_STOP);
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static int getPlayerAniId(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.AniId;
            return -1;
        }
        public static int getPlayerArmor(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Armor;
            return -1;
        }
        public static int getPlayerMeleeWeapon(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Melee;
            return -1;
        }
        public static int getPlayerRangedWeapon(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Ranged;
            return -1;
        }
        public static int getPlayerHelmet(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Helmet;
            return -1;
        }
        public static int getPlayerShield(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.Shield;
            return -1;
        }
        public static void equipArmor(in int playerID, in int armor)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
               if(ServerInstance.ItemManager.Exists(armor))
                {
                    var bs = new BitStream();
                    bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                    bs.Write((byte)eScriptRPC.SCRIPT_ARMOR);
                    bs.Write1();
                    NetStream.WriteItem(ref bs, (uint)armor);
                    ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
                }
            }
        }
        public static void unequipArmor(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_ARMOR);
                bs.Write0();
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static void equipMeleeWeapon(in int playerID, in int melee)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                if (ServerInstance.ItemManager.Exists(melee))
                {
                    var bs = new BitStream();
                    bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                    bs.Write((byte)eScriptRPC.SCRIPT_MELEE_WEAPON);
                    bs.Write1();
                    NetStream.WriteItem(ref bs, (uint)melee);
                    ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
                }
            }
        }
        public static void unequipMeleeWeapon(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_MELEE_WEAPON);
                bs.Write0();
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static void equipRangedWeapon(in int playerID, in int ranged)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                if (ServerInstance.ItemManager.Exists(ranged))
                {
                    var bs = new BitStream();
                    bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                    bs.Write((byte)eScriptRPC.SCRIPT_RANGED_WEAPON);
                    bs.Write1();
                    NetStream.WriteItem(ref bs, (uint)ranged);
                    ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
                }
            }
        }
        public static void unequipRangedWeapon(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_RANGED_WEAPON);
                bs.Write0();
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static void equipHelmet(in int playerID, in int helmet)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                if (ServerInstance.ItemManager.Exists(helmet))
                {
                    var bs = new BitStream();
                    bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                    bs.Write((byte)eScriptRPC.SCRIPT_HELMET);
                    bs.Write1();
                    NetStream.WriteItem(ref bs, (uint)helmet);
                    ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
                }
            }
        }
        public static void unequipHelmet(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_HELMET);
                bs.Write0();
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static void equipShield(in int playerID, in int shield)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                if (ServerInstance.ItemManager.Exists(shield))
                {
                    var bs = new BitStream();
                    bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                    bs.Write((byte)eScriptRPC.SCRIPT_SHIELD);
                    bs.Write1();
                    NetStream.WriteItem(ref bs, (uint)shield);
                    ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
                }
            }
        }
        public static void unequipShield(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_SHIELD);
                bs.Write0();
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static void equipItem(in int playerID, in int item)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                if (ServerInstance.ItemManager.Exists(item))
                {
                    var bs = new BitStream();
                    bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                    bs.Write((byte)eScriptRPC.SCRIPT_EQUIP);
                    bs.Write1();
                    NetStream.WriteItem(ref bs, (uint)item);
                    ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
                }
            }
        }
        public static void unequipItem(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                var bs = new BitStream();
                bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                bs.Write((byte)eScriptRPC.SCRIPT_EQUIP);
                bs.Write0();
                ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
            }
        }
        public static void giveItem(in int playerID, in int itemID, in int amount)
        {
            if (amount < 1) return;
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                if (ServerInstance.ItemManager.Exists(itemID))
                {
                    var bs = new BitStream();
                    bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                    bs.Write((byte)eScriptRPC.SCRIPT_GIVEITEM);
                    bs.WriteCompressed(amount);
                    NetStream.WriteItem(ref bs, (uint)itemID);
                    ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
                }
            }
        }
        public static void removeItem(in int playerID, in int itemID, in int amount)
        {
            if (amount < 1) return;
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
            {
                if (ServerInstance.ItemManager.Exists(itemID))
                {
                    var bs = new BitStream();
                    bs.Write((byte)eNetworkMessage.SCRIPT_RPC);
                    bs.Write((byte)eScriptRPC.SCRIPT_REMOVEITEM);
                    bs.WriteCompressed(amount);
                    NetStream.WriteItem(ref bs, (uint)itemID);
                    ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, player.SystemAddress);
                }
            }
        }
        public static void setPlayerInvisible(in int playerID, in bool toggle)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                player.IsInvisible = toggle;
        }
        public static bool getPlayerInvisible(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.IsInvisible;
            return false; 
        }
        public static void setPlayerInvisibleChannel(in int playerID, in int channel)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                player.InvisibleChannel = channel;
        }
        public static int getPlayerInvisibleChannel(in int playerID)
        {
            Player player = null;
            if (ServerInstance.PlayerManager.players.TryGetValue(playerID, out player))
                return player.InvisibleChannel;
            return -1;
        }
        #endregion
        #region ServerFunc
        public static int getTickCount()
        {
            return (int)Ticks;
        }
        public static string getServerDescription()
        {
            var file = ServerInstance.Config.Data.description_file;
            if (File.Exists(file))
                return File.ReadAllText(file);
            else return "";
        }
        public static void setServerDescription(in string description)
        {
            Network.Network_SetDescription(description.ToCharArray());
        }
        public static string getServerWorld()
        {
            return ServerInstance.Config.Data.world_name;
        }
        /// <summary>
        /// Troche zly sposob na zrobienie tego
        /// </summary>
        /// <param name="world"></param>
        public static void setServerWorld(in string world)
        {
            ServerInstance.Config.Data.world_name = world;
            Network.Network_SetWorld(world.ToCharArray());
        }
        public static int getMaxSlots()
        {
            return (int)ServerInstance.Config.Data.max_slots;
        }
        public static int getPlayersCount()
        {
            return ServerInstance.PlayerManager.players.Count();
        }
        #endregion
        #region UtilityFunc
        public static string rgbToHex(int r, int g, int b)
        {
            if (r < 0) r = 0; if (r > 255) r = 255;
            if (g < 0) g = 0; if (g > 255) g = 255;
            if (b < 0) b = 0; if (b > 255) b = 255;
            return $"{r:X2}{g:X2}{b:X2}";
        }
        public static int[] hexToRgb(string hexColor)
        {
            //Remove # if present
            if (hexColor.IndexOf('#') != -1)
                hexColor = hexColor.Replace("#", "");

            int red = 0;
            int green = 0;
            int blue = 0;

            if (hexColor.Length == 6)
            {
                //#RRGGBB
                red = int.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                green = int.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                blue = int.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);
            }
            else if (hexColor.Length == 3)
            {
                //#RGB
                red = int.Parse(hexColor[0].ToString() + hexColor[0].ToString(), NumberStyles.AllowHexSpecifier);
                green = int.Parse(hexColor[1].ToString() + hexColor[1].ToString(), NumberStyles.AllowHexSpecifier);
                blue = int.Parse(hexColor[2].ToString() + hexColor[2].ToString(), NumberStyles.AllowHexSpecifier);
            }
            return new int[3] { red, green, blue };
        }
        /// <summary>
        /// My own (untested) implementation
        /// </summary>
        public static List<object> sscanf(string format, string text)
        {
            var resultList = new List<object>();
            if(format.Length > 0 && text.Length > 0)
            {
                var strings = text.Split(' ');
                if (strings.Count() == format.Length)
                {
                    int iter = 0;
                    foreach (var c in format)
                    {
                        switch(c)
                        {
                            case 'd': resultList.Add(Int32.Parse(strings[iter])); break;
                            case 'f': resultList.Add(float.Parse(strings[iter])); break;
                            case 's': resultList.Add(strings[iter]); break;
                        }
                        ++iter;
                    }
                }
            }
            return resultList;
        }
        #endregion
    }
}
