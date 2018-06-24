using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace HardwareID
{
    public sealed class HardwareID
    {
        #region properties
        private static string OSName => Get();

        private static string OSManufacturer => Get();

        private static string ProductID => Get();

        private static string SystemModel => Get();

        private static string SystemType => Get();

        private static string PhysicalMemory => Get();

        private static string _data
        {
            get
            {
                return $@"OSName >> {OSName}
OSManufacturer >> {OSManufacturer}
SystemModel >> {SystemModel}
PhysicalMemory >> {PhysicalMemory}";
            }
        }
        #endregion

        #region properties
        private static Dictionary<string, object> data = new Dictionary<string, object>();

        private static Dictionary<string, string> systemInfoFields = new Dictionary<string, string>()
        {
            {"OS Name:",  nameof(OSName) },
            {"OS Manufacturer:", nameof(OSManufacturer) },
            {"Product ID:", nameof(ProductID) },
            {"System Model:", nameof(SystemModel) },
            {"System Type:", nameof(SystemType) },
            {"Total Physical Memory:", nameof(PhysicalMemory) },
        };
        #endregion

        #region constructors
        public static string UniqueID()
        {
            return GetHash(_data);
        }

        public static string UniqueID(string seed)
        {
            return GetHash($"{_data}\nSeed >> {seed}");
        }
        #endregion

        #region helper methods
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

            // split the output based on the system's new line charecter(s) - don't use any empty splits
            foreach (var line in output.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                var workingLine = line.Trim();
                var match = systemInfoFields.FirstOrDefault(x => workingLine.StartsWith(x.Key));
                if (!match.Equals(default(KeyValuePair<string, string>)))
                {
                    var rest = workingLine.Substring(match.Key.Length).Trim();
                    data[match.Value] = rest;
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
            // replace with s += b.ToString("X2"); ??
            //for (int i = 0; i < bt.Count; i++)
            //{
            //    byte b = bt[i];

            //    int n = b;
            //    int n1 = n & 15;
            //    int n2 = (n >> 4) & 15;
            //    if (n2 > 9)
            //        s += ((char)(n2 - 10 + 'A')).ToString(CultureInfo.InvariantCulture);
            //    else
            //        s += n2.ToString(CultureInfo.InvariantCulture);
            //    if (n1 > 9)
            //        s += ((char)(n1 - 10 + 'A')).ToString(CultureInfo.InvariantCulture);
            //    else
            //        s += n1.ToString(CultureInfo.InvariantCulture);
            //    if ((i + 1) != bt.Count && (i + 1) % 2 == 0) s += "-";
            //}
            for (int i = 0; i < bt.Count; i++)
            {
                byte b = bt[i];
                s += b.ToString("X2"); //format into hexidecima 0XFF;
                if ((i + 1) % 2 == 0 && (i + 1) < bt.Count)
                {
                    s += "-";
                }
            }
            return s;
        }


        /// <summary>
        /// Return the requested item by name from data if it is there, else null.
        /// </summary>
        /// <param name="name">The name of the item to fetch (default: caller)</param>
        /// <returns></returns>
        private static dynamic Get([CallerMemberName]string name = null)
        { // TODO changing this to dynamic may make structs not work correctly unless initialized 
            if (data.ContainsKey(name))
            {
                return data[name];
            }
            else
            {
                getSystemInfo();
                return data[name];
            }
        }

        /// <summary>
        /// Store the value in data and raise a PropertyChangedExtended event
        ///   if the new value is different, else return false.
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="value">The desired value to store in data</param>
        /// <param name="name">The name to store the value under (default: caller)</param>
        /// <returns></returns>
        private bool Set<T>(T value, [CallerMemberName]string name = null)
        {
            T oldVal;
            if (data.ContainsKey(name))
            {
                oldVal = (T)data[name];
                if (oldVal == null && value == null)
                {
                    return false;
                }
                if (oldVal != null && oldVal.Equals(value))
                {
                    return false; // NO-OP
                }
                data[name] = value;
            }
            else
            {
                oldVal = default(T);
                data.Add(name, value);
            }
            return true;
        }
        #endregion
    }
}
