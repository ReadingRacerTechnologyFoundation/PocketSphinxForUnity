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
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

namespace Rrtf
{
	//this class can only be attached to the TTS gameobject prefab
	//Make sure this is set high on the script order
	public class TTS : MonoBehaviour
	{

		public static bool IsSpeaking { get; private set; }
		public static bool IsMuted { get; set; }

		private const string DONE_MSG = "DONE";
		private const string BEGIN_MSG = "BEGIN";
		private const string UTTERANCE_STATE_CHANGED_METHOD_NAME = "UtteranceStateChanged";

		//pronunciation dictionary
		private static Dictionary<string, string> dict = null;
		private const string DICTIONARY_PATH = "TTSDictionary";

#if UNITY_ANDROID && !UNITY_EDITOR
	private static AndroidJavaObject javaTTSObj;

	private const float ANDROID_SPEECH_RATE = .8f;

	private void SetSpeechRate(float speechRate)
	{
		javaTTSObj.Call("setSpeechRate",speechRate);
	}
#elif UNITY_IPHONE && !UNITY_EDITOR
	[DllImport("__Internal")]
	private static extern void TextToSpeech(string text);
	
	[DllImport("__Internal")]
	private static extern void initTTS(float speechRate, string gameObjectName, string functionName);
	
	private static bool isTTSInitialized;
	private const float IOS_SPEECH_RATE = 0.04f;
	
	static TTS()
	{
		isTTSInitialized = false;
	}
#endif

#if UNITY_EDITOR
		private static TTS tts;

		private IEnumerator fakeTTS(string s)
		{
			//Debug.Log("TTS: " + s);
			IsSpeaking = true;
			//yield return new WaitForSeconds(1);
			int i = 0;
			while (i++ < 100)
				yield return null;
			IsSpeaking = false;
		}

#endif
		// Use this for initialization
		void Awake()
		{
			IsSpeaking = false;
			IsMuted = false;


#if UNITY_ANDROID && !UNITY_EDITOR
		javaTTSObj = new AndroidJavaObject("com.seashells.unitytts.UnityTextToSpeech",gameObject.name,
		                                   UTTERANCE_STATE_CHANGED_METHOD_NAME);

		SetSpeechRate(ANDROID_SPEECH_RATE);
#elif UNITY_IPHONE && !UNITY_EDITOR
		if(isTTSInitialized) return;
		
		initTTS(IOS_SPEECH_RATE,gameObject.name,UTTERANCE_STATE_CHANGED_METHOD_NAME);
		
		isTTSInitialized = true;
#elif UNITY_EDITOR
			tts = this;
#endif

			if (dict == null) loadPronunciationDictionary();


		}

		private void loadPronunciationDictionary()
		{
			TextAsset textAsset = Resources.Load<TextAsset>(DICTIONARY_PATH);
			if (textAsset == null)
			{
				Debug.LogError("TTS Pronunciation dictionary failed to load: Asset not found");
				return;
			}

			string[] lines = textAsset.text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
			dict = new Dictionary<string, string>(lines.Length);

			foreach (string line in lines)
			{
				string[] parts = line.Split(new char[0], 2);
				parts[0] = parts[0].ToUpper();//we only cap the word. the pronunciation is case dependent
				if (string.IsNullOrEmpty(parts[1]))
				{
					Debug.LogError("Line: --" + line + "-- was either empty or contained no definition");
					continue;
				}

				if (dict.ContainsKey(parts[0]))
				{
					Debug.LogError("Pronunciation Dictionary contains multiple pronuncitians for --" + parts[0] + "-- skipping extra pronunciations");
					continue;
				}

				dict.Add(parts[0], parts[1]);
				//Debug.Log("ADD: -" + parts[0] + "-   as: -" + parts[1] + "-");
			}

		}

		public static string GetPronuciation(string word)
		{
			string output;
			dict.TryGetValue(word, out output);

			if (string.IsNullOrEmpty(output)) return string.Empty;

			return output;
		}

		/// <summary>
		/// Speaks the sentence. Tokenizes the sentence so that the TTS dictionary will be used on it
		/// </summary>
		/// <param name="sentence">Sentence.</param>
		public static void SpeakSentence(string sentence)
		{
			if (IsMuted || string.IsNullOrEmpty(sentence)) return;

			string[] words = Rrtf.BasicFSGRecognizer.TextToWords(sentence);
			System.Text.StringBuilder sb = new System.Text.StringBuilder(sentence.Length);

			string txt;
			foreach (string word in words)
			{
				txt = TTS.GetPronuciation(word);
				if (string.IsNullOrEmpty(txt))
					txt = word.ToLower();

				sb.Append(txt).Append(' ');
			}

			TTS.Speak(sb.ToString());
		}

		/// <summary>
		/// Speak the specified textToSpeak. Works best with single words. If using multiple words use SpeakSentence
		/// </summary>
		/// <param name="textToSpeak">Text to speak.</param>
		/// <param name="isExplicitPronunciation">If set to <c>true</c> is explicit pronunciation.</param>
		public static void Speak(string textToSpeak, bool isExplicitPronunciation = false)
		{
			//Debug.Log("TTS: " + textToSpeak);
			if (IsMuted || string.IsNullOrEmpty(textToSpeak)) return;
			if (!isExplicitPronunciation)
			{
				textToSpeak = textToSpeak.ToLower();
				string pronunciation;
				dict.TryGetValue(textToSpeak.ToUpper(), out pronunciation);
				if (!string.IsNullOrEmpty(pronunciation))
					textToSpeak = pronunciation;
			}
#if UNITY_ANDROID && !UNITY_EDITOR
		textToSpeak = textToSpeak.ToLower();
		TTS.IsSpeaking = true;
		javaTTSObj.Call("speak",textToSpeak);
#elif UNITY_IPHONE && !UNITY_EDITOR
		TTS.IsSpeaking = true;
		TextToSpeech(textToSpeak);
#elif UNITY_EDITOR
			if (IsSpeaking) tts.StopCoroutine("fakeTTS");
			//Debug.Log("fake tts: " + textToSpeak);
			tts.StartCoroutine(tts.fakeTTS(textToSpeak));
#endif
		}

		private void UtteranceStateChanged(string msg)
		{
			if (msg == TTS.BEGIN_MSG)
			{
				Debug.Log("TTS UtteranceStateChanged - Begin Message");
			}
			else if (msg == TTS.DONE_MSG)
			{
				Debug.Log("TTS UtteranceStateChanged - Done Message");
			}
			else
			{
				Debug.Log("TTS UtteranceStateChanged - Unknown Message - " + msg);
			}
			IsSpeaking = (msg == TTS.BEGIN_MSG);
		}
	}
}