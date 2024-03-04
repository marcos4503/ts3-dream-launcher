using Ionic.Zip;
using MarcosTomaz.ATS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
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

namespace LauncherUpdateEnder
{
    /*
    * This is the code responsible by the Launcher Update Ender that applies the
    * downloaded update files on the Dream Launcher files
    */

    public partial class MainWindow : Window
    {
        //Private variables
        private bool isUpdateInProgress = false;

        //Core methods

        public MainWindow()
        {
            //Prepare the window
            InitializeComponent();

            //Prepare the UI
            PrepareTheUI();
        }

        private void PrepareTheUI()
        {
            //Block the window close if install is in progress
            this.Closing += (s, e) =>
            {
                if (isUpdateInProgress == true)
                    e.Cancel = true;
            };

            //Show the status
            statusText.Text = "Resuming";

            //Start the ender
            StartUpdateEnder();
        }

        private void StartUpdateEnder()
        {
            //Inform that the update is in progress
            isUpdateInProgress = true;

            //Start a thread to apply the update
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) => { };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(1000);

                //Launch the notification
                threadTools.ReportNewProgress("Loading");

                //Try to do the task
                try
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(1000);

                    //Launch the notification
                    threadTools.ReportNewProgress("Stopping Processes");

                    //Wait some time
                    threadTools.MakeThreadSleep(1000);

                    //Stop all Dream Launcher processes
                    Process dreamLauncherProcess = Process.GetProcessesByName("TS3 Dream Launcher").FirstOrDefault();
                    if (dreamLauncherProcess != null)
                        KillProcessAndChildren(dreamLauncherProcess.Id);
                    //Wait some time
                    threadTools.MakeThreadSleep(5000);
                    Process sims3LauncherProcess = Process.GetProcessesByName("Sims3Launcher").FirstOrDefault();
                    if (sims3LauncherProcess != null)
                        KillProcessAndChildren(sims3LauncherProcess.Id);
                    //Wait some time
                    threadTools.MakeThreadSleep(5000);

                    //Load the launcher path
                    string launcherPath = File.ReadAllText((Directory.GetCurrentDirectory() + @"/launcherPath.dinf"));

                    //Inform that is copying files
                    threadTools.ReportNewProgress("Copying Files");

                    //Wait some time
                    threadTools.MakeThreadSleep(2000);

                    //Copy the launcher zip file to the launcher path
                    if (File.Exists((launcherPath + "/launcher-current-compilation.zip")) == true)
                        File.Delete((launcherPath + "/launcher-current-compilation.zip"));
                    File.Copy((Directory.GetCurrentDirectory() + @"/launcher-current-compilation.zip"), (launcherPath + "/launcher-current-compilation.zip"));

                    //Show the status
                    threadTools.ReportNewProgress("Extracting Package");

                    //Wait some time
                    threadTools.MakeThreadSleep(1000);

                    //Extract the update ender
                    ZipFile zipFile = ZipFile.Read((launcherPath + "/launcher-current-compilation.zip"));
                    foreach (ZipEntry entry in zipFile)
                        if(entry.FileName.Contains("prefs.json") == false)   //<-- Ignore "prefs.json" if exists inside the ZIP file
                            entry.Extract(launcherPath, ExtractExistingFileAction.OverwriteSilently);
                    zipFile.Dispose();

                    //Show the status
                    threadTools.ReportNewProgress("Cleaning Files");

                    //Wait some time
                    threadTools.MakeThreadSleep(1000);

                    //Clean files
                    File.Delete((launcherPath + "/launcher-current-compilation.zip"));

                    //Show the status
                    threadTools.ReportNewProgress("Initializing Dream Launcher");

                    //Wait some time
                    threadTools.MakeThreadSleep(1000);

                    //Calculate the working directory
                    string workingDirectory = launcherPath;
                    //Start the launcher
                    Process launcherProcess = new Process();
                    launcherProcess.StartInfo.FileName = System.IO.Path.Combine(workingDirectory, "Sims3Launcher.exe");
                    launcherProcess.StartInfo.WorkingDirectory = workingDirectory;
                    launcherProcess.StartInfo.UseShellExecute = true;
                    launcherProcess.Start();

                    //Create the success update file
                    File.WriteAllText((Directory.GetCurrentDirectory() + @"/update-successfully.ok"), "OK");

                    //Return a success response
                    return new string[] { "success" };
                }
                catch (Exception ex)
                {
                    //Write a error
                    File.WriteAllText("error-log.txt", (ex.Message + "\n\n" + ex.StackTrace));

                    //Return a error response
                    return new string[] { "error" };
                }

                //Finish the thread...
                return new string[] { "none" };
            };
            asyncTask.onNewProgress_RunMainThread += (callerWindow, newProgress) =>
            {
                //Show the status
                statusText.Text = newProgress;
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //Get the thread response
                string threadTaskResponse = backgroundResult[0];

                //Inform that update was finished
                isUpdateInProgress = false;

                //If have a response different from success, stop
                if (threadTaskResponse != "success")
                    statusText.Text = "ERROR: Update Error";

                //Hide this window if was a success
                if (threadTaskResponse == "success")
                {
                    this.Visibility = Visibility.Hidden;
                    System.Windows.Application.Current.Shutdown();
                }
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        //Auxiliar methods

        private void KillProcessAndChildren(int processId)
        {
            // Cannot close 'system idle process'.
            if (processId == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + processId);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(processId);
                if(proc.ProcessName.ToLower().Contains("launcherupdateender") == false)   //<--- Ignore if is the process "LauncherUpdateEnder"
                    proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }
    }
}
