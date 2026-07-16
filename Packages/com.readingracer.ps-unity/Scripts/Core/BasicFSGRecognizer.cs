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
using System.Collections.Generic;
using Rrtf.Sphinx;
using System.Text.RegularExpressions;
using System;
using System.Text;

namespace Rrtf
{

    /// <summary>
    /// A SpeechRecognizer that specializes in Finite State Grammars (FSGs)
    /// </summary>
    public sealed class BasicFSGRecognizer : SpeechRecognizer
    {
        /// <summary>
        /// The Default FSG search name.
        /// </summary>
        private static readonly string LISTEN_FOR_SEARCH_NAME = "DefaultFsgListenFor";

        /// <summary>
        /// The minimum Language Model Weight (LM weight)
        /// </summary>
        public const int MIN_LANGUAGE_MODEL_WEIGHT = 0;

        /// <summary>
        /// The maximum language model weight (LM Weight)
        /// </summary>
		public const int MAX_LANGUAGE_MODEL_WEIGHT = 9;

        /// <summary>
        /// the index of the last word that was read. This indexes into the array returned by ListenFor
        /// </summary>
        public int WordIndex { get; private set; }

        private readonly LogMath logmath;
        private readonly int LmModelWeight = 0;

        private string[] wordSet;

        /// <summary>
        /// In order to reduce false positives with longer words, we have added START words. For words like DINOSAUR, we add words like
        /// START_DINOSAUR could be just the partial word DINO. start words only get added if they have MIN_STARTWORD_PHONEMES or more
        /// </summary>
        private const int MIN_STARTWORD_PHONEMES = 4;

        /// <summary>
        /// In the original game <br> was used to indicate a line break. it gets ignored in the FSG
        /// </summary>
        public const string LINE_BREAK = "<br>";

        /// <summary>
        /// Initializes a new instance of the <see cref="SeaShells.SeaShellsRecognizer"/> class.
        /// The defaultLmWeight is used on FSGs only
        /// </summary>
        /// <param name="defaultLmWeight">Default language model weight. must be between 0 and 9. Automatcily clamped</param>
        public BasicFSGRecognizer(int defaultLmWeight, SpeechConfig config) : base(config)
        {
            this.logmath = new LogMath();
            this.LmModelWeight = Mathf.Clamp(defaultLmWeight, MIN_LANGUAGE_MODEL_WEIGHT, MAX_LANGUAGE_MODEL_WEIGHT);
        }

        /// <summary>
        /// Listens for the words in a sentence using an FSG.
        /// Automatically starts the recognizer when this is called using the search SeaShellsRecognizer.FSG_DEFAULT_SEARCH
        /// </summary>
        /// <returns> The words the recognizer is listening to AND in the format they are being heard</returns>
        /// <param name="sentence">Sentence.</param>
        /// <param name="startIndex">Start index. - The starting word</param>
        public string[] ListenFor(string sentence, int startIndex)
        {
            return ListenFor(sentence, this.LmModelWeight, startIndex, 0.5, false);
        }

        /// <summary>
        /// Tell the decoder to listen for a sentence using an FSG (Finite State Grammar)
        /// WORDS MUST EXIST IN THE DICTIONARY. The default dictionary is DefaultStoryList
        /// </summary>
        /// <param name="sentence">The Sentence to listen for</param>
        /// <param name="lmModelWeight">A 0-9 value. The higher the value, the more it will depend on the language model</param>
        /// <param name="startIndex">Word index to start listening to</param>
        /// <param name="forgivenessBalance">A 0-1 value. How easy it is to hear the next word, with 1 being the easiest</param>
        /// <param name="isCloze">By default, the expected word order is the order of the sentence. However, with cloze on, any word can be said in any order</param>
        /// <returns>the list of sanitized words we are listening to</returns>
        public string[] ListenFor(string sentence, int lmModelWeight, int startIndex, double forgivenessBalance, bool isCloze)
        {
            //Debug.Log("forgiveness set: " + forgivenessBalance);
            if (IsListening)
                this.stopRecognizing();

            string[] searchWords = TextToWords(sentence);
            lmModelWeight = Mathf.Clamp(lmModelWeight, MIN_LANGUAGE_MODEL_WEIGHT, MAX_LANGUAGE_MODEL_WEIGHT);
            startIndex = Mathf.Clamp(startIndex, 0, searchWords.Length - 1);

            bool setFSGsuccess = false;
            if (isCloze)
            {
                setFSGsuccess = setFSGforCloze(BasicFSGRecognizer.LISTEN_FOR_SEARCH_NAME, searchWords, startIndex, lmModelWeight, forgivenessBalance);
            }
            else
            {
                setFSGsuccess = setFSG(BasicFSGRecognizer.LISTEN_FOR_SEARCH_NAME, searchWords, startIndex, lmModelWeight, forgivenessBalance);
            }

            if (!setFSGsuccess)
            {
                Debug.LogError("Failed to set FSG");
                return null;
            }

            this.wordSet = searchWords;
            this.WordIndex = startIndex;

            return wordSet;
        }

        /// <summary>
        /// Enables the mic and sets the search. Calls SpeechRecognizer.Recognize internally. This will use the state setup by ListenFor
        /// </summary>
        public void EnableMicAndRecognize()
        {
            StartRecognizing(LISTEN_FOR_SEARCH_NAME);
        }

        //switching to precompiled regex for supposive 30% speed boost.
        private static readonly Regex SPACE_HYPHENS_SPACE = new Regex(@"\s+\-+\s+");
        private static readonly Regex JUNX_REMOVAL = new Regex("[.!?,:;\"\\()'“]");
        private static readonly Regex NON_ALPHA_NUMERIC_OR_HYPHEN = new Regex("[^a-zA-Z0-9-]");
        private const string SPACE = " ";
        /// <summary>
        /// tokenizes a sentence to only words
        /// </summary>
        /// <returns>The to words.</returns>
        /// <param name="sentence">Sentence.</param>
        public static string[] TextToWords(string sentence, bool isRemovingHypthens = true)
        {
            //strip word-final or -initial apostrophes as in Peoples' or 'cause.
            //Currently assuming hyphenated expressions split into two Asr words.
            if (isRemovingHypthens)
                sentence = sentence.Replace("-", string.Empty);

            sentence = SPACE_HYPHENS_SPACE.Replace(sentence, SPACE);
            sentence = sentence.Replace(LINE_BREAK, SPACE);
            //seperates things like "woah...dude" to be "woah   dude"
            sentence = sentence.Replace("...", SPACE);
            sentence = JUNX_REMOVAL.Replace(sentence, String.Empty);

            //remove periods and replace with space ONLY if its at the end of a sentence.

            string[] words = sentence.ToUpper().Split(new char[0], System.StringSplitOptions.RemoveEmptyEntries);
            //System.Text.StringBuilder nToS = new System.Text.StringBuilder();
            for (int i = 0; i < words.Length; i++)
            {
                string current = words[i];

                //string ss = current;
                //if we are removing hyphens then they have already been removed above
                current = NON_ALPHA_NUMERIC_OR_HYPHEN.Replace(current, string.Empty);

                //removing trailing or leading hyphens
                if (current[0] == '-' || current[current.Length - 1] == '-')
                    current = current.Replace("-", string.Empty);

                //if(current.ToUpper() == "TWENTYFIVE") Debug.Log ("IT WAS: " + before);

                if (string.IsNullOrEmpty(current))
                    Debug.LogError("Parsed word changed to empty string: " + words[i]);

                words[i] = current;
            }

            return words;
        }

