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
using System.Threading.Tasks;
using System;
using UnityEngine.Assertions.Comparers;

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
        public const string ModelPathsResourcePath = "RRTF/ModelPaths";
        private static string modelFolder;
        private static string dictionaryPath;

        public enum ACCOUSTIC_MODELS { ADULT = 0, CHILD = 1 }

        //returns the LM weight depending on device and current
        //AM choice
        public static int DefaultLanguageModelWeight
        {
            get
            {
                if (AMChoice == ACCOUSTIC_MODELS.ADULT) return 1;
                else return 5;
            }
        }

        //change this in the static constructor down below for testing purposes within unity
        //accoustic model choice.
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
        public static bool ArePathsFixed { get; private set; }
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
        public static Action<float> OnPathUpdateProgress;
        public static Action<bool> OnPathUpdateCompletion;
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CorrectPathsForEditor()
        {
            if (!FindAnyObjectByType<InitModelPaths>())
            {
                SetupCorrectedAssetPaths(Application.streamingAssetsPath);
            }
        }
#endif
        void Awake()
        {
            //why do i do this? Because Android is stupid. I need to know the folder names and such when I run
            //the createModelCacheForAndroid coroutine. BUt it doesnt matter for the rest
#if UNITY_STANDALONE_ANDROID
    createModelCacheForAndroid();
#else
            SetupCorrectedAssetPaths(Application.streamingAssetsPath);
#endif
        }

        private static void SetupCorrectedAssetPaths(string baseFolder)
        {
            if (ArePathsFixed)
            {
                return;
            }

            if (_modelPathsResource == null)
            {
                _modelPathsResource = Resources.Load<ModelPaths>(ModelPathsResourcePath);
            }

            UpdateAMFolder();
            modelFolder = Path.Combine(baseFolder, _modelPathsResource.ModelFolderRoot);
            dictionaryPath = Path.Combine(modelFolder, _modelPathsResource.DictionarySubpath);
            Debug.Log("ps-unity: paths fixed using modelFolder: " + modelFolder);
            Debug.Log("ps-unity: am folder: " + accousticModelFolder);

            ArePathsFixed = true;
        }

        private async Task createModelCacheForAndroid()
        {
            if (ArePathsFixed || IsCurrentlyUpdatingPaths)
            {
                OnPathUpdateCompletion.Invoke(false);
                return;
            }

            IsCurrentlyUpdatingPaths = true;
            if (_modelPathsResource == null)
            {
                _modelPathsResource = Resources.Load<ModelPaths>(ModelPathsResourcePath);
            }
            await Extract(OnPathUpdateProgress, OnPathUpdateCompletion);

            SetupCorrectedAssetPaths(Application.temporaryCachePath);
            IsCurrentlyUpdatingPaths = false;
        }

        private static void UpdateAMFolder()
        {
            if (IsCurrentlyUpdatingPaths)
            {
                Debug.LogError("ps-unity: can't change accoustic model if android is currently copying folder data");
                return;
            }

            if(!ArePathsFixed)
            {
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
            int[] fileCounter = { 0 };//we need to pass this by ref to an awaitable function, using 'ref' isn't possible
            await RecurseExtract(_modelPathsResource.ModelFolderStructure, progress, fileCounter);

            string rootPath = Path.Combine(Application.temporaryCachePath, _modelPathsResource.ModelFolderRoot);
            onComplete?.Invoke(true);
            return true;
        }

        private async Task RecurseExtract(FolderNode node, Action<float> progress, int[] filesCopiedSoFar)
        {
            string dstFolder = Path.Combine(Application.temporaryCachePath, node.Path);
            Directory.CreateDirectory(dstFolder);

            foreach (string file in node.Files)
            {
                string srcFilePath = Path.Combine(Application.streamingAssetsPath, file);
                string targetFilePath = Path.Combine(dstFolder, file);

                using (DownloadHandlerFile handler = new DownloadHandlerFile(targetFilePath))
                using (UnityWebRequest req = new UnityWebRequest(srcFilePath, UnityWebRequest.kHttpVerbGET))
                {
                    req.downloadHandler = handler;
                    await req.SendWebRequest();

                    if (req.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"Extraction failed for asset {file}: {req.error}");
                    }
                }

                filesCopiedSoFar[0]++;
                float done = filesCopiedSoFar[0] / (float)_modelPathsResource.FileCount;
                progress?.Invoke(done);
            }

            foreach (var folder in node.Folders)
            {
                await RecurseExtract(folder, progress, filesCopiedSoFar);
            }
        }
    }
}