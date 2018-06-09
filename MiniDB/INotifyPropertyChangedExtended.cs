using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    // from https://stackoverflow.com/a/7742890
    //
    // Summary: Notifies clients that a property value is changing, but includes extended event infomation
    /* The following NotifyPropertyChanged Interface is employed when you wish to enforce the inclusion of old and
     * new values. (Users must provide PropertyChangedExtendedEventArgs, PropertyChangedEventArgs are disallowed.) */
    public interface INotifyPropertyChangedExtended: INotifyPropertyChanged
    {
        event PropertyChangedExtendedEventHandler PropertyChangedExtended;
    }

    public delegate void PropertyChangedExtendedEventHandler(object sender, PropertyChangedExtendedEventArgs e);
}
