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
    * This class reads the file of Recommended Mods Library
    */

    public class RecommendedListReader
    {
        //Classes of script
        public class LoadedData
        {
            //*** Data to be saved ***//

            public Mod[] modsList = new Mod[0];
        }

        //Public variables
        public LoadedData loadedData = null;

        //Core methods

        public RecommendedListReader(string recommendedModsListPath)
        {
            //If the file don't exists, don't load
            if(File.Exists(recommendedModsListPath) == false)
            {
                loadedData = new LoadedData();
                return;
            }

            //Load the data
            string loadedDataString = File.ReadAllText(recommendedModsListPath);

            //Convert it to a loaded data object
            loadedData = JsonConvert.DeserializeObject<LoadedData>(loadedDataString);
        }
    }

    /*
     * Auxiliar classes
     * 
     * Classes that are objects that will be used, only to organize data inside 
     * "LoadedData" object in the saves.
    */

    public class Mod
    {
        public string name = "";
        public string category = "";
        public string author = "";
        public string description_enUS = "";
        public string description_ptBR = "";
        public string setupGuide_enUS = "";
        public string setupGuide_ptBR = "";
        public int[] requiredEps = new int[0];
        public string[] requiredRecommendedModFiles = new string[0];
        public string[] requiredPatchModFiles = new string[0];
        public string pageUrl = "";
        public string downloadUrl = "";
    }
}
