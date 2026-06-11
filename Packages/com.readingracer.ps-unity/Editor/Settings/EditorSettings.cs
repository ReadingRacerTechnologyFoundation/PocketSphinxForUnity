using UnityEngine;

namespace Rrtf.Editor
{
    /// <summary>
    /// Decide how you want ps-unity to copy default files
    /// </summary>
    public class EditorSettings : ScriptableObject
    {
        [Tooltip("RRTF provides default ModelData for speech recognition." +
        "This must be located in the StreamingAssets Folder. Setting this to true will automatically copy the data over on startup")]
        public bool importModelData = true;
    }   
}
