using Microsoft.Win32;
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
        string Command = "";
        Process process;

        public Form1()
        {
            InitializeComponent();
            this.ToolLogRegeditLoad();
        }

        private void ToolLogRegeditLoad()
        {
            try
            {
                RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE_STORAGE\\GAME_TOOLS\\TOOL_LOG", true);
                if (reg != null)
                {
                    this.FilePathBox.Text = reg.GetValue("FilePath") as string;
                    this.CommandBox.Text = reg.GetValue("Command") as string;
                }
                reg.Close();
            }
            catch
            {
                //MessageBox.Show(ex.Message);
            }
        }

        
        private void WriteToRegedit()
        {
            try
            {
                RegistryKey rsg = null;
                if (Registry.CurrentUser.OpenSubKey("SOFTWARE_STORAGE\\GAME_TOOLS\\TOOL_LOG") == null)
                {
                    Registry.CurrentUser.CreateSubKey("SOFTWARE_STORAGE\\GAME_TOOLS\\TOOL_LOG");//创建
                }

                rsg = Registry.CurrentUser.OpenSubKey("SOFTWARE_STORAGE\\GAME_TOOLS\\TOOL_LOG", true);

                rsg.SetValue("FilePath", this.FilePathBox.Text);
                rsg.SetValue("Command", this.Command);
                rsg.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartBtn_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Focus();

            this.CreateProcess(ref this.process);

            this.WriteToRegedit();
        }

        /// <summary>
        /// create process
        /// async model
        /// </summary>
        /// <param name="process"></param>
        private void CreateProcess(ref Process process)
        {
            process = new System.Diagnostics.Process();
            string str = this.CommandBox.Text;
            var strArray = str.Split(' ');

            string arguments = "";
            for (int i = 1; i < strArray.Length; i++)
            {
                arguments += strArray[i] + ' ';
            }

            //process.StartInfo.FileName = this.FilePathBox.Text + "\\" + strArray[0];
            process.StartInfo.FileName = strArray[0];
            process.StartInfo.Arguments = arguments;

            this.Command = strArray[0] + " " + arguments;
            // 必须禁用操作系统外壳程序  
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            //Catch exception
            try
            {
                process.Start();
            }
            catch(Win32Exception e)
            {
                MessageBox.Show(e.Message); return;
            }

            this.timer1.Enabled = true;

            // 异步获取命令行内容  
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // 为异步获取订阅事件  
            process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
            process.ErrorDataReceived += new DataReceivedEventHandler(process_ErrorDataReceived);
           
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.LogException(e.Data + "\r\n");
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
            if (!this.process.HasExited)
            {
                int start = this.richTextBox1.SelectionStart;

                int len = this.richTextBox1.SelectionLength;

                int newStart = this.richTextBox1.TextLength;
                //Debug.Print("start:" + start);
                //Debug.Print("newStart:" + newStart);

                this.richTextBox1.AppendText(text);

                int newEnd = this.richTextBox1.TextLength;
                //Debug.Print("newEnd:" + newEnd);

                this.richTextBox1.Select(newStart, newEnd - newStart);
                this.richTextBox1.SelectionColor = color;
                //this.richTextBox1.Select(start, len);
            }

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

        /// <summary>
        /// Exception
        /// </summary>
        /// <param name="text"></param>
        public void LogException(string text)
        {
            LogAppendDelegate lad = new LogAppendDelegate(LogAppend);
            richTextBox1.Invoke(lad, Color.Black, text);
        }

        #endregion

     
        /// <summary>
        /// Timer auto scroll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.process != null && !this.process.HasExited && !this.richTextBox1.isStopScroll)
            {
                // set the current caret position to the end
                this.richTextBox1.SelectionStart = this.richTextBox1.Text.Length;
                // scroll it automatically
                this.richTextBox1.ScrollToCaret();
            }
        }

        private void Browser_Click(object sender, EventArgs e)
        {
            //set root folder
            folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Desktop;
            //set select path
            folderBrowserDialog1.SelectedPath = "C:\\";
            //allow create new folder
            folderBrowserDialog1.ShowNewFolderButton = true;
            //set info
            folderBrowserDialog1.Description = "请选择文件";
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.FilePathBox.Text = folderBrowserDialog1.SelectedPath;
            }
        }
        
        private void FindBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                this.richTextBox1.FindString(this.FindBox.Text);
                this.FindBox.Visible = false;
            }
        }

        /// <summary>
        /// key map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            //map "/" --->find
            if (e.KeyCode == Keys.Oem2)
            {
                this.FindBox.Visible = true;
                this.FindBox.Focus();
            }

            //map "Ctrl+d" --->stop
            if(e.KeyCode == Keys.D && e.Control)
            {
                if (this.process != null && !this.process.HasExited)
                {
                    this.process.Kill();
                    this.richTextBox1.AppendText("\r\nExit!\r\n");
                    
                    // scroll to caret
                    this.richTextBox1.ScrollToCaret();
                }
            }
            //map "n" --->find next
            if(e.KeyCode == Keys.N && !e.Shift)
            {
                this.richTextBox1.FindMove(1);
            }

            //map "Shift+n" --->find prev
            if(e.KeyCode == Keys.N && e.Shift)
            {
                this.richTextBox1.FindMove(-1);
            }
        }
        
    }
}
