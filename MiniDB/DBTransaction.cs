using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    public abstract class DBTransaction : IDatabaseObject
    {
        public ID ID => throw new NotImplementedException();

        public event PropertyChangedExtendedEventHandler PropertyChangedExtended;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
