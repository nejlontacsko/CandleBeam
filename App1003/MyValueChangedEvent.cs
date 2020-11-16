using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1003
{
    public delegate void ValueChangedHandler(object sender, ValueChangedEventArgs eventArgs);

    public class ValueChangedEventArgs : EventArgs
    {
        public byte Value { get; private set; }

        public ValueChangedEventArgs(byte val) => Value = val;
    }
}
