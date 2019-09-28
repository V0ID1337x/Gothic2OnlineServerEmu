using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using RakNet;

namespace G2OServerEmulator
{
    /// <summary>
    /// Przykład użycia NetInt w C#
    /// </summary>
    public struct NetInt4
    {
        [BitInfo(4)]
        public int iValue;
    }
    public struct NetUInt2
    {
        [BitInfo(2)]
        public uint uValue;
    }
    public struct NetUInt3
    {
        [BitInfo(3)]
        public uint uValue;
    }
    public struct NetUInt7
    {
        [BitInfo(7)]
        public uint uValue;
    }
    public struct NetUInt4
    {
        [BitInfo(4)]
        public uint uValue;
    }

    public struct NetUInt8
    {
        [BitInfo(8)]
        public uint uValue;
    }
    public struct NetUInt12
    {
        [BitInfo(12)]
        public uint uValue;
    }
    public struct NetUInt16
    {
        [BitInfo(16)]
        public uint uValue;
    }

    public struct NetUInt20
    {
        [BitInfo(20)]
        public uint uValue;
    }

    public struct NetFloat24
    {
        [BitInfo(24)]
        public int iValue;
    }
    public struct NetFloat18
    {
        [BitInfo(18)]
        public int iValue;
    }
    public static class ByteSerializer
    {
        public static byte[] getBytes(object str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        public static T fromBytes<T>(byte[] arr)
        {
            T str = default(T);

            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (T)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);

            return str;
        }
    }
    /// <summary>
    /// To tylko rozszerzenie klasy BitStream
    /// </summary>
    public class NetStream : BitStream
    {
        public static void WritePedId(ref BitStream bitStream, in int pedId)
        {
            NetUInt8 uId = new NetUInt8();
            uId.uValue = (uint)pedId + 1;
            bitStream.WriteBits(ByteSerializer.getBytes(uId), 8);
        }
        public static bool ReadPedId(ref BitStream bitStream, out int pedId)
        {
            pedId = -1;
            var uID = new NetUInt8();
            var b = new byte[8];
            if (!bitStream.ReadBits(b, 8))
                return false;
            uID = ByteSerializer.fromBytes<NetUInt8>(b);
            pedId = (int)uID.uValue - 1;
            return true;
        }
        public static void WriteVec(ref BitStream bitStream, in Vector3 vec)
        {
            WriteVec(ref bitStream, vec.X, vec.Y, vec.Z);
        }
        /// <summary>
        /// Wpisanie vectora do streamu
        /// nie użyłem "in" przy parametrach ponieważ potrzebna jest ich kopia
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public static void WriteVec(ref BitStream bitStream, float x, float y, float z)
        {
            x *= 0.05f;
            y *= 0.05f;
            z *= 0.05f;

            if (x > 32767.0f) x = 32767.0f;
            else if (x < -32767.0f) x = -32767.0f;

            if (y > 32767.0f) y = 32767.0f;
            else if (y < -32767.0f) y = -32767.0f;

            if (z > 32767.0f) z = 32767.0f;
            else if (z < -32767.0f) z = -32767.0f;

            var fixedX = new NetFloat24();
            var fixedY = new NetFloat24();
            var fixedZ = new NetFloat24();

            fixedX.iValue = (int)(x * 256.0f);
            fixedY.iValue = (int)(y * 256.0f);
            fixedZ.iValue = (int)(z * 256.0f);

            bitStream.WriteBits(ByteSerializer.getBytes(fixedX), 24);
            bitStream.WriteBits(ByteSerializer.getBytes(fixedY), 24);
            bitStream.WriteBits(ByteSerializer.getBytes(fixedZ), 24);
        }
        public static bool ReadVec(ref BitStream bitStream, out Vector3 vec)
        {
            vec = new Vector3(0.0f);
            return ReadVec(ref bitStream, out vec.X, out vec.Y, out vec.Z);
        }
        public static bool ReadVec(ref BitStream bitStream, out float x, out float y, out float z)
        {
            var fixedX = new NetFloat24();
            var fixedY = new NetFloat24();
            var fixedZ = new NetFloat24();
            byte[] byteX = new byte[24];
            byte[] byteY = new byte[24];
            byte[] byteZ = new byte[24];
            x = 0; y = 0; z = 0;

            if (!bitStream.ReadBits(byteX, 24) ||
                !bitStream.ReadBits(byteY, 24) ||
                !bitStream.ReadBits(byteZ, 24))
                return false;

            fixedX = ByteSerializer.fromBytes<NetFloat24>(byteX);
            fixedY = ByteSerializer.fromBytes<NetFloat24>(byteY);
            fixedZ = ByteSerializer.fromBytes<NetFloat24>(byteZ);

            x = ((float)fixedX.iValue / 256.0f) * 20.0f;
            y = ((float)fixedY.iValue / 256.0f) * 20.0f;
            z = ((float)fixedZ.iValue / 256.0f) * 20.0f;

            return true;
        }
        public static void WriteScale(ref BitStream bitStream, in Vector3 vec)
        {
            WriteScale(ref bitStream, vec.X, vec.Y, vec.Z);
        }
        public static void WriteScale(ref BitStream bitStream, float x, float y, float z)
        {
            if (x > 63.0f) x = 63.0f;
            else if (x < 0.0f) x = 0.0f;

            if (y > 63.0f) y = 63.0f;
            else if (y < 0.0f) y = 0.0f;

            if (z > 63.0f) z = 63.0f;
            else if (z < 0.0f) z = 0.0f;

            var fixedX = new NetFloat18();
            var fixedY = new NetFloat18();
            var fixedZ = new NetFloat18();

            fixedX.iValue = (int)(x * 4096.0f);
            fixedY.iValue = (int)(y * 4096.0f);
            fixedZ.iValue = (int)(z * 4096.0f);

            bitStream.WriteBits(ByteSerializer.getBytes(fixedX), 18);
            bitStream.WriteBits(ByteSerializer.getBytes(fixedY), 18);
            bitStream.WriteBits(ByteSerializer.getBytes(fixedZ), 18);
        }

