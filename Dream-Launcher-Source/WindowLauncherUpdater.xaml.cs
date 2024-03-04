using Ionic.Zip;
using MarcosTomaz.ATS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TS3_Dream_Launcher
{
    /*
    * This is the code responsible by the Launcher Updater window
    */

    public partial class WindowLauncherUpdater : Window
    {
        //Private variables
        private bool isUpdateInProgress = false;
        private string myDocumentsPath = "";
        private string launcherPath = "";

        //Core methods

        public WindowLauncherUpdater(string myDocumentsPath, string launcherPath)
        {
            //Prepare the window
            InitializeComponent();

            //Store variables
            this.myDocumentsPath = myDocumentsPath;
            this.launcherPath = launcherPath;

            //Prepare the UI
            PrepareTheUI();
        }

        public void PrepareTheUI()
        {
            //Block the window close if install is in progress
            this.Closing += (s, e) =>
            {
                if(isUpdateInProgress == true)
                    e.Cancel = true;
            };

            //Show the title
            statusText.Text = "Preparing Update";

            //Start the update
            StartUpdate();
        }

        public void StartUpdate()
        {
            //Inform that the update is in progress
            isUpdateInProgress = true;

            //Delete the temporary extraction folder, if exists
            if (Directory.Exists((myDocumentsPath + "/!DL-TmpCache/launcher-updater")) == true)
                Directory.Delete((myDocumentsPath + "/!DL-TmpCache/launcher-updater"), true);
            //Create the temporary directory
            Directory.CreateDirectory((myDocumentsPath + "/!DL-TmpCache/launcher-updater"));

            //Start a thread to download the update
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { myDocumentsPath, launcherPath });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) => { };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(1000);

                //Launch the notification
                threadTools.ReportNewProgress("Downloading Update");

                //Get the start params data
                string myDocumentsPath = startParams[0];
                string launcherPath = startParams[1];

                //Try to do the task
                try
                {
                    //Prepare the download for update finisher
                    if(true == true)
                    {
                        //Prepare the target download URL
                        string downloadUrl = @"https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/launcher-update-ender.zip";
                        string saveAsPath = (myDocumentsPath + @"/!DL-TmpCache/launcher-updater/launcher-update-ender.zip");
                        //Download the file
                        HttpClient httpClient = new HttpClient();
                        HttpResponseMessage httpRequestResult = httpClient.GetAsync(downloadUrl).Result;
                        httpRequestResult.EnsureSuccessStatusCode();
                        Stream downloadStream = httpRequestResult.Content.ReadAsStreamAsync().Result;
                        FileStream fileStream = new FileStream(saveAsPath, FileMode.Create, FileAccess.Write, FileShare.None);
                        downloadStream.CopyTo(fileStream);
                        httpClient.Dispose();
                        fileStream.Dispose();
                        fileStream.Close();
                        downloadStream.Dispose();
                        downloadStream.Close();
                    }

                    //Wait some time
                    threadTools.MakeThreadSleep(1000);

                    //Prepare the download for launcher package
                    if (true == true)
                    {
                        //Prepare the target download URL
                        string downloadUrl = @"https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/launcher-current-compilation.zip";
                        string saveAsPath = (myDocumentsPath + @"/!DL-TmpCache/launcher-updater/launcher-current-compilation.zip");
                        //Download the file
                        HttpClient httpClient = new HttpClient();
                        HttpResponseMessage httpRequestResult = httpClient.GetAsync(downloadUrl).Result;
                        httpRequestResult.EnsureSuccessStatusCode();
                        Stream downloadStream = httpRequestResult.Content.ReadAsStreamAsync().Result;
                        FileStream fileStream = new FileStream(saveAsPath, FileMode.Create, FileAccess.Write, FileShare.None);
                        downloadStream.CopyTo(fileStream);
                        httpClient.Dispose();
                        fileStream.Dispose();
                        fileStream.Close();
                        downloadStream.Dispose();
                        downloadStream.Close();
                    }

                    //Store the launcher path in a file
                    File.WriteAllText((myDocumentsPath + @"/!DL-TmpCache/launcher-updater/launcherPath.dinf"), launcherPath);

                    //Show the status
                    threadTools.ReportNewProgress("Extracting Package");

                    //Wait some time
                    threadTools.MakeThreadSleep(1000);

                    //Extract the update ender
                    ZipFile zipFile = ZipFile.Read((myDocumentsPath + @"/!DL-TmpCache/launcher-updater/launcher-update-ender.zip"));
                    foreach (ZipEntry entry in zipFile)
                        entry.Extract((myDocumentsPath + @"/!DL-TmpCache/launcher-updater"), ExtractExistingFileAction.OverwriteSilently);
                    zipFile.Dispose();

                    //Show the status
                    threadTools.ReportNewProgress("Restarting Updater");

                    //Wait some time
                    threadTools.MakeThreadSleep(1000);

                    //Calculate the working directory
                    string workingDirectory = (myDocumentsPath + @"/!DL-TmpCache/launcher-updater");
                    //Start the update ender
                    Process enderProcess = new Process();
                    enderProcess.StartInfo.FileName = System.IO.Path.Combine(workingDirectory, "LauncherUpdateEnder.exe");
                    enderProcess.StartInfo.WorkingDirectory = workingDirectory;
                    enderProcess.StartInfo.UseShellExecute = true;
                    enderProcess.Start();

                    //Return a success response
                    return new string[] { "success" };
                }
                catch (Exception ex)
                {
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
                    this.Close();

                //Hide this window if was a success
                if (threadTaskResponse == "success")
                    this.Visibility = Visibility.Hidden;
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }
    }
}
