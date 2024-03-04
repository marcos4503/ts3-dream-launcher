using s3pi.Interfaces;
using s3pi.Package;
using s3pi.WrapperDealer;
using System;
using System.Text;

namespace Dl3DxOverlay
{
    /*
     * This is the code responsible by the working of the Dream Launcher CASPS Editor on
     * Command Line Interface - CLI.
    */

    public static class Program
    {
        //Private variables
        private static string[] catchedArguments = null;
        private static IPackage currentPackage = null;

        //Core methods

        [STAThread]
        public static void Main(string[] arguments)
        {
            //Store all the arguments
            catchedArguments = arguments;

            //If don't have the keyword argument, cancel
            if (arguments.Length == 0 || arguments[0] != "dl3")
            {
                Console.WriteLine("The CASPs Editor needs to be runned by the Dream Launcher Mods Manager or by CLI using correct arguments.");
                Console.WriteLine("Press ENTER or CTRL + C to continue...");
                Console.ReadLine();
                return;
            }

            //Go to command receiver
            ReadCommands();
        }

        private static void ReadCommands()
        {
            //Show the input symbol
            Console.Write(">");

            //Warn that is waiting
            string[] typedContent = Console.ReadLine().Split(new[] { ' ' }, 2);
            string typedCommand = typedContent[0].ToLower();
            string typedArguments = (typedContent.Length > 1 ? typedContent[1] : "");

            //Read the command (supported format: //command arg1:"value" arg2:"value" arg3:"value"//)
            switch (typedCommand)
            {
                case "open":
                    OpenPackage(typedArguments);
                    break;
                case "list":
                    ListPackages(typedArguments);
                    break;
                case "update":
                    UpdateCASPParameters(typedArguments);
                    break;
                case "save":
                    SavePackage(typedArguments);
                    break;
                case "close":
                    ClosePackage(typedArguments);
                    break;
                case "pclear":
                    Clear(typedArguments);
                    break;
                case "pexit":
                    Exit(typedArguments);
                    break;
                default:
                    Console.WriteLine("Command not recognized.");
                    break;
            }

            //Go back to command read
            ReadCommands();
        }

        private static void OpenPackage(string arguments)
        {
            //Check if file exists
            if (File.Exists(GetArgument(arguments, "path").WithoutQuotes()) == false)
            {
                Console.WriteLine("<File not found.");
                return;
            }
            //Check if already have a opened package
            if (currentPackage != null)
            {
                Console.WriteLine("<Already have a opened package.");
                return;
            }





            //Wait time
            Thread.Sleep(500);

            //Open the package
            currentPackage = Package.OpenPackage(0, GetArgument(arguments, "path").WithoutQuotes(), true);
            //Inform success
            Console.WriteLine("<Package opened.");
        }