        public static void WriteFatness(ref BitStream bitStream, in float fatness)
        {
            bitStream.WriteFloat16(fatness, -20.0f, 20.0f);
        }

        public static void WriteAngle(ref BitStream bitStream, in float degree)
        {
            bitStream.WriteFloat16(degree, 0.0f, 360.0f);
        }

        public static bool ReadAngle(ref BitStream bitStream, out float degree)
        {
            return bitStream.ReadFloat16(out degree, 0.0f, 360.0f);
        }
        public static bool ReadFatness(ref BitStream bitStream, out float fatness)
        {
            return bitStream.ReadFloat16(out fatness, -20.0f, 20.0f);
        }

        public static void WriteMds(ref BitStream bitStream, in uint mds)
        {
            var val = new NetUInt8();
            val.uValue = mds;

            bitStream.WriteBits(ByteSerializer.getBytes(val), 8);
        }

        public static bool ReadMds(ref BitStream bitStream, out uint mds)
        {
            mds = 0;
            var b = new byte[8];
            if (!bitStream.ReadBits(b, 8)) return false;

            mds = ByteSerializer.fromBytes<uint>(b);
            return true;
        }

        public static void WriteItem(ref BitStream bitStream, in uint item)
        {
            var val = new NetUInt16();
            val.uValue = (uint)item + 1;

            bitStream.WriteBits(ByteSerializer.getBytes(val), 16);
        }
        public static void WriteAniId(ref BitStream bitStream, in int aniId)
        {
            var ani = new NetUInt12();
            ani.uValue = (uint)aniId + 1;
            bitStream.WriteBits(ByteSerializer.getBytes(ani), 12);
        }

        public static bool ReadItem(ref BitStream bitStream, out uint item)
        {
            item = 0;
            byte[] b = new byte[16];
            if (!bitStream.ReadBits(b, 16))
                return false;
            item = ByteSerializer.fromBytes<uint>(b) - 1;
            return true;
        }

        public static bool ReadHp(ref BitStream bitStream, out int hp)
        {
            hp = 1; 
            byte[] b = new byte[20];
            if (!bitStream.ReadBits(b, 20))
                return false;
            hp = ByteSerializer.fromBytes<int>(b);
            return true;
        }
        public static void WriteHp(ref BitStream bitStream, in int hp)
        {
            var val = new NetUInt20();
            val.uValue = (uint)hp;
            bitStream.WriteBits(ByteSerializer.getBytes(val), 20);
        }
        public static void WriteSkill(ref BitStream bitStream, in int skill, in int value)
        {
            var Skill = new NetUInt2();
            Skill.uValue = (uint)skill;
            var Value = new NetUInt7();
            Value.uValue = (uint)value;

            bitStream.WriteBits(ByteSerializer.getBytes(Skill), 2);
            bitStream.WriteBits(ByteSerializer.getBytes(Value), 7);
        }
        public static void WriteTalent(ref BitStream bitStream, in int talent, in bool toggle)
        {
            var val = new NetUInt3();
            val.uValue = (uint)talent;

            bitStream.WriteBits(ByteSerializer.getBytes(val), 3);
            bitStream.Write(toggle);
        }
        public static bool ReadWm(ref BitStream bitStream, out byte wM)
        {
            wM = 0;
            byte[] b = new byte[4];
            if (!bitStream.ReadBits(b, 4))
                return false;
            wM = ByteSerializer.fromBytes<byte>(b);
            return true;
        }

        public static void WriteWm(ref BitStream bitStream, in byte wM)
        {
            var wm = new NetUInt4();
            wm.uValue = wM;
            bitStream.WriteBits(ByteSerializer.getBytes(wm), 4);
        }

        public static bool ReadAniId(ref BitStream bitStream, out int aniId)
        {
            aniId = -1;
            byte[] b = new byte[12];
            if (!bitStream.ReadBits(b, 12))
                return false;
            aniId = ByteSerializer.fromBytes<int>(b);
            return true;
        }
    }
}
