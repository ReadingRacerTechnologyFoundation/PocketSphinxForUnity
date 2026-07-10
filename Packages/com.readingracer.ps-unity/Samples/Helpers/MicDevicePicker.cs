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
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;

namespace Rrtf.Samples
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class MicDevicePicker : MonoBehaviour
    {
        private TMP_Dropdown _dropdown = null;
        void OnEnable()
        {
            _dropdown ??= GetComponent<TMP_Dropdown>();
            _dropdown.onValueChanged.AddListener(OnNewOption);      
        }

        void OnDisable()
        {
            _dropdown.onValueChanged.RemoveListener(OnNewOption);
        }

        IEnumerator Start()
        {
            while(MicController.Instance == null)
            {
                yield return null;
            }

            while(string.IsNullOrEmpty(MicController.Instance.PreferedMicDevice))
            {
                yield return null;
            }
            if(MicController.Instance.MicDevices == null || MicController.Instance.MicDevices.Length == 0)
            {
                Debug.LogError("no microphones were found. You need a microphone for this");
                yield break;
            }

            _dropdown.AddOptions(new List<string>(MicController.Instance.MicDevices));
            _dropdown.SetValueWithoutNotify
                (Array.FindIndex(MicController.Instance.MicDevices, 
                x => x == MicController.Instance.PreferedMicDevice));
        }

        private void OnNewOption(int selection)
        {
            MicController.SetPreferedDevice(MicController.Instance.MicDevices[selection]);
        }
    }
}