        /// <summary>
        /// Gets the word count in a sentence. getWordCount() == ListenFor(sentence).Length
        /// So use this if you need to know the wourdCount before you call ListenFor
        /// </summary>
        /// <returns>The word count.</returns>
        /// <param name="sentence">Sentence.</param>
        public static int GetWordCount(string sentence)
        {
            sentence = sentence.Replace('.', ' ');
            sentence = sentence.Replace('-', ' ');
            sentence = JUNX_REMOVAL.Replace(sentence, string.Empty);

            if (string.IsNullOrEmpty(sentence)) return 0;

            int count = 1;
            int i = 0;
            while (char.IsWhiteSpace(sentence[i])) ++i;

            for (; i < sentence.Length; i++)
            {
                if (char.IsWhiteSpace(sentence[i]))
                {
                    while (i < sentence.Length && char.IsWhiteSpace(sentence[i])) ++i;

                    if (i < sentence.Length) ++count;
                }
            }

            return count;
        }


        //heuroistics to try and make the first word a little less easy to detect incorrectly
        private static readonly string[] firstWordDistractorsArray = { "MISC_DH_EH", "MISC_ZH_IY", "MISC_HH_EH", "MISC_AA", "MISC_AW", "MISC_OY", "MISC_IY", "MISC_UW", "MISC_EH", "MISC_OW", "MISC_DH", "MISC_K", "MISC_T", "MISC_SH", "MISC_ZH", "MISC_NG", "MISC_R", "MISC_L", "MISC_M", "MISC_N" };
        private static readonly string[] firstWordDistractorsPron = { "DH EH", "ZH IY", "HH EH", "AA", "AW", "OY", "IY", "UW", "EH", "OW", "DH", "K", "T", "SH", "ZH", "NG", "R", "L", "M", "N" };


        // special case for cloze 

        /// <summary>
        /// Creates and sets the recognizer to use an FSG.
        /// </summary>
        /// <returns><c>true</c>, if succesful <c>false</c> otherwise.</returns>
        /// <param name="searchName">Search name.</param>
        /// <param name="searchWords">Search words.</param>
        /// <param name="startIndex">Start index of the words in searchWords</param>
        /// <param name="lmModelWeight">Lm model weight.</param>
        private bool setFSGforCloze(string searchName, string[] searchWords, int startIndex, int lmModelWeight, double forgivenessBalance)
        {
            if (forgivenessBalance < 0.01)
            {
                forgivenessBalance = 0.01;
            }
            else if (forgivenessBalance > 0.99)
            {
                forgivenessBalance = 0.99;
            }

            //Debug.Log ("setFSGForCloze - forgivenessBalance is set to " + forgivenessBalance);

            HashSet<string> uniques = new HashSet<string>(searchWords);

            // ensure all sentence words in dictionary
            foreach (string word in uniques)
            {

                if (this.TheDecoder.LookUpWord(word) == null)
                {   //Debug.Log("word not in dic " + "'" + word + "'");
                    this.TheDecoder.AddWord(word); // more efficient to pass 1 (true) on last word only?
                                                   // could add START_ words for truncated readings as well
                                                   // but in the cloze word case let's not do this since cloze is working well. 2/9/2015 Greg
                }
            }

            System.Text.StringBuilder commands = new System.Text.StringBuilder();

            // This is the dial to use to set the overall forgiveness/strictness. Range is [epsilon, 1-epsilon]
            // Limited to not quite zero since zero values would cause problems in the recognizer (perhaps crashes)
            // code for Jan 23 2015 demo was equivalent to a setting of 0.5
            const double PrEpsilon = 0.000001;
            double PrForgivenessBalance = 0.5;
            if (forgivenessBalance < PrEpsilon)
            {
                PrForgivenessBalance = PrEpsilon;
            }
            else if (forgivenessBalance > (1.0 - PrEpsilon))
            {
                PrForgivenessBalance = 1.0 - PrEpsilon;
            }
            else
            {
                PrForgivenessBalance = forgivenessBalance;
            }

            // write the fsg file header info 
            int state_count = 2; // for cloze model, much simpler // searchWords.Length + 1;
            int start_state = 0; // start at beginning startIndex;		// normally 0 for first, but can be anywhere
            int final_state = 1; // state_count -1;

            //never used warning was showing up
            //double normPrExtraDistractorsBeforeCloze = PrForgivenessBalance / (clozeDistractorsArray.Length + 1);
            //double PrSkipDistractor = 1.0 - PrForgivenessBalance; never used warning was showing up
            double normPrCorrectClozeWords = PrForgivenessBalance / searchWords.Length;
            double normPrExtraDistractorsAtCloze = PrForgivenessBalance / searchWords.Length;
            double PrSkipClozeWords = 1.0 - PrForgivenessBalance;
            double PrRestart = 1.0;
            //WARNING WE NEED TO USE .Append(\n) because the c++ code expencts \n not \r\n like it does for windews when you use AppendLine
            commands.AppendNewline("FSG_BEGIN sentence");
            commands.AppendNewline("NUM_STATES " + state_count);
            commands.AppendNewline("START_STATE " + start_state);
            commands.AppendNewline("FINAL_STATE " + final_state);

            for (int i = 0; i < searchWords.Length; i++)
            {
                commands.AddFSGTransition(0, 1, normPrCorrectClozeWords, searchWords[i]);
            }
            commands.AddFSGTransition(0, 0, normPrExtraDistractorsAtCloze, string.Empty); // extra arc to go back
            commands.AddFSGTransition(0, 1, PrSkipClozeWords, string.Empty);

            commands.AddFSGTransition(1, 0, PrRestart, string.Empty);

            // done writing the fsg. fsg must end with a new line otherwise crash
            commands.AppendNewline("FSG_END");

            FsgModel model;
#if UNITY_EDITOR && PS_UNITY_USE_FSG_FILE
            string fsgFileName = "tempFSGcloze";
            if (System.IO.File.Exists(fsgFileName))
                System.IO.File.Delete(fsgFileName);
            System.IO.File.WriteAllLines(fsgFileName, commands.ToString().Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.None));
            model = FsgModel.CreateFromFile(fsgFileName, logmath, lmModelWeight);
#else
			model = FsgModel.CreatefromString(commands.ToString(),logmath,lmModelWeight);
#endif
            return this.AddFSGModel(model, searchName);
        }


