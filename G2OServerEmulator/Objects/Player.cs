using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RakNet;
using static G2OServerEmulator.Server;

namespace G2OServerEmulator
{
    public class Player : Npc
    {
        public class syncData
        {
            public bool Name { get; set; }
            public bool Color { get; set; }
            public bool WeaponMode { get; set; }
            public bool Armor { get; set; }
            public bool Helmet { get; set; }
            public bool MeleeWeapon { get; set; }
            public bool RangedWeapon { get; set; }
            public bool Shield { get; set; }
            public bool Magic { get; set; }
            public bool[] HandSync { get; set; }
            public bool Instance { get; set; }
            public bool Scale { get; set; }
            public bool Fatness { get; set; }
            public bool Visual { get; set; }
        }

        public class stateData
        {
            public bool Dead { get; set; }
            public bool Unconscious { get; set; }
        }
        
        public syncData SyncData { get; set; }
        public stateData StateData { get; set; }
        public Dictionary<int, Player> StreamedPlayers = new Dictionary<int, Player>();
        public List<int> StreamedItems = new List<int>();
        public int VirtualWorld { get; set; }
        private int invisibleChannel; //get set tego wymaga :/
        public int InvisibleChannel { get { return invisibleChannel; } set {
                if (value >= 0) invisibleChannel = value;
            }
        }
        public int Ping { get; set; }

        public bool IsInvisible { get; set; }
        public bool Joined { get; set; }
        public bool Connected { get; set; }
        public bool Pinged { get; set; }

        public long RespawnTime { get; set; }
        public long RespawnDelay { get; set; }
        public long PingTimer { get; set; }
        public long SyncTimer { get; set; }
        public long AnisTimer { get; set; }
        public long VisualTimer { get; set; }
        public long UpdateTimer { get; set; }
        public long MiscTimer { get; set; }
        public long ScriptTimer { get; set; }

        public UInt16 NameSum { get; set; }
        public UInt16 WorldSum { get; set; }

        public SystemAddress SystemAddress { get; set; }
        public string Serial { get; set; }
        public string MacAddr { get; set; }

        //public List<int> Equipment;

        public Player(in int id, in string name) : base(id, name)
        {
            VirtualWorld = 0; InvisibleChannel = 0; Ping = 0;
            IsInvisible = false; Joined = false; Connected = true;
            Pinged = false; RespawnTime = 1000; RespawnDelay = 0;
            PingTimer = 0; SyncTimer = 0; VisualTimer = 0;
            UpdateTimer = 0; MiscTimer = 0; ScriptTimer = 0;
            AnisTimer = 0; NameSum = 0; WorldSum = 0;

            //m_uWorldSum = utl::hashString(m_sWorld.c_str()); // Wydaje mi sie ze to nie bedzie potrzebne
            StateData = new stateData() { Dead = false, Unconscious = false };
            SyncData = new syncData()
            {
                Name = false,
                Color = false,
                WeaponMode = false,
                Armor = false,
                Helmet = false,
                MeleeWeapon = false,
                RangedWeapon = false,
                Shield = false,
                Magic = false,
                HandSync = new bool[2] { false, false },
                Instance = false,
                Fatness = false,
                Scale = false,
                Visual = false
            };
        }

