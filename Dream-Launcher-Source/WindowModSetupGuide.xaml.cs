using System;
using System.Collections.Generic;
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
    * This is the code responsible by the Sims3Pack Pre Mod Installer window
    */

    public partial class WindowModSetupGuide : Window
    {
        //Core methods

        public WindowModSetupGuide(MainWindow mainWindow, string modTitle, string guideString)
        {
            //Prepare the window
            InitializeComponent();

            //Set the title
            this.Title = mainWindow.GetStringApplicationResource("launcher_mods_addNewTab_recommendedGuideWindowTitle");

            //Set the topic title
            guideTitle.Text = mainWindow.GetStringApplicationResource("launcher_mods_addNewTab_recommendedGuideWindowTopicTitle").Replace("%modName%", modTitle);

            //Show the guide content
            guideContent.Text = guideString.Replace("\\b", "\n").Replace("\\B", "\n");
        }
    }
}
