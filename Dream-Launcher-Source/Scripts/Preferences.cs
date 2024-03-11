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

            public SaveInfo[] saveInfo = new SaveInfo[0];

            public string launcherLang = "undefined";
            public string gameLang = "undefined";
            public int gamePriority = 0;
            public int launcherBehaviour = 0;
            public int gameOverlay = 1;
            public bool alreadyTranslated = false;
            public bool alreadyIntelFixed = false;
            public int lastErrorLogsCount = 0;
            public int lastMediaFilesCount = 0;
            public int lastExportFilesCount = 0;
            public int lastSaveFilesCount = 0;

            public bool patchModsSupport = false;
            public bool patchBasicOptimization = false;
            public bool patchFpsLimiter = false;
            public bool patchCpuGpuUpdate = false;
            public bool patchBetterGlobalIllumination = false;
            public bool patchImprovedShaders = false;
            public bool patchShadowExtender = false;
            public bool patchRoutingOptimizations = false;
            public bool patchStoryProgression = false;
            public bool patchInternetRemoval = false;

            public int resolution = 0;
            public int refreshRate = 1;
            public int maxFps = 0;
            public int displayMode = 0;
            public int maxTps = 0;
            public bool debugSmoothPatch = false;
            public bool objectHiding = false;
            public bool animationSmoothing = true;
            public bool advancedRendering = true;
            public int highDetailLots = 0;
            public int reflectionQuality = 0;
            public int antiAliasing = 0;
            public int drawDistance = 0;
            public int visualEffects = 0;
            public int lightShadows = 0;
            public int texturesQuality = 0;
            public int treeDetails = 0;
            public int simsDetails = 0;
            public int speakerSetup = 0;
            public bool defocusMute = false;
            public float voicesVolume = 1.0f;
            public float fxVolume = 1.0f;
            public float musicVolume = 1.0f;
            public float ambientVolume = 1.0f;
            public int audioQuality = 2;
            public int onlineNotifications = 0;
            public int shopMode = 0;
            public bool edgeScrolling = false;
            public int clockFormat = 0;
            public bool lessons = true;
            public bool interactiveLoading = true;
            public bool invertCamH = false;
            public bool invertCamV = false;
            public int memories = 0;
            public int simsLifespan = 2;
            public bool aging = true;
            public bool suppressOpportunities = false;
            public bool noAutonomyActiveSim = false;
            public int simsAutonomyLevel = 2;
            public int petsAutonomyLevel = 2;
            public int summerSeason = 1;
            public int fallSeason = 1;
            public int winterSeason = 1;
            public int springSeason = 1;
            public int temperatureUnit = 0;
            public bool hailWeather = true;
            public bool rainWeather = true;
            public bool snowWeather = true;
            public bool fogWeather = true;
            public int lunarCycle = 2;
            public int storyProgression = 1;
            public bool allowVampires = true;
            public bool allowWerewolves = true;
            public bool allowPets = true;
            public bool allowCelebrities = true;
            public bool allowFairies = true;
            public bool allowWitches = true;
            public bool allowHorses = true;
            public int disableCelebritiesSystem = 0;

            public bool saveCleanSnapsThumbs = true;
            public bool saveCleanMemsPhotoPaint = true;
            public bool saveCleanSeasonCards = true;
            public bool saveCleanPromPhotos = true;
            public bool saveCleanCabinPhotos = true;
            public bool saveCleanWorldShangSimla = false;
            public bool saveCleanWorldChampsLesSims = false;
            public bool saveCleanWorldAlSimhara = false;
            public bool saveCleanWorldSimsUniversity = false;
            public bool saveCleanWorldOasisLanding = false;
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

    /*
     * Auxiliar classes
     * 
     * Classes that are objects that will be used, only to organize data inside 
     * "LoadedData" object in the saves.
    */

    public class SaveInfo
    {
        public string key = "";
        public string value = "";
    }
}