        public void EnterWorld()
        {
            //Callback do modułu
            //onPlayerEnterWorld
            ServerInstance.EventManager.CallEvent("onPlayerEnterWorld", Id);
        }
        /// <summary>
        /// Funkcja wywoływana przy odbieraniu pakietów
        /// </summary>
        /// <param name="maxHp"></param>
        public void UpdateMaxHealth(in int maxHp)
        {
            ServerInstance.EventManager.CallEvent("onPlayerChangeMaxHealth", Id, MaxHealth, maxHp);
            MaxHealth = maxHp;
            //Callback do modułu
            //onPlayerChangeMaxHealth
        }
        /// <summary>
        /// Funkcja wywoływana przy odbieraniu pakietów
        /// </summary>
        /// <param name="wM"></param>
        public void UpdateWeaponMode(in int wM)
        {
            int oldWm = base.WeaponMode;
            WeaponMode = wM;
            SyncData.WeaponMode = true;
            //onPlayerChangeWeaponMode
            ServerInstance.EventManager.CallEvent("onPlayerChangeWeaponMode", Id, oldWm, WeaponMode);
        }
        /// <summary>
        /// Funkcja wywoływana przy odbieraniu pakietów
        /// </summary>
        /// <param name="focusId"></param>
        public void UpdateFocus(in int focusId)
        {
            int oldFid = base.Focus;
            Focus = focusId;
            //onPlayerChangeFocus
            ServerInstance.EventManager.CallEvent("onPlayerChangeFocus", Id, oldFid, Focus);
        }
        /// <summary>
        /// Funkcja wywoływana przy odbieraniu pakietów
        /// </summary>
        /// <param name="world"></param>
        public void UpdateWorld(in string world)
        {
             try
             {
                ServerInstance.EventManager.CallEvent("onPlayerChangeWorld", Id, WorldName, world);
                WorldName = world;
                 //onPlayerChangeWorld
             }
             catch(Exception e)
             {
                 Console.WriteLine(e);
             }
        }
        /// <summary>
        /// Wywołuje callback do modułu
        /// Clientside serio może zmieniać swój kolor???
        /// </summary>
        /// <param name="color"></param>
        public void UpdateColor(in Color color)
        {
            NameColor = color;
            //onPlayerChangeColor
            ServerInstance.EventManager.CallEvent("onPlayerChangeColor", Id, (int)color.r, (int)color.g, (int)color.b);
        }
        /// <summary>
        /// Wywoluje callback do modulu i cos tam jeszcze robi
        /// </summary>
        /// <param name="health"></param>
        public void UpdateHealth(in int health)
        {
            if(!IsDead)
            {
                int oldHp = Health;
                Health = health;
                //onPlayerChangeHealth
                ServerInstance.EventManager.CallEvent("onPlayerChangeHealth", Id, oldHp, health);
                if(IsDead)
                {
                    //onPlayerDead(playerid, killerid)
                    ServerInstance.EventManager.CallEvent("onPlayerDead", Id, -1);
                    Overlays.Clear();

                    RespawnDelay = Ticks + RespawnDelay;
                }
            }
        }
        /// <summary>
        /// Funkcja wywolywana przy odbiorze pakietow
        /// </summary>
        /// <param name="armor"></param>
        public void UpdateArmor(in int armor)
        {
            if (armor == Armor) return;
            Armor = armor;
            SyncData.Armor = true;
            //onPlayerEquipArmor
            ServerInstance.EventManager.CallEvent("onPlayerEquipArmor", Id, armor);
        }
        /// <summary>
        /// funkcja wywolywana przy odbiorze pakietow
        /// </summary>
        /// <param name="helmet"></param>
        public void UpdateHelmet(in int helmet)
        {
            if (helmet == Helmet) return;
            Helmet = helmet;
            SyncData.Helmet = true;
            //onPlayerEquipHelmet
            ServerInstance.EventManager.CallEvent("onPlayerEquipHelmet", Id, helmet);
        }
        /// <summary>
        /// funkcja wywolywana przy odbiorze pakietow
        /// </summary>
        /// <param name="weapon"></param>
        public void UpdateMeleeWeapon(in int melee)
        {
            if (melee == Melee) return;
            Melee = melee;
            SyncData.MeleeWeapon = true;
            //onPlayerEquipMeleeWeapon
            ServerInstance.EventManager.CallEvent("onPlayerEquipMeleeWeapon", Id, melee);
        }
        /// <summary>
        /// funkcja wywolywana przy odbiorze pakietow
        /// </summary>
        /// <param name="weapon"></param>
        public void UpdateRangedWeapon(in int ranged)
        {
            if (ranged == Ranged) return;
            Ranged = ranged;
            SyncData.RangedWeapon = true;
            //onPlayerEquipRangedWeapon
            ServerInstance.EventManager.CallEvent("onPlayerEquipRangedWeapon", Id, ranged);
        }
        /// <summary>
        /// funkcja wywolywana przy odbiorze pakietow
        /// </summary>
        /// <param name="weapon"></param>
        public void UpdateShield(in int shield)
        {
            if (shield == Shield) return;
            Shield = shield;
            SyncData.Shield = true;
            //onPlayerEquipShield
            ServerInstance.EventManager.CallEvent("onPlayerEquipShield", Id, shield);
        }
        /// <summary>
        /// funkcja wywolywana przy odbiorze pakietow
        /// </summary>
        /// <param name="weapon"></param>
        public void UpdateHand(in Hand hand, in int handItem)
        {
            if (handItem == HandItem[(int)hand]) return;
            if(!(handItem == Melee || handItem == Ranged) 
                || handItem == -1)
            {
                HandItem[(int)hand] = handItem;
                SyncData.HandSync[(int)hand] = true;
            }
            //onPlayerEquipHandItem
            ServerInstance.EventManager.CallEvent("onPlayerEquipHandItem", Id, (int)hand, handItem);
        }

