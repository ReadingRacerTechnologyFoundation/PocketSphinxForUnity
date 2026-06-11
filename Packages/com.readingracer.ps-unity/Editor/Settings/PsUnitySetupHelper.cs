using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.IO;
using System;

namespace Rrtf.Editor
{
    [InitializeOnLoad]
    public static class PsUnitySetup
    {
        public const string PackageName = "com.readingracer.ps-unity"; 
        public const string SettingsPath = "Assets/ps_unity_settings.asset";

        private const string IsSetupKey = "ps_unity_setup";

        static PsUnitySetup()
        {
            if (!SessionState.GetBool(IsSetupKey, false))
            {
                Setup();
                SessionState.SetBool(IsSetupKey, true);
            }
        }
        private static void Setup()
        {
            Debug.Log("ps-unity setup running");
            var settings = FindOrCreateEditorSettings();
            if (settings.importModelData)
            {
                FindPackagePath(PackageName, StreamingAssetsSetup);
            }
        }

        private static void StreamingAssetsSetup(string packagePath)
        {
            Debug.Log(packagePath);
            string modelPath = Path.Combine(packagePath, "Dependencies~", "ModelData");
            
            // // Target path in the user's project
            string targetPath = Path.Combine(Application.streamingAssetsPath, "ps-unity-modeldata");

            if (Directory.Exists(modelPath))
            {
                if (!Directory.Exists(targetPath))
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
                if (newPath.EndsWith(".meta") || newPath == ".git") continue; 
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