using MarcosTomaz.ATS;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sims3Launcher
{
    /*
     * This is the code responsible by the working of the Dream Launcher. This is a wrapper that
     * replace the original "Sims3Launcher.exe" to start the Dream Launcher in a safe and
     * confiable way, and in a way that Steam still recognizes as the original game.
    */

    public partial class MainWindow : Window
    {
        //Private variables
        private string realDirPathForThisExecutable = "";
        private Process dreamLauncherCurrentProcess = null;
        private string possibleExecutionError = "";

        //Core methods

        public MainWindow()
        {
            //Process initialization arguments, if have
            bool isLaunchedBySims3Store = ProcessPossibleCliArguments(Environment.GetCommandLineArgs());
            //If the program was launched by The Sims 3 store, e.g., in Browser (store.thesims3.com), then warn the user and cancel this...
            if (isLaunchedBySims3Store == true)
            {
                //Warn to user
                MessageBox.Show("Everything ready!\n\nThe request will be processed soon by Dream Launcher. Please open Dream Launcher if it is not open.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);

                //Stop the execution of this instance
                System.Windows.Application.Current.Shutdown();

                //Cancel the execution
                return;
            }

            //Check if have another process of the launcher already opened. If have, cancel this...
            string processName = Process.GetCurrentProcess().ProcessName;
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length > 1)
            {
                //Warn abou the problem
                MessageBox.Show("There is already an instance of \"Sims3Launcher\" running!", "Error");

                //Stop the execution of this instance
                System.Windows.Application.Current.Shutdown();

                //Cancel the execution
                return;
            }

            //Initialize the window
            InitializeComponent();

            //Create a thread to start and monitor the Dream Launcher process
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { });
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(3000);

                //Request to start Dream Launcher process
                threadTools.ReportNewProgress("startDreamLauncherProcess");

                //Create a monitor loop
                while (true)
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(3000);

                    //If was finished the Dream Launcher process, break the monitor loop
                    if (dreamLauncherCurrentProcess == null  || dreamLauncherCurrentProcess.HasExited == true)
                        break;
                }

                //Return empty response
                return new string[] { };
            };
            asyncTask.onNewProgress_RunMainThread += (sourceWindow, newProgress) =>
            {
                //If don't is the expected request, ignore
                if (newProgress != "startDreamLauncherProcess")
                    return;

                //Identify the real path for this executable
                realDirPathForThisExecutable = System.IO.Path.GetDirectoryName(System.AppContext.BaseDirectory);

                //If was requested to start the Dream Launcher process, try to start it, and store reference for the process
                try
                {
                    //Create a new process
                    Process newProcess = new Process();
                    newProcess.StartInfo.FileName = System.IO.Path.Combine(realDirPathForThisExecutable, "TS3 Dream Launcher.exe");
                    newProcess.StartInfo.WorkingDirectory = realDirPathForThisExecutable;
                    newProcess.StartInfo.UseShellExecute = true;
                    newProcess.StartInfo.Verb = "runas";
                    newProcess.Start();

                    //Store it
                    dreamLauncherCurrentProcess = newProcess;

                    //Hide the window
                    this.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex) { possibleExecutionError = ex.Message; }
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //Finish this application
                ShutdownApplication();
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        private bool ProcessPossibleCliArguments(string[] cliArgs)
        {
            //If not have elements, cancel
            if (cliArgs == null || cliArgs.Length == 0)
                return false;

            //Prepare the value to return
            bool toReturn = false;

            //Debug the CLI Args found
            StringBuilder receivedCliArgs = new StringBuilder();
            receivedCliArgs.Append("Received CLI Arguments...");
            for (int i = 0; i < cliArgs.Length; i++)
                receivedCliArgs.Append(("\nElement " + i + ": \"" + cliArgs[i] + "\""));
            string receivedCliArgsInfoString = receivedCliArgs.ToString();
            Debug.WriteLine(receivedCliArgsInfoString);

            //If is a The Sims 3 Store URI Request...
            if (cliArgs.Length == 2 && receivedCliArgsInfoString.Contains("sims3://") == true)
            {
                //Store the request file, to Dream Launcher check it
                File.WriteAllText((Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Sims3StoreUriRequest.s3dl"), cliArgs[1]);

                //Warn that this open of the "Sims3Launcher.exe" was by the Sims 3 Store
                toReturn = true;
            }

            //Return the result
            return toReturn;
        }

        private void ShutdownApplication()
        {
            //If have error, write the error reason
            if (possibleExecutionError != "")
                File.WriteAllText((Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Sims3Launcher-LastError.log"), possibleExecutionError);

            //Shutdown the application
            System.Windows.Application.Current.Shutdown();
        }
    }
}