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
using System.Text;
using System.Text.RegularExpressions;
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
using TS3_Dream_Launcher.Scripts;

namespace TS3_Dream_Launcher
{
    /*
    * This is the code responsible by the World Installer window
    */

    public partial class WindowWorldInstaller : Window
    {
        //Cache variables
        private bool isExtractionInProgress = false;
        private bool isInstallInProgress = false;
        private Dictionary<string, int> installedWorldsNames = new Dictionary<string, int>();
        private string pickedThumbnailPath = "";

        //Private variables
        private IDictionary<string, Storyboard> animStoryboards = new Dictionary<string, Storyboard>();
        private MainWindow mainWindowRef = null;
        private string contentPath = "";
        private string worldFilePath = "";

        //Public variables
        public List<S3PkWorldItem> instantiatedWorldItems = new List<S3PkWorldItem>();

        //Core methods

        public WindowWorldInstaller(MainWindow mainWindowRef, string contentPath, string worldFilePath)
        {
            //Prepare the window
            InitializeComponent();

            //Store reference to main window
            this.mainWindowRef = mainWindowRef;

            //Store data
            this.contentPath = contentPath;
            this.worldFilePath = worldFilePath;

            //Load all animations
            LoadAllStoryboardsAnimationsReferences();

            //Prepare the UI
            PrepareTheUI();
        }

        private void LoadAllStoryboardsAnimationsReferences()
        {
            //Load references for all storyboards animations of this screen
            animStoryboards.Add("extractionExit", (FindResource("extractionExit") as Storyboard));
            animStoryboards.Add("installEnter", (FindResource("installEnter") as Storyboard));
        }

