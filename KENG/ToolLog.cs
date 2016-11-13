using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToolLog
{
    public partial class ToolLog : Form
    {
        private string command;
        private string Command { get { return this.command; } set { this.command = value; } }

        private string path;
        private string Path { get { return this.path; } set { this.path = value; } }

        Process process;
        
        private Thread threadADB;
        private Thread ThreadADB { get { return this.threadADB; } set { this.threadADB = value; } }
        
        private bool isOutPut = false;
        private bool IsOutPut { get { return this.isOutPut; } set { this.isOutPut = value; } }

        private long stopTime = 0;
        private long StopTime { get { return this.stopTime; } set { this.stopTime = value; } }

        public ToolLog()
        {
            InitializeComponent();
            this.ToolLogRegeditLoad();
            this.InitRichTextBox();
        }

        private void SetCommand(string command, bool isPath)
        {
            this.Command = command;
            this.CommandLabel.Text = "Command: " + command;
        }

        private void SetFilePath(string path)
        {
            if (this.FixedCheckBox.Checked)
            {
                this.Path = path;
            }else
            {
                this.Path = null;
            }
        }

        private void InitConfig()
        {

        }

        private void InitRichTextBox()
        {
            for(int i = 0; i < 10; i++)
            {
                this.richTextBox1.AppendText(" \r\n");
            }
            this.richTextBox1.AppendText("README:\r\n");
            this.richTextBox1.AppendText("1. IF you want to fixed the file path you can input, or else please ignore!\r\n");
            this.richTextBox1.AppendText("2. Map:\r\n");
            this.richTextBox1.AppendText("      1. \"/\": Find.\r\n");
            this.richTextBox1.AppendText("      2. \"Ctrl + d\": Exit.\r\n");
            this.richTextBox1.AppendText("      3. \"n\": Find next.\r\n");
            this.richTextBox1.AppendText("      4. \"Shift + n\": Find prev.\r\n");
            this.richTextBox1.AppendText("      5. \"Mouse left double click\": Clear all back color.\r\n");
            this.richTextBox1.AppendText("      6. \"Mouse right click\": Stop auto scroll.\r\n");
        }

        private void ToolLogRegeditLoad()
        {
            try
            {
                RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE_STORAGE\\GAME_TOOLS\\TOOL_LOG", true);
                if (reg != null)
                {
                    this.SetCommand((reg.GetValue("Command") as string), this.FixedCheckBox.Checked);
                    this.SetFilePath(reg.GetValue("FilePath") as string);
                }
                reg.Close();
            }
            catch
            {
                //MessageBox.Show(ex.Message);
            }
        }

        
        private void WriteToRegedit(string key, object value)
        {
            try
            {
                RegistryKey rsg = null;
                if (Registry.CurrentUser.OpenSubKey("SOFTWARE_STORAGE\\GAME_TOOLS\\TOOL_LOG") == null)
                {
                    Registry.CurrentUser.CreateSubKey("SOFTWARE_STORAGE\\GAME_TOOLS\\TOOL_LOG");//创建
                }

                rsg = Registry.CurrentUser.OpenSubKey("SOFTWARE_STORAGE\\GAME_TOOLS\\TOOL_LOG", true);

                //rsg.SetValue("FilePath", this.FilePathLabel.Text);
                //rsg.SetValue("Command", this.Command);
                rsg.SetValue(key, value);
                rsg.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RuningThreadADB()
        {
            this.CreateProcess(this.CommandLabel.Text.Substring(9), ref this.process);
        }

        /// <summary>
        /// start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartBtn_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Focus();

            var threadStart = new ThreadStart(RuningThreadADB);
            this.ThreadADB = new Thread(threadStart);
            this.ThreadADB.Start();

            this.SetCommand(this.CommandLabel.Text.Substring(9), this.FixedCheckBox.Checked);

            this.WriteToRegedit("FilePath", this.FilePathLabel.Text);
            this.WriteToRegedit("Command", this.CommandLabel.Text.Substring(9));
        }

        private string GetFileName(string[] strArray, bool isPath)
        {
            string fileName = "";
            if (isPath)
            {
                fileName = this.FilePathLabel.Text + "\\" + strArray[0];
            }else
            {
                fileName = strArray[0];
            }
            return fileName;
        }

        private string GetArguments(string[] strArray)
        {
            //var strArray = str.Split(' ');
            string arguments = "";
            for (int i = 1; i < strArray.Length; i++)
            {
                arguments += strArray[i] + ' ';
            }
            return arguments;
        }

        /// <summary>
        /// create process
        /// async model
        /// </summary>
        /// <param name="process"></param>
        private void CreateProcess(string command, ref Process process)
        {
            process = new System.Diagnostics.Process();
            string str = command;
            var strArray = str.Split(' ');

            process.StartInfo.FileName = this.GetFileName(strArray, this.FixedCheckBox.Checked);
            process.StartInfo.Arguments = this.GetArguments(strArray);
            
            var com = strArray[0] + ' ' + this.GetArguments(strArray);
            

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
                process = null;
                MessageBox.Show(e.Message); return;
            }

            //this.timer1.Enabled = true;

            // 异步获取命令行内容  
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            // 为异步获取订阅事件  
            process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
            process.ErrorDataReceived += new DataReceivedEventHandler(process_ErrorDataReceived);

            //
            //ThreadStart threadStart = new ThreadStart(this.OutPut);
            //this.ThreadOutPut = new Thread(threadStart);
            //this.ThreadOutPut.Start();
        }

        private void OutPut()
        {
            while (this.process != null)
            {
                //天坑1号，readline没东西输出的时候，老是duz
                /*string data = "";
                Debug.Print("qian");
                //var error = this.process.StandardError.ReadLine();
                try
                {
                    data = this.process.StandardOutput.ReadLine();
                }
                catch (NullReferenceException)
                {

                }
                Debug.Print("hou");*/
                var data = this.process.StandardOutput.ReadLineAsync();

                this.Log(data);
                //data.Wait();
                //天坑2号，error和output不能放在同一个线程之中，因为总是会因为一个而堵塞。
                /*var error = this.process.StandardError.ReadLine();
                if (!string.IsNullOrEmpty(error))
                {
                    this.LogException(error + "\r\n");
                    continue;
                }*/

                //天坑3号，当我主线程想等待分线程退出后再退出时，分线程join后，主线程等待分线程结束，但是分线程中的invoke同时也在等待主线程的append，且不退出，就发生了死锁。
                //如果在分线程之中用begininvoke,那么分线程就不会等待主线程的append,而是直接返回了，接着分线程退出，接着主线程退出。
                //lock (this) {
                //    if (this.IsOutPutStop)
                //        break;

                /*
                //重构下 用Log代替
                if (String.IsNullOrEmpty(data.Result) == false)
                {
                    Debug.Print("bukong");
                    //if (this.richTextBox1.isStopScroll == true)
                    //    this.richTextBox1.isStopScroll = false;
                    switch (data.Result[0])
                    {
                        case 'E':
                            this.LogError(data + "\r\n");
                            break;
                        case 'W':
                            this.LogWarning(data + "\r\n");
                            break;
                        case 'I':
                            this.LogMessage(data + "\r\n");
                            break;
                        default:
                            this.LogException(data + "\r\n");
                            break;
                    }
                }
                else
                {
                    Debug.Print("kong");
                    if (data.Result == "")
                        continue;
                    if (this.richTextBox1.isStopScroll == false)
                        this.richTextBox1.isStopScroll = true;
                    this.Invoke((Action)delegate
                    {
                        if (this.process != null)
                        {
                            // set the current caret position to the end
                            this.richTextBox1.SelectionStart = this.richTextBox1.GetFirstCharIndexFromLine(this.richTextBox1.BottomLine);
                            // scroll it automatically
                            this.richTextBox1.ScrollToCaret();
                        }

                    });
                }
                */

               // }
            }
           
            
        }
        
        private void Log(Task<string> data)
        {
            LogAppendDelegateTask lad = new LogAppendDelegateTask(LogAppendTask);
            richTextBox1.BeginInvoke(lad, Color.Red, DateTime.Now.ToString("HH:mm:ss-") + data);
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
            if (!this.IsOutPut)
                this.IsOutPut = true;
            if (this.richTextBox1.IsStopScroll)
                this.richTextBox1.StartScroll();
            //this.richTextBox1.StartScroll();
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
                    default:
                        this.LogException(e.Data + "\r\n");
                        break;
                }
            }
            else
            {
                this.StopTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
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

        private delegate void LogAppendDelegateTask(Color color, Task<string> data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        /// <param name="text"></param>
        public void LogAppend(Color color, string text)
        {
            if (this.process != null)
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

        public void LogAppendTask(Color color, Task<string> data)
        {

            if (this.process != null)
            {
                int start = this.richTextBox1.SelectionStart;

                int len = this.richTextBox1.SelectionLength;

                int newStart = this.richTextBox1.TextLength;
                //Debug.Print("start:" + start);
                //Debug.Print("newStart:" + newStart);

                this.richTextBox1.AppendText(data.Result);

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
            richTextBox1.BeginInvoke(lad, Color.Red, DateTime.Now.ToString("HH:mm:ss-") + text);
        }

        /// <summary>
        /// LogWarning
        /// </summary>
        /// <param name="text"></param>
        public void LogWarning(string text)
        {
            LogAppendDelegate lad = new LogAppendDelegate(LogAppend);
            richTextBox1.BeginInvoke(lad, Color.Blue, DateTime.Now.ToString("HH:mm:ss-") + text);
        }

        /// <summary>
        /// LogMessage
        /// </summary>
        /// <param name="text"></param>
        public void LogMessage(string text)
        {
            LogAppendDelegate lad = new LogAppendDelegate(LogAppend);
            richTextBox1.BeginInvoke(lad, Color.DarkGray, DateTime.Now.ToString("HH:mm:ss-") + text);
        }

        /// <summary>
        /// Exception
        /// </summary>
        /// <param name="text"></param>
        public void LogException(string text)
        {
            LogAppendDelegate lad = new LogAppendDelegate(LogAppend);
            richTextBox1.BeginInvoke(lad, Color.Black, text);
        }
        #endregion


        /// <summary>
        /// Timer auto scroll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Debug.Print("stoptime:" + this.StopTime);
            Debug.Print("now:" + now);
            Debug.Print("stopscroll:" + this.richTextBox1.IsStopScroll);
            //if (this.process != null && !this.richTextBox1.isStopScroll)

            

            if (this.process != null && !this.richTextBox1.IsStopScroll)
            {
                
                //var currentLine = this.richTextBox1
                // set the current caret position to the end
                //this.richTextBox1.SelectionStart = this.richTextBox1.Text.Length;
                // scroll it automatically
                if (this.isOutPut)
                {
                    
                }
                //this.richTextBox1.ScrollToCaret();
                if (this.isOutPut && now - this.StopTime >= 2000)
                {
                    this.richTextBox1.ScrollToBottom(true);
                    this.richTextBox1.StopScroll();
                    this.IsOutPut = false;
                }
                else
                {
                    this.richTextBox1.ScrollToBottom(true);
                }
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
                this.FilePathLabel.Text = folderBrowserDialog1.SelectedPath;
            }
        }
        
        private void FindBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                this.FindBox.Visible = false;
                if (string.IsNullOrEmpty(this.FindBox.Text) == true)
                {
                    return;
                }
                switch (this.FindBox.Text[0])
                {
                    case ':':
                        this.richTextBox1.Focus();

                        /*try
                        {
                            if (this.FindBox.Text.Substring(1, 3) != "adb")
                            {
                                MessageBox.Show("指令不存在！");
                                return;
                            }
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            MessageBox.Show("指令不存在！");
                            return;
                        }*/

                        this.SetCommand(this.FindBox.Text.Substring(1), this.FixedCheckBox.Checked);

                        this.CreateProcess(this.Command, ref this.process);
                        
                        this.WriteToRegedit("Command", this.CommandLabel.Text.Substring(9));

                        break;
                    case '/':
                        this.richTextBox1.FindString(this.FindBox.Text.Substring(1));
                        this.FindBox.Visible = false;
                        break;
                }
            }
        }

        /// <summary>
        /// key map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            //map "Shift+;" --->command
            if(e.KeyCode == Keys.Oem1 && e.Shift)
            {
                this.FindBox.Visible = true;
                this.FindBox.Focus();
                this.FindBox.Text = ":";
                this.FindBox.SelectionStart = 1;
            }

            //map "/" --->find
            if (e.KeyCode == Keys.Oem2)
            {
                this.FindBox.Visible = true;
                this.FindBox.Focus();
                this.FindBox.Text = "/";
                this.FindBox.SelectionStart = 1;
            }

            //map "Ctrl+d" --->stop
            if(e.KeyCode == Keys.D && e.Control)
            {
                if (this.process != null && !this.process.HasExited)
                {
                    this.process.Kill();
                    this.process = null;
                    this.richTextBox1.AppendText("\r\nExit!\r\n");

                    // scroll to caret
                    //this.richTextBox1.ScrollToCaret();
                    this.richTextBox1.StopScroll();
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

        private void richTextBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.richTextBox1.ClearAllBackColor();
        }

        private void FixedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.SetFilePath(this.FilePathLabel.Text);
            this.SetCommand(this.Command, this.FixedCheckBox.Checked);
        }

        private void HideCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.HideCheckBox.Checked)
                this.groupBox.Visible = false;
        }

        private void AlwaysHideCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.groupBox.Visible = false;
            this.WriteToRegedit("isConfig", this.AlwaysHideCheckBox.Checked);
        }
        
        private void ToolLog_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(this.process != null)
                this.process.Close();
            //lock ?
            //lock (this)
            //{
                /*this.IsOutPutStop = true;
                if (this.ThreadOutPut != null)
                {
                   this.ThreadOutPut.Join();
                }*/
            //}  
        }
    }
}
