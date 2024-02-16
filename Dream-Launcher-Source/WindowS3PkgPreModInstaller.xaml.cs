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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TS3_Dream_Launcher.Controls.ListItems;

namespace TS3_Dream_Launcher
{
    /*
    * This is the code responsible by the Sims3Pack Pre Mod Installer window
    */

    public partial class WindowS3PkgPreModInstaller : Window
    {
        //Cache variables
        private bool isExtractionInProgress = false;
        private List<S3PkModItem> instantiatedModItems = new List<S3PkModItem>();
        private int instantiatedModItemBeingInstalled = -1;

        //Private variables
        private MainWindow mainWindowRef = null;
        private string s3pkgPathToOpen = "";
        private string targetFolder = "";
        private string targetCategory = "";

        //Core methods

        public WindowS3PkgPreModInstaller(MainWindow mainWindow, string s3pkgPathToOpen, string targetFolder, string targetCategory)
        {
            //Prepare the window
            InitializeComponent();

            //Store reference to main window
            this.mainWindowRef = mainWindow;

            //Store the data
            this.s3pkgPathToOpen = s3pkgPathToOpen;
            this.targetFolder = targetFolder;
            this.targetCategory = targetCategory;

            //Prepare the UI
            PrepareTheUI();
        }

        private void PrepareTheUI()
        {
            //Block the window close if install is in progress
            this.Closing += (s, e) =>
            {
                if (isExtractionInProgress == true)
                {
                    MessageBox.Show(mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_s3pPreModInstaller_promptQuitErrorText"),
                                    mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_s3pPreModInstaller_promptQuitErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                }
            };

            //Change to extracting screen
            extractionUi.Visibility = Visibility.Visible;
            packageList.Visibility = Visibility.Collapsed;

            //Show the title
            this.Title = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_s3pPreModInstaller_title");

            //Prepare the extraction UI
            extractTitle.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_s3pPreModInstaller_extracting");

            //Prepare the package list interface
            pkgListEmpty.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_s3pPreModInstaller_pkgListEmpty");
            pkgListEmpty.Visibility = Visibility.Collapsed;
            pkgListTip.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_s3pPreModInstaller_pkgListTip");

            //Extract and render each package found inside the sims3pack file
            ExtractSims3Pack();
        }

        private void ExtractSims3Pack()
        {
            //Inform that the extraction is in progress
            isExtractionInProgress = true;

            //Create a thread to make the extraction
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { s3pkgPathToOpen, mainWindowRef.myDocumentsPath, (Directory.GetCurrentDirectory() + @"/Content") });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) => { };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(1000);

                //Get the start params data
                string s3pkgPath = startParams[0];
                string documentsPath = startParams[1];
                string contentPath = startParams[2];

                //Try to do the task
                try
                {
                    //Delete the temporary extraction folder, if exists
                    if (Directory.Exists((documentsPath + "/!DL-TmpCache/mod-s3pkg-extract")) == true)
                        Directory.Delete((documentsPath + "/!DL-TmpCache/mod-s3pkg-extract"), true);
                    //Create the temporary directory
                    Directory.CreateDirectory((documentsPath + "/!DL-TmpCache/mod-s3pkg-extract"));

                    //Prepare the path of the target sims3pack inside the cache
                    string s3pkgCachePath = (documentsPath + "/!DL-TmpCache/mod-s3pkg-extract/" + System.IO.Path.GetFileNameWithoutExtension(s3pkgPath) + ".sims3pack");

                    //If the sims3pack cache file already exists inside cache remove it
                    if (File.Exists(s3pkgCachePath) == true)
                        File.Delete(s3pkgCachePath);

                    //Copy the sims3pack file to cache
                    File.Copy(s3pkgPath, s3pkgCachePath);

                    //Extract it to the temporary extraction folder
                    Process process = new Process();
                    process.StartInfo.FileName = System.IO.Path.Combine(contentPath, "tool-s3ce", "s3ce.exe");
                    process.StartInfo.WorkingDirectory = System.IO.Path.Combine(contentPath, "tool-s3ce");
                    process.StartInfo.Arguments = "\"" + s3pkgCachePath + "\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;  //<- Hide the process window
                    process.StartInfo.RedirectStandardOutput = true;
                    process.Start();
                    //Wait process finishes
                    process.WaitForExit();

                    //Wait some time
                    threadTools.MakeThreadSleep(500);

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
                //Render each package found inside sims3pack
                RenderEachPackageFound();
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        private void RenderEachPackageFound()
        {
            //Inform that extraction is done
            isExtractionInProgress = false;

            //Change to package list
            extractionUi.Visibility = Visibility.Collapsed;
            packageList.Visibility = Visibility.Visible;

            //Build the list of package files found inside the extracted sims3pack file
            List<string> packageFilesExtracted = new List<string>();
            foreach (FileInfo file in (new DirectoryInfo((mainWindowRef.myDocumentsPath + "/!DL-TmpCache/mod-s3pkg-extract")).GetFiles()))
                if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".package")
                    packageFilesExtracted.Add(file.FullName);

            //Render each file found inside the sims3pack file
            for(int i = 0; i < packageFilesExtracted.Count; i++)
            {
                //Create the item to display
                S3PkModItem newItem = new S3PkModItem();
                pkgList.Children.Add(newItem);
                instantiatedModItems.Add(newItem);

                //Configure it
                newItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newItem.VerticalAlignment = VerticalAlignment.Top;
                newItem.Width = double.NaN;
                newItem.Height = double.NaN;
                newItem.Margin = new Thickness(0, 0, 0, 4);

                //Inform the data about the mod
                newItem.SetPackageID(i);
                newItem.SetPackagePath(packageFilesExtracted[i]);
                newItem.SetButtonText(mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_s3pPreModInstaller_install"));
                newItem.RegisterOnClickInstall((sender) =>
                {
                    //Open the window of mod installer
                    WindowModInstaller modInstaller = new WindowModInstaller(mainWindowRef, WindowModInstaller.InstallType.Custom, sender.thisPackagePath, targetFolder,
                                                                        (targetCategory + " --- " + System.IO.Path.GetFileNameWithoutExtension(sender.thisPackagePath) + ".package"));
                    modInstaller.Closed += (s, e) =>
                    {
                        //If the mod was installed successfully, disable the install button
                        if (modInstaller.isModInstalledSuccessfully == true)
                        {
                            instantiatedModItems[instantiatedModItemBeingInstalled].installButton.IsEnabled = false;
                            instantiatedModItems[instantiatedModItemBeingInstalled].SetButtonText(mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_s3pPreModInstaller_installed"));
                        }

                        //Clear the information of mod item being installed
                        instantiatedModItemBeingInstalled = -1;

                        //Show this window again
                        this.Visibility = Visibility.Visible;
                    };
                    modInstaller.Show();

                    //Inform id of mod item being installed
                    instantiatedModItemBeingInstalled = sender.thisItemId;

                    //Hide this window
                    this.Visibility = Visibility.Collapsed;
                });
                newItem.Prepare();
            }

            //If don't have package files, show the warning of empty and stop here
            if(packageFilesExtracted.Count == 0)
                pkgListEmpty.Visibility = Visibility.Visible;
        }
    }
}
