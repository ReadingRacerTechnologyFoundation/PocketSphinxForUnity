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
using System.IO;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;

namespace Rrtf
{
    /// <summary>
    /// Since Android needs to use www for retrieving things from the streamingassets folder
    /// but pocketsphinx needs a real folder this will copy the data to a temp data path when on android.
    /// 
    /// It also manages where the model paths are on device.
    /// 
    /// IMPORTANT: For ease of use, this is setup to work in any scene without having to make gameobject yourself.
    /// However, It is reccomemend that for builds, you have a loading scene with this attached to a gameobject.
    /// 
    /// To MODIFY paths: Resources/RRTF/ModelPaths.asset will need to be changed
    /// 
    /// How To Use: 
    /// 1. Drop this on a gameobject
    /// 2. Don't create a SpeechRecognizer/BasicFSGRecognizer until ArePathsFixed = true
    /// 
    /// This also supports two Accoustic model paths. Adult and Child. Adult is the default. If you switch AMChoice to child, the paths will also
    /// update themselves correctly.
    /// </summary>
    public class InitModelPaths : MonoBehaviour
    {
        /// <summary>
        /// the path to the resource this uses to determine what the local paths for the model files should be
        /// </summary>
        public const string ModelPathsResourcePath = "RRTF/ModelPaths";
        private static string modelFolder;
        private static string dictionaryPath;

        public enum ACCOUSTIC_MODELS { ADULT = 0, CHILD = 1 }

        /// <summary>
        /// Useful if you want to change the LM Weight depending on the use of the child or adult accoustic model
        /// </summary>
        public static int DefaultLanguageModelWeight
        {
            get
            {
                if (AMChoice == ACCOUSTIC_MODELS.ADULT) return 1;
                else return 5;
            }
        }

        /// <summary>
        /// Get or Set the Accoustic Model type. Adult or Child. Will update paths accordingly. ArePathsFixed must be true.
        /// </summary>
        public static ACCOUSTIC_MODELS AMChoice
        {
            get { return _amchoice; }
            set
            {
                _amchoice = value;
                UpdateAMFolder();
                Debug.Log("ps-unity. Updated AM Choice: " + accousticModelFolder);
            }
        }
        private static ACCOUSTIC_MODELS _amchoice = ACCOUSTIC_MODELS.ADULT;

        /// <summary>
        /// Will return true once paths are ready to use
        /// </summary>
        public static bool ArePathsFixed { get; private set; } = false;

        /// <summary>
        /// Absolute path to the ModelFolder
        /// </summary>
        public static string ModelFolder
        {
            get
            {
                if (!ArePathsFixed)
                {
                    Debug.LogError("ps-unity. Paths haven't been fixed. This will cause a crash in sphinx");
                }

                return modelFolder;
            }
        }

        /// <summary>
        /// Absolute path to the dectionary file
        /// </summary>
        public static string DictionaryPath
        {
            get
            {
                if (!ArePathsFixed)
                {
                    Debug.LogError("ps-unity. Paths haven't been fixed. This will cause a crash in sphinx");
                }

                return dictionaryPath;
            }
        }

        /// <summary>
        /// Absolute Path to the AccousticModel Folder
        /// </summary>
        public static string AccousticModelFolder
        {
            get
            {
                if (!ArePathsFixed)
                {
                    Debug.LogError("ps-unity. Paths haven't been fixed. This will cause a crash in sphinx");
                }

                return accousticModelFolder;
            }
        }

        //updated automaticlly when am choice is changed
        private static string accousticModelFolder; //assumed to be in the modelFolder
        private static ModelPaths _modelPathsResource;

        /// <summary>
        /// Only used on android. Will return true if currently running the path fixing coroutine
        /// </summary>
        public static bool IsCurrentlyUpdatingPaths { get; private set; } = false;

        /// <summary>
        /// If you are using android and want to know the progress of files being copied. you can subscribe to this action.
        /// </summary>
        public static Action<float> OnPathUpdateProgress;

        /// <summary>
        /// If you are using android and want to know when we have completed fixing paths, you can subscribe to this action.
        /// </summary>
        public static Action<bool> OnPathUpdateCompletion;


#if UNITY_EDITOR
        /// <summary>
        /// In order to avoid having this in every scene in editor, and having this out of the box in every scene in editor
        /// this will take care of that for editor only
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CorrectPathsForEditor()
        {
            if (!FindAnyObjectByType<InitModelPaths>())
            {
                SetupCorrectedAssetPaths(Application.streamingAssetsPath);
            }
        }
#endif
        //why do i do this? Because Android is stupid. I need to know the folder names and such when I run
        //the createModelCacheForAndroid coroutine. BUt it doesnt matter for the rest
#if UNITY_ANDROID && !UNITY_EDITOR
        async Task Start()
        {
            Debug.Log("ps-unity: Setting up Model Cache For Android");
            await CreateModelCacheForAndroid();
        }
#else
        void Start()
        {
            Debug.Log("ps-unity: Setting up Model Cache For windows/ios/etc");
            SetupCorrectedAssetPaths(Application.streamingAssetsPath);
        }
#endif

