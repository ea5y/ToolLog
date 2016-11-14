using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;

namespace Custom
{
    enum ScrollState
    {
        Start,
        Stop
    }

    class CustomRichTextBox : System.Windows.Forms.RichTextBox
    {
        #region property
        private bool isFind = false;
        public bool IsFind { get { return this.isFind; } set { this.isFind = value; } }

        private ScrollState scrollState = ScrollState.Start;

        private bool isStopScroll = false;
        public bool IsStopScroll { get { return this.isStopScroll; } private set { this.isStopScroll = value; } }

        private bool isForceStopScroll = false;
        public bool IsForceStopScroll { get { return this.isForceStopScroll; } private set { this.isForceStopScroll = value; } }

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
        
        private int TopLine
        {
            get
            {
                int topIndex = this.GetCharIndexFromPosition(new System.Drawing.Point(1, 1));
                int topLine = this.GetLineFromCharIndex(topIndex);
                return topLine;
            }
        }

        public int BottomLine
        {
            get
            {
                int bottomIndex = this.GetCharIndexFromPosition(new System.Drawing.Point(1, this.Height - 1));
                int bottomLine = this.GetLineFromCharIndex(bottomIndex);
                return bottomLine;
            }
        }

        private int MiddleLine
        {
            get
            {
                return (this.BottomLine - this.TopLine) / 2 + this.TopLine;
            }
        }

        public int CurrentLine
        {
            get
            {
                return this.GetLineFromCharIndex(this.SelectionStart);
            }
        }
        
        //when i'm paging, scroll to this pos
        private int PageMiddleIndex
        {
            get
            {
                return this.GetFirstCharIndexFromLine(this.CurrentLine - (this.MiddleLine - this.CurrentLine));
            }
        }
        #endregion

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
        
        #region mouse event
        /// <summary>
        /// mouse down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void rtb_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (this.scrollState == ScrollState.Start)
                {
                    this.ForceStopScroll(true);
                    this.scrollState = ScrollState.Stop;
                }
                else
                {
                    this.ForceStopScroll(false);
                    this.scrollState = ScrollState.Start;
                }
            }
            mouseDown = true;

            // reset all the selection stuff
            selOrigin = selStart = selEnd = selPeak = selTrough = GetCharIndexFromPosition(e.Location);
            if (!this.IsFind)
            {
                highlightSelection(1, Text.Length - 1, false);
            }
        }

        /// <summary>
        /// mouse up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void rtb_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        /// <summary>
        /// mouse move
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        #endregion

        #region high light
        /// <summary>
        /// High Light selection
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
        /// High Light find
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="highlight"></param>
        private void highlightFind(int start, int end, bool highlight = true)
        {
            //this.ClearAllBackColor();
            selectText(start, end);
            SelectionBackColor = highlight ? Color.Yellow : defaultBackColour;
        }
        #endregion

        #region custom select
        /// <summary>
        /// custom select
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void selectText(int start, int end)
        {
            SendMessage(Handle, EM_SETSEL, start, end);
        }
        #endregion

        #region find
        /// <summary>
        /// Find
        /// </summary>
        /// <param name="text"></param>
        public void FindString(string text)
        {
            //clear buffer
            this.ClearAllBackColor();
            this.findBufferIndexList.Clear();
            this.findBufferDic.Clear();
            this.currentFind = 0;

            //clear found
            if (text.Length == 0)
            {
                //clear flag
                this.IsFind = false;
                return;
            }

            //find
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
                            if(this.scrollState == ScrollState.Start)
                            {
                                this.scrollState = ScrollState.Stop;
                                this.ForceStopScroll(true);
                            }
                            
                            //
                            this.FindMove(0, null);
                        }
                        if(this.findBufferIndexList.Count == 0)
                        {
                            MessageBox.Show("找不到该字符串！");
                        }

                        break;
                    }

                    //add to buffer
                    this.findBufferIndexList.Add(indexOfText);
                    this.findBufferDic.Add(indexOfText, text);
                    //high light
                    highlightSelection(indexOfText, indexOfText + text.Length);
                    //next start
                    searchStart = indexOfText + text.Length;
                }
                this.IsFind = true;
            }));
        }

        /// <summary>
        /// Find move
        /// </summary>
        /// <param name="i"></param>
        public void FindMove(int i, Label tips)
        {
            //hide tips
            if (tips != null)
                tips.Visible = false;

            Debug.Print("sign:" + i);
            this.currentFind += i;

            //loop
            if (this.currentFind == -1)
            {
                this.currentFind = this.findBufferIndexList.Count - 1; 
                if(tips != null)
                {
                    tips.Text = "Search hit Top, continuing at Bottom!";
                    tips.Visible = true;
                }
            }
            if (this.currentFind == this.findBufferIndexList.Count)
            {
                this.currentFind = 0;
                if (tips != null)
                {
                    tips.Text = "Search hit Bottom, continuing at Top!";
                    tips.Visible = true;
                }
            }
                
            //set found high light
            this.SelectionStart = this.findBufferIndexList[this.currentFind];
            this.highlightFind(this.SelectionStart, this.SelectionStart + this.findBufferDic[this.SelectionStart].Length, true);

            //first fixed scroll pos
            if (this.currentFind == 0)
                this.ScrollToCaret();

            //page down fixed scroll pos
            if (this.CurrentLine >= this.BottomLine)
            {
                this.ScrollToCaret();
                if(this.CurrentLine == this.TopLine)
                {
                    this.SelectionStart = this.PageMiddleIndex;
                    this.ScrollToCaret();
                }
            }

            //page up fixed scroll pos
            if(this.CurrentLine < this.TopLine)
            {
                this.ScrollToCaret();
                if (this.CurrentLine == this.TopLine)
                {
                    this.SelectionStart = this.PageMiddleIndex;
                    this.ScrollToCaret();
                }
            }
                
            //restore high light
            var sign = Math.Sign(i);
            if (sign > 0)
            {
                if (this.currentFind - 1 >= 0)
                {
                    this.SelectionStart = this.findBufferIndexList[this.currentFind - 1];
                    this.highlightSelection(this.SelectionStart, this.SelectionStart + this.findBufferDic[this.SelectionStart].Length, true);
                }else
                {
                    this.SelectionStart = this.findBufferIndexList[this.findBufferIndexList.Count - 1];
                    this.highlightSelection(this.SelectionStart, this.SelectionStart + this.findBufferDic[this.SelectionStart].Length, true);
                }
            }
            if (sign < 0)
            {
                if(this.currentFind + 1 < this.findBufferIndexList.Count)
                {
                    this.SelectionStart = this.findBufferIndexList[this.currentFind + 1];
                    this.highlightSelection(this.SelectionStart, this.SelectionStart + this.findBufferDic[this.SelectionStart].Length, true);
                }else
                {
                    this.SelectionStart = this.findBufferIndexList[0];
                    this.highlightSelection(this.SelectionStart, this.SelectionStart + this.findBufferDic[this.SelectionStart].Length, true);
                }
            }
        }
        #endregion

        public void ClearAllBackColor()
        {
            this.IsFind = false;
            highlightSelection(1, Text.Length - 1, false);
        }

        public void ScrollToBottom(bool to)
        {
            if (to)
            {
                // set the current caret position to the end
                this.SelectionStart = this.TextLength; 
                this.ScrollToCaret();
            }
        }

        public void StopScroll()
        {
            this.IsStopScroll = true;
        }

        public void StartScroll()
        {
            this.IsStopScroll = false;
        }

        public void ForceStopScroll(bool force)
        {
            this.IsForceStopScroll = force;
        }
    }

}
