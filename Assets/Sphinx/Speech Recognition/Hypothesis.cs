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

/**
 * Speech Recognizer is a Unity PocketSphinx framework for speech recognition.
 * To use create a SpeechRecognizer using SpeechCongfig.
 * To Recieve events from it pass functions to SpeechChanged. This is a delegate function of type
 * SpeechEventHandler. SpeechEventArgs will contain the hypothesis (if any) and the eventType.
 * 
 * Add any number of searches then start them with startListening. Only one search can be active at a time.
 * 
 * RecognitionWorker manages listening to the DEFAULT microphone. It is not reccomended you start/stop the mic while
 * using SpeechRecognizer. Do not touch the worker in the unity scene.
 * 
 * Feel free to inherit from SpeechRecognizer for more complex behavior. Make sure to override OnSpeechChanged.
 * Created by Rodrigo Cano 10/2/2014
 **/
namespace Sphinx
{
	/// <summary>
	/// Hypothesis. Used with a decoder to store a hypothesis which can be null,
	/// The best score so far when processing raw audio, 
	/// and the utteranceID when this hypothesis was created.
	/// NewSpeech is any newly found speech since last time a hypothesis was found.
	/// </summary>
	public struct Hypothesis
	{
		public String FullHypothesis;
		public String NewSpeech;
		public int Score;
		public DateTime ArrivalTime; // when the hypothesis was updated

		public void SetToEmpy()
		{
			FullHypothesis = string.Empty;
			NewSpeech = string.Empty;
			Score = 0;
			ArrivalTime = DateTime.Now;
		}
		
		private const string LINE_BREAK = "\n";
		private const string SPACED_LINE_BREAK = " " + LINE_BREAK + " ";
		/// <summary>
		/// Helper method to split FullHypothesis or newspeech into a string[]
		/// </summary>
		/// <param name="s">string you want to split</param>
		/// <param name="removeLineBreaks"></param>
		/// <returns></returns>
		public static string[] SplitThisString(string s, bool removeLineBreaks = true)
		{
			s = s.Replace(" - ", " ");
			s = s.Replace(LINE_BREAK, removeLineBreaks ? " " : SPACED_LINE_BREAK);
			return s.Split(new char[] { }, System.StringSplitOptions.RemoveEmptyEntries);
		}
	}
}

