using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace HardwareID
{
    /// <summary>
    /// Get hardware specific (should be unique) ID based on system properties (RAM size, OS version, etc.)
    /// Values are cached from the system on first ask
    /// </summary>
    public sealed class HardwareID
    {
        #region properties
        /// <summary>
        /// Cache the fields, for quick recapture
        /// </summary>
        private static Dictionary<string, object> data = new Dictionary<string, object>();

        /// <summary>
        /// Keys to parse from SystemInfo
        /// </summary>
        private static Dictionary<string, string> systemInfoFields = new Dictionary<string, string>()
        {
            { "OS Name:",  nameof(OSName) },
            { "OS Manufacturer:", nameof(OSManufacturer) },
            { "Product ID:", nameof(ProductID) },
            { "System Model:", nameof(SystemModel) },
            { "System Type:", nameof(SystemType) },
            { "Total Physical Memory:", nameof(PhysicalMemory) },
        };
        #endregion

        #region properties
        /// <summary>
        /// Get the name of OS type
        /// </summary>
        private static string OSName => Get();

        /// <summary>
        /// Get the manufacturer of the OS
        /// </summary>
        private static string OSManufacturer => Get();

        /// <summary>
        /// Get System Product ID
        /// </summary>
        private static string ProductID => Get();

        /// <summary>
        /// Get the System Model
        /// </summary>
        private static string SystemModel => Get();

        /// <summary>
        /// Gets the System Type
        /// </summary>
        private static string SystemType => Get();

        /// <summary>
        /// Gets the size of the total memory on the system (RAM)
        /// </summary>
        private static string PhysicalMemory => Get();

        /// <summary>
        /// The system name (used on Unix)
        /// </summary>
        private static string UName => Get();

        /// <summary>
        /// Gets the base string used for creating IDs
        /// </summary>
        private static string _data
        {
            get
            {
                if (Environment.OSVersion.ToString().StartsWith("Microsoft"))
                {
                    // on Windows.
                    return $@"OSName >> {OSName}
OSManufacturer >> {OSManufacturer}
SystemModel >> {SystemModel}
PhysicalMemory >> {PhysicalMemory}";
                }
                else
                {
                    // on Unix.
                    return $@"Uname >> {UName}";
                }
            }
        }
        #endregion

        #region public methods
        /// <summary>
        /// Get the ID
        /// </summary>
        /// <returns>string hash of _data property</returns>
        public static string UniqueID()
        {
            return GetHash(_data);
        }

        /// <summary>
        /// Get the ID based on the hardware + provided seed
        /// </summary>
        /// <param name="seed">seed to add to system info for hash</param>
        /// <returns>string hash of _data + seed</returns>
        public static string UniqueID(string seed)
        {
            return GetHash($"{_data}\nSeed >> {seed}");
        }
        #endregion

        #region helper methods
        /// <summary>
        /// Run system process `systeminfo` to get data
        /// </summary>
        private static void GetSystemInfo()
        {
            if (Environment.OSVersion.ToString().StartsWith("Microsoft"))
            {
                // on Windows
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
            else
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "uname";
                p.StartInfo.Arguments = "-v";
                p.Start();
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                data[nameof(UName)] = output;
            }
        }

        /// <summary>
        ///  Get hash of string (translated to bytes)
        /// hashing code from http://forum.codecall.net/topic/78149-c-tutorial-generating-a-unique-hardware-id/
        /// </summary>
        /// <param name="s">the value to hash</param>
        /// <returns>Hexidecimal formatted hash string</returns>
        private static string GetHash(string s)
        {
            // Initialize a new MD5 Crypto Service Provider in order to generate a hash
            MD5 sec = new MD5CryptoServiceProvider();

            // Grab the bytes of the variable 's'
            byte[] bt = Encoding.ASCII.GetBytes(s);

            // Grab the Hexadecimal value of the MD5 hash
            return GetHexString(sec.ComputeHash(bt));
        }

        /// <summary>
        /// Convert byte list to hexadecimal string
        /// </summary>
        /// <param name="bt">Byte list</param>
        /// <returns>hexadecimal string representation of list</returns>
        private static string GetHexString(IList<byte> bt)
        {
            string s = string.Empty;
            for (int i = 0; i < bt.Count; i++)
            {
                byte b = bt[i];
                s += b.ToString("X2"); // format into hexidecimal 0XFF;
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
        /// <returns>the chached value</returns>
        private static dynamic Get([CallerMemberName]string name = null)
        { // TODO changing this to dynamic may make structs not work correctly unless initialized 
            if (data.ContainsKey(name))
            {
                return data[name];
            }
            else
            {
                GetSystemInfo();
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
        /// <returns>bool - successful or not</returns>
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
