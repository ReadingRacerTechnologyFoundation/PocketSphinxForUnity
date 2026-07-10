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
using System;

namespace Rrtf.Sphinx
{
	using SphinxNative;
    using UnityEngine;

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
		public ps_decoder_t RawDecoder { get { return decoder; } }

		private ps_decoder_t decoder;
		private string lastHypothesis = string.Empty;
		private DateTime lastChangeArrivalTime = DateTime.Now;

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
			return PocketSphinx.ps_reinit(decoder, newConfig.Config);
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
			return PocketSphinx.ps_load_dict(decoder, dictPath, null, null);
		}

		/// <summary>
		/// Loads a new dictionary. Overwriting the one used previously, possibly set by SpeechConfig.
		/// </summary>
		/// <returns><c>true</c>, if new dictionary was loaded, <c>false</c> otherwise.</returns>
		/// <param name="dictPath">The Dictionory File Path</param>
		/// <param name="fillerDictPath">The filler dictionray file path.</param>
		public bool LoadNewDictionary(string dictPath, string fillerDictPath)
		{
			return PocketSphinx.ps_load_dict(decoder, dictPath, fillerDictPath, null);
		}

		/// <summary>
		/// Saves the dictionary to file.
		/// </summary>
		/// <returns><c>true</c>, if dictionary to file was saved, <c>false</c> otherwise.</returns>
		/// <param name="dictPath">The path of the new dictionary file</param>
		public bool SaveDictionaryToFile(string dictPath)
		{
			return PocketSphinx.ps_save_dict(decoder, dictPath, null);
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
			isAddedSuccesfully = PocketSphinx.ps_add_word(decoder, word, phones[0].Trim(), true) >= 0;
			//Debug.Log("Added: " + word + "(" + (0+1) + ")" + "  --as " + phones[0].Trim());
			for (int i = 1; i < phones.Length; ++i)
			{
				isAddedSuccesfully = isAddedSuccesfully && PocketSphinx.ps_add_word
					(decoder, word + "(" + (i + 1) + ")",
					 phones[i].Trim(), true) >= 0;
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
			return PocketSphinx.ps_add_word(decoder, word, phones, true) >= 0;
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
			return PocketSphinx.ps_lookup_word(decoder, word);
		}

		/// <summary>
		/// Starts the utterance.
		/// </summary>
		/// <returns><c>true</c>, if utterance was started, <c>false</c> otherwise.</returns>
		/// <param name="inStrictnessLevel">Provide a way to specify the strictness of the score change criterion</param>
		public bool StartUtterance()
		{
			lastHypothesis = string.Empty;

			lastChangeArrivalTime = DateTime.Now;
			return PocketSphinx.ps_start_utt(decoder);
		}

		/// <summary>
		/// Processes the partial raw audio. A whole utterance is not expected when using this.
		/// Not a problem IF a whole utterance is processed when using this.
		/// </summary>
		/// <returns>The partial raw audio.</returns>
		/// <param name="data">Audio data.</param>
		public int ProcessPartialRawAudio(short[] data, int length)
		{
			length = Mathf.Min(data.Length, length);
			return PocketSphinx.ps_process_raw(decoder,
											   data,
											   (uint)length,
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
			string[] lastHypWords = lastHyp.Split(spaceChars);
			string[] newHypWords = newHyp.Split(spaceChars);

			// find the last word in common
			int i = 0;
			for (i = 0; i < lastHypWords.Length; i++)
			{
				if (i < newHypWords.Length)
				{
					if (lastHypWords[i] != newHypWords[i])
					{
						break;
					}
				}
				else
				{
					break;
				}
			}

			string spaceString = " ";
			int numWordsToReturn = newHypWords.Length - i;
			string[] wordsToReturn = new string[numWordsToReturn];
			for (int j = i; j < newHypWords.Length; j++)
			{
				wordsToReturn[j - i] = newHypWords[j];
			}
			returnValue = String.Join(spaceString, wordsToReturn);

			return returnValue;
		}

		/// <summary>
		/// Gets the partial hypothesis.
		/// </summary>
		/// <returns>The partial hypothesis.</returns>
		public Hypothesis GetPartialHypothesis()
		{

			// introduce one-call delay to try to avoid 'flicker' where a word temporarily appears and gets credit, then disappears
			Hypothesis hyp = new Hypothesis();
			hyp.SetToEmpy();
			hyp.FullHypothesis = PocketSphinx.ps_get_hyp(decoder,
											ref hyp.Score);

			if (hyp.FullHypothesis == null)
			{
				hyp.FullHypothesis = string.Empty;
				hyp.NewSpeech = string.Empty;
				return hyp;
			}
			else if (string.IsNullOrEmpty(lastHypothesis))
			{
				lastHypothesis = hyp.FullHypothesis;
				hyp.NewSpeech = hyp.FullHypothesis;

				/****** TODO modify to use the time since the last page turn *******/
				TimeSpan timeSinceLastEmptyHypothesis = hyp.ArrivalTime.Subtract(lastChangeArrivalTime);
				double timeSinceInSeconds = timeSinceLastEmptyHypothesis.TotalSeconds; // includes whole and fractional seconds
				timeSinceInSeconds = Math.Max(0.001, timeSinceInSeconds);

				if (timeSinceInSeconds < 0.20)//avoid sudden new hypothesis that come in too quick.
				{
					hyp.NewSpeech = string.Empty;
				}

				return hyp;
			}

			// if hypothesis has not changed then there is no new speech to report
			if (hyp.FullHypothesis == lastHypothesis)
			{
				hyp.NewSpeech = string.Empty;
			}
			else
			{ 
				int lastIndexOfSpace = hyp.FullHypothesis.LastIndexOf(' ');

				if (string.IsNullOrEmpty(lastHypothesis))
				{ // last hypothesis was empty, report entire new hypothesis
					hyp.NewSpeech = hyp.FullHypothesis;
				}
				else if (lastIndexOfSpace == -1)
				{ // only one word in the full hypothesis, report it
					hyp.NewSpeech = hyp.FullHypothesis;
				}
				else
				{ // report the words in the new hypothesis after the point where the hypothesis diverges from the old one
					hyp.NewSpeech = SpeechAfterDivergencePoint(lastHypothesis, hyp.FullHypothesis);
				}

				//start words are words that someone begins to say but doesn't fully say like
				//START_DINOSAUR might only say "DINO". so if we don't find a _ in the hyp that means no start words
				//and we can fully trust this hypothesis
				if (hyp.NewSpeech.LastIndexOf('_') == -1)
				{
					lastChangeArrivalTime = hyp.ArrivalTime;
				}
			}

			lastHypothesis = hyp.FullHypothesis;

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
			hyp.FullHypothesis = PocketSphinx.ps_get_hyp(decoder, ref hyp.Score);
			hyp.NewSpeech = hyp.FullHypothesis;
			hyp.ArrivalTime = DateTime.Now;

			lastHypothesis = string.Empty;
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
