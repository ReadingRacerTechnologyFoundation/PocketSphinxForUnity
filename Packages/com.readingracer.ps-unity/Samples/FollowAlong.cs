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
using Rrtf.Sphinx;
using UnityEngine.UI;

namespace Rrtf
{
	/// <summary>
	/// A simple example showing how to use the BasicFSGRecognizer. Which specializes in listening to sentences.
	/// </summary>
	public class FollowAlong : MonoBehaviour
	{

		[SerializeField, Tooltip("The string we are listening for")]
		private string _searchString = "Sammy Snake who went to the lake";
		[SerializeField, Tooltip("How easy is it to hear the next word. 1 is very forgiving"), Range(0.0f, 1.0f)]
		private float _foregivness = 0.5f;
		[SerializeField, Tooltip("Each model is specialized in listening to particular types of speakers")]
		private InitModelPaths.ACCOUSTIC_MODELS _accousticModel = InitModelPaths.ACCOUSTIC_MODELS.ADULT;


		[SerializeField, Header("UI components")]
		private Transform _readButton = null;
		[SerializeField]
		private Text _initModelPathsText = null;
		[SerializeField]
		private Text _micDevice = null;
		[SerializeField]
		private Text[] _words = new Text[7];


		private Color _unreadColor = Color.gray;
		private int _wordIndex = 0;
		private string[] _searchWords;
		public BasicFSGRecognizer Recognizer { get; private set; }

		IEnumerator Start()
		{
			_searchWords = BasicFSGRecognizer.TextToWords(_searchString);

			//we need to make sure initModelPaths is done, just for android. 
			//its best if InitModelPaths is placed in a preload scene first
			InitModelPaths.OnPathUpdateProgress = progressUpdate;
			while (!InitModelPaths.ArePathsFixed)
			{
				yield return null;
			}
			_initModelPathsText.gameObject.SetActive(false);

			InitModelPaths.AMChoice = _accousticModel;
			Recognizer = new BasicFSGRecognizer(InitModelPaths.DefaultLanguageModelWeight, CreateConfig());
			Recognizer.SpeechChanged += HandleSpeechEvents;
			_unreadColor = _words[0].color;

			VolumeOutput vo = GetComponent<VolumeOutput>();
			if (vo)
			{
				vo.SetRecognizer(Recognizer);
			}

			if (_micDevice)
			{
				//wait for mic controller to become initialized
				while(MicController.Instance == null)
				{
					yield return null;
				}
				_micDevice.text = "Mic: " + MicController.Instance.MicDevice;
			}
		}

		private void progressUpdate(float p)
		{
			_initModelPathsText.text = "Loading: " + ((int)(p * 100));
		}

		void OnDestroy()
		{
			Recognizer.stopRecognizingAndTurnOffMic();
		}
		public void FingerDown()
		{
			if (Recognizer == null)
			{
				return;
			}

			_readButton.GetComponent<Image>().color = Color.gray;
			Recognizer.ListenFor(_searchString, InitModelPaths.DefaultLanguageModelWeight, _wordIndex, _foregivness, false);
			Recognizer.EnableMicAndRecognize();
			Debug.Log("began listening");
		}

		public void FingerUp()
		{
			if (Recognizer == null)
			{
				return;
			}

			if (_wordIndex >= _words.Length)
			{
				Reset();
			}
			Recognizer.stopRecognizing();
			_readButton.GetComponent<Image>().color = Color.white;
		}
		private void Reset()
		{
			_wordIndex = 0;
			for (int i = 0; i < _words.Length; ++i)
			{
				_words[i].color = _unreadColor;
			}
		}

		protected void HandleSpeechEvents(SpeechRecognizer sender, SpeechEventArgs e)
		{
			//the final hypothesis is more accurate but you'll have to wait to stop listening to get it
			if (e.EventType != SpeechEventArgs.SpeechEventType.PartialHypothesisFound)
			{
				return;
			}

			Debug.Log("new speech: " + e.Hyp.NewSpeech);
			string[] heardWords = Hypothesis.SplitThisString(e.Hyp.NewSpeech);//new speech contains only the new words

			//there's more complicated algorithms for checking newspeech that give better results
			//but we will go with just checking the last two words in newspeech
			for (int i = Mathf.Max(0, heardWords.Length - 2); i < heardWords.Length; ++i)
			{
				if (_searchWords[_wordIndex] == heardWords[i])
				{
					nextWord();
				}
			}
		}

		private void nextWord()
		{
			_words[_wordIndex].color = Color.green;

			_wordIndex += 1;

			if (_wordIndex == _searchWords.Length)
			{
				Recognizer.stopRecognizing();
			}
		}

		private static SpeechConfig CreateConfig()
		{
			//config properties we are using
			const float VAD_THRESHOLD = 2.0f;
			const float FILL_PROB = 1e-3f;
			const bool IS_REMOVING_SILENCE = true;
			SpeechConfig.SetDictionary(InitModelPaths.DictionaryPath);
			SpeechConfig.SetAcousticModel(InitModelPaths.AccousticModelFolder);
			SpeechConfig.SetFloat("-vad_threshold", VAD_THRESHOLD);
			SpeechConfig.SetBoolean("-remove_silence", IS_REMOVING_SILENCE);
			SpeechConfig.SetFloat("-fillprob", FILL_PROB);
			#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_MAC
			SpeechConfig.CreateLogFile(); //I prefer to check the unity log instead in editor
			#endif

			return SpeechConfig.GenerateConfigWithCurrentSettings();
		}

	}
}