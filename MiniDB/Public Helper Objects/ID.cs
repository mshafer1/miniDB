using System;
using System.Collections.Generic;
using System.Diagnostics;

using Newtonsoft.Json;

namespace MiniDB
{
    /// <summary>
    /// A class to create and maintain (update) system/globally unique identifiers.
    /// </summary>
    public class ID : IEquatable<ID>, IComparable<ID>
    {
        #region fields
        /// <summary>
        /// A radom number generator to create the next ID number from
        /// </summary>
        private static Random random = new Random();

        /// <summary>
        /// Gets or sets the numeric part of the ID
        /// </summary>
        [JsonProperty]
        private int id;

        /// <summary>
        /// Gets or sets a hardware specific component to make collisions less likely when merging databases.
        /// </summary>
        [JsonProperty]
        private ulong hardwareComponent;
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ID" /> class.
        /// Fills the properties with default values then call Set on self
        /// </summary>
        public ID()
        {
            this.hardwareComponent = 0;
            this.id = 0;
            this.Set();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ID" /> class.
        /// </summary>
        /// <param name="id">The requested id</param>
        /// <param name="hardwareComponent">The requested hardware component</param>
        public ID(int id, ulong hardwareComponent)
        {
            this.hardwareComponent = hardwareComponent;
            this.Set();
            this.id = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ID" /> class.
        /// Attempt to parse ID from jToken
        /// </summary>
        /// <param name="jsonToken">The NewtonSoft object that contains a serialized ID to parse</param>
        public ID(Newtonsoft.Json.Linq.JToken jsonToken)
        {
            this.Set();
            try
            {
                jsonToken = jsonToken["ID"];
            }
            catch (KeyNotFoundException)
            {
                // NO-OP
                // must already be pointing at the ID
            }

            this.id = System.Convert.ToInt32(jsonToken["id"].ToString());
            this.hardwareComponent = Convert.ToUInt64(jsonToken["hardwareComponent"].ToString());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ID"/> class. Copying the passed in one
        /// </summary>
        /// <param name="other">the id to copy</param>
        public ID(ID other)
        {
            this.id = other.id;
            this.hardwareComponent = other.hardwareComponent;
        }
        #endregion

        #region properties

        #endregion

        /// <summary>
        /// Check if both hardware and system components are equal
        /// </summary>
        /// <param name="obj1">The first ID to compare</param>
        /// <param name="obj2">The second ID to compare to the first one</param>
        /// <returns>True if both components are equal</returns>
        public static bool operator ==(ID obj1, ID obj2)
        {
            bool result = false;
            if (ReferenceEquals(obj1, obj2))
            {
                result = true;
            }
            else if (ReferenceEquals(obj1, null))
            {
                result = false;
            }
            else if (ReferenceEquals(obj2, null))
            {
                result = false;
            }
            else
            {
                result = obj1.id == obj2.id && obj1.hardwareComponent == obj2.hardwareComponent;
            }

            return result;
        }

        /// <summary>
        /// Check if either hardware component or id component is not equal
        /// </summary>
        /// <param name="obj1">The first ID to compare</param>
        /// <param name="obj2">the second ID to compare the first against.</param>
        /// <returns>Return true if one or more of the components are different</returns>
        public static bool operator !=(ID obj1, ID obj2)
        {
            bool result = false;
            if (ReferenceEquals(obj1, obj2))
            {
                result = false;
            }
            else if (ReferenceEquals(obj1, null))
            {
                result = true;
            }
            else if (ReferenceEquals(obj2, null))
            {
                result = true;
            }
            else
            {
                result = obj1.id != obj2.id || obj1.hardwareComponent != obj2.hardwareComponent;
            }

            return result;
        }

        /// <summary>
        /// If other object is an ID, compare to it; else, false.
        /// </summary>
        /// <param name="other">The other object to compare to</param>
        /// <returns>True if both components are equal</returns>
        public override bool Equals(object other)
        {
            bool result = false;
            ID check = other as ID;
            if (check != null)
            {
                result = this.id.Equals(check.id) && this.hardwareComponent.Equals(check.hardwareComponent);
            }

            return result;
        }

        /// <summary>
        /// Determine if this ID is equal (completely by value) to the other id.
        /// </summary>
        /// <param name="other">The other id to compare to</param>
        /// <returns>0 if both components are equal, else comparison of the hardware components if they are different, else comparison of the system components</returns>
        public bool Equals(ID other)
        {
            // use default comparison
            return this.id.Equals(other.id) && this.hardwareComponent.Equals(other.hardwareComponent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Set the ID
        ///     hardware component is determined based on the system,
        ///     id field is determined from the input
        /// </summary>
        /// <param name="input">the integer to use for the system component</param>
        /// <returns>The new object</returns>
        public ID Set(int input)
        {
            this.hardwareComponent = DBHardwareID.IDValueInt();
            this.id = input;
            return this;
        }

        /// <summary>
        /// Set the ID
        ///    hardware component is determined
        ///    id fields is set to next random (to lower chances of collisions)
        /// </summary>
        /// <returns>The new object</returns>
        public ID Set()
        {
            this.hardwareComponent = DBHardwareID.IDValueInt();
            this.id = random.Next(0, int.MaxValue);
            return this;
        }

        /// <summary>
        /// Use system comparison for both parts of the ID - both the hardware and system parts of the ID must match to return 0
        /// </summary>
        /// <param name="other">The ID to compare to</param>
        /// <returns>0 if the same, else a numeric representation of the comparison</returns>
        public int CompareTo(ID other)
        {
            if (this.Equals(other))
            {
                return 0;
            }

            int hardwarePart = this.hardwareComponent.CompareTo(other.hardwareComponent);
            if (hardwarePart != 0)
            {
                return hardwarePart;
            }

            int systemPart = this.id.CompareTo(other.id);
            if (systemPart != 0)
            {
                return systemPart;
            }

            Debug.Fail("Id's are apparently not equal, but both hardware and system parts are . . .");
            return 0;
        }

        /// <summary>
        /// Portray this ID as a string (HardwareComponent:idField)
        /// </summary>
        /// <returns>string formated as: {HardwareComponent:idField}</returns>
        public override string ToString() => $"{this.hardwareComponent}:{this.id}";
    }
}