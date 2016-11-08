using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;

namespace ToolLog
{
    enum ScrollState
    {
        Start,
        Stop
    }

    class CustomRichTextBox : System.Windows.Forms.RichTextBox
    {
        private ScrollState scrollState = ScrollState.Stop;
        public bool isStopScroll = false;
        public bool mouseDown;
        public int selCurrent;
        public int selOrigin;
        public int selStart;
        public int selEnd;
        public int selTrough;
        public int selPeak;
        private Color defaultBackColour;

        private int WM_SETFOCUS = 0x0007;
        private UInt32 EM_SETSEL = 0x00B1;


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        public CustomRichTextBox()
        {
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.rtb_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.rtb_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.rtb_MouseUp);
            defaultBackColour = this.BackColor;
        }

        protected override void WndProc(ref Message m)
        {
            //base.WndProc(ref m);
            // Let everything through except for the WM_SETFOCUS message
            if (m.Msg != WM_SETFOCUS)
                base.WndProc(ref m);

        }
        
        public void ScrollBox(object state)
        {
            if (!(bool)state)
            {
                this.Invoke((Action)delegate () {

                    // set the current caret position to the end
                    this.SelectionStart = this.Text.Length;
                    // scroll it automatically
                    this.ScrollToCaret();
                });
            }
        }

        public void rtb_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (this.scrollState == ScrollState.Stop)
                {
                    this.isStopScroll = true;
                    this.scrollState = ScrollState.Start;
                }
                else
                {
                    this.isStopScroll = false;
                    this.scrollState = ScrollState.Stop;
                }
            }
            mouseDown = true;
            // reset all the selection stuff
            selOrigin = selStart = selEnd = selPeak = selTrough = GetCharIndexFromPosition(e.Location);
            highlightSelection(1, Text.Length - 1, false);
        }

        public void rtb_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        public void rtb_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                selCurrent = GetCharIndexFromPosition(e.Location);

                // First determine the selection direction
                // Note the +1 when selecting backwards because GetCharIndexFromPosition
                // uses the left edge of the nearest character
                if (selCurrent < selOrigin + 1)
                {
                    // If the current selection is smaller than the previous selection,
                    // recolour the now unselected stuff back to the default colour
                    if (selCurrent > selTrough)
                    {
                        highlightSelection(selTrough, selCurrent, false);
                    }
                    selTrough = selCurrent;
                    selEnd = selOrigin + 1;
                    selStart = selCurrent;
                }
                else
                {
                    // If the current selection is smaller than the previous selection,
                    // recolour the now unselected stuff back to the default colour
                    if (selCurrent < selPeak)
                    {
                        highlightSelection(selPeak, selCurrent, false);
                    }
                    selPeak = selCurrent;
                    selStart = selOrigin;
                    selEnd = selCurrent;
                }

                highlightSelection(selStart, selEnd);
            }
        }

        private void highlightSelection(int start, int end, bool highlight = true)
        {
            selectText(start, end);
            SelectionBackColor = highlight ? Color.LightBlue : defaultBackColour;
        }

        private void selectText(int start, int end)
        {
            SendMessage(Handle, EM_SETSEL, start, end);
        }
    }

}
