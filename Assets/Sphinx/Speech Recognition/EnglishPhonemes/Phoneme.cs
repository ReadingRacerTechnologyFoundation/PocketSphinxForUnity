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
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Text;

/**
 * Transfered from Java by Rodrigo Cano 10/7/2014
 */
namespace Sphinx.EnglishPhonemes
{
	/* 
English text to phoneme Package in Java, 
derived from the C source code written by John A. Wasser <speech@John-Wasser.com>, 
available at http://ww.John-Wasser.com/TextToSpeech/

Translated to Java by Olivier Sarrat, olivier_sarrat@hotmail.com
*/
	
	
	/*
**      English to Phoneme translation.
**
**      English.rules are made up of four parts:
**      
**              The left context.
**              The text to match.
**              The right context.
**              The phonemes to substitute for the matched text.
**
**      Procedure:
**
**              Seperate each block of letters (apostrophes included) 
**              and add a space on each side.  For each unmatched 
**              letter in the word, look through the English.rules where the 
**              text to match starts with the letter in the word.  If 
**              the text to match is found and the right and left 
**              context patterns also match, output the phonemes for 
**              that rule and skip to the next unmatched letter.
**
**
**      Special Context Symbols:
**
**              #       One or more vowels
**              :       Zero or more consonants
**              ^       One consonant.
**              .       One of B, D, V, G, J, L, M, N, R, W or Z (voiced 
**                      consonants)
**              %       One of ER, E, ES, ED, ING, ELY (a suffix)
**                      (Right context only)
**              +       One of E, I or Y (a "front" vowel)
*/
	public class Phoneme
	{
		
		public static bool isupper(char chr){
			return(!(chr < 'A' || chr > 'Z'));
		}
		
		public static bool islower(char chr){
			return(!(chr < 'a' || chr > 'z'));
		}
		
		public static bool isalpha(char chr){
			return(isupper(chr) || islower(chr));
		}
		
		public static bool isvowel(char chr)
		{
			return (chr == 'A' || chr == 'E' || chr == 'I' || 
			        chr == 'O' || chr == 'U');
		}
		
		public static bool isconsonant(char chr)
		{
			return (isupper(chr) && !isvowel(chr));
		}
		
		private static string xlate_word(string word)
		{
			int index;      /* Current position in word */
			int type;       /* First letter of match part */
			int wordLength = word.Length;
			
			string phoneme = string.Empty;
			int indexRule;
			
			index = 1;      /* Skip the initial blank */
			do
			{
				if (isupper(word[index]))
					type = word[index] - 'A' + 1;					
				else
					type = 0;
				
				indexRule = find_rule(word, index, English.rules[type]);
				
				if(indexRule == -1)
					index++;
				else{
					phoneme = phoneme + English.rules[type][indexRule][English.OUT_PART];
					index += English.rules[type][indexRule][English.MATCH_PART].Length;
				}
			}
			while (index < wordLength);
			return phoneme;
		}
		
