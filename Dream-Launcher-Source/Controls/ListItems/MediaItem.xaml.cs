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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TS3_Dream_Launcher.Controls.ListItems
{
    /*
     * This script is resposible by the work of the "Media Item" that represents each
     * Media resource found in documents...
    */

    public partial class MediaItem : UserControl
    {
        //Classes of script
        public class ClassDelegates
        {
            public delegate void OnClick();
        }

        //Private variables
        private event ClassDelegates.OnClick onClick;

        //Public variables
        public MainWindow instantiatedBy = null;
        public bool isSelected = false;
        public string filePath = "";

        //Core methods

        public MediaItem(MainWindow instantiatedBy, string filePath)
        {
            //Initialize the component
            InitializeComponent();

            //Inform that is the DataConext of this User Control
            this.DataContext = this;

            //Store all the data
            this.instantiatedBy = instantiatedBy;
            this.filePath = filePath;
        }

        //Public methods

        public void RegisterOnClickCallback(ClassDelegates.OnClick onClick)
        {
            //Register the event
            this.onClick = onClick;
        }

        public void Prepare()
        {
            //Prepare the color highlight
            background.MouseEnter += (s, e) =>
            {
                if (isSelected == false)
                    background.Background = new SolidColorBrush(Color.FromArgb(255, 239, 250, 255));
                if (isSelected == true)
                    background.Background = new SolidColorBrush(Color.FromArgb(255, 212, 250, 253));
            };
            background.MouseLeave += (s, e) =>
            {
                if (isSelected == false)
                    background.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                if (isSelected == true)
                    background.Background = new SolidColorBrush(Color.FromArgb(255, 212, 250, 253));
            };

            //Prepare the selection
            background.MouseDown += (s, e) => 
            {
                //If is currently selected
                if(isSelected == true)
                {
                    //Inform selection
                    isSelected = false;
                    background.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

                    //Run callback
                    if (this.onClick != null)
                        this.onClick();

                    //Cancel
                    return;
                }

                //If is currently not selected
                if(isSelected == false)
                {
                    //Inform selection
                    isSelected = true;
                    background.Background = new SolidColorBrush(Color.FromArgb(255, 212, 250, 253));

                    //Run callback
                    if (this.onClick != null)
                        this.onClick();

                    //Cancel
                    return;
                }
            };

            //Show the media name
            name.Content = System.IO.Path.GetFileNameWithoutExtension(filePath);

            //Prepare the play button
            openButton.Click += (s, e) => 
            {
                //Open the file with default application
                Process fileopener = new Process();
                fileopener.StartInfo.FileName = "explorer";
                fileopener.StartInfo.Arguments = "\"" + filePath + "\"";
                fileopener.Start();
            };

            //Update the thumb of this media
            UpdateThumb();
        }

        //Auxiliar methods

        public void UpdateThumb()
        {
            //Get the extension of media
            string mediaExtension = System.IO.Path.GetExtension(filePath).ToLower().Replace(".", "");

            //If is a screenshot
            if(mediaExtension == "jpg")
            {
                //Prepare the bitmap
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri(filePath);
                bitmapImage.EndInit();

                //Show on place
                ImageBrush imageBrush = new ImageBrush(bitmapImage);
                imageBrush.TileMode = TileMode.None;
                imageBrush.Stretch = Stretch.UniformToFill;
                thumb.Background = imageBrush;
            }

            //If is a video
            if(mediaExtension == "avi")
            {
                //Prepare the video cache and thumb path
                string videoThumbsCache = (instantiatedBy.myDocumentsPath + "/!DL-TmpCache/video-thumbs");
                string targetThumbPath = (instantiatedBy.myDocumentsPath + "/!DL-TmpCache/video-thumbs/" + (System.IO.Path.GetFileNameWithoutExtension(filePath)) + ".jpg");

                //Create directory if not exists
                if (Directory.Exists(videoThumbsCache) == false)
                    Directory.CreateDirectory(videoThumbsCache);

                //If the thumb already exists, delete it
                if (File.Exists(targetThumbPath) == true)
                    File.Delete(targetThumbPath);

                //Get the video thumbnail
                NReco.VideoConverter.FFMpegConverter ffmpeg = new NReco.VideoConverter.FFMpegConverter();
                ffmpeg.GetVideoThumbnail(filePath, targetThumbPath, 1);

                //Prepare the bitmap
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri(targetThumbPath);
                bitmapImage.EndInit();

                //Show on place
                ImageBrush imageBrush = new ImageBrush(bitmapImage);
                imageBrush.TileMode = TileMode.None;
                imageBrush.Stretch = Stretch.UniformToFill;
                thumb.Background = imageBrush;
            }
        }
    }
}