        public void UpdateMagic(in int magic)
        {
            if (Magic == magic) return;
            Magic = magic;
            SyncData.Magic = true;
            //onPlayerSpellSetup(id, item)
            ServerInstance.EventManager.CallEvent("onPlayerSpellSetup", Id, magic);
        }
        /// <summary>
        /// funkcja wywolywana przy odbiorze pakietow
        /// </summary>
        public void Hit(in int id, in byte dmgFlag, int dmg, in bool bDontKill)
        {
            KillerId = id;
            //Call do modułu onPlayerHit(playerId, killedId, dmg, dmgFlag)

            if (!IsDead)
            {
                int eventValue = ServerInstance.EventManager.CallEvent("onPlayerHit", Id, id, dmg, dmgFlag);
                if (eventValue == 0)
                {
                    Health += dmg;

                    var bs = new BitStream();

                    bs.Write((byte)eNetworkMessage.PLAYER_HIT);
                    NetStream.WritePedId(ref bs, KillerId);
                    bs.WriteCompressed(dmg);

                    ServerInstance.Network.Send(ref bs, PacketPriority.HIGH_PRIORITY, PacketReliability.RELIABLE_ORDERED, SystemAddress);
                }   // eventValue to dmg teraz
                else if(eventValue > 0)
                {
                    Health += eventValue;

                    var bs = new BitStream();

                    bs.Write((byte)eNetworkMessage.PLAYER_HIT);
                    NetStream.WritePedId(ref bs, KillerId);
                    bs.WriteCompressed(dmg);

                    ServerInstance.Network.Send(ref bs, PacketPriority.HIGH_PRIORITY, PacketReliability.RELIABLE_ORDERED, SystemAddress);
                } // Jak eventValue to -1 (cancel event) to sie nie wywoluje
            }
            KillerId = -1;
            /*
             * 	SqEvent.m_iValue = dmg;
	            m_iKillerId = id;

	            SqArgs args;
	            args.pushInt(getId());
	            args.pushInt(m_iKillerId);
	            args.pushInt(dmg);
	            args.pushInt(dmgFlag);
	
	            if (!SqMan.call("onPlayerHit", args))
	            {
		            if (!m_bIsDead && SqEvent.m_iValue)
		            {
			            HpControl = m_iHp + SqEvent.m_iValue;
			            if (HpControl < 0) HpControl = 0;

			            setHealth(HpControl);

			            NetStream nsOut;
			            nsOut.Write((MessageID)PLAYER_HIT);
			            nsOut.WritePedId(m_iKillerId);
			            nsOut.WriteCompressed(int16_t(SqEvent.m_iValue));

			            Server.Network.send(nsOut, HIGH_PRIORITY, RELIABLE_ORDERED, getAddr());
		            }
	            }

	            // Restart killer
	            m_iKillerId = -1;

             */
        }
        /// <summary>
        /// Funkcja wywolywana przy odbiorze pakietow
        /// </summary>
        public void MagicCast()
        {
            if(Magic != -1)
            {
                //onPlayerSpellCast
                ServerInstance.EventManager.CallEvent("onPlayerSpellCast", Id);
                var bitStream = new BitStream();
                bitStream.Write((byte)eNetworkMessage.PLAYER_MAGIC);
                NetStream.WritePedId(ref bitStream, Id);
                ServerInstance.Network.SendToStreamed(ref bitStream, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, this);
            }
        }
        /// <summary>
        /// Funkcja wywolywana przy odbiorze pakietow
        /// </summary>
        public void Respawn()
        {
            restart();
            IsSpawned = false;

            BitStream bs = new BitStream();
            bs.Write((byte)eNetworkMessage.PLAYER_RESPAWN);
            ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, SystemAddress);
            bs.Reset();