		private static int find_rule(string word, int index, string[][] chosenRules)
		{
			string[] rule;
			int indexRule = 0;
			string left, match, right;
			int indexMatch = 0;
			int remainder;
			int wordLength = word.Length;
			
			for (;;)        /* Search for the rule */
			{
				rule = chosenRules[indexRule];
				indexRule++;
				match = rule[English.MATCH_PART];
				
				if (match == "!%@$#") /* bad symbol! */
				{
					//System.err.println("Error: Can't find rule for: "+word[index)+" in "+word);
					return -1; /* Skip it! */
				}
				for (remainder = index, indexMatch = 0; (indexMatch != match.Length) && (remainder != wordLength); indexMatch++,remainder++){
					if (match[indexMatch] != word[remainder])
						break;
				}
				
				if (indexMatch != match.Length)     /* found missmatch */
					continue;
				
				left = rule[English.LEFT_PART];
				right = rule[English.RIGHT_PART];
				
				if (!leftmatch(left, word, index-1))
					continue;
				if (!rightmatch(right, word, remainder))
					continue;
				
				return --indexRule;
			}
		}
		
		
		private static bool leftmatch(string pattern, /* pattern to match in text */
		                                 string context, /*text to be matched */
		                                 int indexText)/* index of last char of text to be matched */
		{
			int pat;
			//string text;
			int count;
			
			if (string.IsNullOrEmpty(pattern))   /* null string matches any context */
			{
				return true;
			}
			
			/* point to last character in pattern string */
			count = pattern.Length;
			pat = count - 1;
			
			for (; count > 0; pat--, count--)
			{
				/* First check for simple text or space */
				if (isalpha(pattern[pat]) || pattern[pat] == '\'' || pattern[pat] == ' ')
					if (pattern[pat] != context[indexText])
						return false;
				else
				{
					indexText--;
					continue;
				}
				
				char carpat = pattern[pat];
				if(carpat == '#'){
					/* One or more vowels */
					if (!isvowel(context[indexText]))
						return false;
					
					indexText--;
					
					while (isvowel(context[indexText]))
						indexText--;
				} else if(carpat == ':'){
					/* Zero or more consonants */
					while (isconsonant(context[indexText]))
						indexText--;
				} else if(carpat == '^'){
					/* One consonant */
					if (!isconsonant(context[indexText]))
						return false;
					indexText--;
				} else if(carpat == '.'){
					/* B, D, V, G, J, L, M, N, R, W, Z */
					if (context[indexText] != 'B' && context[indexText] != 'D' && context[indexText] != 'V'
					    && context[indexText] != 'G' && context[indexText] != 'J' && context[indexText] != 'L'
					    && context[indexText] != 'M' && context[indexText] != 'N' && context[indexText] != 'R'
					    && context[indexText] != 'W' && context[indexText] != 'Z')
						return false;
					indexText--;
				} else if(carpat == '+'){
					/* E, I or Y (front vowel) */
					if (context[indexText] != 'E' && context[indexText] != 'I' && context[indexText] != 'Y')
						return false;
					indexText--;
				} else {
					Debug.LogError("Bad char in left rule: '"+pattern[pat]+"'");
					return false;
				}
			}
			
			return true;
		}
		
		
		private static bool rightmatch(string pattern, /* pattern to match in text */
		                                  string context, /*text to be matched */
		                                  int indexText)/* index of last char of text to be matched */
		{
			if (string.IsNullOrEmpty(pattern))   /* null string matches any context */
				return true;
			
			int pat = 0;
			for (pat = 0; pat != pattern.Length; pat++){
				/* First check for simple text or space */			
				if (isalpha(pattern[pat]) || pattern[pat] == '\'' || pattern[pat] == ' ')
					if (pattern[pat] != context[indexText])
						return false;
				else
				{
					indexText++;
					continue;
				}
				char carpat = pattern[pat];
				if(carpat == '#'){
					/* One or more vowels */
					if (!isvowel(context[indexText]))
						return false;
					
					indexText++;
					
					while (isvowel(context[indexText]))
						indexText++;
				} else if(carpat == ':'){
					/* Zero or more consonants */
					while (isconsonant(context[indexText]))
						indexText++;
				} else if(carpat == '^'){
					/* One consonant */
					if (!isconsonant(context[indexText]))
						return false;
					indexText++;
				} else if(carpat == '.'){
					/* B, D, V, G, J, L, M, N, R, W, Z */
					if (context[indexText] != 'B' && context[indexText] != 'D' && context[indexText] != 'V'
					    && context[indexText] != 'G' && context[indexText] != 'J' && context[indexText] != 'L'
					    && context[indexText] != 'M' && context[indexText] != 'N' && context[indexText] != 'R'
					    && context[indexText] != 'W' && context[indexText] != 'Z')
						return false;
					indexText++;
				} else if(carpat == '+'){
					/* E, I or Y (front vowel) */
					if (context[indexText] != 'E' && context[indexText] != 'I' && context[indexText] != 'Y')
						return false;
					indexText++;
				} else if(carpat == '%'){
					/* ER, E, ES, ED, ING, ELY (a suffix) */
					if (context[indexText] == 'E')
					{
						indexText++;
						if (context[indexText] == 'L')
						{
							indexText++;
							if (context[indexText] == 'Y')
							{
								indexText++;
								break;
							}
							else
							{
								indexText--; /* Don't gobble L */
								break;
							}
						}
						else
							if (context[indexText] == 'R' || context[indexText] == 'S' 
							    || context[indexText] == 'D')
								indexText++;
						break;
					}
					else
						if (context[indexText] == 'I')
					{
						indexText++;
						if (context[indexText] == 'N')
						{
							indexText++;
							if (context[indexText] == 'G')
							{
								indexText++;
								break;
							}
						}
						return false;
					}
					else
						return false;
				} else {
					Debug.LogError("Bad char in right rule:'"+pattern[pat]+"'");
					//System.err.println("Bad char in right rule:'"+pattern[pat)+"'");
					return false;
				}
			}
			
			return true;
		}


