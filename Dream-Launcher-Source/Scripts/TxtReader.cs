using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS3_Dream_Launcher.Scripts
{
    /*
     * This script makes reading and writing of TXT files of Smooth Patcher.
    */

    public class TxtReader
    {
        //Private variables
        private string openedFilePath = "";
        private string[] targetTxtLoadedLines = null;

        //Core methods

        public TxtReader(string txtFilePath)
        {
            //Store the opened file path
            openedFilePath = txtFilePath;

            //Read all lines of the target txt file
            targetTxtLoadedLines = File.ReadAllLines(txtFilePath);
        }

        //Public methods

        public void UpdateValue(int lineNumber, string key, string value)
        {
            //Update the value in the given line
            targetTxtLoadedLines[lineNumber] = (key + " = " + value);
        }

        public void Save()
        {
            //Save all edited lines
            File.WriteAllLines(openedFilePath, targetTxtLoadedLines);
        }
    }
}