            SyncData.Name = false;
            SyncData.Color = false;
            SyncData.WeaponMode = false;
            SyncData.Armor = false;
            SyncData.Helmet = false;
            SyncData.MeleeWeapon = false;
            SyncData.RangedWeapon = false;
            SyncData.Shield = false;
            SyncData.HandSync[(int)Hand.LEFT] = false;
            SyncData.HandSync[(int)Hand.RIGHT] = false;
            SyncData.Instance = false;
            SyncData.Scale = false;
            SyncData.Fatness = false;
            SyncData.Visual = false;

            IsActive = false;
            //onPlayerRespawn
            ServerInstance.EventManager.CallEvent("onPlayerRespawn", Id);
            IsActive = true;
            //IsSpawned = true tymczasosow
            IsSpawned = true;
            //correctStream - jakies poprawienie streamera po resecie pozycji postaci

            UInt16 ucFlag = 0;
            if (Instance != "PC_HERO") ucFlag |= 1;
            if (Scale.X != 1.0f || Scale.Y != 1.0f || Scale.Z != 1.0f) ucFlag |= 2;
            if (Fatness != 1.0f) ucFlag |= 4;
            if (!VisualApperance.IsDefault()) ucFlag |= 8;

            
            bs.Write((byte)eNetworkMessage.PLAYER_RESET);
            // ID
            NetStream.WritePedId(ref bs, Id);
            // Flagi bitowe
            bs.Write(ucFlag);
            // Pozycja X Y Z
            NetStream.WriteVec(ref bs, Position);
            // Skill
            var skill = new NetUInt2();
            skill.uValue = Skill[0];
            bs.WriteBits(ByteSerializer.getBytes(skill), 2);
            skill.uValue = Skill[1];
            bs.WriteBits(ByteSerializer.getBytes(skill), 2);
            skill.uValue = Skill[2];
            bs.WriteBits(ByteSerializer.getBytes(skill), 2);
            skill.uValue = Skill[3];
            bs.WriteBits(ByteSerializer.getBytes(skill), 2);
            if ((ucFlag & 1) > 0) bs.WriteCompressed(Instance);
            if ((ucFlag & 2) > 0) NetStream.WriteScale(ref bs, Scale);
            if ((ucFlag & 3) > 0) NetStream.WriteFatness(ref bs, Fatness);

            // Visual
            if((ucFlag & 8) > 0)
            {
                bs.WriteCompressed(VisualApperance.BodyModel);
                bs.WriteCompressed(VisualApperance.HeadModel);
                bs.Write(VisualApperance.BodyTexture);
                bs.Write(VisualApperance.HeadTexture);
            }
           
