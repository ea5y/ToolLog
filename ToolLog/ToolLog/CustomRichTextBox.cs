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
        private bool isFind = false;
        private ScrollState scrollState = ScrollState.Stop;
        public bool isStopScroll = false;

        public List<int> findBufferIndexList = new List<int>();
        private Dictionary<int, string> findBufferDic = new Dictionary<int, string>();
        public int currentFind = 0;

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
            if (!this.isFind)
            {
               
            }
            selOrigin = selStart = selEnd = selPeak = selTrough = GetCharIndexFromPosition(e.Location);
            highlightSelection(1, Text.Length - 1, false);
            //highlightSelection(1, Text.Length - 1, false);
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


        /// <summary>
        /// HeightLight selection
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="highlight"></param>
        private void highlightSelection(int start, int end, bool highlight = true)
        {
            selectText(start, end);
            SelectionBackColor = highlight ? Color.LightBlue : defaultBackColour;
        }

        /// <summary>
        /// HeightLight find
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="highlight"></param>
        private void highlightFind(int start, int end, bool highlight = true)
        {
            selectText(start, end);
            SelectionBackColor = highlight ? Color.Yellow : defaultBackColour;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void selectText(int start, int end)
        {
            SendMessage(Handle, EM_SETSEL, start, end);
        }

        /// <summary>
        /// Find
        /// </summary>
        /// <param name="text"></param>
        public void FindString(string text)
        {
            //clear buffer
            this.findBufferIndexList.Clear();
            this.findBufferDic.Clear();

            if (text.Length == 0)
            {
                //clear flag
                this.isFind = false;
                return;
            }

            int searchStart = 0;
            int indexOfText = 0;
            string test = this.Text;
            this.Invoke((Action)(delegate () {
                while (searchStart <= this.TextLength)
                {
                    indexOfText = test.IndexOf(text, searchStart);
                    if (indexOfText < 0)
                    {
                        if(this.findBufferIndexList.Count != 0)
                        {
                            this.isStopScroll = true;
                            this.FindMove(0);
                        }
                        
                        //MessageBox.Show("找不到该字符串！");
                        break;
                    }
                    //add to buffer
                    this.findBufferIndexList.Add(indexOfText);
                    this.findBufferDic.Add(indexOfText, text);

                    highlightSelection(indexOfText, indexOfText + text.Length);
                    searchStart = indexOfText + text.Length;
                }
                this.isFind = true;
            }));
        }

        public void FindMove(int i)
        {
            
            Debug.Print("sign:" + i);
            this.currentFind += i;

            if (this.currentFind == -1)
                this.currentFind = this.findBufferIndexList.Count - 1;
            this.SelectionStart = this.findBufferIndexList[this.currentFind];
            this.highlightFind(this.SelectionStart, this.SelectionStart + this.findBufferDic[this.SelectionStart].Length, true);
            //if(this.currentFind == 0)
                this.ScrollToCaret();
            

            var sign = Math.Sign(i);
            if (sign > 0)
            {
                if (this.currentFind - 1 >= 0)
                {
                    this.SelectionStart = this.findBufferIndexList[this.currentFind - 1];
                    this.highlightSelection(this.SelectionStart, this.SelectionStart + this.findBufferDic[this.SelectionStart].Length, true);
                }
            }else
            {
                //@TODO
                //if(this.currentFind + 1 == this.findBufferIndexList.Count) 
                this.SelectionStart = this.findBufferIndexList[this.currentFind + 1];
                this.highlightSelection(this.SelectionStart, this.SelectionStart + this.findBufferDic[this.SelectionStart].Length, true);
            }
        }


        /*private async void TaskFind(string text)
        {
            
            await Task.Run(
                () => {
                    int searchStart = 0;
                    int indexOfText = 0;
                    string test;
                    this.Invoke((Action)(delegate () {
                        while (searchStart < this.TextLength)
                        {
                            test = this.Text;
                            indexOfText = test.IndexOf(text, searchStart);
                            //indexOfText = this.Find(text, searchStart, RichTextBoxFinds.None);
                            if (indexOfText < 0)
                                break;
                            //Debug.Print("Find:" + indexOfText);
                            //Debug.Print("SelectionLength:" + this.SelectedText.Length);

                            //Debug.Print("SelectionLength:" + text.Length);
                            //highlightSelection(indexOfText, indexOfText + text.Length);
                            searchStart = indexOfText + text.Length;

                        }
                    }));


                });
        }*/
    }

}
