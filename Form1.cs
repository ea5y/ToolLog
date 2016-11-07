using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;


namespace ToolLog
{

    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        static extern bool HideCaret(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        extern static int GetScrollPos(IntPtr hWnd, int nBar);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

        [DllImport("user32.dll", EntryPoint = "LockWindowUpdate", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LockWindow(IntPtr Handle);
        public Form1()
        {
            InitializeComponent();
            /*using (Process process = new Process())
            {
                process.StartInfo.FileName = "adb";
                process.StartInfo.Arguments = "logcat";
                // 必须禁用操作系统外壳程序  
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                
                process.Start();
                
                string output = process.StandardOutput.ReadToEnd();

                if (String.IsNullOrEmpty(output) == false)
                    this.richTextBox1.AppendText(output + "\r\n");
                process.WaitForExit();
                process.Close();
            }*/
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            using (Process process = new System.Diagnostics.Process())
            {
                //process.StartInfo.FileName = "ping";
                //process.StartInfo.Arguments = "www.ymind.net -t";

                process.StartInfo.FileName = "adb";
                process.StartInfo.Arguments = "logcat";

                // 必须禁用操作系统外壳程序  
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;

                process.Start();

                // 异步获取命令行内容  
                process.BeginOutputReadLine();

                // 为异步获取订阅事件  
                process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
            }
        }

        private void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            // 这里仅做输出的示例，实际上您可以根据情况取消获取命令行的内容  
            // 参考：process.CancelOutputRead()  

            if (String.IsNullOrEmpty(e.Data) == false)
            {
                //this.AppendText(e.Data + "\r\n");

                
                switch (e.Data[0])
                {
                    case 'E':
                        this.LogError(e.Data + "\r\n");
                        break;
                    case 'W':
                        this.LogWarning(e.Data + "\r\n");
                        break;
                    case 'I':
                        this.LogMessage(e.Data + "\r\n");
                        break;
                }
                
            }
               
        }

        #region 解决多线程下控件访问的问题  

        public delegate void AppendTextCallback(string text);

        public void AppendText(string text)
        {
            if (this.richTextBox1.InvokeRequired)
            {
                AppendTextCallback d = new AppendTextCallback(AppendText);
                this.richTextBox1.Invoke(d, text);
            }
            else
            {
                this.richTextBox1.AppendText(text);
            }
        }

        #endregion

        #region 日志信息，支持其他线程访问
        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        /// <param name="text"></param>
        public delegate void LogAppendDelegate(Color color, string text);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        /// <param name="text"></param>
        public void LogAppend(Color color, string text)
        {
            if (this.richTextBox1.isStopScroll)
                        {
                            int pos = this.richTextBox1.SelectionStart;
                            this.richTextBox1.AppendText(text);
                            this.richTextBox1.SelectionStart = pos;
                        }else
                        {
                            this.richTextBox1.SelectionColor = color;
                            this.richTextBox1.AppendText(text);

                        }
            this.richTextBox1.SelectionColor = color;
            this.richTextBox1.AppendText(text);
            MessageBox.Show(this.richTextBox1.Cursor.ToString());
            //MessageBox.Show(this.richTextBox1.SelectionStart + "");
            //this.richTextBox1.SelectionColor = color;
            //this.richTextBox1.Text += text;
        }

        /// <summary>
        /// LogError
        /// </summary>
        /// <param name="text"></param>
        public void LogError(string text)
        {
            LogAppendDelegate lad = new LogAppendDelegate(LogAppend);
            richTextBox1.Invoke(lad, Color.Red, DateTime.Now.ToString("HH:mm:ss-") + text);
        }

        /// <summary>
        /// LogWarning
        /// </summary>
        /// <param name="text"></param>
        public void LogWarning(string text)
        {
            LogAppendDelegate lad = new LogAppendDelegate(LogAppend);
            richTextBox1.Invoke(lad, Color.Blue, DateTime.Now.ToString("HH:mm:ss-") + text);
        }

        /// <summary>
        /// LogMessage
        /// </summary>
        /// <param name="text"></param>
        public void LogMessage(string text)
        {
            LogAppendDelegate lad = new LogAppendDelegate(LogAppend);
            richTextBox1.Invoke(lad, Color.DarkGray, DateTime.Now.ToString("HH:mm:ss-") + text);
        }

        #endregion

        private void richTextBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //HideCaret(Handle);
                //this.button1.Focus();
                //var pos = GetScrollPos(this.richTextBox1.Handle, 1);
                //MessageBox.Show("Pos:" + pos);
                //SetScrollPos(this.richTextBox1.Handle, 1, pos, true);

                //LockWindow(this.richTextBox1.Handle);
                this.richTextBox1.isStopScroll = true;
            }
        }

        private void richTextBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Right)
            {
                //HideCaret(Handle);
                //this.button1.Focus();
                this.richTextBox1.isStopScroll = false;
            }
        }
    }
}
