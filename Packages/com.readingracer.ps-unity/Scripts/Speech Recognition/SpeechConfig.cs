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
namespace Rrtf.Sphinx
{
	using SphinxNative;

	/// <summary>
	/// Speech config. Use This to create a cmd_ln_t/SpeechConfig object.  In order to create a SpeechConfig object
	/// use the static functions to set up its options then call GenerateConfigWithCurrentSettings.
	/// </summary>
	public class SpeechConfig
	{
		//Don't use this directly. unless you really really really sure you know what you are doing
		public readonly cmd_ln_t Config;

		private SpeechConfig(cmd_ln_t config)
		{
			this.Config = config;
		}

		private static cmd_ln_t currentConfig = SB_CmdLn.cmd_ln_init();


		/// <summary>
		/// returns the current configuration that has been created 
		/// by using the static set methods. This configuration can
		/// be then used to create a SpeechRecognizer. 
		/// Automaticly clears the currently being created configuration
		/// allowing you to create a new different configuration
		/// </summary>
		/// <returns>The config with current settings.</returns>
		public static SpeechConfig GenerateConfigWithCurrentSettings()
		{

			cmd_ln_t newConfig = currentConfig;
			currentConfig = SB_CmdLn.cmd_ln_init();
			//Debug.LogError("Config is : " + currentConfig.IsNull());
			return new SpeechConfig(newConfig);
		}

		/// <summary>
		/// clears the current config being built and creates a new one from a file.
		/// Use this first. Can call other setters after this call
		/// </summary>
		/// <param name="path">The file Path.</param>
		public static void UseFileSettings(string path)
		{
			currentConfig = SB_CmdLn.cmd_ln_parse_file_r(cmd_ln_t.CreateNull(),
			                             PocketSphinx.ps_argugments(),
			                             path,false);
		}

		/// <summary>
		/// Resets the configuration that was being built back to
		/// its initial settings
		/// </summary>
		public static void ResetSettings()
		{
			currentConfig = SB_CmdLn.cmd_ln_init();
		}

		/// <summary>
		/// Sets the acoustic model (hmm).
		/// </summary>
		/// <param name="modelPath">hmm Model folder path.</param>
		public static void SetAcousticModel(string modelPath) 
		{
			if(!System.IO.Directory.Exists(modelPath))
			{
				Debug.LogError("ERROR: Acoustic Model folder does not exist: " + modelPath);
				return;
			}
			SB_CmdLn.cmd_ln_set_str_r (currentConfig, "-hmm", modelPath);
		}

		/// <summary>
		/// Sets the Dictionary.
		/// </summary>
		/// <param name="dicPath">path to the dict file</param>
		public static void SetDictionary(string dicPath) 
		{
			if(!System.IO.File.Exists(dicPath))
			{
				Debug.LogError("ERROR: dict file does not exist: " + dicPath);
				return;
			}
			SB_CmdLn.cmd_ln_set_str_r (currentConfig, "-dict", dicPath);
		}

		/// <summary>
		/// Sets the sample rate.
		/// </summary>
		/// <param name="rate">AKA frequency</param>
		public static void SetSampleRate(int rate) 
		{
			if(rate < 1)
			{
				Debug.LogError("ERROR: Attempting to set a samplerate less than 1");
				return;
			}
			SB_CmdLn.cmd_ln_set_int_r(currentConfig,"-samprate",rate);
		}

		/// <summary>
		/// Creates the raw log in the folder of the Applications persistant data path.
		/// Does nothing on mobile.
		/// </summary>
		/// <param name="folderName">Folder name.</param>
		public static void CreateRawLog(string folderName) 
		{
			#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
			Debug.Log("Cannot create raw file on mobile");
			#else

			string path = Application.persistentDataPath + "/" + folderName + "/";
			if(!System.IO.Directory.Exists(path))
			{
				System.IO.Directory.CreateDirectory(path);
				return;
			}

			Debug.Log("Writing raw to " + path);

			SB_CmdLn.cmd_ln_set_str_r(currentConfig,"-rawlongdir",path);
			#endif
		}

