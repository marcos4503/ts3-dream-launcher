using CoroutinesDotNet;
using CoroutinesForWpf;
using MarcosTomaz.ATS;
using System;
using System.Collections;
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
using System.Windows.Shapes;
using TS3_Dream_Launcher.Controls.ListItems;

namespace TS3_Dream_Launcher
{
    /*
    * This is the code responsible by the CASPs Editor
    */

    public partial class WindowCASPsEditor : Window
    {
        //Cache variables
        private Process caspsEditorCliProcess = null;
        private List<string> caspsEditorReceivedOutputLines = new List<string>();
        private List<CaspItem> instantiatedCaspItems = new List<CaspItem>();

        //Private variables
        private MainWindow mainWindow = null;
        private bool isCommandInProgress = false;
        private string contentsPath = "";
        private string caspEditorCachePath = "";
        private string packagePath = "";

        //Core methods

        public WindowCASPsEditor(MainWindow mainWindow, string contentsPath, string caspEditorCachePath, string packagePath)
        {
            //Prepare the window
            InitializeComponent();

            //Store the data
            this.mainWindow = mainWindow;
            this.contentsPath = contentsPath;
            this.caspEditorCachePath = caspEditorCachePath;
            this.packagePath = packagePath;

            //Prepare the UI
            PrepareTheUI();
        }

