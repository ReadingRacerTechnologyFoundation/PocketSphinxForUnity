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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rrtf.Editor
{
    [CustomEditor(typeof(ModelPaths))]
    public class ModelPathsEditor : UnityEditor.Editor
    {
        private SerializedProperty _modelFolderRootProp;
        private SerializedProperty _dictionarySubpathProp;
        private SerializedProperty _adultAccousticModelSubpathProp;
        private SerializedProperty _childAccousticModelSubpathProp;
        private SerializedProperty _filesProp;
        private SerializedProperty _dirsProp;

        private void OnEnable()
        {
            // Cache the properties once when the Inspector target gains focus
            _modelFolderRootProp = serializedObject.FindProperty("_modelFolderRoot");
            _dictionarySubpathProp = serializedObject.FindProperty("_dictionarySubpath");
            _adultAccousticModelSubpathProp = serializedObject.FindProperty("_adultAccousticModelSubpath");
            _childAccousticModelSubpathProp = serializedObject.FindProperty("_childAccousticModelSubpath");
            _filesProp = serializedObject.FindProperty("_files");
            _dirsProp = serializedObject.FindProperty("_dirs");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.DelayedTextField(_modelFolderRootProp);
            EditorGUILayout.PropertyField(_dictionarySubpathProp);
            EditorGUILayout.PropertyField(_adultAccousticModelSubpathProp);
            EditorGUILayout.PropertyField(_childAccousticModelSubpathProp);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_dirsProp, new GUIContent("Directories"));
            EditorGUILayout.PropertyField(_filesProp);
            EditorGUI.EndDisabledGroup();

            if (serializedObject.ApplyModifiedProperties())
            {
                string fullpath = Path.Combine(Application.streamingAssetsPath, _modelFolderRootProp.stringValue);
                int prefixLength = fullpath.Length + 1;//used to make relative paths later. +1 to remove leading slash
                List<string> files = new List<string>(20);
                List<string> dirs = new List<string>(20);
                RecordFolderInfo(fullpath, files, dirs, prefixLength);
                _filesProp.arraySize = files.Count;
                for (int i = 0; i < files.Count; i++)
                {
                    _filesProp.GetArrayElementAtIndex(i).stringValue = files[i];
                }
                _dirsProp.arraySize = dirs.Count;
                for (int i = 0; i < dirs.Count; i++)
                {
                    _dirsProp.GetArrayElementAtIndex(i).stringValue = dirs[i];
                }
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void RecordFolderInfo(string fullpath, List<string> files, List<string> dirs, int prefixLength)
        {
            if (!Directory.Exists(fullpath))
            {
                EditorDialog.DisplayAlertDialog("Failure", "Failure to update directory info. The directory does not exist.", "Ok", DialogIconType.Error);
                return;
            }

            DoRecord(fullpath, files, dirs, prefixLength);
            if (files.Count == 0)
            {
                EditorDialog.DisplayAlertDialog("Failure", "Failure to update directory info. The directory doesn't contain any files", "Ok", DialogIconType.Error);
            }
            else
            {
                Debug.Log("successfully updated model data folder structure");
            }
        }

        /// <summary>
        /// DFS algorithm that finds all the folder and files
        /// </summary>
        /// <param name="path"></param>
        /// <param name="files"></param>
        /// <param name="dirs"></param>
        /// <param name="prefixLength"></param>
        private void DoRecord(string path, List<string> files, List<string> dirs, int prefixLength)
        {
            var newFiles = Directory.GetFiles(path)
                .Where(file =>
                    !file.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase) &&
                    !file.EndsWith(".DS_Store", System.StringComparison.OrdinalIgnoreCase)
                )
                .Select(file => file.Substring(prefixLength));
            files.AddRange(newFiles);
            var newDirs = Directory.GetDirectories(path);

            foreach (var dir in newDirs)
            {
                int oldFileCount = files.Count;
                DoRecord(dir, files, dirs, prefixLength);
                if (oldFileCount != files.Count)//we added new files!
                {
                    dirs.Add(dir.Substring(prefixLength));
                }
            }
        }
    }
}