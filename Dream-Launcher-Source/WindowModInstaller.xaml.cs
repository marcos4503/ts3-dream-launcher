using CoroutinesDotNet;
using CoroutinesForWpf;
using Ionic.Zip;
using MarcosTomaz.ATS;
using s3pi;
using s3pi.Interfaces;
using s3pi.Package;
using System;
using System.Collections;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TS3_Dream_Launcher.Controls.ListItems;

namespace TS3_Dream_Launcher
{
    /*
    * This is the code responsible by the Mod Installer window 
    */

    public partial class WindowModInstaller : Window
    {
        //Public enums
        public enum InstallType
        {
            Recommended,
            Custom
        }

        //Cache variables
        private bool isInstallInProgress = false;
        private string packageFilePathToBeInstalled = "";

        //Private variables
        private IDictionary<string, Storyboard> animStoryboards = new Dictionary<string, Storyboard>();
        private MainWindow mainWindowRef = null;
        private InstallType installType = InstallType.Recommended;
        private string modSourceUri = "";
        private string targetFolder = "";
        private string targetName = "";

        //Public variables
        public bool isModInstalledSuccessfully = false;

        //Core methods

        public WindowModInstaller(MainWindow mainWindow, InstallType installType, string modSourceUri, string targetFolder, string targetName)
        {
            //Prepare the window
            InitializeComponent();

            //Store reference to main window
            this.mainWindowRef = mainWindow;

            //Store the data
            this.installType = installType;
            this.modSourceUri = modSourceUri;
            this.targetFolder = targetFolder;
            this.targetName = targetName;

            //Load all animations
            LoadAllStoryboardsAnimationsReferences();

            //Prepare the UI
            PrepareTheUI();
        }

        private void LoadAllStoryboardsAnimationsReferences()
        {
            //Load references for all storyboards animations of this screen
            animStoryboards.Add("promptExit", (FindResource("promptExit") as Storyboard));
            animStoryboards.Add("installUiEnter", (FindResource("installUiEnter") as Storyboard));
        }

