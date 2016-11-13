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
        #region property
        private string command;
        private string Command { get { return this.command; } set { this.command = value; } }

        private string path;
        private string Path { get { return this.path; } set { this.path = value; } }

        private string isHideConfig;
        private string IsHideConfig { get { return this.isHideConfig; } set { this.isHideConfig = value; } }

        Process process;
        
        private Thread threadADB;
        private Thread ThreadADB { get { return this.threadADB; } set { this.threadADB = value; } }
       
        private long stopTime = 0;
        private long StopTime { get { return this.stopTime; } set { this.stopTime = value; } }
        #endregion

        #region main
        public ToolLog()
        {
            InitializeComponent();
            this.ReadFromRegedt();
            this.InitRichTextBox();
            this.InitConfig();
        }
        #endregion

        #region init
        private void InitForm()
        {
            //this.Size = 
        }

        private void InitConfig()
        {
            if (this.IsHideConfig == "True")
            {
                this.groupBox.Visible = false;
                this.AlwaysHideCheckBox.Checked = true;
                this.richTextBox1.Focus();
            }
               
        }

        private void InitRichTextBox()
        {
            for(int i = 0; i < 8; i++)
            {
                this.richTextBox1.AppendText(" \r\n");
            }
            this.richTextBox1.AppendText("README:\r\n");
            this.richTextBox1.AppendText("1. Config:\r\n");
            this.richTextBox1.AppendText("      * \"File Path\": IF you want to fixed the file path you can Browes, or else please ignore!\r\n");
            this.richTextBox1.AppendText("      * \"Hide\": Hide config panel.\r\n");
            this.richTextBox1.AppendText("      * \"AlwaysHide\": Always hide config panel.\r\n");

            this.richTextBox1.AppendText("2. Map:\r\n");
            this.richTextBox1.AppendText("      * \"/\": Find.\r\n");
            this.richTextBox1.AppendText("      * \"Ctrl + d\": Exit.\r\n");
            this.richTextBox1.AppendText("      * \"n\": Find next.\r\n");
            this.richTextBox1.AppendText("      * \"Shift + n\": Find prev.\r\n");
            this.richTextBox1.AppendText("      * \"Mouse left double click\": Clear all back color.\r\n");
            this.richTextBox1.AppendText("      * \"Mouse right click\": Stop auto scroll.\r\n");

            this.richTextBox1.AppendText("3. Command:\r\n");
            this.richTextBox1.AppendText("      * \":config\": Show the config panel.\r\n");
            this.richTextBox1.AppendText("      * \":clear\": clear all text.\r\n");
            
            for (int i = 0; i < 8; i++)
            {
                this.richTextBox1.AppendText(" \r\n");
            }
        }
        #endregion

        #region set command and path
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
            }
            else
            {
                this.Path = null;
            }
        }
        #endregion

        #region regedit: read and write 
        private void ReadFromRegedt()
        {
            try
            {
                RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE_STORAGE\\GAME_TOOLS\\TOOL_LOG", true);
                if (reg != null)
                {
                    this.SetCommand((reg.GetValue("Command") as string), this.FixedCheckBox.Checked);
                    this.SetFilePath(reg.GetValue("FilePath") as string);
                    this.IsHideConfig = reg.GetValue("IsHideConfig") as string;
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
                
                rsg.SetValue(key, value);
                rsg.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region process
        /// <summary>
        /// process run on the thread adb
        /// </summary>
        private void RuningThreadADB()
        {
            this.CreateProcess(this.CommandLabel.Text.Substring(9), ref this.process);
        }

        /// <summary>
        /// start
        /// </summary>
        private void Start()
        {
            //focus
            this.richTextBox1.Focus();

            //create a new thread for process
            var threadStart = new ThreadStart(RuningThreadADB);
            this.ThreadADB = new Thread(threadStart);
            this.ThreadADB.Start();
            
            //write to regedit    
            this.WriteToRegedit("FilePath", this.FilePathLabel.Text);
            this.WriteToRegedit("Command", this.CommandLabel.Text.Substring(9));
        }
        
        /// <summary>
        /// get file name for crate process
        /// </summary>
        /// <param name="strArray"></param>
        /// <param name="isPath"></param>
        /// <returns></returns>
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

        /// <summary>
        /// get arguments for create process
        /// </summary>
        /// <param name="strArray"></param>
        /// <returns></returns>
        private string GetArguments(string[] strArray)
        {
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
            
            // 异步获取命令行内容  
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            // 为异步获取订阅事件  
            process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
            process.ErrorDataReceived += new DataReceivedEventHandler(process_ErrorDataReceived);
        }
        
        /// <summary>
        /// out put
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            // 这里仅做输出的示例，实际上您可以根据情况取消获取命令行的内容  
            // 参考：process.CancelOutputRead() 
            // force stop judge
            if(!this.richTextBox1.IsForceStopScroll)
            {
                if (this.richTextBox1.IsStopScroll)
                    this.richTextBox1.StartScroll();
            }

            // out put
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

            // if over the stop time 500 ms be judged stop out put
            this.StopTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
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

        #region log info , with BeginInvoke, support other thread
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
            if (this.process != null)
            {
                int start = this.richTextBox1.SelectionStart;

                int len = this.richTextBox1.SelectionLength;

                int newStart = this.richTextBox1.TextLength;

                this.richTextBox1.AppendText(text);

                int newEnd = this.richTextBox1.TextLength;

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

        #endregion

        #region timer
        /// <summary>
        /// Timer auto scroll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            // force stop scroll
            if (this.richTextBox1.IsForceStopScroll && !this.richTextBox1.IsStopScroll)
            {
                this.richTextBox1.StopScroll();
            }
            else
            {
                // stop scroll base on if now is out putting
                if (this.process != null && !this.richTextBox1.IsStopScroll)
                {
                    var now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    if (now - this.StopTime >= 500)
                    {
                        this.richTextBox1.ScrollToBottom(true);
                        this.richTextBox1.StopScroll();
                    }
                    else
                    {
                        this.richTextBox1.ScrollToBottom(true);
                    }
                }
            }
        }
        #endregion
        
        #region view event

        #region config
        /// <summary>
        /// choose file path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// fixed file path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FixedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.SetFilePath(this.FilePathLabel.Text);
            this.SetCommand(this.Command, this.FixedCheckBox.Checked);
        }

        /// <summary>
        /// hide config
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HideCheckBox_CheckedChanged(object sender, EventArgs e)
        {
           
            this.groupBox.Visible = false;
            this.HideCheckBox.Checked = false;
            this.richTextBox1.Focus();
        }

        /// <summary>
        /// always hide config
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlwaysHideCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.AlwaysHideCheckBox.Checked)
            {
                this.groupBox.Visible = false;
                this.richTextBox1.Focus();
            }
            this.WriteToRegedit("IsHideConfig", this.AlwaysHideCheckBox.Checked);
        }
        #endregion

        /// <summary>
        /// Execute
        /// </summary>
        /// <param name="command"></param>
        private void ExecuteCommand(string command)
        {
            switch (command)
            {
                case "config":
                    this.groupBox.Visible = true;
                    this.groupBox.BringToFront();
                    break;
                case "clear":
                    this.richTextBox1.Text = "";
                    break;
                default:
                    this.SetCommand(this.FindBox.Text.Substring(1), this.FixedCheckBox.Checked);
                    this.Start();
                    break;
            }
        }

        /// <summary>
        /// show find box
        /// </summary>
        /// <param name="isShow"></param>
        private void ShowFindBox(bool isShow)
        {
            this.FindBox.Visible = isShow;
            this.FindBox.BringToFront();
        }

        /// <summary>
        /// start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartBtn_Click(object sender, EventArgs e)
        {
            this.SetCommand(this.CommandLabel.Text.Substring(9), this.FixedCheckBox.Checked);
            this.Start();
        }

        
        
        /// <summary>
        /// complete input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                this.ShowFindBox(false);
                if (string.IsNullOrEmpty(this.FindBox.Text) == true)
                {
                    return;
                }
                switch (this.FindBox.Text[0])
                {
                    case ':':
                        this.ExecuteCommand(this.FindBox.Text.Substring(1));
                        break;
                    case '/':
                        this.richTextBox1.FindString(this.FindBox.Text.Substring(1));
                        this.ShowFindBox(false);
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
                this.ShowFindBox(true);
                this.FindBox.Focus();
                this.FindBox.Text = ":";
                this.FindBox.SelectionStart = 1;
            }

            //map "/" --->find
            if (e.KeyCode == Keys.Oem2)
            {
                this.ShowFindBox(true);
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
                    this.richTextBox1.StopScroll();
                    this.richTextBox1.ScrollToCaret();
                }
            }
            //map "n" --->find next
            if(e.KeyCode == Keys.N && !e.Shift)
            {
                this.richTextBox1.FindMove(1, this.TipsLabel);
            }

            //map "Shift+n" --->find prev
            if(e.KeyCode == Keys.N && e.Shift)
            {
                this.richTextBox1.FindMove(-1, this.TipsLabel);
            }
        }
        
        /// <summary>
        /// clear all back color
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void richTextBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.richTextBox1.ClearAllBackColor();
        }

        
        
        /// <summary>
        /// close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolLog_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.process != null)
            {
                //this.process.Kill();
                this.process.Close();
            }
        }
        #endregion
    }
}
