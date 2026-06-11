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
using Rrtf.Sphinx;

namespace Rrtf
{
	/// <summary>
	/// Heard word. Used by ASR/FSGs. Not really sure how it works and don't want to know so I didn't bother
	/// to clean it up. Be careful with it =P
	/// </summary>
	public class HeardWord 
	{
		
		public string 	 hypWord;			// hypothesis word text, no pronunciation indicators
		public int       iSentenceWord;		// index of aligned sentence word, -1 if none
		public int   	 matchLevel;		// degree of match to sentence word coded as follows:
		
		public const int MATCH_UNKNOWN = 0;		// default value: no info
		public const int MATCH_MISCUE = 1;		// heard wrong word
		public const int MATCH_TRUNCATION = 2;	// heard prefix
		public const int MATCH_EXACT = 3;		// heard exact match
		
		public HeardWord(string asrWord) 
		{
			hypWord = HeardWord.taggedWordText(asrWord);			// strip any pronunciation tags
			iSentenceWord = -1;
			matchLevel = MATCH_UNKNOWN;
		}
		
		// strip word text from possible parenthesized alternate pronunciation tag in sphinx result words
		public static string taggedWordText(string taggedWord) {
			int iParen = taggedWord.IndexOf('(');
			return (iParen >= 0) ? taggedWord.Substring(0, iParen) : taggedWord;
		}
		
		// true if asrWord is an exact match to sentence word
		public static bool asrWordMatches(string asrWord, string sentenceWord) {
			return taggedWordText(asrWord) == sentenceWord;
		}
		
		// true if asrWord is a truncation of sentence word
		public static bool asrWordIsTruncationOf(string asrWord, string sentenceWord) {
			return taggedWordText(asrWord) == ("START_" + sentenceWord);
		}

		private class mmScore 
		{	// record kept for one possible word alignment
			public int cost;			// penalty for this alignment
			public int nMatches;		// number of word matches for this alignment
			public int iPrev;			//  sentence index of previous hyp word's alignment
			
			public mmScore(int inCost, int inMatches, int inPrev) {
				cost = inCost;
				nMatches = inMatches;
				iPrev = inPrev;
			}
			
			public mmScore() {			// init to very high score before searching for minimum
				cost = 100000;
				nMatches = 0;
				iPrev = -1;
			}
		}
		
		// cost for mismatch hypWord with sentenceWord
		private static int mismatchCost(string hypWord, string sentenceWord) {
			if (HeardWord.asrWordMatches(hypWord, sentenceWord)) 
				return 0;
			
			if (HeardWord.asrWordIsTruncationOf(hypWord, sentenceWord)) 
				return 0;
			
			// else mismatch
			return 100;
		}
		
		// cost of jump from position i to j
		private static int jumpCost(int from, int to) {	
			if (to == from + 1)
				return 0;
			if (to == from)		// small cost so advancing over HO HO beats repeating HO
				return 1;
			return 100;
		}

		/// <summary>
		/// Dos the multi match.
		/// </summary>
		/// <returns>The multi match.</returns>
		/// <param name="hypWords">Hyp words.</param>
		/// <param name="sentenceWords">Sentence words.</param>
		/// <param name="wordIndex">The index we are currently at in the sentence</param>
		private HeardWord[] doMultiMatch(string[] hypWords, string[] sentenceWords, int wordIndex) 
		{
			// build array or HeardWord's to hold multimatch result
			HeardWord[] wordsHeard = new HeardWord[hypWords.Length];
			
			// store scores in matrix, one row per hypWord with one column for each sentence position it could be aligned with
			mmScore[][] rows = new mmScore[hypWords.Length][];
			for(int i = 0; i < rows.Length; i++) rows[i] = new mmScore[sentenceWords.Length];
			mmScore best;
			
			
			for (int h = 0; h < hypWords.Length; h++) 
			{
				wordsHeard[h] = new HeardWord(hypWords[h]);		// init result element here
				
				for (int s = 0; s < sentenceWords.Length; s++) 
				{			
					int mismatchCostHere = mismatchCost(hypWords[h], sentenceWords[s]);	// match cost this position
					int matchesHere = HeardWord.asrWordMatches(hypWords[h], sentenceWords[s]) ? 1 : 0 ;
					
					if (h == 0) 
					{	// first row, no predecessor => compute jump cost from expected start word
						int cost = mismatchCostHere + jumpCost(wordIndex, s);
						rows[h][s] = new mmScore(cost, matchesHere, -1);
					} 
					else  
					{ 
						// find lowest cost we can achieve here from each possible previous hypword alignment	
						best = new mmScore();	// best found so far
						for (int j = 0; j < sentenceWords.Length; j++) {
							int cost = rows[h-1][j].cost + mismatchCostHere + jumpCost(j, s);
							int matches = rows[h-1][j].nMatches + matchesHere;
							if (cost < best.cost ||
							    (cost == best.cost && matches > best.nMatches) ||
							    (cost == best.cost && matches == best.nMatches && jumpCost(j, s) == 0)) { 	 					
								best = new mmScore(cost, matches, j);	
							}     
						}
						
						// record best value possible for this hypword alignment
						rows[h][s] = best;
					}	
				}
			}
			
			// search last row to find best possible alignment of last hypWord
			int hLast = hypWords.Length -1;
			int best_alignment = -1;
			best = new mmScore();
			for (int i = 0; i < sentenceWords.Length; i++) {
				if (rows[hLast][i].cost < best.cost ||
				    (rows[hLast][i].cost == best.cost && rows[hLast][i].nMatches > best.nMatches)) {
					best = rows[hLast][i];
					best_alignment = i;
				}
			}
			
			// follow predecessor links backwards through rows to record best alignment of each preceding hyp word 
			for (int h = hypWords.Length-1; h >= 0; --h) {
				wordsHeard[h].iSentenceWord = best_alignment;
				
				// record match type
				if  (HeardWord.asrWordMatches(hypWords[h], sentenceWords[best_alignment]))
					wordsHeard[h].matchLevel = HeardWord.MATCH_EXACT; 
				else if (HeardWord.asrWordIsTruncationOf(hypWords[h], sentenceWords[best_alignment]))
					wordsHeard[h].matchLevel = HeardWord.MATCH_TRUNCATION;
				else if (string.IsNullOrEmpty(hypWords[h]))	// sanity check
					wordsHeard[h].matchLevel = HeardWord.MATCH_MISCUE;
				
				// would also record lots of other context about hypWord here
				
				// update alignment to best predecessor alignment
				best_alignment = rows[h][best_alignment].iPrev;
			}
			
			// return the aligned word array
			return wordsHeard;
		}
	}

}