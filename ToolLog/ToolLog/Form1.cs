using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToolLog
{
    public partial class Form1 : Form
    {
       

        public Form1()
        {
            InitializeComponent();
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            // 这里仅做输出的示例，实际上您可以根据情况取消获取命令行的内容  
            // 参考：process.CancelOutputRead()  

            if (String.IsNullOrEmpty(e.Data) == false)
            {
                
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
            
            int start = this.richTextBox1.SelectionStart;
            
            int len = this.richTextBox1.SelectionLength;
            
            int newStart = this.richTextBox1.TextLength;
            //Debug.Print("start:" + start);
            Debug.Print("newStart:" + newStart);

            this.richTextBox1.AppendText(text);

            int newEnd = this.richTextBox1.TextLength;
            Debug.Print("newEnd:" + newEnd);

            this.richTextBox1.Select(newStart, newEnd - newStart);
            this.richTextBox1.SelectionColor = color;            
            this.richTextBox1.Select(start, len);
            
            
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

     
        /// <summary>
        /// Timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!this.richTextBox1.isStopScroll)
            {
                // set the current caret position to the end
                this.richTextBox1.SelectionStart = this.richTextBox1.Text.Length;
                // scroll it automatically
                this.richTextBox1.ScrollToCaret();
            }
            
        }
    }
}
