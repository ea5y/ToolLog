using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToolLog
{
    class CustomRichTextBox : System.Windows.Forms.RichTextBox
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool HideCaret(IntPtr hWnd);

        public bool bReadOnly = true;
        public bool isStopScroll = false;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (this.bReadOnly)
                ; //HideCaret(Handle);
        }
    }
}