		private static string xlate_num0To99(int number)
		{
			if(number < 0 || number > 99)
			{
				Debug.LogError("xlating number out of range 0-99: " + number);
				number = 99;
			}
		
			if(number == 0)
				return EnglishPhonemes.Numbers.ZERO;
			if(number < 10)
				return EnglishPhonemes.Numbers.UNITS[number];
			if(number < 20)
				return EnglishPhonemes.Numbers.TEENS[number - 10];

			int tens = number / 10;
			int ones = number % 10;

			string output = EnglishPhonemes.Numbers.TENS[tens];

			if(ones != 0)
				output += " " + EnglishPhonemes.Numbers.UNITS[ones];

			return output;
		}

		/// <summary>
		/// xlates the numbers 100 to 999
		/// </summary>
		/// <param name="number">Number to xlate</param>
		/// <param name="outputs">the stringbuilders to write to</param>
		/// <param name="start">where to start writing to. may take up to two spots</param>
		/// Those spaces begin at extraspacestart </param>
		/// returns the number of spots written to
		private static int xlate_num100To999(int number, StringBuilder[] outputs, int start)
		{
			if(number < 100 || number > 999)
			{
				Debug.LogError("xlating number out of range 100-999: " + number);
				number = 999;
			}

			int hundreds = number / 100;
			int tens = number % 100;

			outputs[start + 1].Append(outputs[start].ToString());
			outputs[start].Append(' ').Append(EnglishPhonemes.Numbers.UNITS[hundreds]).
				Append(' ').Append(EnglishPhonemes.Numbers.HUNDRED);

			if(tens == 0)
			{
				return 1;
			}

			outputs[start].Append(' ').Append(xlate_num0To99(tens));

			//option for 503 to become 5 o' three
			if(tens < 10)
			{
				outputs[start + 1].Append(' ').Append(EnglishPhonemes.Numbers.UNITS[hundreds])
					.Append(' ').Append(EnglishPhonemes.Numbers.OH).Append(' ')
					.Append(EnglishPhonemes.Numbers.UNITS[tens]);

			}
			else//tens > 9 option for 523 becomes five twenty three
			{
				outputs[start + 1].Append(' ').Append(EnglishPhonemes.Numbers.UNITS[hundreds])
					.Append(' ').Append(xlate_num0To99(tens));
			}

			return 2;
		}
		
		//checks if a number is a valid 4 digit year
		private static bool isYearNumber(int number)
		{
			//spliting a nmuber like 1826 to 18 and 26
			int head = number / 100;
			int tail = number % 100;

			return (head > 9) && tail >= 0;
		}

		//number must be a 4 digit year. ie. 1826.
		/// <summary>
		/// Xlate a number that represents a 4 digit year
		/// </summary>
		/// <returns>The number of stringbuilders written to begining at start</returns>
		/// <param name="number">Number to xlate</param>
		/// <param name="outputs">the stringbuilders to write to</param>
		/// <param name="start">the starting write position</param>
		private static int xlate_yearNumber(int number, StringBuilder[] outputs, int start)
		{
			if(!isYearNumber(number))
			{
				Debug.Log("Cannot convert number to a year number: " + number);
				return 0;
			}

			int head = number / 100;
			int tail = number % 100;

			if(tail == 0)
			{
				outputs[start].Append(' ').Append(xlate_num0To99(head)).Append(' ')
					.Append(EnglishPhonemes.Numbers.HUNDRED);
				return 1;
			}
			if(tail < 10)
			{
				outputs[start].Append(' ').Append(xlate_num0To99(head)).Append(' ')
					.Append(EnglishPhonemes.Numbers.OH).Append(' ').Append(EnglishPhonemes.Numbers.UNITS[tail]);
				return 1;
			}

			outputs[start].Append(' ').Append(xlate_num0To99(head)).Append(' ')
				.Append(xlate_num0To99(tail));

			outputs[start + 1].Append(' ').Append(xlate_num0To99(head)).Append(' ')
				.Append(EnglishPhonemes.Numbers.HUNDRED).Append(' ').Append(xlate_num0To99(tail));


			return 2;
		}