        /// <summary>
        /// Creates a START word by returning the phonemes used for the partial word
        /// </summary>
        /// <param name="word">the word you want a start word for</param>
        /// <returns>phonemes used for a start word</returns>
        private string startWordCalculatePronunciation(string word)
        {
            string returnValue = "EH L AH F"; // default to START_ELEPHANT
                                              // return returnValue;//greg check this out before you use it

            String pronounciation = this.TheDecoder.LookUpWord(word);
            if (word == null) pronounciation = Sphinx.EnglishPhonemes.Phoneme.toPhoneme(word)[0];

            if (pronounciation == null)
            {
                return returnValue;
            }
            if (pronounciation.Length == 0)
            {
                return returnValue;
            }
            //if its a number its going to have multiple pronounciations....
            //not sure how to handle that for now
            string[] phonemes = pronounciation.Split(new char[0] { }, StringSplitOptions.RemoveEmptyEntries);

            if (phonemes.Length < MIN_STARTWORD_PHONEMES) return string.Empty;

            //midpoint phoneme approach
            int midPoint = (phonemes.Length / 2) - 1;//base 1 to base 0 means - 1


            const string start = "START_";
            StringBuilder sb = new StringBuilder(start.Length + word.Length + midPoint * 2 + 2);
            sb.Append(start).Append(word).Append('\t');
            for (int i = 0; i <= midPoint; ++i)
                sb.Append(phonemes[i]).Append(" ");

            return sb.ToString().TrimEnd();

            // TODO: replace stub code with the following algorithm which will calculate one START_ pronunciation for the input word
            // 1. calculate the pronunciation for word, either by looking it up in the dictionary or by using the phoneme synthesis
            // 2. split the pronunciation into an array of phonemes (to help with the subsequent steps)
            // 3. determine the number of phonemes in the pronunciation
            //    a. if number of phonemes is less than four, as in "EH L F" which has three phonemes, return empty string "".
            //       (if there are too many false rejections on short words with this constraint, try number of phonemes less than five as the check)

            // for steps 4 and 5, there are two possibilities, one based on the midpoint phoneme, and one based on the midpoint vowel
            // I would expect the midpoint vowel to be more accurate in terms of modeling what kids might actually say,
            // but the midpoint phoneme is simpler and faster to implement and in terms of the recognition accuracy might be equivalent, 
            // so I recommend trying the midpoint phoneme approach first. 
            // 
            // assume the following words for examples:
            // kitty K IH DH IY
            // simple S IH M P AH L
            // certainly S ER T AX N L IY
            // amazingly AX M EY Z IH NG L IY
            // 
            // Midpoint Phoneme
            // 4. Divide the pronunciation in half, rounding towards the beginning of the word
            // 5. calculate the START_ pronunciation as all the phonemes up to and including the midpoint phoneme
            //    for example:
            //    START_KITTY K IH
            //    START_SIMPLE S IH M
            //    START_CERTAINLY S ER T AX
            //    START_AMAZINGLY AX M EY Z
            // 
            // Midpoint Vowel
            // 4. Calculate the midpoint vowel in the word, rounding towards the beginning of the word.
            //    sphinx documentation lists all the 39 (?) phones including which ones are vowels.
            //    a. if number of vowels is less than two, as in "SH EH L F", return empty string "". 
            //    for example: 
            //    K IH DH IY median vowel is IH
            //    S IH M P AH L median vowel is IH
            //    S ER T AX N L IY median vowel is AX
            //    AX M EY Z IH NG L IY median vowel is EY
            // 5. calculate the START_ pronunciation as all the phonemes up to and including the median vowel
            //    for example: 
            //    START_KITTY K IH
            //    START_SIMPLE S IH
            //    START_CERTAINLY S ER
            //    START_AMAZINGLY AX M
            // 
            // 6. return just the START_ pronunciation, for example "K IH"
        }

        /// <summary>
        /// this is similar to start words but is used to find the end parts of a word for pronounciation like SAUR in DINOSAUR
        /// </summary>
        /// <param name="word">the word you want an end word pronounciation</param>
        /// <returns>phonemes for the end of a word</returns>
        private string endWordCalculatePronunciation(string word)
        {
            string returnValue = "F EH N T"; // default to END_ELEPHANT
                                             // return returnValue;//greg check this out before you use it

            String pronounciation = this.TheDecoder.LookUpWord(word);
            if (word == null) pronounciation = Sphinx.EnglishPhonemes.Phoneme.toPhoneme(word)[0];

            if (pronounciation == null)
            {
                return returnValue;
            }
            if (pronounciation.Length == 0)
            {
                return returnValue;
            }
            //if its a number its going to have multiple pronounciations....
            //not sure how to handle that for now
            string[] phonemes = pronounciation.Split(new char[0] { }, StringSplitOptions.RemoveEmptyEntries);

            if (phonemes.Length < MIN_STARTWORD_PHONEMES) return string.Empty;

            //midpoint phoneme approach
            int midPoint = (phonemes.Length / 2) - 1;//base 1 to base 0 means - 1


            const string endW = "END_";
            StringBuilder sb = new StringBuilder(endW.Length + word.Length + midPoint * 2 + 2);
            // sb.Append(endW).Append(word).Append('\t');
            // 2016-03-01 just the pronunciation?
            for (int i = midPoint + 1; i < phonemes.Length; ++i)
                sb.Append(phonemes[i]).Append(" ");

            return sb.ToString().TrimEnd();

        }

        // 2015-03-10 Greg refactor to use the arrays from Sphinx.English ... 
        // this helper method converts a Char into a uint
        // avoiding arithmetic on character ranges for better compatibility with unicode
        private uint UIntForChar(Char inDigitChar)
        {
            uint returnValue = 0;
            switch (inDigitChar)
            {
                case '0':
                    returnValue = 0;
                    break;
                case '1':
                    returnValue = 1;
                    break;
                case '2':
                    returnValue = 2;
                    break;
                case '3':
                    returnValue = 3;
                    break;
                case '4':
                    returnValue = 4;
                    break;
                case '5':
                    returnValue = 5;
                    break;
                case '6':
                    returnValue = 6;
                    break;
                case '7':
                    returnValue = 7;
                    break;
                case '8':
                    returnValue = 8;
                    break;
                case '9':
                    returnValue = 9;
                    break;
                default:
                    break;
            }
            return returnValue;
        }

