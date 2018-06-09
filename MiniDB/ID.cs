// must be disabled for unit testing
#define ENABLE_SYSTEM_UNIQUE_PART
using Newtonsoft.Json;
using System;


namespace MiniDB
{
    public class ID : IEquatable<ID>, IComparable<ID>
    {
        [JsonProperty()]
        public int id { get; private set; }
#if ENABLE_SYSTEM_UNIQUE_PART
        [JsonProperty()]
        public ulong hardwareComponent { get; private set; }
#endif

        public ID()
        {
#if ENABLE_SYSTEM_UNIQUE_PART
            hardwareComponent = 0;
#endif
            id = 0;
            Set();
        }

        public ID(int id, ulong hardwareComponent)
        {
#if ENABLE_SYSTEM_UNIQUE_PART
            this.hardwareComponent = hardwareComponent;
#endif
            this.Set();
            this.id = id;
        }

        public ID(ID other)
        {
            this.id = other.id;
#if ENABLE_SYSTEM_UNIQUE_PART
            this.hardwareComponent = other.hardwareComponent;
#endif
        }

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
                result = obj1.id == obj2.id 
                    &&
#if ENABLE_SYSTEM_UNIQUE_PART
                    obj1.hardwareComponent == obj2.hardwareComponent
#else
                    true
#endif
                    ;
            }

            return result;
        }

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
                result = obj1.id != obj2.id ||
#if ENABLE_SYSTEM_UNIQUE_PART
                    obj1.hardwareComponent != obj2.hardwareComponent
#else
                    false
#endif
                    ;
            }

            return result;
        }

        public override bool Equals(Object other)
        {
            bool result = false;
            ID check = other as ID;
            if (check != null)
            {
                result = this.id.Equals(check.id) &&
#if ENABLE_SYSTEM_UNIQUE_PART
                    this.hardwareComponent.Equals(check.hardwareComponent)
#else
                    true
#endif
                    ;
            }
            return result;
        }

        public bool Equals(ID other)
        {
            return this.id.Equals(other.id) &&
#if ENABLE_SYSTEM_UNIQUE_PART
                    this.hardwareComponent.Equals(other.hardwareComponent)
#else
                    true
#endif
                    ; // use default comparison
        }

        public ID Set(int input)
        {
#if ENABLE_SYSTEM_UNIQUE_PART
            this.hardwareComponent = DBHardwareID.IDValueInt();
#endif
            id = input;
            return this;
        }

        public ID Set()
        {
#if ENABLE_SYSTEM_UNIQUE_PART
            this.hardwareComponent = DBHardwareID.IDValueInt();
#endif
            id = random.Next(0, int.MaxValue);
            return this;
        }

        public int CompareTo(ID other)
        {
            // keep default comparison
            return this.id.CompareTo(other.id) +
#if ENABLE_SYSTEM_UNIQUE_PART
                    this.hardwareComponent.CompareTo(other.hardwareComponent)
#else
                    0
#endif
                    ;
        }

        public override string ToString()
        {
            if (IntPtr.Size == 4)
            {
                return
#if ENABLE_SYSTEM_UNIQUE_PART
                this.hardwareComponent.ToString() + ":" +
#endif
                    this.id.ToString();
            }
            else
                return this.id.ToString();
        }

        private static Random random = new Random();
    }
}