        private static void SetupCorrectedAssetPaths(string baseFolder)
        {
            if (_modelPathsResource == null)
            {
                _modelPathsResource = Resources.Load<ModelPaths>(ModelPathsResourcePath);
            }

            modelFolder = Path.Combine(baseFolder, _modelPathsResource.ModelFolderRoot);
            dictionaryPath = Path.Combine(modelFolder, _modelPathsResource.DictionarySubpath);
            UpdateAMFolder();
            Debug.Log("ps-unity: paths fixed using modelFolder: " + modelFolder);

            ArePathsFixed = true;
        }

        private async Task CreateModelCacheForAndroid()
        {
            if (ArePathsFixed || IsCurrentlyUpdatingPaths)
            {
                Debug.Log("ps-unity: earlyout1");
                OnPathUpdateCompletion?.Invoke(false);
                return;
            }

            IsCurrentlyUpdatingPaths = true;
            if (_modelPathsResource == null)
            {
                _modelPathsResource = Resources.Load<ModelPaths>(ModelPathsResourcePath);
            }
            await Extract(OnPathUpdateProgress, OnPathUpdateCompletion);

            IsCurrentlyUpdatingPaths = false;
            SetupCorrectedAssetPaths(Application.temporaryCachePath);
        }

        private static void UpdateAMFolder()
        {
            if (IsCurrentlyUpdatingPaths)
            {
                Debug.LogError("ps-unity: can't change accoustic model if android is currently copying folder data");
                return;
            }

            if (_modelPathsResource == null)
            {
                _modelPathsResource = Resources.Load<ModelPaths>(ModelPathsResourcePath);
            }

            if (_amchoice == ACCOUSTIC_MODELS.ADULT)
                accousticModelFolder = Path.Combine(modelFolder, _modelPathsResource.AdultAccousticModelSubpath);
            else
                accousticModelFolder = Path.Combine(modelFolder, _modelPathsResource.ChildAccousticModelSubpath);
        }

        /// <summary>
        /// Extracts the files from the streaming assets zip indicated in _srcFolder and puts them into a temp folder indicated by TargetDirectory
        /// </summary>
        /// <param name="progress">Callback to indicate progress via a 0-100 number</param>
        /// <param name="onComplete">Callback for when extraction has fully completed. Will set true if successfull</param>
        /// <returns>true if successful, otherwise false</returns>
        private async Task<bool> Extract(Action<float> progress, Action<bool> onComplete)
        {
            Debug.Log("ps-unity: extract");
            string srcRoot = Path.Combine(Application.streamingAssetsPath, _modelPathsResource.ModelFolderRoot);
            string dstRoot = Path.Combine(Application.temporaryCachePath, _modelPathsResource.ModelFolderRoot);
            Debug.Log("ps-unity dstRoot: " + dstRoot);
            float totalCopies = _modelPathsResource.Files.Length;
            int copyCount = 0;

            //create new directories. easy
            foreach(string dir in _modelPathsResource.Directories)
            {
                string fullDir = Path.Combine(dstRoot, dir);
                Debug.Log("ps-unity dir create: " + fullDir);
                Directory.CreateDirectory(fullDir);
                Debug.Log("ps unity dir create is success? " + Directory.Exists(fullDir));
            }

            Debug.Log("ps-unity: copying files");
            foreach (string file in _modelPathsResource.Files)
            {
                string srcFilePath = Path.Combine(srcRoot, file);
                Debug.Log("ps-unity: srcPath: " + srcFilePath);
                string targetFilePath = Path.Combine(dstRoot, file);
                Debug.Log("ps-unity: dstPath: " + targetFilePath);

                using (DownloadHandlerFile handler = new DownloadHandlerFile(targetFilePath))
                using (UnityWebRequest req = new UnityWebRequest(srcFilePath, UnityWebRequest.kHttpVerbGET))
                {
                    req.downloadHandler = handler;
                    Debug.Log("ps-unity: sendRequestStart");
                    await req.SendWebRequest();
                    Debug.Log("ps-unity: sendRequestEnd");
                    if (req.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"ps-unity: Extraction failed for asset {file}: {req.error}");
                    }
                }

                float done = ++copyCount / (float)totalCopies;
                progress?.Invoke(done);
            }

            Debug.Log("ps-unity: finishedrecurse");
            onComplete?.Invoke(true);
            return true;
        }
    }
}