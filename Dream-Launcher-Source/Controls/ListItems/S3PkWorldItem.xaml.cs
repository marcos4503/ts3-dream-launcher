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

namespace TS3_Dream_Launcher.Controls.ListItems
{
    /*
     * This script is resposible by the work of the "Sims3Pack World Item" that represents each
     * package file inside a Sims3Pack file.
    */

    public partial class S3PkWorldItem : UserControl
    {
        //Public enums
        public enum FileType
        {
            None,
            World,
            Mod,
            Library
        }

        //Public variables
        public MainWindow instantiatedBy = null;
        public string filePath = "";
        public FileType currentFileType = FileType.None;
        public long currentFileSize = 0;

        //Core methods

        public S3PkWorldItem(MainWindow instantiatedBy)
        {
            //Initialize the component
            InitializeComponent();

            //Inform that is the DataConext of this User Control
            this.DataContext = this;

            //Store all the data
            this.instantiatedBy = instantiatedBy;
        }
    
        public void SetFilePath(string filePath)
        {
            //Store the file path
            this.filePath = filePath;
        }

        public void SetFileType(FileType newType)
        {
            //Store the current file type
            currentFileType = newType;

            //Show the icon of file
            if (currentFileType == FileType.None)
            {
                currentType.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/wfile-type-none.png"));
                changeButton.ToolTip = instantiatedBy.GetStringApplicationResource("launcher_world_install_currentTypeNone");
            }
            if (currentFileType == FileType.World)
            {
                currentType.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/wfile-type-world.png"));
                changeButton.ToolTip = instantiatedBy.GetStringApplicationResource("launcher_world_install_currentTypeWorld");
            }
            if (currentFileType == FileType.Mod)
            {
                currentType.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/wfile-type-mod.png"));
                changeButton.ToolTip = instantiatedBy.GetStringApplicationResource("launcher_world_install_currentTypeMod");
            }
            if (currentFileType == FileType.Library)
            {
                currentType.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/wfile-type-library.png"));
                changeButton.ToolTip = instantiatedBy.GetStringApplicationResource("launcher_world_install_currentTypeLibrary");
            }
        }

        public void Prepare()
        {
            //Prepare the color highlight
            background.MouseEnter += (s, e) =>
            {
                background.Background = new SolidColorBrush(Color.FromArgb(255, 239, 250, 255));
            };
            background.MouseLeave += (s, e) =>
            {
                background.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            };

            //Show the name and size
            name.Content = System.IO.Path.GetFileNameWithoutExtension(filePath);
            size.Content = GetFormattedFileSize((new FileInfo(filePath)).Length).Replace("~", "");

            //Store the file size
            currentFileSize = (new FileInfo(filePath)).Length;

            //Prepare the change button
            changeButton.ContextMenu = new ContextMenu();
            //Setup the context menu display
            changeButton.Click += (s, e) =>
            {
                ContextMenu contextMenu = changeButton.ContextMenu;
                contextMenu.PlacementTarget = changeButton;
                contextMenu.IsOpen = true;
                e.Handled = true;
            };

            //Add "Set Type: None" option to options menu
            MenuItem typeNoneItem = new MenuItem();
            typeNoneItem.Header = instantiatedBy.GetStringApplicationResource("launcher_world_install_setTypeNone");
            typeNoneItem.Click += (s, e) => { SetFileType(FileType.None); };
            changeButton.ContextMenu.Items.Add(typeNoneItem);

            //Add "Set Type: World" option to options menu
            MenuItem typeWorldItem = new MenuItem();
            typeWorldItem.Header = instantiatedBy.GetStringApplicationResource("launcher_world_install_setTypeWorld");
            typeWorldItem.Click += (s, e) => { SetFileType(FileType.World); };
            changeButton.ContextMenu.Items.Add(typeWorldItem);

            //Add "Set Type: Mod" option to options menu
            MenuItem typeModItem = new MenuItem();
            typeModItem.Header = instantiatedBy.GetStringApplicationResource("launcher_world_install_setTypeMod");
            typeModItem.Click += (s, e) => { SetFileType(FileType.Mod); };
            changeButton.ContextMenu.Items.Add(typeModItem);

            //Add "Set Type: Library" option to options menu
            MenuItem typeLibraryItem = new MenuItem();
            typeLibraryItem.Header = instantiatedBy.GetStringApplicationResource("launcher_world_install_setTypeLibrary");
            typeLibraryItem.Click += (s, e) => { SetFileType(FileType.Library); };
            changeButton.ContextMenu.Items.Add(typeLibraryItem);
        }

        //Auxiliar methods

        private string GetFormattedFileSize(double bytesSize)
        {
            //Calculate to MB, KB and GB
            float gbSize = (float)(((bytesSize / 1000.0f) / 1000.0f) / 1000.0f);
            float mbSize = (float)((bytesSize / 1000.0f) / 1000.0f);
            float kbSize = (float)(bytesSize / 1000.0f);

            //Fix the KB size
            if (kbSize > 0.0f && kbSize < 1.0f)
                kbSize = 1.0f;
            //Calculate the pre size prefix
            string prefix = ((kbSize > 0.0f) ? "~" : "");

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
    }
}
