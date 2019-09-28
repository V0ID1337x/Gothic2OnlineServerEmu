using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static G2OServerEmulator.Server;

namespace G2OServerEmulator
{
    /// <summary>
    /// Kolor nicku
    /// </summary>
    public struct Color
    {
        public byte r { get; set; }
        public byte g { get; set; }
        public byte b { get; set; }
        public byte a { get; set; }
    }
    /// <summary>
    /// Wygląd wizualny NPC
    /// </summary>
    public class Visual
    {
        public string BodyModel { get; set; }
        public string HeadModel { get; set; }
        public UInt16 BodyTexture { get; set; }
        public UInt16 HeadTexture { get; set; }

        public void MakeDefault()
        {
            BodyModel = "Hum_Body_Naked0";
            HeadModel = "Hum_Head_Pony";
            BodyTexture = 9;
            HeadTexture = 18;
        }

        public bool IsDefault()
        {
            return (BodyModel == "Hum_Body_Naked0" && HeadModel == "Hum_Head_Pony" && BodyTexture == 9 && HeadTexture == 18);
        }

        public override string ToString()
        {
            return $"Body model: {BodyModel} Body texture: {BodyTexture} Head model: {HeadModel} Head texutre: {HeadTexture}";
        }
    }
    /// <summary>
    /// W przyszłości może się wykorzysta
    /// </summary>
    public struct InventoryItem
    {
        public uint instance { get; set; }
        public uint amount { get; set; }
    }
    /// <summary>
    /// Procent skilli
    /// </summary>
    public enum SkillPercent
    {
        ONEH = 0,
        TWOH,
        BOW,
        CBOW
    }
    /// <summary>
    /// Ręka (przedmioty w ręce)
    /// </summary>
    public enum Hand
    {
        LEFT = 0,
        RIGHT
    }
    public class Npc : ObjectWorld
    {
        private string name;
        /// <summary>
        /// Zwraca wyjątek gdy nazwa jest dłuższa niż dozwolona
        /// </summary>
        public string Name { get { return name; } set {
                if (value.Length < Limits.MAX_PLAYER_NAME)
                    name = value;
                else throw new Exception($"Player name cannot be longer than:{Limits.MAX_PLAYER_NAME}");
            }
        }
        public string Instance { get; set; }

        public Color NameColor { get; set; }
        public Visual VisualApperance = new Visual();
        private Vector3 scale; //bo get set
        public Vector3 Scale { get { return scale; } set {
                if (value.X <= 63.0f &&
                    value.Y <= 63.0f &&
                    value.Z <= 63.0f)
                    scale = value;
            }
        }
        private float angle;
        public float Angle { get { return angle; } set {
                if (value >= 0.0f && value <= 360.0f)
                    angle = value;
            }
        }
        private float fatness;
        public float Fatness { get { return fatness; } set {
                if (value >= -20.0f && value <= 20.0f)
                    fatness = value;
            }
        }
        private int health;
        public int Health { get { return health; } set {
                health = value;
                if(value <= 0)
                {
                    health = 0;
                    IsDead = true;
                    IsUnconscious = false;
                }
            } }
        private int maxHealth;
        public int MaxHealth { get { return maxHealth; } set {
                if (value >= 0) maxHealth = value;
            }
        }
        private int mana;
        public int Mana { get { return mana; } set {
                if (value >= 0) mana = value;
            }
        }
        private int maxMana;
        public int MaxMana { get { return maxMana; } set {
                if (value >= 0) maxMana = value;
            }
        }
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public char MagicLevel { get; set; } // ?? WTF powinno być byte! (unsigned char zamiast signed char)

        public byte[] Skill = new byte[4];
        public bool[] Talent = new bool[7];

        public int Armor { get; set; }
        public int Helmet { get; set; }
        public int Melee { get; set; }
        public int Ranged { get; set; }
        public int Shield { get; set; }
        public int Magic { get; set; }
        public int[] HandItem = new int[2];

        public int Focus { get; set; }
        public int KillerId { get; set; }
        public int AniId { get; set; }
        public int WeaponMode { get; set; }
        public int ComboNr { get; set; }
        public byte BodyState { get; set; }

        public bool IsUnconscious { get; set; }
        public bool IsDead { get; set; }
        public bool IsSpawned { get; set; }
        public bool IsActive { get; set; }

        public string FaceAni { get; set; }

        public List<uint> Overlays = new List<uint>();

        public Npc(in int id, in string name)
        {
            Id = id;
            WorldName = ServerInstance.Config.Data.world_name;
            Name = name;
            NameColor = new Color() { r = 255, g = 255, b = 255, a = 255 };

            IsActive = true;
            IsSpawned = false;

            restart();
        }

        public void restart()
        {
            VisualApperance.MakeDefault();

            Instance = "PC_HERO"; FaceAni = "S_NEUTRAL";
            Position = new Vector3(0.0f);
            Scale = new Vector3(1.0f);

            Angle = 0.0f;
            Fatness = 1.0f;
            Health = 40; MaxHealth = 40;
            Mana = 10; MaxMana = 10;
            Strength = 10; Dexterity = 10;
            MagicLevel = (char)0;
            BodyState = 0;

            Skill[(int)SkillPercent.ONEH] = 10;
            Skill[(int)SkillPercent.TWOH] = 10;
            Skill[(int)SkillPercent.BOW] = 10;
            Skill[(int)SkillPercent.CBOW] = 10;

            for (int i = 0; i < 6; ++i)
                Talent[i] = false;

            Armor = -1;
            Helmet = -1;
            Shield = -1;
            Melee = -1;
            Ranged = -1;
            Magic = -1;

            HandItem[(int)Hand.LEFT] = -1;
            HandItem[(int)Hand.RIGHT] = -1;

            Focus = -1;
            KillerId = -1;
            AniId = -1;
            ComboNr = -1;
            WeaponMode = -1;

            IsUnconscious = false;
            IsDead = false;

            Overlays.Clear();
        }

        public bool ApplyOverlay(in uint Mds)
        {
            // Tu może exception wyskoczyć aka out of range
            if (ServerInstance.MdsManager.mds[(int)Mds] == null)
                return false;
            if (Overlays.Contains(Mds))
                return false;
            Overlays.Add(Mds);
            return true;
        }

        public bool RemoveOverlay(in uint Mds)
        {
            return Overlays.Remove(Mds);
        }
    }
}
