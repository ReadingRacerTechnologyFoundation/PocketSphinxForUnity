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
using System.Collections.Generic;
using System;
using UnityEngine.Analytics;

namespace Sphinx
{
	using SphinxNative;
	/// <summary>
	/// Helper class to help better manage and call pocketsphinx 
	/// decoder functions in a more C# like enviroment
	/// </summary>
	public class Decoder
	{
		/// <summary>
		/// Gets the raw decoder. Be very careful when using this. IF this decoder's reference count = 0;
		/// then this rawdecoder WILL be deallocated.
		/// </summary>
		/// <value>The raw decoder.</value>
		public ps_decoder_t RawDecoder { get{ return decoder; }}

		private ps_decoder_t decoder;
		private string lastHypothesis = string.Empty;
		private int lastHypothesisScore = 0;
		private int scoreDifference = 0; 
		private DateTime lastChangeArrivalTime = DateTime.Now; 

		private string strictnessLevel = "medium"; // medium level 

		// score difference thresholds 
		// example threshold, tuned by hand. 
		// DONE: account for time passing - this should be some amount of difference since the last change to the hypothesis 
		// DONE: ignore the START_ words when calculating this -- e.g. SAMMY to SAMMY SNAKE should be counted, not the intervening SAMMY START_SNAKE 
		// at 11,000 words such as "beautiful" become very difficult to recognize
		private double extremeScoreDifferencePerSecond = 12000.0; 
		private double hardScoreDifferencePerSecond = 15000.0; 
		private double mediumScoreDifferencePerSecond = 17000.0; 
		// no criterion for easy level 

		private long minToughWordLength = 5;  // only use the high score difference per second for "tough" (long) words where the word is likely to be long enough to have reliable scores

		private bool doublecheckedSomeWordThisUtterance = false; // only double check one word per utterance

		private Hypothesis lastHyp; // delay by one hypothesis update (not a new word, just an update)

		/// <summary>
		/// Initializes a new instance of the <see cref="Sphinx.Decoder"/> class.
		/// use SpeechConfig to generate a new conifuration
		/// </summary>
		/// <param name="config">Config.</param>
		public Decoder(SpeechConfig config)
		{
			this.decoder = PocketSphinx.ps_initialize(config.Config);
		}

		/// <summary>
		/// Reinitialize the Decoder to use the specified newConfig.
		/// </summary>
		/// <param name="newConfig">New config.</param>
		public bool Reinitialize(SpeechConfig newConfig)
		{
			return PocketSphinx.ps_reinit(decoder,newConfig.Config);
		}

		/// <summary>
		/// Pocketsphinx ps_decoder_t needs to be freed with its own special function
		/// </summary>
		~Decoder()
		{
			PocketSphinx.ps_free(decoder);
		}

		/// <summary>
		/// Loads a new dictionary. Overwriting the one used previously, possibly set by SpeechConfig.
		/// </summary>
		/// <returns><c>true</c>, if new dictionary was loaded, <c>false</c> otherwise.</returns>
		/// <param name="dictPath">The Dictinory File Path</param>
		public bool LoadNewDictionary(string dictPath)
		{
			return PocketSphinx.ps_load_dict(decoder,dictPath,null,null);
		}

		/// <summary>
		/// Loads a new dictionary. Overwriting the one used previously, possibly set by SpeechConfig.
		/// </summary>
		/// <returns><c>true</c>, if new dictionary was loaded, <c>false</c> otherwise.</returns>
		/// <param name="dictPath">The Dictionory File Path</param>
		/// <param name="fillerDictPath">The filler dictionray file path.</param>
		public bool LoadNewDictionary(string dictPath, string fillerDictPath)
		{
			return PocketSphinx.ps_load_dict(decoder,dictPath,fillerDictPath,null);
		}

