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
using System.Linq;
using Unity.Collections;

namespace Rrtf
{
	/// <summary>
	/// This is the one stop shop for Mic Control. Init is called by the speechrecognizer. You can choose a mic by
	/// setting the PreferedMicDevice at any point. This uses a circular buffer.
	/// </summary>
	public class MicController : MonoBehaviour
	{
		/// <summary>
		/// Delegate for when audiosamples are found from the mic
		/// </summary>
		/// <param name="mc">The Mic Controller that has new data. (its a singleton) </param>
		/// <param name="e">The event base type. It should be empty</param>
		public delegate void NewAudioSamplesFound(MicController mc, EventArgs e);

		/// <summary>
		/// Event fired when new audio samples are found. Use with AudioSamples to get the new samples.
		/// </summary>
		public event NewAudioSamplesFound OnNewAudioSamplesFound;

		/// <summary>
		/// List the available microphone devices
		/// </summary>
		public string[] MicDevices => Microphone.devices;

		/// <summary>
		/// get the name of the mic thats currently being used. This will return the
		/// name of the default device if you use setPreferedDevice(null) or setPreferedDevice(string.Empty).
		/// if this returns null or empty, it means this probably hasn't been inited yet
		/// </summary>
		public string PreferedMicDevice => _preferedDevice;

		/// <summary>
		/// get the mic controller instance. will be null if not inited
		/// </summary>
		public static MicController Instance { get { return _Me; } }

		/// <summary>
		/// last audio samples taken. Use AudioSamplesSize to get the new samples size.
		/// This buffer is larger than whats neccessary. It may also change if resize is needed.
		/// </summary>
		public short[] AudioSamples { get; private set; } = new short[SAMPLE_RATE/4];
		private float[] _floatBuffer = new float[SAMPLE_RATE/4];

		/// <summary>
		/// The number of valid samples in AudioSamples
		/// </summary>
		public int AudioSamplesSize {get; private set;}

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
		private float _maxVolumeSamplesSeconds = 0.25f;

		public const int SAMPLE_RATE = 16000;
		private const string NAME = "MIC_CONTROLLER";
		private static string _preferedDevice = null;

		/// <summary>
		/// This is a wrapper for Unity's Microphone.Start function. It will always wrap
		/// and the sample rate will always be 16000. Do not init yourself. The SpeechRecognizerl should do that
		/// </summary>
		/// <param name="lengthSec">the number of seconds to record</param>
		public static void Init(int lengthSec)
		{
			if (Instance != null)
			{
				Debug.LogWarning("Attempting to create another MicController. Not going to happen");
				return;
			}

			_Me = (new GameObject(NAME)).AddComponent<MicController>();
			_Me._Clip = Microphone.Start(_preferedDevice, true, lengthSec, SAMPLE_RATE);
			//find the default device
			if (string.IsNullOrEmpty(_preferedDevice))
			{
				string tempDevice = "error finding device";
				foreach(string n in Microphone.devices)
				{
					if (Microphone.IsRecording(n))
					{
						tempDevice = n;
						break;
					}
				}

				_preferedDevice = tempDevice;
			}
		}

		/// <summary>
		/// Sets the prefered mic device. If set to null or empty then it will find the default device and use that.
		/// If the device cannot be found in the MicDevices list then this will not do anything.
		/// </summary>
		/// <param name="name">The device you want to use</param>
		public static void SetPreferedDevice(string name)
		{
			if(!string.IsNullOrEmpty(name) && !Microphone.devices.Contains(name))
			{
				return;
			}

			if(_Me == null)
			{
				_preferedDevice = name;
				return;
			}

			float length = _Me._Clip.length;
			Microphone.End(_preferedDevice);//the only way this wouldnt be recording is if _Me == null
			_preferedDevice = name;
			_Me._Clip = Microphone.Start(_preferedDevice, true, (int)length, SAMPLE_RATE);
			if (string.IsNullOrEmpty(_preferedDevice))
			{
				string tempDevice = "error finding device";
				foreach(string n in Microphone.devices)
				{
					if (Microphone.IsRecording(n))
					{
						tempDevice = n;
						break;
					}
				}

				_preferedDevice = tempDevice;
			}

			_Me.AudioSamplesSize = 0;
			_Me.SquaredVolumeHeuristic = 0;
			_Me.NumSamplesInVolume = 0;
			_Me._LastRecordedPos = 0;
		}

		// Update is called once per frame
		void Update()
		{
			if (_Clip == null)
			{
				Debug.Log("clip is null");
				return;
			}

			bool hasNew = GetNewAudioSamples();

			// Debug.Log("found new audio samples: " + AudioSamples.Length);
			if (OnNewAudioSamplesFound != null && AudioSamplesSize > 0 && hasNew)
				OnNewAudioSamplesFound(this, EventArgs.Empty);
		}

		public void ResetUtteranceRMSLevels()
		{
			SquaredVolumeHeuristic = 0.0f;
			NumSamplesInVolume = 0;
		}

		//updates the AudioSamples array with new audio data. Returns true if new data was found
		private bool GetNewAudioSamples(bool isUpdatingPos = true)
		{
			int newPos = Microphone.GetPosition(_preferedDevice);
			//Debug.Log("newPos: " + newPos);
			if (_LastRecordedPos == newPos) return false;

			//we record continuaslly that means wrapping around around the clip
			int size = 	newPos > _LastRecordedPos ?
						newPos - _LastRecordedPos : _Clip.samples - _LastRecordedPos + newPos;

			if(size > _floatBuffer.Length)
			{
				_floatBuffer = new float[size];
				AudioSamples = new short[size];
			}

			AudioSamplesSize = size;
			Span<float> fSamps = _floatBuffer.AsSpan().Slice(0, size);
			Span<short> sSamps = AudioSamples.AsSpan().Slice(0, size);
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

			return true;
		}

		// helper method to calculate sum of squares for use in determining average rms level of utterance
		private float SumOfSquaredValues(Span<short> data)
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
		private void DitherSamples(Span<short> sSamps, Span<float> fSamps)
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