		/// <summary>
		/// Creates the log file. Outputs in debug its location.
		/// </summary>
		public static void CreateLogFile()
		{
			#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
			Debug.Log("Cannot create log file on mobile");
			#else
				
			string path = Application.persistentDataPath + "/SphinxLogs/";
			if(!System.IO.Directory.Exists(path))
			{
				System.IO.Directory.CreateDirectory(path);
				return;
			}

			path += DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Year + "  " + DateTime.Now.TimeOfDay + ".txt";

			System.IO.File.Create(path);

			Debug.Log("Writing log fileto " + path);
			
			SB_CmdLn.cmd_ln_set_str_r(currentConfig,"-logfn",path);
			#endif
		}

		/// <summary>
		/// Sets the keyword threshold. Make it very small to easily allow words through
		/// such as 1e-60. 
		/// </summary>
		/// <param name="threshold">Threshold. Should be less than less than 1 but greater than zero</param>
		public static void SetKeywordThreshold(float threshold) 
		{
			if(threshold < 0 || threshold > 1)
			{
				Debug.LogError("threshhold must be in range [0,1]. Normally very small. set ignored");
				return;
			}
			SB_CmdLn.cmd_ln_set_float_r(currentConfig,"-kws_threshold",threshold);
		}

		/// <summary>
		/// Sets a boolean value. ONLY USE THIS IF YOU KNOW WHAT YOU ARE DOING.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">value <c>true</c> value.</param>
		public static void SetBoolean(string key, bool value) 
		{
			SB_CmdLn.cmd_ln_set_boolean(currentConfig,key,value);
		}

		/// <summary>
		/// Sets an int value. ONLY USE THIS IF YOU KNOW WHAT YOU ARE DOING.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public static void SetInteger(string key, int value) 
		{
			SB_CmdLn.cmd_ln_set_int_r(currentConfig,key,value);
		}

		/// <summary>
		/// Sets a float value. ONLY USE THIS IF YOU KNOW WHAT YOU ARE DOING.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public static void SetFloat(string key, float value) 
		{
			SB_CmdLn.cmd_ln_set_float_r(currentConfig,key,value);
		}

		/// <summary>
		/// Sets the a string value. ONLY USE THIS IF YOU KNOW WHAT YOU ARE DOING.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public static void SetString(string key, string value) 
		{
			SB_CmdLn.cmd_ln_set_str_r(currentConfig,key,value);
		}

		/// <summary>
		/// Gets the sample rate of this speech config
		/// </summary>
		/// <returns>The sample rate. AKA frequency</returns>
		public int GetSampleRate()
		{
			return (int)SB_CmdLn.cmd_ln_float_r (Config, "-samprate");
		}

		/// <summary>
		/// ONLY USE THIS IF YOU KNOW WHAT YOU ARE DOING. THIS WILL CRASH THINGS.
		/// Gets the boolean value associated with the key.
		/// </summary>
		/// <returns><c>true</c>, if boolean was gotten, <c>false</c> otherwise.</returns>
		/// <param name="key">Key.</param>
		public bool GetBoolean(string key) 
		{
			return SB_CmdLn.cmd_ln_bool_r(Config, key);
		}

		/// <summary>
		/// ONLY USE THIS IF YOU KNOW WHAT YOU ARE DOING. THIS WILL CRASH THINGS.
		/// Gets the integer associated with the key.
		/// </summary>
		/// <returns>The integer.</returns>
		/// <param name="key">Key.</param>
		public int GetInteger(string key)
		{
			return Convert.ToInt32(SB_CmdLn.cmd_ln_int_r(Config,key));
		}

		/// <summary>
		/// ONLY USE THIS IF YOU KNOW WHAT YOU ARE DOING. THIS WILL CRASH THINGS.
		/// Gets the float associated with the key.
		/// </summary>
		/// <returns>The float.</returns>
		/// <param name="key">Key.</param>
		public float GetFloat(string key)
		{
			return (float)SB_CmdLn.cmd_ln_float_r(Config,key);
		}
	
		/// <summary>
		/// ONLY USE THIS IF YOU KNOW WHAT YOU ARE DOING. THIS WILL CRASH THINGS.
		/// Gets the string associated with the key.
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="key">Key.</param>
		public string GetString(string key)
		{
			return SB_CmdLn.cmd_ln_str_r(Config,key);
		}
	}
}
