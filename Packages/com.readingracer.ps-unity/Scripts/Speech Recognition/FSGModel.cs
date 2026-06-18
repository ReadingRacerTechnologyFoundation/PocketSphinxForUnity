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
namespace Rrtf.Sphinx
{
    using System;
    using SphinxNative;

	/// <summary>
	/// Wrapper for fsg_model_t. creates FSG model types used by SpeechRecognizer
	/// </summary>
	public class FsgModel
	{
		public readonly fsg_model_t RawModel;

		private FsgModel(fsg_model_t t)
		{
			RawModel = t;
		}

		public static FsgModel CreateFromFile(string modelPath, LogMath logmath, float lw)
		{
			fsg_model_t temp = SB_FsgModel.fsg_model_readfile(modelPath,logmath.RawLogMath, lw);

			if(temp.IsNull())
			{
				Debug.LogError("Critical Error: creating FSG from file failed.");
				Application.Quit();
			}

			return new FsgModel(temp);
		}

		public static FsgModel CreatefromString(string commands, 
		                                        LogMath logmath, float lw)
		{
			int lineCount = 0;
			for(int i = 0; i < commands.Length;++i)
				if(commands[i] == '\n') ++lineCount;

			//counting the words per line
			int[] perLine = new int[lineCount];
			int j = 0;
			bool inWord = false;
			for(int i = 0; i < commands.Length; ++i)
			{
				perLine[j] = 0;
				while(commands[i] != '\n' && i < commands.Length)
				{
					if(char.IsWhiteSpace(commands[i]))
					{
						inWord = false;
					}
					else if(!inWord)//we transitioned to being in a word, count that
					{
						++perLine[j];
						inWord = true;
					}

					++i;
				}

				inWord = false;
				++j;
			}

			Debug.Log("Commands:\n" + commands);
			fsg_model_t model = SB_FsgModel.fsg_model_fromBuildStruct(fsg_builder_t.CreateBuilder(lineCount,perLine,commands),
			                                                          logmath.RawLogMath, lw);

			if(model.IsNull())
			{
				Debug.LogError("Critical Error: creating FSG from file failed.");
				Application.Quit();
			}

			return new FsgModel(model);
		}
	}

}