        private void PrepareTheUI()
        {
            //Block the window close if install is in progress
            this.Closing += (s, e) =>
            {
                if (isCommandInProgress == true)
                {
                    MessageBox.Show(mainWindow.GetStringApplicationResource("launcher_mods_installedTab_caspe_busyText"),
                                    mainWindow.GetStringApplicationResource("launcher_mods_installedTab_caspe_busyTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                    return;
                }

                //Close the package too
                if (caspsEditorCliProcess != null && caspsEditorCliProcess.HasExited == false)
                {
                    //Stop the Dl3CASPsEditor process
                    caspsEditorCliProcess.StandardInput.WriteLine("pexit");
                    //Kill the CMD process
                    caspsEditorCliProcess.Kill();
                }
            };

            //Load the text
            emptyWarn.Content = mainWindow.GetStringApplicationResource("launcher_mods_installedTab_caspe_emptyMsg");
            saveButton.Content = mainWindow.GetStringApplicationResource("launcher_mods_installedTab_caspe_saveButton");

            //Change to the correct screen
            loadingGif.Visibility = Visibility.Visible;
            savingGif.Visibility = Visibility.Collapsed;
            contentScroll.Visibility = Visibility.Collapsed;
            saveButton.IsEnabled = false;

            //Prepare the buttons
            saveButton.Click += (s, e) => { Save(); };

            //Start the CLI process of the Dl3CASPsEditor tool
            StartCliProcess();
        }

        private void StartCliProcess()
        {
            //Inform that read is in progress
            isCommandInProgress = true;

            //Start a new thread to start the process
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) => { };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Prepare the CMD process
                Process cmdProcess = new Process();
                cmdProcess.StartInfo.FileName = "cmd.exe";
                cmdProcess.StartInfo.Arguments = "/k";
                cmdProcess.StartInfo.UseShellExecute = false;
                cmdProcess.StartInfo.CreateNoWindow = true;
                cmdProcess.StartInfo.RedirectStandardInput = true;
                cmdProcess.StartInfo.RedirectStandardOutput = true;
                cmdProcess.StartInfo.WorkingDirectory = @"C:\";
                cmdProcess.OutputDataReceived += new DataReceivedEventHandler((s, e) => 
                {
                    //If don't have data, cancel
                    if (String.IsNullOrEmpty(e.Data) == true)
                        return;

                    //Get the string output for this line
                    string currentLineOutput = e.Data;
                    //If the first character of this line is a ">", symbol of the last command input, remove it
                    if (currentLineOutput[0] == '>')
                        currentLineOutput = currentLineOutput.Remove(0, 1);

                    //Add this new line to list
                    caspsEditorReceivedOutputLines.Add(currentLineOutput);
                });

                //Start the process, enable output reading and store a reference for the process
                cmdProcess.Start();
                cmdProcess.BeginOutputReadLine();
                caspsEditorCliProcess = cmdProcess;

                //Wait time
                threadTools.MakeThreadSleep(500);

                //Open the Dl3CASPsEditor tool in CLI
                caspsEditorCliProcess.StandardInput.WriteLine("\"" + (contentsPath + "/tool-caspe/Dl3CASPsEditor.exe") + "\" dl3");

                //Wait time
                threadTools.MakeThreadSleep(250);

                //Finish the thread...
                return new string[] { "none" };
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //Start a coroutine to open the package and get the CASPs list
                IDisposable routine = Coroutine.Start(OpenPackageAndGetListOfCASPs());
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        private IEnumerator OpenPackageAndGetListOfCASPs()
        {
            //Wait time
            yield return new WaitForSeconds(0.2f);

            //Prepare to listen the response of command
            caspsEditorReceivedOutputLines.Clear();
            //Send "open" command to open this package
            caspsEditorCliProcess.StandardInput.WriteLine("open path:\"" + (packagePath) + "\"");
            //Wait for the "<" that indicates the last command was finished
            while (caspsEditorReceivedOutputLines.Count == 0 || (caspsEditorReceivedOutputLines.Last()[0]) != '<')
                yield return new WaitForSeconds(0.1f);

            //Prepare to listen the response of command
            caspsEditorReceivedOutputLines.Clear();
            //Send "list" command
            caspsEditorCliProcess.StandardInput.WriteLine("list cachepath:\"" + (caspEditorCachePath) + "\"");
            //Wait for the "<" that indicates the last command was finished
            while (caspsEditorReceivedOutputLines.Count == 0 || (caspsEditorReceivedOutputLines.Last()[0]) != '<')
                yield return new WaitForSeconds(0.1f);

            //Get the list of processed resources info
            List<Dictionary<string, string>> processedResources = GetProcessedListOfResources(caspsEditorReceivedOutputLines.ToArray());

            //Wait time
            yield return new WaitForSeconds(0.2f);

            //Render each CASP resource found inside the package
            foreach(Dictionary<string, string> resource in processedResources)
            {
                //Get all data
                string instanceStr = resource["instanceStr"];
                ulong instanceLong = ulong.Parse(resource["instanceUlong"]);
                bool forToddler = bool.Parse(resource["forToddler"]);
                bool forChild = bool.Parse(resource["forChild"]);
                bool forTeen = bool.Parse(resource["forTeen"]);
                bool forYoungAdult = bool.Parse(resource["forYoungAdult"]);
                bool forAdult = bool.Parse(resource["forAdult"]);
                bool forElder = bool.Parse(resource["forElder"]);
                bool forMale = bool.Parse(resource["forMale"]);
                bool forFemale = bool.Parse(resource["forFemale"]);
                bool validForRandom = bool.Parse(resource["validForRandom"]);
                bool validForMaternity = bool.Parse(resource["validForMaternity"]);

                //Draw the item on screen
                CaspItem newItem = new CaspItem(mainWindow, instanceStr, instanceLong, validForRandom, validForMaternity);
                contentList.Children.Add(newItem);
                instantiatedCaspItems.Add(newItem);

                //Configure it
                newItem.HorizontalAlignment = HorizontalAlignment.Left;
                newItem.VerticalAlignment = VerticalAlignment.Top;
                newItem.Width = double.NaN;
                newItem.Height = double.NaN;
                newItem.Margin = new Thickness(4, 4, 4, 4);

                //Inform the data about the mod
                newItem.SetCaspEditorCachePath(caspEditorCachePath);
                newItem.SetGender(forMale, forFemale);
                newItem.SetAges(forToddler, forChild, forTeen, forYoungAdult, forAdult, forElder);
                newItem.Prepare();

                //Wait before render next to avoid UI freezing
                yield return new WaitForSeconds(0.02f);
            }

            //Start a loop to update each instantiated CASP thumbnail item
            foreach (CaspItem item in instantiatedCaspItems)
            {
                //Send call to update
                item.UpdateThumb();

                //Wait before render next to avoid UI freezing
                yield return new WaitForSeconds(0.02f);
            }

            //Show the warning or hide
            if (instantiatedCaspItems.Count == 0)
                emptyWarn.Visibility = Visibility.Visible;
            if (instantiatedCaspItems.Count >= 1)
                emptyWarn.Visibility = Visibility.Collapsed;
            //If have content, enable the save button
            if (instantiatedCaspItems.Count >= 1)
                saveButton.IsEnabled = true;

            //Show the content
            loadingGif.Visibility = Visibility.Collapsed;
            contentScroll.Visibility = Visibility.Visible;

            //Inform that is done the commands
            isCommandInProgress = false;
        }

        private void Save()
        {
            //Start a coroutine to save the changes
            IDisposable routine = Coroutine.Start(SaveChangesAndClose());
        }

        private IEnumerator SaveChangesAndClose()
        {
            //Inform that is processing commands
            isCommandInProgress = true;

            //Change the title
            this.Title = (this.Title + " - Saving changes...");

            //Disable the interactions with casps
            foreach(CaspItem item in instantiatedCaspItems)
            {
                item.Opacity = 0.5f;
                item.IsHitTestVisible = false;
            }

            //Disable the save button and show spinner
            saveButton.IsEnabled = false;
            saveButton.Visibility = Visibility.Collapsed;
            savingGif.Visibility = Visibility.Visible;

            //Wait time
            yield return new WaitForSeconds(0.5f);

            //Update each modified CASP item
            foreach(CaspItem item in instantiatedCaspItems)
            {
                //If this CASP is not modified, ignore it
                if (item.wasChanged == false)
                    continue;

                //Get the modified parameters
                string validForRandom = ((item.validForRandomCBx.IsChecked == true) ? "true" : "false");

                //Prepare to listen the response of command
                caspsEditorReceivedOutputLines.Clear();
                //Send "update" command
                caspsEditorCliProcess.StandardInput.WriteLine("update instance:\"" + item.instanceStr + "\" validforrandom:\"" + validForRandom + "\"");
                //Wait for the "<" that indicates the last command was finished
                while (caspsEditorReceivedOutputLines.Count == 0 || (caspsEditorReceivedOutputLines.Last()[0]) != '<')
                    yield return new WaitForSeconds(0.1f);

                //Wait before render next to avoid UI freezing
                yield return new WaitForSeconds(0.02f);
            }

            //Wait time
            yield return new WaitForSeconds(0.5f);

            //Prepare to listen the response of command
            caspsEditorReceivedOutputLines.Clear();
            //Send "save" command
            caspsEditorCliProcess.StandardInput.WriteLine("save");
            //Wait for the "<" that indicates the last command was finished
            while (caspsEditorReceivedOutputLines.Count == 0 || (caspsEditorReceivedOutputLines.Last()[0]) != '<')
                yield return new WaitForSeconds(0.1f);

            //Wait time
            yield return new WaitForSeconds(0.5f);

            //Prepare to listen the response of command
            caspsEditorReceivedOutputLines.Clear();
            //Send "close" command
            caspsEditorCliProcess.StandardInput.WriteLine("close");
            //Wait for the "<" that indicates the last command was finished
            while (caspsEditorReceivedOutputLines.Count == 0 || (caspsEditorReceivedOutputLines.Last()[0]) != '<')
                yield return new WaitForSeconds(0.1f);

            //Wait time
            yield return new WaitForSeconds(0.5f);

            //Inform that is not processing commands
            isCommandInProgress = false;

            //Stop the Dl3CASPsEditor process
            caspsEditorCliProcess.StandardInput.WriteLine("pexit");
            //Kill the CMD process
            caspsEditorCliProcess.Kill();
            caspsEditorCliProcess = null;

            //Send the notification
            mainWindow.ShowToast(mainWindow.GetStringApplicationResource("launcher_mods_installedTab_caspe_saved"), MainWindow.ToastType.Success);

            //Close this window
            this.Close();
        }

        //Auxiliar methods

        private List<Dictionary<string, string>> GetProcessedListOfResources(string[] listOfResources)
        {
            //Prepare the list to return
            List<Dictionary<string, string>> toReturn = new List<Dictionary<string, string>>();

            //Proccess each resource
            foreach(string resourceLine in listOfResources)
            {
                //If the first char of this line is a "<", ignores it
                if (resourceLine[0] == '<')
                    continue;

                //Create a dictionary for this resource
                Dictionary<string, string> resourceDictionary = new Dictionary<string, string>();
                //Split all parts of this resource
                string[] allParts = resourceLine.Split(',');
                
                //Analyse all parts of this resource
                foreach(string part in allParts)
                {
                    //If this is a empty string, ignore
                    if (part == "")
                        continue;

                    //Split this part between key and value
                    string[] keyAndValue = part.Split(':');

                    //If not have a key and value, ignore it
                    if (keyAndValue.Length != 2)
                        continue;

                    //Add to the dictionary
                    if (resourceDictionary.ContainsKey(keyAndValue[0]) == false)
                        resourceDictionary.Add(keyAndValue[0], keyAndValue[1]);
                }

                //Add the dictionary to list
                toReturn.Add(resourceDictionary);
            }

            //Return the response
            return toReturn;
        }
    }
}