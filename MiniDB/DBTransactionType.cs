using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    /// <summary>
    /// Tracking what part of CRUD this is.
    /// 
    ///  C - create
    ///  R - read (not needed)
    ///  U - update
    ///  D - delete
    ///  
    /// TODO: add Resolve - used to mark sync point
    /// </summary>
    public enum DBTransactionType
    {
        Unknown,
        Add, // create
        Modify, // update
        Delete, // delete
    }
}