            //Add overlay mds to stream
            foreach (var i in Overlays)
                NetStream.WriteMds(ref bs, i);
            //Send to streamed players
            ServerInstance.Network.SendToStreamed(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, this);
        }

        public void Sync()
        {
            if(Joined)
            {
                SyncPing();
                SyncUpdate();
                SyncVisual();

                if (!IsDead)
                    SyncWear();
                else
                    if (Ticks > RespawnDelay) Respawn();
            }
        }

        public void SyncPing()
        {
            if(Ticks > PingTimer)
            {
                if(Pinged)
                {
                    Ping = ServerInstance.Network.GetPing(SystemAddress);

                    var bs = new BitStream();
                    bs.Write((byte)eNetworkMessage.PLAYER_PING);
                    NetStream.WritePedId(ref bs, Id);
                    bs.WriteCompressed(Ping);
                    ServerInstance.Network.SendJoined(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.UNRELIABLE);

                    PingTimer = Ticks + 3000;
                    Pinged = false;
                }
                else
                {
                    ServerInstance.Network.Ping(SystemAddress);

                    PingTimer = Ticks + 1000;
                    Pinged = true;
                }
            }
        }

        public void SyncPure(in BitStream bitStream)
        {
            if (Ticks > SyncTimer)
            {
                if (IsSpawned)
                {
                    var bs = new BitStream();
                    bs.Write((byte)eNetworkMessage.PLAYER_SYNC);
                    NetStream.WritePedId(ref bs, Id);
                    bitStream.ResetReadPointer();
                    bitStream.IgnoreBytes(1);
                    bs.Write(bitStream);
                    ServerInstance.Network.SendToStreamed(ref bs, PacketPriority.MEDIUM_PRIORITY, PacketReliability.UNRELIABLE_SEQUENCED, this);
                }
                SyncTimer = Ticks + 80;
            }
        }
        
        public void SyncAnis(in BitStream bitStream)
        {
            if(Ticks > AnisTimer)
            {
                if(IsSpawned)
                {
                    var bs = new BitStream();
                    bs.Write((byte)eNetworkMessage.PLAYER_ANI);
                    NetStream.WritePedId(ref bs, Id);
                    bitStream.ResetReadPointer();
                    bitStream.IgnoreBytes(1);
                    bs.Write(bitStream);

                    ServerInstance.Network.SendToStreamed(ref bs, PacketPriority.MEDIUM_PRIORITY, PacketReliability.UNRELIABLE, this);
                }
                AnisTimer = Ticks + 20;
            }
        }

        public void SyncUpdate()
        {
            if(Ticks > UpdateTimer)
            {
                UInt16 ucFlag = 0;

                if (SyncData.Name) ucFlag |= 1;
                if (SyncData.Color) ucFlag |= 2;

                if(ucFlag != 0)
                {
                    var bs = new BitStream();

                    bs.Write((byte)eNetworkMessage.PLAYER_UPDATE);
                    NetStream.WritePedId(ref bs, Id);
                    bs.Write(ucFlag);

                    if(SyncData.Name)
                    {
                        bs.WriteCompressed(Name);
                        SyncData.Name = false;
                    }
                    if(SyncData.Color)
                    {
                        bs.Write(NameColor.r);
                        bs.Write(NameColor.g);
                        bs.Write(NameColor.b);

                        SyncData.Color = false;
                    }
                    ServerInstance.Network.SendToAll(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE);
                }
                UpdateTimer = Ticks + 400;
            }
        }

