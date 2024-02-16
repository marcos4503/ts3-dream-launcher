using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TS3_Dream_Launcher.Controls.ListItems
{
    /*
     * This script is resposible by the work of the "LogItem" that represents logs found
    */

    public partial class LogItem : UserControl
    {
        //Private methods
        private MainWindow instantiatedByWindow = null;
        private string nppWorkingDir = "";
        private string nppExePath = "";
        private string logFilePath = "";

        //Core methods

        public LogItem(MainWindow instantiatedBy)
        {
            //Initialize the component
            InitializeComponent();

            //Inform that is the DataConext of this User Control
            this.DataContext = this;

            //Store reference for window that was instantiated this item
            this.instantiatedByWindow = instantiatedBy;
        }
    
        //Public methods

        public void SetTitle(string logTitle)
        {
            //Set this log title
            title.Content = logTitle;
        }

        public void SetNppPath(string workingDirectory, string programPath)
        {
            //Store the path
            nppWorkingDir = workingDirectory;
            nppExePath = programPath;
        }

        public void SetLogPath(string fullLogPath)
        {
            //Store the path
            logFilePath = fullLogPath;
        }
    
        public void Prepare()
        {
            //Prepare this log to be opened
            openButton.Click += (s, e) => 
            {
                //If don't have notepad++, cancel
                if(File.Exists(nppExePath) == false)
                {
                    instantiatedByWindow.ShowToast(instantiatedByWindow.GetStringApplicationResource("launcher_cache_logsViewerOpenError"), MainWindow.ToastType.Error);
                    return;
                }

                //Start the CMD process
                Process cmdProcess = new Process();
                cmdProcess.StartInfo.FileName = "cmd.exe";
                cmdProcess.StartInfo.RedirectStandardInput = true;
                cmdProcess.StartInfo.UseShellExecute = false;
                cmdProcess.Start();
                //Repass commands to CMD window
                using (StreamWriter sw = cmdProcess.StandardInput)
                {
                    if (sw.BaseStream.CanWrite)
                    {
                        sw.WriteLine(("cd /d \"" + nppWorkingDir + "\""));
                        sw.WriteLine(("\"" + nppExePath + "\" \"" + logFilePath + "\""));
                    }
                }
            };
        }
    }
}
