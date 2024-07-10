using MarcosTomaz.ATS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
     * This script is resposible by the work of the "ToolItem" that represents each tool available
     * in the Dream Launcher.
    */

    public partial class LocalToolItem : UserControl
    {
        //Cache variables
        private bool isRunning = false;
        private Process toolProcess = null;

        //Private variables
        private MainWindow instantiatedByWindow = null;
        private string toolExeFolderPath = "";
        private string toolExeName = "";

        //Core methods

        public LocalToolItem(MainWindow instantiatedBy)
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

        public void SetLocalToolExePath(string toolExeFolderPath, string toolExeName)
        {
            //Store the information
            this.toolExeFolderPath = toolExeFolderPath;
            this.toolExeName = toolExeName;
        }

        public void Prepare()
        {
            //Update the button
            UpdateButton();

            //Setup the button click
            button.Click += (s, e) => { OnClickButton(); };
        }

        //Auxiliar methods

        private void UpdateButton()
        {
            //If is running, show running information
            if (isRunning == true)
            {
                button.IsEnabled = false;
                button.Content = instantiatedByWindow.GetStringApplicationResource("launcher_tools_running");
            }

            //If is not running, show information
            if (isRunning == false)
            {
                button.Content = instantiatedByWindow.GetStringApplicationResource("launcher_tools_open");
                button.IsEnabled = true;
            }
        }

        private void OnClickButton()
        {
            //Open the tool
            OpenTool();
        }

        private void OpenTool()
        {
            //Start a thread to open and monitor the tool
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(instantiatedByWindow, new string[] { (toolExeFolderPath + "/" + toolExeName) });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) =>
            {
                //Start the tool
                Process newProcess = new Process();
                newProcess.StartInfo.FileName = System.IO.Path.Combine(new string[] { toolExeFolderPath, toolExeName });
                newProcess.StartInfo.WorkingDirectory = System.IO.Path.Combine(toolExeFolderPath);
                newProcess.Start();
                //Store it
                toolProcess = newProcess;

                //Add the task to queue
                instantiatedByWindow.AddTask((toolExeName + "_Running"), "Running tool.");

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
                instantiatedByWindow.RemoveTask((toolExeName + "_Running"));

                //Inform that is not running more
                isRunning = false;

                //Update the button
                UpdateButton();
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }
    }
}
