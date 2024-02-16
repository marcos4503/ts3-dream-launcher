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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TS3_Dream_Launcher.Controls.ListItems
{
    /*
     * This script is resposible by the work of the "ToolItem" that represents each tool available
     * in the Dream Launcher.
    */

    public partial class ToolItem : UserControl
    {
        //Cache variables
        private bool isRunning = false;
        private Process toolProcess = null;

        //Private variables
        private MainWindow instantiatedByWindow = null;
        private string contentsFolderPath = "";
        private string toolFolderName = "";
        private string toolExePathInsideFolder = "";
        private string myDocumentsPath = "";
        private string toolDownloadUrl = "";

        //Core methods

        public ToolItem(MainWindow instantiatedBy)
        {
            //Initialize the component
            InitializeComponent();

            //Inform that is the DataConext of this User Control
            this.DataContext = this;

            //Store reference for window that was instantiated this item
            this.instantiatedByWindow = instantiatedBy;
        }

        //Public methods

        public void SetIcon(string iconUri)
        {
            //Set the patch icon
            ImageBrush photoBrush = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/" + iconUri)));
            photoBrush.Stretch = Stretch.UniformToFill;
            icon.Background = photoBrush;
        }

        public void SetInformation(string information)
        {
            //Set the information
            info.ToolTip = information;
        }

        public void SetTitle(string toolName)
        {
            //Set the title
            title.Text = toolName;
        }

        public void SetDescription(string toolDescription)
        {
            //Set the description
            description.Text = toolDescription;
        }

        public void SetCreator(string creatorName)
        {
            //Set the creator
            createdby.Text = instantiatedByWindow.GetStringApplicationResource("launcher_tools_createdBy").Replace("%name%", creatorName);
        }

        public void SetContentsFolderPath(string contentsFolderPath)
        {
            //Store the information
            this.contentsFolderPath = contentsFolderPath;
        }

        public void SetToolFolderName(string toolFolderName)
        {
            //Store the information
            this.toolFolderName = toolFolderName;
        }

        public void SetToolExePathInsideFolder(string toolExePathInsideFolder)
        {
            //Store the information
            this.toolExePathInsideFolder = toolExePathInsideFolder;
        }

        public void SetMyDocumentsPath(string myDocumentsPath)
        {
            //Store the information
            this.myDocumentsPath = myDocumentsPath;
        }

        public void SetToolDownloadURL(string toolDownloadUrl)
        {
            //Store the information
            this.toolDownloadUrl = toolDownloadUrl;
        }

        public void Prepare()
        {
            //Show only the button
            button.Visibility = Visibility.Visible;
            working.Visibility = Visibility.Collapsed;

            //Update the button
            UpdateButton();

            //Setup the button click
            button.Click += (s, e) => { OnClickButton(); };
        }

        //Auxiliar methods

        private bool isToolInstalled()
        {
            //Prepare the response
            bool toReturn = false;

            //Check if is installed
            if (File.Exists((contentsFolderPath + "/" + toolFolderName + "/" + toolExePathInsideFolder)) == true)
                toReturn = true;

            //Return the response
            return toReturn;
        }
    
        private void UpdateButton()
        {
            //If is running, show running information
            if(isRunning == true)
            {
                button.IsEnabled = false;
                button.Content = instantiatedByWindow.GetStringApplicationResource("launcher_tools_running");
            }

            //If is not running, show information
            if(isRunning == false)
            {
                if (isToolInstalled() == true)
                    button.Content = instantiatedByWindow.GetStringApplicationResource("launcher_tools_open");
                if (isToolInstalled() == false)
                    button.Content = instantiatedByWindow.GetStringApplicationResource("launcher_tools_install");
                button.IsEnabled = true;
            }
        }
    
        private void OnClickButton()
        {
            //If the tool is not installed, start install it, and cancel
            if(isToolInstalled() == false)
            {
                InstallTool();
                return;
            }

            //Open the tool
            OpenTool();
        }

        private void InstallTool()
        {
            //Start a thread to do the install
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(instantiatedByWindow, new string[] 
            {
                contentsFolderPath,
                toolFolderName,
                toolExePathInsideFolder,
                myDocumentsPath,
                toolDownloadUrl
            });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) =>
            {
                //Add the task to queue
                instantiatedByWindow.AddTask((toolFolderName + "_Install"), "Installing tool.");

                //Show progressbar
                button.Visibility = Visibility.Collapsed;
                working.Visibility = Visibility.Visible;
            };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Get all needed variables
                string contentsFolderPath = startParams[0];
                string toolFolderName = startParams[1];
                string toolExePathInsideFolder = startParams[2];
                string myDocumentsPath = startParams[3];
                string toolDownloadUrl = startParams[4];

                //Wait some time
                threadTools.MakeThreadSleep(5000);

                //Try to do the task
                try
                {
                    //Create the tool folder
                    Directory.CreateDirectory((contentsFolderPath + "/" + toolFolderName));

                    //Download the tool
                    string saveAsPath = (myDocumentsPath + @"/!DL-TmpCache/dl-" + toolFolderName + ".zip");
                    //Download the "Mods" folder sync
                    HttpClient httpClient = new HttpClient();
                    HttpResponseMessage httpRequestResult = httpClient.GetAsync(toolDownloadUrl).Result;
                    httpRequestResult.EnsureSuccessStatusCode();
                    Stream downloadStream = httpRequestResult.Content.ReadAsStreamAsync().Result;
                    FileStream fileStream = new FileStream(saveAsPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    downloadStream.CopyTo(fileStream);
                    httpClient.Dispose();
                    fileStream.Dispose();
                    fileStream.Close();
                    downloadStream.Dispose();
                    downloadStream.Close();

                    //Prepare the position of temp file
                    string tempFilePath = (contentsFolderPath + "/" + System.IO.Path.GetFileName(saveAsPath));
                    //Copy the downloaded file to contents folder
                    File.Copy(saveAsPath, tempFilePath);

                    //Extract to install folder
                    ZipFile zipFile = ZipFile.Read(tempFilePath);
                    foreach (ZipEntry entry in zipFile)
                        entry.Extract((contentsFolderPath + "/" + toolFolderName), ExtractExistingFileAction.OverwriteSilently);
                    zipFile.Dispose();

                    //Delete the copied temp file
                    File.Delete(tempFilePath);

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
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //Get thread response
                string threadTaskResponse = backgroundResult[0];

                //If have a response different from success, inform error
                if (threadTaskResponse != "success")
                    instantiatedByWindow.ShowToast(instantiatedByWindow.GetStringApplicationResource("launcher_tools_installError"), MainWindow.ToastType.Error);

                //Remove the task from queue
                instantiatedByWindow.RemoveTask((toolFolderName + "_Install"));

                //Hide progressbar
                button.Visibility = Visibility.Visible;
                working.Visibility = Visibility.Collapsed;

                //Update the button
                UpdateButton();
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        private void OpenTool()
        {
            //Start a thread to open and monitor the tool
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(instantiatedByWindow, new string[] { (contentsFolderPath + "/" + toolFolderName + "/" + toolExePathInsideFolder) });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) =>
            {
                //Start the tool
                Process newProcess = new Process();
                newProcess.StartInfo.FileName = System.IO.Path.Combine(new string[] { contentsFolderPath, toolFolderName, toolExePathInsideFolder });
                newProcess.StartInfo.WorkingDirectory = System.IO.Path.Combine(contentsFolderPath, toolFolderName);
                newProcess.Start();
                //Store it
                toolProcess = newProcess;

                //Add the task to queue
                instantiatedByWindow.AddTask((toolFolderName + "_Running"), "Running tool.");

                //Inform that is running
                isRunning = true;

                //Update the button
                UpdateButton();
            };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Create a monitor loop
                while (true)
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(1500);

                    //If was finished the tool process, break the monitor loop
                    if (toolProcess.HasExited == true)
                        break;
                }

                //Finish the thread...
                return new string[] { "none" };
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //Remove the task from queue
                instantiatedByWindow.RemoveTask((toolFolderName + "_Running"));

                //Inform that is not running more
                isRunning = false;

                //Update the button
                UpdateButton();
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }
    }
}
