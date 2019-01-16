using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinLZW
{
    public static class ExtensionMethods
    {
        public static void Raise(this EventHandler handler, object sender)
        {
            if (handler != null)
            {
                handler(sender, EventArgs.Empty);
            }
        }
    }
}
