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
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using System.IO;
using System;

namespace Rrtf.Editor
{
    /// <summary>
    /// This class is made to setup two things
    /// 1. Streaming Assets files. You can choose not to do this by using the Assets/ps_unity_settings asset
    /// 2. Resources/RRTF/ModelPaths. This will ALWAYS be copied if its missing. However, if you want to modify 
    /// your own model paths, just modify the one in resources. InitModelPaths will always look for this file.
    /// </summary>
    [InitializeOnLoad]
    public static class PsUnitySetup
    {
        public const string PackageName = "com.readingracer.ps-unity"; 
        public const string SettingsPath = "Assets/ps_unity_settings.asset";

        private const string IsSetupKey = "ps_unity_setup";

        private const string ModelPathsSrcPath = "Packages/com.readingracer.ps-unity/DEFAULT_ModelPaths.asset";
        private const string ModelPathsDstPath = "Assets/Resources/" + InitModelPaths.ModelPathsResourcePath + ".asset";

        static PsUnitySetup()
        {
            EditorApplication.delayCall += WaitForImport;
        }

        private static void WaitForImport()
        {
            // Guard to ensure it only runs once per session
            if (SessionState.GetBool(IsSetupKey, false)) return;

            // If Unity is still currently importing or compiling, wait for the next cycle
            if (EditorApplication.isUpdating || EditorApplication.isCompiling)
            {
                EditorApplication.delayCall += WaitForImport;
                return;
            }

            Setup();
            SessionState.SetBool(IsSetupKey, true);
        }

        // [MenuItem("GameObject/TestPSUnity")]
        private static void Setup()
        {
            Debug.Log("ps-unity setup running");
            var settings = FindOrCreateEditorSettings();
            if (settings.importModelData)
            {
                FindPackagePath(PackageName, OnPackageFound);
            }
        }

        private static void CopyModelPaths()
        {
            if (AssetDatabase.LoadAssetAtPath<ModelPaths>(ModelPathsDstPath) != null)
            {
                return;
            }

            Debug.Assert(AssetDatabase.LoadAssetAtPath<ModelPaths>(ModelPathsSrcPath) != null, 
                ModelPathsDstPath + "is missing. Please check you didn't accidentally delete it. This is needed");

            //create folders if they don't exist
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string targetSubfolder =  Path.GetDirectoryName(ModelPathsDstPath);
            string targetDir = Path.Combine(projectRoot, targetSubfolder);
            Directory.CreateDirectory(targetDir);

            if (AssetDatabase.CopyAsset(ModelPathsSrcPath, ModelPathsDstPath))
            {
                AssetDatabase.Refresh();
                Debug.Log("successfully copied ModelPaths to resources");
            }
            else
            {
                Debug.LogError("Failed to copy ModelPaths to resources. This will cause a crash");
            }
        }

        private static void OnPackageFound(string packagePath)
        {
            StreamingAssetsSetup(packagePath);
            CopyModelPaths();
            Debug.Log("ps-unity setup finished");
        }

        private static void StreamingAssetsSetup(string packagePath)
        {
            string modelPath = Path.Combine(packagePath, "Dependencies~", "ModelData");
            
            // Target path in the user's project
            string targetPath = Path.Combine(Application.streamingAssetsPath, "ps-unity-modeldata");

            if (Directory.Exists(modelPath))
            {

                //meta files might get added so we just gotta make sure that there's actually stuff in that folder
                if (!Directory.Exists(targetPath) || (Directory.GetFiles(targetPath).Length == 0))
                {
                    Directory.CreateDirectory(targetPath);
                    CopyFilesRecursively(modelPath, targetPath);
                    AssetDatabase.Refresh();
                    Debug.Log("[ps-unity] Successfully copied ModelData assets to StreamingAssets!");
                }
            }
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                // Skip .meta files from the source if any exist
                if (newPath.EndsWith(".meta") || newPath.EndsWith(".git"))
                {
                    continue;
                }
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        public static EditorSettings FindOrCreateEditorSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<EditorSettings>(SettingsPath);

            if (!settings)
            {
                settings = ScriptableObject.CreateInstance<EditorSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        private static void FindPackagePath(string packageName, Action<string> onComplete)
        {
            var req = Client.List(true);
            void Progress()
            {
                if (!req.IsCompleted)
                    return;

                EditorApplication.update -= Progress;
                string path = string.Empty;

                if (req.Status == StatusCode.Success)
                {
                    foreach (var package in req.Result)
                    {
                        if (package.name == packageName)
                        {
                            path = package.resolvedPath;
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(path))
                    {
                         Debug.LogError($"ps-unity was not able to copy the model data to the streaming assets folder due the package not being found: {packageName}");
                    }
                }
                else
                {
                    Debug.LogError($"ps-unity was not able to copy the model data to the streaming assets folder due to a package error: {req.Error.message}");
                }

                onComplete?.Invoke(path);
            }
            EditorApplication.update += Progress;
        }
    }
}