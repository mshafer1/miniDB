using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Management;

using HardwareID;
using System.Diagnostics;

namespace MiniDB
{
    public static class DBHardwareID
    {
        //static string fastID() => HardwareID.HardwareID.FastID();
        static string ID() => HardwareID.HardwareID.UniqueID();
        static string ID(string seed) => HardwareID.HardwareID.UniqueID(seed);

        public static UInt64 IDValueInt()
        {
            string id = ID();
            //Debug.WriteLine(id);
            byte[] bytes = Encoding.ASCII.GetBytes(id);
            ulong result = BitConverter.ToUInt64(bytes, 0);
            //Debug.WriteLine(result);
            return result;
        }

        public static byte[] IDValueBytes()
        {
            string id = ID();
            //Debug.WriteLine(id);
            byte[] bytes = Encoding.ASCII.GetBytes(id);
            return bytes;
        }

        public static UInt64 IDValueInt(string seed)
        {
            string id = ID(seed);
            //Debug.WriteLine(id);
            byte[] bytes = Encoding.ASCII.GetBytes(id);
            ulong result = BitConverter.ToUInt64(bytes, 0);
            //Debug.WriteLine(result);
            return result;
        }

        public static byte[] IDValueBytes(string seed)
        {
            string id = ID(seed);
            //Debug.WriteLine(id);
            byte[] bytes = Encoding.ASCII.GetBytes(id);
            return bytes;
        }
    }
}