        private void PrepareTheUI()
        {
            //Block the window close if install is in progress
            this.Closing += (s, e) =>
            {
                if (isExtractionInProgress == true)
                {
                    MessageBox.Show(mainWindowRef.GetStringApplicationResource("launcher_world_install_exitFail0Text"),
                                    mainWindowRef.GetStringApplicationResource("launcher_world_install_exitFail0Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                    return;
                }

                if (isInstallInProgress == true)
                {
                    MessageBox.Show(mainWindowRef.GetStringApplicationResource("launcher_world_install_exitFail1Text"),
                                    mainWindowRef.GetStringApplicationResource("launcher_world_install_exitFail1Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                    return;
                }
            };

            //Fill the language texts
            this.Title = mainWindowRef.GetStringApplicationResource("launcher_world_install_title");
            extractTitle.Text = mainWindowRef.GetStringApplicationResource("launcher_world_install_extractingTitle");
            name.LabelName = mainWindowRef.GetStringApplicationResource("launcher_world_install_worldName");
            pickThumb.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_world_install_pickThumbTooltip");
            titleMeta.Text = mainWindowRef.GetStringApplicationResource("launcher_world_install_metaTitle");
            titleFiles.Text = mainWindowRef.GetStringApplicationResource("launcher_world_install_filesTitle");
            tipText.Text = mainWindowRef.GetStringApplicationResource("launcher_world_install_tipText");
            installButton.Content = mainWindowRef.GetStringApplicationResource("launcher_world_install_installButton");

            //Change to extraction screen
            extractionUi.Visibility = Visibility.Visible;
            installUi.Visibility = Visibility.Collapsed;

            //Search by all worlds names installed
            if (Directory.Exists((mainWindowRef.myDocumentsPath + "/!DL-Static/custom-worlds")) == true)
                foreach (FileInfo file in (new DirectoryInfo((mainWindowRef.myDocumentsPath + "/!DL-Static/custom-worlds")).GetFiles()))
                    if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".json")
                        installedWorldsNames.Add(System.IO.Path.GetFileNameWithoutExtension(file.FullName).ToLower(), 0);

            //Setup the validation in for the text field
            name.RegisterOnTextChangedValidationCallback((currentInput) =>
            {
                //Prepare the value to return
                string toReturn = "";

                //Check if is empty, cancel here
                if (currentInput == "")
                {
                    toReturn = mainWindowRef.GetStringApplicationResource("launcher_world_install_invalidNameEmpty");
                    return toReturn;
                }
                //Check if is too long
                if (currentInput.Length > 64)
                    toReturn = mainWindowRef.GetStringApplicationResource("launcher_world_install_invalidNameLong");
                //Check if have only allowed characters
                if (Regex.IsMatch(currentInput, @"^[a-zA-Z ]+$") == false)
                    toReturn = mainWindowRef.GetStringApplicationResource("launcher_world_install_invalidNameLetters");
                //Check if have double spaces
                if (currentInput.Contains("  ") == true)
                    toReturn = mainWindowRef.GetStringApplicationResource("launcher_world_install_invalidNameSpace");
                //Check if the first or last character is space
                if (currentInput[0].ToString() == " " || currentInput[currentInput.Length - 1].ToString() == " ")
                    toReturn = mainWindowRef.GetStringApplicationResource("launcher_world_install_invalidNameSpaceFirst");
                //Check if the file already exists
                if(installedWorldsNames.ContainsKey(currentInput.ToLower()) == true)
                    toReturn = mainWindowRef.GetStringApplicationResource("launcher_world_install_invalidNameExists");

                //Return the value
                return toReturn;
            });

            //Prepare the thumbnail picker
            pickThumb.MouseDown += (s, e) =>
            {
                //Open the file picker
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Filter = "Image Files|*.png;*.jpg;*.jpeg";
                bool? result = fileDialog.ShowDialog();

                //If don't have picked file, cancel
                if (result == false || fileDialog.FileName == "")
                    return;

                //Get the resolution of image
                BitmapFrame bitmapFrame = BitmapFrame.Create(new Uri(fileDialog.FileName), BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);

                //If is not a square image, warn and cancel
                if (bitmapFrame.PixelWidth != bitmapFrame.PixelHeight)
                {
                    MessageBox.Show(mainWindowRef.GetStringApplicationResource("launcher_world_install_invalidImageText"),
                                    mainWindowRef.GetStringApplicationResource("launcher_world_install_invalidImageTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //Load the bitmap from the file and resize it
                BitmapImage bitmapSource = new BitmapImage(new Uri(fileDialog.FileName));
                TransformedBitmap transformedBitmap = new TransformedBitmap(bitmapSource, new ScaleTransform(512.0d / ((float)bitmapFrame.PixelWidth), 512.0d / ((float)bitmapFrame.PixelHeight)));

                //Prepare the path for the thumbnail on cache
                string thumbnailTargetPath = (mainWindowRef.myDocumentsPath + "/!DL-TmpCache/world-thumb-install.bmp");
                //If the thumbnail already exists, delete it
                if (File.Exists(thumbnailTargetPath) == true)
                    File.Delete(thumbnailTargetPath);

                //Save the resized copied thumbnail to cache
                BitmapSource imageToSave = transformedBitmap;
                using (var fileStream = new FileStream(thumbnailTargetPath, FileMode.Create))
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(imageToSave));
                    encoder.Save(fileStream);
                }

                //Inform the path to the thumbnail image
                pickedThumbnailPath = thumbnailTargetPath;

                //Prepare the picked thumbnail
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri(pickedThumbnailPath);
                bitmapImage.EndInit();
                //Show on place
                ImageBrush imageBrush = new ImageBrush(bitmapImage);
                imageBrush.TileMode = TileMode.None;
                imageBrush.Stretch = Stretch.UniformToFill;
                thumb.Background = imageBrush;
            };

            //Prepare the install button
            installButton.Click += (s, e) =>
            {
                //Finish the install
                IDisposable routine = Coroutine.Start(FinishTheInstall());
            };

            //Start the extraction
            ExtractSims3Pack();
        }

        private void ExtractSims3Pack()
        {
            //Inform that the extraction is in progress
            isExtractionInProgress = true;

            //Create a thread to make the extraction
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) => { };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(1000);

                //Try to do the task
                try
                {
                    //Delete the temporary extraction folder, if exists
                    if (Directory.Exists((mainWindowRef.myDocumentsPath + "/!DL-TmpCache/world-s3pkg-extract")) == true)
                        Directory.Delete((mainWindowRef.myDocumentsPath + "/!DL-TmpCache/world-s3pkg-extract"), true);
                    //Create the temporary directory
                    Directory.CreateDirectory((mainWindowRef.myDocumentsPath + "/!DL-TmpCache/world-s3pkg-extract"));

                    //Prepare the path of the target sims3pack inside the cache
                    string s3pkgCachePath = (mainWindowRef.myDocumentsPath + "/!DL-TmpCache/world-s3pkg-extract/" + System.IO.Path.GetFileNameWithoutExtension(worldFilePath) + ".sims3pack");

                    //If the sims3pack cache file already exists inside cache remove it
                    if (File.Exists(s3pkgCachePath) == true)
                        File.Delete(s3pkgCachePath);

                    //Copy the sims3pack file to cache
                    File.Copy(worldFilePath, s3pkgCachePath);

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
                    threadTools.MakeThreadSleep(1000);

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
                //Render each package found, and enable the installator
                IDisposable routine = Coroutine.Start(RenderEachPackageFoundAndEnableTheInstallator());
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        private IEnumerator RenderEachPackageFoundAndEnableTheInstallator()
        {
            //Put the name automatically
            name.textBox.Text = System.IO.Path.GetFileNameWithoutExtension(worldFilePath);

            //Build the list of package files found inside the extracted sims3pack file
            List<string> packageFilesExtracted = new List<string>();
            foreach (FileInfo file in (new DirectoryInfo((mainWindowRef.myDocumentsPath + "/!DL-TmpCache/world-s3pkg-extract")).GetFiles()))
                if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".package")
                    packageFilesExtracted.Add(file.FullName);

            //Render each file found inside the sims3pack file
            for(int i = 0; i < packageFilesExtracted.Count; i++)
            {
                //Create the item to display
                S3PkWorldItem newItem = new S3PkWorldItem(mainWindowRef);
                filesList.Children.Add(newItem);
                instantiatedWorldItems.Add(newItem);

                //Configure it
                newItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newItem.VerticalAlignment = VerticalAlignment.Top;
                newItem.Width = double.NaN;
                newItem.Height = double.NaN;
                newItem.Margin = new Thickness(0, 0, 0, 0);

                //Inform the data about the mod
                newItem.SetFilePath(packageFilesExtracted[i]);
                newItem.SetFileType(S3PkWorldItem.FileType.Mod);
                newItem.Prepare();
            }

            //If have files, try to determine the world file automatically
            if(packageFilesExtracted.Count > 0)
            {
                //Store the ID of current world file
                int currentWorldFileId = 0;
                //Try do determine the world file automatically
                for (int i = 0; i < instantiatedWorldItems.Count; i++)
                    if (instantiatedWorldItems[i].currentFileSize >= instantiatedWorldItems[currentWorldFileId].currentFileSize)
                        currentWorldFileId = i;
                //Set the world file type as "world"
                instantiatedWorldItems[currentWorldFileId].SetFileType(S3PkWorldItem.FileType.World);

                //Test the world name, check the validation
                name.hasError();
            }

            //If don't have files inside the sims3pack file, warn and disable install button
            if (packageFilesExtracted.Count == 0)
            {
                //Disable the UI
                content.IsHitTestVisible = false;
                content.Opacity = 0.25f;
                installButton.IsEnabled = false;

                //Show the warning
                MessageBox.Show(mainWindowRef.GetStringApplicationResource("launcher_world_install_installEmptyText"),
                                mainWindowRef.GetStringApplicationResource("launcher_world_install_installEmptyTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //Wait time
            yield return new WaitForSeconds(1.0);

            //Exit from prompt
            animStoryboards["extractionExit"].Begin();

            //Wait time
            yield return new WaitForSeconds(0.3);

            //Disable the prompt and enable install ui
            extractionUi.Visibility = Visibility.Collapsed;
            installUi.Visibility = Visibility.Visible;

            //Start the animation
            animStoryboards["installEnter"].Begin();

            //Wait time
            yield return new WaitForSeconds(0.3);

            //Inform that extraction was done
            isExtractionInProgress = false;
        }
    
        private IEnumerator FinishTheInstall()
        {
            //Prepare the validation result
            bool hasErrors = false;

            //Count quantity of world files type
            int worldFilesType = 0;
            foreach (S3PkWorldItem item in instantiatedWorldItems)
                if (item.currentFileType == S3PkWorldItem.FileType.World)
                    worldFilesType += 1;

            //If have less than 1 world file
            if (worldFilesType == 0)
            {
                MessageBox.Show(mainWindowRef.GetStringApplicationResource("launcher_world_install_installErrorTextTooLessWorlds"),
                                mainWindowRef.GetStringApplicationResource("launcher_world_install_installErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                hasErrors = true;
            }
            //If have more than 1 world file
            if (worldFilesType > 1)
            {
                MessageBox.Show(mainWindowRef.GetStringApplicationResource("launcher_world_install_installErrorTextTooManyWorlds"),
                                mainWindowRef.GetStringApplicationResource("launcher_world_install_installErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                hasErrors = true;
            }
            //If have an error in name of the world, cancel
            if (name.hasError() == true)
            {
                MessageBox.Show(mainWindowRef.GetStringApplicationResource("launcher_world_install_installErrorTextNameError"),
                                mainWindowRef.GetStringApplicationResource("launcher_world_install_installErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                hasErrors = true;
            }

            //If don't have errors, continue
            if (hasErrors == false)
            {
                //Inform that is installing
                isInstallInProgress = true;

                //Change the UI
                content.IsHitTestVisible = false;
                content.Opacity = 0.25f;
                installButton.IsEnabled = false;
                installButton.Visibility = Visibility.Collapsed;
                installingGif.Visibility = Visibility.Visible;

                //Wait time
                yield return new WaitForSeconds(2.0);

                //Save the JSON of the world
                WorldInfo worldInfo = new WorldInfo((mainWindowRef.myDocumentsPath + "/!DL-Static/custom-worlds/" + name.textBox.Text + ".json"));

                //Save the thumbnail
                if (pickedThumbnailPath == "")
                {
                    BitmapSource imageToSave = (BitmapSource)((ImageBrush)thumb.Background).ImageSource;
                    using (var fileStream = new FileStream((mainWindowRef.myDocumentsPath + "/!DL-Static/custom-worlds/" + name.textBox.Text + ".bmp"), FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(imageToSave));
                        encoder.Save(fileStream);
                    }
                }
                if (pickedThumbnailPath != "")
                    File.Copy(pickedThumbnailPath, (mainWindowRef.myDocumentsPath + "/!DL-Static/custom-worlds/" + name.textBox.Text + ".bmp"));

                //Prepare the list of files
                List<string> installedFiles = new List<string>();

                //Copy all files to right places
                foreach(S3PkWorldItem item in instantiatedWorldItems)
                {
                    //If is type of none, skip it
                    if (item.currentFileType == S3PkWorldItem.FileType.None)
                        continue;

                    //If is a world file type
                    if (item.currentFileType == S3PkWorldItem.FileType.World)
                    {
                        //Prepare the target final path
                        string finalPath = (((new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent).FullName + "/GameData/Shared/NonPackaged/Worlds/" + name.textBox.Text + ".world"));

                        //Move the file
                        File.Move(item.filePath, finalPath);

                        //Add it to list of installed files
                        installedFiles.Add(finalPath);
                    }

                    //If is a mod file type
                    if (item.currentFileType == S3PkWorldItem.FileType.Mod)
                    {
                        //Prepare the target final path
                        string finalPath = (mainWindowRef.myDocumentsPath + "/Mods/Packages/DL3-Custom/OTHERS --- World Dependency - " + name.textBox.Text + ".package");

                        //Move the file
                        File.Move(item.filePath, finalPath);

                        //Add it to list of installed files
                        installedFiles.Add(finalPath);
                    }
                }

                //Save the file
                worldInfo.loadedData.files = installedFiles.ToArray();
                worldInfo.Save();

                //Wait time
                yield return new WaitForSeconds(1.0);

                //Inform that installation was finished
                isInstallInProgress = false;

                //Close this window
                this.Close();

                //Force update the worlds list
                mainWindowRef.UpdateWorldList();
            }
        }
    }
}