        private void PrepareTheUI()
        {
            //Block the window close if install is in progress
            this.Closing += (s, e) =>
            {
                if(isInstallInProgress == true)
                {
                    MessageBox.Show(mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_promptQuitErrorText"),
                                    mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_promptQuitErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                }
            };

            //Change to prompt screen
            confirmationPrompt.Visibility = Visibility.Visible;
            installUi.Visibility = Visibility.Collapsed;

            //Show the title
            this.Title = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_title");

            //Prepare the install prompt
            question.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_promptText");
            modInfo.Text = targetName.Split(" --- ")[1].Replace(".package", "");
            yes.Content = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_promptYes");
            no.Content = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_promptNo");

            //Prepare the buttons
            yes.Click += (s, e) =>
            {
                //Disable buttons
                yes.IsEnabled = false;
                no.IsEnabled = false;

                //Start the installation
                IDisposable routine = Coroutine.Start(StartInstallationRoutine());
            };
            no.Click += (s , e) => { this.Close(); };

            //Show the title
            title.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installTitle");
            if (installType == InstallType.Recommended)
                type.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installTypeR");
            if (installType == InstallType.Custom)
                type.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installTypeC");
            name.Text = targetName.Split(" --- ")[1].Replace(".package", "");

            //Prepare the interface for start
            step1Busy.Visibility = Visibility.Collapsed;
            step1Error.Visibility = Visibility.Collapsed;
            step1Success.Visibility = Visibility.Collapsed;
            step1Pending.Visibility = Visibility.Visible;
            step1Content.Visibility = Visibility.Collapsed;
            step1SubProgress.Visibility = Visibility.Collapsed;
            step1Title.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep1Title");
            step2Busy.Visibility = Visibility.Collapsed;
            step2Error.Visibility = Visibility.Collapsed;
            step2Success.Visibility = Visibility.Collapsed;
            step2Pending.Visibility = Visibility.Visible;
            step2Content.Visibility = Visibility.Collapsed;
            step2Title.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep2Title");
            step3Busy.Visibility = Visibility.Collapsed;
            step3Error.Visibility = Visibility.Collapsed;
            step3Success.Visibility = Visibility.Collapsed;
            step3Pending.Visibility = Visibility.Visible;
            step3Content.Visibility = Visibility.Collapsed;
            step3Title.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep3Title");
            step4Busy.Visibility = Visibility.Collapsed;
            step4Error.Visibility = Visibility.Collapsed;
            step4Success.Visibility = Visibility.Collapsed;
            step4Pending.Visibility = Visibility.Visible;
            step4Content.Visibility = Visibility.Collapsed;
            step4Title.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep4Title");

            //Hide the status
            errorReason.Visibility = Visibility.Collapsed;
            doneReason.Visibility = Visibility.Collapsed;
        }

        private IEnumerator StartInstallationRoutine()
        {
            //Exit from prompt
            animStoryboards["promptExit"].Begin();

            //Wait time
            yield return new WaitForSeconds(0.3);

            //Disable the prompt and enable install ui
            confirmationPrompt.Visibility = Visibility.Collapsed;
            installUi.Visibility = Visibility.Visible;

            //Start the animation
            animStoryboards["installUiEnter"].Begin();

            //Wait time
            yield return new WaitForSeconds(0.3);

            //Start the installation
            StartInstallation();
        }

        private void StartInstallation()
        {
            //Inform that the install is in progress
            isInstallInProgress = true;

            //Start step 1
            Step1_Start();
        }

        //Install methods

        private void Step1_Start()
        {
            //Change the UI
            step1Content.Visibility = Visibility.Visible;
            if (installType == InstallType.Recommended)
                step1ContentText.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep1TxtDownload");
            if (installType == InstallType.Custom)
                step1ContentText.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep1TxtLoad");
            step1Busy.Visibility = Visibility.Visible;
            step1Error.Visibility = Visibility.Collapsed;
            step1Success.Visibility = Visibility.Collapsed;
            step1Pending.Visibility = Visibility.Collapsed;

            //If is recommended...
            if (installType == InstallType.Recommended)
                Step1_DownloadModIfNecessary();

            //If is custom...
            if (installType == InstallType.Custom)
                Step1_LoadCustomMod();
        }

        private void Step1_DownloadModIfNecessary()
        {
            //Get the name to save the mod zip
            string[] uriParts = modSourceUri.Split(" | ")[0].Split("/");
            string targetDownloadZipName = uriParts[uriParts.Length - 1];

            //If the mod, already exists downloaded in cache, skip to extraction
            if(File.Exists((mainWindowRef.myDocumentsPath + "/!DL-TmpCache/" + targetDownloadZipName + ".ok")) == true)
            {
                Step1_ExtractDownloadedMod();
                return;
            }

            //If don't exists, start a thread to download it
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { modSourceUri, mainWindowRef.myDocumentsPath });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) => {  };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(250);

                //Get the start params data
                string modUrl = startParams[0];
                string documentsPath = startParams[1];

