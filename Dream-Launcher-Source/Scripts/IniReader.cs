using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS3_Dream_Launcher.Scripts
{
    /*
     * This script makes reading and writing of INI files.
    */

    public class IniReader
    {
        //Private variables
        private string openedFilePath = "";
        private string[] targetIniLoadedLines = null;

        //Core methods

        public IniReader(string iniFilePath)
        {
            //Store the opened file path
            openedFilePath = iniFilePath;

            //Read all lines of the target ini file
            targetIniLoadedLines = File.ReadAllLines(iniFilePath);
        }
    
        //Public methods

        public void UpdateValue(string key, string value)
        {
            //Try to find the key and edit his value, if found the key
            for(int i = 0; i < targetIniLoadedLines.Length; i++)
                if (targetIniLoadedLines[i].Split("=")[0].Replace(" ", "").ToLower() == key.ToLower())
                {
                    //Get the value
                    string rawValue = targetIniLoadedLines[i];

                    //Separate the values
                    string splittedKey = rawValue.Split("=")[0].Replace(" ", "");
                    string splittedValue = "";

                    //Change the value
                    splittedValue = value;

                    //Set the value
                    targetIniLoadedLines[i] = (splittedKey + " = " + splittedValue);

                    //Stop the loop
                    break;
                }  
        }

        public string GetValue(string key)
        {
            //Prepare to return
            string toReturn = "";

            //Try to get the value from the loaded INI file
            for (int i = 0; i < targetIniLoadedLines.Length; i++)
                if (targetIniLoadedLines[i].Split("=")[0].Replace(" ", "").ToLower() == key.ToLower())
                {
                    //Get the value
                    toReturn = targetIniLoadedLines[i].Replace(" ", "").Replace("=", "").Replace(key, "");

                    //Stop the loop
                    break;
                }

            //Return the value
            return toReturn;
        }

        public void Save()
        {
            //Save all edited lines
            File.WriteAllLines(openedFilePath, targetIniLoadedLines);
        }
    }
}