        public void SyncVisual()
        {
            if(Ticks > VisualTimer)
            {
                UInt16 ucFlag = 0;
                if (SyncData.Instance) ucFlag |= 1;
                if (SyncData.Scale) ucFlag |= 2;
                if (SyncData.Fatness) ucFlag |= 4;
                if (SyncData.Visual) ucFlag |= 8;

                if(ucFlag != 0)
                {
                    var bs = new BitStream();

                    bs.Write((byte)eNetworkMessage.PLAYER_VISUAL);
                    NetStream.WritePedId(ref bs, Id);
                    bs.Write(ucFlag);

                    if(SyncData.Instance)
                    {
                        bs.WriteCompressed(Instance);
                        SyncData.Instance = false;
                    }

                    if(SyncData.Scale)
                    {
                        NetStream.WriteScale(ref bs, Scale);
                        SyncData.Scale = false;
                    }

                    if(SyncData.Fatness)
                    {
                        NetStream.WriteFatness(ref bs, Fatness);
                        SyncData.Fatness = false;
                    }

                    if(SyncData.Visual)
                    {
                        bs.WriteCompressed(VisualApperance.BodyModel);
                        bs.WriteCompressed(VisualApperance.HeadModel);
                        bs.Write(VisualApperance.BodyTexture);
                        bs.Write(VisualApperance.HeadTexture);
                        SyncData.Visual = false;
                    }
                    ServerInstance.Network.SendToStreamed(ref bs, PacketPriority.MEDIUM_PRIORITY, PacketReliability.RELIABLE, this);
                    ServerInstance.Network.Send(ref bs, PacketPriority.MEDIUM_PRIORITY, PacketReliability.RELIABLE_ORDERED, SystemAddress);
                }
                VisualTimer = Ticks + 300;
            }
        }

        public void SyncWear()
        {
            UInt16 ucFlag = 0;

            if (SyncData.Armor) ucFlag |= 1;
            if (SyncData.Helmet) ucFlag |= 2;
            if (SyncData.MeleeWeapon) ucFlag |= 4;
            if (SyncData.RangedWeapon) ucFlag |= 8;
            if (SyncData.Shield) ucFlag |= 16;
            if (SyncData.Magic) ucFlag |= 32;
            if (SyncData.HandSync[0]) ucFlag |= 64;
            if (SyncData.HandSync[1]) ucFlag |= 128;

            if(ucFlag != 0)
            {
                var bs = new BitStream();

                bs.Write((byte)eNetworkMessage.PLAYER_WEAR);
                NetStream.WritePedId(ref bs, Id);
                bs.Write(ucFlag);

                if (SyncData.Armor) NetStream.WriteItem(ref bs, (uint)Armor);
                if (SyncData.Helmet) NetStream.WriteItem(ref bs, (uint)Helmet);
                if (SyncData.MeleeWeapon) NetStream.WriteItem(ref bs, (uint)Melee);
                if (SyncData.RangedWeapon) NetStream.WriteItem(ref bs, (uint)Ranged);
                if (SyncData.Shield) NetStream.WriteItem(ref bs, (uint)Shield);
                if (SyncData.Magic) NetStream.WriteItem(ref bs, (uint)Magic);
                if (SyncData.HandSync[0]) NetStream.WriteItem(ref bs, (uint)HandItem[0]);
                if (SyncData.HandSync[1]) NetStream.WriteItem(ref bs, (uint)HandItem[1]);



                SyncData.Armor = false;
                SyncData.Helmet = false;
                SyncData.MeleeWeapon = false;
                SyncData.RangedWeapon = false;
                SyncData.Shield = false;
                SyncData.Magic = false;
                SyncData.HandSync[0] = false;
                SyncData.HandSync[1] = false;

                ServerInstance.Network.SendToStreamed(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE_ORDERED, this);
            }
        }
        public void SyncInfo(Player player)
        {
            var bs = new BitStream();
           
            bs.Write((byte)eNetworkMessage.PLAYER_INFO);
            NetStream.WritePedId(ref bs, Id);
            NetStream.WriteVec(ref bs, Position);
            NetStream.WriteAngle(ref bs, Angle);
          
            ServerInstance.Network.Send(ref bs, PacketPriority.LOW_PRIORITY, PacketReliability.UNRELIABLE, player.SystemAddress);
        }
    }

}
