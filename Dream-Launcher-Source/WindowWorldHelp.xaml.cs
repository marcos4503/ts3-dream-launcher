using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public partial class WindowWorldHelp : Window
    {
        //Core methods

        public WindowWorldHelp(MainWindow mainWindow)
        {
            //Prepare the window
            InitializeComponent();

            //Set the title
            this.Title = mainWindow.GetStringApplicationResource("launcher_world_helpTitle");

            //Load all text
            topic1Title.Text = mainWindow.GetStringApplicationResource("launcher_world_help_topic1Title");
            topic1Text.Text = mainWindow.GetStringApplicationResource("launcher_world_help_topic1Text");
            topic2Title.Text = mainWindow.GetStringApplicationResource("launcher_world_help_topic2Title");
            topic2Text.Text = mainWindow.GetStringApplicationResource("launcher_world_help_topic2Text");
            topic3Title.Text = mainWindow.GetStringApplicationResource("launcher_world_help_topic3Title");
            topic3Text.Text = mainWindow.GetStringApplicationResource("launcher_world_help_topic3Text");
            topic4Title.Text = mainWindow.GetStringApplicationResource("launcher_world_help_topic4Title");
            topic4Text.Text = mainWindow.GetStringApplicationResource("launcher_world_help_topic4Text");
            topic5Title.Text = mainWindow.GetStringApplicationResource("launcher_world_help_topic5Title");
            topic5Text.Text = mainWindow.GetStringApplicationResource("launcher_world_help_topic5Text");

            //Build the links
            link1.Content = "MY SIM REALITY";
            link1.Click += (s, e) => { Process.Start(new ProcessStartInfo { FileName = "https://www.mysimrealty.com/Sims3_Worlds.html", UseShellExecute = true }); };
            link2.Content = "THE SIMS CATALOG";
            link2.Click += (s, e) => { Process.Start(new ProcessStartInfo { FileName = "https://thesimscatalog.com/sims3/category/worlds/", UseShellExecute = true }); };
            link3.Content = "MOD THE SIMS";
            link3.Click += (s, e) => { Process.Start(new ProcessStartInfo { FileName = "https://modthesims.info/downloads/ts3/519/?showType=1&gs=2&p=1&threadcategory=Neighbourhoods+and+Worlds", UseShellExecute = true }); };
            link4.Content = "THE SIMS RESOURCE";
            link4.Click += (s, e) => { Process.Start(new ProcessStartInfo { FileName = "https://www.thesimsresource.com/downloads/browse/category/sims3-worlds/", UseShellExecute = true }); };
            link5.Content = "DREAM LAUNCHER REPOSITORY";
            link5.Click += (s, e) => { Process.Start(new ProcessStartInfo { FileName = "https://github.com/marcos4503/ts3-dream-launcher", UseShellExecute = true }); };
        }
    }
}
