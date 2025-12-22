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
using System.Collections.Generic;
using UnityEngine;

public class VolumeOutput : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.Text _volumeText = null;
    [SerializeField]
    private UnityEngine.UI.Slider _slider = null;
    private SeaShells.SeaShellsRecognizer _recognizer;
    private UnityEngine.UI.Image _sliderFill;
    private float _maxHeardVolume = 1000;
    void Start()
    {
        _sliderFill = _slider.fillRect.GetComponent<UnityEngine.UI.Image>();
    }

    public void SetRecognizer(SeaShells.SeaShellsRecognizer recognizer)
    {
        _recognizer = recognizer;
    }

    void Update()
    {
        string text = "Volume: ";
        float sliderVal = 0;
        if (_recognizer != null && _recognizer.IsListening)
        {
            float v = Mathf.Sqrt(MicController.Instance.SquaredVolumeHeuristic);
            _maxHeardVolume = Mathf.Max(v, _maxHeardVolume);
            sliderVal = v / _maxHeardVolume;
            text = "Volume: " + v;
        }

        _volumeText.text = text;
        _slider.value = sliderVal;
        _sliderFill.color = Lerp3(Color.green, Color.yellow, new Color(1, 165.0f/255.0f, 0), sliderVal);//last color is orange
    }

    /// <summary>
    /// Lerps between three colors (a, b, c).
    /// t ranges from 0 → 1.
    /// 0.0 → a, 0.5 → b, 1.0 → c
    /// </summary>
    public static Color Lerp3(Color a, Color b, Color c, float t)
    {
        if (t < 0.5f)
        {
            // First half: a → b
            return Color.Lerp(a, b, t * 2f);
        }
        else
        {
            // Second half: b → c
            return Color.Lerp(b, c, (t - 0.5f) * 2f);
        }
    }
}
