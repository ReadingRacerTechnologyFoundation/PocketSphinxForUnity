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
