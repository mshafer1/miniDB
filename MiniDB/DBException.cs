using System;
using System.Diagnostics.CodeAnalysis;

namespace MiniDB
{
    /// <summary>
    /// An exception class for when a general error occurs with a database
    /// </summary>
    public class DBException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DBException" /> class.
        /// </summary>
        public DBException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DBException" /> class.
        /// </summary>
        /// <param name="message">Why it can't create a database</param>
        public DBException(string message)
        : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DBException" /> class.
        /// </summary>
        /// <param name="message">Why it can't create a database</param>
        /// <param name="inner">The exception that is getting wrapped</param>
        public DBException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }

    /// <summary>
    /// An exception class for erros with creating a Database
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1402:FileMayOnlyContainASingleType", Justification = "Empty types")]
    public class DBCreationException : DBException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DBCreationException" /> class.
        /// </summary>
        public DBCreationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DBCreationException" /> class.
        /// </summary>
        /// <param name="message">Why it can't create a database</param>
        public DBCreationException(string message)
        : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DBCreationException" /> class.
        /// </summary>
        /// <param name="message">Why it can't create a database</param>
        /// <param name="inner">The exception that is getting wrapped</param>
        public DBCreationException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }

    /// <summary>
    /// An exception class for Undo errors from DataBase.
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1402:FileMayOnlyContainASingleType", Justification = "Empty types")]
    public class DBCannotUndoException : DBException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DBCannotUndoException" /> class.
        /// </summary>
        public DBCannotUndoException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DBCannotUndoException" /> class.
        /// </summary>
        /// <param name="message">Why it can't undo</param>
        public DBCannotUndoException(string message)
        : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DBCannotUndoException" /> class.
        /// </summary>
        /// <param name="message">Why it can't undo</param>
        /// <param name="inner">The exception that is getting wrapped</param>
        public DBCannotUndoException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }

    /// <summary>
    /// An exception class for errors with attempting to redo on the Database.
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1402:FileMayOnlyContainASingleType", Justification = "Empty types")]
    public class DBCannotRedoException : DBException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DBCannotRedoException" /> class.
        /// </summary>
        public DBCannotRedoException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DBCannotRedoException" /> class.
        /// </summary>
        /// <param name="message">Why it can't redo</param>
        public DBCannotRedoException(string message)
        : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DBCannotRedoException" /> class.
        /// </summary>
        /// <param name="message">Why it can't redo</param>
        /// <param name="inner">The exception that is getting wrapped</param>
        public DBCannotRedoException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}