                //Try to do the task
                try
                {
                    //Get download URLs
                    string[] downloadUrls = modUrl.Split(" | ");

                    //Notify the download progress, if have more than one part
                    if (downloadUrls.Length > 1)
                        threadTools.ReportNewProgress("ui::progress::" + (0.0f).ToString());

                    //Download all needed files
                    for (int i = 0; i < downloadUrls.Length; i++)
                    {
                        //Get the name of the file to save as
                        string[] uriParts = downloadUrls[i].Split("/");
                        string fileNameToSave = uriParts[uriParts.Length - 1];

                        //Prepare the target download URL
                        string downloadUrl = downloadUrls[i];
                        string saveAsPath = (documentsPath + @"/!DL-TmpCache/" + fileNameToSave);
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

                        //Notify the download progress, if have more than one part
                        if (downloadUrls.Length > 1)
                            threadTools.ReportNewProgress("ui::progress::" + (((float) (i + 1) / (float) downloadUrls.Length) * 100.0f));

                        //Wait some time
                        threadTools.MakeThreadSleep(250);
                    }

                    //Create the OK file
                    string[] uriParts2 = downloadUrls[0].Split("/");
                    string firstFileName = uriParts2[uriParts2.Length - 1];
                    File.WriteAllText((documentsPath + "/!DL-TmpCache/" + firstFileName + ".ok"), "OK");

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
                //Get the message
                string tag = newProgress.Split("::")[0];
                string key = newProgress.Split("::")[1];
                string value = newProgress.Split("::")[2];

                //If is to update the progressbar
                if (tag == "ui" && key == "progress")
                {
                    step1SubProgress.Visibility = Visibility.Visible;
                    step1SubProgress.IsIndeterminate = false;
                    step1SubProgress.Value = float.Parse(value);
                }
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //Get the thread response
                string threadTaskResponse = backgroundResult[0];

                //Disable the progress bar of download if is success or fail
                step1SubProgress.Visibility = Visibility.Collapsed;

                //If have a response different from success, stop
                if (threadTaskResponse != "success")
                {
                    //Show the error
                    step1Content.Visibility = Visibility.Collapsed;
                    step1Busy.Visibility = Visibility.Collapsed;
                    step1Error.Visibility = Visibility.Visible;
                    step1Success.Visibility = Visibility.Collapsed;
                    step1Pending.Visibility = Visibility.Collapsed;
                    errorReasonTxt.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep1TxtDownloadError");
                    errorReason.Visibility = Visibility.Visible;
                    //Inform that the install is not in progress
                    isInstallInProgress = false;
                }

                //If have a response of success, continues
                if (threadTaskResponse == "success")
                    Step1_ExtractDownloadedMod();
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        private void Step1_ExtractDownloadedMod()
        {
            //Change the UI
            step1ContentText.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep1TxtExtract");

            //Create a thread to make the extraction
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { modSourceUri, mainWindowRef.myDocumentsPath, (Directory.GetCurrentDirectory() + @"/Content") });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) => { };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(250);

                //Get the start params data
                string modUrl = startParams[0];
                string documentsPath = startParams[1];
                string contentPath = startParams[2];

                //Try to do the task
                try
                {
                    //Delete the temporary extraction folder, if exists
                    if (Directory.Exists((documentsPath + "/!DL-TmpCache/mod-tmp-extract")) == true)
                        Directory.Delete((documentsPath + "/!DL-TmpCache/mod-tmp-extract"), true);
                    //Create the temporary directory
                    Directory.CreateDirectory((documentsPath + "/!DL-TmpCache/mod-tmp-extract"));

                    //Get the zip file name to extract
                    string[] uriParts = modUrl.Split(" | ")[0].Split("/");
                    string targetDownloadZipName = uriParts[uriParts.Length - 1];
                    string filePathToExtract = (documentsPath + "/!DL-TmpCache/" + targetDownloadZipName);

                    //Extract the file to a folder
                    if(System.IO.Path.GetExtension(filePathToExtract).ToLower().Replace(".", "") == "zip") //<-- If is a ZIP file
                    {
                        ZipFile zipFile = ZipFile.Read(filePathToExtract);
                        foreach (ZipEntry entry in zipFile)
                            entry.Extract((documentsPath + @"/!DL-TmpCache/mod-tmp-extract"), ExtractExistingFileAction.OverwriteSilently);
                        zipFile.Dispose();
                    }
                    if(System.IO.Path.GetExtension(filePathToExtract).ToLower().Replace(".", "") == "001") //<-- If is a 001 file
                    {
                        Process process = new Process();
                        process.StartInfo.FileName = System.IO.Path.Combine(contentPath, "tool-szip", "7z.exe");
                        process.StartInfo.WorkingDirectory = System.IO.Path.Combine(contentPath, "tool-szip");
                        process.StartInfo.Arguments = "x \""+ filePathToExtract + "\" -o\""+ (documentsPath + @"/!DL-TmpCache/mod-tmp-extract") + "\"";
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;  //<- Hide the process window
                        process.StartInfo.RedirectStandardOutput = true;
                        process.Start();
                        //Wait process finishes
                        process.WaitForExit();
                    }

                    //Prepare the target package file
                    string targetPackageFile = "";

                    //Find a GL global lang package file to be the target package file
                    foreach (FileInfo file in (new DirectoryInfo((documentsPath + @"/!DL-TmpCache/mod-tmp-extract")).GetFiles()))
                        if (file.Name.ToLower().Contains("gl_") == true)
                        {
                            targetPackageFile = file.FullName;
                            break;
                        }
                    //If the game is using "pt-BR" language, find a BR package file to be the target package file (if exists a BR package file)
                    if(mainWindowRef.launcherPrefs.loadedData.gameLang == "pt-BR")
                        foreach (FileInfo file in (new DirectoryInfo((documentsPath + @"/!DL-TmpCache/mod-tmp-extract")).GetFiles()))
                            if (file.Name.ToLower().Contains("br_") == true)
                            {
                                targetPackageFile = file.FullName;
                                break;
                            }

                    //Return a success response
                    return new string[] { "success", targetPackageFile };
                }
                catch (Exception ex)
                {
                    //Return a error response
                    return new string[] { "error", "" };
                }

