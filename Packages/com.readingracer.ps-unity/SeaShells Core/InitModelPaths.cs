/*
 * Copyright (c) 2025 Reading Racer Technology Foundation
 *
 * This file is part of ReadingRacerTechnologyFoundation/PocketSphinxForUnity
 *
 * SPDX-License-Identifier: LGPL-3.0-or-later
 *
 * This software is distributed without any warranty.
 * See the LICENSE file in the project root for full terms.
 *
 * This source may contain or make use of third-party components,
 * including PocketSphinx and SphinxBase, which are licensed separately.
 * See THIRD_PARTY_LICENSES.txt for details.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;

namespace Rrtf
{
    /// <summary>
    /// This is a stupid class that only exists because android is stupid.
    /// Since Android needs to use www for retrieving things from the streamingassets folder
    /// but pocketsphinx needs a real folder this will copy the data to a temp data path when on android
    /// if we need it to. then modify the data path accordingly.
    /// 
    /// IMPORTANT: make a scene with this object is loaded BEFORE any scene with speech recognition.-
    /// 
    /// How To Use: Let the awake be called before anything that uses this class. Then you are free to choose
    /// the AMChoice. When a SeashellsRecognizer is created. It will automatically load the correct model path based
    /// on the AMChoice.
    /// </summary>
    public class InitModelPaths : MonoBehaviour
    {

        public enum ACCOUSTIC_MODELS { ADULT = 0, CHILD = 1 }

        //returns the LM weight depending on device and current
        //AM choice
        public static int DefaultLanguageModelWeight
        {
            get
            {

#if !UNITY_EDITOR && UNITY_ANDROID
			// 2015-10-22 Greg: on Android let's try language model weight one step lower to reduce false positives (mostly hallucinations)
			if(AMChoice == ACCOUSTIC_MODELS.ADULT) return 0;
			//must be child
			if(IsContextDependent) return 5;
			else return 5;
#else
                if (AMChoice == ACCOUSTIC_MODELS.ADULT) return 1;
                //must be child
                else return 5;
#endif
            }
        }

        //change this in the static constructor down below for testing purposes within unity
        //accoustic model choice.
        public static ACCOUSTIC_MODELS AMChoice
        {
            get { return _amchoice; }
            set
            {
                //Debug.Log("Changed AM CHOICE: " + value.ToString() + " which is actually a reference to " + CHILD_AM_FOLDER);
                _amchoice = value;
                updateAMFolder();
            }
        }
        private static ACCOUSTIC_MODELS _amchoice;
        public static bool IsDoneFixingPaths { get; private set; }

        static InitModelPaths()
        {
            //assumed to be in the streaming assets folder. Must be in the streaming assets folder.
            modelFolder = Path.Combine("ps-unity-modeldata", "ps_all_english");
            //assumed to be in the model folder
            dictionaryPath = "lm/default_dictionary.dic";

            // 2015-05-14 change initial acoustic models to kid in order to see if the mic test bug switches to appearing on grownup models
            // AMChoice = ACCOUSTIC_MODELS.CHILD_CI; 
            AMChoice = ACCOUSTIC_MODELS.ADULT;
            IsDoneFixingPaths = false;
        }

        //ASSUMED TO BE IN THE MODELS FOLDER!!!
        private const string ADULT_AM_FOLDER = "hmm/en-us-semi";
        public static string CHILD_AM_FOLDER = "hmm/rrtraining_mmie01.ci_cont";

        public static string ModelFolder
        {
            get
            {
                if (arePathsFixed) return modelFolder;
                else
                {
#if UNITY_EDITOR
                    if (Application.isPlaying)
                    {
                        return Path.Combine(Application.streamingAssetsPath, modelFolder);
                    }
                    else
                    {
                        //won't ever happen since this isn't used outside of editor playing. 
                        //used to fix compilation warnings and error from unity
                        Debug.LogError("EMpty ModelFolder. should not happen");
                        return string.Empty;
                    }
#else
                return Path.Combine(Application.streamingAssetsPath, modelFolder);
#endif
                }
            }
        }
        public static string DictionaryPath
        {
            get
            {
                if (arePathsFixed) return dictionaryPath;
                else return Path.Combine(ModelFolder, dictionaryPath);
            }
        }
        public static string AccousticModelFolder
        {
            get
            {
                if (arePathsFixed)
                {
                    return accousticModelFolder;
                }
                else return Path.Combine(ModelFolder, accousticModelFolder);
            }
        }

        //how much time do we wait before we load
        [Tooltip("if you want to add a load delay")]
        public float MinLoadDelay = 0;

        private static string modelFolder;//assumed to be in the streaming assets folder
        private static string dictionaryPath;//assumed to be in the modelFolde

        //updated automaticlly when am choice is changed
        private static string accousticModelFolder; //assumed to be in the modelFolder

        /// <summary>
        /// The are paths fixed. If the paths are fixed then there is no need for the accessors to fix them.
        /// </summary>
        private static bool arePathsFixed = false;

        /// <summary>
        /// The accoustic model file list. The name of the txt that holdes the 
        /// name of the files in the accoustic model folder. 
        /// </summary>
        private static string accousticModelFileList = "AccousticModelFileListForAndroid";

        private static string adultModelFileList = "AdultModelFileList";

        private static string childModelFileList = "ChildeModelFileList";
        private float progress = 0f;

        // Use this for initialization
        void Awake()
        {
            //why do i do this? Because Android is stupid. I need to know the folder names and such when I run
            //the createModelCacheForAndroid coroutine. BUt it doesnt matter for the rest
#if UNITY_EDITOR || !UNITY_ANDROID
            StartCoroutine(createPathsForDevices());
#else
		StartCoroutine(createModelCacheForAndroid());
#endif
            arePathsFixed = true;
        }

        private IEnumerator createPathsForDevices()
        {
            IsDoneFixingPaths = false;
            modelFolder = Path.Combine(Application.streamingAssetsPath, modelFolder);
            dictionaryPath = Path.Combine(modelFolder, dictionaryPath);
            Debug.Log("am path: " + accousticModelFolder);
            accousticModelFolder = Path.Combine(modelFolder, accousticModelFolder);

            float timer = 0f;
            while (timer < MinLoadDelay)
            {
                timer += Time.deltaTime;
                progress = timer / MinLoadDelay;

                yield return null;
            }
            yield return null;

            IsDoneFixingPaths = true;
        }

        private string streamingassetspathtemp;
        private IEnumerator createModelCacheForAndroid()
        {
            IsDoneFixingPaths = false;

            //for the minimumLoadDelay
            float timeStart = Time.timeSinceLevelLoad;

            string localModelPath = Path.Combine(Application.temporaryCachePath, modelFolder);
            //		if(Directory.Exists(localModelPath))
            //			Directory.Delete(localModelPath,true);

            /*********DICTIONARY********/
            //copy and update the accoustic model
            UnityWebRequest wwwDict = UnityWebRequest.Get(Path.Combine(
                Path.Combine(Application.streamingAssetsPath, modelFolder),
                dictionaryPath));
            dictionaryPath = Path.Combine(localModelPath, dictionaryPath);

            if (!(new FileInfo(dictionaryPath)).Directory.Exists)
                (new FileInfo(dictionaryPath)).Directory.Create();

            if (!(new FileInfo(dictionaryPath)).Exists)
            {
                yield return wwwDict.SendWebRequest();
                if (wwwDict.result == UnityWebRequest.Result.Success)
                {
                    File.WriteAllBytes(dictionaryPath, wwwDict.downloadHandler.data);
                    Debug.Log("Created: " + dictionaryPath);
                }
                else
                {
                    Debug.LogError("failed to write dictionary: " + dictionaryPath);
                }
            }
            /********Dictionary******/


            //write all the accousticModelFiles but first create the directory
            string localAccousticFolder = Path.Combine(localModelPath, accousticModelFolder);
            if (!(new DirectoryInfo(localAccousticFolder)).Exists)
            {
                (new DirectoryInfo(localAccousticFolder)).Create();
                Debug.Log("localAccousticFolder: " + localAccousticFolder);
            }

            string streamingAMFolder = Path.Combine(Application.streamingAssetsPath, modelFolder);
            streamingassetspathtemp = streamingAMFolder;
            streamingAMFolder = Path.Combine(streamingAMFolder, accousticModelFolder);
            Debug.Log("streaming AM Folder: " + streamingAMFolder);

            string[] fileNames = ((TextAsset)Resources.Load(accousticModelFileList)).text.
                Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);
            Debug.Log("Retrieved filenames, length = " + fileNames.Length);

            foreach (string file in fileNames)
            {
                string localFile = Path.Combine(localAccousticFolder, file);
                if ((new FileInfo(localFile)).Exists) continue;

                UnityWebRequest folderEntries = UnityWebRequest.Get(Path.Combine(streamingAMFolder, file));

                yield return folderEntries.SendWebRequest();
                if (folderEntries.result == UnityWebRequest.Result.Success)
                {
                    File.WriteAllBytes(localFile, folderEntries.downloadHandler.data);
                    Debug.Log("Created: " + Path.Combine(localAccousticFolder, file));
                }
                else
                {
                    Debug.LogError("failure to create: " + Path.Combine(localAccousticFolder, file));
                }
            }

            modelFolder = localModelPath;
            accousticModelFolder = localAccousticFolder;
            //dictionary is assogined above

            //****************creating the other am folders*************************

            yield return StartCoroutine(copyAMFiles(localModelPath, ADULT_AM_FOLDER, adultModelFileList));
            yield return StartCoroutine(copyAMFiles(localModelPath, CHILD_AM_FOLDER, childModelFileList));

            //*****************done creating the other AM folders*******************

            //checks to see if at least minloadDelay time has passed if it hasnt then we wait.
            //float timeElapsed = Time.timeSinceLevelLoad - timeStart;
            //yield return new WaitForSeconds(Mathf.Max(0, MinLoadDelay - timeElapsed));


            float timer = Time.timeSinceLevelLoad - timeStart;
            while (timer < MinLoadDelay)
            {
                timer += Time.deltaTime;
                progress = timer / MinLoadDelay;

                yield return null;
            }
            yield return new WaitForSeconds(.25f);

            IsDoneFixingPaths = true;
        }

        private IEnumerator copyAMFiles(string localModelPath, string folderName, string fileListtxt)
        {
            string localFolder = Path.Combine(localModelPath, folderName);

            if (!(new DirectoryInfo(localFolder)).Exists)
                (new DirectoryInfo(localFolder)).Create();

            //string streamingFolder = Path.Combine(Application.streamingAssetsPath,modelFolder);
            string streamingFolder = streamingassetspathtemp;
            streamingFolder = Path.Combine(streamingFolder, folderName);
            Debug.Log("streaming folder name: " + streamingFolder);
            string[] fileNames = ((TextAsset)Resources.Load(fileListtxt)).text.
                Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);
            Debug.Log("Retrieved filenames, length = " + fileNames.Length);

            foreach (string file in fileNames)
            {
                string destPath = Path.Combine(localFolder, file);
                if ((new FileInfo(destPath).Exists)) continue;

                UnityWebRequest folderEntries = UnityWebRequest.Get(Path.Combine(streamingFolder, file));
                yield return folderEntries.SendWebRequest();
                if (folderEntries.result == UnityWebRequest.Result.Success)
                {
                    File.WriteAllBytes(destPath, folderEntries.downloadHandler.data);
                }
                else
                {
                    Debug.LogError("failed: " + destPath);
                }
            }

            yield return null;
        }

        private static void updateAMFolder()
        {
            if (arePathsFixed)
            {
                if (_amchoice == ACCOUSTIC_MODELS.ADULT)
                    accousticModelFolder = Path.Combine(modelFolder, InitModelPaths.ADULT_AM_FOLDER);
                else
                    accousticModelFolder = Path.Combine(modelFolder, InitModelPaths.CHILD_AM_FOLDER);
            }
            else
            {
                if (_amchoice == ACCOUSTIC_MODELS.ADULT)
                    InitModelPaths.accousticModelFolder = InitModelPaths.ADULT_AM_FOLDER;
                else
                    InitModelPaths.accousticModelFolder = InitModelPaths.CHILD_AM_FOLDER;
            }
        }

        //the accousticmodel folder is assumed to be in the modelFolder
        private static bool createFolderListTxt(string folderName, string txtName)
        {
            string localModelFolder = Application.dataPath + "/StreamingAssets/" + modelFolder + folderName;
            if (!Directory.Exists(localModelFolder))
            {
                Debug.LogError("CRITICAL ERROR: " + localModelFolder + " does not exist. folder txt will not be updated");
                return false;
            }

            //get all the file names in a single string
            string[] files = Directory.GetFiles(localModelFolder);
            List<string> fileNames = new List<string>();
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo info = new FileInfo(files[i]);
                if (info.Extension != ".meta")
                    fileNames.Add(info.Name);
            }

            //write to accousticModelFileList file txt
            File.WriteAllLines(Application.dataPath + "/Resources/" + txtName + ".txt", fileNames.ToArray());

            return true;
        }

        /// <summary>
        /// creates a txt file containing all the files in the resources txt;
        /// </summary>
#if UNITY_EDITOR
        [UnityEditor.MenuItem("RRTF/Create LM File List")]
#endif
        public static bool createFileListTxt()
        {
            return createFolderListTxt(accousticModelFolder, accousticModelFileList) &&
                createFolderListTxt(ADULT_AM_FOLDER, adultModelFileList) &&
                createFolderListTxt(CHILD_AM_FOLDER, childModelFileList);
        }
    }
}