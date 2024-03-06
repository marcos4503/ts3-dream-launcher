using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS3_Dream_Launcher.Scripts
{
    /*
     * This class manage the load and save of a world info file
    */

    public class WorldInfo
    {
        //Classes of script
        public class LoadedData
        {
            //*** Data to be saved ***//

            public string[] files = new string[0];
        }

        //Private variables
        private string saveFilePath = "";

        //Public variables
        public LoadedData loadedData = null;

        //Core methods

        public WorldInfo(string filePath)
        {
            //Store the save file path
            this.saveFilePath = filePath;

            //Check if save file exists
            bool saveExists = File.Exists(filePath);

            //If have a save file, load it
            if (saveExists == true)
                Load();
            //If a save file don't exists, create it
            if (saveExists == false)
                Save();
        }

        private void Load()
        {
            //Load the data
            string loadedDataString = File.ReadAllText(saveFilePath);

            //Convert it to a loaded data object
            loadedData = JsonConvert.DeserializeObject<LoadedData>(loadedDataString);
        }

        //Public methods

        public void Save()
        {
            //If the loaded data is null, create one
            if (loadedData == null)
                loadedData = new LoadedData();

            //Save the data
            File.WriteAllText(saveFilePath, JsonConvert.SerializeObject(loadedData));

            //Load the data to update loaded data
            Load();
        }
    }
}
