using System;
using System.Text;

namespace MiniDB
{
    /// <summary>
    /// System specific ID implementation for Database
    /// </summary>
    public static class DBHardwareID
    {
        /// <summary>
        /// Get string value of ID with seed
        /// </summary>
        /// <returns>string ID field (hexadecimal formatted)</returns>
        public static string ID() => HardwareID.HardwareID.UniqueID();

        /// <summary>
        /// Get string value of ID with seed
        /// </summary>
        /// <param name="seed">additional data to include in the hashing algorithm</param>
        /// <returns>string ID field (hexadecimal formatted)</returns>
        public static string ID(string seed) => HardwareID.HardwareID.UniqueID(seed);

        /// <summary>
        /// Get hardware ID as unigned 64-bit integer
        /// </summary>
        /// <returns>ID value</returns>
        public static ulong IDValueInt()
        {
            string id = ID();
            byte[] bytes = Encoding.ASCII.GetBytes(id);
            ulong result = BitConverter.ToUInt64(bytes, 0);
            return result;
        }

        /// <summary>
        /// Get hardware ID as byte array
        /// </summary>
        /// <returns>ID value as bytes</returns>
        public static byte[] IDValueBytes()
        {
            string id = ID();
            byte[] bytes = Encoding.ASCII.GetBytes(id);
            return bytes;
        }

        /// <summary>
        /// Get hardware as Unsigned 64 bit as byte array
        /// </summary>
        /// <param name="seed">Seed pass to ID method</param>
        /// <returns>ID value</returns>
        public static ulong IDValueInt(string seed)
        {
            string id = ID(seed);
            byte[] bytes = Encoding.ASCII.GetBytes(id);
            ulong result = BitConverter.ToUInt64(bytes, 0);
            return result;
        }

        /// <summary>
        /// Get hardware ID as byte array
        /// </summary>
        /// <param name="seed">Seed pass to ID method</param>
        /// <returns>ID value</returns>
        public static byte[] IDValueBytes(string seed)
        {
            string id = ID(seed);
            byte[] bytes = Encoding.ASCII.GetBytes(id);
            return bytes;
        }
    }
}