		/// <summary>
		/// Saves the dictionary to file.
		/// </summary>
		/// <returns><c>true</c>, if dictionary to file was saved, <c>false</c> otherwise.</returns>
		/// <param name="dictPath">The path of the new dictionary file</param>
		public bool SaveDictionaryToFile(string dictPath)
		{
			return PocketSphinx.ps_save_dict(decoder,dictPath,null);
		}
		
		/// <summary>
		/// Adds a word to the dictionary this decoder is using the default phonemes generator
		/// class
		/// </summary>
		/// <returns><c>true</c>, if word was added, <c>false</c> otherwise.</returns>
		/// <param name="word">Word.</param>
		public bool AddWord(string word)
		{
			bool isAddedSuccesfully = true;
			string[] phones = EnglishPhonemes.Phoneme.toPhoneme(word);
			isAddedSuccesfully = PocketSphinx.ps_add_word(decoder,word,phones[0].Trim(),true) >= 0;
			//Debug.Log("Added: " + word + "(" + (0+1) + ")" + "  --as " + phones[0].Trim());
			for(int i = 1 ; i < phones.Length; ++i)
			{
				isAddedSuccesfully = isAddedSuccesfully && PocketSphinx.ps_add_word
					(decoder,word + "(" + (i+1) + ")",
					 phones[i].Trim(),true) >= 0;
				//Debug.Log("Added: " + word + "(" + (i+1) + ")" + "  --as " + phones[i].Trim());
			}

			return isAddedSuccesfully;
		}

		/// <summary>
		/// Adds a word to the dictionary using the specified phones.
		/// </summary>
		/// <returns><c>true</c>, if word was added, <c>false</c> otherwise.</returns>
		/// <param name="word">Word.</param>
		/// <param name="phones">Phones.</param>
		public bool AddWord(string word, string phones)
		{
			return PocketSphinx.ps_add_word(decoder,word,phones,true) >= 0;
		}

		/// <summary>
		/// Looks up word in the dictionary. returns null if not in the dictionary
		/// </summary>
		/// <returns>The phonemes that matke up the word OR null if it is not found</returns>
		/// <param name="word">Word.</param>
		public String LookUpWord(string word)
		{
			//changed by Rodrigo Cano on 2/22/15 due to needing to use the pronounciation to generate
			//start words now.
			return PocketSphinx.ps_lookup_word(decoder,word);
		}

		/// <summary>
		/// Starts the utterance.
		/// </summary>
		/// <returns><c>true</c>, if utterance was started, <c>false</c> otherwise.</returns>
		/// <param name="inStrictnessLevel">Provide a way to specify the strictness of the score change criterion</param>
		public bool StartUtterance(string inStrictnessLevel)
		{
			lastHypothesis = string.Empty;

			lastHyp.Score = 0;
			lastHyp.FullHypothesis = string.Empty;
			lastHyp.NewSpeech = lastHyp.FullHypothesis;
			lastHyp.ArrivalTime = DateTime.Now;

			lastHypothesisScore = 0;
			scoreDifference = 0; 
			lastChangeArrivalTime = DateTime.Now; 
			strictnessLevel = inStrictnessLevel;
			return PocketSphinx.ps_start_utt(decoder);
		}

		// typically call this once per sentence in order to double check words once
		public void ResetDoublecheck()
		{
			doublecheckedSomeWordThisUtterance = false; 
		}

		/// <summary>
		/// Processes the partial raw audio. A whole utterance is not expected when using this.
		/// Not a problem IF a whole utterance is processed when using this.
		/// </summary>
		/// <returns>The partial raw audio.</returns>
		/// <param name="data">Audio data.</param>
		public int ProcessPartialRawAudio(short[] data)
		{
			return PocketSphinx.ps_process_raw(decoder,
			                                   data,
			                                   Convert.ToUInt32(data.Length),
			                                   false,
			                                   false);
		}

		/// <summary>
		/// Gets the number of audio data frames that have been searched by this decoder.
		/// </summary>
		/// <returns>The of frames searched.</returns>
		public int NumberOfFramesSearched()
		{
			return PocketSphinx.ps_get_n_frames(decoder);
		}