        private string startWordPronunciationForDate(Char inThousandsDigit, Char inHundredsDigit)
        {
            string returnValue = string.Empty;

            if (inThousandsDigit == '1')
            {
                returnValue = Sphinx.EnglishPhonemes.Numbers.TEENS[UIntForChar(inHundredsDigit)]; // 1066, 1132 
            }
            else if (inHundredsDigit == '0')
            {
                returnValue = Sphinx.EnglishPhonemes.Numbers.TENS[UIntForChar(inThousandsDigit)]; // 2015, 3042, 4023, ...
            }
            else
            {
                returnValue = Sphinx.EnglishPhonemes.Numbers.TENS[UIntForChar(inThousandsDigit)] + " " + Sphinx.EnglishPhonemes.Numbers.UNITS[UIntForChar(inHundredsDigit)]; // 2312, 3241, ...
            }
            return returnValue;
        }

        private string startWordPronunciationForHundreds(Char inHundredsDigit)
        {
            string returnValue = Sphinx.EnglishPhonemes.Numbers.UNITS[UIntForChar(inHundredsDigit)]; // 104
            return returnValue;
        }

        private string startWordPronunciationForTens(Char inTensDigit)
        {
            string returnValue = string.Empty;
            switch (inTensDigit)
            {
                case '0':
                case '1':
                    returnValue = string.Empty; // eleven, twelve - can't be read in part
                    break;
                default:
                    returnValue = Sphinx.EnglishPhonemes.Numbers.TENS[UIntForChar(inTensDigit)];
                    break;
            }
            return returnValue;
        }

        private string startWordPronunciationForNumber(string originalWord)
        {
            // handle the following cases: one, two, three, four digit numbers
            string returnValue = string.Empty;

            if (originalWord.Length == 0)
            {
                returnValue = string.Empty;
            }
            else if (originalWord.Length == 1)
            {
                returnValue = string.Empty; // no special treatment for 0 through 9
            }
            else if (originalWord.Length == 2)
            {
                if (System.Char.IsDigit(originalWord[0]) && System.Char.IsDigit(originalWord[1]))
                {
                    if (originalWord[1] == '0')
                    {
                        returnValue = string.Empty; // no special treatment
                    }
                    else
                    {
                        returnValue = startWordPronunciationForTens(originalWord[0]);
                    }
                }
            }
            else if (originalWord.Length == 3)
            {
                if (System.Char.IsDigit(originalWord[0]) && System.Char.IsDigit(originalWord[1]) && System.Char.IsDigit(originalWord[2]))
                {
                    returnValue = startWordPronunciationForHundreds(originalWord[0]);
                }
            }
            else if (originalWord.Length == 4)
            {
                if (System.Char.IsDigit(originalWord[0]) && System.Char.IsDigit(originalWord[1]) && System.Char.IsDigit(originalWord[2]) && System.Char.IsDigit(originalWord[3]))
                {
                    returnValue = startWordPronunciationForDate(originalWord[0], originalWord[1]);
                }
            }

            return returnValue;
        }

        // 2015-11-19 anti-words
        char anticharacter(char inChar)
        {
            char returnValue = 'z';
            switch (inChar)
            {
                case 'A':
                case 'a':
                    returnValue = 'f';
                    break;
                case 'E':
                case 'e':
                    returnValue = 'k';
                    break;
                case 'I':
                case 'i':
                    returnValue = 'p';
                    break;
                case 'O':
                case 'o':
                    returnValue = 's';
                    break;
                case 'U':
                case 'u':
                    returnValue = 't';
                    break;
                case 'Y':
                case 'y':
                    returnValue = 'x';
                    break;

                case 'F':
                case 'f':
                    returnValue = 'a';
                    break;
                case 'K':
                case 'k':
                    returnValue = 'e';
                    break;
                case 'P':
                case 'p':
                    returnValue = 'i';
                    break;
                case 'S':
                case 's':
                    returnValue = 'o';
                    break;
                case 'T':
                case 't':
                    returnValue = 'u';
                    break;
                case 'X':
                case 'x':
                    returnValue = 'y';
                    break;

                case 'B':
                case 'b':
                    returnValue = 'h';
                    break;
                case 'C':
                case 'c':
                    returnValue = 'm';
                    break;
                case 'D':
                case 'd':
                    returnValue = 'q';
                    break;
                case 'G':
                case 'g':
                    returnValue = 'n';
                    break;
                case 'J':
                case 'j':
                    returnValue = 'w';
                    break;
                case 'R':
                case 'r':
                    returnValue = 'h';
                    break;
                case 'V':
                case 'v':
                    returnValue = 'l';
                    break;

                case 'H':
                case 'h':
                    returnValue = 'b';
                    break;
                case 'M':
                case 'm':
                    returnValue = 'c';
                    break;
                case 'Q':
                case 'q':
                    returnValue = 'd';
                    break;
                case 'N':
                case 'n':
                    returnValue = 'g';
                    break;
                case 'W':
                case 'w':
                    returnValue = 'j';
                    break;
                case 'L':
                case 'l':
                    returnValue = 'v';
                    break;
                default:
                    returnValue = inChar;
                    break;
            }
            return returnValue;

        }

        // calculate the anti-word (cf. antimatter)
        string calculateNonsenseWord(string inWord)
        {
            string returnValue = "";
            for (int i = 0; i < inWord.Length; i++)
            {
                returnValue += anticharacter(inWord[i]);
            }
            //Debug.Log ("calculateNonsenseWord input: " + inWord + " output: : " + returnValue);
            return returnValue;
        }