                //Finish the thread...
                return new string[] { "none", "" };
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //Get the thread response
                string threadTaskResponse = backgroundResult[0];
                string targetPackageFile = backgroundResult[1];


                //If have a response different from success, stop
                if (threadTaskResponse != "success" || targetPackageFile == "")
                {
                    //Show the error
                    step1Content.Visibility = Visibility.Collapsed;
                    step1Busy.Visibility = Visibility.Collapsed;
                    step1Error.Visibility = Visibility.Visible;
                    step1Success.Visibility = Visibility.Collapsed;
                    step1Pending.Visibility = Visibility.Collapsed;
                    if (threadTaskResponse != "success")
                        errorReasonTxt.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep1TxtExtractError0");
                    if (threadTaskResponse == "success" && targetPackageFile == "")
                        errorReasonTxt.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep1TxtExtractError1");
                    errorReason.Visibility = Visibility.Visible;
                    //Inform that the install is not in progress
                    isInstallInProgress = false;
                }

                //If have a response of success, continues
                if (threadTaskResponse == "success" && targetPackageFile != "")
                {
                    packageFilePathToBeInstalled = targetPackageFile;
                    Step1_Finish();
                }
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        private void Step1_LoadCustomMod()
        {
            //Create a thread to do the copy
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { modSourceUri, mainWindowRef.myDocumentsPath });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) => { };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(250);

                //Get the start params data
                string originalModPath = startParams[0];
                string documentsPath = startParams[1];

                //Try to do the task
                try
                {
                    //Prepate the path to save the cache of the mod to be installed
                    string cacheModPath = (documentsPath + "/!DL-TmpCache/" + System.IO.Path.GetFileNameWithoutExtension(originalModPath) + ".package");

                    //If the file copy already exists in cache, delete it
                    if (File.Exists(cacheModPath) == true)
                        File.Delete(cacheModPath);

                    //Copy the original mod file to the cache
                    File.Copy(originalModPath, cacheModPath);

                    //Return a success response
                    return new string[] { "success", cacheModPath };
                }
                catch (Exception ex)
                {
                    //Return a error response
                    return new string[] { "error", "" };
                }

                //Finish the thread...
                return new string[] { "none", "" };
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //Get the thread response
                string threadTaskResponse = backgroundResult[0];
                string targetPackageFile = backgroundResult[1];


