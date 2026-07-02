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
        private SerializedProperty _filecountProp;
        private SerializedProperty _rootProp;

        private void OnEnable()
        {
            // Cache the properties once when the Inspector target gains focus
            _modelFolderRootProp = serializedObject.FindProperty("_modelFolderRoot");
            _dictionarySubpathProp = serializedObject.FindProperty("_dictionarySubpath");
            _adultAccousticModelSubpathProp = serializedObject.FindProperty("_adultAccousticModelSubpath");
            _childAccousticModelSubpathProp = serializedObject.FindProperty("_childAccousticModelSubpath");
            _filecountProp = serializedObject.FindProperty("_fileCount");
            _rootProp = serializedObject.FindProperty("_root");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.DelayedTextField(_modelFolderRootProp);
            EditorGUILayout.PropertyField(_dictionarySubpathProp);
            EditorGUILayout.PropertyField(_adultAccousticModelSubpathProp);
            EditorGUILayout.PropertyField(_childAccousticModelSubpathProp);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_filecountProp);
            EditorGUI.EndDisabledGroup();

            if (serializedObject.ApplyModifiedProperties())
            {
                string fullpath = Path.Combine(Application.streamingAssetsPath, _modelFolderRootProp.stringValue);
                FolderNode root = RecordFolderInfo(fullpath, out int filecount);
                _filecountProp.intValue = filecount;
                _rootProp.managedReferenceValue = root;
                serializedObject.ApplyModifiedProperties();
            }
        }

        public static FolderNode RecordFolderInfo(string fullpath, out int filecount)
        {
            filecount = 0;
            if (!Directory.Exists(fullpath))
            {
                EditorDialog.DisplayAlertDialog("Failure", "Failure to update directory info. The directory does not exist.", "Ok", DialogIconType.Error);
                return null;
            }

            FolderNode root = DoRecord(fullpath, ref filecount);
            if (root == null || filecount == 0)
            {
                EditorDialog.DisplayAlertDialog("Failure", "Failure to update directory info. The directory doesn't contain any files", "Ok", DialogIconType.Error);
                root = null;
            } 
            else
            {
                Debug.Log("successfully updated model data folder structure");
            }
            return root;
        }

        public static FolderNode DoRecord(string path, ref int fileCount)
        {
            FolderNode n = null;
            var files = Directory.GetFiles(path)
                .Where(file =>
                    !file.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase) &&
                    !file.EndsWith(".DS_Store", System.StringComparison.OrdinalIgnoreCase)
                    ).ToArray();
            var dirs = Directory.GetDirectories(path);

            List<FolderNode> childNodes = new List<FolderNode>(dirs.Length);
            foreach (var dir in dirs)
            {
                var node = DoRecord(dir, ref fileCount);
                if (node != null)
                {
                    childNodes.Add(node);
                }
            }

            //if there actually is stuff, fill out n
            if (childNodes.Count > 0 || files.Length > 0)
            {
                n = new FolderNode
                {
                    Path = path,
                    Files = new List<string>(files),
                    Folders = childNodes
                };
                fileCount += files.Length;
            }

            return n;
        }
    }
}