        // 2015-11-23 semi-words
        char semicharacter(char inChar)
        {
            char returnValue = inChar;
            switch (inChar)
            {
                case 'A':
                case 'a':
                    returnValue = 'E';
                    break;
                case 'E':
                case 'e':
                    returnValue = 'I';
                    break;
                case 'I':
                case 'i':
                    returnValue = 'A';
                    break;
                case 'O':
                case 'o':
                    returnValue = 'U';
                    break;
                case 'U':
                case 'u':
                    returnValue = 'O';
                    break;
                case 'Y':
                case 'y':
                    returnValue = 'W';
                    break;
                case 'W':
                case 'w':
                    returnValue = 'Y';
                    break;

                case 'F':
                case 'f':
                    returnValue = 'V';
                    break;
                case 'V':
                case 'v':
                    returnValue = 'F';
                    break;

                case 'K':
                case 'k':
                    returnValue = 'G';
                    break;
                case 'G':
                case 'g':
                    returnValue = 'K';
                    break;

                case 'P':
                case 'p':
                    returnValue = 'B';
                    break;
                case 'B':
                case 'b':
                    returnValue = 'P';
                    break;

                case 'S':
                case 's':
                    returnValue = 'Z';
                    break;
                case 'Z':
                case 'z':
                    returnValue = 'S';
                    break;

                case 'T':
                case 't':
                    returnValue = 'D';
                    break;
                case 'D':
                case 'd':
                    returnValue = 'T';
                    break;

                case 'X':
                case 'x':
                    returnValue = 'y';
                    break;

                case 'C':
                case 'c':
                    returnValue = 'J';
                    break;
                case 'J':
                case 'j':
                    returnValue = 'C';
                    break;

                case 'H':
                case 'h':
                    returnValue = 'H';
                    break;
                case 'Q':
                case 'q':
                    returnValue = 'Q';
                    break;

                case 'R':
                case 'r':
                    returnValue = 'L';
                    break;
                case 'M':
                case 'm':
                    returnValue = 'N';
                    break;
                case 'N':
                case 'n':
                    returnValue = 'M';
                    break;
                case 'L':
                case 'l':
                    returnValue = 'R';
                    break;
                default:
                    returnValue = inChar;
                    break;
            }
            return returnValue;

        }

        // calculate the semi-word - similar to but different from the original word at each position
        string calculateSemiWord(string inWord)
        {
            string returnValue = "";
            for (int i = 0; i < inWord.Length; i++)
            {
                returnValue += semicharacter(inWord[i]);
            }
            //Debug.Log ("calculateSemiWord input: " + inWord + " output: : " + returnValue);
            return returnValue;
        }

        public static char maybeYesNewVowelsChar(char inChar)
        {
            char returnValue = inChar;
            switch (inChar)
            {

                case 'A':
                case 'a':
                    returnValue = 'O';
                    break;

                case 'O':
                case 'o':
                    returnValue = 'A';
                    break;

                case 'E':
                case 'e':
                    returnValue = 'I';
                    break;

                case 'I':
                case 'i':
                    returnValue = 'E';
                    break;

                case 'U':
                case 'u':
                    returnValue = 'Y';
                    break;

                case 'Y':
                case 'y':
                    returnValue = 'U';
                    break;

                default:
                    returnValue = inChar;
                    break;
            }
            return returnValue;

        }

        // calculate the maybe yes with just the vowels changed
        public static string calculateMaybeYesNewVowelsWord(string inWord)
        {
            string returnValue = ""; // don't need special character - just call this method again // "\'"; // detect MaybeYes words
            for (int i = 0; i < inWord.Length; i++)
            {
                returnValue += maybeYesNewVowelsChar(inWord[i]);
            }
            //Debug.Log ("calculateMaybeYesNewVowelsWord input: " + inWord + " output: : " + returnValue);
            return returnValue;
        }

        // 2015-12-03 only the consonants are different - "maybe yes"
        // the maybeYes is reversible, e.g. f(dog) = tok, f(tok) = dog
        public static char maybeYesChar(char inChar)
        {
            char returnValue = inChar;
            switch (inChar)
            {

                case 'F':
                case 'f':
                    returnValue = 'V';
                    break;
                case 'V':
                case 'v':
                    returnValue = 'F';
                    break;

                case 'K':
                case 'k':
                    returnValue = 'G';
                    break;
                case 'G':
                case 'g':
                    returnValue = 'K';
                    break;

                case 'P':
                case 'p':
                    returnValue = 'B';
                    break;
                case 'B':
                case 'b':
                    returnValue = 'P';
                    break;

                case 'S':
                case 's':
                    returnValue = 'Z';
                    break;
                case 'Z':
                case 'z':
                    returnValue = 'S';
                    break;

                case 'T':
                case 't':
                    returnValue = 'D';
                    break;
                case 'D':
                case 'd':
                    returnValue = 'T';
                    break;

                case 'C':
                case 'c':
                    returnValue = 'J';
                    break;
                case 'J':
                case 'j':
                    returnValue = 'C';
                    break;

                case 'R':
                case 'r':
                    returnValue = 'L';
                    break;
                case 'M':
                case 'm':
                    returnValue = 'N';
                    break;
                case 'N':
                case 'n':
                    returnValue = 'M';
                    break;
                case 'L':
                case 'l':
                    returnValue = 'R';
                    break;
                default:
                    returnValue = inChar;
                    break;
            }
            return returnValue;

        }

        // calculate the maybe yes - similar to but different from the original word at each position
        public static string calculateMaybeYesWord(string inWord)
        {
            string returnValue = ""; // don't need special character - just call this method again //  = "\'"; // detect MaybeYes words
            for (int i = 0; i < inWord.Length; i++)
            {
                returnValue += maybeYesChar(inWord[i]);
            }
            //Debug.Log ("calculateMaybeYesWord input: " + inWord + " output: : " + returnValue);
            return returnValue;
        }

        // 2015-11-18 renormalize the language model weight to use more of the lower and upper range. this version is half as likely for each item below 0.20
        // 2015-11-24 use 1.5 as the base to accomodate adding the semi-words
        private static readonly double[] renormedForgivenessArray = {
                0.00115292,
                0.00144115,
                0.00180144,
                0.00225180,
                0.00281475,
                0.00351844,
                0.00439805,
                0.00549756,
                0.00687195,
                0.00858993,
                0.01073742,
                0.01342177,
                0.01677722,
                0.02097152,
                0.02621440,
                0.03276800,
                0.04096000,
                0.05120000,
                0.06400000,
                0.08000000,
                0.10000000,
                0.11333333,
                0.12666667,
                0.14000000,
                0.15333333,
                0.16666667,
                0.18000000,
                0.19333333,
                0.20666667,
                0.22000000,
                0.23333333,
                0.24666667,
                0.26000000,
                0.27333333,
                0.28666667,
                0.30000000,
                0.31333333,
                0.32666667,
                0.34000000,
                0.35333333,
                0.36666667,
                0.38000000,
                0.39333333,
                0.40666667,
                0.42000000,
                0.43333333,
                0.44666667,
                0.46000000,
                0.47333333,
                0.48666667,
                0.50000000,
                0.51333333,
                0.52666667,
                0.54000000,
                0.55333333,
                0.56666667,
                0.58000000,
                0.59333333,
                0.60666667,
                0.62000000,
                0.63333333,
                0.64666667,
                0.66000000,
                0.67333333,
                0.68666667,
                0.70000000,
                0.71333333,
                0.72666667,
                0.74000000,
                0.75333333,
                0.76666667,
                0.78000000,
                0.79333333,
                0.80666667,
                0.82000000,
                0.83333333,
                0.84666667,
                0.86000000,
                0.87333333,
                0.88666667,
                0.90000000,
                0.92000000,
                0.93600000,
                0.94880000,
                0.95904000,
                0.96723200,
                0.97378560,
                0.97902848,
                0.98322278,
                0.98657823,
                0.98926258,
                0.99141007,
                0.99312805,
                0.99450244,
                0.99560195,
                0.99648156,
                0.99718525,
                0.99774820,
                0.99819856,
                0.99855885,
                0.99884708,
                0.99907766};

