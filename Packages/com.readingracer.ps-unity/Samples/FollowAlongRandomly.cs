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
using System.Collections.Generic;

namespace Rrtf
{
	/// <summary>
	/// A simple example showing how to use the BasicFSGRecognizer for words in any order.
	/// cloze words can be said in any order
	/// </summary>
	public class FollowAlongRandomly : MonoBehaviour
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
		private Color _readColor = Color.green;
		private string[] _searchWords;
		private Dictionary<string, Text> _wordsToUI = new Dictionary<string, Text>();
		private int _readWordsCount = 0;
		public BasicFSGRecognizer Recognizer { get; private set; }

		IEnumerator Start()
		{
			_searchWords = BasicFSGRecognizer.TextToWords(_searchString);
			for (int i = 0; i < _searchWords.Length; ++i)
			{
				_wordsToUI[_searchWords[i]] = _words[i];
			}
			_readWordsCount = 0;

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
			//set cloze to true for random order listening!
			Recognizer.ListenFor(_searchString, InitModelPaths.DefaultLanguageModelWeight, 0, _foregivness, true);
			Recognizer.EnableMicAndRecognize();
			Debug.Log("began listening");
		}

		public void FingerUp()
		{
			if (Recognizer == null)
			{
				return;
			}

			Reset();

			Recognizer.stopRecognizing();
			_readButton.GetComponent<Image>().color = Color.white;
		}
		private void Reset()
		{
			for (int i = 0; i < _words.Length; ++i)
			{
				_words[i].color = _unreadColor;
			}
			_readWordsCount = 0;
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

			for (int i = 0; i < heardWords.Length; ++i)
			{
				if (_wordsToUI[heardWords[i]].color == _unreadColor)
				{
					++_readWordsCount;
					_wordsToUI[heardWords[i]].color = _readColor;

					if (_readWordsCount == _searchWords.Length)
					{
						Recognizer.stopRecognizing();
					}
				}
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