		/// <summary>
		/// Ends the current utterance.
		/// </summary>
		/// <returns><c>true</c>, if utterance was ended, <c>false</c> otherwise.</returns>
		public bool EndUtterance()
		{
			return PocketSphinx.ps_end_utt(decoder);
		}


		// calculate speech after divergence point
		// DONE : replace this just-last-word stub code with algorithm to return correct speech
		public string SpeechAfterDivergencePoint(string lastHyp, string newHyp)
		{
			string returnValue = string.Empty;
			char[] spaceChars = { ' ' }; 
			string[] lastHypWords = lastHyp.Split (spaceChars);
			string[] newHypWords = newHyp.Split (spaceChars); 

			// find the last word in common
			int i = 0;
			for (i = 0; i < lastHypWords.Length; i++) {
				if (i < newHypWords.Length) {
					if (lastHypWords [i] != newHypWords [i]) {
						break;
					}
				} else {
					break;
				}
			}

			string spaceString = " ";
			int numWordsToReturn = newHypWords.Length - i; 
			string[] wordsToReturn = new string[numWordsToReturn];
			for (int j = i; j < newHypWords.Length; j++) {
				wordsToReturn [j - i] = newHypWords [j];
			}
			returnValue = String.Join (spaceString, wordsToReturn);


			/***************
			string returnValue = string.Empty; 
			int lastIndexOfSpace = newHyp.LastIndexOf (' '); 
			if (lastIndexOfSpace < 0) {
				returnValue = newHyp;
			} else {
				returnValue = newHyp.Substring (lastIndexOfSpace);
			}
			****************/

		    // hyp.NewSpeech = hyp.FullHypothesis.Substring (lastIndexOfSpace); // just the last word
			return returnValue; 
		}

