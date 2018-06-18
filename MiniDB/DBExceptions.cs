using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    public class DBException : Exception
    {
        public DBException()
        {
        }

        public DBException(string message)
        : base(message)
        {
        }

        public DBException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }

    public class DBCreationException : DBException
    {
        public DBCreationException()
        {
        }

        public DBCreationException(string message)
        : base(message)
        {
        }

        public DBCreationException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }

    public class DBCannotUndoException : DBException
    {
        public DBCannotUndoException()
        {
        }

        public DBCannotUndoException(string message)
        : base(message)
        {
        }

        public DBCannotUndoException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }

    public class DBCannotRedoException : DBException
    {
        public DBCannotRedoException()
        {
        }

        public DBCannotRedoException(string message)
        : base(message)
        {
        }

        public DBCannotRedoException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}
