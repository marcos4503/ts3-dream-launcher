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
using System.Windows.Shapes;

namespace TS3_Dream_Launcher
{
    /*
    * This is the code responsible by the Save Cleaner window
    */

    public partial class WindowSaveCleaner : Window
    {
        //Cache variables
        private bool isCleaningInProgress = false;

        //Private variables
        private MainWindow mainWindowRef = null;
        private string saveGameDirPath = "";
        private string[] saveGameNhdFilesPaths = new string[0];
        private int saveGameMainNhdIndex = -1;

        //Core methods

        public WindowSaveCleaner(MainWindow mainWindowRef, string saveGameDirPath, string[] saveGameNhdFilesPaths, int saveGameMainNhdIndex)
        {
            //Prepare the window
            InitializeComponent();

            //Store reference to main window
            this.mainWindowRef = mainWindowRef;

            //Store data
            this.saveGameDirPath = saveGameDirPath;
            this.saveGameNhdFilesPaths = saveGameNhdFilesPaths;
            this.saveGameMainNhdIndex = saveGameMainNhdIndex;

            //Prepare the UI
            PrepareTheUI();
        }

        private void PrepareTheUI()
        {
            //Block the window close if a clean is in progress
            this.Closing += (s, e) =>
            {
                if (isCleaningInProgress == true)
                {
                    MessageBox.Show(mainWindowRef.GetStringApplicationResource("launcher_save_cleanExitErrorText"),
                                    mainWindowRef.GetStringApplicationResource("launcher_save_cleanExitErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                }
            };

            //Show the title
            this.Title = mainWindowRef.GetStringApplicationResource("launcher_save_clean_title");

            //Render the language
            title.Text = (new DirectoryInfo(saveGameDirPath)).Name.Split(".")[0];
            prefsTitle.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_box");
            snapsThumbsLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_snapsAndThumbs");
            snapsThumbsHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_snapsAndThumbsHelp");
            memsPhotosPaintLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_memsPhotosPaint");
            memsPhotosPaintHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_memsPhotosPaintHelp");
            seasonsCardsLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_seasonsCards");
            seasonsCardsHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_seasonsCardsHelp");
            promPhotosLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_promPhotos");
            promPhotosHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_promPhotosHelp");
            cabinPhotosLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_cabinPhotos");
            cabinPhotosHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_cabinPhotosHelp");
            shangSimlaWorldLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_shangSimla");
            shangSimlaWorldHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_worldHelp");
            champsLesSimsLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_champsLesSims");
            champsLesSimsHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_worldHelp");
            alSimharaLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_alSimhara");
            alSimharaHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_worldHelp");
            simsUniversityLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_simsUniversity");
            simsUniversityHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_worldHelp");
            oasisLandingLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_oasisLanding");
            oasisLandingHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_worldHelp");
            tipText.Text = mainWindowRef.GetStringApplicationResource("launcher_save_clean_tipText");
            clearButton.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_clearButton");
        }
    }
}