        /// <summary>
        /// Creates and sets the recognizer to use an FSG.
        /// </summary>
        /// <returns><c>true</c>, if succesful <c>false</c> otherwise.</returns>
        /// <param name="searchName">Search name.</param>
        /// <param name="searchWords">Search words.</param>
        /// <param name="startIndex">Start index of the words in searchWords</param>
        /// <param name="lmModelWeight">Lm model weight.</param>
        private bool setFSG(string searchName, string[] searchWords, int startIndex, int lmModelWeight, double forgivenessBalance)
        {

            //hotfix by Rod 1/31/2015. setting the forgiveness to 0 causes a crash due to failing to create the fsg
            //ERROR: "fsg_model.c", line 672: Line[20]: transition spec malformed; Expecting float as transition probability
            // if(forgivenessBalance < .15) forgivenessBalance = .15;
            // Greg 2/2/2015 this particular crash fixed by not subtracting the extra distractors' probability from PrCorrectThisWord
            // however we should still clamp above zero and below one since either 0.0 or 1.0 will result in an ill-formed language model
            if (forgivenessBalance < 0.01)
            {
                forgivenessBalance = 0.01;
            }
            else if (forgivenessBalance > 0.99)
            {
                forgivenessBalance = 0.99;
            }

            double originalForgivenessBalance = forgivenessBalance;
            int fbIndex = (int)(Math.Round(originalForgivenessBalance * 100.0));
            if (fbIndex < renormedForgivenessArray.Length)
            {
                forgivenessBalance = renormedForgivenessArray[fbIndex];
                //Debug.Log ("forgivenessBalance of " + originalForgivenessBalance.ToString() + "renormed using base10 to " + forgivenessBalance.ToString ());
            }

            HashSet<string> uniques = new HashSet<string>(searchWords);

            // ensure all sentence words in dictionary
            foreach (string word in uniques)
            {

                if (this.TheDecoder.LookUpWord(word) == null)
                {
                    //Debug.Log ("word not in dic " + "'" + word + "'");
                    this.TheDecoder.AddWord(word); // more efficient to pass 1 (true) on last word only?
                }

                string nonsenseWordDistractor = calculateNonsenseWord(word);
                if (this.TheDecoder.LookUpWord(nonsenseWordDistractor) == null)
                {
                    this.TheDecoder.AddWord(nonsenseWordDistractor);
                }

                string semiWordDistractor = calculateSemiWord(word);
                if (this.TheDecoder.LookUpWord(semiWordDistractor) == null)
                {
                    this.TheDecoder.AddWord(semiWordDistractor);
                }

                string nearWordDistractor = calculateMaybeYesWord(word);
                if (this.TheDecoder.LookUpWord(nearWordDistractor) == null)
                {
                    this.TheDecoder.AddWord(nearWordDistractor);
                }

                string maybeYesNewVowelsWord = calculateMaybeYesNewVowelsWord(word);
                if (this.TheDecoder.LookUpWord(maybeYesNewVowelsWord) == null)
                {
                    this.TheDecoder.AddWord(maybeYesNewVowelsWord);
                }


                // TODO: add START_ words for truncated readings as well
                // check START_ for every word, whether it was in the dictionary or not
                // that way the original dictionary can contain just the full words
                // and some hand-written exceptions to the START_ algorithm
                // this will help reduce file size of the dictionary, perhaps by 0.5 to 2.0 megabytes 
                // (50k to 100k words times 10 to 20 bytes per START_ pronunciation)
                string startWordWord = startWord(word); // for example, START_ELEPHANT
                if (this.TheDecoder.LookUpWord(startWordWord) == null)
                {
                    ;//Debug.Log ("START_word not in dic " + "'" + startWordWord + "'");
                    string startNumberPron = startWordPronunciationForNumber(word);
                    if (!string.IsNullOrEmpty(startNumberPron))
                    {
                        //Debug.Log ("adding pronunciation for START_" + startWordWord + " " + startNumberPron);
                        this.TheDecoder.AddWord(startWordWord, startNumberPron);
                    }
                    else
                    {
                        string startWordPron = startWordCalculatePronunciation(word); // calculate truncation such as EH L AH F 
                        if (!string.IsNullOrEmpty(startWordPron))
                        {
                            //Debug.Log ("adding calculated pronunciation for " + startWordWord + " " + startWordPron); 
                            this.TheDecoder.AddWord(startWordWord, startWordPron);
                        }
                    }


                }

                // 2016-03-01 endWord
                string endWordWord = endWord(word); // for example, END_ELEPHANT
                if (this.TheDecoder.LookUpWord(endWordWord) == null)
                {
                    //Debug.Log ("END_word not in dic " + "'" + endWordWord + "'");
                    string endNumberPron = startWordPronunciationForNumber(word);
                    if (!string.IsNullOrEmpty(endNumberPron))
                    {
                        //Debug.Log ("adding pronunciation for END_" + endWordWord + " " + endNumberPron);
                        this.TheDecoder.AddWord(endWordWord, endNumberPron);
                    }
                    else
                    {
                        string endWordPron = endWordCalculatePronunciation(word); // calculate truncation such as EH L AH F 
                        if (!string.IsNullOrEmpty(endWordPron))
                        {
                            //Debug.Log ("adding calculated pronunciation for " + endWordWord + " " + endWordPron); 
                            this.TheDecoder.AddWord(endWordWord, endWordPron);
                        }
                    }


                }

            }



            // ensure all sentence words in dictionary
            for (int r = 0; r < firstWordDistractorsArray.Length; r++)
            {
                string distractor = firstWordDistractorsArray[r];
                if (this.TheDecoder.LookUpWord(distractor) == null)
                {   //Debug.Log("word not in dic " + "'" + word + "'");
                    if (r < firstWordDistractorsPron.Length)
                    {
                        string distractorPron = firstWordDistractorsPron[r];
                        this.TheDecoder.AddWord(distractor, distractorPron); // more efficient to pass 1 (true) on last word only?
                                                                             // don't add START_ for the distractor words
                    }
                }
            }



            System.Text.StringBuilder commands = new System.Text.StringBuilder();

            // This is the dial to use to set the overall forgiveness/strictness. Range is [epsilon, 1-epsilon]
            // Limited to not quite zero since zero values would cause problems in the recognizer (perhaps crashes)
            // code for Jan 23 2015 demo was equivalent to a setting of 0.5
            const double PrEpsilon = 0.000001;
            double PrForgivenessBalance = 0.5;
            if (forgivenessBalance < PrEpsilon)
            {
                PrForgivenessBalance = PrEpsilon;
            }
            else if (forgivenessBalance > (1.0 - PrEpsilon))
            {
                PrForgivenessBalance = 1.0 - PrEpsilon;
            }
            else
            {
                PrForgivenessBalance = forgivenessBalance;
            }

            // log PrForgivenessBalance just before using it 
            //Debug.Log ("setFSG : PrForgivenessBalance equals " + PrForgivenessBalance); 

            // LM probabilities.
            // TODO: load these from config file and/or provide some API for changing them
            // original number was 0.9 - this was too permissive, it would respond to blah blah blah by crediting words
            // set PrCorrect to 0.2 - use the score threshold to set strictness level
            // 0.2 was too strict - resetting to 0.5
            double PrCorrect = PrForgivenessBalance; // 0.5;	

            double PrCorrectEasyWords = 3.0 * (PrForgivenessBalance / 4.0); // change to 0.375 for revision 2037 // build // 2.0 * (PrForgivenessBalance / 4.0); // 0.25;		
            double PrTruncate = 1.0 * (1.0 - PrForgivenessBalance) / 12.0; // 0.05;
            double PrResume = 2.0 * (1.0 - PrForgivenessBalance) / 12.0; // 0.1;
                                                                         // double PrRestart = 1.0 * (1.0 - PrForgivenessBalance) / 12.0; // 0.05;   // need PrRestart even in race mode, to allow player to recover from a missed word 
                                                                         // double PrRestart = 6.0 * (1.0 - PrForgivenessBalance) / 12.0; // 0.05;   // need PrRestart even in race mode, to allow player to recover from a missed word 
            double PrRestart = 2.0 * (1.0 - PrForgivenessBalance) / 12.0; // 0.05;   // need PrRestart even in race mode, to allow player to recover from a missed word 
            double PrSkipReplacement = 2.0 * (1.0 - PrForgivenessBalance) / 12.0; // some portion of the restart should go to anti-words  			
            double PrSemiWord = 1.0 * (1.0 - PrForgivenessBalance) / 12.0; // semi-words with vowels the same
            double PrMaybeYesWord = 1.0 * (1.0 - PrForgivenessBalance) / 12.0; // semi-words with vowels the same
            double PrMaybeYesNewVowelsWord = 1.0 * (1.0 - PrForgivenessBalance) / 12.0; // semi-words with vowels changed

            double PrJumpBack = 1.0 * (1.0 - PrForgivenessBalance) / 12.0; // 0.05;  // also need to jump back (but not forward) in race mode, here to allow a player to recover from several missed words 
            double PrRepeat = (1.0 * PrForgivenessBalance) / 4.0; // 1.0 * (1.0 - PrForgivenessBalance) / 12.0; // 0.05; // the repeat should be based on PrForgiveness, not 1 - PrForgiveness

            double PrSkip = 1.0 * (1.0 - PrForgivenessBalance) / 12.0; // 0.3; //  0.2; 			
            double PrSkipEasyWords = 1.0 * (1.0 - PrCorrectEasyWords) / 12.0; // 0.45; 		

            // double PrExtraDistractorsForFirstWord = 2.0 * (1.0 - PrForgivenessBalance) / 12.0;

            // write the fsg file header info 
            int state_count = searchWords.Length + 1;
            int start_state = startIndex;       // normally 0 for first, but can be anywhere
            int final_state = state_count - 1;

            //WARNING WE NEED TO USE .Append(\n) because the c++ code expencts \n not \r\n like it does for windews when you use AppendLine
            commands.AppendNewline("FSG_BEGIN sentence");
            commands.AppendNewline("NUM_STATES " + state_count);
            commands.AppendNewline("START_STATE " + start_state);
            commands.AppendNewline("FINAL_STATE " + final_state);

            // factor to normalize transition probabilities based on sentence length
            int n = searchWords.Length - 1;
            if (n < 1)
                n = 1;

            // add state transitions
            for (int i = 0; i < state_count - 1; i++)
            {
                // emit word i for transition from state i to i + 1 with probability PrCorrect
                double PrCorrectThisWord = PrCorrect;
                double PrSkipThisWord = PrSkip;

                if (searchWords[i].Equals("a", System.StringComparison.OrdinalIgnoreCase)
                    || searchWords[i].Equals("an", System.StringComparison.OrdinalIgnoreCase)
                    || searchWords[i].Equals("I", System.StringComparison.OrdinalIgnoreCase)
                    )
                {
                    PrCorrectThisWord = PrCorrectEasyWords;
                    PrSkipThisWord = PrSkipEasyWords;
                    PrSkipThisWord += PrTruncate; // no truncations for very short words, add these back in
                }
                if ((i > 0) && searchWords[i - 1].Equals("d", System.StringComparison.OrdinalIgnoreCase)
                    && searchWords[i].Equals("e", System.StringComparison.OrdinalIgnoreCase))
                {
                    PrCorrectThisWord += PrSkipThisWord;
                    PrSkipThisWord = 2.0 * PrEpsilon; // still emit skip arc, just with tiny probability
                                                      //Debug.Log ("setFSG - found special case D E - setting PrCorrectThisWord for this E to " + PrCorrectThisWord); 
                }

                // override for the first word and the first word that we are listening for 
                // that way if there is a reset after TTS speaks the word, we won't be likelier to hear the word
                // having these extra distractors on the first word and the first word being listened to should help reduce false positives from background speech
                // -- PrCorrect should be PrCorrect, but PrSkipThisWord should be higher to 
                // account for all the other ways in which the other ways can be not said correctly
                // however we also need to subtract the extra probability for the first word distractors
                // if ((i == 0) || (i == startIndex))  {
                // 2015-02-26 try only on the expected word - reduce strictness by not doubling the number of distractors available
                // but still prevent the effect where a reset after TTS makes the word likelier to false-positive
                if (i == startIndex)
                {
                    PrSkipThisWord += PrRestart;
                    PrSkipThisWord += PrJumpBack;
                    double PrExtraDistractorsForFirstWord = PrCorrectThisWord / 3.0; // one third of the weight of the first word goes to distractors 

                    PrCorrectThisWord = 2.0 * (PrCorrectThisWord / 3.0); // rest goes to PrCorrectThisWord

                    // special treatment for first word to make it not hallucinated
                    int numFirstWordDistractors = firstWordDistractorsArray.Length;
                    double normPrExtraDistractorsForFirstWord = PrExtraDistractorsForFirstWord / numFirstWordDistractors;
                    foreach (string distractor in firstWordDistractorsArray)
                    {
                        commands.AddFSGTransition(i, i, normPrExtraDistractorsForFirstWord, distractor);
                    }
                }

                commands.AddFSGTransition(i, i + 1, PrCorrectThisWord, searchWords[i]);
                // and also add the anti-words
                // 2015-12-07 if the word is more than two letters long and not THE 
                if ((searchWords[i].Length > 2) && (searchWords[i].ToUpper() != "THE"))
                {
                    commands.AddFSGTransition(i, i, PrSkipReplacement, calculateNonsenseWord(searchWords[i]));
                    commands.AddFSGTransition(i, i, PrSemiWord, calculateSemiWord(searchWords[i]));
                    commands.AddFSGTransition(i, i, PrMaybeYesWord, calculateMaybeYesWord(searchWords[i]));
                    commands.AddFSGTransition(i, i, PrMaybeYesNewVowelsWord, calculateMaybeYesNewVowelsWord(searchWords[i]));
                }

                // truncations not yet implemented for words not in dictionary
                // 01-14-2016 based on the debug statements below, the START_ words are already limited to longer words.
                //   thus the issues we are seeing with IF and IN must be due to something else. perhaps one of the garbage word distractors sounds too much like IF, IN, IS
                if (this.TheDecoder.LookUpWord(startWord(searchWords[i])) != null)
                {
                    // 2015-01-14 if the word is more than two letters long and not THE 
                    // 2016-03-03 change to : if word is more than one letter long and not THE
                    if ((searchWords[i].Length > 1) && (searchWords[i].ToUpper() != "THE"))
                    {
                        //Debug.Log ("using START_ word for long word " + searchWords[i]); 

                        // 2016-03-01 split probability between START_ word and END_ word
                        //emit word i truncation for transition from state i to state i with probability PrTruncate
                        commands.AddFSGTransition(i, i, PrTruncate / 2.0, startWord(searchWords[i]));

                        //emit word i truncation for transition from state i to i + 1 with probability PrResume
                        commands.AddFSGTransition(i, i + 1, PrResume / 2.0, startWord(searchWords[i]));
                    }
                    else
                    {
                        //Debug.Log ("not using START_ word for short word " + searchWords[i]); 
                    }
                }
                else
                {
                    //Debug.Log ("START_ word not in dictionary for " + searchWords[i]); 
                }

                if (this.TheDecoder.LookUpWord(endWord(searchWords[i])) != null)
                {
                    // 2015-01-14 if the word is more than two letters long and not THE 
                    if ((searchWords[i].Length > 2) && (searchWords[i].ToUpper() != "THE"))
                    {
                        //Debug.Log ("using END_ word for long word " + searchWords[i]); 

                        // 2016-03-01 split probability between START_ word and END_ word
                        //emit word i truncation for transition from state i to state i with probability PrTruncate
                        commands.AddFSGTransition(i, i, PrTruncate / 2.0, endWord(searchWords[i]));

                        //emit word i truncation for transition from state i to i + 1 with probability PrResume
                        commands.AddFSGTransition(i, i + 1, PrResume / 2.0, endWord(searchWords[i]));
                    }
                    else
                    {
                        //Debug.Log ("not using END_ word for short word " + searchWords[i]); 
                    }
                }
                else
                {
                    //Debug.Log ("END_ word not in dictionary for " + searchWords[i]); 
                }

                if (PrRestart > PrEpsilon)
                {
                    //if i <> 0 emit null word for jump from state i back to state 0 with probability PrRestart
                    if (i != 0)
                    {
                        commands.AddFSGTransition(i, 0, PrRestart / n, string.Empty);

                    }
                }

                if (PrRepeat > PrEpsilon)
                {
                    //emit word i for transition from state i to state i with probability PrRepeat
                    commands.AddFSGTransition(i, i, PrRepeat / n, searchWords[i]);
                }

                if (PrJumpBack > PrEpsilon)
                {
                    // emit null word for jump from state i to state j with probability PrJump for all states j < i except 0
                    for (int j = 1; j < i; j++)
                    {
                        double NormPrJumpBack = PrJumpBack;
                        if (i > 1)
                        {
                            NormPrJumpBack = NormPrJumpBack / i;
                        }
                        commands.AddFSGTransition(i, j, NormPrJumpBack, string.Empty);
                    }
                }

                if (PrSkipThisWord > PrEpsilon)
                {
                    // emit null word for jump from state i to state i+1 with probability PrSkip 
                    int iNext = i + 1;
                    if (iNext < state_count)
                    {
                        commands.AddFSGTransition(i, iNext, PrSkipThisWord, string.Empty);
                    }
                }
            }

            if (PrRestart > PrEpsilon)
            {
                // add jump from final state back to start with probability PrRestart
                commands.AddFSGTransition(final_state, 0, PrRestart / n, string.Empty);
            }

            // use the jump back probability
            if (PrJumpBack > PrEpsilon)
            {
                // add jump from final state back to each earlier state
                for (int st = 1; st < state_count - 1; st++)
                {
                    commands.AddFSGTransition(final_state, st, PrJumpBack / n, string.Empty);
                }
            }


            // done writing the fsg. fsg must end with a new line otherwise crash
            commands.AppendNewline("FSG_END");

            FsgModel model;

            int adjusted_lmModelWeight = lmModelWeight;
            if (InitModelPaths.AMChoice == InitModelPaths.ACCOUSTIC_MODELS.CHILD)
            {
                Debug.Log("adjusting language model weight for sentence reading case");
                adjusted_lmModelWeight = 6;
            }
#if UNITY_EDITOR && PS_UNITY_USE_FSG_FILE
            string fsgFileName = "tempFSG";
            if (System.IO.File.Exists(fsgFileName))
                System.IO.File.Delete(fsgFileName);
            System.IO.File.WriteAllLines(fsgFileName, commands.ToString().Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.None));
            model = FsgModel.CreateFromFile(fsgFileName, logmath, adjusted_lmModelWeight);
#else
			model = FsgModel.CreatefromString(commands.ToString(),logmath,adjusted_lmModelWeight);
#endif

            return this.AddFSGModel(model, searchName);
        }

        public static string startWord(string word)
        {
            return "START_" + word;
        }

        public static string endWord(string word)
        {
            return "END_" + word;
        }
    }//end BasicFSGRecognizer

}//end namespace