                //If have a response different from success, stop
                if (threadTaskResponse != "success" || targetPackageFile == "")
                {
                    //Show the error
                    step1Content.Visibility = Visibility.Collapsed;
                    step1Busy.Visibility = Visibility.Collapsed;
                    step1Error.Visibility = Visibility.Visible;
                    step1Success.Visibility = Visibility.Collapsed;
                    step1Pending.Visibility = Visibility.Collapsed;
                    errorReasonTxt.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep1TxtLoadError");
                    errorReason.Visibility = Visibility.Visible;
                    //Inform that the install is not in progress
                    isInstallInProgress = false;
                }

                //If have a response of success, continues
                if (threadTaskResponse == "success" && targetPackageFile != "")
                {
                    packageFilePathToBeInstalled = targetPackageFile;
                    Step1_Finish();
                }
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        private void Step1_Finish()
        {
            //Change the UI
            step1Busy.Visibility = Visibility.Collapsed;
            step1Error.Visibility = Visibility.Collapsed;
            step1Success.Visibility = Visibility.Visible;
            step1Pending.Visibility = Visibility.Collapsed;
            step1Content.Visibility = Visibility.Collapsed;

            //Start the step 2
            Step2_Start();
        }



        private void Step2_Start()
        {
            //Change the UI
            step2Content.Visibility = Visibility.Visible;
            step2ContentText.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep2TxtValidating").Replace("%pack%", System.IO.Path.GetFileName(packageFilePathToBeInstalled));
            step2Busy.Visibility = Visibility.Visible;
            step2Error.Visibility = Visibility.Collapsed;
            step2Success.Visibility = Visibility.Collapsed;
            step2Pending.Visibility = Visibility.Collapsed;

            //Start the checking
            Step2_ValidatePackage();
        }

