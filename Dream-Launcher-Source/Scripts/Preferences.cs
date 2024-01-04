using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS3_Dream_Launcher.Scripts
{
    /*
     * This class manage the load and save of launcher settings
    */

    public class Preferences
    {
        //Classes of script
        public class LoadedData
        {
            //*** Data to be saved ***//

            public string launcherLang = "undefined";
            public bool alreadyTranslated = false;
            public bool alreadyIntelFixed = false;
        }

        //Public variables
        public LoadedData loadedData = null;

        //Core methods

        public Preferences()
        {
            //Check if save file exists
            bool saveExists = File.Exists((Directory.GetCurrentDirectory() + @"/Content/prefs.json"));

            //If have a save file, load it
            if(saveExists == true)
                Load();
            //If a save file don't exists, create it
            if (saveExists == false)
                Save();
        }

        private void Load()
        {
            //Load the data
            string loadedDataString = File.ReadAllText((Directory.GetCurrentDirectory() + @"/Content/prefs.json"));

            //Convert it to a loaded data object
            loadedData = JsonConvert.DeserializeObject<LoadedData>(loadedDataString);
        }

        //Public methods

        public void Save()
        {
            //If the loaded data is null, create one
            if(loadedData == null)
                loadedData = new LoadedData();

            //Save the data
            File.WriteAllText((Directory.GetCurrentDirectory() + @"/Content/prefs.json"), JsonConvert.SerializeObject(loadedData));

            //Load the data to update loaded data
            Load();
        }
    }
}