        private static void ListPackages(string arguments)
        {
            //Check if cache path exists
            if(GetArgument(arguments, "cachepath") != "")
                if(Directory.Exists(GetArgument(arguments, "cachepath")) == false)
                {
                    Console.WriteLine("<Cache path not found.");
                    return;
                }
            //Check if have a opened package
            if (currentPackage == null)
            {
                Console.WriteLine("<No package opened.");
                return;
            }





            //Wait time
            Thread.Sleep(500);

            //Get the cache path
            string cachePath = GetArgument(arguments, "cachepath").WithoutQuotes();

            //If have a cache path
            if(cachePath != "")
            {
                //Delete the cache folder if exists
                if (Directory.Exists(cachePath) == true)
                    Directory.Delete(cachePath, true);

                //Re-create the cache folder
                Directory.CreateDirectory(cachePath);
            }

            //Read all resources
            foreach (IResourceIndexEntry item in currentPackage.GetResourceList)
            {
                //If is a THUM resource and cache path is enabled
                if(cachePath != "")
                    if (isThumbResource(GetLongConvertedToHexStr(item.ResourceType, 8)) == true)
                    {
                        //Build the name for file to save
                        string fileName = (cachePath + ("/THUM_" + GetLongConvertedToHexStr(item.Instance, 16) + ".png"));
                        //Save it to cache
                        IResource resource = WrapperDealer.GetResource(0, currentPackage, item, true);
                        Stream targetStream = resource.Stream;
                        targetStream.Position = 0;
                        if (targetStream.Length == item.Memsize)
                        {
                            BinaryWriter writer = new BinaryWriter(new FileStream(fileName, FileMode.Create));
                            writer.Write((new BinaryReader(targetStream)).ReadBytes((int)targetStream.Length));
                            writer.Close();
                        }
                        targetStream.Dispose();
                        targetStream.Close();
                    }

                //If is a ICON resource and cache path is enabled
                if (cachePath != "")
                    if (isIconResource(GetLongConvertedToHexStr(item.ResourceType, 8)) == true)
                    {
                        //Build the name for file to save
                        string fileName = (cachePath + ("/ICON_" + GetLongConvertedToHexStr(item.Instance, 16) + ".png"));
                        //Save it to cache
                        IResource resource = WrapperDealer.GetResource(0, currentPackage, item, true);
                        Stream targetStream = resource.Stream;
                        targetStream.Position = 0;
                        if (targetStream.Length == item.Memsize)
                        {
                            BinaryWriter writer = new BinaryWriter(new FileStream(fileName, FileMode.Create));
                            writer.Write((new BinaryReader(targetStream)).ReadBytes((int)targetStream.Length));
                            writer.Close();
                        }
                        targetStream.Dispose();
                        targetStream.Close();
                    }

                //If is a CASP resource
                if (GetLongConvertedToHexStr(item.ResourceType, 8) == "0x034AEECB")
                {
                    //Prepare the informations
                    StringBuilder stringBuilder = new StringBuilder();

                    //Append all data
                    stringBuilder.Append(("instanceStr:" + GetLongConvertedToHexStr(item.Instance, 16) + ","));
                    stringBuilder.Append(("instanceUlong:" + item.Instance + ","));

                    //Collect CAS Part info
                    Stream sourceStream = WrapperDealer.GetResource(0, currentPackage, item, true).Stream;
                    CASPartResource.CASPartResource sourceCaspart = new CASPartResource.CASPartResource(1, sourceStream);

                    //Continue to append all data
                    stringBuilder.Append(("forToddler:" + sourceCaspart.AgeGender.Age.HasFlag(CASPartResource.AgeFlags.Toddler) + ","));
                    stringBuilder.Append(("forChild:" + sourceCaspart.AgeGender.Age.HasFlag(CASPartResource.AgeFlags.Child) + ","));
                    stringBuilder.Append(("forTeen:" + sourceCaspart.AgeGender.Age.HasFlag(CASPartResource.AgeFlags.Teen) + ","));
                    stringBuilder.Append(("forYoungAdult:" + sourceCaspart.AgeGender.Age.HasFlag(CASPartResource.AgeFlags.YoungAdult) + ","));
                    stringBuilder.Append(("forAdult:" + sourceCaspart.AgeGender.Age.HasFlag(CASPartResource.AgeFlags.Adult) + ","));
                    stringBuilder.Append(("forElder:" + sourceCaspart.AgeGender.Age.HasFlag(CASPartResource.AgeFlags.Elder) + ","));
                    stringBuilder.Append(("forMale:" + sourceCaspart.AgeGender.Gender.HasFlag(CASPartResource.GenderFlags.Male) + ","));
                    stringBuilder.Append(("forFemale:" + sourceCaspart.AgeGender.Gender.HasFlag(CASPartResource.GenderFlags.Female) + ","));
                    stringBuilder.Append(("validForRandom:" + sourceCaspart.ClothingCategory.HasFlag(CASPartResource.ClothingCategoryFlags.ValidForRandom) + ","));
                    stringBuilder.Append(("validForMaternity:" + sourceCaspart.ClothingCategory.HasFlag(CASPartResource.ClothingCategoryFlags.ValidForMaternity) + ","));

                    //Close streams
                    sourceStream.Dispose();
                    sourceStream.Close();

                    //List this CASP in the console
                    Console.WriteLine(stringBuilder.ToString());
                }
            }

            //Wait time
            Thread.Sleep(250);

            //Inform success
            Console.WriteLine("<Listing finished.");
        }