		/// <summary>
		/// Gets the partial hypothesis.
		/// </summary>
		/// <returns>The partial hypothesis.</returns>
		public Hypothesis GetPartialHypothesis()
		{
			bool suppressThisWord = false; 

			// introduce one-call delay to try to avoid 'flicker' where a word temporarily appears and gets credit, then disappears
			Hypothesis hyp = lastHyp;

					lastHyp.Score = 0;
					lastHyp.FullHypothesis = PocketSphinx.ps_get_hyp (decoder,
			                                        ref lastHyp.Score);
					lastHyp.ArrivalTime = DateTime.Now; 

			// Debug.Log ("recognized: " + lastHyp.FullHypothesis); 

			// report on effectiveness of one-call delay
			// 2015-02-27 Greg: turn the one-call delay off 
			// with the additional first/expected word distractors
			//  -- it appears to lead to getting stuck on short words from fluent readings more often than it rules out short words
			// may want to consider setting the language model weight for grownup models a little lower (say, 8) 
			// if hallucinations are still a problem,
			// since setting it down to 6 helped with the children's acoustic models
#if false
			if ((hyp.FullHypothesis != null) && (lastHyp.FullHypothesis != null) && (hyp.FullHypothesis.Length > lastHyp.FullHypothesis.Length)) {
				Debug.Log("GetPartialHypothesis - " + lastHyp.FullHypothesis + " will be shorter than: " + hyp.FullHypothesis); 
				hyp.NewSpeech = string.Empty; 
				return hyp; // pretend that this was not heard - need two in a row to hear it
			}
#endif
#if false
			if ((hyp.FullHypothesis != null) && (lastHyp.FullHypothesis != null) && (hyp.FullHypothesis != lastHyp.FullHypothesis)) {
				Debug.Log("GetPartialHypothesis - waiting for stability, " + lastHyp.FullHypothesis + " will be different than: " + hyp.FullHypothesis); 
				hyp.NewSpeech = string.Empty; 
				return hyp; // pretend that this was not heard - need two in a row to hear it
			}
#endif

					if (hyp.FullHypothesis == null) {
							hyp.FullHypothesis = string.Empty;
							hyp.NewSpeech = string.Empty;
							return hyp;
					} else if (string.IsNullOrEmpty(lastHypothesis)) {
							lastHypothesis = hyp.FullHypothesis;
							hyp.NewSpeech = hyp.FullHypothesis;

				/****** TODO modify to use the time since the last page turn *******/
							TimeSpan timeSinceLastEmptyHypothesis = hyp.ArrivalTime.Subtract (lastChangeArrivalTime); 
							double timeSinceInSeconds = timeSinceLastEmptyHypothesis.TotalSeconds; // includes whole and fractional seconds 
							if (timeSinceInSeconds < 0.001) {
								timeSinceInSeconds = 0.001; // avoid divide by zero and nonsensical values, round up to one millisecond
							}

							if (timeSinceInSeconds < 0.20) {
								//Debug.Log ("lastHypothesis was empty but " + hyp.NewSpeech + " arrived too quickly, probably from page turn, disregarding");
								hyp.NewSpeech = string.Empty; 
							} else {
								//Debug.Log ("lastHypothesis was empty, new speech is the full hypothesis: " + hyp.NewSpeech); 
							}
                  /*********************/ 

							return hyp;
					}

					// if hypothesis has not changed then there is no new speech to report
					if (hyp.FullHypothesis == lastHypothesis) {
							hyp.NewSpeech = string.Empty; 
					} else { // hypothesis has changed, so report the rightmost word
							// calculate the elapsed time since the last change
							TimeSpan elapsedTime = hyp.ArrivalTime.Subtract (lastChangeArrivalTime); 
							double elapsedTimeInSeconds = elapsedTime.TotalSeconds; // includes whole and fractional seconds 
							if (elapsedTimeInSeconds < 0.001) {
									elapsedTimeInSeconds = 0.001; // avoid divide by zero and nonsensical values, round up to one millisecond
							}
							scoreDifference = (- hyp.Score) - (- lastHypothesisScore);  // e.g. (- -1800) - (- -1400) = 200 
							double scoreDifferencePerSecond = scoreDifference / elapsedTimeInSeconds; 

							int lastIndexOfSpace = hyp.FullHypothesis.LastIndexOf (' ');

							// Debug.Log ("lastIndexOfSpace: " + lastIndexOfSpace.ToString ()); 
							if (string.IsNullOrEmpty(lastHypothesis)) { // last hypothesis was empty, report entire new hypothesis
									hyp.NewSpeech = hyp.FullHypothesis;
							} else if (lastIndexOfSpace == -1) { // only one word in the full hypothesis, report it
									hyp.NewSpeech = hyp.FullHypothesis;
							} else { // report the words in the new hypothesis after the point where the hypothesis diverges from the old one
									hyp.NewSpeech = SpeechAfterDivergencePoint (lastHypothesis, hyp.FullHypothesis); 
									// hyp.NewSpeech = hyp.FullHypothesis.Substring (lastIndexOfSpace); // just the last word
							}

							// write out the new speech / full hypothesis
							// Debug.Log ("hyp.FullHypothesis: " + hyp.FullHypothesis + " hyp.NewSpeech: " + hyp.NewSpeech); 
			
							// the difference between utterance scores from this and the last changed hypothesis can be used to "second-guess" the recognizer to provide stricter or more forgiving game behavior
							double maxScoreDifferencePerSecond = double.PositiveInfinity; 
							if (strictnessLevel == "extreme") {
									maxScoreDifferencePerSecond = extremeScoreDifferencePerSecond;
							} else if (strictnessLevel == "hard") {
									maxScoreDifferencePerSecond = hardScoreDifferencePerSecond;
							} else if (strictnessLevel == "medium") {
									maxScoreDifferencePerSecond = mediumScoreDifferencePerSecond; 
							}

							//string baseReport = "new speech: " + hyp.NewSpeech + "   score difference: " + scoreDifference.ToString () + "   elapsed seconds: " + elapsedTimeInSeconds.ToString () + "score difference per second: " + scoreDifferencePerSecond.ToString () + "  max: " + maxScoreDifferencePerSecond;
							//Debug.Log (baseReport); 


							if (scoreDifferencePerSecond > maxScoreDifferencePerSecond) {
									string NewSpeechTrimmed = hyp.NewSpeech.Trim (' '); 
									if (NewSpeechTrimmed.StartsWith ("START_")) {
											// let it go through, don't double check distractors
											//string scoreReport = "new speech: " + hyp.NewSpeech + "   score difference is high but this is a distractor, let it pass";
											//Debug.Log(scoreReport);
									} else if (NewSpeechTrimmed.LastIndexOf (' ') != -1) {
											// if there is more than one word, we can't tell which word might be wrong, let them pass
											//string scoreReport = "new speech: " + hyp.NewSpeech + "   score difference is high but there is more than one word in this update so we cannot tell which one might be wrong, let them pass";
											//Debug.Log(scoreReport);
									} else if (NewSpeechTrimmed.Length > (minToughWordLength - 1)) {
											if (doublecheckedSomeWordThisUtterance == false) {
												//string scoreReport = "new speech: " + hyp.NewSpeech + "   score difference: " + scoreDifference.ToString () + "   elapsed seconds: " + elapsedTimeInSeconds.ToString () + "score difference per second: " + scoreDifferencePerSecond.ToString () + " -- may not want to trust this word!";
												//Debug.Log (scoreReport); 
												// the hyp.NewSpeech needs to stay the same so that we don't check it twice 
												suppressThisWord = true; // suppress this word due to score higher than threshold
												doublecheckedSomeWordThisUtterance = true; 
											} else {
												//string scoreReport = "new speech: " + hyp.NewSpeech + "   score difference: " + scoreDifference.ToString () + "   elapsed seconds: " + elapsedTimeInSeconds.ToString () + "score difference per second: " + scoreDifferencePerSecond.ToString () + " -- doubtful but letting it through, one word has already been double checked";
												//Debug.Log (scoreReport); 
											}
									} else {
											//string scoreReport = "new speech: " + hyp.NewSpeech + "   score difference is high but this word is not tough, let it go"; 
											//Debug.Log (scoreReport); 
									}
							}

							// reset the hypothesis score and the arrival time of the last change 
							// but only for non-START_ words, that is, completed words. 
							// that way the score difference from SAMMY to SNAKE 
							// in SAMMY -> SAMMY START_SNAKE -> SAMMY SNAKE 
							// gets attributed to SNAKE
							if (hyp.NewSpeech.LastIndexOf ('_') == -1) {
									lastHypothesisScore = hyp.Score; 
									lastChangeArrivalTime = hyp.ArrivalTime; 
							} else {
									//string resetReport = "not resetting last hypothesis score and arrival time for " + hyp.NewSpeech; 
									// Debug.Log (resetReport); 
							}
					} 

					lastHypothesis = hyp.FullHypothesis;

					if (suppressThisWord == true) {
							hyp.NewSpeech = string.Empty;
					}
				
			return hyp;
		}

		/// <summary>
		/// Gets the final hypothesis. FullHypothesis == newSpeech in this case
		/// </summary>
		/// <returns>The final hypothesis.</returns>
		public Hypothesis GetFinalHypothesis()
		{
			Hypothesis hyp;
			hyp.Score = 0;
			hyp.FullHypothesis = PocketSphinx.ps_get_hyp(decoder,ref hyp.Score);
			hyp.NewSpeech = hyp.FullHypothesis;
			hyp.ArrivalTime = DateTime.Now;

			lastHypothesis = string.Empty;
			lastHyp = hyp; 
			return hyp;
		}

		/// <summary>
		/// Determines whether speech was recognized when processing audio data last. 
		/// This occurs during the use of ProcessPartialRawAudio.
		/// </summary>
		/// <returns><c>true</c> if new speech was detectedh; otherwise, <c>false</c>.</returns>
		public bool NewSpeechDetected()
		{
			return PocketSphinx.ps_get_in_speech(decoder);
		}
	}
}
