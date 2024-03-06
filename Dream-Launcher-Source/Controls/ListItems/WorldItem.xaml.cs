using System;
using System.Collections.Generic;
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
using TS3_Dream_Launcher.Scripts;

namespace TS3_Dream_Launcher.Controls.ListItems
{
    /*
     * This script is resposible by the work of the "World Item" that represents each
     * Custom World installed in game...
    */

    public partial class WorldItem : UserControl
    {
        //Private variables
        private WorldInfo worldInfo = null;
        private string thumbPath = "";

        //Public variables
        public MainWindow instantiatedBy = null;
        public string worldFilePath = "";
        public bool foundProblem = false;

        //Core methods

        public WorldItem(MainWindow instantiatedBy, string worldFilePath)
        {
            //Initialize the component
            InitializeComponent();

            //Inform that is the DataConext of this User Control
            this.DataContext = this;

            //Store all the data
            this.instantiatedBy = instantiatedBy;
            this.worldFilePath = worldFilePath;
        }
    
        //Public methods

        public void Prepare()
        {
            //Load the world info
            worldInfo = new WorldInfo(worldFilePath);

            //Check if all files exists
            foreach(string file in worldInfo.loadedData.files)
                if(File.Exists(file) == false)
                {
                    foundProblem = true;
                    break;
                }
            //If found problem, change the border color to red
            if (foundProblem == true)
                background.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

            //Show the name of the world
            name.Content = System.IO.Path.GetFileNameWithoutExtension(worldFilePath);

            //Prepare the file thumb path
            string fileThumbPath = (System.IO.Path.GetDirectoryName(worldFilePath) + "/" + System.IO.Path.GetFileNameWithoutExtension(worldFilePath) + ".bmp" );
            //Store the thumb path
            thumbPath = fileThumbPath;
            //Show the file thumb
            if (File.Exists(fileThumbPath) == true)
            {
                //Prepare the bitmap
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri(fileThumbPath);
                bitmapImage.EndInit();

                //Show on place
                ImageBrush imageBrush = new ImageBrush(bitmapImage);
                imageBrush.TileMode = TileMode.None;
                imageBrush.Stretch = Stretch.UniformToFill;
                thumb.Background = imageBrush;
            }

            //Prepare the more options button
            moreButton.ContextMenu = new ContextMenu();
            //Setup the context menu display
            moreButton.Click += (s, e) =>
            {
                ContextMenu contextMenu = moreButton.ContextMenu;
                contextMenu.PlacementTarget = moreButton;
                contextMenu.IsOpen = true;
                e.Handled = true;
            };

            //Add "uninstall" option to options menu
            MenuItem uninstallItem = new MenuItem();
            uninstallItem.Header = instantiatedBy.GetStringApplicationResource("launcher_world_unintallButton");
            uninstallItem.Click += (s, e) => { Uninstall(); };
            moreButton.ContextMenu.Items.Add(uninstallItem);
        }

        //Auxiliar methods

        private void Uninstall()
        {
            //Show the confirmation
            MessageBoxResult dialogResult = MessageBox.Show(instantiatedBy.GetStringApplicationResource("launcher_world_unintallDialogText"),
                                                            instantiatedBy.GetStringApplicationResource("launcher_world_unintallDialogTitle"), MessageBoxButton.YesNo, MessageBoxImage.Warning);

            //If is desired to delete, do it
            if (dialogResult == MessageBoxResult.Yes)
            {
                //Delete each file
                foreach(string filePath in worldInfo.loadedData.files)
                    if (File.Exists(filePath) == true)
                        File.Delete(filePath);

                //Delete the thumb if have
                if (File.Exists(thumbPath) == true)
                    File.Delete(thumbPath);

                //Delete this world info file
                File.Delete(worldFilePath);

                //Update worlds list
                instantiatedBy.UpdateWorldList();
            }
        }
    }
}