        private static void UpdateCASPParameters(string arguments)
        {
            //Check if have a opened package
            if (currentPackage == null)
            {
                Console.WriteLine("<No package opened.");
                return;
            }





            //Wait time
            Thread.Sleep(500);

            //Get the parameters
            string caspInstanceHexToEdit = GetArgument(arguments, "instance");
            string validForRandom = GetArgument(arguments, "validforrandom").ToLower();

            //Prepare the store for resource
            IResourceIndexEntry desiredEntry = null;
            //Try to found a correspondent resource
            foreach (IResourceIndexEntry item in currentPackage.GetResourceList)
                if(GetLongConvertedToHexStr(item.ResourceType, 8) == "0x034AEECB")
                    if (GetLongConvertedToHexStr(item.Instance, 16) == caspInstanceHexToEdit)
                    {
                        desiredEntry = item;
                        break;
                    }

            //If not found a resource, cancel
            if (desiredEntry == null)
            {
                Console.WriteLine("<Resource not found.");
                return;
            }

            //Load the resource
            Stream sourceStream = WrapperDealer.GetResource(1, currentPackage, desiredEntry, true).Stream;
            CASPartResource.CASPartResource sourceCaspart = new CASPartResource.CASPartResource(1, sourceStream);

            //Apply changes
            if(validForRandom == "true" || validForRandom == "false")
            {
                if (validForRandom == "true")
                    sourceCaspart.ClothingCategory |= CASPartResource.ClothingCategoryFlags.ValidForRandom;
                if (validForRandom == "false")
                    sourceCaspart.ClothingCategory &= ~CASPartResource.ClothingCategoryFlags.ValidForRandom;
            }

            //Delete the old resource
            currentPackage.DeleteResource(desiredEntry);

            //Add the new modified resource
            currentPackage.AddResource(((IResourceKey)desiredEntry), ((AResource)sourceCaspart).Stream, true);

            //Release streams
            sourceStream.Dispose();
            sourceStream.Close();
            ((AResource)sourceCaspart).Stream.Dispose();
            ((AResource)sourceCaspart).Stream.Close();

            //Inform success
            Console.WriteLine("<CASP parameters updated.");
        }

        private static void SavePackage(string arguments)
        {
            //Check if have a opened package
            if (currentPackage == null)
            {
                Console.WriteLine("<No package opened.");
                return;
            }





            //Wait time
            Thread.Sleep(500);

            //Save the package
            currentPackage.SavePackage();

            //Inform success
            Console.WriteLine("<Package saved.");
        }

        private static void ClosePackage(string arguments)
        {
            //Check if have a opened package
            if (currentPackage == null)
            {
                Console.WriteLine("<No package opened.");
                return;
            }





            //Wait time
            Thread.Sleep(500);

            //Close the package
            Package.ClosePackage(0, currentPackage);
            currentPackage = null;

            //Inform success
            Console.WriteLine("<Package closed.");
        }

        private static void Clear(string arguments)
        {
            //Clear the lines
            Console.Clear();
        }

        private static void Exit(string arguments)
        {
            //Exit the program
            Environment.Exit(0);
        }

        //Auxiliar methods

