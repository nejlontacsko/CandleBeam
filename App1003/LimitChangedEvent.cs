using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1003
{
    public delegate void LimitChangedHandler(object sender, LimitChangedEventArgs eventArgs);
    public class LimitChangedEventArgs : EventArgs
    {
        public BytePair Pair { get; private set; }

        public LimitChangedEventArgs(BytePair pair) => Pair = pair;
    }
}
