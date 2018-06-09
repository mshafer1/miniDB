using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;

namespace HardwareID
{
    public class HardwareID
    {
        public static string UniqueID()
        {
            string data = "Volume Info >> " + /*VolumeSerialNumber +*/
                "  OSName >> " + OSName +
                "  OSManufacturer >>" + OSManufacturer +
                //"  Product ID >> " + ProductID + -- apparently can change with a windows update
                "  SystemModel >> " + SystemModel +
                "  PhysicalMemory >> " + PhysicalMemory;
            //Debug.WriteLine(data);
            return GetHash(data);
        }

        public static string UniqueID(string seed)
        {
            string data = "Volume Info >> " + /*VolumeSerialNumber +*/
               "  OSName >> " + OSName +
               "  OSManufacturer >>" + OSManufacturer +
               //"  Product ID >> " + ProductID + -- apparently can change with a windows update
               "  SystemModel >> " + SystemModel +
               "  PhysicalMemory >> " + PhysicalMemory + 
               "  Seed >> " + seed;
            //Debug.WriteLine(data);
            return GetHash(data);
        }


        private static string OSName { get => Get(); }
        private static string OSManufacturer { get => Get(); }
        private static string ProductID{ get => Get(); }
        private static string SystemModel { get => Get(); }
        private static string SystemType { get => Get(); }
        private static string PhysicalMemory { get => Get(); }

        private static Dictionary<string, string> systemInfoFields = new Dictionary<string, string>()
        {
            {"OS Name:",  nameof(OSName) },
            { "OS Manufacturer:", nameof(OSManufacturer) },
            { "Product ID:", nameof(ProductID) },
            { "System Model:", nameof(SystemModel) },
            { "System Type:", nameof(SystemType) },
            { "Total Physical Memory:", nameof(PhysicalMemory) },
        };

        private static void getSystemInfo()
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "systeminfo";
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            foreach (var line in output.Split('\n'))
            {
                var workingLine = line.Trim();
                var match = systemInfoFields.FirstOrDefault(x => workingLine.StartsWith(x.Key));
                if (!match.Equals(default(KeyValuePair<string, string>)))
                {
                    var rest = workingLine.Substring(match.Key.Length).Trim();
                    fields[match.Value] = rest;
                }
            }
        }

        // hashing code from http://forum.codecall.net/topic/78149-c-tutorial-generating-a-unique-hardware-id/
        private static string GetHash(string s)
        {
            //Initialize a new MD5 Crypto Service Provider in order to generate a hash
            MD5 sec = new MD5CryptoServiceProvider();
            //Grab the bytes of the variable 's'
            byte[] bt = Encoding.ASCII.GetBytes(s);
            //Grab the Hexadecimal value of the MD5 hash
            return GetHexString(sec.ComputeHash(bt));
        }

        private static string GetHexString(IList<byte> bt)
        {
            string s = string.Empty;
            for (int i = 0; i < bt.Count; i++)
            {
                byte b = bt[i];
                int n = b;
                int n1 = n & 15;
                int n2 = (n >> 4) & 15;
                if (n2 > 9)
                    s += ((char)(n2 - 10 + 'A')).ToString(CultureInfo.InvariantCulture);
                else
                    s += n2.ToString(CultureInfo.InvariantCulture);
                if (n1 > 9)
                    s += ((char)(n1 - 10 + 'A')).ToString(CultureInfo.InvariantCulture);
                else
                    s += n1.ToString(CultureInfo.InvariantCulture);
                if ((i + 1) != bt.Count && (i + 1) % 2 == 0) s += "-";
            }
            return s;
        }


        /// <summary>
        /// Return the requested item by name from fields if it is there, else null.
        /// </summary>
        /// <param name="name">The name of the item to fetch (default: caller)</param>
        /// <returns></returns>
        protected static dynamic Get([CallerMemberName]string name = null)
        { // TODO changing this to dynamic may make structs not work correctly unless initialized 
            if(fields.ContainsKey(name))
            {
                return fields[name];
            }
            else
            {
                getSystemInfo();
                return fields[name];
            }
        }

        /// <summary>
        /// Store the value in fields and raise a PropertyChangedExtended event
        ///   if the new value is different, else return false.
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="value">The desired value to store in fields</param>
        /// <param name="name">The name to store the value under (default: caller)</param>
        /// <returns></returns>
        protected bool Set<T>(T value, [CallerMemberName]string name = null)
        {
            T oldVal;
            if (fields.ContainsKey(name))
            {
                oldVal = (T)fields[name];
                if (oldVal == null && value == null)
                {
                    return false;
                }
                if (oldVal != null && oldVal.Equals(value))
                {
                    return false; // NO-OP
                }
                fields[name] = value;
            }
            else
            {
                oldVal = default(T);
                fields.Add(name, value);
            }
            return true;
        }

        protected static Dictionary<string, object>  fields = new Dictionary<string, object>();
    }
}
