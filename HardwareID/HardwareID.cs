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
    public static class HardwareID
    {
        #region constants

        /// <summary>
        /// Keys to parse from SystemInfo
        /// </summary>
        private static readonly IReadOnlyDictionary<string, string> SystemInfoFields = new Dictionary<string, string>()
        {
            { "OS Name:",  nameof(OSName) },
            { "OS Manufacturer:", nameof(OSManufacturer) },
            { "Product ID:", nameof(ProductID) },
            { "System Model:", nameof(SystemModel) },
            { "System Type:", nameof(SystemType) },
            { "Total Physical Memory:", nameof(PhysicalMemory) },
        };
        #endregion

        #region fields

        /// <summary>
        /// Cache the fields, for quick recapture
        /// </summary>
        private static readonly Dictionary<string, string> Keys = new Dictionary<string, string>();
        #endregion

        #region properties

        /// <summary>
        /// Gets the name of OS type
        /// </summary>
        private static string OSName => Get();

        /// <summary>
        /// Gets the manufacturer of the OS
        /// </summary>
        private static string OSManufacturer => Get();

        /// <summary>
        /// Gets System Product ID
        /// </summary>
        private static string ProductID => Get();

        /// <summary>
        /// Gets the System Model
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
        /// Gets the system name (used on Unix)
        /// </summary>
        private static string UName => Get();

        /// <summary>
        /// Gets the base string used for creating IDs
        /// </summary>
        private static string DataString
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
            return GetHash(DataString);
        }

        /// <summary>
        /// Get the ID based on the hardware + provided seed
        /// </summary>
        /// <param name="seed">seed to add to system info for hash</param>
        /// <returns>string hash of _data + seed</returns>
        public static string UniqueID(string seed)
        {
            return GetHash($"{DataString}\nSeed >> {seed}");
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
                    var match = SystemInfoFields.FirstOrDefault(x => workingLine.StartsWith(x.Key));
                    if (!match.Equals(default(KeyValuePair<string, string>)))
                    {
                        var rest = workingLine.Substring(match.Key.Length).Trim();
                        Keys[match.Value] = rest;
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
                Keys[nameof(UName)] = output;
            }
        }

        /// <summary>
        ///  Get hash of string (translated to bytes)
        /// hashing code from http://forum.codecall.net/topic/78149-c-tutorial-generating-a-unique-hardware-id/
        /// Not using the builtin s.GetHashCode as its implementation is different in different versions of CLR,
        ///   so we are using MD5 to keep consistency across platforms/compiles.
        /// </summary>
        /// <param name="s">the value to hash</param>
        /// <returns>Hexadecimal formatted hash string</returns>
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
            var s = new StringBuilder();
            for (int i = 0; i < bt.Count; i++)
            {
                byte b = bt[i];
                s.Append(b.ToString("X2")); // format into hexidecimal 0xFF;
                if ((i + 1) % 2 == 0 && (i + 1) < bt.Count)
                {
                    s.Append("-");
                }
            }

            return s.ToString();
        }

        /// <summary>
        /// Return the requested item by name from data if it is there, else null.
        /// </summary>
        /// <param name="name">The name of the item to fetch (default: caller)</param>
        /// <returns>the cached value</returns>
        private static string Get([CallerMemberName]string name = null)
        {
            if (Keys.ContainsKey(name))
            {
                return Keys[name];
            }
            else
            {
                GetSystemInfo();
                return Keys[name];
            }
        }

        #endregion
    }
}