        private void Step2_ValidatePackage()
        {
            //Create a thread to validate the package
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { packageFilePathToBeInstalled });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) => { };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(500);

                //Get the start params data
                string targetPackageFile = startParams[0];

                //Try to do the task
                try
                {
                    //Try to open the package file to check if is a valid The Sims 3 package file
                    IPackage package = Package.OpenPackage(0, targetPackageFile, false);
                    Package.ClosePackage(0, package);

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
                //Get the thread response
                string threadTaskResponse = backgroundResult[0];

                //If have a response different from success, stop
                if (threadTaskResponse != "success")
                {
                    //Show the error
                    step2Content.Visibility = Visibility.Collapsed;
                    step2Busy.Visibility = Visibility.Collapsed;
                    step2Error.Visibility = Visibility.Visible;
                    step2Success.Visibility = Visibility.Collapsed;
                    step2Pending.Visibility = Visibility.Collapsed;
                    errorReasonTxt.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep2TxtValidatingError");
                    errorReason.Visibility = Visibility.Visible;
                    //Inform that the install is not in progress
                    isInstallInProgress = false;
                }
                //If have a response of success, continues
                if (threadTaskResponse == "success")
                    Step2_Finish();
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        private void Step2_Finish()
        {
            //Change the UI
            step2Busy.Visibility = Visibility.Collapsed;
            step2Error.Visibility = Visibility.Collapsed;
            step2Success.Visibility = Visibility.Visible;
            step2Pending.Visibility = Visibility.Collapsed;
            step2Content.Visibility = Visibility.Collapsed;

            //Start the step 3
            Step3_Start();
        }



        private void Step3_Start()
        {
            //Change the UI
            step3Content.Visibility = Visibility.Visible;
            step3ContentText.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep3TitleS0");
            step3Busy.Visibility = Visibility.Visible;
            step3Error.Visibility = Visibility.Collapsed;
            step3Success.Visibility = Visibility.Collapsed;
            step3Pending.Visibility = Visibility.Collapsed;

            //Start the checking
            Step3_ConflictCheck();
        }

        private void Step3_ConflictCheck()
        {
            //Read this PDF to understand better mods conflicts (available in this project in "Libraries/Useful/Mod Conflicts.pdf")
            //https://simlogical.com/ContentUploadsRemote/uploads/1551/Understanding_Mod_CC_Conflicts_Delphy_Dashboard.pdf

            //Create a thread to validate the package
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { packageFilePathToBeInstalled, mainWindowRef.myDocumentsPath });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) => { };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(250);

                //Get the start params data
                string targetPackageFile = startParams[0];
                string documentsPath = startParams[1];

                //Try to do the task
                try
                {
                    //Inform that is building invetory
                    threadTools.ReportNewProgress("ui::status::inventory");

                    //Prepare the dictionary of resources of this mod to be installed
                    Dictionary<string, int> toInstallModResources = new Dictionary<string, int>();
                    //Open the package and store all resources keys in dictionary
                    IPackage package = Package.OpenPackage(0, targetPackageFile, false);
                    foreach(IResourceIndexEntry item in package.GetResourceList)
                    {
                        //Get the resource info
                        string typeHex = GetLongConvertedToHexStr(item.ResourceType, 8);
                        string groupHex = GetLongConvertedToHexStr(item.ResourceGroup, 8);
                        string instanceHex = GetLongConvertedToHexStr(item.Instance, 16);
                        //Build the TGI or Resource Key
                        string resourceKey = typeHex + "-" + groupHex + "-" + instanceHex;

                        //Add to dictionary of resources, if don't exists
                        if (toInstallModResources.ContainsKey(resourceKey) == false)
                            toInstallModResources.Add(resourceKey, -1);
                    }
                    //Close the package
                    Package.ClosePackage(0, package);

                    //Wait some time
                    threadTools.MakeThreadSleep(250);

                    //Prepare the conflict count
                    int conflictCount = 0;
                    //Inform that is analysing
                    threadTools.ReportNewProgress("ui::analyses::" + conflictCount);
                    threadTools.ReportNewProgress("ui::progress::" + 0.0f);

                    //Prepare the list of files to analyse
                    List<string> packageFilesToBeAnalyzed = new List<string>();
                    int filesAnalyzedCount = 0;
                    //Add all CUSTOM mods
                    foreach (FileInfo file in (new DirectoryInfo((documentsPath + "/Mods/Packages/DL3-Custom")).GetFiles()))
                        if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".package" || System.IO.Path.GetExtension(file.FullName).ToLower() == ".disabled")
                            packageFilesToBeAnalyzed.Add(file.FullName);
                    //Add all RECOMMENDED mods
                    foreach (FileInfo file in (new DirectoryInfo((documentsPath + "/Mods/Packages/DL3-Recommended")).GetFiles()))
                        if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".package" || System.IO.Path.GetExtension(file.FullName).ToLower() == ".disabled")
                            packageFilesToBeAnalyzed.Add(file.FullName);
                    //Add all PATCH mods
                    foreach (FileInfo file in (new DirectoryInfo((documentsPath + "/Mods/Packages/DL3-Patches")).GetFiles()))
                        if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".package" || System.IO.Path.GetExtension(file.FullName).ToLower() == ".disabled")
                            packageFilesToBeAnalyzed.Add(file.FullName);

                    //Start analysing each installed mod
                    foreach (string modFile in packageFilesToBeAnalyzed)
                    {
                        //Open the mod package
                        IPackage modPkg = Package.OpenPackage(0, modFile, false);
                        //Read all resources to check if have conflicts
                        foreach (IResourceIndexEntry item in modPkg.GetResourceList)
                        {
                            //Get the resource info
                            string typeHex = GetLongConvertedToHexStr(item.ResourceType, 8);
                            string groupHex = GetLongConvertedToHexStr(item.ResourceGroup, 8);
                            string instanceHex = GetLongConvertedToHexStr(item.Instance, 16);
                            //Build the TGI or Resource Key
                            string resourceKey = typeHex + "-" + groupHex + "-" + instanceHex;

                            //If is a "_KEY" resource, skip this resource
                            if (resourceKey == "0x0166038C-0x00000000-0x0000000000000000" || resourceKey == "0x0166038C-0x00000000-0x9A04BBD3B97097D5")
                                continue;
                            //If is a "_XML" resource of manifest, skip this resource
                            if (resourceKey == "0x73E93EEB-0x00000000-0x0000000000000000")
                                continue;
                            //If is a "_IMG" resource of empty overlay or dropshadow, skip this resource
                            if (resourceKey == "0x00B2D882-0x00000000-0x75F8F21E0F143CAC" || resourceKey == "0x00B2D882-0x00000000-0x8BBC4B03744AE422")
                                continue;
                            //If is a "COMP" resource, skip this resource
                            if (resourceKey == "0x044AE110-0x02000000-0x0000000000000000")
                                continue;

                            //If is a conflict...
                            if (toInstallModResources.ContainsKey(resourceKey) == true)
                            {
                                //Add the conflict to UI
                                threadTools.ReportNewProgress("list::add::" + System.IO.Path.GetFileNameWithoutExtension(modFile) + "₢₢₢" + instanceHex);

                                //Increase conflicts counter
                                conflictCount += 1;
                            }
                        }
                        //Close the mod package
                        Package.ClosePackage(0, package);

                        //Increase the analyzed files count
                        filesAnalyzedCount += 1;

                        //Update the progress
                        threadTools.ReportNewProgress("ui::analyses::" + conflictCount);
                        threadTools.ReportNewProgress("ui::progress::" + ((float) filesAnalyzedCount / (float)packageFilesToBeAnalyzed.Count) * 100.0f);

                        //Wait some time before go to next
                        threadTools.MakeThreadSleep(15);
                    }

                    //Inform the final result
                    threadTools.ReportNewProgress("ui::final::" + conflictCount);

                    //If don't have conflicts, inform success
                    if (conflictCount == 0)
                        return new string[] { "success" };

                    //Return a neutral response
                    return new string[] { "none" };
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
                //Get the message
                string tag = newProgress.Split("::")[0];
                string key = newProgress.Split("::")[1];
                string value = newProgress.Split("::")[2];

                //If is to inform that is building invetory
                if(tag == "ui" && key == "status" && value == "inventory")
                    step3ContentText.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep3TitleS1");

                //If is to inform that is analysing
                if (tag == "ui" && key == "analyses")
                    step3ContentText.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep3TitleS2").Replace("%n%", value);

                //If is to update the progressbar
                if (tag == "ui" && key == "progress")
                {
                    step3Progress.IsIndeterminate = false;
                    step3Progress.Value = float.Parse(value);
                }

                //If is desired to add a item to list
                if (tag == "list" && key == "add")
                {
                    //Get the mod name
                    string modName = value.Split("₢₢₢")[0];
                    string instanceHex = value.Split("₢₢₢")[1];

                    //Add the item to list
                    ConflictItem newItem = new ConflictItem(mainWindowRef, modName, instanceHex);
                    conflictsList.Children.Add(newItem);

                    //Configure it
                    newItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                    newItem.VerticalAlignment = VerticalAlignment.Top;
                    newItem.Width = double.NaN;
                    newItem.Height = double.NaN;
                    newItem.Margin = new Thickness(0, 0, 0, 4);

                    //Move the scrollview to down
                    conflictsScroll.ScrollToEnd();
                }

                //If is to inform that is analysing
                if (tag == "ui" && key == "final")
                    step3ContentText.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep3TitleS3").Replace("%n%", value);
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //Get the thread response
                string threadTaskResponse = backgroundResult[0];

                //If have a response different from success, stop
                if (threadTaskResponse != "success")
                {
                    //Show the error
                    step3Progress.Visibility = Visibility.Collapsed;
                    step3Busy.Visibility = Visibility.Collapsed;
                    step3Error.Visibility = Visibility.Visible;
                    step3Success.Visibility = Visibility.Collapsed;
                    step3Pending.Visibility = Visibility.Collapsed;
                    errorReasonTxt.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep3TxtError");
                    errorReason.Visibility = Visibility.Visible;
                    //Inform that the install is not in progress
                    isInstallInProgress = false;
                }
                //If have a response of success, continues
                if (threadTaskResponse == "success")
                    Step3_Finish();
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        private void Step3_Finish()
        {
            //Change the UI
            step3Busy.Visibility = Visibility.Collapsed;
            step3Error.Visibility = Visibility.Collapsed;
            step3Success.Visibility = Visibility.Visible;
            step3Pending.Visibility = Visibility.Collapsed;
            step3Content.Visibility = Visibility.Collapsed;

            //Start the step 4
            Step4_Start();
        }



        private void Step4_Start()
        {
            //Change the UI
            step4Content.Visibility = Visibility.Visible;
            step4ContentText.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep4Txt");
            step4Busy.Visibility = Visibility.Visible;
            step4Error.Visibility = Visibility.Collapsed;
            step4Success.Visibility = Visibility.Collapsed;
            step4Pending.Visibility = Visibility.Collapsed;

            //Start the install finishing
            Step4_FinishInstall();
        }

        private void Step4_FinishInstall()
        {
            //Create a thread to validate the package
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { packageFilePathToBeInstalled, targetFolder, targetName });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) => { };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(500);

                //Get the start params data
                string targetPackageFile = startParams[0];
                string saveInFolder = startParams[1];
                string saveName = startParams[2];

                //Try to do the task
                try
                {
                    //If the mod file already exists with PACKAGE extension, inform error
                    if(File.Exists(saveInFolder + "/" + saveName) == true)
                        return new string[] { "alreadyExists" };
                    //If the mod file already exists with DISABLED extension, inform error
                    if (File.Exists(saveInFolder + "/" + saveName.Replace(".package", ".disabled")) == true)
                        return new string[] { "alreadyExists" };

                    //Copy the mod to be installed and save it as the target name
                    File.Copy(targetPackageFile, (saveInFolder + "/" + saveName));

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
                //Get the thread response
                string threadTaskResponse = backgroundResult[0];

                //If have a response different from success, stop
                if (threadTaskResponse != "success")
                {
                    //Show the error
                    step4Content.Visibility = Visibility.Collapsed;
                    step4Busy.Visibility = Visibility.Collapsed;
                    step4Error.Visibility = Visibility.Visible;
                    step4Success.Visibility = Visibility.Collapsed;
                    step4Pending.Visibility = Visibility.Collapsed;
                    errorReasonTxt.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep4TxtError");
                    errorReason.Visibility = Visibility.Visible;
                    //Inform that the install is not in progress
                    isInstallInProgress = false;
                }
                //If have a response of success, continues
                if (threadTaskResponse == "success")
                    Step4_Finish();
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        private void Step4_Finish()
        {
            //Change the UI
            step4Busy.Visibility = Visibility.Collapsed;
            step4Error.Visibility = Visibility.Collapsed;
            step4Success.Visibility = Visibility.Visible;
            step4Pending.Visibility = Visibility.Collapsed;
            step4Content.Visibility = Visibility.Collapsed;

            //Show the info
            doneReasonTxt.Text = mainWindowRef.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installFinish");
            doneReason.Visibility = Visibility.Visible;

            //Inform that the mod was installed successfully
            isModInstalledSuccessfully = true;

            //Inform that install was finished
            isInstallInProgress = false;
        }

        //Auxiliar methods

        private string GetLongConvertedToHexStr(ulong originalValue, int digits)
        {
            //Prepare the result
            string result = "0x";

            //Convert long to hex string
            string tmpHex = originalValue.ToString("X");

            //Add the digits if necessary
            while (tmpHex.Length < digits)
                tmpHex = "0" + tmpHex;

            //Concat the values
            result += tmpHex;

            //Return the result
            return result;
        }
    }
}
