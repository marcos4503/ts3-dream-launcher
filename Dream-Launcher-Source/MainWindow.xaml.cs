using CoroutinesDotNet;
using CoroutinesForWpf;
using Ionic.Zip;
using MarcosTomaz.ATS;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using TS3_Dream_Launcher.Controls.ListItems;
using TS3_Dream_Launcher.Scripts;
using NetFwTypeLib;
using System.Xml;

namespace TS3_Dream_Launcher
{
    public partial class MainWindow : Window
    {
        //Enums of script
        public enum ToastType
        {
            Error,
            Success
        }
        public enum LauncherPage
        {
            home,
            saves,
            exports,
            worlds,
            media,
            cache,
            patches,
            mods,
            tools,
            settings
        }
        private enum ExpansionPack
        {
            WorldAdventures,
            Ambitions,
            LateNight,
            Generations,
            Pets,
            Showtime,
            Supernatural,
            Seasons,
            UniversityLife,
            IslandParadise,
            IntoTheFuture
        }
        private enum StuffPack
        {
            HighEndLoft,
            FastLane,
            OutdoorLiving,
            TownLife,
            MasterSuite,
            KatyPerry,
            Diesel,
            y70y80y90,
            Movie
        }
        private enum PatchMode
        {
            CheckIntegrity,
            Install
        }
        private enum ModCategory
        {
            All,
            Contents,
            Graphics,
            Sounds,
            Fixes,
            Gameplay,
            Sliders,
            Others,
            FromPatches
        }
        private enum ModScreen
        {
            Recommended,
            Custom
        }

        //Private classes
        private class TipItemTextFormatted
        {
            public string title = "";
            public string text = "";
        }

        //Cache variables
        private double thisWindowLeftPosition = 0;
        private double thisWindowTopPosition = 0;
        private WindowIconViewer currentOpenedIconViewer = null;
        private bool isToastHistoryToggled = false;
        private int currentToastsInHistory = 0;
        private bool isPlayingTheGame = false;
        private bool[] availableExpansionPacks = new bool[12];
        private bool[] availableStuffPacks = new bool[10];
        private LauncherPage currentLauncherPageViewing = LauncherPage.home;
        private IDisposable logsViewerOpenRoutine = null;
        private IDisposable installedModsUpdateRoutine = null;
        private IDisposable installedModsFilterRoutine = null;
        private ModCategory currentSeeingModsCategory = ModCategory.All;
        private bool isDownloadingRecommendedLibrary = false;
        private IDisposable recommendedModsUpdateRoutine = null;
        private ModCategory currentSeeingRecModsCategory = ModCategory.All;
        private IDisposable recommendedModsFilterRoutine = null;
        private IDisposable mediaListUpdateRoutine = null;
        private IDisposable worldListUpdateRoutine = null;
        private IDisposable exportListUpdateRoutine = null;
        private IDisposable saveListUpdateRoutine = null;
        private IDisposable showVaultTaskRoutine = null;
        private IDisposable hideVaultTaskRoutine = null;
        private IDisposable showTipsSectionRoutine = null;
        private IDisposable hideTipsSectionRoutine = null;
        private bool isTipsSectionToggled = false;
        private bool wasRenderedAllPatches = false;

        //Private variables
        private IDictionary<string, Storyboard> animStoryboards = new Dictionary<string, Storyboard>();
        private System.Windows.Forms.NotifyIcon launcherTrayIcon = null;
        private IDictionary<string, string> runningTasks = new Dictionary<string, string>();
        private Process currentGameProcess = null;
        private Process currentGameOverlayProcess = null;

        //Public variables
        public string myDocumentsPath = "";
        public Preferences launcherPrefs = null;
        public List<PatchItem> instantiatedPatchItems = new List<PatchItem>();
        public List<CacheItem> instantiatedCacheItems = new List<CacheItem>();
        public List<LogItem> instantiatedLogItems = new List<LogItem>();
        public List<ToolItem> instantiatedToolItems = new List<ToolItem>();
        public List<InstalledModItem> instantiatedModsItems = new List<InstalledModItem>();
        public List<StoreModItem> instantiatedRecModItems = new List<StoreModItem>();
        public List<MediaItem> instantiatedMediaItems = new List<MediaItem>();
        public List<WorldItem> instantiatedWorldItems = new List<WorldItem>();
        public List<ExportItem> instantiatedExportItems = new List<ExportItem>();
        public List<SaveItem> instantiatedSaveItems = new List<SaveItem>();

        //Core methods

        public MainWindow()
        {
            //Check if have another process of the launcher already opened. If have, cancel this...
            string processName = Process.GetCurrentProcess().ProcessName;
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length > 1)
            {
                //Warn abou the problem
                MessageBox.Show("There is already an instance of \"The Sims 3 Dream Launcher\" running!", "Error");

                //Stop the execution of this instance
                System.Windows.Application.Current.Shutdown();

                //Cancel the execution
                return;
            }
            //Check if was started without admin rights. If yes, cancel this...
            if(isRunningWithAdminRights() == false)
            {
                //Warn abou the problem
                MessageBox.Show("Error when starting! The Dream Launcher needs to be runned with Admin rights. There are some patching operations and features of Dream Launcher that require these rights.", "Error");

                //Stop the execution of this instance
                System.Windows.Application.Current.Shutdown();

                //Cancel the execution
                return;
            }

            //Initialize the Window
            InitializeComponent();

            //Load the launcher preferences and get it
            launcherPrefs = new Preferences();

            //Inform the save informations and save it
            SaveInfo saveInfo1 = new SaveInfo();
            saveInfo1.key = "launcherVersion";
            saveInfo1.value = GetLauncherVersion();
            SaveInfo saveInfo2 = new SaveInfo();
            saveInfo2.key = "saveVersion";
            saveInfo2.value = "1.0.0";
            launcherPrefs.loadedData.saveInfo = new SaveInfo[] { saveInfo1, saveInfo2 };
            launcherPrefs.Save();

            //Load all storyboards animations references
            LoadAllStoryboardsAnimationsReferences();

            //If was not defined a language yet
            if (launcherPrefs.loadedData.launcherLang == "undefined")
                PrepareAndShowScreen1_Language();
            //If was already defined a language
            if (launcherPrefs.loadedData.launcherLang != "undefined")
                PrepareAndShowScreen2_Intro();

            //Override the closing task of the window
            this.Closing += (s, e) => { e.Cancel = true; CheckToExitFromLauncherAndWarnIfHaveTasksRunning(); };

            //Prepare the tray icon
            launcherTrayIcon = new System.Windows.Forms.NotifyIcon();
            launcherTrayIcon.Visible = true;
            launcherTrayIcon.Text = "The Sims 3\nDream Launcher";
            launcherTrayIcon.MouseClick += (s, e) => { BringTheWindowToFront(); };
            UpdateLauncherSystemTray();

            //Prepare the toast dismiss button
            toastDismiss.Click += (s, e) => { IDisposable dismissCoroutine = Coroutine.Start(WaitAndDismissTheToast()); };
        }

        private void LoadAllStoryboardsAnimationsReferences()
        {
            //Load references for all storyboards animations of this screen
            animStoryboards.Add("screen1FadeOut", (FindResource("screen1FadeOut") as Storyboard));
            animStoryboards.Add("screen3FadeIn", (FindResource("screen3FadeIn") as Storyboard));
            animStoryboards.Add("screen3FadeOut", (FindResource("screen3FadeOut") as Storyboard));
            animStoryboards.Add("screen4FadeIn", (FindResource("screen4FadeIn") as Storyboard));
            animStoryboards.Add("screen4FadeOut", (FindResource("screen4FadeOut") as Storyboard));
            animStoryboards.Add("screen5FadeIn", (FindResource("screen5FadeIn") as Storyboard));
            animStoryboards.Add("screen5FadeOut", (FindResource("screen5FadeOut") as Storyboard));
            animStoryboards.Add("screen6FadeIn", (FindResource("screen6FadeIn") as Storyboard));
            animStoryboards.Add("toastEnter", (FindResource("toastEnter") as Storyboard));
            animStoryboards.Add("toastExit", (FindResource("toastExit") as Storyboard));
            animStoryboards.Add("toastHistoryEnter", (FindResource("toastHistoryEnter") as Storyboard));
            animStoryboards.Add("toastHistoryExit", (FindResource("toastHistoryExit") as Storyboard));
            animStoryboards.Add("logsViewerEntry", (FindResource("logsViewerEntry") as Storyboard));
            animStoryboards.Add("logsViewerExit", (FindResource("logsViewerExit") as Storyboard));
            animStoryboards.Add("installedModsLoadExit", (FindResource("installedModsLoadExit") as Storyboard));
            animStoryboards.Add("vaultTaskEnter", (FindResource("vaultTaskEnter") as Storyboard));
            animStoryboards.Add("vaultTaskExit", (FindResource("vaultTaskExit") as Storyboard));
            animStoryboards.Add("tipsEnter", (FindResource("tipsEnter") as Storyboard));
            animStoryboards.Add("tipsExit", (FindResource("tipsExit") as Storyboard));
        }

        private void PrepareAndShowScreen1_Language()
        {
            //Disable others screens and enable this
            s1_language.Visibility = Visibility.Visible;
            s2_intro.Visibility = Visibility.Collapsed;
            s3_checks.Visibility = Visibility.Collapsed;
            s4_translate.Visibility = Visibility.Collapsed;
            s5_intelFix.Visibility = Visibility.Collapsed;
            s6_launcher.Visibility = Visibility.Collapsed;

            //Prepare the screen
            langCheckEnUs.Visibility = Visibility.Collapsed;
            langCheckPtBR.Visibility = Visibility.Collapsed;
            langSave.IsEnabled = false;

            //Prepare the callback for buttons
            langSelectEnUs.Click += (s, e) => {
                langSelectTitle.Content = "Please, select a language for the Launcher!";
                langSave.Content = "SAVE";
                langCheckEnUs.Visibility = Visibility.Visible;
                langCheckPtBR.Visibility = Visibility.Collapsed;
                langSave.IsEnabled = true;
            };
            langSelectPtBr.Click += (s, e) => {
                langSelectTitle.Content = "Por favor, selecione um idioma para o Launcher!";
                langSave.Content = "SALVAR";
                langCheckEnUs.Visibility = Visibility.Collapsed;
                langCheckPtBR.Visibility = Visibility.Visible;
                langSave.IsEnabled = true;
            };

            //Prepare the save button
            langSave.Click += (s, e) =>
            {
                langSave.IsEnabled = false;
                langSave.Visibility = Visibility.Hidden;
                animStoryboards["screen1FadeOut"].Begin();
                IDisposable exitCoroutine = Coroutine.Start(PrepareAndShowScreen1_Language_Exit());

                //Change the language and save
                if (langCheckEnUs.Visibility == Visibility.Visible)
                    launcherPrefs.loadedData.launcherLang = "en-us";
                if (langCheckPtBR.Visibility == Visibility.Visible)
                    launcherPrefs.loadedData.launcherLang = "pt-br";
                launcherPrefs.Save();
            };
        }

        private IEnumerator PrepareAndShowScreen1_Language_Exit()
        {
            //Wait some time..
            yield return new WaitForSeconds(1.0f);

            //Call the next screen
            PrepareAndShowScreen2_Intro();
        }

        private void PrepareAndShowScreen2_Intro()
        {
            //Disable others screens and enable this
            s1_language.Visibility = Visibility.Collapsed;
            s2_intro.Visibility = Visibility.Visible;
            s3_checks.Visibility = Visibility.Collapsed;
            s4_translate.Visibility = Visibility.Collapsed;
            s5_intelFix.Visibility = Visibility.Collapsed;
            s6_launcher.Visibility = Visibility.Collapsed;

            //Load the correct language
            ResourceDictionary resourceDictionary = new ResourceDictionary();
            switch (launcherPrefs.loadedData.launcherLang)
            {
                case "en-us":
                    resourceDictionary.Source = new Uri("..\\Resources\\Languages\\LangStrings.xaml", UriKind.Relative);
                    break;
                case "pt-br":
                    resourceDictionary.Source = new Uri("..\\Resources\\Languages\\LangStrings-PT-BR.xaml", UriKind.Relative);
                    break;
            }
            this.Resources.MergedDictionaries.Add(resourceDictionary);

            //Play the EA The Sims 3 intro
            introVideoPlayer.Source = new Uri(@"Content/intro.wmv", UriKind.Relative);
            introVideoPlayer.Play();

            //Start the coroutine of exit
            IDisposable exitCoroutine = Coroutine.Start(PrepareAndShowScreen2_Intro_Exit());
        }

        private IEnumerator PrepareAndShowScreen2_Intro_Exit()
        {
            //Detect the time to wait
            float waitSeconds = 5.0f;
#if DEBUG
            waitSeconds = 0.8f;
            introVideoPlayer.IsMuted = true;
#endif

            //Wait some time..
            yield return new WaitForSeconds(waitSeconds);

            //Fully stop the video
            introVideoPlayer.Stop();

            //Call the next screen
            PrepareAndShowScreen3_Checks();
        }

        private void PrepareAndShowScreen3_Checks()
        {
            //Disable others screens and enable this
            s1_language.Visibility = Visibility.Collapsed;
            s2_intro.Visibility = Visibility.Collapsed;
            s3_checks.Visibility = Visibility.Visible;
            s4_translate.Visibility = Visibility.Collapsed;
            s5_intelFix.Visibility = Visibility.Collapsed;
            s6_launcher.Visibility = Visibility.Collapsed;

            //Play the fade-in animation
            animStoryboards["screen3FadeIn"].Begin();

            //Start the thread to check requeriments
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { });
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(750);

                //Prepare the value to return
                List<string> toReturn = new List<string>();

                //Do the check #1...
                bool ts3Exists = File.Exists((Directory.GetCurrentDirectory() + @"/TS3.exe"));
                bool ts3WExists = File.Exists((Directory.GetCurrentDirectory() + @"/TS3W.exe"));
                bool sims3launcherExists = File.Exists((Directory.GetCurrentDirectory() + @"/Sims3Launcher.exe"));
                bool sims3launcherWExists = File.Exists((Directory.GetCurrentDirectory() + @"/Sims3LauncherW.exe"));
                bool s3launcherExists = File.Exists((Directory.GetCurrentDirectory() + @"/S3Launcher.exe"));
                if (ts3Exists == true && ts3WExists == true && sims3launcherExists == true && sims3launcherWExists == true && s3launcherExists == true)
                    toReturn.Add("check1-success");
                else
                    toReturn.Add("check1-error");

                //Do the check #2...
                string fourthFoldersUpFolderName = "";
                try { fourthFoldersUpFolderName = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.Name; }
                catch (Exception ex) { }
                if(fourthFoldersUpFolderName == "steamapps")
                    toReturn.Add("check2-success");
                else
                    toReturn.Add("check2-error");

                //Do the check #3...
                if(File.Exists((Directory.GetCurrentDirectory() + @"/skuversion.txt")) == true)
                {
                    //Read all lines of file
                    string[] skuLines = File.ReadAllLines((Directory.GetCurrentDirectory() + @"/skuversion.txt"));

                    //Search by "GameVersion" in all lines
                    foreach(string line in skuLines)
                        if(line.Contains("GameVersion") == true)
                        {
                            //Get the game version
                            string filtredVersion = line.Replace(" ", "").Replace("GameVersion=", "");

                            //Check if is the supported version
                            if(filtredVersion == "1.67.2.024037")
                                toReturn.Add("check3-success");
                            else
                                toReturn.Add("check3-error");

                            //Break the loop
                            break;
                        }
                }
                else
                    toReturn.Add("check3-error");

                //Wait some time
                threadTools.MakeThreadSleep(350);

                //Return the result of this background code...
                return toReturn.ToArray();
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //Prepare the "Close" button
                checkClose.Click += (s, e) => { System.Windows.Application.Current.Shutdown(); };

                //See the result of Check #1...
                if (backgroundResult[0].Equals("check1-success") == false)
                {
                    //Show the error
                    checking.Visibility = Visibility.Collapsed;
                    checkError.Visibility = Visibility.Visible;
                    checkErrorReason.Text = GetStringApplicationResource("screen3_checkError1");

                    //Cancel the execution
                    return;
                }
                //See the result of Check #2...
                if (backgroundResult[1].Equals("check2-success") == false)
                {
                    //Show the error
                    checking.Visibility = Visibility.Collapsed;
                    checkError.Visibility = Visibility.Visible;
                    checkErrorReason.Text = GetStringApplicationResource("screen3_checkError2");
                    //Cancel the execution
                    return;
                }
                //See the result of Check #3...
                if (backgroundResult[2].Equals("check3-success") == false)
                {
                    //Show the error
                    checking.Visibility = Visibility.Collapsed;
                    checkError.Visibility = Visibility.Visible;
                    checkErrorReason.Text = GetStringApplicationResource("screen3_checkError3");

                    //Cancel the execution
                    return;
                }

                //Play the fade-out animation
                animStoryboards["screen3FadeOut"].Begin();

                //Start the coroutine of exit
                IDisposable exitCoroutine = Coroutine.Start(PrepareAndShowScreen3_Checks_Exit());
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        private IEnumerator PrepareAndShowScreen3_Checks_Exit()
        {
            //Wait some time..
            yield return new WaitForSeconds(0.5f);

            //Call the next screen
            PrepareAndShowScreen4_Translate();
        }

        private void PrepareAndShowScreen4_Translate()
        {
            //If is already translated, skip this
            if (launcherPrefs.loadedData.alreadyTranslated == true)
            {
                PrepareAndShowScreen5_IntelFix();
                return;
            }

            //Disable others screens and enable this
            s1_language.Visibility = Visibility.Collapsed;
            s2_intro.Visibility = Visibility.Collapsed;
            s3_checks.Visibility = Visibility.Collapsed;
            s4_translate.Visibility = Visibility.Visible;
            s5_intelFix.Visibility = Visibility.Collapsed;
            s6_launcher.Visibility = Visibility.Collapsed;

            //Play the fade-in animation
            animStoryboards["screen4FadeIn"].Begin();

            //Prepare the UI
            noTranslate.Click += (s, e) => 
            {
                //Disable all buttons
                noTranslate.IsEnabled = false;
                neverTranslate.IsEnabled = false;
                yesTranslate.IsEnabled = false;

                //Play the fade-out animation
                animStoryboards["screen4FadeOut"].Begin();
                //Start the coroutine of exit
                IDisposable exitCoroutine = Coroutine.Start(PrepareAndShowScreen4_Translate_Exit());
            };
            neverTranslate.Click += (s, e) =>
            {
                //Disable all buttons
                noTranslate.IsEnabled = false;
                neverTranslate.IsEnabled = false;
                yesTranslate.IsEnabled = false;

                //Inform that is translated
                launcherPrefs.loadedData.alreadyTranslated = true;
                launcherPrefs.Save();

                //Play the fade-out animation
                animStoryboards["screen4FadeOut"].Begin();
                //Start the coroutine of exit
                IDisposable exitCoroutine = Coroutine.Start(PrepareAndShowScreen4_Translate_Exit());
            };
            yesTranslate.Click += (s, e) =>
            {
                //Disable all buttons
                noTranslate.IsEnabled = false;
                neverTranslate.IsEnabled = false;
                yesTranslate.IsEnabled = false;

                //Do pt-pt to pt-br translate patch
                DoPatch_PtPtToPtBrTranslate();

                //Play the fade-out animation
                animStoryboards["screen4FadeOut"].Begin();
                //Start the coroutine of exit
                IDisposable exitCoroutine = Coroutine.Start(PrepareAndShowScreen4_Translate_Exit());
            };
        }

        private IEnumerator PrepareAndShowScreen4_Translate_Exit()
        {
            //Wait some time..
            yield return new WaitForSeconds(0.5f);

            //Call the next screen
            PrepareAndShowScreen5_IntelFix();
        }

        private void PrepareAndShowScreen5_IntelFix()
        {
            //Disable others screens and enable this
            s1_language.Visibility = Visibility.Collapsed;
            s2_intro.Visibility = Visibility.Collapsed;
            s3_checks.Visibility = Visibility.Collapsed;
            s4_translate.Visibility = Visibility.Collapsed;
            s5_intelFix.Visibility = Visibility.Visible;
            s6_launcher.Visibility = Visibility.Collapsed;

            //Play the fade-in animation
            animStoryboards["screen5FadeIn"].Begin();

            //Start the thread to handle the intel fix
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { launcherPrefs.loadedData.alreadyIntelFixed.ToString() });
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(1000);

                //Get the processor name
                string processorName = "";
                ManagementObjectSearcher mos = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
                foreach (ManagementObject mo in mos.Get())
                    processorName = mo["Name"].ToString();
                mos.Dispose();

                //If is already intel fixed, skip this
                if (startParams[0].ToLower() == "true")
                    return new string[] { "alreadyPatched" };

                //If is not a intel, skip this
                if(processorName.ToLower().Contains("intel") == false)
                    return new string[] { "notIntel" };

                //If is a intel alder lake or newer, request to patch
                for(int i = 2; i < 30; i++)
                    if(processorName.ToLower().Contains(("-1" + i)) == true)
                        return new string[] { "isAlderLake+", processorName };

                //Return empty response
                return new string[] { "notIntelAlderLake+" };
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //If is already patched, skip to next screen
                if (backgroundResult[0] == "alreadyPatched")
                {
                    //Play the fade-out animation
                    animStoryboards["screen5FadeOut"].Begin();
                    //Start the coroutine of exit
                    IDisposable exitCoroutine = Coroutine.Start(PrepareAndShowScreen5_IntelFix_Exit());
                    //Cancel
                    return;
                }

                //If is not a intel CPU, skip to next screen
                if (backgroundResult[0] == "notIntel")
                {
                    //Play the fade-out animation
                    animStoryboards["screen5FadeOut"].Begin();
                    //Start the coroutine of exit
                    IDisposable exitCoroutine = Coroutine.Start(PrepareAndShowScreen5_IntelFix_Exit());
                    //Cancel
                    return;
                }

                //If is a alder lake or newer CPU, show the dialog
                if (backgroundResult[0] == "isAlderLake+")
                {
                    //Show the dialog
                    intelLoading.Visibility = Visibility.Collapsed;
                    intelDialog.Visibility = Visibility.Visible;
                    //Prepare the UI
                    intelText.Text = GetStringApplicationResource("screen5_dialogText").Replace("%CPU%", backgroundResult[1]);

                    noIntelFix.Click += (s, e) => 
                    {
                        //Disable all buttons
                        noIntelFix.IsEnabled = false;
                        neverIntelFix.IsEnabled = false;
                        yesIntelFix.IsEnabled = false;

                        //Play the fade-out animation
                        animStoryboards["screen5FadeOut"].Begin();
                        //Start the coroutine of exit
                        IDisposable exitCoroutine = Coroutine.Start(PrepareAndShowScreen5_IntelFix_Exit());
                    };
                    neverIntelFix.Click += (s, e) =>
                    {
                        //Disable all buttons
                        noIntelFix.IsEnabled = false;
                        neverIntelFix.IsEnabled = false;
                        yesIntelFix.IsEnabled = false;

                        //Inform that is translated
                        launcherPrefs.loadedData.alreadyIntelFixed = true;
                        launcherPrefs.Save();

                        //Play the fade-out animation
                        animStoryboards["screen5FadeOut"].Begin();
                        //Start the coroutine of exit
                        IDisposable exitCoroutine = Coroutine.Start(PrepareAndShowScreen5_IntelFix_Exit());
                    };
                    yesIntelFix.Click += (s, e) =>
                    {
                        //Disable all buttons
                        noIntelFix.IsEnabled = false;
                        neverIntelFix.IsEnabled = false;
                        yesIntelFix.IsEnabled = false;

                        //Do the alder lake patch
                        DoPatch_AlderLakePatch();

                        //Play the fade-out animation
                        animStoryboards["screen5FadeOut"].Begin();
                        //Start the coroutine of exit
                        IDisposable exitCoroutine = Coroutine.Start(PrepareAndShowScreen5_IntelFix_Exit());
                    };
                    //Cancel
                    return;
                }

                //If is not a intel BIG.little architeture, skip to next screen
                if (backgroundResult[0] == "notIntelAlderLake+")
                {
                    //Play the fade-out animation
                    animStoryboards["screen5FadeOut"].Begin();
                    //Start the coroutine of exit
                    IDisposable exitCoroutine = Coroutine.Start(PrepareAndShowScreen5_IntelFix_Exit());
                    //Cancel
                    return;
                }
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        private IEnumerator PrepareAndShowScreen5_IntelFix_Exit()
        {
            //Wait some time..
            yield return new WaitForSeconds(1.0f);

            //Call the next screen
            PrepareAndShowScreen6_Launcher();
        }

        private void PrepareAndShowScreen6_Launcher()
        {
            //Disable others screens and enable this
            s1_language.Visibility = Visibility.Collapsed;
            s2_intro.Visibility = Visibility.Collapsed;
            s3_checks.Visibility = Visibility.Collapsed;
            s4_translate.Visibility = Visibility.Collapsed;
            s5_intelFix.Visibility = Visibility.Collapsed;
            s6_launcher.Visibility = Visibility.Visible;

            //Disable all red dots notifications
            DisableAllRedDotsNotifications();

            //Play the fade-in animation
            animStoryboards["screen6FadeIn"].Begin();

            //Show the launcher version in header
            this.Title = (this.Title + " - " + GetLauncherVersion());

            //Show the game version
            gameVersion.Content = "-";
            string[] skuLines = File.ReadAllLines((Directory.GetCurrentDirectory() + @"/skuversion.txt"));
            foreach (string line in skuLines)
                if (line.Contains("GameVersion") == true)
                    gameVersion.Content = ("Game Version: " + line.Replace(" ", "").Replace("GameVersion=", ""));

            //Show a random wallpaper
            ImageBrush wallpaperBrush = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/wallpaper-" + (new Random().Next(0, 10)) + ".png")));
            wallpaperBrush.Stretch = Stretch.UniformToFill;
            playWallpaper.Background = wallpaperBrush;

            //Prepare the play button
            playGame.Click += (s, e) => 
            {
                //Try to launch the game
                try
                {
                    //Get the correct working directory
                    string workingDirectory = Directory.GetCurrentDirectory();

                    //Create a new process of the game
                    Process newProcess = new Process();
                    newProcess.StartInfo.FileName = System.IO.Path.Combine(workingDirectory, "TS3W.exe");
                    newProcess.StartInfo.WorkingDirectory = workingDirectory;
                    newProcess.Start();
                    //Set the process cpu priority
                    if (launcherPrefs.loadedData.gamePriority == 0)
                        newProcess.PriorityClass = ProcessPriorityClass.Normal;
                    if (launcherPrefs.loadedData.gamePriority == 1)
                        newProcess.PriorityClass = ProcessPriorityClass.High;

                    //Store it
                    currentGameProcess = newProcess;

                    //Add a task of playing
                    AddTask("playing", "Playing the game.");
                    //Store the current window position
                    thisWindowLeftPosition = this.Left;
                    thisWindowTopPosition = this.Top;
                    //Hide this window (if is allowed)
                    if(launcherPrefs.loadedData.launcherBehaviour == 0)
                        this.Visibility = Visibility.Collapsed;
                    //Block the UI
                    BlockLauncherUiExceptHomePage(true);
                    //Inform that is playing
                    isPlayingTheGame = true;
                    //Update the tray icon
                    UpdateLauncherSystemTray();

                    //Create a thread to monitor the game process
                    AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { });
                    asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                    {
                        //Create a monitor loop
                        while (true)
                        {
                            //Wait some time
                            threadTools.MakeThreadSleep(3000);

                            //If was finished the game process, break the monitor loop
                            if (currentGameProcess.HasExited == true)
                                break;
                        }

                        //Return empty response
                        return new string[] { };
                    };
                    asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                    {
                        //Remove the task
                        RemoveTask("playing");
                        //Show this window again
                        this.Visibility = Visibility.Visible;
                        //Restore the current window position
                        this.Left = thisWindowLeftPosition;
                        this.Top = thisWindowTopPosition;
                        //Unlock the UI
                        BlockLauncherUiExceptHomePage(false);
                        //Inform that is not playing
                        isPlayingTheGame = false;
                        //Update the tray icon
                        UpdateLauncherSystemTray();

                        //If not known the my documents path, stops here
                        if (myDocumentsPath == "")
                            return;

                        //Post-game tasks...

                        //Recalculate all cache types sizes
                        RecalculateAllCacheTypesSizes();
                        //Re-update the medias list
                        UpdateMediaList();
                        //Re-update the exports list
                        UpdateExportList();
                        //Re-update the saves list
                        UpdateSaveList();
                    };
                    asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);

                    //If the overlay is enabled, create a thread to start the overlay
                    if (launcherPrefs.loadedData.gameOverlay != 0)
                        StartGameOverlayThread();
                }
                catch (Exception ex) { ShowToast((GetStringApplicationResource("launcher_launchGameProblem") + " \"" + ex.Message + "\""), ToastType.Error); }
            };

            //Prepare the toast history caller button
            toggleToastsHistory.Click += (s, e) =>
            {
                //If is not toggled
                if(isToastHistoryToggled == false)
                {
                    //Hide the counter
                    toastsCounterBg.Visibility = Visibility.Collapsed;
                    toastsCounterT.Visibility = Visibility.Collapsed;

                    //Open the toasts history
                    toastsHistory.Visibility = Visibility.Visible;
                    animStoryboards["toastHistoryEnter"].Begin();

                    //Inform that is toggled
                    isToastHistoryToggled = true;
                    return;
                }

                //If is toggled
                if (isToastHistoryToggled == true)
                {
                    //Close the toasts history
                    IDisposable dismissCoroutine = Coroutine.Start(WaitAndCloseToastHistory());

                    //Inform that is toggled
                    isToastHistoryToggled = false;
                    return;
                }
            };

            //Setup the navigation buttons
            goHome.Click += (s, e) => { SwitchPage(LauncherPage.home); };
            goSaves.Click += (s, e) => { SwitchPage(LauncherPage.saves); };
            goExports.Click += (s, e) => { SwitchPage(LauncherPage.exports); };
            goWorlds.Click += (s, e) => { SwitchPage(LauncherPage.worlds); };
            goMedia.Click += (s, e) => { SwitchPage(LauncherPage.media); };
            goCache.Click += (s, e) => { SwitchPage(LauncherPage.cache); };
            goPatches.Click += (s, e) => { SwitchPage(LauncherPage.patches); };
            goMods.Click += (s, e) => { SwitchPage(LauncherPage.mods); };
            goTools.Click += (s, e) => { SwitchPage(LauncherPage.tools); };
            goSettings.Click += (s, e) => { SwitchPage(LauncherPage.settings); };
            goGithub.Click += (s, e) => { System.Diagnostics.Process.Start(new ProcessStartInfo {FileName = "https://github.com/marcos4503/ts3-dream-launcher", UseShellExecute = true }); };
            goDonate.Click += (s, e) => { System.Diagnostics.Process.Start(new ProcessStartInfo {FileName = "https://www.paypal.com/donate/?hosted_button_id=MVDJY3AXLL8T2", UseShellExecute = true }); };
            goGuide.Click += (s, e) => { System.Diagnostics.Process.Start(new ProcessStartInfo { FileName = "https://steamcommunity.com/sharedfiles/filedetails/?id=3118587838", UseShellExecute = true }); };
            goExit.Click += (s, e) => { CheckToExitFromLauncherAndWarnIfHaveTasksRunning(); };

            //Auto switch to home page of the launcher
            SwitchPage(LauncherPage.home);

            //Check all available DLCs
            CheckAvailableDLCs();

            //Try to determine the my documents path
            myDocumentsPath = TryToDetermineTheSims3FolderPathInComputerMyDocuments();

            //If not found the my documents path, stops here
            if(myDocumentsPath == "")
            {
                //Disable all navigation buttons
                goSaves.IsEnabled = false;
                goExports.IsEnabled = false;
                goWorlds.IsEnabled = false;
                goMedia.IsEnabled = false;
                goCache.IsEnabled = false;
                goPatches.IsEnabled = false;
                goMods.IsEnabled = false;
                goTools.IsEnabled = false;
                goSettings.IsEnabled = false;

                //Enable the warning
                documentsNotFound.Visibility = Visibility.Visible;

                //Stop the execution
                return;
            }

            //Create the folder of cache of the Dream Launcher in my documents
            Directory.CreateDirectory((myDocumentsPath + "/!DL-TmpCache"));
            //Create the folder of static of the Dream Launcher in my documents
            Directory.CreateDirectory((myDocumentsPath + "/!DL-Static"));
            //Create the game folders that not exists, needed by the Launcher (to avoid crashing on acessing a folder that not exists yet)
            Directory.CreateDirectory((myDocumentsPath + "/Collections"));
            Directory.CreateDirectory((myDocumentsPath + "/ContentPatch"));
            Directory.CreateDirectory((myDocumentsPath + "/DCBackup"));
            Directory.CreateDirectory((myDocumentsPath + "/DCCache"));
            Directory.CreateDirectory((myDocumentsPath + "/Downloads"));
            Directory.CreateDirectory((myDocumentsPath + "/Exports"));
            Directory.CreateDirectory((myDocumentsPath + "/FeaturedItems"));
            Directory.CreateDirectory((myDocumentsPath + "/IGACache"));
            Directory.CreateDirectory((myDocumentsPath + "/InstalledWorlds"));
            Directory.CreateDirectory((myDocumentsPath + "/Library"));
            Directory.CreateDirectory((myDocumentsPath + "/Recorded Videos"));
            Directory.CreateDirectory((myDocumentsPath + "/SavedOutfits"));
            Directory.CreateDirectory((myDocumentsPath + "/SavedSims"));
            Directory.CreateDirectory((myDocumentsPath + "/Saves"));
            Directory.CreateDirectory((myDocumentsPath + "/Screenshots"));
            Directory.CreateDirectory((myDocumentsPath + "/SigsCache"));
            Directory.CreateDirectory((myDocumentsPath + "/Thumbnails"));
            Directory.CreateDirectory((myDocumentsPath + "/WorldCaches"));

            //Get a copy of "Options.ini" as template for settings apply, if don't have one
            LoadNewOptionsTemplateIfDontHaveOne();
            //Setup the reset options template button
            set_ResetOptTemplate.Click += (s, e) => 
            {
                //Clear the current template
                if (File.Exists((Directory.GetCurrentDirectory() + @"/Content/options-template.ini")) == true)
                    File.Delete((Directory.GetCurrentDirectory() + @"/Content/options-template.ini"));
                //Load a new
                LoadNewOptionsTemplateIfDontHaveOne();
                //Warn that is done
                MessageBox.Show(GetStringApplicationResource("launcher_settings_launcher_resetTemplate_dialogText"), 
                                GetStringApplicationResource("launcher_settings_launcher_resetTemplate_dialogTitle"),
                                MessageBoxButton.OK, MessageBoxImage.Information);
            };
            //Show the saved settings and automatically apply all defined settings
            ShowAllSettings();
            ApplyAllSettings();
            //Prepare the save settings button to save, and apply the settings automatically
            settingsSave.Click += (s, e) => 
            { 
                SaveAllSettings();
                ApplyAllSettings();
            };

            //Prepare the patch system
            BuildPatchesListAndPreparePatchSystem();

            //Prepare the cache system
            BuildAndPrepareCacheCleanListSystem();

            //Prepare the tools system
            BuildAndPrepareToolsListSystem();

            //Prepare the mods system
            BuildAndPrepareModsListSystem();

            //Prepare the media system
            BuildAndPrepareMediaListSystem();

            //Prepare the worlds system
            BuildAndPrepareWorldsSystem();

            //Prepare the exports system
            BuildAndPrepareExportsSystem();

            //Prepare the save system
            BuildAndPrepareSaveSystem();

            //Prepare the tips system
            BuildAndPrepareTipsSystem();

            //Check if have a new update and warn if was updated
            CheckUpdates();
            WarnIfLauncherWasSuccessfullyUpdated();
        }

        //Tasks manager

        public void AddTask(string id, string description)
        {
            //Add the task for the queue
            runningTasks.Add(id, description);

            //Update the tasks display
            UpdateTasksDisplay();
        }

        public void RemoveTask(string id)
        {
            //Remove the task from the queue
            runningTasks.Remove(id);

            //Update the tasks display
            UpdateTasksDisplay();
        }

        private void UpdateTasksDisplay()
        {
            //Count tasks quantity
            int tasksQuantity = runningTasks.Keys.Count;

            //If don't have more tasks
            if(tasksQuantity == 0)
            {
                doingTasksGif.Visibility = Visibility.Collapsed;
                doingTasksOk.Visibility = Visibility.Visible;
                tasksStatus.Content = GetStringApplicationResource("launcher_taks_readyToPlay");
                playGame.IsEnabled = true;
            }

            //If is doing tasks
            if(tasksQuantity >= 1)
            {
                doingTasksGif.Visibility = Visibility.Visible;
                doingTasksOk.Visibility = Visibility.Collapsed;
                tasksStatus.Content = GetStringApplicationResource("launcher_taks_working").Replace("%n%", tasksQuantity.ToString());
                playGame.IsEnabled = false;
            }

            //Update the tray icon
            UpdateLauncherSystemTray();

            //Update the patches install availability
            UpdatePatchesInstallAvailability();
        }

        public int GetRunningTasksCount()
        {
            //Return the running tasks count
            return runningTasks.Keys.Count;
        }

        //Toast manager

        public void ShowToast(string message, ToastType toastType)
        {
            //Show the message
            toastMessage.Text = message;
            if(toastType == ToastType.Success)
            {
                toastBg.Fill = new SolidColorBrush(Color.FromArgb(230, 0, 46, 136));
                toastBg.Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 7, 70));
            }
            if (toastType == ToastType.Error)
            {
                toastBg.Fill = new SolidColorBrush(Color.FromArgb(230, 136, 0, 0));
                toastBg.Stroke = new SolidColorBrush(Color.FromArgb(255, 70, 0, 0));
            }

            //Hide the empty message
            toastHistEmpty.Visibility = Visibility.Collapsed;
            //Add this toast to history
            if(currentToastsInHistory < 99)
                currentToastsInHistory += 1;
            //If the toast history is closed, show the notification
            if(isToastHistoryToggled == false)
            {
                toastsCounterBg.Visibility = Visibility.Visible;
                toastsCounterBg.IsHitTestVisible = false;
                toastsCounterT.Visibility = Visibility.Visible;
                toastsCounterT.IsHitTestVisible = false;
                toastsCounterT.Content = currentToastsInHistory.ToString();
            }

            //Add the toast in the history container
            Border toastBackground = new Border();
            toastsHistoryContainer.Children.Add(toastBackground);
            toastBackground.CornerRadius = new CornerRadius(8.0f, 8.0f, 0, 8.0f);
            toastBackground.BorderThickness = new Thickness(0, 0, 0, 0);
            toastBackground.BorderBrush = null;
            toastBackground.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            toastBackground.Margin = new Thickness(16, 4, 16, 4);
            toastBackground.Padding = new Thickness(4, 4, 4, 4);
            toastBackground.HorizontalAlignment = HorizontalAlignment.Stretch;
            toastBackground.VerticalAlignment = VerticalAlignment.Top;
            toastBackground.Width = double.NaN;
            toastBackground.Height = double.NaN;
            StackPanel toastOrganizer = new StackPanel();
            toastOrganizer.HorizontalAlignment = HorizontalAlignment.Stretch;
            toastOrganizer.VerticalAlignment = VerticalAlignment.Top;
            toastOrganizer.Width = double.NaN;
            toastOrganizer.Height = double.NaN;
            toastBackground.Child = toastOrganizer;
            TextBlock newToastMsg = new TextBlock();
            newToastMsg.FontSize = 10;
            newToastMsg.Text = message;
            newToastMsg.TextWrapping = TextWrapping.Wrap;
            newToastMsg.HorizontalAlignment = HorizontalAlignment.Stretch;
            newToastMsg.VerticalAlignment = VerticalAlignment.Top;
            newToastMsg.Width = double.NaN;
            newToastMsg.Height = double.NaN;
            toastOrganizer.Children.Add(newToastMsg);
            TextBlock newToastTime = new TextBlock();
            newToastTime.HorizontalAlignment = HorizontalAlignment.Stretch;
            newToastTime.VerticalAlignment = VerticalAlignment.Top;
            newToastTime.Width = double.NaN;
            newToastTime.Height = double.NaN;
            newToastTime.FontSize = 8;
            newToastTime.TextAlignment = TextAlignment.Right;
            newToastTime.Foreground = new SolidColorBrush(Color.FromArgb(255, 136, 136, 136));
            newToastTime.Text = (GetStringApplicationResource("launcher_toastsHistoryAt") + " " + DateTime.Now.ToString("H:mm:ss"));
            toastOrganizer.Children.Add(newToastTime);

            //Move the scrollview to end
            toastHistoryScroll.ScrollToEnd();

            //Enable the toast
            toastPopUp.Visibility = Visibility.Visible;
            animStoryboards["toastEnter"].Begin();
        }

        private IEnumerator WaitAndDismissTheToast()
        {
            //Do the dismiss animation
            animStoryboards["toastExit"].Begin();

            //Wait some time..
            yield return new WaitForSeconds(0.5f);

            //Dismiss the toast
            toastPopUp.Visibility = Visibility.Collapsed;
        }

        private IEnumerator WaitAndCloseToastHistory()
        {
            //Do the dismiss animation
            animStoryboards["toastHistoryExit"].Begin();

            //Wait some time..
            yield return new WaitForSeconds(0.5f);

            //Dismiss the toast history
            toastsHistory.Visibility = Visibility.Collapsed;
        }

        //Pages switch

        private void SwitchPage(LauncherPage desiredPage)
        {
            //----- Start of notifications disablers... -----//
            if (desiredPage == LauncherPage.saves)
                savesWarn.Visibility = Visibility.Collapsed;
            if (desiredPage == LauncherPage.exports)
                exportsWarn.Visibility = Visibility.Collapsed;
            if (desiredPage == LauncherPage.worlds)
                worldsWarn.Visibility = Visibility.Collapsed;
            if (desiredPage == LauncherPage.media)
                mediaWarn.Visibility = Visibility.Collapsed;
            if (desiredPage == LauncherPage.cache)
                cacheWarn.Visibility = Visibility.Collapsed;
            if (desiredPage == LauncherPage.patches)
                patchesWarn.Visibility = Visibility.Collapsed;
            //-----  End of notifications disablers...  -----//

            //Prepare the data
            Color btSelectedColor = Color.FromArgb(255, 0, 40, 86);
            Color btUnselectedColor = Color.FromArgb(255, 44, 103, 169);

            //Prepare the dictionary of pages
            int pageIdToShow = -1;
            switch (desiredPage)
            {
                case LauncherPage.home:
                    pageIdToShow = 0;
                    break;
                case LauncherPage.saves:
                    pageIdToShow = 1;
                    break;
                case LauncherPage.exports:
                    pageIdToShow = 2;
                    break;
                case LauncherPage.worlds:
                    pageIdToShow = 3;
                    break;
                case LauncherPage.media:
                    pageIdToShow = 4;
                    break;
                case LauncherPage.cache:
                    pageIdToShow = 5;
                    break;
                case LauncherPage.patches:
                    pageIdToShow = 6;
                    break;
                case LauncherPage.mods:
                    pageIdToShow = 7;
                    break;
                case LauncherPage.tools:
                    pageIdToShow = 8;
                    break;
                case LauncherPage.settings:
                    pageIdToShow = 9;
                    break;
            }

            //Prepare the list of buttons
            List<Controls.BeautyButton.BeautyButton> buttons = new List<Controls.BeautyButton.BeautyButton>();
            buttons.Add(goHome);
            buttons.Add(goSaves);
            buttons.Add(goExports);
            buttons.Add(goWorlds);
            buttons.Add(goMedia);
            buttons.Add(goCache);
            buttons.Add(goPatches);
            buttons.Add(goMods);
            buttons.Add(goTools);
            buttons.Add(goSettings);

            //Prepare the list of pages
            List<Grid> pages = new List<Grid>();
            pages.Add(pageHome);
            pages.Add(pageSaves);
            pages.Add(pageExports);
            pages.Add(pageWorlds);
            pages.Add(pageMedia);
            pages.Add(pageCache);
            pages.Add(pagePatches);
            pages.Add(pageMods);
            pages.Add(pageTools);
            pages.Add(pageSettings);

            //Prepare the list of titles
            List<string> titles = new List<string>();
            titles.Add(GetStringApplicationResource("launcher_button_goHome"));
            titles.Add(GetStringApplicationResource("launcher_button_goSaves")); 
            titles.Add(GetStringApplicationResource("launcher_button_goExports"));
            titles.Add(GetStringApplicationResource("launcher_button_goWorlds"));
            titles.Add(GetStringApplicationResource("launcher_button_goMedia"));
            titles.Add(GetStringApplicationResource("launcher_button_goCache"));
            titles.Add(GetStringApplicationResource("launcher_button_goPatches"));
            titles.Add(GetStringApplicationResource("launcher_button_goMods")); 
            titles.Add(GetStringApplicationResource("launcher_button_goTools"));
            titles.Add(GetStringApplicationResource("launcher_button_goSettings"));

            //If was found a ID
            if (pageIdToShow != -1)
            {
                //Disable all pages
                for(int i = 0; i < buttons.Count; i++)
                {
                    buttons[i].Background = new SolidColorBrush(btUnselectedColor);
                    pages[i].Visibility = Visibility.Collapsed;
                }

                //Enable the desired page
                buttons[pageIdToShow].Background = new SolidColorBrush(btSelectedColor);
                pages[pageIdToShow].Visibility = Visibility.Visible;
                pageTitle.Content = titles[pageIdToShow];
            }

            //Clear the lists
            buttons.Clear();
            pages.Clear();
            titles.Clear();

            //Inform the current viewing page
            currentLauncherPageViewing = desiredPage;
        }

        public void EnablePageRedDotNotification(LauncherPage desiredPage)
        {
            //If the page to show red dot is the same that is being viewed, cancel
            if (desiredPage == currentLauncherPageViewing)
                return;

            //Show the red dot...
            if (desiredPage == LauncherPage.saves)
                savesWarn.Visibility = Visibility.Visible;
            if (desiredPage == LauncherPage.exports)
                exportsWarn.Visibility = Visibility.Visible;
            if (desiredPage == LauncherPage.worlds)
                worldsWarn.Visibility = Visibility.Visible;
            if (desiredPage == LauncherPage.media)
                mediaWarn.Visibility = Visibility.Visible;
            if (desiredPage == LauncherPage.cache)
                cacheWarn.Visibility = Visibility.Visible;
            if (desiredPage == LauncherPage.patches)
                patchesWarn.Visibility = Visibility.Visible;
        }

        private void DisableAllRedDotsNotifications()
        {
            //Disable all red dots notifications
            savesWarn.Visibility = Visibility.Collapsed;
            exportsWarn.Visibility = Visibility.Collapsed;
            worldsWarn.Visibility = Visibility.Collapsed;
            mediaWarn.Visibility = Visibility.Collapsed;
            cacheWarn.Visibility = Visibility.Collapsed;
            patchesWarn.Visibility = Visibility.Collapsed;
        }

        //Patches manager

        private void BuildPatchesListAndPreparePatchSystem()
        {
            //Prepare the restart button
            patchRestartButton.Click += (s, e) => { CheckToExitFromLauncherAndWarnIfHaveTasksRunning(); };

            //Instantiate all patch items
            InstantiateEachPatchItemAndCheckIntegrityOfInstalleds();

            //Update the patches install availability
            UpdatePatchesInstallAvailability();
        }

        private void InstantiateEachPatchItemAndCheckIntegrityOfInstalleds()
        {
            //Alder Lake+ Support
            if(true == true)
            {
                //Instantiate and store reference for it
                PatchItem newPatchItem = new PatchItem(this, -1);
                patchesList.Children.Add(newPatchItem);
                instantiatedPatchItems.Add(newPatchItem);
                //Set it up
                newPatchItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newPatchItem.VerticalAlignment = VerticalAlignment.Stretch;
                newPatchItem.Width = double.NaN;
                newPatchItem.Height = double.NaN;
                newPatchItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this patch item
                newPatchItem.SetPatchIcon("Resources/patch-0.png");
                newPatchItem.SetPatchTitle(GetStringApplicationResource("launcher_patch0_title"));
                newPatchItem.SetPatchDescription(GetStringApplicationResource("launcher_patch0_description"));
                //Show if is installed
                bool patchResultExists = File.Exists((Directory.GetCurrentDirectory() + @"/TS3W-backup.exe"));
                if (patchResultExists == true)
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.InstalledWithNoActions);
                if (patchResultExists == false)
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.NotInstalledWithNoActions);
            }

            //PT-PT to PT-BR better support
            if (true == true)
            {
                //Instantiate and store reference for it
                PatchItem newPatchItem = new PatchItem(this, -1);
                patchesList.Children.Add(newPatchItem);
                instantiatedPatchItems.Add(newPatchItem);
                //Set it up
                newPatchItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newPatchItem.VerticalAlignment = VerticalAlignment.Stretch;
                newPatchItem.Width = double.NaN;
                newPatchItem.Height = double.NaN;
                newPatchItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this patch item
                newPatchItem.SetPatchIcon("Resources/patch-1.png");
                newPatchItem.SetPatchTitle(GetStringApplicationResource("launcher_patch1_title"));
                newPatchItem.SetPatchDescription(GetStringApplicationResource("launcher_patch1_description"));
                //Show if is installed
                bool patchResultExists = File.Exists(((new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent).FullName + "/47890_install-backup.vdf"));
                if (patchResultExists == true)
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.InstalledWithNoActions);
                if (patchResultExists == false)
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.NotInstalledWithNoActions);
            }

            //Mods Support
            if (true == true)
            {
                //Instantiate and store reference for it
                PatchItem newPatchItem = new PatchItem(this, instantiatedPatchItems.Count);
                patchesList.Children.Add(newPatchItem);
                instantiatedPatchItems.Add(newPatchItem);
                //Set it up
                newPatchItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newPatchItem.VerticalAlignment = VerticalAlignment.Stretch;
                newPatchItem.Width = double.NaN;
                newPatchItem.Height = double.NaN;
                newPatchItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this patch item
                newPatchItem.SetPatchIcon("Resources/patch-2.png");
                newPatchItem.SetPatchTitle(GetStringApplicationResource("launcher_patch2_title"));
                newPatchItem.SetPatchDescription(GetStringApplicationResource("launcher_patch2_description"));
                //Show if is installed
                if (launcherPrefs.loadedData.patchModsSupport == true)
                {
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    DoPatch_ModsSupport(PatchMode.CheckIntegrity, newPatchItem.thisInstantiationIdInList);
                }
                if (launcherPrefs.loadedData.patchModsSupport == false)
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);
                //Add callbacks for buttons
                newPatchItem.RegisterOnClickInstallCallback((thisPatchItem) => { DoPatch_ModsSupport(PatchMode.Install, thisPatchItem.thisInstantiationIdInList); });
            }

            //Basic Optimization
            if(true == true)
            {
                //Instantiate and store reference for it
                PatchItem newPatchItem = new PatchItem(this, instantiatedPatchItems.Count);
                patchesList.Children.Add(newPatchItem);
                instantiatedPatchItems.Add(newPatchItem);
                //Disable the interaction, if don't have requiremenet patches installed
                if(launcherPrefs.loadedData.patchModsSupport == false)
                {
                    newPatchItem.IsHitTestVisible = false;
                    newPatchItem.Opacity = 0.30f;
                }
                //Set it up
                newPatchItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newPatchItem.VerticalAlignment = VerticalAlignment.Stretch;
                newPatchItem.Width = double.NaN;
                newPatchItem.Height = double.NaN;
                newPatchItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this patch item
                newPatchItem.SetPatchIcon("Resources/patch-3.png");
                newPatchItem.SetPatchTitle(GetStringApplicationResource("launcher_patch3_title"));
                newPatchItem.SetPatchDescription(GetStringApplicationResource("launcher_patch3_description"));
                //Show if is installed
                if (launcherPrefs.loadedData.patchBasicOptimization == true)
                {
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    DoPatch_BasicOptimization(PatchMode.CheckIntegrity, newPatchItem.thisInstantiationIdInList);
                }
                if (launcherPrefs.loadedData.patchBasicOptimization == false)
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);
                //Add callbacks for buttons
                newPatchItem.RegisterOnClickInstallCallback((thisPatchItem) => { DoPatch_BasicOptimization(PatchMode.Install, thisPatchItem.thisInstantiationIdInList); });
            }

            //FPS Limiter
            if (true == true)
            {
                //Instantiate and store reference for it
                PatchItem newPatchItem = new PatchItem(this, instantiatedPatchItems.Count);
                patchesList.Children.Add(newPatchItem);
                instantiatedPatchItems.Add(newPatchItem);
                //Set it up
                newPatchItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newPatchItem.VerticalAlignment = VerticalAlignment.Stretch;
                newPatchItem.Width = double.NaN;
                newPatchItem.Height = double.NaN;
                newPatchItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this patch item
                newPatchItem.SetPatchIcon("Resources/patch-4.png");
                newPatchItem.SetPatchTitle(GetStringApplicationResource("launcher_patch4_title"));
                newPatchItem.SetPatchDescription(GetStringApplicationResource("launcher_patch4_description"));
                //Show if is installed
                if (launcherPrefs.loadedData.patchFpsLimiter == true)
                {
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    DoPatch_FpsLimiter(PatchMode.CheckIntegrity, newPatchItem.thisInstantiationIdInList);
                }
                if (launcherPrefs.loadedData.patchFpsLimiter == false)
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);
                //Add callbacks for buttons
                newPatchItem.RegisterOnClickInstallCallback((thisPatchItem) => { DoPatch_FpsLimiter(PatchMode.Install, thisPatchItem.thisInstantiationIdInList); });
            }

            //GPU and CPU update
            if (true == true)
            {
                //Instantiate and store reference for it
                PatchItem newPatchItem = new PatchItem(this, instantiatedPatchItems.Count);
                patchesList.Children.Add(newPatchItem);
                instantiatedPatchItems.Add(newPatchItem);
                //Set it up
                newPatchItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newPatchItem.VerticalAlignment = VerticalAlignment.Stretch;
                newPatchItem.Width = double.NaN;
                newPatchItem.Height = double.NaN;
                newPatchItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this patch item
                newPatchItem.SetPatchIcon("Resources/patch-5.png");
                newPatchItem.SetPatchTitle(GetStringApplicationResource("launcher_patch5_title"));
                newPatchItem.SetPatchDescription(GetStringApplicationResource("launcher_patch5_description"));
                //Show if is installed
                if (launcherPrefs.loadedData.patchCpuGpuUpdate == true)
                {
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    DoPatch_GpuCpuUpdate(PatchMode.CheckIntegrity, newPatchItem.thisInstantiationIdInList);
                }
                if (launcherPrefs.loadedData.patchCpuGpuUpdate == false)
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);
                //Add callbacks for buttons
                newPatchItem.RegisterOnClickInstallCallback((thisPatchItem) => { DoPatch_GpuCpuUpdate(PatchMode.Install, thisPatchItem.thisInstantiationIdInList); });
            }

            //Better Global Illumination
            if (true == true)
            {
                //Instantiate and store reference for it
                PatchItem newPatchItem = new PatchItem(this, instantiatedPatchItems.Count);
                patchesList.Children.Add(newPatchItem);
                instantiatedPatchItems.Add(newPatchItem);
                //Disable the interaction, if don't have requiremenet patches installed
                if (launcherPrefs.loadedData.patchModsSupport == false)
                {
                    newPatchItem.IsHitTestVisible = false;
                    newPatchItem.Opacity = 0.30f;
                }
                //Set it up
                newPatchItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newPatchItem.VerticalAlignment = VerticalAlignment.Stretch;
                newPatchItem.Width = double.NaN;
                newPatchItem.Height = double.NaN;
                newPatchItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this patch item
                newPatchItem.SetPatchIcon("Resources/patch-6.png");
                newPatchItem.SetPatchTitle(GetStringApplicationResource("launcher_patch6_title"));
                newPatchItem.SetPatchDescription(GetStringApplicationResource("launcher_patch6_description"));
                //Show if is installed
                if (launcherPrefs.loadedData.patchBetterGlobalIllumination == true)
                {
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    DoPatch_BetterGlobalIllumination(PatchMode.CheckIntegrity, newPatchItem.thisInstantiationIdInList);
                }
                if (launcherPrefs.loadedData.patchBetterGlobalIllumination == false)
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);
                //Add callbacks for buttons
                newPatchItem.RegisterOnClickInstallCallback((thisPatchItem) => { DoPatch_BetterGlobalIllumination(PatchMode.Install, thisPatchItem.thisInstantiationIdInList); });
            }

            //Improved Shading
            if (true == true)
            {
                //Instantiate and store reference for it
                PatchItem newPatchItem = new PatchItem(this, instantiatedPatchItems.Count);
                patchesList.Children.Add(newPatchItem);
                instantiatedPatchItems.Add(newPatchItem);
                //Set it up
                newPatchItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newPatchItem.VerticalAlignment = VerticalAlignment.Stretch;
                newPatchItem.Width = double.NaN;
                newPatchItem.Height = double.NaN;
                newPatchItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this patch item
                newPatchItem.SetPatchIcon("Resources/patch-7.png");
                newPatchItem.SetPatchTitle(GetStringApplicationResource("launcher_patch7_title"));
                newPatchItem.SetPatchDescription(GetStringApplicationResource("launcher_patch7_description"));
                //Show if is installed
                if (launcherPrefs.loadedData.patchImprovedShaders == true)
                {
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    DoPatch_ImprovedShaders(PatchMode.CheckIntegrity, newPatchItem.thisInstantiationIdInList);
                }
                if (launcherPrefs.loadedData.patchImprovedShaders == false)
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);
                //Add callbacks for buttons
                newPatchItem.RegisterOnClickInstallCallback((thisPatchItem) => { DoPatch_ImprovedShaders(PatchMode.Install, thisPatchItem.thisInstantiationIdInList); });
            }

            //Shadow Extender
            if (true == true)
            {
                //Instantiate and store reference for it
                PatchItem newPatchItem = new PatchItem(this, instantiatedPatchItems.Count);
                patchesList.Children.Add(newPatchItem);
                instantiatedPatchItems.Add(newPatchItem);
                //Set it up
                newPatchItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newPatchItem.VerticalAlignment = VerticalAlignment.Stretch;
                newPatchItem.Width = double.NaN;
                newPatchItem.Height = double.NaN;
                newPatchItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this patch item
                newPatchItem.SetPatchIcon("Resources/patch-8.png");
                newPatchItem.SetPatchTitle(GetStringApplicationResource("launcher_patch8_title"));
                newPatchItem.SetPatchDescription(GetStringApplicationResource("launcher_patch8_description"));
                //Show if is installed
                if (launcherPrefs.loadedData.patchShadowExtender == true)
                {
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    DoPatch_ShadowExtender(PatchMode.CheckIntegrity, newPatchItem.thisInstantiationIdInList);
                }
                if (launcherPrefs.loadedData.patchShadowExtender == false)
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);
                //Add callbacks for buttons
                newPatchItem.RegisterOnClickInstallCallback((thisPatchItem) => { DoPatch_ShadowExtender(PatchMode.Install, thisPatchItem.thisInstantiationIdInList); });
            }

            //Routing optimization
            if (true == true)
            {
                //Instantiate and store reference for it
                PatchItem newPatchItem = new PatchItem(this, instantiatedPatchItems.Count);
                patchesList.Children.Add(newPatchItem);
                instantiatedPatchItems.Add(newPatchItem);
                //Disable the interaction, if don't have requiremenet patches installed
                if (launcherPrefs.loadedData.patchModsSupport == false)
                {
                    newPatchItem.IsHitTestVisible = false;
                    newPatchItem.Opacity = 0.30f;
                }
                //Set it up
                newPatchItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newPatchItem.VerticalAlignment = VerticalAlignment.Stretch;
                newPatchItem.Width = double.NaN;
                newPatchItem.Height = double.NaN;
                newPatchItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this patch item
                newPatchItem.SetPatchIcon("Resources/patch-9.png");
                newPatchItem.SetPatchTitle(GetStringApplicationResource("launcher_patch9_title"));
                newPatchItem.SetPatchDescription(GetStringApplicationResource("launcher_patch9_description"));
                //Show if is installed
                if (launcherPrefs.loadedData.patchRoutingOptimizations == true)
                {
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    DoPatch_RoutingOptimizations(PatchMode.CheckIntegrity, newPatchItem.thisInstantiationIdInList);
                }
                if (launcherPrefs.loadedData.patchRoutingOptimizations == false)
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);
                //Add callbacks for buttons
                newPatchItem.RegisterOnClickInstallCallback((thisPatchItem) => { DoPatch_RoutingOptimizations(PatchMode.Install, thisPatchItem.thisInstantiationIdInList); });
            }

            //Better Story Progression
            if (true == true)
            {
                //Instantiate and store reference for it
                PatchItem newPatchItem = new PatchItem(this, instantiatedPatchItems.Count);
                patchesList.Children.Add(newPatchItem);
                instantiatedPatchItems.Add(newPatchItem);
                //Disable the interaction, if don't have requiremenet patches installed
                if (launcherPrefs.loadedData.patchModsSupport == false)
                {
                    newPatchItem.IsHitTestVisible = false;
                    newPatchItem.Opacity = 0.30f;
                }
                //Set it up
                newPatchItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newPatchItem.VerticalAlignment = VerticalAlignment.Stretch;
                newPatchItem.Width = double.NaN;
                newPatchItem.Height = double.NaN;
                newPatchItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this patch item
                newPatchItem.SetPatchIcon("Resources/patch-11.png");
                newPatchItem.SetPatchTitle(GetStringApplicationResource("launcher_patch11_title"));
                newPatchItem.SetPatchDescription(GetStringApplicationResource("launcher_patch11_description"));
                //Show if is installed
                if (launcherPrefs.loadedData.patchStoryProgression == true)
                {
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    DoPatch_BetterStoryProgression(PatchMode.CheckIntegrity, newPatchItem.thisInstantiationIdInList);
                }
                if (launcherPrefs.loadedData.patchStoryProgression == false)
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);
                //Add callbacks for buttons
                newPatchItem.RegisterOnClickInstallCallback((thisPatchItem) => { DoPatch_BetterStoryProgression(PatchMode.Install, thisPatchItem.thisInstantiationIdInList); });
            }

            //Internet removal
            if (true == true)
            {
                //Instantiate and store reference for it
                PatchItem newPatchItem = new PatchItem(this, instantiatedPatchItems.Count);
                patchesList.Children.Add(newPatchItem);
                instantiatedPatchItems.Add(newPatchItem);
                //Set it up
                newPatchItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newPatchItem.VerticalAlignment = VerticalAlignment.Stretch;
                newPatchItem.Width = double.NaN;
                newPatchItem.Height = double.NaN;
                newPatchItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this patch item
                newPatchItem.SetPatchIcon("Resources/patch-10.png");
                newPatchItem.SetPatchTitle(GetStringApplicationResource("launcher_patch10_title"));
                newPatchItem.SetPatchDescription(GetStringApplicationResource("launcher_patch10_description"));
                //Show if is installed
                if (launcherPrefs.loadedData.patchInternetRemoval == true)
                {
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    DoPatch_InternetRemoval(PatchMode.CheckIntegrity, newPatchItem.thisInstantiationIdInList);
                }
                if (launcherPrefs.loadedData.patchInternetRemoval == false)
                    newPatchItem.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);
                //Add callbacks for buttons
                newPatchItem.RegisterOnClickInstallCallback((thisPatchItem) => { DoPatch_InternetRemoval(PatchMode.Install, thisPatchItem.thisInstantiationIdInList); });
            }

            //Add the final spacer
            Grid finalSpacer = new Grid();
            patchesList.Children.Add(finalSpacer);
            //Set it up
            finalSpacer.HorizontalAlignment = HorizontalAlignment.Stretch;
            finalSpacer.VerticalAlignment = VerticalAlignment.Top;
            finalSpacer.Width = double.NaN;
            finalSpacer.Height = 16.0f;

            //Inform that was rendered all patches
            wasRenderedAllPatches = true;
        }

        private void DoPatch_AlderLakePatch()
        {
            //Add the task
            AddTask("alderLakePatching", "Do the patch for Alder Lake CPUs.");

            //Create a thread to make the patch
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { });
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(3000);

                //Try to make the patch
                try
                {
                    //If have a backup, restore the backup
                    if (File.Exists((Directory.GetCurrentDirectory() + @"/TS3W-backup.exe")) == true)
                    {
                        File.Delete((Directory.GetCurrentDirectory() + @"/TS3W.exe"));
                        File.Copy((Directory.GetCurrentDirectory() + @"/TS3W-backup.exe"), (Directory.GetCurrentDirectory() + @"/TS3W.exe"));
                    }
                    //If was never made a backup, do a backup of "TS3W.exe" file...
                    if (File.Exists((Directory.GetCurrentDirectory() + @"/TS3W-backup.exe")) == false)
                        File.Copy((Directory.GetCurrentDirectory() + @"/TS3W.exe"), (Directory.GetCurrentDirectory() + @"/TS3W-backup.exe"));

                    //Do the patching
                    PeNet.PeFile peFile = new PeNet.PeFile((Directory.GetCurrentDirectory() + @"/TS3W.exe"));
                    peFile.AddImport("IntelFix.dll", "_DllMain@12");
                    File.WriteAllBytes((Directory.GetCurrentDirectory() + @"/TS3W.exe"), peFile.RawFile.ToArray());
                    if (File.Exists((Directory.GetCurrentDirectory() + @"/IntelFix.dll")) == true)
                        File.Delete((Directory.GetCurrentDirectory() + @"/IntelFix.dll"));
                    File.Copy((Directory.GetCurrentDirectory() + @"/Content/IntelFix.dll"), (Directory.GetCurrentDirectory() + @"/IntelFix.dll"));

                    //Return a success response
                    return new string[] { "success" };
                }
                catch (Exception ex)
                {
                    //Return a error response
                    return new string[] { "error", ex.Message };
                }

                //Return a default result
                return new string[] { "none" };
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //If have a success
                if (backgroundResult[0] == "success")
                {
                    //Inform that is patched
                    launcherPrefs.loadedData.alreadyIntelFixed = true;
                    launcherPrefs.Save();
                    ShowToast(GetStringApplicationResource("launcher_alderLakePatchSuccess"), ToastType.Success);
                }
                //If have a error
                if (backgroundResult[0] == "error")
                {
                    ShowToast((GetStringApplicationResource("launcher_alderLakePatchProblem") + " \"" + backgroundResult[1] + "\""), ToastType.Error);
                    EnablePageRedDotNotification(LauncherPage.patches);
                }

                //Remove the task
                RemoveTask("alderLakePatching");
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        private void DoPatch_PtPtToPtBrTranslate()
        {
            //Add the task
            AddTask("ptPtToPtBrTranslatePatching", "Do the pt-PT to pt-BR translation of game.");

            //Create a thread to make the patch
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { });
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(5000);

                //Try to make the patch
                try
                {
                    //Get the path do two folders up
                    string localeFileDirectory = (new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent).FullName;

                    //If have a backup, restore the backup
                    if (File.Exists((localeFileDirectory + "/47890_install-backup.vdf")) == true)
                    {
                        File.Delete((localeFileDirectory + "/47890_install.vdf"));
                        File.Copy((localeFileDirectory + "/47890_install-backup.vdf"), (localeFileDirectory + "/47890_install.vdf"));
                    }
                    //If was never made a backup, do a backup of "47890_install.vdf" file...
                    if (File.Exists((localeFileDirectory + "/47890_install-backup.vdf")) == false)
                        File.Copy((localeFileDirectory + "/47890_install.vdf"), (localeFileDirectory + "/47890_install-backup.vdf"));

                    //Translate the file from pt-PT to pt-BR
                    string localeContent = File.ReadAllText((localeFileDirectory + "/47890_install.vdf"));
                    string localeContent0 = localeContent.Replace("\"https://pt.thesims3.com/register.html\"", "\"https://br.thesims3.com/register.html\"");
                    string localeContent1 = localeContent0.Replace("\"pt-pt\"", "\"pt-BR\"");
                    string localeContent2 = localeContent1.Replace("\"PT\"", "\"BR\"");
                    File.WriteAllText((localeFileDirectory + "/47890_install.vdf"), localeContent2);

                    //Return a success response
                    return new string[] { "success" };
                }
                catch (Exception ex)
                {
                    //Return a error response
                    return new string[] { "error", ex.Message };
                }

                //Return a default result
                return new string[] { "none" };
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //If have a success
                if (backgroundResult[0] == "success")
                {
                    //Inform that is translated
                    launcherPrefs.loadedData.alreadyTranslated = true;
                    launcherPrefs.Save();
                    ShowToast(GetStringApplicationResource("launcher_ptptToptbrTranslateSuccess"), ToastType.Success);
                }
                //If have a error
                if (backgroundResult[0] == "error") 
                {
                    ShowToast((GetStringApplicationResource("launcher_ptptToptbrTranslateProblem") + " \"" + backgroundResult[1] + "\""), ToastType.Error);
                    EnablePageRedDotNotification(LauncherPage.patches);
                }

                //Remove the task
                RemoveTask("ptPtToPtBrTranslatePatching");
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        private void DoPatch_ModsSupport(PatchMode doPatchMode, int instantiatedPatchItemInList)
        {
            //
            //
            //
            //INTEGRITY CHECK...
            //
            //
            //

            //If is desired to only check integrity
            if (doPatchMode == PatchMode.CheckIntegrity)
            {
                //Add the task to queue
                AddTask("patch_modsSupport_integrity", "Checking patch.");

                //--- Start a thread to check patch integrity ---//
                AsyncTaskSimplified aTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                aTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(1000);
                    //Prepare the response to return
                    string toReturn = "ok";

                    //Check integrity of this patch...
                    if (Directory.Exists((myDocumentsPath + "/Mods")) == false)
                        toReturn = "problemFound";
                    if (File.Exists((myDocumentsPath + "/Mods/DreamLauncher.dl3")) == false)
                        toReturn = "problemFound";

                    //Finish the thread...
                    return new string[] { toReturn, (startParams[0]) };
                };
                aTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of problem, inform it
                    if (threadTaskResponse == "problemFound")
                    {
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.InstalledWithProblem);
                        ShowToast(GetStringApplicationResource("launcher_patches_problemFound"), ToastType.Error);
                        EnablePageRedDotNotification(LauncherPage.patches);
                    }

                    //Remove the task from queue
                    RemoveTask("patch_modsSupport_integrity");
                };
                aTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }

            //
            //
            //
            //INSTALLING...
            //
            //
            //

            //If is desired to install
            if (doPatchMode == PatchMode.Install)
            {
                //--- Start a thread to do the patching ---//
                AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) =>
                {
                    //Get the instantiated patch item in list
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(startParams[0]))];
                    //Change the UI
                    instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installing);
                    //Add the task to queue
                    AddTask("patch_modsSupport", "Installing patch.");
                };
                asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(5000);

                    //Try to do the task
                    try
                    {
                        //If "Mods" folder exists, and file "DreamLauncher.dl3" don't exists inside "Mods" folder, delete the "Mods" folder
                        if (Directory.Exists((myDocumentsPath + "/Mods")) == true)
                            if (File.Exists((myDocumentsPath + "/Mods/DreamLauncher.dl3")) == false)
                                Directory.Delete((myDocumentsPath + "/Mods"), true);

                        //If "Mods" folder don't exists, download it and install
                        if (Directory.Exists((myDocumentsPath + "/Mods")) == false)
                        {
                            //Prepare the target download URL
                            string downloadUrl = @"https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/patch-mods-support.zip";
                            string saveAsPath = (myDocumentsPath + @"/!DL-TmpCache/patch-mods-support.zip");
                            //Download the "Mods" folder sync
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

                            //Extract the downloaded patch
                            ZipFile zipFile = ZipFile.Read(saveAsPath);
                            foreach (ZipEntry entry in zipFile)
                                entry.Extract((myDocumentsPath + @"/!DL-TmpCache"), ExtractExistingFileAction.OverwriteSilently);
                            zipFile.Dispose();

                            //Put the downloaded mods folder into the place
                            Directory.Move((myDocumentsPath + @"/!DL-TmpCache/Mods"), (myDocumentsPath + @"/Mods"));
                        }

                        //Return a success response
                        return new string[] { "success", (startParams[0]) };
                    }
                    catch (Exception ex)
                    {
                        //Return a error response
                        return new string[] { "error", (startParams[0]) };
                    }

                    //Finish the thread...
                    return new string[] { "none", (startParams[0]) };
                };
                asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of success, inform it
                    if (threadTaskResponse == "success")
                    {
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallSuccess").Replace("%name%", GetStringApplicationResource("launcher_patch2_title"))), ToastType.Success);
                        launcherPrefs.loadedData.patchModsSupport = true;
                        launcherPrefs.Save();

                        //Request the restart
                        restartPopUp.Visibility = Visibility.Visible;
                    }
                    //If have a response different from success, inform error
                    if (threadTaskResponse != "success")
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallError").Replace("%name%", GetStringApplicationResource("launcher_patch2_title"))), ToastType.Error);

                    //Update the UI
                    if (launcherPrefs.loadedData.patchModsSupport == true)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    if (launcherPrefs.loadedData.patchModsSupport == false)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);

                    //Remove the task from queue
                    RemoveTask("patch_modsSupport");
                };
                asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }
        }

        private void DoPatch_BasicOptimization(PatchMode doPatchMode, int instantiatedPatchItemInList)
        {
            //
            //
            //
            //INTEGRITY CHECK...
            //
            //
            //

            //If is desired to only check integrity
            if (doPatchMode == PatchMode.CheckIntegrity)
            {
                //Add the task to queue
                AddTask("patch_basicOptimization_integrity", "Checking patch.");

                //--- Start a thread to check patch integrity ---//
                AsyncTaskSimplified aTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                aTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(1000);
                    //Prepare the response to return
                    string toReturn = "ok";

                    //Prepare a list of files
                    List<string> filesToCheck = new List<string>();
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/ErrorTrap.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/HideExpansionPacksGameIcons.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/MasterController.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/MasterController_Cheats.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/MasterController_Integration.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/MemoriesDisabled.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/Overwatch.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/Overwatch_Tuning.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/Register.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/Register_Tuning.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/SmoothPatch.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/SmoothPatch_MasterController.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/Traffic.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/Traffic_Tuning.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/Traveler.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/Traveler_Tuning.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/GoHere.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/GoHere_Tuning.package"));
                    filesToCheck.Add((Directory.GetCurrentDirectory() + @"/ddraw.dll"));
                    filesToCheck.Add((Directory.GetCurrentDirectory() + @"/TS3Patch.asi"));
                    filesToCheck.Add((Directory.GetCurrentDirectory() + @"/TS3Patch.txt"));

                    //Check integrity of this patch...
                    foreach(string filePath in filesToCheck)
                        if(File.Exists(filePath) == false)
                        {
                            toReturn = "problemFound";
                            break;
                        }

                    //Finish the thread...
                    return new string[] { toReturn, (startParams[0]) };
                };
                aTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of problem, inform it
                    if (threadTaskResponse == "problemFound")
                    {
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.InstalledWithProblem);
                        ShowToast(GetStringApplicationResource("launcher_patches_problemFound"), ToastType.Error);
                        EnablePageRedDotNotification(LauncherPage.patches);
                    }

                    //Remove the task from queue
                    RemoveTask("patch_basicOptimization_integrity");
                };
                aTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }

            //
            //
            //
            //INSTALLING...
            //
            //
            //

            //If is desired to install
            if (doPatchMode == PatchMode.Install)
            {
                //--- Start a thread to do the patching ---//
                AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) =>
                {
                    //Get the instantiated patch item in list
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(startParams[0]))];
                    //Change the UI
                    instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installing);
                    //Add the task to queue
                    AddTask("patch_basicOptimization", "Installing patch.");
                };
                asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(5000);

                    //Try to do the task
                    try
                    {
                        //Prepare a list of files
                        List<string> filesToCheck = new List<string>();
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/ErrorTrap.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/HideExpansionPacksGameIcons.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/MasterController.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/MasterController_Cheats.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/MasterController_Integration.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/MemoriesDisabled.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/Overwatch.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/Overwatch_Tuning.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/Register.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/Register_Tuning.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/SmoothPatch.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/SmoothPatch_MasterController.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/Traffic.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/Traffic_Tuning.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/Traveler.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/Traveler_Tuning.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/GoHere.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/GoHere_Tuning.package"));
                        filesToCheck.Add((Directory.GetCurrentDirectory() + @"/ddraw.dll"));
                        filesToCheck.Add((Directory.GetCurrentDirectory() + @"/TS3Patch.asi"));
                        filesToCheck.Add((Directory.GetCurrentDirectory() + @"/TS3Patch.txt"));
                        //Remove all files, if exists
                        foreach (string filePath in filesToCheck)
                            if (File.Exists(filePath) == true)
                                File.Delete(filePath);

                        //Download the patch files...
                        //Prepare the target download URL
                        string downloadUrl = @"https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/patch-basic-optimization.zip";
                        string saveAsPath = (myDocumentsPath + @"/!DL-TmpCache/patch-basic-optimization.zip");
                        //Download the "Mods" folder sync
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

                        //Extract the downloaded patch files
                        ZipFile zipFile = ZipFile.Read(saveAsPath);
                        foreach (ZipEntry entry in zipFile)
                            entry.Extract((myDocumentsPath + @"/!DL-TmpCache"), ExtractExistingFileAction.OverwriteSilently);
                        zipFile.Dispose();

                        //Put the files in the right place
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Pkgs/ErrorTrap.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/ErrorTrap.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Pkgs/HideExpansionPacksGameIcons.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/HideExpansionPacksGameIcons.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Pkgs/MasterController.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/MasterController.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Pkgs/MasterController_Cheats.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/MasterController_Cheats.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Pkgs/MasterController_Integration.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/MasterController_Integration.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Pkgs/MemoriesDisabled.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/MemoriesDisabled.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Pkgs/Overwatch.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/Overwatch.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Pkgs/Overwatch_Tuning.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/Overwatch_Tuning.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Pkgs/Register.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/Register.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Pkgs/Register_Tuning.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/Register_Tuning.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Pkgs/SmoothPatch.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/SmoothPatch.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Pkgs/SmoothPatch_MasterController.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/SmoothPatch_MasterController.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Pkgs/Traffic.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/Traffic.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Pkgs/Traffic_Tuning.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/Traffic_Tuning.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Pkgs/Traveler.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/Traveler.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Pkgs/Traveler_Tuning.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/Traveler_Tuning.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Pkgs/GoHere.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/GoHere.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Pkgs/GoHere_Tuning.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/GoHere_Tuning.package"));
                        Directory.Delete((myDocumentsPath + "/!DL-TmpCache/Pkgs"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Root/ddraw.dll"), (Directory.GetCurrentDirectory() + @"/ddraw.dll"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Root/TS3Patch.asi"), (Directory.GetCurrentDirectory() + @"/TS3Patch.asi"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/Root/TS3Patch.txt"), (Directory.GetCurrentDirectory() + @"/TS3Patch.txt"));
                        Directory.Delete((myDocumentsPath + "/!DL-TmpCache/Root"));

                        //Return a success response
                        return new string[] { "success", (startParams[0]) };
                    }
                    catch (Exception ex)
                    {
                        //Return a error response
                        return new string[] { "error", (startParams[0]) };
                    }

                    //Finish the thread...
                    return new string[] { "none", (startParams[0]) };
                };
                asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of success, inform it
                    if (threadTaskResponse == "success")
                    {
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallSuccess").Replace("%name%", GetStringApplicationResource("launcher_patch3_title"))), ToastType.Success);
                        launcherPrefs.loadedData.patchBasicOptimization = true;
                        launcherPrefs.Save();

                        //Request the restart
                        restartPopUp.Visibility = Visibility.Visible;
                    }
                    //If have a response different from success, inform error
                    if (threadTaskResponse != "success")
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallError").Replace("%name%", GetStringApplicationResource("launcher_patch3_title"))), ToastType.Error);

                    //Update the UI
                    if (launcherPrefs.loadedData.patchBasicOptimization == true)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    if (launcherPrefs.loadedData.patchBasicOptimization == false)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);

                    //Remove the task from queue
                    RemoveTask("patch_basicOptimization");
                };
                asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }
        }

        private void DoPatch_FpsLimiter(PatchMode doPatchMode, int instantiatedPatchItemInList)
        {
            //
            //
            //
            //INTEGRITY CHECK...
            //
            //
            //

            //If is desired to only check integrity
            if (doPatchMode == PatchMode.CheckIntegrity)
            {
                //Add the task to queue
                AddTask("patch_fpsLimiter_integrity", "Checking patch.");

                //--- Start a thread to check patch integrity ---//
                AsyncTaskSimplified aTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                aTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(1000);
                    //Prepare the response to return
                    string toReturn = "ok";

                    //Prepare a list of files
                    List<string> filesToCheck = new List<string>();
                    filesToCheck.Add((Directory.GetCurrentDirectory() + @"/antilag.cfg"));
                    filesToCheck.Add((Directory.GetCurrentDirectory() + @"/d3d9.dll"));

                    //Check integrity of this patch...
                    foreach (string filePath in filesToCheck)
                        if (File.Exists(filePath) == false)
                        {
                            toReturn = "problemFound";
                            break;
                        }

                    //Finish the thread...
                    return new string[] { toReturn, (startParams[0]) };
                };
                aTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of problem, inform it
                    if (threadTaskResponse == "problemFound")
                    {
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.InstalledWithProblem);
                        ShowToast(GetStringApplicationResource("launcher_patches_problemFound"), ToastType.Error);
                        EnablePageRedDotNotification(LauncherPage.patches);
                    }

                    //Remove the task from queue
                    RemoveTask("patch_fpsLimiter_integrity");
                };
                aTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }

            //
            //
            //
            //INSTALLING...
            //
            //
            //

            //If is desired to install
            if (doPatchMode == PatchMode.Install)
            {
                //--- Start a thread to do the patching ---//
                AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) =>
                {
                    //Get the instantiated patch item in list
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(startParams[0]))];
                    //Change the UI
                    instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installing);
                    //Add the task to queue
                    AddTask("patch_fpsLimiter", "Installing patch.");
                };
                asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(5000);

                    //Try to do the task
                    try
                    {
                        //Prepare a list of files
                        List<string> filesToCheck = new List<string>();
                        filesToCheck.Add((Directory.GetCurrentDirectory() + @"/antilag.cfg"));
                        filesToCheck.Add((Directory.GetCurrentDirectory() + @"/d3d9.dll"));
                        //Remove all files, if exists
                        foreach (string filePath in filesToCheck)
                            if (File.Exists(filePath) == true)
                                File.Delete(filePath);

                        //Download the patch files...
                        //Prepare the target download URL
                        string downloadUrl = @"https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/patch-fps-limiter.zip";
                        string saveAsPath = (myDocumentsPath + @"/!DL-TmpCache/patch-fps-limiter.zip");
                        //Download the "Mods" folder sync
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

                        //Extract the downloaded patch files
                        ZipFile zipFile = ZipFile.Read(saveAsPath);
                        foreach (ZipEntry entry in zipFile)
                            entry.Extract((myDocumentsPath + @"/!DL-TmpCache"), ExtractExistingFileAction.OverwriteSilently);
                        zipFile.Dispose();

                        //Put the files in the right place
                        File.Move((myDocumentsPath + "/!DL-TmpCache/antilag.cfg"), (Directory.GetCurrentDirectory() + @"/antilag.cfg"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/d3d9.dll"), (Directory.GetCurrentDirectory() + @"/d3d9.dll"));

                        //Return a success response
                        return new string[] { "success", (startParams[0]) };
                    }
                    catch (Exception ex)
                    {
                        //Return a error response
                        return new string[] { "error", (startParams[0]) };
                    }

                    //Finish the thread...
                    return new string[] { "none", (startParams[0]) };
                };
                asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of success, inform it
                    if (threadTaskResponse == "success")
                    {
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallSuccess").Replace("%name%", GetStringApplicationResource("launcher_patch4_title"))), ToastType.Success);
                        launcherPrefs.loadedData.patchFpsLimiter = true;
                        launcherPrefs.Save();

                        //Request the restart
                        restartPopUp.Visibility = Visibility.Visible;
                    }
                    //If have a response different from success, inform error
                    if (threadTaskResponse != "success")
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallError").Replace("%name%", GetStringApplicationResource("launcher_patch4_title"))), ToastType.Error);

                    //Update the UI
                    if (launcherPrefs.loadedData.patchFpsLimiter == true)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    if (launcherPrefs.loadedData.patchFpsLimiter == false)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);

                    //Remove the task from queue
                    RemoveTask("patch_fpsLimiter");
                };
                asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }
        }

        private void DoPatch_GpuCpuUpdate(PatchMode doPatchMode, int instantiatedPatchItemInList)
        {
            //
            //
            //
            //INTEGRITY CHECK...
            //
            //
            //

            //If is desired to only check integrity
            if (doPatchMode == PatchMode.CheckIntegrity)
            {
                //Add the task to queue
                AddTask("patch_cpuGpuUpdate_integrity", "Checking patch.");

                //--- Start a thread to check patch integrity ---//
                AsyncTaskSimplified aTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                aTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(1000);
                    //Prepare the response to return
                    string toReturn = "ok";

                    //If files exists...
                    if (File.Exists((Directory.GetCurrentDirectory() + @"/GraphicsCards.sgr")) == true && File.Exists((Directory.GetCurrentDirectory() + @"/GraphicsRules.sgr")) == true)
                    {
                        //Read "GraphicsCards.sgr" file..
                        string graphicsCardsSgr = File.ReadAllText((Directory.GetCurrentDirectory() + @"/GraphicsCards.sgr"));
                        if(graphicsCardsSgr.Contains("GeForce RTX 3060 Ti") == false)
                            toReturn = "problemFound";

                        //Read "GraphicsRules.sgr" file...
                        string graphicsRulesSgr = File.ReadAllText((Directory.GetCurrentDirectory() + @"/GraphicsRules.sgr"));
                        if(graphicsRulesSgr.Contains("seti cpuLevelLow        3") == false)
                            toReturn = "problemFound";
                    }
                    //If files don't exists, inform error
                    if (File.Exists((Directory.GetCurrentDirectory() + @"/GraphicsCards.sgr")) == false || File.Exists((Directory.GetCurrentDirectory() + @"/GraphicsRules.sgr")) == false)
                        toReturn = "problemFound";

                    //Finish the thread...
                    return new string[] { toReturn, (startParams[0]) };
                };
                aTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of problem, inform it
                    if (threadTaskResponse == "problemFound")
                    {
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.InstalledWithProblem);
                        ShowToast(GetStringApplicationResource("launcher_patches_problemFound"), ToastType.Error);
                        EnablePageRedDotNotification(LauncherPage.patches);
                    }

                    //Remove the task from queue
                    RemoveTask("patch_cpuGpuUpdate_integrity");
                };
                aTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }

            //
            //
            //
            //INSTALLING...
            //
            //
            //

            //If is desired to install
            if (doPatchMode == PatchMode.Install)
            {
                //--- Start a thread to do the patching ---//
                AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) =>
                {
                    //Get the instantiated patch item in list
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(startParams[0]))];
                    //Change the UI
                    instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installing);
                    //Add the task to queue
                    AddTask("patch_cpuGpuUpdate", "Installing patch.");
                };
                asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(5000);

                    //Try to do the task
                    try
                    {
                        //If don't have backup files, create it
                        if (File.Exists((Directory.GetCurrentDirectory() + @"/GraphicsCards-backup.sgr")) == false)
                            File.Copy((Directory.GetCurrentDirectory() + @"/GraphicsCards.sgr"), (Directory.GetCurrentDirectory() + @"/GraphicsCards-backup.sgr"));
                        if (File.Exists((Directory.GetCurrentDirectory() + @"/GraphicsRules-backup.sgr")) == false)
                            File.Copy((Directory.GetCurrentDirectory() + @"/GraphicsRules.sgr"), (Directory.GetCurrentDirectory() + @"/GraphicsRules-backup.sgr"));

                        //Prepare a list of files
                        List<string> filesToCheck = new List<string>();
                        filesToCheck.Add((Directory.GetCurrentDirectory() + @"/GraphicsCards.sgr"));
                        filesToCheck.Add((Directory.GetCurrentDirectory() + @"/GraphicsRules.sgr"));
                        //Remove all files, if exists
                        foreach (string filePath in filesToCheck)
                            if (File.Exists(filePath) == true)
                                File.Delete(filePath);

                        //Download the patch files...
                        //Prepare the target download URL
                        string downloadUrl = @"https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/patch-gpu-cpu-update.zip";
                        string saveAsPath = (myDocumentsPath + @"/!DL-TmpCache/patch-gpu-cpu-update.zip");
                        //Download the "Mods" folder sync
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

                        //Extract the downloaded patch files
                        ZipFile zipFile = ZipFile.Read(saveAsPath);
                        foreach (ZipEntry entry in zipFile)
                            entry.Extract((myDocumentsPath + @"/!DL-TmpCache"), ExtractExistingFileAction.OverwriteSilently);
                        zipFile.Dispose();

                        //Put the files in the right place
                        File.Move((myDocumentsPath + "/!DL-TmpCache/GraphicsCards.sgr"), (Directory.GetCurrentDirectory() + @"/GraphicsCards.sgr"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/GraphicsRules.sgr"), (Directory.GetCurrentDirectory() + @"/GraphicsRules.sgr"));

                        //Delete the "DeviceConfig.log" file if exists
                        if (File.Exists((myDocumentsPath + "/DeviceConfig.log")) == true)
                            File.Delete((myDocumentsPath + "/DeviceConfig.log"));

                        //Return a success response
                        return new string[] { "success", (startParams[0]) };
                    }
                    catch (Exception ex)
                    {
                        //Return a error response
                        return new string[] { "error", (startParams[0]) };
                    }

                    //Finish the thread...
                    return new string[] { "none", (startParams[0]) };
                };
                asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of success, inform it
                    if (threadTaskResponse == "success")
                    {
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallSuccess").Replace("%name%", GetStringApplicationResource("launcher_patch5_title"))), ToastType.Success);
                        launcherPrefs.loadedData.patchCpuGpuUpdate = true;
                        launcherPrefs.Save();
                    }
                    //If have a response different from success, inform error
                    if (threadTaskResponse != "success")
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallError").Replace("%name%", GetStringApplicationResource("launcher_patch5_title"))), ToastType.Error);

                    //Update the UI
                    if (launcherPrefs.loadedData.patchCpuGpuUpdate == true)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    if (launcherPrefs.loadedData.patchCpuGpuUpdate == false)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);

                    //Remove the task from queue
                    RemoveTask("patch_cpuGpuUpdate");
                };
                asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }
        }

        private void DoPatch_BetterGlobalIllumination(PatchMode doPatchMode, int instantiatedPatchItemInList)
        {
            //
            //
            //
            //INTEGRITY CHECK...
            //
            //
            //

            //If is desired to only check integrity
            if (doPatchMode == PatchMode.CheckIntegrity)
            {
                //Add the task to queue
                AddTask("patch_betterGlobalIllumination_integrity", "Checking patch.");

                //--- Start a thread to check patch integrity ---//
                AsyncTaskSimplified aTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                aTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(1000);
                    //Prepare the response to return
                    string toReturn = "ok";

                    //Prepare a list of files
                    List<string> filesToCheck = new List<string>();
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/AutoLightsOverhaul.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/AutoLightsOverhaulCommon.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/BoringBonesFixedLightning.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/ImprovedEnvironmentalShadows.package"));

                    //Check integrity of this patch...
                    foreach (string filePath in filesToCheck)
                        if (File.Exists(filePath) == false)
                        {
                            toReturn = "problemFound";
                            break;
                        }

                    //Finish the thread...
                    return new string[] { toReturn, (startParams[0]) };
                };
                aTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of problem, inform it
                    if (threadTaskResponse == "problemFound")
                    {
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.InstalledWithProblem);
                        ShowToast(GetStringApplicationResource("launcher_patches_problemFound"), ToastType.Error);
                        EnablePageRedDotNotification(LauncherPage.patches);
                    }

                    //Remove the task from queue
                    RemoveTask("patch_betterGlobalIllumination_integrity");
                };
                aTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }

            //
            //
            //
            //INSTALLING...
            //
            //
            //

            //If is desired to install
            if (doPatchMode == PatchMode.Install)
            {
                //--- Start a thread to do the patching ---//
                AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) =>
                {
                    //Get the instantiated patch item in list
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(startParams[0]))];
                    //Change the UI
                    instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installing);
                    //Add the task to queue
                    AddTask("patch_betterGlobalIllumination", "Installing patch.");
                };
                asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(5000);

                    //Try to do the task
                    try
                    {
                        //Prepare a list of files
                        List<string> filesToCheck = new List<string>();
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/AutoLightsOverhaul.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/AutoLightsOverhaulCommon.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/BoringBonesFixedLightning.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/ImprovedEnvironmentalShadows.package"));
                        //Remove all files, if exists
                        foreach (string filePath in filesToCheck)
                            if (File.Exists(filePath) == true)
                                File.Delete(filePath);

                        //Download the patch files...
                        //Prepare the target download URL
                        string downloadUrl = @"https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/patch-better-global-ilumination.zip";
                        string saveAsPath = (myDocumentsPath + @"/!DL-TmpCache/patch-better-global-ilumination.zip");
                        //Download the "Mods" folder sync
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

                        //Extract the downloaded patch files
                        ZipFile zipFile = ZipFile.Read(saveAsPath);
                        foreach (ZipEntry entry in zipFile)
                            entry.Extract((myDocumentsPath + @"/!DL-TmpCache"), ExtractExistingFileAction.OverwriteSilently);
                        zipFile.Dispose();

                        //Put the files in the right place
                        File.Move((myDocumentsPath + "/!DL-TmpCache/AutoLightsOverhaul.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/AutoLightsOverhaul.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/AutoLightsOverhaulCommon.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/AutoLightsOverhaulCommon.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/BoringBonesFixedLightning.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/BoringBonesFixedLightning.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/ImprovedEnvironmentalShadows.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/ImprovedEnvironmentalShadows.package"));

                        //Return a success response
                        return new string[] { "success", (startParams[0]) };
                    }
                    catch (Exception ex)
                    {
                        //Return a error response
                        return new string[] { "error", (startParams[0]) };
                    }

                    //Finish the thread...
                    return new string[] { "none", (startParams[0]) };
                };
                asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of success, inform it
                    if (threadTaskResponse == "success")
                    {
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallSuccess").Replace("%name%", GetStringApplicationResource("launcher_patch6_title"))), ToastType.Success);
                        launcherPrefs.loadedData.patchBetterGlobalIllumination = true;
                        launcherPrefs.Save();
                    }
                    //If have a response different from success, inform error
                    if (threadTaskResponse != "success")
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallError").Replace("%name%", GetStringApplicationResource("launcher_patch6_title"))), ToastType.Error);

                    //Update the UI
                    if (launcherPrefs.loadedData.patchBetterGlobalIllumination == true)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    if (launcherPrefs.loadedData.patchBetterGlobalIllumination == false)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);

                    //Remove the task from queue
                    RemoveTask("patch_betterGlobalIllumination");
                };
                asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }
        }

        private void DoPatch_ImprovedShaders(PatchMode doPatchMode, int instantiatedPatchItemInList)
        {
            //
            //
            //
            //INTEGRITY CHECK...
            //
            //
            //

            //If is desired to only check integrity
            if (doPatchMode == PatchMode.CheckIntegrity)
            {
                //Add the task to queue
                AddTask("patch_improvedShader_integrity", "Checking patch.");

                //--- Start a thread to check patch integrity ---//
                AsyncTaskSimplified aTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                aTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(1000);
                    //Prepare the response to return
                    string toReturn = "ok";

                    //Prepare a list of files
                    List<string> filesToCheck = new List<string>();
                    filesToCheck.Add((Directory.GetCurrentDirectory() + @"/D3DShaderReplacer.asi"));
                    filesToCheck.Add((Directory.GetCurrentDirectory() + @"/D3DShaderReplacer.cfg"));
                    filesToCheck.Add((Directory.GetCurrentDirectory() + @"/ddraw.dll"));
                    //Check integrity of this patch...
                    foreach (string filePath in filesToCheck)
                        if (File.Exists(filePath) == false)
                        {
                            toReturn = "problemFound";
                            break;
                        }

                    //Prepare a list of folders
                    List<string> foldersToCheck = new List<string>();
                    foldersToCheck.Add((Directory.GetCurrentDirectory() + @"/shader_replace"));
                    foldersToCheck.Add((Directory.GetCurrentDirectory() + @"/shader_textures"));
                    //Check integrity of this patch...
                    foreach (string folderPath in foldersToCheck)
                        if (Directory.Exists(folderPath) == false)
                        {
                            toReturn = "problemFound";
                            break;
                        }

                    //Finish the thread...
                    return new string[] { toReturn, (startParams[0]) };
                };
                aTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of problem, inform it
                    if (threadTaskResponse == "problemFound")
                    {
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.InstalledWithProblem);
                        ShowToast(GetStringApplicationResource("launcher_patches_problemFound"), ToastType.Error);
                        EnablePageRedDotNotification(LauncherPage.patches);
                    }

                    //Remove the task from queue
                    RemoveTask("patch_improvedShader_integrity");
                };
                aTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }

            //
            //
            //
            //INSTALLING...
            //
            //
            //

            //If is desired to install
            if (doPatchMode == PatchMode.Install)
            {
                //--- Start a thread to do the patching ---//
                AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) =>
                {
                    //Get the instantiated patch item in list
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(startParams[0]))];
                    //Change the UI
                    instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installing);
                    //Add the task to queue
                    AddTask("patch_improvedShader", "Installing patch.");
                };
                asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(5000);

                    //Try to do the task
                    try
                    {
                        //Prepare a list of files
                        List<string> filesToCheck = new List<string>();
                        filesToCheck.Add((Directory.GetCurrentDirectory() + @"/D3DShaderReplacer.asi"));
                        filesToCheck.Add((Directory.GetCurrentDirectory() + @"/D3DShaderReplacer.cfg"));
                        foreach (string filePath in filesToCheck)
                            if (File.Exists(filePath) == true)
                                File.Delete(filePath);
                        //Prepare a list of folders
                        List<string> foldersToCheck = new List<string>();
                        foldersToCheck.Add((Directory.GetCurrentDirectory() + @"/shader_replace"));
                        foldersToCheck.Add((Directory.GetCurrentDirectory() + @"/shader_textures"));
                        foreach (string folderPath in foldersToCheck)
                            if (Directory.Exists(folderPath) == true)
                                Directory.Delete(folderPath, true);

                        //Download the patch files...
                        //Prepare the target download URL
                        string downloadUrl = @"https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/patch-pixel-and-shader-tweaks.zip";
                        string saveAsPath = (myDocumentsPath + @"/!DL-TmpCache/patch-pixel-and-shader-tweaks.zip");
                        //Download the "Mods" folder sync
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

                        //Extract the downloaded patch files
                        ZipFile zipFile = ZipFile.Read(saveAsPath);
                        foreach (ZipEntry entry in zipFile)
                            entry.Extract((myDocumentsPath + @"/!DL-TmpCache"), ExtractExistingFileAction.OverwriteSilently);
                        zipFile.Dispose();

                        //Put the files in the right place
                        if (File.Exists((Directory.GetCurrentDirectory() + @"/ddraw.dll")) == false)
                            File.Move((myDocumentsPath + "/!DL-TmpCache/ddraw.dll"), (Directory.GetCurrentDirectory() + @"/ddraw.dll"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/D3DShaderReplacer.asi"), (Directory.GetCurrentDirectory() + @"/D3DShaderReplacer.asi"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/D3DShaderReplacer.cfg"), (Directory.GetCurrentDirectory() + @"/D3DShaderReplacer.cfg"));
                        Directory.CreateDirectory((Directory.GetCurrentDirectory() + @"/shader_replace"));
                        foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/!DL-TmpCache/shader_replace")).GetFiles()))
                            File.Copy(file.FullName, (Directory.GetCurrentDirectory() + @"/shader_replace/" + file.Name));
                        Directory.Delete((myDocumentsPath + "/!DL-TmpCache/shader_replace"), true);
                        Directory.CreateDirectory((Directory.GetCurrentDirectory() + @"/shader_textures"));
                        foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/!DL-TmpCache/shader_textures")).GetFiles()))
                            File.Copy(file.FullName, (Directory.GetCurrentDirectory() + @"/shader_textures/" + file.Name));
                        Directory.Delete((myDocumentsPath + "/!DL-TmpCache/shader_textures"), true);

                        //Return a success response
                        return new string[] { "success", (startParams[0]) };
                    }
                    catch (Exception ex)
                    {
                        //Return a error response
                        return new string[] { "error", (startParams[0]) };
                    }

                    //Finish the thread...
                    return new string[] { "none", (startParams[0]) };
                };
                asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of success, inform it
                    if (threadTaskResponse == "success")
                    {
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallSuccess").Replace("%name%", GetStringApplicationResource("launcher_patch7_title"))), ToastType.Success);
                        launcherPrefs.loadedData.patchImprovedShaders = true;
                        launcherPrefs.Save();
                    }
                    //If have a response different from success, inform error
                    if (threadTaskResponse != "success")
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallError").Replace("%name%", GetStringApplicationResource("launcher_patch7_title"))), ToastType.Error);

                    //Update the UI
                    if (launcherPrefs.loadedData.patchImprovedShaders == true)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    if (launcherPrefs.loadedData.patchImprovedShaders == false)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);

                    //Remove the task from queue
                    RemoveTask("patch_improvedShader");
                };
                asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }
        }

        private void DoPatch_ShadowExtender(PatchMode doPatchMode, int instantiatedPatchItemInList)
        {
            //
            //
            //
            //INTEGRITY CHECK...
            //
            //
            //

            //If is desired to only check integrity
            if (doPatchMode == PatchMode.CheckIntegrity)
            {
                //Add the task to queue
                AddTask("patch_shadowExtender_integrity", "Checking patch.");

                //--- Start a thread to check patch integrity ---//
                AsyncTaskSimplified aTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                aTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(1000);
                    //Prepare the response to return
                    string toReturn = "ok";

                    //Prepare a list of files
                    List<string> filesToCheck = new List<string>();
                    filesToCheck.Add((Directory.GetCurrentDirectory() + @"/ShadowExtender.cfg"));
                    filesToCheck.Add((Directory.GetCurrentDirectory() + @"/TS3ShadowExtender.asi"));
                    filesToCheck.Add((Directory.GetCurrentDirectory() + @"/ddraw.dll"));
                    //Check integrity of this patch...
                    foreach (string filePath in filesToCheck)
                        if (File.Exists(filePath) == false)
                        {
                            toReturn = "problemFound";
                            break;
                        }

                    //Finish the thread...
                    return new string[] { toReturn, (startParams[0]) };
                };
                aTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of problem, inform it
                    if (threadTaskResponse == "problemFound")
                    {
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.InstalledWithProblem);
                        ShowToast(GetStringApplicationResource("launcher_patches_problemFound"), ToastType.Error);
                        EnablePageRedDotNotification(LauncherPage.patches);
                    }

                    //Remove the task from queue
                    RemoveTask("patch_shadowExtender_integrity");
                };
                aTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }

            //
            //
            //
            //INSTALLING...
            //
            //
            //

            //If is desired to install
            if (doPatchMode == PatchMode.Install)
            {
                //--- Start a thread to do the patching ---//
                AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) =>
                {
                    //Get the instantiated patch item in list
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(startParams[0]))];
                    //Change the UI
                    instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installing);
                    //Add the task to queue
                    AddTask("patch_shadowExtender", "Installing patch.");
                };
                asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(5000);

                    //Try to do the task
                    try
                    {
                        //Prepare a list of files
                        List<string> filesToCheck = new List<string>();
                        filesToCheck.Add((Directory.GetCurrentDirectory() + @"/ShadowExtender.cfg"));
                        filesToCheck.Add((Directory.GetCurrentDirectory() + @"/TS3ShadowExtender.asi"));
                        foreach (string filePath in filesToCheck)
                            if (File.Exists(filePath) == true)
                                File.Delete(filePath);

                        //Download the patch files...
                        //Prepare the target download URL
                        string downloadUrl = @"https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/patch-shadow-extender.zip";
                        string saveAsPath = (myDocumentsPath + @"/!DL-TmpCache/patch-shadow-extender.zip");
                        //Download the "Mods" folder sync
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

                        //Extract the downloaded patch files
                        ZipFile zipFile = ZipFile.Read(saveAsPath);
                        foreach (ZipEntry entry in zipFile)
                            entry.Extract((myDocumentsPath + @"/!DL-TmpCache"), ExtractExistingFileAction.OverwriteSilently);
                        zipFile.Dispose();

                        //Put the files in the right place
                        if (File.Exists((Directory.GetCurrentDirectory() + @"/ddraw.dll")) == false)
                            File.Move((myDocumentsPath + "/!DL-TmpCache/ddraw.dll"), (Directory.GetCurrentDirectory() + @"/ddraw.dll"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/ShadowExtender.cfg"), (Directory.GetCurrentDirectory() + @"/ShadowExtender.cfg"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/TS3ShadowExtender.asi"), (Directory.GetCurrentDirectory() + @"/TS3ShadowExtender.asi"));

                        //Return a success response
                        return new string[] { "success", (startParams[0]) };
                    }
                    catch (Exception ex)
                    {
                        //Return a error response
                        return new string[] { "error", (startParams[0]) };
                    }

                    //Finish the thread...
                    return new string[] { "none", (startParams[0]) };
                };
                asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of success, inform it
                    if (threadTaskResponse == "success")
                    {
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallSuccess").Replace("%name%", GetStringApplicationResource("launcher_patch8_title"))), ToastType.Success);
                        launcherPrefs.loadedData.patchShadowExtender = true;
                        launcherPrefs.Save();
                    }
                    //If have a response different from success, inform error
                    if (threadTaskResponse != "success")
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallError").Replace("%name%", GetStringApplicationResource("launcher_patch8_title"))), ToastType.Error);

                    //Update the UI
                    if (launcherPrefs.loadedData.patchShadowExtender == true)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    if (launcherPrefs.loadedData.patchShadowExtender == false)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);

                    //Remove the task from queue
                    RemoveTask("patch_shadowExtender");
                };
                asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }
        }

        private void DoPatch_RoutingOptimizations(PatchMode doPatchMode, int instantiatedPatchItemInList)
        {
            //
            //
            //
            //INTEGRITY CHECK...
            //
            //
            //

            //If is desired to only check integrity
            if (doPatchMode == PatchMode.CheckIntegrity)
            {
                //Add the task to queue
                AddTask("patch_routesOptimization_integrity", "Checking patch.");

                //--- Start a thread to check patch integrity ---//
                AsyncTaskSimplified aTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                aTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(1000);
                    //Prepare the response to return
                    string toReturn = "ok";

                    //Prepare a list of files
                    List<string> filesToCheck = new List<string>();
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/BetterRoutingForGameObjects.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/NoFootTapping.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/NoWhiningMotives.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/RouteFixF4V9.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/NoRouteFailAnimation.package"));

                    //Check integrity of this patch...
                    foreach (string filePath in filesToCheck)
                        if (File.Exists(filePath) == false)
                        {
                            toReturn = "problemFound";
                            break;
                        }

                    //Finish the thread...
                    return new string[] { toReturn, (startParams[0]) };
                };
                aTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of problem, inform it
                    if (threadTaskResponse == "problemFound")
                    {
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.InstalledWithProblem);
                        ShowToast(GetStringApplicationResource("launcher_patches_problemFound"), ToastType.Error);
                        EnablePageRedDotNotification(LauncherPage.patches);
                    }

                    //Remove the task from queue
                    RemoveTask("patch_routesOptimization_integrity");
                };
                aTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }

            //
            //
            //
            //INSTALLING...
            //
            //
            //

            //If is desired to install
            if (doPatchMode == PatchMode.Install)
            {
                //--- Start a thread to do the patching ---//
                AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) =>
                {
                    //Get the instantiated patch item in list
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(startParams[0]))];
                    //Change the UI
                    instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installing);
                    //Add the task to queue
                    AddTask("patch_routesOptimizations", "Installing patch.");
                };
                asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(5000);

                    //Try to do the task
                    try
                    {
                        //Prepare a list of files
                        List<string> filesToCheck = new List<string>();
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/BetterRoutingForGameObjects.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/FasterElevatorMoving.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/FasterElevatorMoving.package.disabled"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/NoFootTapping.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/NoWhiningMotives.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/RouteFixF4V9.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/NoRouteFailAnimation.package"));
                        //Remove all files, if exists
                        foreach (string filePath in filesToCheck)
                            if (File.Exists(filePath) == true)
                                File.Delete(filePath);

                        //Download the patch files...
                        //Prepare the target download URL
                        string downloadUrl = @"https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/patch-routes-optimizations.zip";
                        string saveAsPath = (myDocumentsPath + @"/!DL-TmpCache/patch-routes-optimizations.zip");
                        //Download the "Mods" folder sync
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

                        //Extract the downloaded patch files
                        ZipFile zipFile = ZipFile.Read(saveAsPath);
                        foreach (ZipEntry entry in zipFile)
                            entry.Extract((myDocumentsPath + @"/!DL-TmpCache"), ExtractExistingFileAction.OverwriteSilently);
                        zipFile.Dispose();

                        //Put the files in the right place
                        if (Directory.Exists(((new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent).FullName + "/EP3")) == true)
                            File.Move((myDocumentsPath + "/!DL-TmpCache/FasterElevatorMoving.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/FasterElevatorMoving.package"));
                        if (Directory.Exists(((new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent).FullName + "/EP3")) == false)
                            File.Move((myDocumentsPath + "/!DL-TmpCache/FasterElevatorMoving.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/FasterElevatorMoving.package.disabled"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/BetterRoutingForGameObjects.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/BetterRoutingForGameObjects.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/NoFootTapping.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/NoFootTapping.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/NoWhiningMotives.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/NoWhiningMotives.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/RouteFixF4V9.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/RouteFixF4V9.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/NoRouteFailAnimation.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/NoRouteFailAnimation.package"));

                        //Return a success response
                        return new string[] { "success", (startParams[0]) };
                    }
                    catch (Exception ex)
                    {
                        //Return a error response
                        return new string[] { "error", (startParams[0]) };
                    }

                    //Finish the thread...
                    return new string[] { "none", (startParams[0]) };
                };
                asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of success, inform it
                    if (threadTaskResponse == "success")
                    {
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallSuccess").Replace("%name%", GetStringApplicationResource("launcher_patch9_title"))), ToastType.Success);
                        launcherPrefs.loadedData.patchRoutingOptimizations = true;
                        launcherPrefs.Save();
                    }
                    //If have a response different from success, inform error
                    if (threadTaskResponse != "success")
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallError").Replace("%name%", GetStringApplicationResource("launcher_patch9_title"))), ToastType.Error);

                    //Update the UI
                    if (launcherPrefs.loadedData.patchRoutingOptimizations == true)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    if (launcherPrefs.loadedData.patchRoutingOptimizations == false)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);

                    //Remove the task from queue
                    RemoveTask("patch_routesOptimizations");
                };
                asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }
        }

        private void DoPatch_BetterStoryProgression(PatchMode doPatchMode, int instantiatedPatchItemInList)
        {
            //
            //
            //
            //INTEGRITY CHECK...
            //
            //
            //

            //If is desired to only check integrity
            if (doPatchMode == PatchMode.CheckIntegrity)
            {
                //Add the task to queue
                AddTask("patch_betterStoryProgression_integrity", "Checking patch.");

                //--- Start a thread to check patch integrity ---//
                AsyncTaskSimplified aTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                aTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(1000);
                    //Prepare the response to return
                    string toReturn = "ok";

                    //Prepare a list of files
                    List<string> filesToCheck = new List<string>();
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/SecondImage.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Tuning.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Meanies.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Lovers.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Career.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Extra.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Money.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Population.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Relationship.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Skill.package"));
                    filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_CopsAndRobbers.package"));

                    //Check integrity of this patch...
                    foreach (string filePath in filesToCheck)
                        if (File.Exists(filePath) == false)
                        {
                            toReturn = "problemFound";
                            break;
                        }

                    //Finish the thread...
                    return new string[] { toReturn, (startParams[0]) };
                };
                aTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of problem, inform it
                    if (threadTaskResponse == "problemFound")
                    {
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.InstalledWithProblem);
                        ShowToast(GetStringApplicationResource("launcher_patches_problemFound"), ToastType.Error);
                        EnablePageRedDotNotification(LauncherPage.patches);
                    }

                    //Remove the task from queue
                    RemoveTask("patch_betterStoryProgression_integrity");
                };
                aTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }

            //
            //
            //
            //INSTALLING...
            //
            //
            //

            //If is desired to install
            if (doPatchMode == PatchMode.Install)
            {
                //--- Start a thread to do the patching ---//
                AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) =>
                {
                    //Get the instantiated patch item in list
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(startParams[0]))];
                    //Change the UI
                    instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installing);
                    //Add the task to queue
                    AddTask("patch_betterStoryProgression", "Installing patch.");
                };
                asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(5000);

                    //Try to do the task
                    try
                    {
                        //Prepare a list of files
                        List<string> filesToCheck = new List<string>();
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/SecondImage.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Tuning.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Meanies.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Lovers.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Career.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_FairiesAndWerewolves.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_FairiesAndWerewolves.package.disabled"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Extra.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Money.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Population.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Relationship.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Skill.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_CopsAndRobbers.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_VampiresAndSlayers.package"));
                        filesToCheck.Add((myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_VampiresAndSlayers.package.disabled"));
                        //Remove all files, if exists
                        foreach (string filePath in filesToCheck)
                            if (File.Exists(filePath) == true)
                                File.Delete(filePath);

                        //Download the patch files...
                        //Prepare the target download URL
                        string downloadUrl = @"https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/patch-better-story-progression.zip";
                        string saveAsPath = (myDocumentsPath + @"/!DL-TmpCache/patch-better-story-progression.zip");
                        //Download the "Mods" folder sync
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

                        //Extract the downloaded patch files
                        ZipFile zipFile = ZipFile.Read(saveAsPath);
                        foreach (ZipEntry entry in zipFile)
                            entry.Extract((myDocumentsPath + @"/!DL-TmpCache"), ExtractExistingFileAction.OverwriteSilently);
                        zipFile.Dispose();

                        //Put the files in the right place
                        File.Move((myDocumentsPath + "/!DL-TmpCache/SecondImage.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/SecondImage.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/StoryProgression.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/StoryProgression_Tuning.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Tuning.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/StoryProgression_Meanies.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Meanies.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/StoryProgression_Lovers.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Lovers.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/StoryProgression_Career.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Career.package"));
                        if (Directory.Exists(((new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent).FullName + "/EP7")) == true)
                            File.Move((myDocumentsPath + "/!DL-TmpCache/StoryProgression_FairiesAndWerewolves.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_FairiesAndWerewolves.package"));
                        if (Directory.Exists(((new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent).FullName + "/EP7")) == false)
                            File.Move((myDocumentsPath + "/!DL-TmpCache/StoryProgression_FairiesAndWerewolves.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_FairiesAndWerewolves.package.disabled"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/StoryProgression_Extra.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Extra.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/StoryProgression_Money.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Money.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/StoryProgression_Population.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Population.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/StoryProgression_Relationship.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Relationship.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/StoryProgression_Skill.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_Skill.package"));
                        File.Move((myDocumentsPath + "/!DL-TmpCache/StoryProgression_CopsAndRobbers.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_CopsAndRobbers.package"));
                        if (Directory.Exists(((new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent).FullName + "/EP7")) == true)
                            File.Move((myDocumentsPath + "/!DL-TmpCache/StoryProgression_VampiresAndSlayers.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_VampiresAndSlayers.package"));
                        if (Directory.Exists(((new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent).FullName + "/EP7")) == false)
                            File.Move((myDocumentsPath + "/!DL-TmpCache/StoryProgression_VampiresAndSlayers.package"), (myDocumentsPath + "/Mods/Packages/DL3-Patches/StoryProgression_VampiresAndSlayers.package.disabled"));

                        //Return a success response
                        return new string[] { "success", (startParams[0]) };
                    }
                    catch (Exception ex)
                    {
                        //Return a error response
                        return new string[] { "error", (startParams[0]) };
                    }

                    //Finish the thread...
                    return new string[] { "none", (startParams[0]) };
                };
                asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of success, inform it
                    if (threadTaskResponse == "success")
                    {
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallSuccess").Replace("%name%", GetStringApplicationResource("launcher_patch11_title"))), ToastType.Success);
                        launcherPrefs.loadedData.patchStoryProgression = true;
                        launcherPrefs.Save();
                    }

                    //If have a response different from success, inform error
                    if (threadTaskResponse != "success")
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallError").Replace("%name%", GetStringApplicationResource("launcher_patch11_title"))), ToastType.Error);

                    //Update the UI
                    if (launcherPrefs.loadedData.patchStoryProgression == true)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    if (launcherPrefs.loadedData.patchStoryProgression == false)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);

                    //Remove the task from queue
                    RemoveTask("patch_betterStoryProgression");
                };
                asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }
        }

        private void DoPatch_InternetRemoval(PatchMode doPatchMode, int instantiatedPatchItemInList)
        {
            //
            //
            //
            //INTEGRITY CHECK...
            //
            //
            //

            //If is desired to only check integrity
            if (doPatchMode == PatchMode.CheckIntegrity)
            {
                //Add the task to queue
                AddTask("patch_internetRemoval_integrity", "Checking patch.");

                //--- Start a thread to check patch integrity ---//
                AsyncTaskSimplified aTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                aTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(1000);
                    //Prepare the response to return
                    string toReturn = "ok";

                    //Check if have the rule "The Sims 3 - DL3 Block Patch"
                    bool haveRule = false;
                    //Check all rules
                    INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                    foreach (INetFwRule rule in firewallPolicy.Rules)
                        if (rule.Name == "The Sims 3 - DL3 Block Patch")
                        {
                            haveRule = true;
                            break;
                        }
                    //If don't have the rule, inform error
                    if(haveRule == false)
                        toReturn = "problemFound";

                    //Finish the thread...
                    return new string[] { toReturn, (startParams[0]) };
                };
                aTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of problem, inform it
                    if (threadTaskResponse == "problemFound")
                    {
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.InstalledWithProblem);
                        ShowToast(GetStringApplicationResource("launcher_patches_problemFound"), ToastType.Error);
                        EnablePageRedDotNotification(LauncherPage.patches);
                    }

                    //Remove the task from queue
                    RemoveTask("patch_internetRemoval_integrity");
                };
                aTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }

            //
            //
            //
            //INSTALLING...
            //
            //
            //

            //If is desired to install
            if (doPatchMode == PatchMode.Install)
            {
                //--- Start a thread to do the patching ---//
                AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { (instantiatedPatchItemInList.ToString()) });
                asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) =>
                {
                    //Get the instantiated patch item in list
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(startParams[0]))];
                    //Change the UI
                    instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installing);
                    //Add the task to queue
                    AddTask("patch_internetRemoval", "Installing patch.");
                };
                asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(1500);

                    //Try to do the task
                    try
                    {
                        //Check if have the rule "The Sims 3 - DL3 Block Patch"
                        bool haveRule = false;
                        //Check all rules
                        INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                        foreach (INetFwRule rule in firewallPolicy.Rules)
                            if (rule.Name == "The Sims 3 - DL3 Block Patch")
                            {
                                haveRule = true;
                                break;
                            }
                        //If the rule already exists, remove it
                        firewallPolicy.Rules.Remove("The Sims 3 - DL3 Block Patch");

                        //Prepare the new rule
                        INetFwRule newFirewallRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                        newFirewallRule.Name = "The Sims 3 - DL3 Block Patch";
                        newFirewallRule.InterfaceTypes = "All";
                        newFirewallRule.Enabled = true;
                        newFirewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
                        newFirewallRule.ApplicationName = (Directory.GetCurrentDirectory() + @"\TS3W.exe");
                        newFirewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
                        //Add the block rule for the windows firewall
                        firewallPolicy.Rules.Add(newFirewallRule);

                        //Return a success response
                        return new string[] { "success", (startParams[0]) };
                    }
                    catch (Exception ex)
                    {
                        //Return a error response
                        return new string[] { "error", (startParams[0]) };
                    }

                    //Finish the thread...
                    return new string[] { "none", (startParams[0]) };
                };
                asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
                {
                    //Get the instantiated patch item in list and thread response
                    PatchItem instantiatedPatchItemInList = ((MainWindow)callerWindow).instantiatedPatchItems[(int.Parse(backgroundResult[1]))];
                    string threadTaskResponse = backgroundResult[0];

                    //If have a response of success, inform it
                    if (threadTaskResponse == "success")
                    {
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallSuccess").Replace("%name%", GetStringApplicationResource("launcher_patch10_title"))), ToastType.Success);
                        launcherPrefs.loadedData.patchInternetRemoval = true;
                        launcherPrefs.Save();
                    }
                    //If have a response different from success, inform error
                    if (threadTaskResponse != "success")
                        ShowToast((GetStringApplicationResource("launcher_patches_statusInstallError").Replace("%name%", GetStringApplicationResource("launcher_patch10_title"))), ToastType.Error);

                    //Update the UI
                    if (launcherPrefs.loadedData.patchInternetRemoval == true)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.Installed);
                    if (launcherPrefs.loadedData.patchInternetRemoval == false)
                        instantiatedPatchItemInList.SetPatchStatus(PatchItem.PatchStatus.NotInstalled);

                    //Remove the task from queue
                    RemoveTask("patch_internetRemoval");
                };
                asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
            }
        }

        private void UpdatePatchesInstallAvailability()
        {
            //If not rendered all patchs in the list yet, ignore this call
            if (wasRenderedAllPatches == false)
                return;

            //Get quantity of running tasks
            int runningTasks = GetRunningTasksCount();

            //If have none task running, enable all patches install
            if (runningTasks == 0)
                SetEnabledAllPatchesButton(true);

            //If have tasks running, disable all patches install
            if (runningTasks > 0)
                SetEnabledAllPatchesButton(false);
        }

        private void SetEnabledAllPatchesButton(bool enabled)
        {
            //Scan all instantiated patches
            foreach (PatchItem item in instantiatedPatchItems)
                if(item.title.Text.Contains("Alder Lake") == false && item.title.Text.Contains("PT-BR") == false)   //<- Ignore patches of "Alder Lake+ Support" and "PT-PT to PT-BR Better Support"
                {
                    //If is desired to enable..
                    if(enabled == true)
                    {
                        item.installButton.IsEnabled = true;
                        item.reInstallButton.IsEnabled = true;
                    }

                    //If is desired to disable...
                    if (enabled == false)
                    {
                        item.installButton.IsEnabled = false;
                        item.reInstallButton.IsEnabled = false;
                    }
                }
        }

        //Exit manager

        private void CheckToExitFromLauncherAndWarnIfHaveTasksRunning()
        {
            //If not have a task running, just close the application
            if(GetRunningTasksCount() == 0)
            {
                System.Windows.Application.Current.Shutdown();
                return;
            }

            //If have a task running, warn before close the application
            if(GetRunningTasksCount() > 0)
            {
                //Display a dialog
                MessageBoxResult dialogResult = MessageBox.Show(GetStringApplicationResource("launcher_taskCloseWarnTxt"),
                                                                GetStringApplicationResource("launcher_taskCloseWarnTit"), MessageBoxButton.YesNo, MessageBoxImage.Warning);

                //If is desired to kill the launcher, do it
                if(dialogResult == MessageBoxResult.Yes)
                    System.Windows.Application.Current.Shutdown();
            }
        }

        //System tray manager

        private void BringTheWindowToFront()
        {
            //If the window is collapsed, ignore
            if (this.Visibility == Visibility.Collapsed)
                return;

            //If is minimized, restore it
            if(this.WindowState == WindowState.Minimized)
                this.WindowState = WindowState.Normal;

            //Bring the window to front, if is not collapsed
            this.Activate();
            this.Topmost = true;
            this.Topmost = false;
        }

        private void UpdateLauncherSystemTray()
        {
            //Prepare the context menu
            System.Windows.Forms.ContextMenuStrip contextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            if(isPlayingTheGame == true)
                contextMenuStrip.Items.Add("Kill Game", null, ((s, e) => { try { currentGameProcess.Kill(); } catch (Exception ex) {  } }));
            contextMenuStrip.Items.Add("Quit", null, ((s, e) => { CheckToExitFromLauncherAndWarnIfHaveTasksRunning(); }));

            //If is not playing
            if (isPlayingTheGame == false)
            {
                //If is busy, show the icon of busy
                if(GetRunningTasksCount() >= 1)
                    launcherTrayIcon.Icon = new System.Drawing.Icon(@"Content/tray-busy.ico");
                //If is not busy, show the icon of idle
                if (GetRunningTasksCount() == 0)
                    launcherTrayIcon.Icon = new System.Drawing.Icon(@"Content/tray-off.ico");
            }

            //If is playing
            if(isPlayingTheGame == true)
            {
                //Show the icon of playing
                launcherTrayIcon.Icon = new System.Drawing.Icon(@"Content/tray-on.ico");
            }

            //Update the context menu
            launcherTrayIcon.ContextMenuStrip = contextMenuStrip;
        }

        //DLCs manager

        private void CheckAvailableDLCs()
        {
            //Get the path to root of install
            string installPath = (new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent).FullName;

            //Prepare the list of EPs names
            List<string> epsNames = new List<string>();
            epsNames.Add("");
            epsNames.Add(GetStringApplicationResource("launcher_dlc_ep1"));
            epsNames.Add(GetStringApplicationResource("launcher_dlc_ep2"));
            epsNames.Add(GetStringApplicationResource("launcher_dlc_ep3"));
            epsNames.Add(GetStringApplicationResource("launcher_dlc_ep4"));
            epsNames.Add(GetStringApplicationResource("launcher_dlc_ep5"));
            epsNames.Add(GetStringApplicationResource("launcher_dlc_ep6"));
            epsNames.Add(GetStringApplicationResource("launcher_dlc_ep7"));
            epsNames.Add(GetStringApplicationResource("launcher_dlc_ep8"));
            epsNames.Add(GetStringApplicationResource("launcher_dlc_ep9"));
            epsNames.Add(GetStringApplicationResource("launcher_dlc_ep10"));
            epsNames.Add(GetStringApplicationResource("launcher_dlc_ep11"));
            //Prepare the list of EPs UI elements
            List<Image> epsIcons = new List<Image>();
            epsIcons.Add(null);
            epsIcons.Add(dlc_ep1);
            epsIcons.Add(dlc_ep2);
            epsIcons.Add(dlc_ep3);
            epsIcons.Add(dlc_ep4);
            epsIcons.Add(dlc_ep5);
            epsIcons.Add(dlc_ep6);
            epsIcons.Add(dlc_ep7);
            epsIcons.Add(dlc_ep8);
            epsIcons.Add(dlc_ep9);
            epsIcons.Add(dlc_ep10);
            epsIcons.Add(dlc_ep11);
            //Check each installed expansion
            for(int i = 1; i < epsNames.Count; i++)
            {
                //Compile informations about this EP
                Image dlcIcon = epsIcons[i];
                string dlcCode = ("EP" + i);
                string dlcName = epsNames[i];
                string dlcSkuInfoPath = (installPath + "/" + dlcCode + "/Game/Bin/skuversion.txt");

                //Prepare the EP icon in the UI
                if (File.Exists(dlcSkuInfoPath) == true)
                {
                    availableExpansionPacks[i] = true;
                    dlcIcon.ToolTip = (dlcName + " - v" + GetVersionBySkuContent(File.ReadAllLines(dlcSkuInfoPath)));
                    dlcIcon.Opacity = 0.8f;
                }
                if (File.Exists(dlcSkuInfoPath) == false)
                {
                    dlcIcon.ToolTip = dlcName;
                    dlcIcon.Opacity = 0.2f;
                }
            }
            //Clear the lists
            epsNames.Clear();
            epsIcons.Clear();



            //Prepare the list of SPs names
            List<string> spsNames = new List<string>();
            spsNames.Add("");
            spsNames.Add(GetStringApplicationResource("launcher_dlc_sp1"));
            spsNames.Add(GetStringApplicationResource("launcher_dlc_sp2"));
            spsNames.Add(GetStringApplicationResource("launcher_dlc_sp3"));
            spsNames.Add(GetStringApplicationResource("launcher_dlc_sp4"));
            spsNames.Add(GetStringApplicationResource("launcher_dlc_sp5"));
            spsNames.Add(GetStringApplicationResource("launcher_dlc_sp6"));
            spsNames.Add(GetStringApplicationResource("launcher_dlc_sp7"));
            spsNames.Add(GetStringApplicationResource("launcher_dlc_sp8"));
            spsNames.Add(GetStringApplicationResource("launcher_dlc_sp9"));
            //Prepare the list of SPs UI elements
            List<Image> spsIcons = new List<Image>();
            spsIcons.Add(null);
            spsIcons.Add(dlc_sp1);
            spsIcons.Add(dlc_sp2);
            spsIcons.Add(dlc_sp3);
            spsIcons.Add(dlc_sp4);
            spsIcons.Add(dlc_sp5);
            spsIcons.Add(dlc_sp6);
            spsIcons.Add(dlc_sp7);
            spsIcons.Add(dlc_sp8);
            spsIcons.Add(dlc_sp9);
            //Check each installed stuff pack
            for (int i = 1; i < spsNames.Count; i++)
            {
                //Compile informations about this SP
                Image dlcIcon = spsIcons[i];
                string dlcCode = ("SP" + i);
                string dlcName = spsNames[i];
                string dlcSkuInfoPath = (installPath + "/" + dlcCode + "/Game/Bin/skuversion.txt");

                //Prepare the SP icon in the UI
                if (File.Exists(dlcSkuInfoPath) == true)
                {
                    availableStuffPacks[i] = true;
                    dlcIcon.ToolTip = (dlcName + " - v" + GetVersionBySkuContent(File.ReadAllLines(dlcSkuInfoPath)));
                    dlcIcon.Opacity = 0.8f;
                }
                if (File.Exists(dlcSkuInfoPath) == false)
                {
                    dlcIcon.ToolTip = dlcName;
                    dlcIcon.Opacity = 0.2f;
                }
            }
            //Clear the lists
            spsNames.Clear();
            spsIcons.Clear();



            //Setup the icon viewer for each EP
            dlc_ep1.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_ep1"), "Resources/dlc_ep1.png"); };
            dlc_ep2.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_ep2"), "Resources/dlc_ep2.png"); };
            dlc_ep3.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_ep3"), "Resources/dlc_ep3.png"); };
            dlc_ep4.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_ep4"), "Resources/dlc_ep4.png"); };
            dlc_ep5.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_ep5"), "Resources/dlc_ep5.png"); };
            dlc_ep6.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_ep6"), "Resources/dlc_ep6.png"); };
            dlc_ep7.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_ep7"), "Resources/dlc_ep7.png"); };
            dlc_ep8.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_ep8"), "Resources/dlc_ep8.png"); };
            dlc_ep9.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_ep9"), "Resources/dlc_ep9.png"); };
            dlc_ep10.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_ep10"), "Resources/dlc_ep10.png"); };
            dlc_ep11.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_ep11"), "Resources/dlc_ep11.png"); };
            //Setup the icon viewer for each SP
            dlc_sp1.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_sp1"), "Resources/dlc_sp1.png"); };
            dlc_sp2.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_sp2"), "Resources/dlc_sp2.png"); };
            dlc_sp3.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_sp3"), "Resources/dlc_sp3.png"); };
            dlc_sp4.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_sp4"), "Resources/dlc_sp4.png"); };
            dlc_sp5.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_sp5"), "Resources/dlc_sp5.png"); };
            dlc_sp6.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_sp6"), "Resources/dlc_sp6.png"); };
            dlc_sp7.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_sp7"), "Resources/dlc_sp7.png"); };
            dlc_sp8.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_sp8"), "Resources/dlc_sp8.png"); };
            dlc_sp9.MouseDown += (o, e) => { OpenIconViewer(GetStringApplicationResource("launcher_dlc_sp9"), "Resources/dlc_sp9.png"); };
        }

        private string GetVersionBySkuContent(string[] skuFileLines)
        {
            //Prepare to return the version
            string version = "Unknown";

            //Search by "GameVersion" in all lines
            foreach (string line in skuFileLines)
                if (line.Contains("GameVersion") == true)
                {
                    //Get the game version
                    string filtredVersion = line.Replace(" ", "").Replace("GameVersion=", "");

                    //Inform the version
                    version = filtredVersion;

                    //Break the loop
                    break;
                }

            //Return the version
            return version;
        }

        private void OpenIconViewer(string titleToShow, string imageUriToShow)
        {
            //If have a icon viewer opened, close it
            if(currentOpenedIconViewer != null)
                currentOpenedIconViewer.Close();

            //Prepare the window to show
            currentOpenedIconViewer = new WindowIconViewer(titleToShow, imageUriToShow);
            currentOpenedIconViewer.Closed += (s, e) => { currentOpenedIconViewer = null; };
            currentOpenedIconViewer.Show();
        }

        private bool isExpansionPackInstalled(ExpansionPack expansionPack)
        {
            //Prepare to return
            bool toReturn = false;

            //Check if the desired expansion pack is installed...
            if (expansionPack == ExpansionPack.WorldAdventures)
                toReturn = isExpansionPackInstalled(1);
            if (expansionPack == ExpansionPack.Ambitions)
                toReturn = isExpansionPackInstalled(2);
            if (expansionPack == ExpansionPack.LateNight)
                toReturn = isExpansionPackInstalled(3);
            if (expansionPack == ExpansionPack.Generations)
                toReturn = isExpansionPackInstalled(4);
            if (expansionPack == ExpansionPack.Pets)
                toReturn = isExpansionPackInstalled(5);
            if (expansionPack == ExpansionPack.Showtime)
                toReturn = isExpansionPackInstalled(6);
            if (expansionPack == ExpansionPack.Supernatural)
                toReturn = isExpansionPackInstalled(7);
            if (expansionPack == ExpansionPack.Seasons)
                toReturn = isExpansionPackInstalled(8);
            if (expansionPack == ExpansionPack.UniversityLife)
                toReturn = isExpansionPackInstalled(9);
            if (expansionPack == ExpansionPack.IslandParadise)
                toReturn = isExpansionPackInstalled(10);
            if (expansionPack == ExpansionPack.IntoTheFuture)
                toReturn = isExpansionPackInstalled(11);

            //Return the response
            return toReturn;
        }

        private bool isExpansionPackInstalled(int expansionPackNumber)
        {
            //Prepare to return
            bool toReturn = false;

            //Check if the desired expansion pack is installed
            if(expansionPackNumber >= 1 && expansionPackNumber < 12)
                if (availableExpansionPacks[expansionPackNumber] == true)
                    toReturn = true;

            //Return the response
            return toReturn;
        }

        private bool isStuffPackInstalled(StuffPack stuffPack)
        {
            //Prepare to return
            bool toReturn = false;

            //Check if the desired stuff pack is installed...
            if (stuffPack == StuffPack.HighEndLoft)
                toReturn = isStuffPackInstalled(1);
            if (stuffPack == StuffPack.FastLane)
                toReturn = isStuffPackInstalled(2);
            if (stuffPack == StuffPack.OutdoorLiving)
                toReturn = isStuffPackInstalled(3);
            if (stuffPack == StuffPack.TownLife)
                toReturn = isStuffPackInstalled(4);
            if (stuffPack == StuffPack.MasterSuite)
                toReturn = isStuffPackInstalled(5);
            if (stuffPack == StuffPack.KatyPerry)
                toReturn = isStuffPackInstalled(6);
            if (stuffPack == StuffPack.Diesel)
                toReturn = isStuffPackInstalled(7);
            if (stuffPack == StuffPack.y70y80y90)
                toReturn = isStuffPackInstalled(8);
            if (stuffPack == StuffPack.Movie)
                toReturn = isStuffPackInstalled(9);

            //Return the response
            return toReturn;
        }

        private bool isStuffPackInstalled(int stuffPackNumber)
        {
            //Prepare to return
            bool toReturn = false;

            //Check if the desired stuff pack is installed
            if (stuffPackNumber >= 1 && stuffPackNumber < 10)
                if (availableStuffPacks[stuffPackNumber] == true)
                    toReturn = true;

            //Return the response
            return toReturn;
        }

        //Launcher tools

        private string GetLauncherVersion()
        {
            //Prepare te storage
            string version = "";

            //Get the version
            string[] versionNumbers = Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.');
            version = (versionNumbers[0] + "." + versionNumbers[1] + "." + versionNumbers[2]);

            //Return the version
            return version;
        }

        private string TryToDetermineTheSims3FolderPathInComputerMyDocuments()
        {
            //Prepare the value to return
            string path = "";

            //If don't defined a game language
            if(launcherPrefs.loadedData.gameLang == "undefined")
            {
                string ptTargetFolderPath = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Electronic Arts/Os Sims 3");
                if (File.Exists((ptTargetFolderPath + "/Options.ini")) == true)
                    path = ptTargetFolderPath;
                string enBrTargetFolderPath = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Electronic Arts/The Sims 3");
                if (File.Exists((enBrTargetFolderPath + "/Options.ini")) == true)
                    path = enBrTargetFolderPath;
            }
            //If "da-DK"
            if (launcherPrefs.loadedData.gameLang == "da-DK")
            {
                string targetFolderPath = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Electronic Arts/Sims 3");
                if (File.Exists((targetFolderPath + "/Options.ini")) == true)
                    path = targetFolderPath;
            }
            //If "nl-NL"
            if (launcherPrefs.loadedData.gameLang == "nl-NL")
            {
                string targetFolderPath = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Electronic Arts/De Sims 3");
                if (File.Exists((targetFolderPath + "/Options.ini")) == true)
                    path = targetFolderPath;
            }
            //If "en-GB"
            if (launcherPrefs.loadedData.gameLang == "en-GB")
            {
                string targetFolderPath = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Electronic Arts/The Sims 3");
                if (File.Exists((targetFolderPath + "/Options.ini")) == true)
                    path = targetFolderPath;
            }
            //If "en-US"
            if (launcherPrefs.loadedData.gameLang == "en-US")
            {
                string targetFolderPath = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Electronic Arts/The Sims 3");
                if (File.Exists((targetFolderPath + "/Options.ini")) == true)
                    path = targetFolderPath;
            }
            //If "fr-FR"
            if (launcherPrefs.loadedData.gameLang == "fr-FR")
            {
                string targetFolderPath = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Electronic Arts/Les Sims 3");
                if (File.Exists((targetFolderPath + "/Options.ini")) == true)
                    path = targetFolderPath;
            }
            //If "it-IT"
            if (launcherPrefs.loadedData.gameLang == "it-IT")
            {
                string targetFolderPath = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Electronic Arts/The Sims 3");
                if (File.Exists((targetFolderPath + "/Options.ini")) == true)
                    path = targetFolderPath;
            }
            //If "pt-PT"
            if (launcherPrefs.loadedData.gameLang == "pt-PT")
            {
                string targetFolderPath = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Electronic Arts/Os Sims 3");
                if (File.Exists((targetFolderPath + "/Options.ini")) == true)
                    path = targetFolderPath;
            }
            //If "pt-BR"
            if (launcherPrefs.loadedData.gameLang == "pt-BR")
            {
                string targetFolderPath = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Electronic Arts/The Sims 3");
                if (File.Exists((targetFolderPath + "/Options.ini")) == true)
                    path = targetFolderPath;
            }
            //If "es-ES"
            if (launcherPrefs.loadedData.gameLang == "es-ES")
            {
                string targetFolderPath = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Electronic Arts/Los Sims 3");
                if (File.Exists((targetFolderPath + "/Options.ini")) == true)
                    path = targetFolderPath;
            }
            //If "es-MX"
            if (launcherPrefs.loadedData.gameLang == "es-MX")
            {
                string targetFolderPath = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Electronic Arts/Los Sims 3");
                if (File.Exists((targetFolderPath + "/Options.ini")) == true)
                    path = targetFolderPath;
            }
            //If "sv-SE"
            if (launcherPrefs.loadedData.gameLang == "sv-SE")
            {
                string targetFolderPath = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Electronic Arts/The Sims 3");
                if (File.Exists((targetFolderPath + "/Options.ini")) == true)
                    path = targetFolderPath;
            }

            //Return the value
            return path;
        }

        public string GetStringApplicationResource(string resourceKey)
        {
            //Prepare the string to return
            string toReturn = "###";

            //Get the resource
            //string resourceGetted = (Application.Current.Resources[resourceKey] as string);
            string resourceGetted = (this.Resources.MergedDictionaries[0][resourceKey] as string);
            if (resourceGetted != null)
                toReturn = resourceGetted;

            //Return the string
            return toReturn;
        }

        private bool isRunningWithAdminRights()
        {
            //Prepare the response to return
            bool toReturn = false;

            //Try to check
            try
            {
                //Inform the response
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    toReturn = (new WindowsPrincipal(identity)).IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch (Exception ex) { toReturn = false; }

            //Return the response
            return toReturn;
        }

        private void CloneDirectory(string root, string dest)
        {
            foreach (var directory in Directory.GetDirectories(root))
            {
                //Get the path of the new directory
                var newDirectory = System.IO.Path.Combine(dest, System.IO.Path.GetFileName(directory));
                //Create the directory if it doesn't already exist
                Directory.CreateDirectory(newDirectory);
                //Recursively clone the directory
                CloneDirectory(directory, newDirectory);
            }

            foreach (var file in Directory.GetFiles(root))
            {
                File.Copy(file, System.IO.Path.Combine(dest, System.IO.Path.GetFileName(file)));
            }
        }

        private long GetDirectorySize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += GetDirectorySize(di);
            }
            return size;
        }

        private void BlockLauncherUiExceptHomePage(bool lockNow)
        {
            //If is desired to lock the UI..
            if(lockNow == true)
            {
                playBtText.Content = GetStringApplicationResource("launcher_button_playingButton");
                goSaves.IsEnabled = false;
                goExports.IsEnabled = false;
                goWorlds.IsEnabled = false;
                goMedia.IsEnabled = false;
                goCache.IsEnabled = false;
                goPatches.IsEnabled = false;
                goMods.IsEnabled = false;
                goTools.IsEnabled = false;
                goSettings.IsEnabled = false;
                goGithub.IsEnabled = false;
                goDonate.IsEnabled = false;
                goGuide.IsEnabled = false;
                goExit.IsEnabled = false;
            }

            //If is desired to unlock the UI..
            if (lockNow == false)
            {
                playBtText.Content = GetStringApplicationResource("launcher_button_playButton");
                if (myDocumentsPath != "")
                {
                    goSaves.IsEnabled = true;
                    goExports.IsEnabled = true;
                    goWorlds.IsEnabled = true;
                    goMedia.IsEnabled = true;
                    goCache.IsEnabled = true;
                    goPatches.IsEnabled = true;
                    goMods.IsEnabled = true;
                    goTools.IsEnabled = true;
                    goSettings.IsEnabled = true;
                }
                goGithub.IsEnabled = true;
                goDonate.IsEnabled = true;
                goGuide.IsEnabled = true;
                goExit.IsEnabled = true;
            }
        }

        public void SetInteractionBlockerEnabled(bool enabled)
        {
            //If is desired to activate it
            if (enabled == true)
                interactionBlocker.Visibility = Visibility.Visible;

            //If is desired to disable it
            if (enabled == false)
                interactionBlocker.Visibility = Visibility.Collapsed;
        }

        public void StartGameOverlayThread()
        {
            //Create the thread for start the overlay
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { });
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait initial time to wait the game open
                threadTools.MakeThreadSleep(30000);

                //Get the correct working directory
                string workingDirectory = Directory.GetCurrentDirectory();

                //Create the overlay process
                Process process = new Process();
                process.StartInfo.FileName = System.IO.Path.Combine(workingDirectory, "Dl3DxOverlay.exe");
                process.StartInfo.WorkingDirectory = workingDirectory;
                process.StartInfo.Arguments = ("\"TS3W\" " + ((launcherPrefs.loadedData.gameOverlay == 1) ? "0" : "1") + " 1800");
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;  //<- Hide the process window
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                //Store it
                currentGameOverlayProcess = process;

                //Create a monitor loop
                while (true)
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(5000);

                    //If was finished the overlay process or the game was closed, break the monitor loop
                    if (currentGameOverlayProcess.HasExited == true || isPlayingTheGame == false)
                        break;
                }

                //Return empty response
                return new string[] { };
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //Stop it
                currentGameOverlayProcess.Kill();
                currentGameOverlayProcess.Dispose();

                //Clear it
                currentGameOverlayProcess = null;
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        public void CheckUpdates()
        {
            //Start a thread to download the current version info
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) =>
            {
                //Add the task to queue
                AddTask("loadingCurrentVersion", "Checking for a new version.");
            };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(5000);

                //Try to do the task
                try
                {
                    //Prepare the target download URL
                    string downloadUrl = @"https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/launcher-current-version.txt";
                    string saveAsPath = (myDocumentsPath + @"/!DL-TmpCache/launcher-current-version.txt");
                    //Download the current version file
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

                //If have a response of success, compare the versions
                if (threadTaskResponse == "success")
                {
                    //Get the versions
                    string localCurrentVersion = GetLauncherVersion();
                    string remoteCurrentVersion = File.ReadAllText((myDocumentsPath + @"/!DL-TmpCache/launcher-current-version.txt"));

                    //Setup the update callback
                    updateButton.Click += ((s, e) => { StartLauncherUpdater(); });
                    //Show the version notification
                    updateText.Text = GetStringApplicationResource("launcher_updateNotifier").Replace("%v%", remoteCurrentVersion);

                    //If the version is different, enable the update notification
                    if (localCurrentVersion != remoteCurrentVersion)
                        updateNotifier.Visibility = Visibility.Visible;
                }

                //Remove the task from queue
                RemoveTask("loadingCurrentVersion");
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        public void StartLauncherUpdater()
        {
            //Cancel if is running some takss
            if(GetRunningTasksCount() > 0)
            {
                ShowToast(GetStringApplicationResource("launcher_updateWait"), ToastType.Error);
                return;
            }

            //Add task to queue
            AddTask("updatingLauncher", "Updating the Launcher.");
            //Hide this window
            this.Visibility = Visibility.Collapsed;

            //Open the updater window and install event
            WindowLauncherUpdater updater = new WindowLauncherUpdater(myDocumentsPath, Directory.GetCurrentDirectory());
            updater.Closed += (s, e) =>
            {
                ShowToast(GetStringApplicationResource("launcher_updateError"), ToastType.Error);
                RemoveTask("updatingLauncher");
                this.Visibility = Visibility.Visible;
            };
            updater.Show();
        }

        public void WarnIfLauncherWasSuccessfullyUpdated()
        {
            //If the successfully update file don't exists, cancel
            if (File.Exists((myDocumentsPath + @"/!DL-TmpCache/launcher-updater/update-successfully.ok")) == false)
                return;

            //Warn about the update
            ShowToast(GetStringApplicationResource("launcher_updateSuccess"), ToastType.Success);

            //Remove the status file
            File.Delete((myDocumentsPath + @"/!DL-TmpCache/launcher-updater/update-successfully.ok"));
        }

        //Settings manager

        private void LoadNewOptionsTemplateIfDontHaveOne()
        {
            //Prepare the options template path
            string optionsTemplatePath = (Directory.GetCurrentDirectory() + @"/Content/options-template.ini");

            //If already exists a options template, cancel
            if (File.Exists(optionsTemplatePath) == true)
                return;

            //Copy the options.ini from my documents to be a options template
            if (File.Exists((myDocumentsPath + "/Options.ini")) == true)
                File.Copy((myDocumentsPath + "/Options.ini"), optionsTemplatePath);
        }

        private void ShowAllSettings()
        {
            //Show all defined settings

            //Launcher tab
            //*** set_LauncherLang
            if (launcherPrefs.loadedData.launcherLang == "en-us")
                set_LauncherLang.SelectedIndex = 0;
            if (launcherPrefs.loadedData.launcherLang == "pt-br")
                set_LauncherLang.SelectedIndex = 1;
            //*** set_GameLang
            if (launcherPrefs.loadedData.gameLang == "undefined")
                set_GameLang.SelectedIndex = 0;
            if (launcherPrefs.loadedData.gameLang == "da-DK")
                set_GameLang.SelectedIndex = 1;
            if (launcherPrefs.loadedData.gameLang == "nl-NL")
                set_GameLang.SelectedIndex = 2;
            if (launcherPrefs.loadedData.gameLang == "en-GB")
                set_GameLang.SelectedIndex = 3;
            if (launcherPrefs.loadedData.gameLang == "en-US")
                set_GameLang.SelectedIndex = 4;
            if (launcherPrefs.loadedData.gameLang == "fr-FR")
                set_GameLang.SelectedIndex = 5;
            if (launcherPrefs.loadedData.gameLang == "it-IT")
                set_GameLang.SelectedIndex = 6;
            if (launcherPrefs.loadedData.gameLang == "pt-PT")
                set_GameLang.SelectedIndex = 7;
            if (launcherPrefs.loadedData.gameLang == "pt-BR")
                set_GameLang.SelectedIndex = 8;
            if (launcherPrefs.loadedData.gameLang == "es-ES")
                set_GameLang.SelectedIndex = 9;
            if (launcherPrefs.loadedData.gameLang == "es-MX")
                set_GameLang.SelectedIndex = 10;
            if (launcherPrefs.loadedData.gameLang == "sv-SE")
                set_GameLang.SelectedIndex = 11;
            //*** set_priority
            set_priority.SelectedIndex = launcherPrefs.loadedData.gamePriority;
            //*** set_launcherBehaviour
            set_launcherBehaviour.SelectedIndex = launcherPrefs.loadedData.launcherBehaviour;
            //*** set_gameOverlay
            set_gameOverlay.SelectedIndex = launcherPrefs.loadedData.gameOverlay;

            //Graphics tab
            //*** set_Resolution
            set_Resolution.SelectedIndex = launcherPrefs.loadedData.resolution;
            set_Resolution.SelectionChanged += (s, e) => 
            {
                //Get the screen resolution of the windows, converted to pixels
                System.Drawing.Rectangle windowsResolution = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                long windowsPixels = (windowsResolution.Width * windowsResolution.Height);

                //Convert the selected resolution for pixels
                string selectedResolution = set_Resolution.SelectedValue.ToString();
                string resolutionRaw = selectedResolution.Split(" ")[1];
                int resolutionWidth = int.Parse(resolutionRaw.Split("x")[0]);
                int resolutionHeight = int.Parse(resolutionRaw.Split("x")[1]);
                int selectedPixels = (resolutionWidth * resolutionHeight);

                //If the selected resolution is greather than windows resolution, undo the selection
                if(selectedPixels > windowsPixels)
                {
                    ShowToast(GetStringApplicationResource("launcher_settings_graphics_resolutionError"), ToastType.Error);
                    set_Resolution.SelectedIndex = launcherPrefs.loadedData.resolution;
                }
            };
            //*** set_RefreshRate
            set_RefreshRate.SelectedIndex = launcherPrefs.loadedData.refreshRate;
            set_RefreshRate.SelectionChanged += (s, e) => 
            {
                //Get displays informations
                AboutDisplays.DisplayInfo[] allDisplaysInformations = new AboutDisplays(AboutDisplays.DisplaysMode.GetCurrentDisplaySettingsOnly).GetDisplaysInformations();

                //Convert the selected resolution for integer
                string selectedResolution = set_RefreshRate.SelectedValue.ToString();
                string resolutionRaw = selectedResolution.Split(" ")[1];
                int selectedRefreshRate = int.Parse(resolutionRaw.Replace("hz", ""));

                //If the selected refresh rate is greather than windows refresh rate, undo the selection
                if (selectedRefreshRate > allDisplaysInformations[0].dmDisplayFrequency)
                {
                    ShowToast(GetStringApplicationResource("launcher_settings_graphics_refreshRateError"), ToastType.Error);
                    set_RefreshRate.SelectedIndex = launcherPrefs.loadedData.refreshRate;
                }
            };
            //*** set_MaxFps
            set_MaxFps.SelectedIndex = launcherPrefs.loadedData.maxFps;
            //*** set_Fullscreen
            set_Fullscreen.SelectedIndex = launcherPrefs.loadedData.displayMode;
            //*** set_Tps
            set_Tps.SelectedIndex = launcherPrefs.loadedData.maxTps;
            //*** set_ensureSp
            set_ensureSp.IsChecked = launcherPrefs.loadedData.debugSmoothPatch;
            //*** set_objectHiding
            set_objectHiding.IsChecked = launcherPrefs.loadedData.objectHiding;
            //*** set_animSmooth
            set_animSmooth.IsChecked = launcherPrefs.loadedData.animationSmoothing;
            //*** set_advancedRender
            set_advancedRender.IsChecked = launcherPrefs.loadedData.advancedRendering;
            //*** set_highDetailLots
            set_highDetailLots.SelectedIndex = launcherPrefs.loadedData.highDetailLots;
            //*** set_reflections
            set_reflections.SelectedIndex = launcherPrefs.loadedData.reflectionQuality;
            //*** set_antiAliasing
            set_antiAliasing.SelectedIndex = launcherPrefs.loadedData.antiAliasing;
            //*** set_drawDistance
            set_drawDistance.SelectedIndex = launcherPrefs.loadedData.drawDistance;
            //*** set_visualEffects
            set_visualEffects.SelectedIndex = launcherPrefs.loadedData.visualEffects;
            //*** set_lightShadows
            set_lightShadows.SelectedIndex = launcherPrefs.loadedData.lightShadows;
            //*** set_textureQuality
            set_textureQuality.SelectedIndex = launcherPrefs.loadedData.texturesQuality;
            //*** set_treeDetails
            set_treeDetails.SelectedIndex = launcherPrefs.loadedData.treeDetails;
            //*** set_simsDetails
            set_simsDetails.SelectedIndex = launcherPrefs.loadedData.simsDetails;

            //Sound tab
            //*** set_speakerSetup
            set_speakerSetup.SelectedIndex = launcherPrefs.loadedData.speakerSetup;
            //*** set_focusMute
            set_focusMute.IsChecked = launcherPrefs.loadedData.defocusMute;
            //*** set_voicesVolume
            set_voicesVolume.Value = launcherPrefs.loadedData.voicesVolume;
            //*** set_fxVolume
            set_fxVolume.Value = launcherPrefs.loadedData.fxVolume;
            //*** set_musicVolume
            set_musicVolume.Value = launcherPrefs.loadedData.musicVolume;
            //*** set_ambientVolume
            set_ambientVolume.Value = launcherPrefs.loadedData.ambientVolume;
            //*** set_audioQuality
            set_audioQuality.SelectedIndex = launcherPrefs.loadedData.audioQuality;

            //General tab
            //*** set_edgeScrolling
            set_edgeScrolling.IsChecked = launcherPrefs.loadedData.edgeScrolling;
            //*** set_clockFormat
            set_clockFormat.SelectedIndex = launcherPrefs.loadedData.clockFormat;
            //*** set_enableLessons
            set_enableLessons.IsChecked = launcherPrefs.loadedData.lessons;
            //*** set_interactLoad
            set_interactLoad.IsChecked = launcherPrefs.loadedData.interactiveLoading;
            //*** set_invertH
            set_invertH.IsChecked = launcherPrefs.loadedData.invertCamH;
            //*** set_invertV
            set_invertV.IsChecked = launcherPrefs.loadedData.invertCamV;
            //*** set_memories
            set_memories.SelectedIndex = launcherPrefs.loadedData.memories;

            //Game tab
            //*** set_simsLifespan
            set_simsLifespan.SelectedIndex = launcherPrefs.loadedData.simsLifespan;
            set_simsLifespan.SelectionChanged += (s, e) => { RenderSimLifespan(set_simsLifespan.SelectedIndex); };
            RenderSimLifespan(set_simsLifespan.SelectedIndex);
            //*** set_aging
            set_aging.IsChecked = launcherPrefs.loadedData.aging;
            //*** set_suppressOpportunities
            set_suppressOpportunities.IsChecked = launcherPrefs.loadedData.suppressOpportunities;
            //*** set_disableAutonomy
            set_disableAutonomy.IsChecked = launcherPrefs.loadedData.noAutonomyActiveSim;
            //*** set_simsAutonomy
            set_simsAutonomy.SelectedIndex = launcherPrefs.loadedData.simsAutonomyLevel;
            //*** set_petsAutonomy
            set_petsAutonomy.SelectedIndex = launcherPrefs.loadedData.petsAutonomyLevel;

            //Environment tab
            //*** set_summerSeason
            set_summerSeason.SelectedIndex = launcherPrefs.loadedData.summerSeason;
            //*** set_fallSeason
            set_fallSeason.SelectedIndex = launcherPrefs.loadedData.fallSeason;
            //*** set_springSeason
            set_springSeason.SelectedIndex = launcherPrefs.loadedData.springSeason;
            //*** set_winterSeason
            set_winterSeason.SelectedIndex = launcherPrefs.loadedData.winterSeason;
            //*** set_tempUnit
            set_tempUnit.SelectedIndex = launcherPrefs.loadedData.temperatureUnit;
            //*** set_weatHail
            set_weatHail.IsChecked = launcherPrefs.loadedData.hailWeather;
            //*** set_weatRain
            set_weatRain.IsChecked = launcherPrefs.loadedData.rainWeather;
            //*** set_weatSnow
            set_weatSnow.IsChecked = launcherPrefs.loadedData.snowWeather;
            //*** set_weatFog
            set_weatFog.IsChecked = launcherPrefs.loadedData.fogWeather;
            //*** set_lunarCycle
            set_lunarCycle.SelectedIndex = launcherPrefs.loadedData.lunarCycle;

            //Demography tab
            //*** set_storyProgression
            set_storyProgression.SelectedIndex = launcherPrefs.loadedData.storyProgression;
            //** set_vampires
            set_vampires.IsChecked = launcherPrefs.loadedData.allowVampires;
            //** set_werewolves
            set_werewolves.IsChecked = launcherPrefs.loadedData.allowWerewolves;
            //** set_pets
            set_pets.IsChecked = launcherPrefs.loadedData.allowPets;
            //** set_celebrities
            set_celebrities.IsChecked = launcherPrefs.loadedData.allowCelebrities;
            //** set_fairies
            set_fairies.IsChecked = launcherPrefs.loadedData.allowFairies;
            //** set_witches
            set_witches.IsChecked = launcherPrefs.loadedData.allowWitches;
            //** set_horses
            set_horses.IsChecked = launcherPrefs.loadedData.allowHorses;
            //** set_celebritiesSystem
            set_celebritiesSystem.SelectedIndex = launcherPrefs.loadedData.disableCelebritiesSystem;

            //Online tab
            //*** set_onlineNotify
            set_onlineNotify.SelectedIndex = launcherPrefs.loadedData.onlineNotifications;
            //*** set_shopMode
            set_shopMode.SelectedIndex = launcherPrefs.loadedData.shopMode;

            //...
        }

        private void SaveAllSettings()
        {
            //Apply all settings to preferences

            //Launcher tab
            //*** set_LauncherLang
            if (set_LauncherLang.SelectedIndex == 0)
                launcherPrefs.loadedData.launcherLang = "en-us";
            if (set_LauncherLang.SelectedIndex == 1)
                launcherPrefs.loadedData.launcherLang = "pt-br";
            //*** set_GameLang
            if (set_GameLang.SelectedIndex == 0)
                launcherPrefs.loadedData.gameLang = "undefined";
            if (set_GameLang.SelectedIndex == 1)
                launcherPrefs.loadedData.gameLang = "da-DK";
            if (set_GameLang.SelectedIndex == 2)
                launcherPrefs.loadedData.gameLang = "nl-NL";
            if (set_GameLang.SelectedIndex == 3)
                launcherPrefs.loadedData.gameLang = "en-GB";
            if (set_GameLang.SelectedIndex == 4)
                launcherPrefs.loadedData.gameLang = "en-US";
            if (set_GameLang.SelectedIndex == 5)
                launcherPrefs.loadedData.gameLang = "fr-FR";
            if (set_GameLang.SelectedIndex == 6)
                launcherPrefs.loadedData.gameLang = "it-IT";
            if (set_GameLang.SelectedIndex == 7)
                launcherPrefs.loadedData.gameLang = "pt-PT";
            if (set_GameLang.SelectedIndex == 8)
                launcherPrefs.loadedData.gameLang = "pt-BR";
            if (set_GameLang.SelectedIndex == 9)
                launcherPrefs.loadedData.gameLang = "es-ES";
            if (set_GameLang.SelectedIndex == 10)
                launcherPrefs.loadedData.gameLang = "es-MX";
            if (set_GameLang.SelectedIndex == 11)
                launcherPrefs.loadedData.gameLang = "sv-SE";
            //*** set_priority
            launcherPrefs.loadedData.gamePriority = set_priority.SelectedIndex;
            //*** set_launcherBehaviour
            launcherPrefs.loadedData.launcherBehaviour = set_launcherBehaviour.SelectedIndex;
            //*** set_gameOverlay
            launcherPrefs.loadedData.gameOverlay = set_gameOverlay.SelectedIndex;

            //Graphics tab
            //*** set_Resolution
            launcherPrefs.loadedData.resolution = set_Resolution.SelectedIndex;
            //*** set_RefreshRate
            launcherPrefs.loadedData.refreshRate = set_RefreshRate.SelectedIndex;
            //*** set_MaxFps
            launcherPrefs.loadedData.maxFps = set_MaxFps.SelectedIndex;
            //*** set_Fullscreen
            launcherPrefs.loadedData.displayMode = set_Fullscreen.SelectedIndex;
            //*** set_Tps
            launcherPrefs.loadedData.maxTps = set_Tps.SelectedIndex;
            //*** set_ensureSp
            launcherPrefs.loadedData.debugSmoothPatch = ((bool)set_ensureSp.IsChecked);
            //*** set_objectHiding
            launcherPrefs.loadedData.objectHiding = ((bool)set_objectHiding.IsChecked);
            //*** set_animSmooth
            launcherPrefs.loadedData.animationSmoothing = ((bool)set_animSmooth.IsChecked);
            //*** set_advancedRender
            launcherPrefs.loadedData.advancedRendering = ((bool)set_advancedRender.IsChecked);
            //*** set_highDetailLots
            launcherPrefs.loadedData.highDetailLots = set_highDetailLots.SelectedIndex;
            //*** set_reflections
            launcherPrefs.loadedData.reflectionQuality = set_reflections.SelectedIndex;
            //*** set_antiAliasing
            launcherPrefs.loadedData.antiAliasing = set_antiAliasing.SelectedIndex;
            //*** set_drawDistance
            launcherPrefs.loadedData.drawDistance = set_drawDistance.SelectedIndex;
            //*** set_visualEffects
            launcherPrefs.loadedData.visualEffects = set_visualEffects.SelectedIndex;
            //*** set_lightShadows
            launcherPrefs.loadedData.lightShadows = set_lightShadows.SelectedIndex;
            //*** set_textureQuality
            launcherPrefs.loadedData.texturesQuality = set_textureQuality.SelectedIndex;
            //*** set_treeDetails
            launcherPrefs.loadedData.treeDetails = set_treeDetails.SelectedIndex;
            //*** set_simsDetails
            launcherPrefs.loadedData.simsDetails = set_simsDetails.SelectedIndex;

            //Sounds tab
            //*** set_speakerSetup
            launcherPrefs.loadedData.speakerSetup = set_speakerSetup.SelectedIndex;
            //*** set_focusMute
            launcherPrefs.loadedData.defocusMute = ((bool)set_focusMute.IsChecked);
            //*** set_voicesVolume
            launcherPrefs.loadedData.voicesVolume = ((float)set_voicesVolume.Value);
            //*** set_fxVolume
            launcherPrefs.loadedData.fxVolume = ((float)set_fxVolume.Value);
            //*** set_musicVolume
            launcherPrefs.loadedData.musicVolume = ((float)set_musicVolume.Value);
            //*** set_ambientVolume
            launcherPrefs.loadedData.ambientVolume = ((float)set_ambientVolume.Value);
            //*** set_audioQuality
            launcherPrefs.loadedData.audioQuality = set_audioQuality.SelectedIndex;

            //General tab
            //*** set_edgeScrolling
            launcherPrefs.loadedData.edgeScrolling = ((bool)set_edgeScrolling.IsChecked);
            //*** set_clockFormat
            launcherPrefs.loadedData.clockFormat = set_clockFormat.SelectedIndex;
            //*** set_enableLessons
            launcherPrefs.loadedData.lessons = ((bool)set_enableLessons.IsChecked);
            //*** set_interactLoad
            launcherPrefs.loadedData.interactiveLoading = ((bool)set_interactLoad.IsChecked);
            //*** set_invertH
            launcherPrefs.loadedData.invertCamH = ((bool)set_invertH.IsChecked);
            //*** set_invertV
            launcherPrefs.loadedData.invertCamV = ((bool)set_invertV.IsChecked);
            //*** set_memories
            launcherPrefs.loadedData.memories = set_memories.SelectedIndex;

            //Game tab
            //*** set_simsLifespan
            launcherPrefs.loadedData.simsLifespan = set_simsLifespan.SelectedIndex;
            //*** set_aging
            launcherPrefs.loadedData.aging = ((bool)set_aging.IsChecked);
            //*** set_suppressOpportunities
            launcherPrefs.loadedData.suppressOpportunities = ((bool)set_suppressOpportunities.IsChecked);
            //*** set_disableAutonomy
            launcherPrefs.loadedData.noAutonomyActiveSim = ((bool)set_disableAutonomy.IsChecked);
            //*** set_simsAutonomy
            launcherPrefs.loadedData.simsAutonomyLevel = set_simsAutonomy.SelectedIndex;
            //*** set_petsAutonomy
            launcherPrefs.loadedData.petsAutonomyLevel = set_petsAutonomy.SelectedIndex;

            //Environment tab
            //*** set_summerSeason
            launcherPrefs.loadedData.summerSeason = set_summerSeason.SelectedIndex;
            //*** set_springSeason
            launcherPrefs.loadedData.springSeason = set_springSeason.SelectedIndex;
            //*** set_fallSeason
            launcherPrefs.loadedData.fallSeason = set_fallSeason.SelectedIndex;
            //*** set_winterSeason
            launcherPrefs.loadedData.winterSeason = set_winterSeason.SelectedIndex;
            //*** set_tempUnit
            launcherPrefs.loadedData.temperatureUnit = set_tempUnit.SelectedIndex;
            //*** set_weatHail
            launcherPrefs.loadedData.hailWeather = ((bool)set_weatHail.IsChecked);
            //*** set_weatRain
            launcherPrefs.loadedData.rainWeather = ((bool)set_weatRain.IsChecked);
            //*** set_weatSnow
            launcherPrefs.loadedData.snowWeather = ((bool)set_weatSnow.IsChecked);
            //*** set_weatFog
            launcherPrefs.loadedData.fogWeather = ((bool)set_weatFog.IsChecked);
            //*** set_lunarCycle
            launcherPrefs.loadedData.lunarCycle = set_lunarCycle.SelectedIndex;

            //Online tab
            //*** set_onlineNotify
            launcherPrefs.loadedData.onlineNotifications = set_onlineNotify.SelectedIndex;
            //*** set_shopMode
            launcherPrefs.loadedData.shopMode = set_shopMode.SelectedIndex;

            //Demography tab
            //*** 
            launcherPrefs.loadedData.storyProgression = set_storyProgression.SelectedIndex;
            //*** 
            launcherPrefs.loadedData.allowVampires = ((bool)set_vampires.IsChecked);
            //*** 
            launcherPrefs.loadedData.allowWerewolves = ((bool)set_werewolves.IsChecked);
            //*** 
            launcherPrefs.loadedData.allowPets = ((bool)set_pets.IsChecked);
            //*** 
            launcherPrefs.loadedData.allowCelebrities = ((bool)set_celebrities.IsChecked);
            //*** 
            launcherPrefs.loadedData.allowFairies = ((bool)set_fairies.IsChecked);
            //*** 
            launcherPrefs.loadedData.allowWitches = ((bool)set_witches.IsChecked);
            //*** 
            launcherPrefs.loadedData.allowHorses = ((bool)set_horses.IsChecked);
            //*** 
            launcherPrefs.loadedData.disableCelebritiesSystem = set_celebritiesSystem.SelectedIndex;

            //Save to file
            launcherPrefs.Save();
        }

        private void ApplyAllSettings()
        {
            //Create a thread to remove the temporary task
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { });
            asyncTask.onStartTask_RunMainThread += (Window sourceWindow, string[] startParameters) =>
            {
                //Add a task to signal that is applying the settings
                AddTask("applySettings", "Applying all defined settings.");

                //Block the save button
                settingsSave.IsEnabled = false;
                set_ResetOptTemplate.IsEnabled = false;
            };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(1000);

                //Load all settings (read-only)
                Preferences launcherSettings = new Preferences();

                //---------------------- Locale In Regedit ----------------------//

                //If the game language is not undefined
                if(launcherSettings.loadedData.gameLang != "undefined")
                {
                    //Extract the locale and country
                    string locale = launcherSettings.loadedData.gameLang;
                    string country = launcherSettings.loadedData.gameLang.Split("-")[1];

                    //Translate the Base Game, Expansion Packs and Stuff Packs
                    if (true == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\The Sims 3", country, locale);
                    if (isStuffPackInstalled(StuffPack.y70y80y90) == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\The Sims 3 70s 80s & 90s Stuff", country, locale);
                    if (isExpansionPackInstalled(ExpansionPack.Ambitions) == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\The Sims 3 Ambitions", country, locale);
                    if (isStuffPackInstalled(StuffPack.Diesel) == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\the sims 3 diesel stuff", country, locale);
                    if (isStuffPackInstalled(StuffPack.FastLane) == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\The Sims 3 Fast Lane Stuff", country, locale);
                    if (isExpansionPackInstalled(ExpansionPack.Generations) == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\The Sims 3 Generations", country, locale);
                    if (isStuffPackInstalled(StuffPack.HighEndLoft) == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\The Sims 3 High End Loft Stuff", country, locale);
                    if (isExpansionPackInstalled(ExpansionPack.IntoTheFuture) == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\The Sims 3 Into the Future", country, locale);
                    if (isExpansionPackInstalled(ExpansionPack.IslandParadise) == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\The Sims 3 Island Paradise", country, locale);
                    if (isExpansionPackInstalled(ExpansionPack.LateNight) == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\The Sims 3 Late Night", country, locale);
                    if (isStuffPackInstalled(StuffPack.MasterSuite) == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\The Sims 3 Master Suite Stuff", country, locale);
                    if (isStuffPackInstalled(StuffPack.Movie) == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\the sims 3 movie stuff", country, locale);
                    if (isStuffPackInstalled(StuffPack.OutdoorLiving) == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\The Sims 3 Outdoor Living Stuff", country, locale);
                    if (isExpansionPackInstalled(ExpansionPack.Pets) == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\The Sims 3 Pets", country, locale);
                    if (isExpansionPackInstalled(ExpansionPack.Seasons) == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\the sims 3 seasons", country, locale);
                    if (isExpansionPackInstalled(ExpansionPack.Showtime) == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\The Sims 3 Showtime", country, locale);
                    if (isExpansionPackInstalled(ExpansionPack.Supernatural) == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\the sims 3 supernatural", country, locale);
                    if (isStuffPackInstalled(StuffPack.TownLife) == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\The Sims 3 Town Life Stuff", country, locale);
                    if (isExpansionPackInstalled(ExpansionPack.UniversityLife) == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\the sims 3 university life", country, locale);
                    if (isExpansionPackInstalled(ExpansionPack.WorldAdventures) == true)
                        UpdateLanguageOfKeyInRegistry(@"SOFTWARE\WOW6432Node\Sims(Steam)\The Sims 3 World Adventures", country, locale);
                }

                //---------------------- Game Settings and Preferences ----------------------//

                //Produce the DxDiag relatory and get driver version number (this can demand some time...)
                string currentMainDisplayDriverVersion = GetMainDisplayDriverVersion();

                //Search by new keys existing in Options.ini of MyDocuments and add all new keys to options template...
                AddNewOptionsKeysToExistingOptionsTemplate();

                //Copy the options template for my documents
                if (File.Exists((myDocumentsPath + "/Options.ini")) == true)
                    File.Delete((myDocumentsPath + "/Options.ini"));
                File.Copy((Directory.GetCurrentDirectory() + @"/Content/options-template.ini"), (myDocumentsPath + "/Options.ini"));
                //Copy the options template for root of ts3 (smooth patcher)
                if (File.Exists((Directory.GetCurrentDirectory() + @"/TS3Patch.txt")) == true)
                    File.Delete((Directory.GetCurrentDirectory() + @"/TS3Patch.txt"));
                File.Copy((Directory.GetCurrentDirectory() + @"/Content/smooth-patch-template.txt"), (Directory.GetCurrentDirectory() + @"/TS3Patch.txt"));
                //Copy the options template for antilag (anti lag)
                if (File.Exists((Directory.GetCurrentDirectory() + @"/antilag.cfg")) == true)
                    File.Delete((Directory.GetCurrentDirectory() + @"/antilag.cfg"));
                File.Copy((Directory.GetCurrentDirectory() + @"/Content/antilag-template.cfg"), (Directory.GetCurrentDirectory() + @"/antilag.cfg"));

                //Read the Options.ini
                IniReader optionsIni = new IniReader((myDocumentsPath + "/Options.ini"));
                TxtReader smoothPatchOptionsTxt = new TxtReader((Directory.GetCurrentDirectory() + @"/TS3Patch.txt"));
                TxtReader antiLagOptionsTxt = new TxtReader((Directory.GetCurrentDirectory() + @"/antilag.cfg"));

                //**** Graphics

                //*** set_Resolution and //*** set_RefreshRate
                string resolutionString = "";
                if (launcherSettings.loadedData.resolution == 0)
                    resolutionString = "1280 720";
                if (launcherSettings.loadedData.resolution == 1)
                    resolutionString = "1366 768";
                if (launcherSettings.loadedData.resolution == 2)
                    resolutionString = "1600 900";
                if (launcherSettings.loadedData.resolution == 3)
                    resolutionString = "1920 1080";
                if (launcherSettings.loadedData.resolution == 4)
                    resolutionString = "2560 1080";
                if (launcherSettings.loadedData.resolution == 5)
                    resolutionString = "2560 1440";
                if (launcherSettings.loadedData.resolution == 6)
                    resolutionString = "3200 1800";
                if (launcherSettings.loadedData.resolution == 7)
                    resolutionString = "3440 1440";
                if (launcherSettings.loadedData.resolution == 8)
                    resolutionString = "3840 2160";
                if (launcherSettings.loadedData.resolution == 9)
                    resolutionString = "3840 1600";
                if (launcherSettings.loadedData.resolution == 10)
                    resolutionString = "5120 2160";
                if (launcherSettings.loadedData.refreshRate == 0)
                    resolutionString += " 50";
                if (launcherSettings.loadedData.refreshRate == 1)
                    resolutionString += " 60";
                if (launcherSettings.loadedData.refreshRate == 2)
                    resolutionString += " 75";
                if (launcherSettings.loadedData.refreshRate == 3)
                    resolutionString += " 90";
                if (launcherSettings.loadedData.refreshRate == 4)
                    resolutionString += " 100";
                if (launcherSettings.loadedData.refreshRate == 5)
                    resolutionString += " 120";
                if (launcherSettings.loadedData.refreshRate == 6)
                    resolutionString += " 144";
                if (launcherSettings.loadedData.refreshRate == 7)
                    resolutionString += " 165";
                if (launcherSettings.loadedData.refreshRate == 8)
                    resolutionString += " 180";
                if (launcherSettings.loadedData.refreshRate == 9)
                    resolutionString += " 200";
                if (launcherSettings.loadedData.refreshRate == 10)
                    resolutionString += " 240";
                if (launcherSettings.loadedData.refreshRate == 11)
                    resolutionString += " 360";
                optionsIni.UpdateValue("resolution", resolutionString);
                //*** set_Fullscreen
                if (launcherSettings.loadedData.displayMode == 0)
                {
                    optionsIni.UpdateValue("fullscreen", "0");
                    smoothPatchOptionsTxt.UpdateValue(12, "Borderless", "0");
                }
                if (launcherSettings.loadedData.displayMode == 1)
                {
                    optionsIni.UpdateValue("fullscreen", "0");
                    smoothPatchOptionsTxt.UpdateValue(12, "Borderless", "1");
                }
                if (launcherSettings.loadedData.displayMode == 2)
                {
                    optionsIni.UpdateValue("fullscreen", "1");
                    smoothPatchOptionsTxt.UpdateValue(12, "Borderless", "0");
                }
                //*** set_MaxFps
                if (launcherSettings.loadedData.maxFps == 0)
                    antiLagOptionsTxt.UpdateValue(8, "FPSlimit", "0");
                if (launcherSettings.loadedData.maxFps == 1)
                    antiLagOptionsTxt.UpdateValue(8, "FPSlimit", "60");
                if (launcherSettings.loadedData.maxFps == 2)
                    antiLagOptionsTxt.UpdateValue(8, "FPSlimit", "75");
                if (launcherSettings.loadedData.maxFps == 3)
                    antiLagOptionsTxt.UpdateValue(8, "FPSlimit", "90");
                if (launcherSettings.loadedData.maxFps == 4)
                    antiLagOptionsTxt.UpdateValue(8, "FPSlimit", "120");
                //*** set_Tps
                if (launcherSettings.loadedData.maxTps == 0)
                    smoothPatchOptionsTxt.UpdateValue(6, "TPS", "500");
                if (launcherSettings.loadedData.maxTps == 1)
                    smoothPatchOptionsTxt.UpdateValue(6, "TPS", "1000");
                //*** set_ensureSp
                if (launcherSettings.loadedData.debugSmoothPatch == false)
                    smoothPatchOptionsTxt.UpdateValue(15, "Debug", "0");
                if (launcherSettings.loadedData.debugSmoothPatch == true)
                    smoothPatchOptionsTxt.UpdateValue(15, "Debug", "1");
                //*** set_objectHiding
                if (launcherSettings.loadedData.objectHiding == false)
                    optionsIni.UpdateValue("objecthiding", "0");
                if (launcherSettings.loadedData.objectHiding == true)
                    optionsIni.UpdateValue("objecthiding", "1");
                //*** set_animSmooth
                if (launcherSettings.loadedData.animationSmoothing == false)
                    optionsIni.UpdateValue("animationsmoothing", "0");
                if (launcherSettings.loadedData.animationSmoothing == true)
                    optionsIni.UpdateValue("animationsmoothing", "1");
                //*** set_advancedRender
                if (launcherSettings.loadedData.advancedRendering == false)
                    optionsIni.UpdateValue("advancedrendering", "0");
                if (launcherSettings.loadedData.advancedRendering == true)
                    optionsIni.UpdateValue("advancedrendering", "1");
                //*** set_highDetailLots
                if (launcherSettings.loadedData.highDetailLots == 0)
                    optionsIni.UpdateValue("maxactivelots", "1");
                if (launcherSettings.loadedData.highDetailLots == 1)
                    optionsIni.UpdateValue("maxactivelots", "2");
                if (launcherSettings.loadedData.highDetailLots == 2)
                    optionsIni.UpdateValue("maxactivelots", "3");
                if (launcherSettings.loadedData.highDetailLots == 3)
                    optionsIni.UpdateValue("maxactivelots", "4");
                //*** set_reflections
                if (launcherSettings.loadedData.reflectionQuality == 0)
                    optionsIni.UpdateValue("generalreflections", "0");
                if (launcherSettings.loadedData.reflectionQuality == 1)
                    optionsIni.UpdateValue("generalreflections", "1");
                if (launcherSettings.loadedData.reflectionQuality == 2)
                    optionsIni.UpdateValue("generalreflections", "2");
                if (launcherSettings.loadedData.reflectionQuality == 3)
                    optionsIni.UpdateValue("generalreflections", "3");
                //*** set_antiAliasing
                if (launcherSettings.loadedData.antiAliasing == 0)
                    optionsIni.UpdateValue("edgesmoothing", "0");
                if (launcherSettings.loadedData.antiAliasing == 1)
                    optionsIni.UpdateValue("edgesmoothing", "1");
                if (launcherSettings.loadedData.antiAliasing == 2)
                    optionsIni.UpdateValue("edgesmoothing", "2");
                if (launcherSettings.loadedData.antiAliasing == 3)
                    optionsIni.UpdateValue("edgesmoothing", "3");
                //*** set_drawDistance
                if (launcherSettings.loadedData.drawDistance == 0)
                    optionsIni.UpdateValue("drawdistance", "1");
                if (launcherSettings.loadedData.drawDistance == 1)
                    optionsIni.UpdateValue("drawdistance", "2");
                if (launcherSettings.loadedData.drawDistance == 2)
                    optionsIni.UpdateValue("drawdistance", "3");
                //*** set_visualEffects
                if (launcherSettings.loadedData.visualEffects == 0)
                    optionsIni.UpdateValue("visualeffects", "1");
                if (launcherSettings.loadedData.visualEffects == 1)
                    optionsIni.UpdateValue("visualeffects", "2");
                if (launcherSettings.loadedData.visualEffects == 2)
                    optionsIni.UpdateValue("visualeffects", "3");
                //*** set_lighShadows
                if (launcherSettings.loadedData.lightShadows == 0)
                    optionsIni.UpdateValue("lightingquality", "1");
                if (launcherSettings.loadedData.lightShadows == 1)
                    optionsIni.UpdateValue("lightingquality", "2");
                if (launcherSettings.loadedData.lightShadows == 2)
                    optionsIni.UpdateValue("lightingquality", "3");
                //*** set_textureQuality
                if (launcherSettings.loadedData.texturesQuality == 0)
                    optionsIni.UpdateValue("texturequality", "1");
                if (launcherSettings.loadedData.texturesQuality == 1)
                    optionsIni.UpdateValue("texturequality", "2");
                if (launcherSettings.loadedData.texturesQuality == 2)
                    optionsIni.UpdateValue("texturequality", "3");
                //*** set_treeDetails
                if (launcherSettings.loadedData.treeDetails == 0)
                    optionsIni.UpdateValue("treequality", "1");
                if (launcherSettings.loadedData.treeDetails == 1)
                    optionsIni.UpdateValue("treequality", "2");
                if (launcherSettings.loadedData.treeDetails == 2)
                    optionsIni.UpdateValue("treequality", "3");
                if (launcherSettings.loadedData.treeDetails == 3)
                    optionsIni.UpdateValue("treequality", "4");
                //*** set_simsQuality
                if (launcherSettings.loadedData.simsDetails == 0)
                    optionsIni.UpdateValue("simquality", "1");
                if (launcherSettings.loadedData.simsDetails == 1)
                    optionsIni.UpdateValue("simquality", "2");
                if (launcherSettings.loadedData.simsDetails == 2)
                    optionsIni.UpdateValue("simquality", "3");
                if (launcherSettings.loadedData.simsDetails == 3)
                    optionsIni.UpdateValue("simquality", "4");

                //**** Sounds

                //*** set_speakerSetup
                if (launcherSettings.loadedData.speakerSetup == 0)
                    optionsIni.UpdateValue("audiooutputmode", "1");
                if (launcherSettings.loadedData.speakerSetup == 1)
                    optionsIni.UpdateValue("audiooutputmode", "2");
                if (launcherSettings.loadedData.speakerSetup == 2)
                    optionsIni.UpdateValue("audiooutputmode", "3");
                //*** set_focusMute
                if (launcherSettings.loadedData.defocusMute == false)
                    optionsIni.UpdateValue("focusmute", "0");
                if (launcherSettings.loadedData.defocusMute == true)
                    optionsIni.UpdateValue("focusmute", "1");
                //*** set_voicesVolume
                optionsIni.UpdateValue("voicelevel", ((int)(launcherSettings.loadedData.voicesVolume * 255.0f)).ToString("F0"));
                optionsIni.UpdateValue("voicemute", "0");
                //*** set_fxVolume
                optionsIni.UpdateValue("soundfxlevel", ((int)(launcherSettings.loadedData.fxVolume * 255.0f)).ToString("F0"));
                optionsIni.UpdateValue("soundfxmute", "0");
                //*** set_musicVolume
                optionsIni.UpdateValue("musiclevel", ((int)(launcherSettings.loadedData.musicVolume * 255.0f)).ToString("F0"));
                optionsIni.UpdateValue("musicmute", "0");
                //*** set_ambientVolume
                optionsIni.UpdateValue("ambientlevel", ((int)(launcherSettings.loadedData.ambientVolume * 255.0f)).ToString("F0"));
                optionsIni.UpdateValue("ambientmute", "0");
                //*** set_audioQuality
                if (launcherSettings.loadedData.audioQuality == 0)
                    optionsIni.UpdateValue("audioquality", "1");
                if (launcherSettings.loadedData.audioQuality == 1)
                    optionsIni.UpdateValue("audioquality", "2");
                if (launcherSettings.loadedData.audioQuality == 2)
                    optionsIni.UpdateValue("audioquality", "3");

                //**** General

                //*** set_edgeScrolling
                if (launcherSettings.loadedData.edgeScrolling == false)
                    optionsIni.UpdateValue("edgescrolling", "0");
                if (launcherSettings.loadedData.edgeScrolling == true)
                    optionsIni.UpdateValue("edgescrolling", "1");
                //*** set_clockFormat
                if (launcherSettings.loadedData.clockFormat == 0)
                    optionsIni.UpdateValue("twelvehourclock", "0");
                if (launcherSettings.loadedData.clockFormat == 1)
                    optionsIni.UpdateValue("twelvehourclock", "1");
                //*** set_enableLessons
                if (launcherSettings.loadedData.lessons == false)
                    optionsIni.UpdateValue("enabletutorial", "0");
                if (launcherSettings.loadedData.lessons == true)
                    optionsIni.UpdateValue("enabletutorial", "1");
                //*** set_interactLoad
                if (launcherSettings.loadedData.interactiveLoading == false)
                    optionsIni.UpdateValue("enableinteractiveloading", "0");
                if (launcherSettings.loadedData.interactiveLoading == true)
                    optionsIni.UpdateValue("enableinteractiveloading", "1");
                //*** set_invertH
                if (launcherSettings.loadedData.invertCamH == false)
                    optionsIni.UpdateValue("inverthorizontalrotation", "0");
                if (launcherSettings.loadedData.invertCamH == true)
                    optionsIni.UpdateValue("inverthorizontalrotation", "1");
                //*** set_invertV
                if (launcherSettings.loadedData.invertCamV == false)
                    optionsIni.UpdateValue("invertverticalrotation", "0");
                if (launcherSettings.loadedData.invertCamV == true)
                    optionsIni.UpdateValue("invertverticalrotation", "1");
                //*** set_memories
                if (launcherSettings.loadedData.memories == 0)
                    optionsIni.UpdateValue("enablememories", "1");
                if (launcherSettings.loadedData.memories == 1)
                    optionsIni.UpdateValue("enablememories", "2");
                if (launcherSettings.loadedData.memories == 2)
                    optionsIni.UpdateValue("enablememories", "3");
                //Disable the information usage share
                optionsIni.UpdateValue("enabletelemetry", "0");

                //**** Game

                //*** set_simsLifespan
                if (launcherSettings.loadedData.simsLifespan == 0)
                {
                    //Short
                    optionsIni.UpdateValue("aginginterval", "0");
                    //Sim
                    optionsIni.UpdateValue("agingstagelengthbaby", "2");
                    optionsIni.UpdateValue("agingstagelengthtoddler", "2");
                    optionsIni.UpdateValue("agingstagelengthchild", "4");
                    optionsIni.UpdateValue("agingstagelengthteen", "6");
                    optionsIni.UpdateValue("agingstagelengthyoungadult", "8");
                    optionsIni.UpdateValue("agingstagelengthadult", "8");
                    optionsIni.UpdateValue("agingstagelengthelder", "6");
                    //Dog
                    optionsIni.UpdateValue("agingstagelengthpuppy", "3");
                    optionsIni.UpdateValue("agingstagelengthdogadult", "10");
                    optionsIni.UpdateValue("agingstagelengthdogelder", "4");
                    //Cat
                    optionsIni.UpdateValue("agingstagelengthkitten", "3");
                    optionsIni.UpdateValue("agingstagelengthcatadult", "10");
                    optionsIni.UpdateValue("agingstagelengthcatelder", "4");
                    //Horse
                    optionsIni.UpdateValue("agingstagelengthfoal", "2");
                    optionsIni.UpdateValue("agingstagelengthhorseadult", "13");
                    optionsIni.UpdateValue("agingstagelengthhorseelder", "6");
                }
                if (launcherSettings.loadedData.simsLifespan == 1)
                {
                    //Medium
                    optionsIni.UpdateValue("aginginterval", "1");
                    //Sim
                    optionsIni.UpdateValue("agingstagelengthbaby", "3");
                    optionsIni.UpdateValue("agingstagelengthtoddler", "6");
                    optionsIni.UpdateValue("agingstagelengthchild", "8");
                    optionsIni.UpdateValue("agingstagelengthteen", "12");
                    optionsIni.UpdateValue("agingstagelengthyoungadult", "15");
                    optionsIni.UpdateValue("agingstagelengthadult", "15");
                    optionsIni.UpdateValue("agingstagelengthelder", "10");
                    //Dog
                    optionsIni.UpdateValue("agingstagelengthpuppy", "5");
                    optionsIni.UpdateValue("agingstagelengthdogadult", "20");
                    optionsIni.UpdateValue("agingstagelengthdogelder", "8");
                    //Cat
                    optionsIni.UpdateValue("agingstagelengthkitten", "5");
                    optionsIni.UpdateValue("agingstagelengthcatadult", "22");
                    optionsIni.UpdateValue("agingstagelengthcatelder", "10");
                    //Horse
                    optionsIni.UpdateValue("agingstagelengthfoal", "4");
                    optionsIni.UpdateValue("agingstagelengthhorseadult", "27");
                    optionsIni.UpdateValue("agingstagelengthhorseelder", "14");
                }
                if (launcherSettings.loadedData.simsLifespan == 2)
                {
                    //Normal
                    optionsIni.UpdateValue("aginginterval", "2");
                    //Sim
                    optionsIni.UpdateValue("agingstagelengthbaby", "5");
                    optionsIni.UpdateValue("agingstagelengthtoddler", "9");
                    optionsIni.UpdateValue("agingstagelengthchild", "14");
                    optionsIni.UpdateValue("agingstagelengthteen", "20");
                    optionsIni.UpdateValue("agingstagelengthyoungadult", "25");
                    optionsIni.UpdateValue("agingstagelengthadult", "25");
                    optionsIni.UpdateValue("agingstagelengthelder", "20");
                    //Dog
                    optionsIni.UpdateValue("agingstagelengthpuppy", "8");
                    optionsIni.UpdateValue("agingstagelengthdogadult", "32");
                    optionsIni.UpdateValue("agingstagelengthdogelder", "14");
                    //Cat
                    optionsIni.UpdateValue("agingstagelengthkitten", "8");
                    optionsIni.UpdateValue("agingstagelengthcatadult", "36");
                    optionsIni.UpdateValue("agingstagelengthcatelder", "15");
                    //Horse
                    optionsIni.UpdateValue("agingstagelengthfoal", "10");
                    optionsIni.UpdateValue("agingstagelengthhorseadult", "40");
                    optionsIni.UpdateValue("agingstagelengthhorseelder", "18");
                }
                if (launcherSettings.loadedData.simsLifespan == 3)
                {
                    //Long
                    optionsIni.UpdateValue("aginginterval", "3");
                    //Sim
                    optionsIni.UpdateValue("agingstagelengthbaby", "8");
                    optionsIni.UpdateValue("agingstagelengthtoddler", "16");
                    optionsIni.UpdateValue("agingstagelengthchild", "28");
                    optionsIni.UpdateValue("agingstagelengthteen", "50");
                    optionsIni.UpdateValue("agingstagelengthyoungadult", "90");
                    optionsIni.UpdateValue("agingstagelengthadult", "90");
                    optionsIni.UpdateValue("agingstagelengthelder", "25");
                    //Dog
                    optionsIni.UpdateValue("agingstagelengthpuppy", "26");
                    optionsIni.UpdateValue("agingstagelengthdogadult", "108");
                    optionsIni.UpdateValue("agingstagelengthdogelder", "26");
                    //Cat
                    optionsIni.UpdateValue("agingstagelengthkitten", "28");
                    optionsIni.UpdateValue("agingstagelengthcatadult", "120");
                    optionsIni.UpdateValue("agingstagelengthcatelder", "26");
                    //Horse
                    optionsIni.UpdateValue("agingstagelengthfoal", "20");
                    optionsIni.UpdateValue("agingstagelengthhorseadult", "150");
                    optionsIni.UpdateValue("agingstagelengthhorseelder", "26");
                }
                if (launcherSettings.loadedData.simsLifespan == 4)
                {
                    //Epic
                    optionsIni.UpdateValue("aginginterval", "4");
                    //Sim
                    optionsIni.UpdateValue("agingstagelengthbaby", "40");
                    optionsIni.UpdateValue("agingstagelengthtoddler", "100");
                    optionsIni.UpdateValue("agingstagelengthchild", "115");
                    optionsIni.UpdateValue("agingstagelengthteen", "160");
                    optionsIni.UpdateValue("agingstagelengthyoungadult", "230");
                    optionsIni.UpdateValue("agingstagelengthadult", "230");
                    optionsIni.UpdateValue("agingstagelengthelder", "120");
                    //Dog
                    optionsIni.UpdateValue("agingstagelengthpuppy", "90");
                    optionsIni.UpdateValue("agingstagelengthdogadult", "382");
                    optionsIni.UpdateValue("agingstagelengthdogelder", "120");
                    //Cat
                    optionsIni.UpdateValue("agingstagelengthkitten", "98");
                    optionsIni.UpdateValue("agingstagelengthcatadult", "398");
                    optionsIni.UpdateValue("agingstagelengthcatelder", "128");
                    //Horse
                    optionsIni.UpdateValue("agingstagelengthfoal", "98");
                    optionsIni.UpdateValue("agingstagelengthhorseadult", "450");
                    optionsIni.UpdateValue("agingstagelengthhorseelder", "158");
                }
                //*** set_aging
                if (launcherSettings.loadedData.aging == false)
                    optionsIni.UpdateValue("enableaging", "0");
                if (launcherSettings.loadedData.aging == true)
                    optionsIni.UpdateValue("enableaging", "1");
                //*** set_suppressOpportunities
                if (launcherSettings.loadedData.suppressOpportunities == false)
                    optionsIni.UpdateValue("supressopportunitydialogs", "0");
                if (launcherSettings.loadedData.suppressOpportunities == true)
                    optionsIni.UpdateValue("supressopportunitydialogs", "1");
                //*** set_disableAutonomy
                if (launcherSettings.loadedData.noAutonomyActiveSim == false)
                    optionsIni.UpdateValue("disableautonomyforselectedsim", "0");
                if (launcherSettings.loadedData.noAutonomyActiveSim == true)
                    optionsIni.UpdateValue("disableautonomyforselectedsim", "1");
                //*** set_simsAutonomy
                if (launcherSettings.loadedData.simsAutonomyLevel == 0)
                    optionsIni.UpdateValue("autonomylevel", "0");
                if (launcherSettings.loadedData.simsAutonomyLevel == 1)
                    optionsIni.UpdateValue("autonomylevel", "1");
                if (launcherSettings.loadedData.simsAutonomyLevel == 2)
                    optionsIni.UpdateValue("autonomylevel", "2");
                //*** set_petsAutonomy
                if (launcherSettings.loadedData.petsAutonomyLevel == 0)
                    optionsIni.UpdateValue("petautonomylevel", "0");
                if (launcherSettings.loadedData.petsAutonomyLevel == 1)
                    optionsIni.UpdateValue("petautonomylevel", "1");
                if (launcherSettings.loadedData.petsAutonomyLevel == 2)
                    optionsIni.UpdateValue("petautonomylevel", "2");

                //**** Recording

                //Set default settings
                optionsIni.UpdateValue("videocapturesize", "2");
                optionsIni.UpdateValue("videocapturesound", "1");
                optionsIni.UpdateValue("videocapturehideui", "1");
                optionsIni.UpdateValue("videocapturequality", "3");
                optionsIni.UpdateValue("videocapturetime", "5");

                //**** Online

                //*** set_onlineNotify
                if (launcherSettings.loadedData.onlineNotifications == 0)
                    optionsIni.UpdateValue("receiveconnecttns", "0");
                if (launcherSettings.loadedData.onlineNotifications == 1)
                    optionsIni.UpdateValue("receiveconnecttns", "1");
                //*** set_shopMode
                if (launcherSettings.loadedData.shopMode == 0)
                    optionsIni.UpdateValue("enableingamestore", "0");
                if (launcherSettings.loadedData.shopMode == 1)
                    optionsIni.UpdateValue("enableingamestore", "1");

                //*** Environment

                //*** set_summerSeason
                if (launcherSettings.loadedData.summerSeason == 0)
                {
                    optionsIni.UpdateValue("summerenabled", "0");
                    optionsIni.UpdateValue("summerlength", "28");
                }
                if (launcherSettings.loadedData.summerSeason == 1)
                {
                    optionsIni.UpdateValue("summerenabled", "1");
                    optionsIni.UpdateValue("summerlength", "7");
                }
                if (launcherSettings.loadedData.summerSeason == 2)
                {
                    optionsIni.UpdateValue("summerenabled", "1");
                    optionsIni.UpdateValue("summerlength", "14");
                }
                //*** set_winterSeason
                if (launcherSettings.loadedData.winterSeason == 0)
                {
                    optionsIni.UpdateValue("winterenabled", "0");
                    optionsIni.UpdateValue("winterlength", "28");
                }
                if (launcherSettings.loadedData.winterSeason == 1)
                {
                    optionsIni.UpdateValue("winterenabled", "1");
                    optionsIni.UpdateValue("winterlength", "7");
                }
                if (launcherSettings.loadedData.winterSeason == 2)
                {
                    optionsIni.UpdateValue("winterenabled", "1");
                    optionsIni.UpdateValue("winterlength", "14");
                }
                //*** set_fallSeason
                if (launcherSettings.loadedData.fallSeason == 0)
                {
                    optionsIni.UpdateValue("fallenabled", "0");
                    optionsIni.UpdateValue("falllength", "28");
                }
                if (launcherSettings.loadedData.fallSeason == 1)
                {
                    optionsIni.UpdateValue("fallenabled", "1");
                    optionsIni.UpdateValue("falllength", "7");
                }
                if (launcherSettings.loadedData.fallSeason == 2)
                {
                    optionsIni.UpdateValue("fallenabled", "1");
                    optionsIni.UpdateValue("falllength", "14");
                }
                //*** set_springSeason
                if (launcherSettings.loadedData.springSeason == 0)
                {
                    optionsIni.UpdateValue("springenabled", "0");
                    optionsIni.UpdateValue("springlength", "28");
                }
                if (launcherSettings.loadedData.springSeason == 1)
                {
                    optionsIni.UpdateValue("springenabled", "1");
                    optionsIni.UpdateValue("springlength", "7");
                }
                if (launcherSettings.loadedData.springSeason == 2)
                {
                    optionsIni.UpdateValue("springenabled", "1");
                    optionsIni.UpdateValue("springlength", "14");
                }
                //*** set_tempUnit
                if (launcherSettings.loadedData.temperatureUnit == 0)
                    optionsIni.UpdateValue("iscelcius", "0");
                if (launcherSettings.loadedData.temperatureUnit == 1)
                    optionsIni.UpdateValue("iscelcius", "1");
                //*** set_weatHail
                if (launcherSettings.loadedData.hailWeather == false)
                    optionsIni.UpdateValue("hailenabled", "0");
                if (launcherSettings.loadedData.hailWeather == true)
                    optionsIni.UpdateValue("hailenabled", "1");
                //*** set_weatRain
                if (launcherSettings.loadedData.rainWeather == false)
                    optionsIni.UpdateValue("rainenabled", "0");
                if (launcherSettings.loadedData.rainWeather == true)
                    optionsIni.UpdateValue("rainenabled", "1");
                //*** set_weatSnow
                if (launcherSettings.loadedData.snowWeather == false)
                    optionsIni.UpdateValue("snowenabled", "0");
                if (launcherSettings.loadedData.snowWeather == true)
                    optionsIni.UpdateValue("snowenabled", "1");
                //*** set_weatFog
                if (launcherSettings.loadedData.fogWeather == false)
                    optionsIni.UpdateValue("fogenabled", "0");
                if (launcherSettings.loadedData.fogWeather == true)
                    optionsIni.UpdateValue("fogenabled", "1");
                //*** set_lunarCycle
                if (launcherSettings.loadedData.lunarCycle == 0) //<- Normal - 2 Days
                {
                    optionsIni.UpdateValue("enablelunarcycle", "1");
                    optionsIni.UpdateValue("lunarcyclelength", "1");
                    optionsIni.UpdateValue("enablelunarphase", "0");
                    optionsIni.UpdateValue("lunarphaselength", "0");
                }
                if (launcherSettings.loadedData.lunarCycle == 1) //<- Normal - 4 Days
                {
                    optionsIni.UpdateValue("enablelunarcycle", "1");
                    optionsIni.UpdateValue("lunarcyclelength", "2");
                    optionsIni.UpdateValue("enablelunarphase", "0");
                    optionsIni.UpdateValue("lunarphaselength", "0");
                }
                if (launcherSettings.loadedData.lunarCycle == 2) //<- Normal - 6 Days
                {
                    optionsIni.UpdateValue("enablelunarcycle", "1");
                    optionsIni.UpdateValue("lunarcyclelength", "3");
                    optionsIni.UpdateValue("enablelunarphase", "0");
                    optionsIni.UpdateValue("lunarphaselength", "0");
                }
                if (launcherSettings.loadedData.lunarCycle == 3) //<- Normal - 8 Days
                {
                    optionsIni.UpdateValue("enablelunarcycle", "1");
                    optionsIni.UpdateValue("lunarcyclelength", "4");
                    optionsIni.UpdateValue("enablelunarphase", "0");
                    optionsIni.UpdateValue("lunarphaselength", "0");
                }
                if (launcherSettings.loadedData.lunarCycle == 4) //<- Normal - 10 Days
                {
                    optionsIni.UpdateValue("enablelunarcycle", "1");
                    optionsIni.UpdateValue("lunarcyclelength", "5");
                    optionsIni.UpdateValue("enablelunarphase", "0");
                    optionsIni.UpdateValue("lunarphaselength", "0");
                }
                if (launcherSettings.loadedData.lunarCycle == 5) //<- Fixed - Full Moon
                {
                    optionsIni.UpdateValue("enablelunarcycle", "0");
                    optionsIni.UpdateValue("lunarcyclelength", "1");
                    optionsIni.UpdateValue("enablelunarphase", "1");
                    optionsIni.UpdateValue("lunarphaselength", "0");
                }
                if (launcherSettings.loadedData.lunarCycle == 6) //<- Fixed - First Crescent
                {
                    optionsIni.UpdateValue("enablelunarcycle", "0");
                    optionsIni.UpdateValue("lunarcyclelength", "1");
                    optionsIni.UpdateValue("enablelunarphase", "1");
                    optionsIni.UpdateValue("lunarphaselength", "1");
                }
                if (launcherSettings.loadedData.lunarCycle == 7) //<- Fixed - Crescent
                {
                    optionsIni.UpdateValue("enablelunarcycle", "0");
                    optionsIni.UpdateValue("lunarcyclelength", "1");
                    optionsIni.UpdateValue("enablelunarphase", "1");
                    optionsIni.UpdateValue("lunarphaselength", "2");
                }
                if (launcherSettings.loadedData.lunarCycle == 8) //<- Fixed - Second Crescent
                {
                    optionsIni.UpdateValue("enablelunarcycle", "0");
                    optionsIni.UpdateValue("lunarcyclelength", "1");
                    optionsIni.UpdateValue("enablelunarphase", "1");
                    optionsIni.UpdateValue("lunarphaselength", "3");
                }
                if (launcherSettings.loadedData.lunarCycle == 9) //<- Fixed - New Moon
                {
                    optionsIni.UpdateValue("enablelunarcycle", "0");
                    optionsIni.UpdateValue("lunarcyclelength", "1");
                    optionsIni.UpdateValue("enablelunarphase", "1");
                    optionsIni.UpdateValue("lunarphaselength", "4");
                }
                if (launcherSettings.loadedData.lunarCycle == 10) //<- Fixed - First Waning
                {
                    optionsIni.UpdateValue("enablelunarcycle", "0");
                    optionsIni.UpdateValue("lunarcyclelength", "1");
                    optionsIni.UpdateValue("enablelunarphase", "1");
                    optionsIni.UpdateValue("lunarphaselength", "5");
                }
                if (launcherSettings.loadedData.lunarCycle == 11) //<- Fixed - Waning
                {
                    optionsIni.UpdateValue("enablelunarcycle", "0");
                    optionsIni.UpdateValue("lunarcyclelength", "1");
                    optionsIni.UpdateValue("enablelunarphase", "1");
                    optionsIni.UpdateValue("lunarphaselength", "6");
                }
                if (launcherSettings.loadedData.lunarCycle == 12) //<- Fixed - Second Waning
                {
                    optionsIni.UpdateValue("enablelunarcycle", "0");
                    optionsIni.UpdateValue("lunarcyclelength", "1");
                    optionsIni.UpdateValue("enablelunarphase", "1");
                    optionsIni.UpdateValue("lunarphaselength", "7");
                }

                //**** Demography

                //*** set_storyProgression
                if (launcherSettings.loadedData.storyProgression == 0)
                    optionsIni.UpdateValue("enablestoryprogression", "0");
                if (launcherSettings.loadedData.storyProgression == 1)
                    optionsIni.UpdateValue("enablestoryprogression", "1");
                //*** set_vampire
                if (launcherSettings.loadedData.allowVampires == false)
                    optionsIni.UpdateValue("enablevampires", "0");
                if (launcherSettings.loadedData.allowVampires == true)
                    optionsIni.UpdateValue("enablevampires", "1");
                //*** set_werewolves
                if (launcherSettings.loadedData.allowWerewolves == false)
                    optionsIni.UpdateValue("enablewerewolves", "0");
                if (launcherSettings.loadedData.allowWerewolves == true)
                    optionsIni.UpdateValue("enablewerewolves", "1");
                //*** set_pets
                if (launcherSettings.loadedData.allowPets == false)
                    optionsIni.UpdateValue("enablepets", "0");
                if (launcherSettings.loadedData.allowPets == true)
                    optionsIni.UpdateValue("enablepets", "1");
                //*** set_celebrities
                if (launcherSettings.loadedData.allowCelebrities == false)
                    optionsIni.UpdateValue("enablecelebrities", "0");
                if (launcherSettings.loadedData.allowCelebrities == true)
                    optionsIni.UpdateValue("enablecelebrities", "1");
                //*** set_fairies
                if (launcherSettings.loadedData.allowFairies == false)
                    optionsIni.UpdateValue("enablefairies", "0");
                if (launcherSettings.loadedData.allowFairies == true)
                    optionsIni.UpdateValue("enablefairies", "1");
                //*** set_witches
                if (launcherSettings.loadedData.allowWitches == false)
                    optionsIni.UpdateValue("enablewitches", "0");
                if (launcherSettings.loadedData.allowWitches == true)
                    optionsIni.UpdateValue("enablewitches", "1");
                //*** set_horses
                if (launcherSettings.loadedData.allowHorses == false)
                    optionsIni.UpdateValue("enablehorses", "0");
                if (launcherSettings.loadedData.allowHorses == true)
                    optionsIni.UpdateValue("enablehorses", "1");
                //*** set_celebritiesSystem
                if (launcherSettings.loadedData.disableCelebritiesSystem == 0)
                    optionsIni.UpdateValue("enableoptoutceleb", "0");
                if (launcherSettings.loadedData.disableCelebritiesSystem == 1)
                    optionsIni.UpdateValue("enableoptoutceleb", "1");

                //**** Constant

                //*** Inform current last device (driver version of main display) to avoid reset of settings on update gpu driver
                string[] currentLastDeviceInfo = optionsIni.GetValue("lastdevice").Split(";");
                string[] currentDriverVersionInfo = currentMainDisplayDriverVersion.Split(".");
                optionsIni.UpdateValue("lastdevice", (currentLastDeviceInfo[0] + ";" + currentLastDeviceInfo[1] + ";" + currentLastDeviceInfo[2] + ";" + currentDriverVersionInfo.Last()));
                //**** Inform to don't show login warning if is not logged in
                optionsIni.UpdateValue("requireloginbeforeload", "1");



                //Save the updates
                optionsIni.Save();
                smoothPatchOptionsTxt.Save();
                antiLagOptionsTxt.Save();

                //Return empty response
                return new string[] { };
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //Remove the added task
                RemoveTask("applySettings");

                //Restore the save button
                settingsSave.IsEnabled = true;
                set_ResetOptTemplate.IsEnabled = true;
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        private void UpdateLanguageOfKeyInRegistry(string key, string country, string locale)
        {
            //Get reference for HKEY_LOCAL_MACHINE
            RegistryKey hkeyLocalMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

            //Find the key
            RegistryKey targetKey = hkeyLocalMachine.OpenSubKey((@"" + key), true);

            //If not found the key, ignore it
            if (targetKey == null)
                return;

            //Update the key value and close it
            targetKey.SetValue("country", country);
            targetKey.SetValue("locale", locale);
            targetKey.Close();
        }
    
        private string GetMainDisplayDriverVersion()
        {
            //Prepare to return
            string toReturn = "0";

            //Start the DxDiag to get a relatory of all details about Directx on this machine
            Process process = new Process();
            process.StartInfo.FileName = System.IO.Path.Combine(Environment.SystemDirectory, "dxdiag.exe");
            process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            process.StartInfo.Arguments = "/x DxDiagRelatory.xml";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            //Wait process finishes
            process.WaitForExit();

            //Read the relatory output
            XmlDocument dxDiagRelatory = new XmlDocument();
            dxDiagRelatory.Load((Directory.GetCurrentDirectory() + "/DxDiagRelatory.xml"));
            //Get display devices node
            XmlNode displayDevices = dxDiagRelatory.DocumentElement.SelectSingleNode("/DxDiag/DisplayDevices");
            //Get main display driver version
            foreach (XmlNode node in displayDevices.ChildNodes[0].ChildNodes)
                if (node.Name.ToLower() == "driverversion")
                    toReturn = node.InnerText;

            //Return the result
            return toReturn;
        }
    
        private void AddNewOptionsKeysToExistingOptionsTemplate()
        {
            //If options template don't exists, cancel
            if (File.Exists((Directory.GetCurrentDirectory() + @"/Content/options-template.ini")) == false)
                return;
            //If options don't exists in mydocuments, cancel
            if (File.Exists((myDocumentsPath + "/Options.ini")) == false)
                return;

            //Read all lines of both options ini
            List<string> optionsTemplateLines = new List<string>();
            foreach (string line in File.ReadAllLines((Directory.GetCurrentDirectory() + @"/Content/options-template.ini")))
                optionsTemplateLines.Add(line);
            string[] optionsMyDocuments = File.ReadAllLines((myDocumentsPath + "/Options.ini"));

            //Check each line of options in my documents...
            for(int i = 0; i < optionsMyDocuments.Length; i++)
            {
                //If is title line, ignore it
                if (optionsMyDocuments[i].Contains("[options]") == true)
                    continue;

                //Get the current line key
                string currentKey = optionsMyDocuments[i].Split("=")[0].Replace(" ", "");

                //Prepare the result of this line exists
                bool thisKeyExistsInTemplate = false;
                //Check if this key exists in the options template
                for(int x = 0; x < optionsTemplateLines.Count; x++)
                    if(optionsTemplateLines[x].Contains(currentKey) == true)
                    {
                        thisKeyExistsInTemplate = true;
                        break;
                    }
                //If this key don't exists in the template, add it
                if(thisKeyExistsInTemplate == false)
                {
                    optionsTemplateLines.Add(optionsMyDocuments[i]);
                    optionsTemplateLines.Add("");
                }
            }

            //Save the updated options template
            File.WriteAllLines((Directory.GetCurrentDirectory() + @"/Content/options-template.ini"), optionsTemplateLines.ToArray());
        }
    
        private void RenderSimLifespan(int lifespanSelected)
        {
            //Render sim life time...

            //Prepare the variables
            string simBaby = "-";
            string simToddler = "-";
            string simChild = "-";
            string simTeen = "-";
            string simYoungAdult = "-";
            string simAdult = "-";
            string simElder = "-";
            string catKitten = "-";
            string catAdult = "-";
            string catElder = "-";
            string dogPuppy = "-";
            string dogAdult = "-";
            string dogElder = "-";
            string horseFoal = "-";
            string horseAdult = "-";
            string horseElder = "-";

            //Short
            if(lifespanSelected == 0)
            {
                //Sim
                simBaby = "2";
                simToddler = "2";
                simChild = "4";
                simTeen = "6";
                simYoungAdult = "8";
                simAdult = "8";
                simElder = "6";
                //Dog
                dogPuppy = "3";
                dogAdult = "10";
                dogElder = "4";
                //Cat
                catKitten = "3";
                catAdult = "10";
                catElder = "4";
                //Horse
                horseFoal = "2";
                horseAdult = "13";
                horseElder = "6";
            }

            //Medium
            if (lifespanSelected == 1)
            {
                //Sim
                simBaby = "3";
                simToddler = "6";
                simChild = "8";
                simTeen = "12";
                simYoungAdult = "15";
                simAdult = "15";
                simElder = "10";
                //Dog
                dogPuppy = "5";
                dogAdult = "20";
                dogElder = "8";
                //Cat
                catKitten = "5";
                catAdult = "22";
                catElder = "10";
                //Horse
                horseFoal = "4";
                horseAdult = "27";
                horseElder = "14";
            }

            //Normal
            if (lifespanSelected == 2)
            {
                //Sim
                simBaby = "5";
                simToddler = "9";
                simChild = "14";
                simTeen = "20";
                simYoungAdult = "25";
                simAdult = "25";
                simElder = "20";
                //Dog
                dogPuppy = "8";
                dogAdult = "32";
                dogElder = "14";
                //Cat
                catKitten = "8";
                catAdult = "36";
                catElder = "15";
                //Horse
                horseFoal = "10";
                horseAdult = "40";
                horseElder = "18";
            }

            //Long
            if (lifespanSelected == 3)
            {
                //Sim
                simBaby = "8";
                simToddler = "16";
                simChild = "28";
                simTeen = "50";
                simYoungAdult = "90";
                simAdult = "90";
                simElder = "25";
                //Dog
                dogPuppy = "26";
                dogAdult = "108";
                dogElder = "26";
                //Cat
                catKitten = "28";
                catAdult = "120";
                catElder = "26";
                //Horse
                horseFoal = "20";
                horseAdult = "150";
                horseElder = "26";
            }

            //Epic
            if (lifespanSelected == 4)
            {
                //Sim
                simBaby = "40";
                simToddler = "100";
                simChild = "115";
                simTeen = "160";
                simYoungAdult = "230";
                simAdult = "230";
                simElder = "120";
                //Dog
                dogPuppy = "90";
                dogAdult = "382";
                dogElder = "120";
                //Cat
                catKitten = "98";
                catAdult = "398";
                catElder = "128";
                //Horse
                horseFoal = "98";
                horseAdult = "450";
                horseElder = "158";
            }

            //Show it on texts...

            //Sim
            StringBuilder simLifespanTxt = new StringBuilder();
            simLifespanTxt.AppendLine(GetStringApplicationResource("launcher_settings_graphics_lifespanSimBaby").Replace("%d%", simBaby));
            simLifespanTxt.AppendLine(GetStringApplicationResource("launcher_settings_graphics_lifespanSimToddler").Replace("%d%", simToddler));
            simLifespanTxt.AppendLine(GetStringApplicationResource("launcher_settings_graphics_lifespanSimChild").Replace("%d%", simChild));
            simLifespanTxt.AppendLine(GetStringApplicationResource("launcher_settings_graphics_lifespanSimTeen").Replace("%d%", simTeen));
            simLifespanTxt.AppendLine(GetStringApplicationResource("launcher_settings_graphics_lifespanSimYoungAdult").Replace("%d%", simYoungAdult));
            simLifespanTxt.AppendLine(GetStringApplicationResource("launcher_settings_graphics_lifespanSimAdult").Replace("%d%", simAdult));
            simLifespanTxt.AppendLine(GetStringApplicationResource("launcher_settings_graphics_lifespanSimElder").Replace("%d%", simElder));
            simLifespan.Text = simLifespanTxt.ToString();
            //Cat
            StringBuilder catLifespanTxt = new StringBuilder();
            catLifespanTxt.AppendLine(GetStringApplicationResource("launcher_settings_graphics_lifespanCatKitten").Replace("%d%", catKitten));
            catLifespanTxt.AppendLine(GetStringApplicationResource("launcher_settings_graphics_lifespanCatAdult").Replace("%d%", catAdult));
            catLifespanTxt.AppendLine(GetStringApplicationResource("launcher_settings_graphics_lifespanCatElder").Replace("%d%", catElder));
            catLifespan.Text = catLifespanTxt.ToString();
            //Dog
            StringBuilder dogLifespanTxt = new StringBuilder();
            dogLifespanTxt.AppendLine(GetStringApplicationResource("launcher_settings_graphics_lifespanDogPuppy").Replace("%d%", dogPuppy));
            dogLifespanTxt.AppendLine(GetStringApplicationResource("launcher_settings_graphics_lifespanDogAdult").Replace("%d%", dogAdult));
            dogLifespanTxt.AppendLine(GetStringApplicationResource("launcher_settings_graphics_lifespanDogElder").Replace("%d%", dogElder));
            dogLifespan.Text = dogLifespanTxt.ToString();
            //Horse
            StringBuilder horseLifespanTxt = new StringBuilder();
            horseLifespanTxt.AppendLine(GetStringApplicationResource("launcher_settings_graphics_lifespanHorseFoal").Replace("%d%", horseFoal));
            horseLifespanTxt.AppendLine(GetStringApplicationResource("launcher_settings_graphics_lifespanHorseAdult").Replace("%d%", horseAdult));
            horseLifespanTxt.AppendLine(GetStringApplicationResource("launcher_settings_graphics_lifespanHorseElder").Replace("%d%", horseElder));
            horseLifespan.Text = horseLifespanTxt.ToString();
        }
    
        //Cache manager

        private void BuildAndPrepareCacheCleanListSystem()
        {
            //Prepare the close button of logs viewer popup
            logsViewerClose.Click += (s, e) => { CloseErrorTrapLogsViewerPopUp(); };

            //---------------// Error Trap Logs //---------------//
            if (launcherPrefs.loadedData.patchBasicOptimization == true)
            {
                //Instantiate and store reference for it
                CacheItem newCacheItem = new CacheItem(this);
                cacheList.Children.Add(newCacheItem);
                instantiatedCacheItems.Add(newCacheItem);
                //Set it up
                newCacheItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newCacheItem.VerticalAlignment = VerticalAlignment.Top;
                newCacheItem.Width = double.NaN;
                newCacheItem.Height = double.NaN;
                newCacheItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this item
                newCacheItem.SetTasksID("errorTrapCache");
                newCacheItem.SetIcon("Resources/cache-0a.png");
                newCacheItem.SetTitle(GetStringApplicationResource("launcher_cache_item0atitle"));
                newCacheItem.SetDescription(GetStringApplicationResource("launcher_cache_item0adescription"));
                newCacheItem.SetMyDocumentsPath(myDocumentsPath);
                //Set the garbage size checker code
                newCacheItem.RegisterOnCalculateGarbageCallback((myDocumentsPath, mainWindow) =>
                {
                    //Prepare the cache size
                    int currentLogsCount = 0;

                    //Count log quantity
                    foreach (FileInfo file in (new DirectoryInfo(myDocumentsPath).GetFiles()))
                        if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".xml")
                            if (file.Name.Contains("ScriptError_") == true)
                                currentLogsCount += 1;

                    //Run on UI
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        //Get the last errors count
                        int lastErrorsCount = mainWindow.launcherPrefs.loadedData.lastErrorLogsCount;

                        //If the current logs count is not zero, check if was increased
                        if(currentLogsCount > 0)
                            if(currentLogsCount > lastErrorsCount)
                            {
                                //Warn about this
                                mainWindow.ShowToast(mainWindow.GetStringApplicationResource("launcher_cache_newErrorLogMsg"), ToastType.Error);
                                mainWindow.EnablePageRedDotNotification(LauncherPage.cache);
                            }

                        //Save the errors count
                        mainWindow.launcherPrefs.loadedData.lastErrorLogsCount = currentLogsCount;
                        mainWindow.launcherPrefs.Save();
                    }));

                    //Return the cache size
                    return (currentLogsCount + " Logs");
                });
                //Set the clean code
                newCacheItem.RegisterOnClickClearCallback((myDocumentsPath) =>
                {
                    //Prepare the list of files to delete
                    List<string> toDelete = new List<string>();

                    //Delete all logs
                    foreach (FileInfo file in (new DirectoryInfo(myDocumentsPath).GetFiles()))
                        if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".xml")
                            if (file.Name.Contains("ScriptError_") == true)
                                toDelete.Add(file.FullName);

                    //Delete files
                    foreach (string file in toDelete)
                        File.Delete(file);
                });
                //Set the additional button
                newCacheItem.SetAdditionalButton(GetStringApplicationResource("launcher_cache_seeLogsButton"), () => { OpenErrorTrapLogsViewerPopUp(); });
                //Do the first garbage calc
                newCacheItem.CalculateThisCacheSize();
            }

            //---------------// Launcher Cache //---------------//
            if (true == true)
            {
                //Instantiate and store reference for it
                CacheItem newCacheItem = new CacheItem(this);
                cacheList.Children.Add(newCacheItem);
                instantiatedCacheItems.Add(newCacheItem);
                //Set it up
                newCacheItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newCacheItem.VerticalAlignment = VerticalAlignment.Top;
                newCacheItem.Width = double.NaN;
                newCacheItem.Height = double.NaN;
                newCacheItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this item
                newCacheItem.SetTasksID("dreamLauncherCache");
                newCacheItem.SetIcon("Resources/icon-64x.png");
                newCacheItem.SetTitle(GetStringApplicationResource("launcher_cache_item0title"));
                newCacheItem.SetDescription(GetStringApplicationResource("launcher_cache_item0description"));
                newCacheItem.SetMyDocumentsPath(myDocumentsPath);
                //Set the garbage size checker code
                newCacheItem.RegisterOnCalculateGarbageCallback((myDocumentsPath, mainWindow) => 
                {
                    //Prepare the cache size
                    double cacheSize = 0;

                    //Calculate all cache size
                    foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/!DL-TmpCache")).GetFiles()))
                        cacheSize += file.Length;

                    //Return the cache size
                    return GetFormattedCacheSize(cacheSize);
                });
                //Set the clean code
                newCacheItem.RegisterOnClickClearCallback((myDocumentsPath) => 
                {
                    //Prepare a list of files to be cleaned
                    List<string> filesToBeRemoved = new List<string>();
                    //Search by files to remove
                    foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/!DL-TmpCache")).GetFiles()))
                        filesToBeRemoved.Add(file.FullName);
                    //Remove all files
                    foreach (string filePath in filesToBeRemoved)
                        if (File.Exists(filePath) == true)
                            File.Delete(filePath);

                    //Prepare a list of folders to be cleaned
                    List<string> foldersToBeRemoved = new List<string>();
                    //Search by folders to remove
                    foreach (DirectoryInfo dir in (new DirectoryInfo((myDocumentsPath + "/!DL-TmpCache")).GetDirectories()))
                        foldersToBeRemoved.Add(dir.FullName);
                    //Remove all directories
                    foreach (string dirPath in foldersToBeRemoved)
                        if (Directory.Exists(dirPath) == true)
                            Directory.Delete(dirPath, true);
                });
                //Do the first garbage calc
                newCacheItem.CalculateThisCacheSize();
            }

            //---------------// CAS Parts //---------------//
            if (true == true)
            {
                //Instantiate and store reference for it
                CacheItem newCacheItem = new CacheItem(this);
                cacheList.Children.Add(newCacheItem);
                instantiatedCacheItems.Add(newCacheItem);
                //Set it up
                newCacheItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newCacheItem.VerticalAlignment = VerticalAlignment.Top;
                newCacheItem.Width = double.NaN;
                newCacheItem.Height = double.NaN;
                newCacheItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this item
                newCacheItem.SetTasksID("casPartsCache");
                newCacheItem.SetIcon("Resources/cache-1.png");
                newCacheItem.SetTitle(GetStringApplicationResource("launcher_cache_item1title"));
                newCacheItem.SetDescription(GetStringApplicationResource("launcher_cache_item1description"));
                newCacheItem.SetMyDocumentsPath(myDocumentsPath);
                //Set the garbage size checker code
                newCacheItem.RegisterOnCalculateGarbageCallback((myDocumentsPath, mainWindow) =>
                {
                    //Prepare the cache size
                    double cacheSize = 0;

                    //Calculate all cache size
                    if (File.Exists((myDocumentsPath + "/CASPartCache.package")) == true)
                        cacheSize += (new FileInfo((myDocumentsPath + "/CASPartCache.package"))).Length;

                    //Return the cache size
                    return GetFormattedCacheSize(cacheSize);
                });
                //Set the clean code
                newCacheItem.RegisterOnClickClearCallback((myDocumentsPath) =>
                {
                    //Delete the file
                    if (File.Exists((myDocumentsPath + "/CASPartCache.package")) == true)
                        File.Delete((myDocumentsPath + "/CASPartCache.package"));
                });
                //Do the first garbage calc
                newCacheItem.CalculateThisCacheSize();
            }

            //---------------// Compositor //---------------//
            if (true == true)
            {
                //Instantiate and store reference for it
                CacheItem newCacheItem = new CacheItem(this);
                cacheList.Children.Add(newCacheItem);
                instantiatedCacheItems.Add(newCacheItem);
                //Set it up
                newCacheItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newCacheItem.VerticalAlignment = VerticalAlignment.Top;
                newCacheItem.Width = double.NaN;
                newCacheItem.Height = double.NaN;
                newCacheItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this item
                newCacheItem.SetTasksID("compositorCache");
                newCacheItem.SetIcon("Resources/cache-2.png");
                newCacheItem.SetTitle(GetStringApplicationResource("launcher_cache_item2title"));
                newCacheItem.SetDescription(GetStringApplicationResource("launcher_cache_item2description"));
                newCacheItem.SetMyDocumentsPath(myDocumentsPath);
                //Set the garbage size checker code
                newCacheItem.RegisterOnCalculateGarbageCallback((myDocumentsPath, mainWindow) =>
                {
                    //Prepare the cache size
                    double cacheSize = 0;

                    //Calculate all cache size
                    if (File.Exists((myDocumentsPath + "/compositorCache.package")) == true)
                        cacheSize += (new FileInfo((myDocumentsPath + "/compositorCache.package"))).Length;

                    //Return the cache size
                    return GetFormattedCacheSize(cacheSize);
                });
                //Set the clean code
                newCacheItem.RegisterOnClickClearCallback((myDocumentsPath) =>
                {
                    //Delete the file
                    if (File.Exists((myDocumentsPath + "/compositorCache.package")) == true)
                        File.Delete((myDocumentsPath + "/compositorCache.package"));
                });
                //Do the first garbage calc
                newCacheItem.CalculateThisCacheSize();
            }

            //---------------// Script //---------------//
            if (true == true)
            {
                //Instantiate and store reference for it
                CacheItem newCacheItem = new CacheItem(this);
                cacheList.Children.Add(newCacheItem);
                instantiatedCacheItems.Add(newCacheItem);
                //Set it up
                newCacheItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newCacheItem.VerticalAlignment = VerticalAlignment.Top;
                newCacheItem.Width = double.NaN;
                newCacheItem.Height = double.NaN;
                newCacheItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this item
                newCacheItem.SetTasksID("scriptCache");
                newCacheItem.SetIcon("Resources/cache-3.png");
                newCacheItem.SetTitle(GetStringApplicationResource("launcher_cache_item3title"));
                newCacheItem.SetDescription(GetStringApplicationResource("launcher_cache_item3description"));
                newCacheItem.SetMyDocumentsPath(myDocumentsPath);
                //Set the garbage size checker code
                newCacheItem.RegisterOnCalculateGarbageCallback((myDocumentsPath, mainWindow) =>
                {
                    //Prepare the cache size
                    double cacheSize = 0;

                    //Calculate all cache size
                    if (File.Exists((myDocumentsPath + "/scriptCache.package")) == true)
                        cacheSize += (new FileInfo((myDocumentsPath + "/scriptCache.package"))).Length;

                    //Return the cache size
                    return GetFormattedCacheSize(cacheSize);
                });
                //Set the clean code
                newCacheItem.RegisterOnClickClearCallback((myDocumentsPath) =>
                {
                    //Delete the file
                    if (File.Exists((myDocumentsPath + "/scriptCache.package")) == true)
                        File.Delete((myDocumentsPath + "/scriptCache.package"));
                });
                //Do the first garbage calc
                newCacheItem.CalculateThisCacheSize();
            }

            //---------------// Sim Compositor //---------------//
            if (true == true)
            {
                //Instantiate and store reference for it
                CacheItem newCacheItem = new CacheItem(this);
                cacheList.Children.Add(newCacheItem);
                instantiatedCacheItems.Add(newCacheItem);
                //Set it up
                newCacheItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newCacheItem.VerticalAlignment = VerticalAlignment.Top;
                newCacheItem.Width = double.NaN;
                newCacheItem.Height = double.NaN;
                newCacheItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this item
                newCacheItem.SetTasksID("simCompositorCache");
                newCacheItem.SetIcon("Resources/cache-4.png");
                newCacheItem.SetTitle(GetStringApplicationResource("launcher_cache_item4title"));
                newCacheItem.SetDescription(GetStringApplicationResource("launcher_cache_item4description"));
                newCacheItem.SetMyDocumentsPath(myDocumentsPath);
                //Set the garbage size checker code
                newCacheItem.RegisterOnCalculateGarbageCallback((myDocumentsPath, mainWindow) =>
                {
                    //Prepare the cache size
                    double cacheSize = 0;

                    //Calculate all cache size
                    if (File.Exists((myDocumentsPath + "/simCompositorCache.package")) == true)
                        cacheSize += (new FileInfo((myDocumentsPath + "/simCompositorCache.package"))).Length;

                    //Return the cache size
                    return GetFormattedCacheSize(cacheSize);
                });
                //Set the clean code
                newCacheItem.RegisterOnClickClearCallback((myDocumentsPath) =>
                {
                    //Delete the file
                    if (File.Exists((myDocumentsPath + "/simCompositorCache.package")) == true)
                        File.Delete((myDocumentsPath + "/simCompositorCache.package"));
                });
                //Do the first garbage calc
                newCacheItem.CalculateThisCacheSize();
            }

            //---------------// Social //---------------//
            if (true == true)
            {
                //Instantiate and store reference for it
                CacheItem newCacheItem = new CacheItem(this);
                cacheList.Children.Add(newCacheItem);
                instantiatedCacheItems.Add(newCacheItem);
                //Set it up
                newCacheItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newCacheItem.VerticalAlignment = VerticalAlignment.Top;
                newCacheItem.Width = double.NaN;
                newCacheItem.Height = double.NaN;
                newCacheItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this item
                newCacheItem.SetTasksID("socialCache");
                newCacheItem.SetIcon("Resources/cache-5.png");
                newCacheItem.SetTitle(GetStringApplicationResource("launcher_cache_item5title"));
                newCacheItem.SetDescription(GetStringApplicationResource("launcher_cache_item5description"));
                newCacheItem.SetMyDocumentsPath(myDocumentsPath);
                //Set the garbage size checker code
                newCacheItem.RegisterOnCalculateGarbageCallback((myDocumentsPath, mainWindow) =>
                {
                    //Prepare the cache size
                    double cacheSize = 0;

                    //Calculate all cache size
                    if (File.Exists((myDocumentsPath + "/socialCache.package")) == true)
                        cacheSize += (new FileInfo((myDocumentsPath + "/socialCache.package"))).Length;

                    //Return the cache size
                    return GetFormattedCacheSize(cacheSize);
                });
                //Set the clean code
                newCacheItem.RegisterOnClickClearCallback((myDocumentsPath) =>
                {
                    //Delete the file
                    if (File.Exists((myDocumentsPath + "/socialCache.package")) == true)
                        File.Delete((myDocumentsPath + "/socialCache.package"));
                });
                //Do the first garbage calc
                newCacheItem.CalculateThisCacheSize();
            }

            //---------------// Repository Index Cache //---------------//
            if (true == true)
            {
                //Instantiate and store reference for it
                CacheItem newCacheItem = new CacheItem(this);
                cacheList.Children.Add(newCacheItem);
                instantiatedCacheItems.Add(newCacheItem);
                //Set it up
                newCacheItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newCacheItem.VerticalAlignment = VerticalAlignment.Top;
                newCacheItem.Width = double.NaN;
                newCacheItem.Height = double.NaN;
                newCacheItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this item
                newCacheItem.SetTasksID("repoIndexCache");
                newCacheItem.SetIcon("Resources/cache-6.png");
                newCacheItem.SetTitle(GetStringApplicationResource("launcher_cache_item6title"));
                newCacheItem.SetDescription(GetStringApplicationResource("launcher_cache_item6description"));
                newCacheItem.SetMyDocumentsPath(myDocumentsPath);
                //Set the garbage size checker code
                newCacheItem.RegisterOnCalculateGarbageCallback((myDocumentsPath, mainWindow) =>
                {
                    //Prepare the cache size
                    double cacheSize = 0;

                    //Calculate all cache size
                    //missingdeps.idx
                    if (File.Exists((myDocumentsPath + "/DCCache/missingdeps.idx")) == true)
                        cacheSize += (new FileInfo((myDocumentsPath + "/DCCache/missingdeps.idx"))).Length;
                    //dcc.ent
                    if (File.Exists((myDocumentsPath + "/DCCache/dcc.ent")) == true)
                        cacheSize += (new FileInfo((myDocumentsPath + "/DCCache/dcc.ent"))).Length;
                    //Downloads/*.bin
                    foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/Downloads")).GetFiles()))
                        if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".bin")
                            cacheSize += file.Length;
                    //SigsCache/*.bin
                    foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/SigsCache")).GetFiles()))
                        if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".bin")
                            cacheSize += file.Length;
                    //SavedSims/Downloadedsims.index
                    if (Directory.Exists((myDocumentsPath + "/SavedSims")) == true)
                        if (File.Exists((myDocumentsPath + "/SavedSims/Downloadedsims.index")) == true)
                            cacheSize += (new FileInfo((myDocumentsPath + "/SavedSims/Downloadedsims.index"))).Length;
                    //IGACache/*.*
                    foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/IGACache")).GetFiles()))
                        cacheSize += file.Length;

                    //Return the cache size
                    return GetFormattedCacheSize(cacheSize);
                });
                //Set the clean code
                newCacheItem.RegisterOnClickClearCallback((myDocumentsPath) =>
                {
                    //Prepare the list of files to delete
                    List<string> toDelete = new List<string>();

                    //Delete all cache
                    //missingdeps.idx
                    if (File.Exists((myDocumentsPath + "/DCCache/missingdeps.idx")) == true)
                        toDelete.Add((myDocumentsPath + "/DCCache/missingdeps.idx"));
                    //dcc.ent
                    if (File.Exists((myDocumentsPath + "/DCCache/dcc.ent")) == true)
                        toDelete.Add((myDocumentsPath + "/DCCache/dcc.ent"));
                    //Downloads/*.bin
                    foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/Downloads")).GetFiles()))
                        if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".bin")
                            toDelete.Add(file.FullName);
                    //SigsCache/*.bin
                    foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/SigsCache")).GetFiles()))
                        if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".bin")
                            toDelete.Add(file.FullName);
                    //SavedSims/Downloadedsims.index
                    if (File.Exists((myDocumentsPath + "/SavedSims/Downloadedsims.index")) == true)
                        toDelete.Add((myDocumentsPath + "/SavedSims/Downloadedsims.index"));
                    //IGACache/*.*
                    foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/IGACache")).GetFiles()))
                        toDelete.Add(file.FullName);

                    //Delete all marked files
                    foreach (string filePath in toDelete)
                        File.Delete(filePath);
                });
                //Do the first garbage calc
                newCacheItem.CalculateThisCacheSize();
            }

            //---------------// Thumbnails //---------------//
            if (true == true)
            {
                //Instantiate and store reference for it
                CacheItem newCacheItem = new CacheItem(this);
                cacheList.Children.Add(newCacheItem);
                instantiatedCacheItems.Add(newCacheItem);
                //Set it up
                newCacheItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newCacheItem.VerticalAlignment = VerticalAlignment.Top;
                newCacheItem.Width = double.NaN;
                newCacheItem.Height = double.NaN;
                newCacheItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this item
                newCacheItem.SetTasksID("thumbsCache");
                newCacheItem.SetIcon("Resources/cache-7.png");
                newCacheItem.SetTitle(GetStringApplicationResource("launcher_cache_item7title"));
                newCacheItem.SetDescription(GetStringApplicationResource("launcher_cache_item7description"));
                newCacheItem.SetMyDocumentsPath(myDocumentsPath);
                //Set the garbage size checker code
                newCacheItem.RegisterOnCalculateGarbageCallback((myDocumentsPath, mainWindow) =>
                {
                    //Prepare the cache size
                    double cacheSize = 0;

                    //Calculate all cache size
                    //Thumbnails/*.package
                    foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/Thumbnails")).GetFiles()))
                        if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".package")
                            cacheSize += file.Length;
                    //FeaturedItems/thumb_*.png
                    foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/FeaturedItems")).GetFiles()))
                        if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".png")
                            if(file.Name.Contains("thumb_") == true)
                                cacheSize += file.Length;

                    //Return the cache size
                    return GetFormattedCacheSize(cacheSize);
                });
                //Set the clean code
                newCacheItem.RegisterOnClickClearCallback((myDocumentsPath) =>
                {
                    //Prepare the list of files to delete
                    List<string> toDelete = new List<string>();

                    //Thumbnails/*.package
                    foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/Thumbnails")).GetFiles()))
                        if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".package")
                            toDelete.Add(file.FullName);
                    //FeaturedItems/thumb_*.png
                    foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/FeaturedItems")).GetFiles()))
                        if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".png")
                            if (file.Name.Contains("thumb_") == true)
                                toDelete.Add(file.FullName);

                    //Delete all marked files
                    foreach (string filePath in toDelete)
                        File.Delete(filePath);
                });
                //Do the first garbage calc
                newCacheItem.CalculateThisCacheSize();
            }

            //---------------// Worldcache //---------------//
            if (true == true)
            {
                //Instantiate and store reference for it
                CacheItem newCacheItem = new CacheItem(this);
                cacheList.Children.Add(newCacheItem);
                instantiatedCacheItems.Add(newCacheItem);
                //Set it up
                newCacheItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newCacheItem.VerticalAlignment = VerticalAlignment.Top;
                newCacheItem.Width = double.NaN;
                newCacheItem.Height = double.NaN;
                newCacheItem.Margin = new Thickness(0, 8, 0, 0);
                //Fill this item
                newCacheItem.SetTasksID("worldCache");
                newCacheItem.SetIcon("Resources/cache-8.png");
                newCacheItem.SetTitle(GetStringApplicationResource("launcher_cache_item8title"));
                newCacheItem.SetDescription(GetStringApplicationResource("launcher_cache_item8description"));
                newCacheItem.SetMyDocumentsPath(myDocumentsPath);
                //Set the garbage size checker code
                newCacheItem.RegisterOnCalculateGarbageCallback((myDocumentsPath, mainWindow) =>
                {
                    //Prepare the cache size
                    double cacheSize = 0;

                    //Calculate all cache size
                    if (Directory.Exists((myDocumentsPath + "/WorldCaches")) == true)
                        cacheSize += GetDirectorySize(new DirectoryInfo((myDocumentsPath + "/WorldCaches")));

                    //Return the cache size
                    return GetFormattedCacheSize(cacheSize);
                });
                //Set the clean code
                newCacheItem.RegisterOnClickClearCallback((myDocumentsPath) =>
                {
                    //Clear the cache
                    if (Directory.Exists((myDocumentsPath + "/WorldCaches")) == true)
                        Directory.Delete((myDocumentsPath + "/WorldCaches"), true);
                });
                //Do the first garbage calc
                newCacheItem.CalculateThisCacheSize();
            }


            //Add the final spacer
            Grid finalSpacer = new Grid();
            cacheList.Children.Add(finalSpacer);
            //Set it up
            finalSpacer.HorizontalAlignment = HorizontalAlignment.Stretch;
            finalSpacer.VerticalAlignment = VerticalAlignment.Top;
            finalSpacer.Width = double.NaN;
            finalSpacer.Height = 16.0f;
        }

        private string GetFormattedCacheSize(double bytesSize)
        {
            //Calculate to MB, KB and GB
            float gbSize = (float)(((bytesSize / 1000.0f) / 1000.0f) / 1000.0f);
            float mbSize = (float)((bytesSize / 1000.0f) / 1000.0f);
            float kbSize = (float)(bytesSize / 1000.0f);

            //Fix the KB size
            if (kbSize > 0.0f && kbSize < 1.0f)
                kbSize = 1.0f;
            //Calculate the pre size prefix
            string prefix = ((kbSize > 0.0f) ? "~": "");

            //Prepare the final size
            string formattedSize = "";

            //Select the correct unit
            if (mbSize < 1)
                formattedSize = (prefix + kbSize.ToString("F0") + " KB");
            if (mbSize >= 1 && mbSize < 1000)
                formattedSize = (prefix + mbSize.ToString("F1") + " MB");
            if (mbSize >= 1000)
                formattedSize = (prefix + gbSize.ToString("F1") + " GB");

            //Return the cache size
            return formattedSize;
        }

        private void RecalculateAllCacheTypesSizes()
        {
            //Recalculate all cache types sizes
            foreach (CacheItem item in instantiatedCacheItems)
                item.CalculateThisCacheSize();
        }
    
        private void OpenErrorTrapLogsViewerPopUp()
        {
            //If the routine is already running, stop it
            if (logsViewerOpenRoutine != null)
            {
                logsViewerOpenRoutine.Dispose();
                logsViewerOpenRoutine = null;
            }

            //Start the logs viewer open routine
            logsViewerOpenRoutine = Coroutine.Start(LogsViewerOpenRoutine());
        }

        private IEnumerator LogsViewerOpenRoutine()
        {
            //Show the title
            logsViewerTitle.Content = (GetStringApplicationResource("launcher_cache_logsViewerTitle") + " - " + GetStringApplicationResource("launcher_cache_logsViewerLoad"));
            //Hide no logs warn
            logsViewerEmpty.Visibility = Visibility.Collapsed;

            //Open the popup running animation
            logsViewerPopUp.Visibility = Visibility.Visible;
            animStoryboards["logsViewerEntry"].Begin();

            //Wait end of animation
            yield return new WaitForSeconds(0.5);

            //Stop the animation
            animStoryboards["logsViewerEntry"].Stop();

            //Prepare the logs count
            int logsCount = 0;
            bool alreadyInstantiatedTheFirst = false;
            //Instatiate each log file
            foreach (FileInfo file in (new DirectoryInfo(myDocumentsPath).GetFiles()))
                if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".xml")
                    if (file.Name.Contains("ScriptError_") == true)
                    {
                        //Create the log item to display
                        LogItem newLogItem = new LogItem(this);
                        logsViewerList.Children.Add(newLogItem);
                        instantiatedLogItems.Add(newLogItem);

                        //Configure it
                        newLogItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                        newLogItem.VerticalAlignment = VerticalAlignment.Top;
                        newLogItem.Width = double.NaN;
                        newLogItem.Height = double.NaN;
                        if (alreadyInstantiatedTheFirst == false)
                            newLogItem.Margin = new Thickness(0, 8, 0, 8);
                        if (alreadyInstantiatedTheFirst == true)
                            newLogItem.Margin = new Thickness(0, 0, 0, 8);

                        //Inform the data about the log
                        newLogItem.SetTitle(file.Name);
                        newLogItem.SetNppPath((Directory.GetCurrentDirectory() + @"/Content/tool-npp"), (Directory.GetCurrentDirectory() + @"/Content/tool-npp/notepad++.exe"));
                        newLogItem.SetLogPath(file.FullName);
                        newLogItem.Prepare();

                        //Increase the log count
                        logsCount += 1;
                        alreadyInstantiatedTheFirst = true;

                        //Wait some time, before instantiate the next item, to avoid UI freezing
                        yield return new WaitForSeconds(0.05f);
                    }

            //Update the title
            logsViewerTitle.Content = (GetStringApplicationResource("launcher_cache_logsViewerTitle") + " - " + GetStringApplicationResource("launcher_cache_logsViewerCounter").Replace("%n%", logsCount.ToString()));

            //Prepare the no logs warn
            if (logsCount == 0)
                logsViewerEmpty.Visibility = Visibility.Visible;
            if (logsCount >= 1)
                logsViewerEmpty.Visibility = Visibility.Collapsed;

            //Clear this routine reference
            logsViewerOpenRoutine = null;
        }

        private void CloseErrorTrapLogsViewerPopUp()
        {
            //Start the logs viewer close routine
            IDisposable logsViewerCloseRoutine = Coroutine.Start(LogsViewerCloseRoutine());
        }

        private IEnumerator LogsViewerCloseRoutine()
        {
            //Close the popup running animation
            animStoryboards["logsViewerExit"].Begin();

            //Wait end of animation
            yield return new WaitForSeconds(0.5);

            //Close the popup
            logsViewerPopUp.Visibility = Visibility.Collapsed;
            //Stop the animation
            animStoryboards["logsViewerExit"].Stop();

            //Clear all instantiated logs
            foreach (LogItem item in instantiatedLogItems)
                logsViewerList.Children.Remove(item);
            instantiatedLogItems.Clear();
        }
    
        //Tools manager

        private void BuildAndPrepareToolsListSystem()
        {
            //---------------// Notepad++ //---------------//
            if (true == true)
            {
                //Instantiate and store reference for it
                ToolItem newToolItem = new ToolItem(this);
                toolsList.Children.Add(newToolItem);
                instantiatedToolItems.Add(newToolItem);
                //Set it up
                newToolItem.HorizontalAlignment = HorizontalAlignment.Left;
                newToolItem.VerticalAlignment = VerticalAlignment.Stretch;
                newToolItem.Width = double.NaN;
                newToolItem.Height = double.NaN;
                newToolItem.Margin = new Thickness(4, 4, 4, 4);
                //Fill this item
                newToolItem.SetIcon("Resources/tool-npp.png");
                newToolItem.SetInformation(GetStringApplicationResource("launcher_tools_nppInformation"));
                newToolItem.SetTitle(GetStringApplicationResource("launcher_tools_nppTitle"));
                newToolItem.SetDescription(GetStringApplicationResource("launcher_tools_nppDescription"));
                newToolItem.SetCreator("Don Ho");
                newToolItem.SetContentsFolderPath((Directory.GetCurrentDirectory() + @"/Content"));
                newToolItem.SetToolFolderName("tool-npp");
                newToolItem.SetToolExePathInsideFolder("notepad++.exe");
                newToolItem.SetMyDocumentsPath(myDocumentsPath);
                newToolItem.SetToolDownloadURL("https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/tool-notepadpp.zip");
                //Prepare the tool
                newToolItem.Prepare();
            }

            //---------------// S3PE //---------------//
            if (true == true)
            {
                //Instantiate and store reference for it
                ToolItem newToolItem = new ToolItem(this);
                toolsList.Children.Add(newToolItem);
                instantiatedToolItems.Add(newToolItem);
                //Set it up
                newToolItem.HorizontalAlignment = HorizontalAlignment.Left;
                newToolItem.VerticalAlignment = VerticalAlignment.Stretch;
                newToolItem.Width = double.NaN;
                newToolItem.Height = double.NaN;
                newToolItem.Margin = new Thickness(4, 4, 4, 4);
                //Fill this item
                newToolItem.SetIcon("Resources/tool-s3pe.png");
                newToolItem.SetInformation(GetStringApplicationResource("launcher_tools_s3peInformation"));
                newToolItem.SetTitle(GetStringApplicationResource("launcher_tools_s3peTitle"));
                newToolItem.SetDescription(GetStringApplicationResource("launcher_tools_s3peDescription"));
                newToolItem.SetCreator("Peter L Jones");
                newToolItem.SetContentsFolderPath((Directory.GetCurrentDirectory() + @"/Content"));
                newToolItem.SetToolFolderName("tool-s3pe");
                newToolItem.SetToolExePathInsideFolder("s3pe.exe");
                newToolItem.SetMyDocumentsPath(myDocumentsPath);
                newToolItem.SetToolDownloadURL("https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/tool-s3pe.zip");
                //Prepare the tool
                newToolItem.Prepare();
            }

            //---------------// S3OC //---------------//
            if (true == true)
            {
                //Instantiate and store reference for it
                ToolItem newToolItem = new ToolItem(this);
                toolsList.Children.Add(newToolItem);
                instantiatedToolItems.Add(newToolItem);
                //Set it up
                newToolItem.HorizontalAlignment = HorizontalAlignment.Left;
                newToolItem.VerticalAlignment = VerticalAlignment.Stretch;
                newToolItem.Width = double.NaN;
                newToolItem.Height = double.NaN;
                newToolItem.Margin = new Thickness(4, 4, 4, 4);
                //Fill this item
                newToolItem.SetIcon("Resources/tool-s3oc.png");
                newToolItem.SetInformation(GetStringApplicationResource("launcher_tools_s3ocInformation"));
                newToolItem.SetTitle(GetStringApplicationResource("launcher_tools_s3ocTitle"));
                newToolItem.SetDescription(GetStringApplicationResource("launcher_tools_s3ocDescription"));
                newToolItem.SetCreator("Peter L Jones");
                newToolItem.SetContentsFolderPath((Directory.GetCurrentDirectory() + @"/Content"));
                newToolItem.SetToolFolderName("tool-s3oc");
                newToolItem.SetToolExePathInsideFolder("s3oc.exe");
                newToolItem.SetMyDocumentsPath(myDocumentsPath);
                newToolItem.SetToolDownloadURL("https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/tool-s3oc.zip");
                //Prepare the tool
                newToolItem.Prepare();
            }

            //---------------// Dashboard //---------------//
            if (true == true)
            {
                //Instantiate and store reference for it
                ToolItem newToolItem = new ToolItem(this);
                toolsList.Children.Add(newToolItem);
                instantiatedToolItems.Add(newToolItem);
                //Set it up
                newToolItem.HorizontalAlignment = HorizontalAlignment.Left;
                newToolItem.VerticalAlignment = VerticalAlignment.Stretch;
                newToolItem.Width = double.NaN;
                newToolItem.Height = double.NaN;
                newToolItem.Margin = new Thickness(4, 4, 4, 4);
                //Fill this item
                newToolItem.SetIcon("Resources/tool-dashboard.png");
                newToolItem.SetInformation(GetStringApplicationResource("launcher_tools_dshInformation"));
                newToolItem.SetTitle(GetStringApplicationResource("launcher_tools_dshTitle"));
                newToolItem.SetDescription(GetStringApplicationResource("launcher_tools_dshDescription"));
                newToolItem.SetCreator("Tashiketh");
                newToolItem.SetContentsFolderPath((Directory.GetCurrentDirectory() + @"/Content"));
                newToolItem.SetToolFolderName("tool-dashboard");
                newToolItem.SetToolExePathInsideFolder("Sims3Dashboard.exe");
                newToolItem.SetMyDocumentsPath(myDocumentsPath);
                newToolItem.SetToolDownloadURL("https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/tool-dashboard.zip");
                //Prepare the tool
                newToolItem.Prepare();
            }

            //---------------// Easy STBL Manager //---------------//
            if (true == true)
            {
                //Instantiate and store reference for it
                ToolItem newToolItem = new ToolItem(this);
                toolsList.Children.Add(newToolItem);
                instantiatedToolItems.Add(newToolItem);
                //Set it up
                newToolItem.HorizontalAlignment = HorizontalAlignment.Left;
                newToolItem.VerticalAlignment = VerticalAlignment.Stretch;
                newToolItem.Width = double.NaN;
                newToolItem.Height = double.NaN;
                newToolItem.Margin = new Thickness(4, 4, 4, 4);
                //Fill this item
                newToolItem.SetIcon("Resources/tool-ese.png");
                newToolItem.SetInformation(GetStringApplicationResource("launcher_tools_eseInformation"));
                newToolItem.SetTitle(GetStringApplicationResource("launcher_tools_eseTitle"));
                newToolItem.SetDescription(GetStringApplicationResource("launcher_tools_eseDescription"));
                newToolItem.SetCreator("CmarNYC");
                newToolItem.SetContentsFolderPath((Directory.GetCurrentDirectory() + @"/Content"));
                newToolItem.SetToolFolderName("tool-ese");
                newToolItem.SetToolExePathInsideFolder("EasySTBLmanager.exe");
                newToolItem.SetMyDocumentsPath(myDocumentsPath);
                newToolItem.SetToolDownloadURL("https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/tool-ese.zip");
                //Prepare the tool
                newToolItem.Prepare();
            }

            //---------------// Package Viewer //---------------//
            if (true == true)
            {
                //Instantiate and store reference for it
                ToolItem newToolItem = new ToolItem(this);
                toolsList.Children.Add(newToolItem);
                instantiatedToolItems.Add(newToolItem);
                //Set it up
                newToolItem.HorizontalAlignment = HorizontalAlignment.Left;
                newToolItem.VerticalAlignment = VerticalAlignment.Stretch;
                newToolItem.Width = double.NaN;
                newToolItem.Height = double.NaN;
                newToolItem.Margin = new Thickness(4, 4, 4, 4);
                //Fill this item
                newToolItem.SetIcon("Resources/tool-pkgv.png");
                newToolItem.SetInformation(GetStringApplicationResource("launcher_tools_pkgInformation"));
                newToolItem.SetTitle(GetStringApplicationResource("launcher_tools_pkgTitle"));
                newToolItem.SetDescription(GetStringApplicationResource("launcher_tools_pkgDescription"));
                newToolItem.SetCreator("Kuree");
                newToolItem.SetContentsFolderPath((Directory.GetCurrentDirectory() + @"/Content"));
                newToolItem.SetToolFolderName("tool-pkgv");
                newToolItem.SetToolExePathInsideFolder("PackageViewer.exe");
                newToolItem.SetMyDocumentsPath(myDocumentsPath);
                newToolItem.SetToolDownloadURL("https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/tool-pkgv.zip");
                //Prepare the tool
                newToolItem.Prepare();
            }

            //---------------// Sims3Pack Multi Installer //---------------//
            if (true == true)
            {
                //Instantiate and store reference for it
                ToolItem newToolItem = new ToolItem(this);
                toolsList.Children.Add(newToolItem);
                instantiatedToolItems.Add(newToolItem);
                //Set it up
                newToolItem.HorizontalAlignment = HorizontalAlignment.Left;
                newToolItem.VerticalAlignment = VerticalAlignment.Stretch;
                newToolItem.Width = double.NaN;
                newToolItem.Height = double.NaN;
                newToolItem.Margin = new Thickness(4, 4, 4, 4);
                //Fill this item
                newToolItem.SetIcon("Resources/tool-s3mi.png");
                newToolItem.SetInformation(GetStringApplicationResource("launcher_tools_s3miInformation"));
                newToolItem.SetTitle(GetStringApplicationResource("launcher_tools_s3miTitle"));
                newToolItem.SetDescription(GetStringApplicationResource("launcher_tools_s3miDescription"));
                newToolItem.SetCreator("Tashiketh");
                newToolItem.SetContentsFolderPath((Directory.GetCurrentDirectory() + @"/Content"));
                newToolItem.SetToolFolderName("tool-s3mi");
                newToolItem.SetToolExePathInsideFolder("Sims3PackMultiInstaller.exe");
                newToolItem.SetMyDocumentsPath(myDocumentsPath);
                newToolItem.SetToolDownloadURL("https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/tool-s3mi.zip");
                //Prepare the tool
                newToolItem.Prepare();
            }

            //---------------// 7zip //---------------//
            if (true == true)
            {
                //Instantiate and store reference for it
                ToolItem newToolItem = new ToolItem(this);
                toolsList.Children.Add(newToolItem);
                instantiatedToolItems.Add(newToolItem);
                //Set it up
                newToolItem.HorizontalAlignment = HorizontalAlignment.Left;
                newToolItem.VerticalAlignment = VerticalAlignment.Stretch;
                newToolItem.Width = double.NaN;
                newToolItem.Height = double.NaN;
                newToolItem.Margin = new Thickness(4, 4, 4, 4);
                //Fill this item
                newToolItem.SetIcon("Resources/tool-szip.png");
                newToolItem.SetInformation(GetStringApplicationResource("launcher_tools_szipInformation"));
                newToolItem.SetTitle(GetStringApplicationResource("launcher_tools_szipTitle"));
                newToolItem.SetDescription(GetStringApplicationResource("launcher_tools_szipDescription"));
                newToolItem.SetCreator("Igor Pavlov");
                newToolItem.SetContentsFolderPath((Directory.GetCurrentDirectory() + @"/Content"));
                newToolItem.SetToolFolderName("tool-szip");
                newToolItem.SetToolExePathInsideFolder("7zFM.exe");
                newToolItem.SetMyDocumentsPath(myDocumentsPath);
                newToolItem.SetToolDownloadURL("https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/tool-szip.zip");
                //Prepare the tool
                newToolItem.Prepare();
            }

            //---------------// Sims3Pack Command Line Interface Extractor //---------------//
            if (true == true)
            {
                //Instantiate and store reference for it
                ToolItem newToolItem = new ToolItem(this);
                toolsList.Children.Add(newToolItem);
                instantiatedToolItems.Add(newToolItem);
                //Set it up
                newToolItem.HorizontalAlignment = HorizontalAlignment.Left;
                newToolItem.VerticalAlignment = VerticalAlignment.Stretch;
                newToolItem.Width = double.NaN;
                newToolItem.Height = double.NaN;
                newToolItem.Margin = new Thickness(4, 4, 4, 4);
                //Fill this item
                newToolItem.SetIcon("Resources/tool-s3ce.png");
                newToolItem.SetInformation(GetStringApplicationResource("launcher_tools_s3ceInformation"));
                newToolItem.SetTitle(GetStringApplicationResource("launcher_tools_s3ceTitle"));
                newToolItem.SetDescription(GetStringApplicationResource("launcher_tools_s3ceDescription"));
                newToolItem.SetCreator("MarkJS");
                newToolItem.SetContentsFolderPath((Directory.GetCurrentDirectory() + @"/Content"));
                newToolItem.SetToolFolderName("tool-s3ce");
                newToolItem.SetToolExePathInsideFolder("s3ce.exe");
                newToolItem.SetMyDocumentsPath(myDocumentsPath);
                newToolItem.SetToolDownloadURL("https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/tool-s3ce.zip");
                //Prepare the tool
                newToolItem.Prepare();
            }

            //---------------// CASPs Editor //---------------//
            if (true == true)
            {
                //Instantiate and store reference for it
                ToolItem newToolItem = new ToolItem(this);
                toolsList.Children.Add(newToolItem);
                instantiatedToolItems.Add(newToolItem);
                //Set it up
                newToolItem.HorizontalAlignment = HorizontalAlignment.Left;
                newToolItem.VerticalAlignment = VerticalAlignment.Stretch;
                newToolItem.Width = double.NaN;
                newToolItem.Height = double.NaN;
                newToolItem.Margin = new Thickness(4, 4, 4, 4);
                //Fill this item
                newToolItem.SetIcon("Resources/tool-caspe.png");
                newToolItem.SetInformation(GetStringApplicationResource("launcher_tools_caspeInformation"));
                newToolItem.SetTitle(GetStringApplicationResource("launcher_tools_caspeTitle"));
                newToolItem.SetDescription(GetStringApplicationResource("launcher_tools_caspeDescription"));
                newToolItem.SetCreator("marcos4503");
                newToolItem.SetContentsFolderPath((Directory.GetCurrentDirectory() + @"/Content"));
                newToolItem.SetToolFolderName("tool-caspe");
                newToolItem.SetToolExePathInsideFolder("Dl3CASPsEditor.exe");
                newToolItem.SetMyDocumentsPath(myDocumentsPath);
                newToolItem.SetToolDownloadURL("https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/tool-caspe.zip");
                //Prepare the tool
                newToolItem.Prepare();
            }
        }
    
        //Mods Manager

        private void BuildAndPrepareModsListSystem()
        {
            //Disable the mods button. Will be enabled after the first installed mods update
            goMods.IsEnabled = false;

            //If don't have mods support patch installed, hide the mods button
            if(launcherPrefs.loadedData.patchModsSupport == false)
            {
                goMods.IsEnabled = false;
                return;
            }

            //Scan the mods folder and warn if found unrecognized files or directories inside
            ScanModsFolderAndRemoveAllUnrecognizedFilesAndFolders();

            //Setup the updates in the mods tabs
            modsTabs.SelectionChanged += (s, e) => 
            {
                //If this event is not being fired by the mods tabs, cancel
                if (e.OriginalSource != modsTabs)
                    return;

                //Update the recommended mods list, if is changed to add tab
                if (modsTabs.SelectedIndex == 1)
                {
                    UpdateRecommendedModsList();
                    SetModInstallScreen(ModScreen.Recommended);
                }

                //Update the installed mods list, if is changed to installed tab
                if (modsTabs.SelectedIndex == 0)
                    UpdateInstalledMods();
            };

            //Prepare the search box
            instModsSearchBar.TextChanged += (s, e) => { ApplySearchAndCategoryFilters(); };
            instModsSearchBar.LostFocus += (s, e) =>
            {
                if (instModsSearchBar.Text == "")
                    instModsSearchHint.Visibility = Visibility.Visible;
            };
            instModsSearchBar.GotFocus += (s, e) => { instModsSearchHint.Visibility = Visibility.Collapsed; };

            //Prepare the category buttons
            instModsAll.Click += (s, e) => { SetInstalledModsCategoryFilter(ModCategory.All); };
            instModsContents.Click += (s, e) => { SetInstalledModsCategoryFilter(ModCategory.Contents); };
            instModsGraphics.Click += (s, e) => { SetInstalledModsCategoryFilter(ModCategory.Graphics); };
            instModsSounds.Click += (s, e) => { SetInstalledModsCategoryFilter(ModCategory.Sounds); };
            instModsFixes.Click += (s, e) => { SetInstalledModsCategoryFilter(ModCategory.Fixes); };
            instModsGameplay.Click += (s, e) => { SetInstalledModsCategoryFilter(ModCategory.Gameplay); };
            instModsSliders.Click += (s, e) => { SetInstalledModsCategoryFilter(ModCategory.Sliders); };
            instModsOthers.Click += (s, e) => { SetInstalledModsCategoryFilter(ModCategory.Others); };
            instModsPatches.Click += (s, e) => { SetInstalledModsCategoryFilter(ModCategory.FromPatches); };

            //Update the installed mods and show the "All" category automatically
            UpdateInstalledMods();

            //Prepare the add mod screens buttons
            installRecommended.Click += (s, e) => { SetModInstallScreen(ModScreen.Recommended); };
            installCustom.Click += (s, e) => { SetModInstallScreen(ModScreen.Custom); };

            //Auto change the Add tab to see Recommended screen
            SetModInstallScreen(ModScreen.Recommended);

            //Prepare the recommended category buttons
            recModsAll.Click += (s, e) => { SetRecommendedModsCategoryFilter(ModCategory.All); };
            recModsContents.Click += (s, e) => { SetRecommendedModsCategoryFilter(ModCategory.Contents); };
            recModsGraphics.Click += (s, e) => { SetRecommendedModsCategoryFilter(ModCategory.Graphics); };
            recModsSounds.Click += (s, e) => { SetRecommendedModsCategoryFilter(ModCategory.Sounds); };
            recModsFixes.Click += (s, e) => { SetRecommendedModsCategoryFilter(ModCategory.Fixes); };
            recModsGameplay.Click += (s, e) => { SetRecommendedModsCategoryFilter(ModCategory.Gameplay); };
            recModsSliders.Click += (s, e) => { SetRecommendedModsCategoryFilter(ModCategory.Sliders); };
            recModsOthers.Click += (s, e) => { SetRecommendedModsCategoryFilter(ModCategory.Others); };

            //Prepare the button picker
            instCustomButton.Click += (s, e) => { OpenCustomPackageModPicker(); };
        }

        public void UpdateInstalledMods()
        {
            //If the routine is already running, stop it
            if (installedModsUpdateRoutine != null)
            {
                installedModsUpdateRoutine.Dispose();
                installedModsUpdateRoutine = null;
            }

            //Start the installed mods update routine
            installedModsUpdateRoutine = Coroutine.Start(UpdateInstalledModsRoutine());
        }

        private IEnumerator UpdateInstalledModsRoutine()
        {
            //Show the loading indicator
            instModsLoading.Visibility = Visibility.Visible;
            animStoryboards["installedModsLoadExit"].Stop();
            //Wait end of animation
            yield return new WaitForSeconds(0.2f);
            
            //Clear all mods previously rendered
            foreach (InstalledModItem item in instantiatedModsItems)
                instModsList.Children.Remove(item);
            instantiatedModsItems.Clear();

            //Prepare the mods counters
            int contentsCounter = 0;
            int graphicsCounter = 0;
            int soundsCounter = 0;
            int fixesCounter = 0;
            int gameplayCounter = 0;
            int slidersCounter = 0;
            int othersCounter = 0;
            int patchesCounter = 0;

            //Query all mods of all categories
            List<string> allModsList = new List<string>();
            QueryAllModsOfCategoryInFolder(ref allModsList, ref contentsCounter, "Packages/DL3-Recommended", "CONTENTS --- ");
            QueryAllModsOfCategoryInFolder(ref allModsList, ref contentsCounter, "Packages/DL3-Custom", "CONTENTS --- ");
            QueryAllModsOfCategoryInFolder(ref allModsList, ref graphicsCounter, "Packages/DL3-Recommended", "GRAPHICS --- ");
            QueryAllModsOfCategoryInFolder(ref allModsList, ref graphicsCounter, "Packages/DL3-Custom", "GRAPHICS --- ");
            QueryAllModsOfCategoryInFolder(ref allModsList, ref soundsCounter, "Packages/DL3-Recommended", "SOUNDS --- ");
            QueryAllModsOfCategoryInFolder(ref allModsList, ref soundsCounter, "Packages/DL3-Custom", "SOUNDS --- ");
            QueryAllModsOfCategoryInFolder(ref allModsList, ref fixesCounter, "Packages/DL3-Recommended", "FIXES --- ");
            QueryAllModsOfCategoryInFolder(ref allModsList, ref fixesCounter, "Packages/DL3-Custom", "FIXES --- ");
            QueryAllModsOfCategoryInFolder(ref allModsList, ref gameplayCounter, "Packages/DL3-Recommended", "GAMEPLAY --- ");
            QueryAllModsOfCategoryInFolder(ref allModsList, ref gameplayCounter, "Packages/DL3-Custom", "GAMEPLAY --- ");
            QueryAllModsOfCategoryInFolder(ref allModsList, ref slidersCounter, "Packages/DL3-Recommended", "SLIDERS --- ");
            QueryAllModsOfCategoryInFolder(ref allModsList, ref slidersCounter, "Packages/DL3-Custom", "SLIDERS --- ");
            QueryAllModsOfCategoryInFolder(ref allModsList, ref othersCounter, "Packages/DL3-Recommended", "OTHERS --- ");
            QueryAllModsOfCategoryInFolder(ref allModsList, ref othersCounter, "Packages/DL3-Custom", "OTHERS --- ");
            QueryAllModsOfCategoryInFolder(ref allModsList, ref patchesCounter, "Packages/DL3-Patches", "");
            QueryAllModsOfCategoryInFolder(ref allModsList, ref patchesCounter, "Packages", "");

            //Render all mods...
            foreach(string item in allModsList)
            {
                //Create the item to display
                InstalledModItem newItem = new InstalledModItem(this);
                instModsList.Children.Add(newItem);
                instantiatedModsItems.Add(newItem);

                //Configure it
                newItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newItem.VerticalAlignment = VerticalAlignment.Top;
                newItem.Width = double.NaN;
                newItem.Height = double.NaN;
                newItem.Margin = new Thickness(0, 0, 0, 4);

                //Inform the data about the mod
                newItem.SetModPath(item);
                newItem.SetContentsPath((Directory.GetCurrentDirectory() + @"/Content"));
                newItem.Prepare();

                //Wait before render next to avoid UI freezing
                yield return new WaitForSeconds(0.01f);
            }

            //Show counters in all categories
            int allModsCount = (contentsCounter + graphicsCounter + soundsCounter + fixesCounter + gameplayCounter + slidersCounter + othersCounter + patchesCounter);
            instModsAll.Content = (GetStringApplicationResource("launcher_mods_installedTab_cat_all") + " - " + (allModsCount));
            instModsContents.Content = (GetStringApplicationResource("launcher_mods_installedTab_cat_contents") + " - " + (contentsCounter));
            instModsGraphics.Content = (GetStringApplicationResource("launcher_mods_installedTab_cat_graphics") + " - " + (graphicsCounter));
            instModsSounds.Content = (GetStringApplicationResource("launcher_mods_installedTab_cat_sounds") + " - " + (soundsCounter));
            instModsFixes.Content = (GetStringApplicationResource("launcher_mods_installedTab_cat_fixes") + " - " + (fixesCounter));
            instModsGameplay.Content = (GetStringApplicationResource("launcher_mods_installedTab_cat_gameplay") + " - " + (gameplayCounter));
            instModsSliders.Content = (GetStringApplicationResource("launcher_mods_installedTab_cat_sliders") + " - " + (slidersCounter));
            instModsOthers.Content = (GetStringApplicationResource("launcher_mods_installedTab_cat_others") + " - " + (othersCounter));
            instModsPatches.Content = ("FP - " + (patchesCounter));

            //Count the total size of mods folder
            long totalSizeInBytes = 0;
            foreach (string modPath in allModsList)
                if (File.Exists(modPath) == true)
                    totalSizeInBytes += (new FileInfo(modPath)).Length;
            //Render the mods folder size
            modsFolderSize.Text = GetFormattedCacheSize(totalSizeInBytes).Replace("~", "");

            //Auto change to category to all
            SetInstalledModsCategoryFilter(currentSeeingModsCategory);

            //Run the animation of loading exit
            animStoryboards["installedModsLoadExit"].Begin();
            //Wait end of animation
            yield return new WaitForSeconds(0.2f);
            //Hide the loading indicator
            instModsLoading.Visibility = Visibility.Collapsed;

            //Make the mods button available again, if is not available
            if(goMods.IsEnabled == false)
                goMods.IsEnabled = true;

            //Auto clear routine reference
            installedModsUpdateRoutine = null;
        }

        private void QueryAllModsOfCategoryInFolder(ref List<string> filesListToAdd, ref int counterToIncrease, string folderPathToQuery, string categoryCheckerPrefix)
        {
            //Query all mods
            foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/Mods/" + folderPathToQuery)).GetFiles()))
                if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".package" || System.IO.Path.GetExtension(file.FullName).ToLower() == ".disabled")
                    if (System.IO.Path.GetFileNameWithoutExtension(file.FullName).Contains(categoryCheckerPrefix) == true)
                    {
                        filesListToAdd.Add(file.FullName);
                        counterToIncrease += 1;
                    }    
        }

        private void SetInstalledModsCategoryFilter(ModCategory category)
        {
            //Prepare the data
            Color btSelectedColor = Color.FromArgb(255, 44, 103, 169);
            Color btUnselectedColor = Color.FromArgb(255, 168, 171, 175);

            //Prepare the dictionary of categories
            int categoryIdToShow = -1;
            switch (category)
            {
                case ModCategory.All:
                    categoryIdToShow = 0;
                    break;
                case ModCategory.Contents:
                    categoryIdToShow = 1;
                    break;
                case ModCategory.Graphics:
                    categoryIdToShow = 2;
                    break;
                case ModCategory.Sounds:
                    categoryIdToShow = 3;
                    break;
                case ModCategory.Fixes:
                    categoryIdToShow = 4;
                    break;
                case ModCategory.Gameplay:
                    categoryIdToShow = 5;
                    break;
                case ModCategory.Sliders:
                    categoryIdToShow = 6;
                    break;
                case ModCategory.Others:
                    categoryIdToShow = 7;
                    break;
                case ModCategory.FromPatches:
                    categoryIdToShow = 8;
                    break;
            }

            //Prepare the list of buttons
            List<Controls.BeautyButton.BeautyButton> buttons = new List<Controls.BeautyButton.BeautyButton>();
            buttons.Add(instModsAll);
            buttons.Add(instModsContents);
            buttons.Add(instModsGraphics);
            buttons.Add(instModsSounds);
            buttons.Add(instModsFixes);
            buttons.Add(instModsGameplay);
            buttons.Add(instModsSliders);
            buttons.Add(instModsOthers);
            buttons.Add(instModsPatches);

            //If was found a ID
            if (categoryIdToShow != -1)
            {
                //Unhighlight all buttons
                for (int i = 0; i < buttons.Count; i++)
                    buttons[i].Background = new SolidColorBrush(btUnselectedColor);

                //Highlight the desired button
                buttons[categoryIdToShow].Background = new SolidColorBrush(btSelectedColor);
            }

            //Clear the lists
            buttons.Clear();

            //Inform the current viewing category
            currentSeeingModsCategory = category;

            //If category is different from all, disable the search
            if(category != ModCategory.All)
            {
                instModsSearch.Opacity = 0.25f;
                instModsSearch.IsHitTestVisible = false;
                instModsSearchBar.Text = "";
                instModsSearchHint.Visibility = Visibility.Visible;
            }
            //If category is all, enable the search
            if (category == ModCategory.All)
            {
                instModsSearch.Opacity = 1.0f;
                instModsSearch.IsHitTestVisible = true;
                instModsSearchBar.Text = "";
            }

            //Auto apply the filter
            ApplySearchAndCategoryFilters();
        }

        private void ApplySearchAndCategoryFilters()
        {
            //If the routine is already running, stop it
            if (installedModsFilterRoutine != null)
            {
                installedModsFilterRoutine.Dispose();
                installedModsFilterRoutine = null;
            }

            //Start the installed mods filter routine
            installedModsFilterRoutine = Coroutine.Start(ApplyFilterOnInstalledModsRoutine());
        }

        private IEnumerator ApplyFilterOnInstalledModsRoutine()
        {
            //Show the loading indicator
            instModsFiltering.Visibility = Visibility.Visible;
            //Wait some time before apply filter
            yield return new WaitForSeconds(0.15f);

            //Hide the empty mods warning
            instModsEmpty.Visibility = Visibility.Collapsed;
            //Hide search results warning
            instModsSearchResults.Visibility = Visibility.Collapsed;

            //If have something typed in the search bar, change the color
            if (instModsSearchBar.Text != "")
                instModsSearchBar.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 176, 1));
            if (instModsSearchBar.Text == "")
                instModsSearchBar.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 171, 173, 179));

            //Hide all items
            foreach (InstalledModItem item in instantiatedModsItems)
            {
                //Hide
                item.Visibility = Visibility.Collapsed;

                //Wait before render next to avoid UI freezing
                yield return new WaitForSeconds(0.0025f);
            }

            //Enable only that corresponds with the filter...

            //Enable the items that corresponds with category
            foreach (InstalledModItem item in instantiatedModsItems)
            {
                //If is not seeing all, enable only mods that corresponds with desired category
                if (currentSeeingModsCategory != ModCategory.All)
                {
                    if (currentSeeingModsCategory == ModCategory.Contents && item.modCategory == InstalledModItem.ModCategory.Contents)
                        item.Visibility = Visibility.Visible;
                    if (currentSeeingModsCategory == ModCategory.Graphics && item.modCategory == InstalledModItem.ModCategory.Graphics)
                        item.Visibility = Visibility.Visible;
                    if (currentSeeingModsCategory == ModCategory.Sounds && item.modCategory == InstalledModItem.ModCategory.Sounds)
                        item.Visibility = Visibility.Visible;
                    if (currentSeeingModsCategory == ModCategory.Fixes && item.modCategory == InstalledModItem.ModCategory.Fixes)
                        item.Visibility = Visibility.Visible;
                    if (currentSeeingModsCategory == ModCategory.Gameplay && item.modCategory == InstalledModItem.ModCategory.Gameplay)
                        item.Visibility = Visibility.Visible;
                    if (currentSeeingModsCategory == ModCategory.Sliders && item.modCategory == InstalledModItem.ModCategory.Sliders)
                        item.Visibility = Visibility.Visible;
                    if (currentSeeingModsCategory == ModCategory.Others && item.modCategory == InstalledModItem.ModCategory.Others)
                        item.Visibility = Visibility.Visible;
                    if (currentSeeingModsCategory == ModCategory.FromPatches && item.modCategory == InstalledModItem.ModCategory.Patches)
                        item.Visibility = Visibility.Visible;
                }
                //If is seeing all, enable it
                if (currentSeeingModsCategory == ModCategory.All)
                {
                    //Enable it
                    item.Visibility = Visibility.Visible;

                    //If have something typed in search, disable if don't have the searched terms
                    if (instModsSearchBar.Text != "")
                        if (item.title.Text.ToLower().Contains(instModsSearchBar.Text.ToLower()) == false)
                            item.Visibility = Visibility.Collapsed;
                }

                //Wait before render next to avoid UI freezing
                yield return new WaitForSeconds(0.0025f);
            }

            //Count visible mods and show empty warning if necessary
            int visibleModsCount = 0;
            //Count
            foreach (InstalledModItem item in instantiatedModsItems)
                if (item.Visibility == Visibility.Visible)
                    visibleModsCount += 1;
            //If don't have visible mods, warn
            if (visibleModsCount == 0)
                instModsEmpty.Visibility = Visibility.Visible;
            //If have more than zero visible mods, and is in "All" category, with searche terms typed, show result quantity
            if(visibleModsCount > 0 && currentSeeingModsCategory == ModCategory.All && instModsSearchBar.Text != "")
            {
                instModsSearchResults.Content = GetStringApplicationResource("launcher_mods_installedTab_modsSearchResult").Replace("%n%", visibleModsCount.ToString());
                instModsSearchResults.Visibility = Visibility.Visible;
            }

            //Hide the loading indicator
            instModsFiltering.Visibility = Visibility.Collapsed;

            //Auto clear routine reference
            installedModsFilterRoutine = null;
        }
    
        private void ScanModsFolderAndRemoveAllUnrecognizedFilesAndFolders()
        {
            //Get the mydocuments fixed path
            string fixedMyDocumentsPath = myDocumentsPath.Replace("/", "\\");

            //------------------------------------------------------------------------//
            //------------------------------ Check Mods ------------------------------//
            //------------------------------------------------------------------------//
            if (true == true)
            {
                //Query all files
                List<string> filesList = new List<string>();
                foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/Mods")).GetFiles()))
                    filesList.Add(file.FullName);
                //Query all directories
                List<string> foldersList = new List<string>();
                foreach (DirectoryInfo dir in (new DirectoryInfo((myDocumentsPath + "/Mods")).GetDirectories()))
                    foldersList.Add(dir.FullName);

                //Delete unrecognized files
                foreach(string filePath in filesList)
                {
                    //Prepare information
                    bool deleteThis = false;

                    //Get the file name
                    string fileName = System.IO.Path.GetFileName(filePath);

                    //If is not a valid file
                    if (fileName != "!DON'T-Touch-Nothing-Here!" && fileName != "!NAO-Toque-Em-Nada-Aqui!" && fileName != "DreamLauncher.dl3" && fileName != "Resource.cfg")
                        deleteThis = true;

                    //If is not desired to delete this, continues
                    if (deleteThis == false)
                        continue;

                    //Delete it
                    File.Delete(filePath);
                    //Warn
                    ShowToast(GetStringApplicationResource("launcher_mods_deleteUnrecognized").Replace("%file%", (filePath.Replace((fixedMyDocumentsPath + "\\"), "")) ), ToastType.Error);
                }

                //Delete unrecognized folders
                foreach (string folderPath in foldersList)
                {
                    //Prepare information
                    bool deleteThis = false;

                    //Get the folder name
                    string dirName = new System.IO.DirectoryInfo(folderPath).Name;

                    //If is not a valid folder
                    if (dirName != "Cache" && dirName != "Overrides" && dirName != "Packages")
                        deleteThis = true;

                    //If is not desired to delete this, continues
                    if (deleteThis == false)
                        continue;

                    //Delete it
                    Directory.Delete(folderPath, true);
                    //Warn
                    ShowToast(GetStringApplicationResource("launcher_mods_deleteUnrecognized").Replace("%file%", (folderPath.Replace((fixedMyDocumentsPath + "\\"), ""))), ToastType.Error);
                }
            }

            //---------------------------------------------------------------------------------//
            //------------------------------ Check Mods/Packages ------------------------------//
            //---------------------------------------------------------------------------------//
            if (true == true)
            {
                //Query all files
                List<string> filesList = new List<string>();
                foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/Mods/Packages")).GetFiles()))
                    filesList.Add(file.FullName);
                //Query all directories
                List<string> foldersList = new List<string>();
                foreach (DirectoryInfo dir in (new DirectoryInfo((myDocumentsPath + "/Mods/Packages")).GetDirectories()))
                    foldersList.Add(dir.FullName);

                //Delete unrecognized files
                foreach (string filePath in filesList)
                {
                    //Prepare information
                    bool deleteThis = false;

                    //Get the file name
                    string fileName = System.IO.Path.GetFileName(filePath);

                    //If is not a valid file
                    if (fileName != "!DON'T-Touch-Nothing-Here!" && fileName != "!NAO-Toque-Em-Nada-Aqui!" && fileName != "NoIntro.package" && fileName != "NoModInfo.package" && fileName != "ZoomInCAS.package")
                        deleteThis = true;

                    //If is not desired to delete this, continues
                    if (deleteThis == false)
                        continue;

                    //Delete it
                    File.Delete(filePath);
                    //Warn
                    ShowToast(GetStringApplicationResource("launcher_mods_deleteUnrecognized").Replace("%file%", (filePath.Replace((fixedMyDocumentsPath + "\\"), ""))), ToastType.Error);
                }

                //Delete unrecognized folders
                foreach (string folderPath in foldersList)
                {
                    //Prepare information
                    bool deleteThis = false;

                    //Get the folder name
                    string dirName = new System.IO.DirectoryInfo(folderPath).Name;

                    //If is not a valid folder
                    if (dirName != "DL3-Custom" && dirName != "DL3-Patches" && dirName != "DL3-Recommended")
                        deleteThis = true;

                    //If is not desired to delete this, continues
                    if (deleteThis == false)
                        continue;

                    //Delete it
                    Directory.Delete(folderPath, true);
                    //Warn
                    ShowToast(GetStringApplicationResource("launcher_mods_deleteUnrecognized").Replace("%file%", (folderPath.Replace((fixedMyDocumentsPath + "\\"), ""))), ToastType.Error);
                }
            }

            //---------------------------------------------------------------------------------------------//
            //------------------------------ Check Mods/Packages/DL3-Patches ------------------------------//
            //---------------------------------------------------------------------------------------------//
            if (true == true)
            {
                //Query all files
                List<string> filesList = new List<string>();
                foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/Mods/Packages/DL3-Patches")).GetFiles()))
                    filesList.Add(file.FullName);
                //Query all directories
                List<string> foldersList = new List<string>();
                foreach (DirectoryInfo dir in (new DirectoryInfo((myDocumentsPath + "/Mods/Packages/DL3-Patches")).GetDirectories()))
                    foldersList.Add(dir.FullName);

                //Delete unrecognized files
                foreach (string filePath in filesList)
                {
                    //Prepare information
                    bool deleteThis = false;

                    //Get the file name
                    string fileExtension = System.IO.Path.GetExtension(filePath).Replace(".", "").ToLower();

                    //If is not a valid file
                    if (fileExtension != "package" && fileExtension != "disabled")
                        deleteThis = true;

                    //If is not desired to delete this, continues
                    if (deleteThis == false)
                        continue;

                    //Delete it
                    File.Delete(filePath);
                    //Warn
                    ShowToast(GetStringApplicationResource("launcher_mods_deleteUnrecognized").Replace("%file%", (filePath.Replace((fixedMyDocumentsPath + "\\"), ""))), ToastType.Error);
                }

                //Delete unrecognized folders
                foreach (string folderPath in foldersList)
                {
                    //Prepare information
                    bool deleteThis = false;

                    //Delete any folder
                    deleteThis = true;

                    //If is not desired to delete this, continues
                    if (deleteThis == false)
                        continue;

                    //Delete it
                    Directory.Delete(folderPath, true);
                    //Warn
                    ShowToast(GetStringApplicationResource("launcher_mods_deleteUnrecognized").Replace("%file%", (folderPath.Replace((fixedMyDocumentsPath + "\\"), ""))), ToastType.Error);
                }
            }

            //--------------------------------------------------------------------------------------------//
            //------------------------------ Check Mods/Packages/DL3-Custom ------------------------------//
            //--------------------------------------------------------------------------------------------//
            if (true == true)
            {
                //Query all files
                List<string> filesList = new List<string>();
                foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/Mods/Packages/DL3-Custom")).GetFiles()))
                    filesList.Add(file.FullName);
                //Query all directories
                List<string> foldersList = new List<string>();
                foreach (DirectoryInfo dir in (new DirectoryInfo((myDocumentsPath + "/Mods/Packages/DL3-Custom")).GetDirectories()))
                    foldersList.Add(dir.FullName);

                //Delete unrecognized files
                foreach (string filePath in filesList)
                {
                    //Prepare information
                    bool deleteThis = false;

                    //Get the file name and extension
                    string fileExtension = System.IO.Path.GetExtension(filePath).Replace(".", "").ToLower();
                    string[] filePrefixAndName = System.IO.Path.GetFileNameWithoutExtension(filePath).Split(" --- ");

                    //If is not a valid file extension
                    if (fileExtension != "package" && fileExtension != "disabled")
                        deleteThis = true;
                    //Check the prefix
                    if(filePrefixAndName.Length == 2)
                    {
                        if (filePrefixAndName[0] != "CONTENTS" && filePrefixAndName[0] != "GRAPHICS" && filePrefixAndName[0] != "SOUNDS" && filePrefixAndName[0] != "FIXES" && filePrefixAndName[0] != "GAMEPLAY" &&
                            filePrefixAndName[0] != "SLIDERS" && filePrefixAndName[0] != "OTHERS")
                            deleteThis = true;
                    }
                    //If don't have a prefix
                    if (filePrefixAndName.Length != 2)
                        deleteThis = true;

                    //If is not desired to delete this, continues
                    if (deleteThis == false)
                        continue;

                    //Delete it
                    File.Delete(filePath);
                    //Warn
                    ShowToast(GetStringApplicationResource("launcher_mods_deleteUnrecognized").Replace("%file%", (filePath.Replace((fixedMyDocumentsPath + "\\"), ""))), ToastType.Error);
                }

                //Delete unrecognized folders
                foreach (string folderPath in foldersList)
                {
                    //Prepare information
                    bool deleteThis = false;

                    //Delete any folder
                    deleteThis = true;

                    //If is not desired to delete this, continues
                    if (deleteThis == false)
                        continue;

                    //Delete it
                    Directory.Delete(folderPath, true);
                    //Warn
                    ShowToast(GetStringApplicationResource("launcher_mods_deleteUnrecognized").Replace("%file%", (folderPath.Replace((fixedMyDocumentsPath + "\\"), ""))), ToastType.Error);
                }
            }

            //-------------------------------------------------------------------------------------------------//
            //------------------------------ Check Mods/Packages/DL3-Recommended ------------------------------//
            //-------------------------------------------------------------------------------------------------//
            if (true == true)
            {
                //Query all files
                List<string> filesList = new List<string>();
                foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/Mods/Packages/DL3-Recommended")).GetFiles()))
                    filesList.Add(file.FullName);
                //Query all directories
                List<string> foldersList = new List<string>();
                foreach (DirectoryInfo dir in (new DirectoryInfo((myDocumentsPath + "/Mods/Packages/DL3-Recommended")).GetDirectories()))
                    foldersList.Add(dir.FullName);

                //Delete unrecognized files
                foreach (string filePath in filesList)
                {
                    //Prepare information
                    bool deleteThis = false;

                    //Get the file name and extension
                    string fileExtension = System.IO.Path.GetExtension(filePath).Replace(".", "").ToLower();
                    string[] filePrefixAndName = System.IO.Path.GetFileNameWithoutExtension(filePath).Split(" --- ");

                    //If is not a valid file extension
                    if (fileExtension != "package" && fileExtension != "disabled")
                        deleteThis = true;
                    //Check the prefix
                    if (filePrefixAndName.Length == 2)
                    {
                        if (filePrefixAndName[0] != "CONTENTS" && filePrefixAndName[0] != "GRAPHICS" && filePrefixAndName[0] != "SOUNDS" && filePrefixAndName[0] != "FIXES" && filePrefixAndName[0] != "GAMEPLAY" &&
                            filePrefixAndName[0] != "SLIDERS" && filePrefixAndName[0] != "OTHERS")
                            deleteThis = true;
                    }
                    //If don't have a prefix
                    if (filePrefixAndName.Length != 2)
                        deleteThis = true;

                    //If is not desired to delete this, continues
                    if (deleteThis == false)
                        continue;

                    //Delete it
                    File.Delete(filePath);
                    //Warn
                    ShowToast(GetStringApplicationResource("launcher_mods_deleteUnrecognized").Replace("%file%", (filePath.Replace((fixedMyDocumentsPath + "\\"), ""))), ToastType.Error);
                }

                //Delete unrecognized folders
                foreach (string folderPath in foldersList)
                {
                    //Prepare information
                    bool deleteThis = false;

                    //Delete any folder
                    deleteThis = true;

                    //If is not desired to delete this, continues
                    if (deleteThis == false)
                        continue;

                    //Delete it
                    Directory.Delete(folderPath, true);
                    //Warn
                    ShowToast(GetStringApplicationResource("launcher_mods_deleteUnrecognized").Replace("%file%", (folderPath.Replace((fixedMyDocumentsPath + "\\"), ""))), ToastType.Error);
                }
            }
        }
    
        private void SetModInstallScreen(ModScreen screen)
        {
            //Prepare the data
            Color btSelectedColor = Color.FromArgb(255, 44, 103, 169);
            Color btUnselectedColor = Color.FromArgb(255, 168, 171, 175);

            //If is desired to see recommended mods
            if (screen == ModScreen.Recommended)
            {
                //Change button colors
                installRecommended.Background = new SolidColorBrush(btSelectedColor);
                installCustom.Background = new SolidColorBrush(btUnselectedColor);
                //Show the correct screen
                modsRecommended.Visibility = Visibility.Visible;
                modsCustom.Visibility = Visibility.Collapsed;
            }

            //If is desired to see custom mods
            if (screen == ModScreen.Custom)
            {
                //Change button colors
                installRecommended.Background = new SolidColorBrush(btUnselectedColor);
                installCustom.Background = new SolidColorBrush(btSelectedColor);
                //Show the correct screen
                modsRecommended.Visibility = Visibility.Collapsed;
                modsCustom.Visibility = Visibility.Visible;
            }
        }

        public void UpdateRecommendedModsList()
        {
            //If is already downloading the recommended library, cancel
            if (isDownloadingRecommendedLibrary == true)
                return;

            //If the recommended library is more old than 5 minutes, auto clear it
            if(File.Exists((myDocumentsPath + "/!DL-TmpCache/recommended-mods-library.json")) == true)
            {
                //Calculate the minutes old of recommended library
                long minutesOld = (new TimeSpan(DateTime.Now.Ticks) - new TimeSpan(File.GetLastWriteTime((myDocumentsPath + "/!DL-TmpCache/recommended-mods-library.json")).Ticks)).Minutes;

                //If library is more old than 5 minutes, delete it
                if (minutesOld >= 5)
                    File.Delete((myDocumentsPath + "/!DL-TmpCache/recommended-mods-library.json"));
            }

            //If the library file don't exists in cache, download it
            if(File.Exists((myDocumentsPath + "/!DL-TmpCache/recommended-mods-library.json")) == false)
            {
                //Start a thread to download the mod library
                AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { });
                asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) =>
                {
                    //Show loading screen
                    recModsLoading.Visibility = Visibility.Visible;
                    recModsLoadError.Visibility = Visibility.Collapsed;
                    //Inform that is downloading the library
                    isDownloadingRecommendedLibrary = true;
                    //Add the task to queue
                    AddTask("loadingRecommendedModsLibrary", "Downloading Recommended Mods library for installation.");
                };
                asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(1000);

                    //Try to do the task
                    try
                    {
                        //Prepare the target download URL
                        string downloadUrl = @"https://marcos4503.github.io/ts3-dream-launcher/Repository-Pages/recommended-mods-library.json";
                        string saveAsPath = (myDocumentsPath + @"/!DL-TmpCache/recommended-mods-library.json");
                        //Download the "Mods" folder sync
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

                    //Hide loading screen
                    recModsLoading.Visibility = Visibility.Collapsed;
                    //Inform that is not downloading the library
                    isDownloadingRecommendedLibrary = false;
                    //Remove the task from queue
                    RemoveTask("loadingRecommendedModsLibrary");

                    //If have a response different from success, inform error
                    if (threadTaskResponse != "success")
                        recModsLoadError.Visibility = Visibility.Visible;

                    //If have a response of success, recall this method to start rendering recommended mods
                    if (threadTaskResponse == "success")
                        UpdateRecommendedModsList();
                };
                asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);

                //Cancel the current operation
                return;
            }

            //If the routine is already running, stop it
            if (recommendedModsUpdateRoutine != null)
            {
                recommendedModsUpdateRoutine.Dispose();
                recommendedModsUpdateRoutine = null;
            }

            //Start the recommended mods update routine
            recommendedModsUpdateRoutine = Coroutine.Start(RenderRecommendedModsRoutine());
        }

        private IEnumerator RenderRecommendedModsRoutine()
        {
            //Show the loading indicator
            recModsLoading.Visibility = Visibility.Visible;
            //Wait end of animation
            yield return new WaitForSeconds(0.2);

            //Clear all mods previously rendered
            foreach (StoreModItem item in instantiatedRecModItems)
                recModsList.Children.Remove(item);
            instantiatedRecModItems.Clear();

            //Read the recommended mods list file
            RecommendedListReader library = new RecommendedListReader((myDocumentsPath + @"/!DL-TmpCache/recommended-mods-library.json"));

            //Render all mods...
            foreach (Mod modItem in library.loadedData.modsList)
            {
                //Create the item to display
                StoreModItem newItem = new StoreModItem(this);
                recModsList.Children.Add(newItem);
                instantiatedRecModItems.Add(newItem);

                //Configure it
                newItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newItem.VerticalAlignment = VerticalAlignment.Top;
                newItem.Width = double.NaN;
                newItem.Height = double.NaN;
                newItem.Margin = new Thickness(0, 0, 0, 4);

                //Inform the data about the mod
                newItem.SetGameInstallPath((new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent).FullName);
                newItem.SetRecommendedModsPath((myDocumentsPath + @"/Mods/Packages/DL3-Recommended"));
                newItem.SetPatchModsPath((myDocumentsPath + @"/Mods/Packages/DL3-Patches"));
                newItem.SetTitle(modItem.name);
                newItem.SetModCategory(modItem.category);
                newItem.SetAuthor(modItem.author);
                newItem.SetDescription(modItem.description_enUS, modItem.description_ptBR);
                newItem.SetRequiredEPs(modItem.requiredEps);
                newItem.SetRequiredRecommendedModsFiles(modItem.requiredRecommendedModFiles);
                newItem.SetRequiredPatchModsFiles(modItem.requiredPatchModFiles);
                newItem.SetModPageURL(modItem.pageUrl);
                newItem.SetModDownloadURL(modItem.downloadUrl);
                newItem.Prepare();

                //Wait before render next to avoid UI freezing
                yield return new WaitForSeconds(0.01f);
            }

            //Auto change to category of all
            SetRecommendedModsCategoryFilter(currentSeeingRecModsCategory);

            //Wait end of animation
            yield return new WaitForSeconds(0.2);
            //Hide the loading indicator
            recModsLoading.Visibility = Visibility.Collapsed;

            //Auto clear routine reference
            recommendedModsUpdateRoutine = null;
        }

        private void SetRecommendedModsCategoryFilter(ModCategory category)
        {
            //Prepare the data
            Color btSelectedColor = Color.FromArgb(255, 44, 103, 169);
            Color btUnselectedColor = Color.FromArgb(255, 168, 171, 175);

            //Prepare the dictionary of categories
            int categoryIdToShow = -1;
            switch (category)
            {
                case ModCategory.All:
                    categoryIdToShow = 0;
                    break;
                case ModCategory.Contents:
                    categoryIdToShow = 1;
                    break;
                case ModCategory.Graphics:
                    categoryIdToShow = 2;
                    break;
                case ModCategory.Sounds:
                    categoryIdToShow = 3;
                    break;
                case ModCategory.Fixes:
                    categoryIdToShow = 4;
                    break;
                case ModCategory.Gameplay:
                    categoryIdToShow = 5;
                    break;
                case ModCategory.Sliders:
                    categoryIdToShow = 6;
                    break;
                case ModCategory.Others:
                    categoryIdToShow = 7;
                    break;
            }

            //Prepare the list of buttons
            List<Controls.BeautyButton.BeautyButton> buttons = new List<Controls.BeautyButton.BeautyButton>();
            buttons.Add(recModsAll);
            buttons.Add(recModsContents);
            buttons.Add(recModsGraphics);
            buttons.Add(recModsSounds);
            buttons.Add(recModsFixes);
            buttons.Add(recModsGameplay);
            buttons.Add(recModsSliders);
            buttons.Add(recModsOthers);

            //If was found a ID
            if (categoryIdToShow != -1)
            {
                //Unhighlight all buttons
                for (int i = 0; i < buttons.Count; i++)
                    buttons[i].Background = new SolidColorBrush(btUnselectedColor);

                //Highlight the desired button
                buttons[categoryIdToShow].Background = new SolidColorBrush(btSelectedColor);
            }

            //Clear the lists
            buttons.Clear();

            //Inform the current viewing category
            currentSeeingRecModsCategory = category;

            //Auto apply the filter
            ApplyRecommendedCategoryFilters();
        }

        private void ApplyRecommendedCategoryFilters()
        {
            //If the routine is already running, stop it
            if (recommendedModsFilterRoutine != null)
            {
                recommendedModsFilterRoutine.Dispose();
                recommendedModsFilterRoutine = null;
            }

            //Start the recommended mods filter routine
            recommendedModsFilterRoutine = Coroutine.Start(ApplyFilterOnRecommendedModsRoutine());
        }

        private IEnumerator ApplyFilterOnRecommendedModsRoutine()
        {
            //Show the loading indicator
            recModsFiltering.Visibility = Visibility.Visible;
            //Wait some time before apply filter
            yield return new WaitForSeconds(0.15f);

            //Hide all items
            foreach (StoreModItem item in instantiatedRecModItems)
            {
                //Hide
                item.Visibility = Visibility.Collapsed;

                //Wait before render next to avoid UI freezing
                yield return new WaitForSeconds(0.0025f);
            }

            //Enable only that corresponds with the filter...

            //Enable the items that corresponds with category
            foreach (StoreModItem item in instantiatedRecModItems)
            {
                //If is not seeing all, enable only mods that corresponds with desired category
                if (currentSeeingRecModsCategory != ModCategory.All)
                {
                    if (currentSeeingRecModsCategory == ModCategory.Contents && item.modCategory == StoreModItem.ModCategory.Contents)
                        item.Visibility = Visibility.Visible;
                    if (currentSeeingRecModsCategory == ModCategory.Graphics && item.modCategory == StoreModItem.ModCategory.Graphics)
                        item.Visibility = Visibility.Visible;
                    if (currentSeeingRecModsCategory == ModCategory.Sounds && item.modCategory == StoreModItem.ModCategory.Sounds)
                        item.Visibility = Visibility.Visible;
                    if (currentSeeingRecModsCategory == ModCategory.Fixes && item.modCategory == StoreModItem.ModCategory.Fixes)
                        item.Visibility = Visibility.Visible;
                    if (currentSeeingRecModsCategory == ModCategory.Gameplay && item.modCategory == StoreModItem.ModCategory.Gameplay)
                        item.Visibility = Visibility.Visible;
                    if (currentSeeingRecModsCategory == ModCategory.Sliders && item.modCategory == StoreModItem.ModCategory.Sliders)
                        item.Visibility = Visibility.Visible;
                    if (currentSeeingRecModsCategory == ModCategory.Others && item.modCategory == StoreModItem.ModCategory.Others)
                        item.Visibility = Visibility.Visible;
                }
                //If is seeing all, enable it
                if (currentSeeingRecModsCategory == ModCategory.All)
                    item.Visibility = Visibility.Visible;

                //Wait before render next to avoid UI freezing
                yield return new WaitForSeconds(0.0025f);
            }

            //Hide the loading indicator
            recModsFiltering.Visibility = Visibility.Collapsed;

            //Auto clear routine reference
            recommendedModsFilterRoutine = null;
        }
    
        private void OpenCustomPackageModPicker()
        {
            //If don't have S3CLI Extractor, cancel
            if (File.Exists((Directory.GetCurrentDirectory() + @"/Content/tool-s3ce/s3ce.exe")) == false)
            {
                ShowToast(GetStringApplicationResource("launcher_mods_addNewTab_customAddError"), MainWindow.ToastType.Error);
                return;
            }

            //Open the file picker 
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "TS3 Mod File|*.package;*.sims3pack";
            bool? result = fileDialog.ShowDialog();

            //If don't have picker file, cancel
            if (result == false || fileDialog.FileName == "")
                return;

            //Get the extension of the mod to be installed
            string modExtension = System.IO.Path.GetExtension(fileDialog.FileName).ToLower().Replace(".", "");

            //Get the target category prefix
            string categoryPrefix = "";
            if (instCustomCategory.SelectedIndex == 0)
                categoryPrefix = "CONTENTS";
            if (instCustomCategory.SelectedIndex == 1)
                categoryPrefix = "GRAPHICS";
            if (instCustomCategory.SelectedIndex == 2)
                categoryPrefix = "SOUNDS";
            if (instCustomCategory.SelectedIndex == 3)
                categoryPrefix = "FIXES";
            if (instCustomCategory.SelectedIndex == 4)
                categoryPrefix = "GAMEPLAY";
            if (instCustomCategory.SelectedIndex == 5)
                categoryPrefix = "SLIDERS";
            if (instCustomCategory.SelectedIndex == 6)
                categoryPrefix = "OTHERS";

            //Start the mod installer or pre mod installer, according to the extension of selected mod file...
            if(modExtension == "package")     //<--- If is a "package" mod
            {
                //Open the window of mod installer
                WindowModInstaller modInstaller = new WindowModInstaller(this, WindowModInstaller.InstallType.Custom, fileDialog.FileName, (myDocumentsPath + @"/Mods/Packages/DL3-Custom"),
                                                                        (categoryPrefix + " --- " + System.IO.Path.GetFileNameWithoutExtension(fileDialog.FileName) + ".package"));
                modInstaller.Closed += (s, e) =>
                {
                    SetInteractionBlockerEnabled(false);
                    RemoveTask("modInstalling");
                };
                modInstaller.Show();
            }
            if(modExtension == "sims3pack")   //<--- If is a "sims3pack" mod
            {
                //Open the window of sims3pack pre mod installer
                WindowS3PkgPreModInstaller modInstaller = new WindowS3PkgPreModInstaller(this, fileDialog.FileName, (myDocumentsPath + @"/Mods/Packages/DL3-Custom"), categoryPrefix);
                modInstaller.Closed += (s, e) =>
                {
                    SetInteractionBlockerEnabled(false);
                    RemoveTask("modInstalling");
                };
                modInstaller.Show();
            }

            //Block the UI
            SetInteractionBlockerEnabled(true);
            //Add the task to queue
            AddTask("modInstalling", "Running Mod installer.");
        }
    
        //Media manager

        private void BuildAndPrepareMediaListSystem()
        {
            //Disable the buttons
            SetEnabledMediaActionButtons(false);

            //Prepare the buttons
            mediaCopy.Click += (s, e) => { CopySelectedMedia(); };
            mediaCut.Click += (s, e) => { CutSelectedMedia(); };
            mediaDelete.Click += (s, e) => { DeleteSelectedMedia(); };

            //Update the media list
            UpdateMediaList();
        }

        private void SetEnabledMediaActionButtons(bool enable)
        {
            //If is desired to disable
            if(enable == false)
            {
                mediaCut.Opacity = 0.25f;
                mediaCut.IsHitTestVisible = false;
                mediaCopy.Opacity = 0.25f;
                mediaCopy.IsHitTestVisible = false;
                mediaDelete.Opacity = 0.25f;
                mediaDelete.IsHitTestVisible = false;
            }

            //If is desired to enable
            if (enable == true)
            {
                mediaCut.Opacity = 0.85f;
                mediaCut.IsHitTestVisible = true;
                mediaCopy.Opacity = 0.85f;
                mediaCopy.IsHitTestVisible = true;
                mediaDelete.Opacity = 0.85f;
                mediaDelete.IsHitTestVisible = true;
            }
        }

        private void CutSelectedMedia()
        {
            //Prepare the file list
            List<string> fileList = new List<string>();

            //Mount file list
            foreach (MediaItem item in instantiatedMediaItems)
                if (item.isSelected == true)
                    fileList.Add(item.filePath);

            //Prepare the folder selected path
            string selectedFolderPath = "";
            //Open folder picker dialog
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    selectedFolderPath = dialog.SelectedPath;
            }
            //If no folder was selected, cancel
            if (selectedFolderPath == "")
                return;

            //Move all files to selected place
            foreach (string filePath in fileList)
                File.Move(filePath, (selectedFolderPath + "/" + System.IO.Path.GetFileName(filePath)));

            //Warn about the cut
            MessageBox.Show(GetStringApplicationResource("launcher_media_cutDoneText"),
                            GetStringApplicationResource("launcher_media_cutDoneTitle"), MessageBoxButton.OK, MessageBoxImage.Information);

            //Update media list
            UpdateMediaList();
        }

        private void CopySelectedMedia()
        {
            //Prepare the file list
            List<string> fileList = new List<string>();

            //Mount file list
            foreach (MediaItem item in instantiatedMediaItems)
                if (item.isSelected == true)
                    fileList.Add(item.filePath);

            //Prepare the folder selected path
            string selectedFolderPath = "";
            //Open folder picker dialog
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    selectedFolderPath = dialog.SelectedPath;
            }
            //If no folder was selected, cancel
            if (selectedFolderPath == "")
                return;

            //Copy all files to selected place
            foreach (string filePath in fileList)
                File.Copy(filePath, (selectedFolderPath + "/" + System.IO.Path.GetFileName(filePath)));

            //Warn about the copy
            MessageBox.Show(GetStringApplicationResource("launcher_media_copyDoneText"),
                            GetStringApplicationResource("launcher_media_copyDoneTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteSelectedMedia()
        {
            //Prepare the file list
            List<string> fileList = new List<string>();

            //Mount file list
            foreach (MediaItem item in instantiatedMediaItems)
                if (item.isSelected == true)
                    fileList.Add(item.filePath);

            //Show the confirmation dialog
            MessageBoxResult dialogResult = MessageBox.Show(GetStringApplicationResource("launcher_media_deleteConfirmText"),
                                                            GetStringApplicationResource("launcher_media_deleteConfirmTitle"), MessageBoxButton.YesNo, MessageBoxImage.Warning);

            //If is desired to delete, do it
            if (dialogResult == MessageBoxResult.Yes)
            {
                //Delete each file
                foreach (string filePath in fileList)
                    File.Delete(filePath);

                //Update media list
                UpdateMediaList();
            }
        }

        private void ProcessMediaSelection()
        {
            //Prepare the medias selected counter
            int mediasSelectedCount = 0;

            //Count medias selected
            foreach (MediaItem item in instantiatedMediaItems)
                if (item.isSelected == true)
                    mediasSelectedCount += 1;

            //Enable or disable the action buttons
            if (mediasSelectedCount == 0)
                SetEnabledMediaActionButtons(false);
            if (mediasSelectedCount >= 1)
                SetEnabledMediaActionButtons(true);

            //Show the selection counter
            if (mediasSelectedCount == 0)
                mediaCount.Content = GetStringApplicationResource("launcher_media_statusCount").Replace("%n%", instantiatedMediaItems.Count.ToString());
            if (mediasSelectedCount >= 1)
                mediaCount.Content = GetStringApplicationResource("launcher_media_statusCountWithSelecion").Replace("%n%", instantiatedMediaItems.Count.ToString()).Replace("%s%", mediasSelectedCount.ToString());
        }

        private void UpdateMediaList()
        {
            //If the routine is already running, stop it
            if (mediaListUpdateRoutine != null)
            {
                mediaListUpdateRoutine.Dispose();
                mediaListUpdateRoutine = null;
            }

            //Start the media update routine
            mediaListUpdateRoutine = Coroutine.Start(UpdateMediaListRoutine());
        }

        private IEnumerator UpdateMediaListRoutine()
        {
            //Add task to list
            AddTask("mediaListUpdate", "Updating the Media list.");

            //Show the loading indicator
            mediaLoad.Visibility = Visibility.Visible;
            mediaContent.Visibility = Visibility.Collapsed;
            mediaCount.Content = GetStringApplicationResource("launcher_media_statusLoad");
            mediasSize.Content = "-";

            //Disable the action buttons
            SetEnabledMediaActionButtons(false);

            //Wait some time
            yield return new WaitForSeconds(1.0f);

            //Clear all medias previously rendered
            foreach (MediaItem item in instantiatedMediaItems)
                mediaList.Children.Remove(item);
            instantiatedMediaItems.Clear();

            //Prepare the mods counters
            int screenshotsCounter = 0;
            int videosCounter = 0;

            //Prepare the list of media files
            List<string> allMediasList = new List<string>();

            //Fill the list of media files
            foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/Screenshots")).GetFiles()))
                if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".jpg")
                {
                    allMediasList.Add(file.FullName);
                    screenshotsCounter += 1;
                }
            foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/Recorded Videos")).GetFiles()))
                if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".avi")
                {
                    allMediasList.Add(file.FullName);
                    videosCounter += 1;
                }

            //Render all media
            foreach(string mediaFilePath in allMediasList)
            {
                //Draw the item on screen
                MediaItem newItem = new MediaItem(this, mediaFilePath);
                mediaList.Children.Add(newItem);
                instantiatedMediaItems.Add(newItem);

                //Configure it
                newItem.HorizontalAlignment = HorizontalAlignment.Left;
                newItem.VerticalAlignment = VerticalAlignment.Top;
                newItem.Width = double.NaN;
                newItem.Height = double.NaN;
                newItem.Margin = new Thickness(4, 4, 4, 4);

                //Inform the data about the media
                newItem.RegisterOnClickCallback(() => { ProcessMediaSelection(); });
                newItem.Prepare();

                //Wait before render next to avoid UI freezing
                yield return new WaitForSeconds(0.02f);
            }

            //Count total media files size
            long totalSizeInBytes = 0;
            foreach (string mediaPath in allMediasList)
                if (File.Exists(mediaPath) == true)
                    totalSizeInBytes += (new FileInfo(mediaPath)).Length;

            //Enable or hide the empty media warning
            if (instantiatedMediaItems.Count == 0)
                mediaEmptyWarn.Visibility = Visibility.Visible;
            if (instantiatedMediaItems.Count >= 1)
                mediaEmptyWarn.Visibility = Visibility.Collapsed;

            //Wait some time
            yield return new WaitForSeconds(1.0f);

            //Show new media notification, if necessary
            int currentMediasCount = (screenshotsCounter + videosCounter);
            int lastMediasCount = launcherPrefs.loadedData.lastMediaFilesCount;
            //If the current medias count is not zero, check if was increased
            if (currentMediasCount > 0)
                if (currentMediasCount > lastMediasCount)
                    EnablePageRedDotNotification(LauncherPage.media);
            //Save the medias count
            launcherPrefs.loadedData.lastMediaFilesCount = currentMediasCount;
            launcherPrefs.Save();

            //Remove the task
            RemoveTask("mediaListUpdate");

            //Show the content
            mediaLoad.Visibility = Visibility.Collapsed;
            mediaContent.Visibility = Visibility.Visible;
            mediaCount.Content = GetStringApplicationResource("launcher_media_statusCount").Replace("%n%", (screenshotsCounter + videosCounter).ToString());
            mediasSize.Content = GetFormattedCacheSize(totalSizeInBytes).Replace("~", "");

            //Auto clear routine reference
            mediaListUpdateRoutine = null;
        }
    
        //Worlds manager

        private void BuildAndPrepareWorldsSystem()
        {
            //Create the "custom-worlds" folder if not exists
            if (Directory.Exists((myDocumentsPath + "/!DL-Static/custom-worlds")) == false)
                Directory.CreateDirectory((myDocumentsPath + "/!DL-Static/custom-worlds"));

            //Prepare the more options button
            worldsMore.ContextMenu = new ContextMenu();
            //Setup the context menu display
            worldsMore.Click += (s, e) =>
            {
                ContextMenu contextMenu = worldsMore.ContextMenu;
                contextMenu.PlacementTarget = worldsMore;
                contextMenu.IsOpen = true;
                e.Handled = true;
            };

            //Add "help" option to options menu
            MenuItem helpItem = new MenuItem();
            helpItem.Header = GetStringApplicationResource("launcher_world_optionsHelp");
            helpItem.Click += (s, e) => { OpenWorldsHelpWindow(); };
            worldsMore.ContextMenu.Items.Add(helpItem);

            //Prepare the add world button
            addNewWorld.Click += (s, e) => { OpenCustomWorldPicker(); };

            //Update the worlds list
            UpdateWorldList();
        }

        private void OpenWorldsHelpWindow()
        {
            //Block interactions
            SetInteractionBlockerEnabled(true);

            //Open help window
            WindowWorldHelp worldsHelp = new WindowWorldHelp(this);
            worldsHelp.Closed += (s, e) =>
            {
                SetInteractionBlockerEnabled(false);
            };
            worldsHelp.Show();
        }

        public void UpdateWorldList()
        {
            //If the routine is already running, stop it
            if (worldListUpdateRoutine != null)
            {
                worldListUpdateRoutine.Dispose();
                worldListUpdateRoutine = null;
            }

            //Start the worlds update routine
            worldListUpdateRoutine = Coroutine.Start(UpdateWorldListRoutine());
        }

        private IEnumerator UpdateWorldListRoutine()
        {
            //Add task to list
            AddTask("worldListUpdate", "Updating and checking the World list.");

            //Show the loading indicator
            worldLoad.Visibility = Visibility.Visible;
            worldContent.Visibility = Visibility.Collapsed;
            addNewWorld.IsEnabled = false;

            //Wait some time
            yield return new WaitForSeconds(1.0f);

            //Clear all worlds previously rendered
            foreach (WorldItem item in instantiatedWorldItems)
                worldList.Children.Remove(item);
            instantiatedWorldItems.Clear();

            //Prepare the list of world files
            List<string> allWorldsList = new List<string>();

            //Fill the list of world files
            foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/!DL-Static/custom-worlds")).GetFiles()))
                if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".json")
                    allWorldsList.Add(file.FullName);

            //Render all worlds
            foreach (string worldFilePath in allWorldsList)
            {
                //Draw the item on screen
                WorldItem newItem = new WorldItem(this, worldFilePath);
                worldList.Children.Add(newItem);
                instantiatedWorldItems.Add(newItem);

                //Configure it
                newItem.HorizontalAlignment = HorizontalAlignment.Left;
                newItem.VerticalAlignment = VerticalAlignment.Top;
                newItem.Width = double.NaN;
                newItem.Height = double.NaN;
                newItem.Margin = new Thickness(4, 4, 4, 4);

                //Inform the data about the media
                newItem.Prepare();

                //Wait before render next to avoid UI freezing
                yield return new WaitForSeconds(0.02f);
            }

            //Enable or hide the empty world warning
            if (instantiatedWorldItems.Count == 0)
                worldEmptyWarn.Visibility = Visibility.Visible;
            if (instantiatedWorldItems.Count >= 1)
                worldEmptyWarn.Visibility = Visibility.Collapsed;

            //Prepare the check variable
            bool foundProblems = false;
            //Check each installed world to know if have problems
            foreach (WorldItem item in instantiatedWorldItems)
                if (item.foundProblem == true)
                    foundProblems = true;
            //If found problem, notify
            if(foundProblems == true)
            {
                //Send notification
                ShowToast(GetStringApplicationResource("launcher_world_problemFound"), ToastType.Error);

                //Enable the dot notification
                EnablePageRedDotNotification(LauncherPage.worlds);
            }

            //Wait some time
            yield return new WaitForSeconds(1.0f);

            //Remove the task
            RemoveTask("worldListUpdate");

            //Show the content
            worldLoad.Visibility = Visibility.Collapsed;
            worldContent.Visibility = Visibility.Visible;
            addNewWorld.IsEnabled = true;

            //Auto clear routine reference
            mediaListUpdateRoutine = null;
        }

        private void OpenCustomWorldPicker()
        {
            //If the patch of "Mods Support" is not installed, cancel
            if (launcherPrefs.loadedData.patchModsSupport == false)
            {
                ShowToast(GetStringApplicationResource("launcher_world_customAddErrorPatch"), ToastType.Error);
                return;
            }
            //If don't have S3CLI Extractor, cancel
            if (File.Exists((Directory.GetCurrentDirectory() + @"/Content/tool-s3ce/s3ce.exe")) == false)
            {
                ShowToast(GetStringApplicationResource("launcher_world_customAddError"), MainWindow.ToastType.Error);
                return;
            }

            //Open the file picker 
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "TS3 World File|*.sims3pack";
            bool? result = fileDialog.ShowDialog();

            //If don't have picker file, cancel
            if (result == false || fileDialog.FileName == "")
                return;

            //Block the UI
            SetInteractionBlockerEnabled(true);
            //Add the task to queue
            AddTask("worldInstalling", "Running World installer.");

            //Open the window of world installer
            WindowWorldInstaller worldInstaller = new WindowWorldInstaller(this, (Directory.GetCurrentDirectory() + @"/Content"), fileDialog.FileName);
            worldInstaller.Closed += (s, e) =>
            {
                SetInteractionBlockerEnabled(false);
                RemoveTask("worldInstalling");
            };
            worldInstaller.Show();
        }
    
        //Exports manager

        private void BuildAndPrepareExportsSystem()
        {
            //Disable the buttons
            SetEnabledExportActionButtons(false);

            //Prepare the buttons
            exportImport.Click += (s, e) => { ImportExport(); };
            exportCopy.Click += (s, e) => { CopySelectedExport(); };
            exportDelete.Click += (s, e) => { DeleteSelectedExport(); };

            //Update the exports list
            UpdateExportList();
        }

        private void SetEnabledExportActionButtons(bool enable)
        {
            //If is desired to disable
            if (enable == false)
            {
                exportCopy.Opacity = 0.25f;
                exportCopy.IsHitTestVisible = false;
                exportDelete.Opacity = 0.25f;
                exportDelete.IsHitTestVisible = false;
            }

            //If is desired to enable
            if (enable == true)
            {
                exportCopy.Opacity = 0.85f;
                exportCopy.IsHitTestVisible = true;
                exportDelete.Opacity = 0.85f;
                exportDelete.IsHitTestVisible = true;
            }
        }
   
        private void ImportExport()
        {
            //Open the file picker 
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "TS3 File|*.package;*.sims3pack";
            bool? result = fileDialog.ShowDialog();

            //If don't have picked file, cancel
            if (result == false || fileDialog.FileName == "")
                return;

            //Calculate the target file path
            string targetFinalPath = (myDocumentsPath + "/Exports/" + System.IO.Path.GetFileName(fileDialog.FileName));

            //Check if the file already exists in destination
            if (File.Exists(targetFinalPath) == true)
            {
                //Display a dialog and cancel
                MessageBox.Show(GetStringApplicationResource("launcher_export_importErrorText"),
                                GetStringApplicationResource("launcher_export_importErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Copy the selected path to the exports
            File.Copy(fileDialog.FileName, targetFinalPath);

            //Force update the exports list
            UpdateExportList();
        }

        private void CopySelectedExport()
        {
            //Prepare the file list
            List<string> fileList = new List<string>();

            //Mount file list
            foreach (ExportItem item in instantiatedExportItems)
                if (item.isSelected == true)
                    fileList.Add(item.filePath);

            //Prepare the folder selected path
            string selectedFolderPath = "";
            //Open folder picker dialog
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    selectedFolderPath = dialog.SelectedPath;
            }
            //If no folder was selected, cancel
            if (selectedFolderPath == "")
                return;

            //Copy all files to selected place
            foreach (string filePath in fileList)
                File.Copy(filePath, (selectedFolderPath + "/" + System.IO.Path.GetFileName(filePath)));

            //Warn about the copy
            MessageBox.Show(GetStringApplicationResource("launcher_export_copyDoneText"),
                            GetStringApplicationResource("launcher_export_copyDoneTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteSelectedExport()
        {
            //Prepare the file list
            List<string> fileList = new List<string>();

            //Mount file list
            foreach (ExportItem item in instantiatedExportItems)
                if (item.isSelected == true)
                    fileList.Add(item.filePath);

            //Show the confirmation dialog
            MessageBoxResult dialogResult = MessageBox.Show(GetStringApplicationResource("launcher_export_deleteConfirmText"),
                                                            GetStringApplicationResource("launcher_export_deleteConfirmTitle"), MessageBoxButton.YesNo, MessageBoxImage.Warning);

            //If is desired to delete, do it
            if (dialogResult == MessageBoxResult.Yes)
            {
                //Delete each file
                foreach (string filePath in fileList)
                    File.Delete(filePath);

                //Update exports list
                UpdateExportList();
            }
        }

        private void ProcessExportSelection()
        {
            //Prepare the exports selected counter
            int exportsSelectedCount = 0;

            //Count exports selected
            foreach (ExportItem item in instantiatedExportItems)
                if (item.isSelected == true)
                    exportsSelectedCount += 1;

            //Enable or disable the action buttons
            if (exportsSelectedCount == 0)
                SetEnabledExportActionButtons(false);
            if (exportsSelectedCount >= 1)
                SetEnabledExportActionButtons(true);

            //Show the selection counter
            if (exportsSelectedCount == 0)
                exportCount.Content = GetStringApplicationResource("launcher_export_statusCount").Replace("%n%", instantiatedExportItems.Count.ToString());
            if (exportsSelectedCount >= 1)
                exportCount.Content = GetStringApplicationResource("launcher_export_statusCountWithSelecion").Replace("%n%", instantiatedExportItems.Count.ToString()).Replace("%s%", exportsSelectedCount.ToString());
        }
    
        private void UpdateExportList()
        {
            //If the routine is already running, stop it
            if (exportListUpdateRoutine != null)
            {
                exportListUpdateRoutine.Dispose();
                exportListUpdateRoutine = null;
            }

            //Start the export update routine
            exportListUpdateRoutine = Coroutine.Start(UpdateExportListRoutine());
        }

        private IEnumerator UpdateExportListRoutine()
        {
            //Add task to list
            AddTask("exportListUpdate", "Updating the Export list.");

            //Disable the import button
            exportImport.IsEnabled = false;
            //Show the loading indicator
            exportLoad.Visibility = Visibility.Visible;
            exportContent.Visibility = Visibility.Collapsed;
            exportCount.Content = GetStringApplicationResource("launcher_export_statusLoading");
            exportSize.Content = "-";

            //Disable the action buttons
            SetEnabledExportActionButtons(false);

            //Wait some time
            yield return new WaitForSeconds(1.0f);

            //Clear all exports previously rendered
            foreach (ExportItem item in instantiatedExportItems)
                exportList.Children.Remove(item);
            instantiatedExportItems.Clear();

            //Prepare the list of export files
            List<string> allExportsList = new List<string>();

            //Fill the list of exports files
            foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/Exports")).GetFiles()))
                if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".package")
                    allExportsList.Add(file.FullName);
            foreach (FileInfo file in (new DirectoryInfo((myDocumentsPath + "/Exports")).GetFiles()))
                if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".sims3pack")
                    allExportsList.Add(file.FullName);

            //Render all exports
            foreach (string exportFilePath in allExportsList)
            {
                //Draw the item on screen
                ExportItem newItem = new ExportItem(this, exportFilePath);
                exportList.Children.Add(newItem);
                instantiatedExportItems.Add(newItem);

                //Configure it
                newItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newItem.VerticalAlignment = VerticalAlignment.Top;
                newItem.Width = double.NaN;
                newItem.Height = double.NaN;
                newItem.Margin = new Thickness(0, 0, 0, 8);

                //Inform the data about the export
                newItem.RegisterOnClickCallback(() => { ProcessExportSelection(); });
                newItem.Prepare();

                //Wait before render next to avoid UI freezing
                yield return new WaitForSeconds(0.02f);
            }

            //Count total export files size
            long totalSizeInBytes = 0;
            foreach (string exportPath in allExportsList)
                if (File.Exists(exportPath) == true)
                    totalSizeInBytes += (new FileInfo(exportPath)).Length;

            //Enable or hide the empty export warning
            if (instantiatedExportItems.Count == 0)
                exportEmptyWarn.Visibility = Visibility.Visible;
            if (instantiatedExportItems.Count >= 1)
                exportEmptyWarn.Visibility = Visibility.Collapsed;

            //Wait some time
            yield return new WaitForSeconds(1.0f);

            //Show new export notification, if necessary
            int currentExportCount = allExportsList.Count;
            int lastExportCount = launcherPrefs.loadedData.lastExportFilesCount;
            //If the current exports count is not zero, check if was increased
            if (currentExportCount > 0)
                if (currentExportCount > lastExportCount)
                    EnablePageRedDotNotification(LauncherPage.exports);
            //Save the exports count
            launcherPrefs.loadedData.lastExportFilesCount = currentExportCount;
            launcherPrefs.Save();

            //Remove the task
            RemoveTask("exportListUpdate");

            //Show the content
            exportLoad.Visibility = Visibility.Collapsed;
            exportContent.Visibility = Visibility.Visible;
            exportCount.Content = GetStringApplicationResource("launcher_export_statusCount").Replace("%n%", allExportsList.Count.ToString());
            exportSize.Content = GetFormattedCacheSize(totalSizeInBytes).Replace("~", "");
            //Enable the import button
            exportImport.IsEnabled = true;

            //Auto clear routine reference
            exportListUpdateRoutine = null;
        }
    
        //Saves manager

        private void BuildAndPrepareSaveSystem()
        {
            //Create the "save-vault" folder if not exists
            if (Directory.Exists((myDocumentsPath + "/!DL-Static/save-vault")) == false)
                Directory.CreateDirectory((myDocumentsPath + "/!DL-Static/save-vault"));

            //Prepare the buttons
            importSave.Click += (s, e) => { ImportSaveGame(); };

            //Update the save list
            UpdateSaveList();
        }

        private void ImportSaveGame()
        {
            //Prepare the folder selected path
            string selectedFolderPath = "";
            //Open folder picker dialog
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    selectedFolderPath = dialog.SelectedPath;
            }
            //If no folder was selected, cancel
            if (selectedFolderPath == "")
                return;

            //Prepare the response if found NHD file
            bool foundNhdFile = false;
            //Check if the selected folder have a NHD file
            foreach (FileInfo file in (new DirectoryInfo(selectedFolderPath).GetFiles()))
                if(System.IO.Path.GetExtension(file.FullName).Replace(".", "").ToLower() == "nhd")
                {
                    foundNhdFile = true;
                    break;
                }
            //If not found a NHD file, cancel
            if (foundNhdFile == false)
            {
                MessageBox.Show(GetStringApplicationResource("launcher_save_importError0Text"),
                                GetStringApplicationResource("launcher_save_importError0Title"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            //Get directory info of the folder selected
            DirectoryInfo dirInfo = new DirectoryInfo(selectedFolderPath);
            //Split the directory name
            string[] dirNameParts = dirInfo.Name.Split(".");
            //Check if the name have ".sims3" in the end
            if(dirNameParts.Length <= 1 || dirNameParts[dirNameParts.Length - 1].ToLower() != "sims3")
            {
                MessageBox.Show(GetStringApplicationResource("launcher_save_importError0Text"),
                                GetStringApplicationResource("launcher_save_importError0Title"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            //Check if the folder already exists in the save folder
            if(Directory.Exists((myDocumentsPath + "/Saves/" + dirInfo.Name)) == true)
            {
                MessageBox.Show(GetStringApplicationResource("launcher_save_importError1Text"),
                                GetStringApplicationResource("launcher_save_importError1Title"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            //Check if the folder is in drive C
            if(selectedFolderPath[0].ToString().ToLower() != "c")
            {
                MessageBox.Show(GetStringApplicationResource("launcher_save_importError2Text"),
                                GetStringApplicationResource("launcher_save_importError2Title"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            //Move the selected folder to the destination
            Directory.Move(selectedFolderPath, (myDocumentsPath + "/Saves/" + dirInfo.Name));

            //Force update save list
            UpdateSaveList();
        }

        public void UpdateSaveList()
        {
            //If the routine is already running, stop it
            if (saveListUpdateRoutine != null)
            {
                saveListUpdateRoutine.Dispose();
                saveListUpdateRoutine = null;
            }

            //Start the save update routine
            saveListUpdateRoutine = Coroutine.Start(UpdateSaveListRoutine());
        }

        private IEnumerator UpdateSaveListRoutine()
        {
            //Add task to list
            AddTask("saveListUpdate", "Updating the Save Game list.");

            //Disable the import button
            importSave.Opacity = 0.25f;
            importSave.IsHitTestVisible = false;
            //Show the loading indicator
            saveLoad.Visibility = Visibility.Visible;
            saveContent.Visibility = Visibility.Collapsed;
            saveCount.Content = GetStringApplicationResource("launcher_save_statusLoading");

            //Wait some time
            yield return new WaitForSeconds(1.0f);

            //Clear all saves previously rendered
            foreach (SaveItem item in instantiatedSaveItems)
                saveList.Children.Remove(item);
            instantiatedSaveItems.Clear();

            //Prepare the list of save files
            List<string> allSaveList = new List<string>();

            //Fill the list of saves files
            foreach (DirectoryInfo dir in (new DirectoryInfo((myDocumentsPath + "/Saves")).GetDirectories()))
            {
                //Get the directory name
                string[] dirNameParts = dir.Name.Split(".");

                //If contains ".backup" in the end of name, ignore it
                if (dirNameParts.Length <= 1 || dirNameParts[dirNameParts.Length - 1].ToLower() == "backup")
                    continue;

                //Prepare the response if have NHD file
                bool foundNhdFile = false;
                //Try to found an NHD file
                foreach (FileInfo file in (new DirectoryInfo(dir.FullName).GetFiles()))
                    if (System.IO.Path.GetExtension(file.FullName).Replace(".", "").ToLower() == "nhd")
                    {
                        foundNhdFile = true;
                        break;
                    }
                //If not found the NHD file, ignore it
                if (foundNhdFile == false)
                    continue;

                //Add to list of save files
                allSaveList.Add(dir.FullName);
            }

            //Check each save in list, to check if is a bad save game
            for(int i = 0; i < allSaveList.Count; i++)
            {
                //Get the directory name parts
                string[] dirNameParts = (new DirectoryInfo(allSaveList[i])).Name.Split(".");

                //If this is not a bad save game, ignore it
                if (dirNameParts[dirNameParts.Length - 1].ToLower() != "bad")
                    continue;

                //Prepare the new desired name for this save game folder
                string newFolderName = "";
                for (int x = 0; x < dirNameParts.Length; x++)
                    if (x < (dirNameParts.Length - 1))
                        newFolderName += (((x == 0) ? "" : ".") + dirNameParts[x]);
                //Prepare the new path for this savegame renamed
                string renamedPath = (new DirectoryInfo(allSaveList[i])).Parent.FullName + "/" + newFolderName;

                //Create a bad file to signal that this is a corrupted game
                File.WriteAllText((allSaveList[i] + "/!bad-game!.bad"), "BAD GAME!");

                //Rename the save game folder
                Directory.Move(allSaveList[i], renamedPath);

                //Update the path in the list
                allSaveList[i] = renamedPath;
            }

            //Render all saves
            foreach (string saveFilePath in allSaveList)
            {
                //Draw the item on screen
                SaveItem newItem = new SaveItem(this, saveFilePath);
                saveList.Children.Add(newItem);
                instantiatedSaveItems.Add(newItem);

                //Configure it
                newItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newItem.VerticalAlignment = VerticalAlignment.Top;
                newItem.Width = double.NaN;
                newItem.Height = double.NaN;
                newItem.Margin = new Thickness(0, 0, 0, 8);

                //Inform the data about the save
                newItem.Prepare();

                //Wait before render next to avoid UI freezing
                yield return new WaitForSeconds(0.7f);
            }

            //Enable or hide the empty saves warning
            if (instantiatedSaveItems.Count == 0)
                saveEmptyWarn.Visibility = Visibility.Visible;
            if (instantiatedSaveItems.Count >= 1)
                saveEmptyWarn.Visibility = Visibility.Collapsed;

            //Wait some time
            yield return new WaitForSeconds(1.0f);

            //Show new save notification, if necessary
            int currentSaveCount = allSaveList.Count;
            int lastSaveCount = launcherPrefs.loadedData.lastSaveFilesCount;
            //If the current saves count is not zero, check if was increased
            if (currentSaveCount > 0)
                if (currentSaveCount > lastSaveCount)
                    EnablePageRedDotNotification(LauncherPage.saves);
            //Save the saves count
            launcherPrefs.loadedData.lastSaveFilesCount = currentSaveCount;
            launcherPrefs.Save();

            //Check each save game to see if have a bad save game
            foreach(string saveGamePath in allSaveList)
                if(File.Exists((saveGamePath + "/!bad-game!.bad")) == true)
                {
                    ShowToast(GetStringApplicationResource("launcher_save_badGameDetected"), ToastType.Error);
                    EnablePageRedDotNotification(LauncherPage.saves);
                    break;
                }

            //Remove the task
            RemoveTask("saveListUpdate");

            //Show the content
            saveLoad.Visibility = Visibility.Collapsed;
            saveContent.Visibility = Visibility.Visible;
            saveCount.Content = GetStringApplicationResource("launcher_save_statusCount").Replace("%n%", allSaveList.Count.ToString());
            //Enable the import button
            importSave.Opacity = 0.85f;
            importSave.IsHitTestVisible = true;

            //Auto clear routine reference
            saveListUpdateRoutine = null;
        }
    
        public void ShowVaultTaskBlocker(string messageToShow)
        {
            //Show the message
            vaultTaskMessage.Text = messageToShow;

            //If the routine is already running, stop it
            if (showVaultTaskRoutine != null)
            {
                showVaultTaskRoutine.Dispose();
                showVaultTaskRoutine = null;
            }

            //Start the show routine
            showVaultTaskRoutine = Coroutine.Start(ShowVaultTaskBlockerRoutine());
        }

        private IEnumerator ShowVaultTaskBlockerRoutine()
        {
            //Enable the blocker and run animation of enter
            vaultTaskPopUp.Visibility = Visibility.Visible;
            animStoryboards["vaultTaskEnter"].Begin();

            //Wait end of animation
            yield return new WaitForSeconds(0.3f);

            //Stop the animation
            animStoryboards["vaultTaskEnter"].Stop();

            //Auto clear reference of routine
            showVaultTaskRoutine = null;
        }

        public void HideVaultTaskBlocker()
        {
            //If the routine is already running, stop it
            if (hideVaultTaskRoutine != null)
            {
                hideVaultTaskRoutine.Dispose();
                hideVaultTaskRoutine = null;
            }

            //Start the show routine
            hideVaultTaskRoutine = Coroutine.Start(HideVaultTaskBlockerRoutine());
        }

        private IEnumerator HideVaultTaskBlockerRoutine()
        {
            //Run the animation of exit
            animStoryboards["vaultTaskExit"].Begin();

            //Wait end of animation
            yield return new WaitForSeconds(0.3f);

            //Disable the blocker and stop the animation
            vaultTaskPopUp.Visibility = Visibility.Collapsed;
            animStoryboards["vaultTaskExit"].Stop();

            //Auto clear reference of routine
            hideVaultTaskRoutine = null;
        }
    
        //Tips manager

        private void BuildAndPrepareTipsSystem()
        {
            //Render all tips
            IDisposable coroutine = Coroutine.Start(RenderAllTipsRoutine());

            //Setup the open button
            tipsPush.MouseDown += (s, e) => 
            {
                //If is loading, ignore it
                if (tipsLoad.Visibility == Visibility.Visible)
                    return;

                //If is closed, open
                if(isTipsSectionToggled == false)
                {
                    OpenTipsSection();
                    isTipsSectionToggled = true;
                    return;
                }

                //If is opened, close
                if (isTipsSectionToggled == true)
                {
                    CloseTipsSection();
                    isTipsSectionToggled = false;
                    return;
                }
            };

            //Prepare the background closer
            tipsPopUpBackground.MouseDown += (s, e) => 
            {
                CloseTipsSection();
                isTipsSectionToggled = false;
                return;
            };
        }

        private TipItemTextFormatted GetGrossTipStringFormatted(string source)
        {
            //Prepare the value to return
            TipItemTextFormatted toReturn = new TipItemTextFormatted();

            //Split string parts
            string[] stringParts = source.Split("§");

            //Inform the title and text
            toReturn.title = stringParts[0].Replace("\n", "");

            //Get the text
            string textToReturn = stringParts[1];

            //Replace line breaks with spaces, and remove double spaces
            textToReturn = textToReturn.Replace("\n", " ").Replace("\r", " ").Replace("  ", " ").Replace("  ", " ");

            //Replace '\b' signals with line breaks
            textToReturn = textToReturn.Replace("\\b", "\n").Replace("\\B", "\n");

            //Fix string to don't have spaces before and after line breaks
            string[] textToReturnLines = textToReturn.Split("\n");
            for(int i = 0; i < textToReturnLines.Length; i++)
            {
                //Skip line if don't have text
                if (textToReturnLines[i].Length == 0)
                    continue;

                //Remove spaces in start of the line
                while (textToReturnLines[i][0] == ' ')
                    textToReturnLines[i] = textToReturnLines[i].Remove(0, 1);

                //Remove spaces in end of the line
                while (textToReturnLines[i][(textToReturnLines[i].Length - 1)] == ' ')
                    textToReturnLines[i] = textToReturnLines[i].Remove((textToReturnLines[i].Length - 1), 1);
            }
            //Reset the text to return
            textToReturn = "";
            //Rebuild the original string
            for(int i = 0; i < textToReturnLines.Length; i++)
            {
                if (i > 0)
                    textToReturn += "\n";
                textToReturn += textToReturnLines[i];
            }

            //Inform the text
            toReturn.text = textToReturn;

            //Return the value
            return toReturn;
        }

        private IEnumerator RenderAllTipsRoutine()
        {
            //Show the loading indicator
            tipsLoad.Visibility = Visibility.Visible;
            tipsOpen.Visibility = Visibility.Collapsed;
            tipsClose.Visibility = Visibility.Collapsed;
            //Show loading cursor
            tipsPush.Cursor = Cursors.Wait;

            //Wait some time
            yield return new WaitForSeconds(1.0f);

            //Prepare the index of perfomance tips
            int performanceTipsIndex = 0;
            //Render each performance tip registered in lang files
            while (true == true)
            {
                //Wait before render next to avoid UI freezing
                yield return new WaitForSeconds(0.25f);

                //Get the tip
                string tipGross = GetStringApplicationResource(("launcher_home_performanceTip_" + performanceTipsIndex));

                //If is a error string, cancel
                if (tipGross == "###")
                    break;

                //Get the tip processed
                TipItemTextFormatted tipProcessed = GetGrossTipStringFormatted(tipGross);

                //Render this tip
                TipItem newItem = new TipItem((performanceTipsIndex + 1).ToString(), tipProcessed.title, tipProcessed.text);
                performanceTipsList.Children.Add(newItem);

                //Configure it
                newItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newItem.VerticalAlignment = VerticalAlignment.Top;
                newItem.Width = double.NaN;
                newItem.Height = double.NaN;
                newItem.Margin = new Thickness(0, 0, 0, 8);

                //Increase the index
                performanceTipsIndex += 1;
            }

            //Prepare the index of maintenance tips
            int maintenanceTipsIndex = 0;
            //Render each maintenance tip registered in lang files
            while (true == true)
            {
                //Wait before render next to avoid UI freezing
                yield return new WaitForSeconds(0.5f);

                //Get the tip
                string tipGross = GetStringApplicationResource(("launcher_home_maintenanceTip_" + maintenanceTipsIndex));

                //If is a error string, cancel
                if (tipGross == "###")
                    break;

                //Get the tip processed
                TipItemTextFormatted tipProcessed = GetGrossTipStringFormatted(tipGross);

                //Render this tip
                TipItem newItem = new TipItem((maintenanceTipsIndex + 1).ToString(), tipProcessed.title, tipProcessed.text);
                maintenanceTipsList.Children.Add(newItem);

                //Configure it
                newItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newItem.VerticalAlignment = VerticalAlignment.Top;
                newItem.Width = double.NaN;
                newItem.Height = double.NaN;
                newItem.Margin = new Thickness(0, 0, 0, 8);

                //Increase the index
                maintenanceTipsIndex += 1;
            }

            //Wait some time
            yield return new WaitForSeconds(1.0f);

            //Show loading cursor
            tipsPush.Cursor = Cursors.Hand;
            //Show the open indicator
            tipsLoad.Visibility = Visibility.Collapsed;
            tipsOpen.Visibility = Visibility.Visible;
            tipsClose.Visibility = Visibility.Collapsed;
        }

        private void OpenTipsSection()
        {
            //If the routine is already running, stop it
            if (showTipsSectionRoutine != null)
            {
                showTipsSectionRoutine.Dispose();
                showTipsSectionRoutine = null;
            }

            //Start the show routine
            showTipsSectionRoutine = Coroutine.Start(OpenTipsSectionRoutine());
        }

        private IEnumerator OpenTipsSectionRoutine()
        {
            //Change to close icon
            tipsOpen.Visibility = Visibility.Collapsed;
            tipsClose.Visibility = Visibility.Visible;

            //Enable the tips UI and run animation
            tipsPopUpBackground.Visibility = Visibility.Visible;
            animStoryboards["tipsEnter"].Begin();

            //Wait end of animation
            yield return new WaitForSeconds(0.3f);

            //Stop the animation
            animStoryboards["tipsEnter"].Stop();
            tipsPopUp.Margin = new Thickness(0, 0, 0, 0);

            //Auto clear reference of routine
            showTipsSectionRoutine = null;
        }

        private void CloseTipsSection()
        {
            //If the routine is already running, stop it
            if (hideTipsSectionRoutine != null)
            {
                hideTipsSectionRoutine.Dispose();
                hideTipsSectionRoutine = null;
            }

            //Start the show routine
            hideTipsSectionRoutine = Coroutine.Start(CloseTipsSectionRoutine());
        }

        private IEnumerator CloseTipsSectionRoutine()
        {
            //Change to close icon
            tipsOpen.Visibility = Visibility.Visible;
            tipsClose.Visibility = Visibility.Collapsed;

            //Run the animation of exit
            animStoryboards["tipsExit"].Begin();

            //Wait end of animation
            yield return new WaitForSeconds(0.3f);

            //Disable the tips and stop animation
            tipsPopUpBackground.Visibility = Visibility.Collapsed;
            animStoryboards["tipsExit"].Stop();
            tipsPopUp.Margin = new Thickness(0, 0, -472, 0);

            //Auto clear reference of routine
            hideTipsSectionRoutine = null;
        }
    }
}