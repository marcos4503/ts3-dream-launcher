using CoroutinesDotNet;
using CoroutinesForWpf;
using MarcosTomaz.ATS;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
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
using TS3_Dream_Launcher.Scripts;

namespace TS3_Dream_Launcher
{
    public partial class MainWindow : Window
    {
        //Enums of script
        private enum ToastType
        {
            Error,
            Success
        }
        private enum LauncherPage
        {
            home,
            saves,
            sims,
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

        //Cache variables
        private double thisWindowLeftPosition = 0;
        private double thisWindowTopPosition = 0;
        private WindowIconViewer currentOpenedIconViewer = null;
        private bool isToastHistoryToggled = false;
        private int currentToastsInHistory = 0;
        private bool isPlayingTheGame = false;
        private string myDocumentsPath = "";
        private bool[] availableExpansionPacks = new bool[12];
        private bool[] availableStuffPacks = new bool[10];

        //Private variables
        private Preferences launcherPrefs = null;
        private IDictionary<string, Storyboard> animStoryboards = new Dictionary<string, Storyboard>();
        private System.Windows.Forms.NotifyIcon launcherTrayIcon = null;
        private IDictionary<string, string> runningTasks = new Dictionary<string, string>();
        private Process currentGameProcess = null;

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
                bool steamAppIdExists = File.Exists(((new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent).FullName + "/steam_appid.txt"));
                bool installScriptExists = File.Exists(((new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent).FullName + "/installscript_249181_windows.vdf"));
                if(steamAppIdExists == true || installScriptExists == true)
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
            ImageBrush wallpaperBrush = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/wallpaper-" + (new Random().Next(0, 4)) + ".jpg")));
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

                    //Store it
                    currentGameProcess = newProcess;

                    //Add a task of playing
                    AddTask("playing", "Playing the game.");
                    //Store the current window position
                    thisWindowLeftPosition = this.Left;
                    thisWindowTopPosition = this.Top;
                    //Hide this window
                    this.Visibility = Visibility.Collapsed;
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
                        //Inform that is not playing
                        isPlayingTheGame = false;
                        //Update the tray icon
                        UpdateLauncherSystemTray();
                    };
                    asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
                }
                catch (Exception ex) { ShowToast((GetStringApplicationResource("launcher_launchGameProblem") + " \"" + ex.Message + "\""), ToastType.Error); }
            };

            //Add a temporary task to wait for play
            AddTask("prepareLauncher", "Preparing the Launcher to play!");
            //Create a thread to remove the temporary task
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { });
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(3000);

                //Return empty response
                return new string[] { };
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) => 
            {
                //Remove the task
                RemoveTask("prepareLauncher");
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);

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
            goSims.Click += (s, e) => { SwitchPage(LauncherPage.sims); };
            goWorlds.Click += (s, e) => { SwitchPage(LauncherPage.worlds); };
            goMedia.Click += (s, e) => { SwitchPage(LauncherPage.media); };
            goCache.Click += (s, e) => { SwitchPage(LauncherPage.cache); };
            goPatches.Click += (s, e) => { SwitchPage(LauncherPage.patches); };
            goMods.Click += (s, e) => { SwitchPage(LauncherPage.mods); };
            goTools.Click += (s, e) => { SwitchPage(LauncherPage.tools); };
            goSettings.Click += (s, e) => { SwitchPage(LauncherPage.settings); };
            goGithub.Click += (s, e) => { System.Diagnostics.Process.Start(new ProcessStartInfo {FileName = "https://github.com/marcos4503/ts3-dream-launcher", UseShellExecute = true }); };
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
                goSims.IsEnabled = false;
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

            //Get a copy of "Options.ini" as template for settings appy, if don't have one
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
            //Show the settings
            ShowAllSettings();
            //Prepare the save settings button to save, and apply the settings automatically
            settingsSave.Click += (s, e) => 
            { 
                SaveAllSettings();
                ApplyAllSettings(); 
            };
            //Apply all settings automatically
            ApplyAllSettings();
        }

        //Tasks manager

        private void AddTask(string id, string description)
        {
            //Add the task for the queue
            runningTasks.Add(id, description);

            //Update the tasks display
            UpdateTasksDisplay();
        }

        private void RemoveTask(string id)
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
        }

        private int GetRunningTasksCount()
        {
            //Return the running tasks count
            return runningTasks.Keys.Count;
        }

        //Toast manager

        private void ShowToast(string message, ToastType toastType)
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
                case LauncherPage.sims:
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
            buttons.Add(goSims);
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
            pages.Add(pageSims);
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
            titles.Add(GetStringApplicationResource("launcher_button_goSims"));
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
                if(dialogResult == MessageBoxResult.No)
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

        private string GetStringApplicationResource(string resourceKey)
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

            //Graphics tab
            //*** set_Resolution
            launcherPrefs.loadedData.resolution = set_Resolution.SelectedIndex;
            //*** set_RefreshRate
            launcherPrefs.loadedData.refreshRate = set_RefreshRate.SelectedIndex;
            //*** set_MaxFps
            launcherPrefs.loadedData.maxFps = set_MaxFps.SelectedIndex;

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
                threadTools.MakeThreadSleep(5000);

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

                //Copy the options template for my documents
                if (File.Exists((myDocumentsPath + "/Options.ini")) == true)
                    File.Delete((myDocumentsPath + "/Options.ini"));
                File.Copy((Directory.GetCurrentDirectory() + @"/Content/options-template.ini"), (myDocumentsPath + "/Options.ini"));

                //Read the Options.ini
                IniReader optionsIni = new IniReader((myDocumentsPath + "/Options.ini"));

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

             

                //Save the updates
                optionsIni.Save();

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

        //Patches

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
                    ShowToast((GetStringApplicationResource("launcher_alderLakePatchProblem") + " \"" + backgroundResult[1] + "\""), ToastType.Error);

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
                    ShowToast((GetStringApplicationResource("launcher_ptptToptbrTranslateProblem") + " \"" + backgroundResult[1] + "\""), ToastType.Error);

                //Remove the task
                RemoveTask("ptPtToPtBrTranslatePatching");
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }
    }
}
