using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    /// <summary>
    /// Public delegate for classes that want to register for the PropertyChangedExtended event.
    /// </summary>
    /// <param name="sender">The object that had a changed property</param>
    /// <param name="e">Information about the change</param>
    public delegate void PropertyChangedExtendedEventHandler(object sender, PropertyChangedExtendedEventArgs e);

    /// <summary>
    /// Notifies clients that a property value is changing, but includes extended event infomation
    /// The following NotifyPropertyChanged Interface is employed when you wish to enforce the inclusion of old and
    /// new values. (Users must provide PropertyChangedExtendedEventArgs, PropertyChangedEventArgs are disallowed.)
    /// </summary>
    public interface INotifyPropertyChangedExtended : INotifyPropertyChanged
    {
        /// <summary>
        /// An event for adding info on top of the default property changed
        /// </summary>
        event PropertyChangedExtendedEventHandler PropertyChangedExtended;
    }
}
