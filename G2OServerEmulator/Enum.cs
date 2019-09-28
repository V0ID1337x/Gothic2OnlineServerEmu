using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G2OServerEmulator
{
    struct Defines
    {
        public static string DATA_ZIP = "data.bin";
    }
    struct Bits
    {
        public static uint ITEMS = 16;
        public static uint ITEMS_GROUND = 8;
        public static uint MDS = 8;
        public static uint INSTANCES = 10;
    }
    struct Limits
    {
        public static uint MAX_QUERY = 30;
        public static uint MAX_ITEMS_GROUND = 255;
        public static uint MAX_PLAYER_NAME = 16;
        public static uint MAX_ITEMS = (1 << 16) - 1; // Bits.ITEMS = 16
        public static uint MAX_MDS = 1 << 8; // Bits.MDS = 8
        public static uint MAX_INSTANCES = 1 << 10; // Bits.INSTANCES = 10
    }
    struct Version
    {
        public static int Major = 0;
        public static int Minor = 1;
        public static int Patch = 5;
    }
    enum ePacketType
    {
        PACKET_RAW,
        PACKET_CONNECT,
        PACKET_SERVER,
        PACKET_CHAT,
        PACKET_PLAYER,
        PACKET_ITEM,
        PACKET_PATCHER,
        PACKET_SCRIPT
    };

    enum eNetworkMessage
    {
        // Connection
        CONNECT_REQUEST = 134,
        CONNECT_ACCEPT,
        CONNECT_JOIN,
        CONNECT_KICK,
        CONNECT_BANNED,
        CONNECT_WRONG_VERSION,
        CONNECT_FULL,
        CONNECT_NICKNAME_USED,

        // Disconnect
        DISCONNECT_CRASH,

        // Server
        SERVER_TIME,

        // Chat
        CHAT_MESSAGE,
        CHAT_COMMAND,

        // Player
        PLAYER_CREATE,
        PLAYER_DESTROY,
        PLAYER_SPAWN,
        PLAYER_RESPAWN,
        PLAYER_UNSPAWN,
        PLAYER_RESET,
        PLAYER_SYNC,
        PLAYER_ANI,
        PLAYER_INFO,
        PLAYER_UPDATE,
        PLAYER_WEAR,
        PLAYER_VISUAL,
        PLAYER_HIT,
        PLAYER_WORLD_CHANGE,
        PLAYER_WORLD_ENTER,
        PLAYER_SKILL,
        PLAYER_OVERLAY,
        PLAYER_PING,
        PLAYER_MAGIC,
        PLAYER_MOB,

        // Item
        ITEM_SPAWN,
        ITEM_DESTROY,
        ITEM_CREATE,

        // Script
        SCRIPT_RPC,
    };

    enum ePacketRange
    {
        PLAYER_BEGIN = eNetworkMessage.PLAYER_CREATE,
        PLAYER_END = eNetworkMessage.PLAYER_MOB,
    };

    enum ePacketLimit
    {
        PLAYER_LIMIT = ePacketRange.PLAYER_END - ePacketRange.PLAYER_BEGIN + 1,
    };

    enum eScriptRPC
    {
        SCRIPT_PACKET,
        SCRIPT_SKILL,
        SCRIPT_STR,
        SCRIPT_DEX,
        SCRIPT_TALENT,
        SCRIPT_ARMOR,
        SCRIPT_MELEE_WEAPON,
        SCRIPT_RANGED_WEAPON,
        SCRIPT_HELMET,
        SCRIPT_SHIELD,
        SCRIPT_WORLD,
        SCRIPT_COLOR,
        SCRIPT_NAME,
        SCRIPT_EQUIP,
        SCRIPT_UNEQUIP,
        SCRIPT_GIVEITEM,
        SCRIPT_REMOVEITEM,
        SCRIPT_INSTANCE,
        SCRIPT_FATNESS,
        SCRIPT_SCALE,
        SCRIPT_VISUAL,
        SCRIPT_POSITION,
        SCRIPT_ROTATION,
        SCRIPT_FOCUS,
        SCRIPT_ANI,
        SCRIPT_ANI_ID,
        SCRIPT_ANI_STOP,
        SCRIPT_HP,
        SCRIPT_MAX_HP,
        SCRIPT_MANA,
        SCRIPT_MAX_MANA,
        SCRIPT_MAGIC_LVL,
        SCRIPT_WEAPONMODE,
        SCRIPT_OVERLAY,
    };

    enum ePatcherMessage
    {
        PATCHER_CONNECT,
        PATCHER_DISCONNECT,
        PATCHER_INIT,
        PATCHER_FILE_INIT,
        PATCHER_FILE_PROGRESS,
        PATCHER_FILE_END,
        PATCHER_FINISH
    };

    enum eAutoPatcher
    {
        AP_SCRIPTS,
        AP_DOWNLOAD
    };

    enum eAPUserId
    {
        APU_QUERY_CONNECT,
        APU_USER_INIT,
        APU_UNSAFE_MOD,
        APU_VDFS_CHECK
    };
}
