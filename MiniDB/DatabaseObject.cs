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
    public class DatabaseObject : IDatabaseItem
    {
        [JsonProperty()]
        public virtual ID ID { get; private set; }

        public DatabaseObject()
        {
            ID = new ID();
            ID.Set();
        }

        public void setID() { ID.Set(); } // randomly re-assign

        
    }
}
