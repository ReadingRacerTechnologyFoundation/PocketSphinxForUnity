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
using System;

namespace Rrtf
{
	/// <summary>
	/// This is the one stop shop for Mic Control.
	/// </summary>
	public class MicController : MonoBehaviour
	{
		public delegate void NewAudioSamplesFound(MicController mc, EventArgs e);
		public event NewAudioSamplesFound OnNewAudioSamplesFound;

		public static bool IsInited { get; private set; }
		public static MicController Instance { get { return _Me; } }

		//last audio samples taken. Might be null.
		public short[] AudioSamples { get; private set; }

		/// <summary>
		/// This is a volume heuristic.
		/// </summary>
		public float SquaredVolumeHeuristic { get; private set; }

		/// <summary>
		/// number of samples used to calculate the volume heuristic
		/// </summary>
		public float NumSamplesInVolume { get; private set; }

		/// <summary>
		/// when calculating volume, we will only look at the last 3 seconds
		/// </summary>
		public float MaxVolumeSamplesSeconds
		{
			get => _maxVolumeSamplesSeconds;
			set
			{
				_maxVolumeSamplesSeconds = Mathf.Max(0.1f, value);
			}
		}

		private int _LastRecordedPos = 0;
		private AudioClip _Clip;
		private static MicController _Me;
		private float _maxVolumeSamplesSeconds = 1.0f;

		public const int SAMPLE_RATE = 16000;
		private const string NAME = "MIC_CONTROLLER";

		/// <summary>
		/// This is a wrapper for Unity's Microphone.Start function. It will always wrap
		/// and the sample rate will always be 16000.
		/// </summary>
		/// <param name="deviceName">the mic device you want to enable</param>
		/// <param name="lengthSec">the number of seconds to record</param>
		public static void Init(string deviceName, int lengthSec)
		{
			if (Instance != null)
			{
				Debug.LogWarning("Attempting to create another MicController. Not going to happen");
				return;
			}

			_Me = (new GameObject(NAME)).AddComponent<MicController>();
			_Me._Clip = Microphone.Start(null, true, lengthSec, SAMPLE_RATE);
		}

		// Update is called once per frame
		void Update()
		{
			if (_Clip == null)
			{
				Debug.Log("clip is null");
				return;
			}

			AudioSamples = GetNewRecordedAudio();

			if (AudioSamples == null)
				return;

			// Debug.Log("found new audio samples: " + AudioSamples.Length);
			if (OnNewAudioSamplesFound != null)
				OnNewAudioSamplesFound(this, EventArgs.Empty);
		}

		public void ResetUtteranceRMSLevels()
		{
			SquaredVolumeHeuristic = 0.0f;
			NumSamplesInVolume = 0;
		}

		//returns null if no new data. This sometimes happens since 
		//the mic doesn't record every frame.
		//isUpdatingPos should be false when calling from EndWork()
		//this is to allow the other decoders to get the data when they are
		//updated in Update()
		private short[] GetNewRecordedAudio(bool isUpdatingPos = true)
		{
			int newPos = Microphone.GetPosition(null);
			//Debug.Log("newPos: " + newPos);
			if (_LastRecordedPos == newPos) return null;

			//we record continuaslly that means wrapping around around the clip
			int size = (newPos > _LastRecordedPos ?
						newPos - _LastRecordedPos : _Clip.samples - _LastRecordedPos + newPos);

			float[] fSamps = new float[size];
			short[] sSamps = new short[size];
			_Clip.GetData(fSamps, _LastRecordedPos);

			DitherSamples(sSamps, fSamps);

			if (isUpdatingPos)
				_LastRecordedPos = newPos;

			// calculate the RMS level

			if (size > 0)
			{
				float maxVolumeSamples = SAMPLE_RATE * _maxVolumeSamplesSeconds;
				float sumSquaredSampleValues = SumOfSquaredValues(sSamps);
				SquaredVolumeHeuristic = (SquaredVolumeHeuristic * NumSamplesInVolume + sumSquaredSampleValues) / (NumSamplesInVolume + size);
				NumSamplesInVolume = Mathf.Min(maxVolumeSamples, NumSamplesInVolume + size);
			}


			return sSamps;
		}

		// helper method to calculate sum of squares for use in determining average rms level of utterance
		private float SumOfSquaredValues(short[] data)
		{
			float returnValue = 0.0f;
			for (int i = 0; i < data.Length; i++)
			{
				returnValue += (float)data[i] * (float)data[i];
			}
			return returnValue;
		}

		//Dithers the samples. No error checking. length of each array must be the same
		//dithers a 32bit float audio sample ([-1,1])
		//into a 16bit short audio sample
		private void DitherSamples(short[] sSamps, float[] fSamps)
		{
			float linVal;
			float frac;
			const float SHORT_MAX = (float)short.MaxValue;
			for (int i = 0; i < sSamps.Length; ++i)
			{
				linVal = SHORT_MAX * fSamps[i];
				frac = linVal - (int)linVal;
				sSamps[i] = (short)(UnityEngine.Random.value < Mathf.Abs(frac) ? Mathf.Ceil(linVal) : Mathf.Floor(linVal));
			}
		}

		public void UnInit()
		{
			Microphone.End(null);
			_Me = null;
		}

		void OnDestroy()
		{
			UnInit();
		}
	}
}