        private static string GetArgument(string sourceArguments, string desiredKey)
        {
            //Supported format for arguments: //arg1:"value" arg2:"value" arg3:"value"//

            //Prepare to return
            string toReturn = "";

            //Prepare the dictionary
            Dictionary<string, string> argumentsDictionary = new Dictionary<string, string>();

            //Split the arguments
            string[] arguments = sourceArguments.Split("\" ");

            //Add arguments to dictionary
            for(int i = 0; i < arguments.Length; i++)
            {
                //Get the key and value of arguments
                string[] argumentSplitted = arguments[i].Split(":\"");
                string key = argumentSplitted[0].ToLower();
                string value = ((argumentSplitted.Length > 1) ? argumentSplitted[1] : "");

                //If this is the last parameter, remove the " on the end of value
                if (i == (arguments.Length - 1))
                    if(value.Length > 0)
                        if (value[value.Length - 1] == '\"')
                            value = value.Remove(value.Length - 1);

                //If not exists in dictionary, add it
                if (argumentsDictionary.ContainsKey(key) == false)
                    argumentsDictionary.Add(key, value);
            }

            //If the key exists in arguments, get the value
            if (argumentsDictionary.ContainsKey(desiredKey.ToLower()) == true)
                toReturn = argumentsDictionary[desiredKey.ToLower()];

            //Return the result
            return toReturn;
        }

        private static string GetLongConvertedToHexStr(ulong originalValue, int digits)
        {
            //Prepare the result
            string result = "0x";

            //Convert long to hex string
            string tmpHex = originalValue.ToString("X");

            //Add the digits if necessary
            while (tmpHex.Length < digits)
                tmpHex = "0" + tmpHex;

            //Concat the values
            result += tmpHex;

            //Return the result
            return result;
        }

        private static bool isThumbResource(string resourceTypeHex)
        {
            //Prepare the value to return
            bool toReturn = false;

            //Build a array with valid THUM resouce type hex
            string[] validList = new string[25];
            validList[0] = "0x0580A2B4";
            validList[1] = "0x0580A2B5";
            validList[2] = "0x0580A2B6";
            validList[3] = "0x0589DC44";
            validList[4] = "0x0589DC45";
            validList[5] = "0x0589DC46";
            validList[6] = "0x05B17698";
            validList[7] = "0x05B17699";
            validList[8] = "0x05B1769A";
            validList[9] = "0x05B1B524";
            validList[10] = "0x05B1B525";
            validList[11] = "0x05B1B526";
            validList[12] = "0x2653E3C8";
            validList[13] = "0x2653E3C9";
            validList[14] = "0x2653E3CA";
            validList[15] = "0x2D4284F0";
            validList[16] = "0x2D4284F1";
            validList[17] = "0x2D4284F2";
            validList[18] = "0x5DE9DBA0";
            validList[19] = "0x5DE9DBA1";
            validList[20] = "0x5DE9DBA2";
            validList[21] = "0x626F60CC";
            validList[22] = "0x626F60CD";
            validList[23] = "0x626F60CE";
            validList[24] = "0xFCEAB65B";

            //Check if value exists in the array
            for (int i = 0; i < validList.Length; i++)
                if (validList[i] == resourceTypeHex)
                {
                    toReturn = true;
                    break;
                }

            //Return the value
            return toReturn;
        }

        private static bool isIconResource(string resourceTypeHex)
        {
            //Prepare the value to return
            bool toReturn = false;

            //Build a array with valid ICON resouce type hex
            string[] validList = new string[7];
            validList[0] = "0x2E75C764";
            validList[1] = "0x2E75C765";
            validList[2] = "0x2E75C766";
            validList[3] = "0x2E75C767";
            validList[4] = "0xD84E7FC5";
            validList[5] = "0xD84E7FC6";
            validList[6] = "0xD84E7FC7";

            //Check if value exists in the array
            for (int i = 0; i < validList.Length; i++)
                if (validList[i] == resourceTypeHex)
                {
                    toReturn = true;
                    break;
                }

            //Return the value
            return toReturn;
        }

        //Extension methods

        private static string WithoutQuotes(this string source)
        {
            //Remove the quotes
            return source.Replace("\"", "").Replace("'", "");
        }
    }
}