		/// <summary>
		/// Xlate_numbers from 1000 to 9999.
		/// </summary>
		/// <param name="number">Number.</param>
		/// <param name="outputs">stringbuilders to append to/write to.</param>
		/// <param name="start">The first string builder</param>
		/// <param name="count">The number of stringbuilders to write to starting at start</param>
		/// returns how many stringbuilders were written to
		private static int xlate_num1000To9999(int number, StringBuilder[] outputs, int start)
		{
			if(number < 1000 || number > 9999)
			{
				Debug.LogError("xlating number out of range 1000-9999: " + number);
				number = 9999;
			}

			int thous = number / 1000;
			int rest = number % 1000;

			outputs[start].Append(' ').Append(EnglishPhonemes.Numbers.UNITS[thous]).
					Append(' ').Append(EnglishPhonemes.Numbers.THOUSAND);

			if(rest == 0)
			{
				return 1;
			}

			if(rest > 99)
			{
				return xlate_num100To999(rest, outputs,start);
			}
			else
			{
				outputs[start].Append(' ').Append(xlate_num0To99(rest));
				return 1;
			}
		}

		public static string[] xlate_number(string num)
		{
			int number;
			string suffix = string.Empty;//for things like 8th 
			try
			{
				//for taking into account things like 1st or 88s
				if(num.Length > 2 && 
				   char.IsLetter(num[num.Length - 1]) && char.IsLetter(num[num.Length - 2]))
				{
					suffix = xlate_word(" " + num.Substring(num.Length - 2) + " ").TrimStart();
					num = num.Substring(0,num.Length-2);
				}
				else if(num.Length > 1 && char.IsLetter(num[num.Length - 1]))
				{
					suffix = xlate_word(" " + num.Substring(num.Length - 1) + " ").TrimStart();
					num = num.Substring(0,num.Length-1);
				}

				number = Convert.ToInt32(num);
			}
			catch(FormatException e)
			{
				Debug.LogError("Error converting -" + num + "-");
				Debug.LogException(e);
				throw e;
			}

			StringBuilder[] outputs = new StringBuilder[] 
				{new StringBuilder(string.Empty), new StringBuilder(string.Empty), new StringBuilder(string.Empty), 
				 new StringBuilder(string.Empty), new StringBuilder(string.Empty), new StringBuilder(string.Empty)};

			int start = 0;
			if(number > 9)
			{
				foreach(char n in num)
					outputs[start].Append(' ').Append(EnglishPhonemes.Numbers.UNITS[Convert.ToInt32(n.ToString())]);

				outputs[start].Append(' ');
				++start;
			}


			if(number > 9999)
			{
				return new string[] { outputs[start].ToString() };
			}
			else if(number > 999)
			{
				if(isYearNumber(number))
					start += xlate_yearNumber(number,outputs,start);

				start += xlate_num1000To9999(number,outputs,start);
			}
			else if(number > 99)
			{
				start += xlate_num100To999(number,outputs, start);
			}
			else
			{
				outputs[start].Append(' ').Append(xlate_num0To99(number));
				++start;
			}

			string[] outStrings = new string[start];
			for(int i = 0; i < start;++i)
				outStrings[i] = outputs[i].Append(' ').Append(suffix).ToString();

			return outStrings;
		}

		private static readonly Regex CONTAINS_NUMBER = new Regex(@"\d");
		public static string[] toPhoneme(string text)
		{
			try
			{
				if(CONTAINS_NUMBER.IsMatch(text))
				{
					return xlate_number(text);
				}

				return(new string[] {xlate_word(" "+text.ToUpper()+" ")});
			}
			catch(Exception e)
			{
				Debug.LogError("Something is making toPhenome() crash: -" + text + "-");
				throw e;
			}
		}
	}
}

