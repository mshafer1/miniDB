using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MiniDB
{
    public class DatabaseObject : DatabaseItem
    {
        [JsonProperty()]
        public ID ID { get; private set; } // using private set to prevent children classes from creating a new ID, but allowing Newtonsoft.json to tweak it.

        public DatabaseObject()
        {
            ID = new ID();
            ID.Set();
        }

        public void setID() { ID.Set(); } // randomly re-assign
    }
}
