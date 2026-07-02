using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Rrtf
{
    [CreateAssetMenu(fileName = "ModelPaths", menuName = "RRTF/ModelPaths")]
    public class ModelPaths : ScriptableObject
    {
        public string ModelFolderRoot => _modelFolderRoot;
        public string DictionarySubpath => _dictionarySubpath;
        public string AdultAccousticModelSubpath => _adultAccousticModelSubpath;
        public string ChildAccousticModelSubpath => _childAccousticModelSubpath;
        public FolderNode ModelFolderStructure => _root;
        public int FileCount => _fileCount;

        [SerializeField, Tooltip("The root of all your modeldata in the streaming assets folder")]
        private string _modelFolderRoot;
        [SerializeField, Tooltip("The dictionary path relative to the modelFolderRoot")]
        private string _dictionarySubpath;
        [SerializeField, Tooltip("The path to the accoustic model folder relative to the modelFolderRoot")]
        private string _adultAccousticModelSubpath;
        [SerializeField, Tooltip("Not required unles you want to provide a secondary accoustic model type. This was originally used for children's accoustic models")]
        private string _childAccousticModelSubpath;

        [SerializeField]
        private int _fileCount; // set by editor script
        [SerializeReference]
        private FolderNode _root;

        void OnValidate()
        {
            Debug.Assert(
                !string.IsNullOrEmpty(_modelFolderRoot) && 
                !string.IsNullOrEmpty(_dictionarySubpath) && 
                !string.IsNullOrEmpty(_adultAccousticModelSubpath),
                "One of your folder paths is empty. This will cause a crash", this);
            
            string mfr = Path.Combine(Application.streamingAssetsPath, _modelFolderRoot);
            string dic = Path.Combine(mfr, _dictionarySubpath);
            string am = Path.Combine(mfr, _adultAccousticModelSubpath);

            Debug.Assert(Directory.Exists(mfr), "Your ModelFolderRoot does not exists in streaming assets", this);
            Debug.Assert(File.Exists(dic), "Your DictionarySubpath does not exists in streaming assets", this);
            Debug.Assert(Directory.Exists(am), "Your AdultAccousticModelSubpath does not exists in streaming assets", this);

            if (string.IsNullOrEmpty(_childAccousticModelSubpath))
            {
                string childAm = Path.Combine(mfr, _childAccousticModelSubpath);
                Debug.Assert(Directory.Exists(childAm), "Your child accoustic model folder does not exist. Leave ChildAccousticModelSubPath empty if it is unused", this);
            }
        }
    }


    [Serializable]
    public class FolderNode
    {
        public string Path;
        [SerializeReference]
        public List<string> Files;
        [SerializeReference]
        public List<FolderNode> Folders;
    }
}