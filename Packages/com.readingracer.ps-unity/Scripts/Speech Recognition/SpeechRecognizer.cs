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
	/// Speech event arguments. Used to retrieve the event type and 
	/// Hypothesis during a partial and final hypothesis event.
	/// </summary>
	public class SpeechEventArgs : EventArgs
	{
		public enum SpeechEventType {SpeechBegins, SpeechEnds, PartialHypothesisFound, FinalHypothesisFound};
		public string SearchMode;
		public SpeechEventType EventType { get; private set; }
		public Hypothesis Hyp { get; private set; }

		public SpeechEventArgs(SpeechEventType eventType)
		{
			this.EventType = eventType;
			Hyp.SetToEmpy ();
		}

		public SpeechEventArgs(SpeechEventType eventType, Hypothesis hyp)
		{
			this.EventType = eventType;
			this.Hyp = hyp;
		}
	}

	/// <summary>
	/// Speech event handler. Follows MSDN guideliens
	/// </summary>
	public delegate void SpeechEventHandler( SpeechRecognizer sender, SpeechEventArgs e );

	
	/// <summary>
	/// Inherit and override this then pass it to recognitionManager
	/// </summary>
	public class SpeechRecognizer
	{
		/// <summary>
		/// Occurs when speech has changed. Add methods to this to get events.
		/// </summary>
		public event SpeechEventHandler SpeechChanged;

		//are we currently listening
		public bool IsListening {get { return _IsListening; } set { /*Debug.Log ("setting: " + value);*/
				_IsListening = value;}}
		private bool _IsListening;
		public string CurrentSearch {get; private set;} 

		private ps_decoder_t rawDecoder { get{ return TheDecoder.RawDecoder; } } //careful with this. its the opaque struct PocketSphinx uses
		private readonly HashSet<string> searchNames;
		private readonly RecognitionWorker.SpeechEvent speechEvent;

		//REALLY bad idea to modify these. t
		protected SpeechConfig config;
		protected readonly Decoder TheDecoder;

		public const string WORKER_NAME = "RecognitionWorker";
		/// <summary>
		/// The worker which manages the DEFAULT microphone, decoding and executes events.
		/// </summary>
		private static RecognitionWorker worker;

		// example thresholds
//		private double highRMSLevel = 400.0; // tuned by hand 
//		private double lowRMSLevel = 30.0; // relatively silent

		public SpeechRecognizer(SpeechConfig config)
		{
			//added by Rod - 12/7/14
			//fixing a bug caused by using the speechrecognizer in more than one scene. 
			//the speechrecognizer would continue to hold a reference to a worker between scenes even though
			//the worker was not instantiated in the new scene.
			GameObject foundWorker = GameObject.Find(WORKER_NAME);
			if(foundWorker == null || worker.GetComponent<RecognitionWorker>() == null)
				SpeechRecognizer.worker = (new GameObject(WORKER_NAME)).AddComponent<RecognitionWorker>();
			else
				SpeechRecognizer.worker = foundWorker.GetComponent<RecognitionWorker>();
			

			this.TheDecoder = new Decoder(config);
			//this.rawDecoder = TheDecoder.RawDecoder;
			this.config = config;
			
			this.searchNames = new HashSet<string>();
			this.CurrentSearch = string.Empty;
			this.IsListening = false;

			if(this.config.GetSampleRate() != MicController.SAMPLE_RATE)
			{
				Debug.LogError("Critical Error: SpeechConfig sampleRate must be set to: " 
				               + this.config.GetSampleRate());
				Application.Quit();
			}

			this.speechEvent = new RecognitionWorker.SpeechEvent(OnSpeechChanged);
		}

		/// <summary>
		/// Reinitialize the recogizer to use the newConfig. resets ALL SEARCHES!
		/// this will crash if you try to call a search that was set up before this.
		/// </summary>
		/// returns false if it fails or the recognizer was ON.
		/// <param name="newConfig">New config.</param>
		public bool Reinitialize(SpeechConfig newConfig)
		{
			if (IsListening) {
				Debug.Log ("SpeechRecognizer Reinitialize - IsListening is true, will not reinitialize"); 
				return false;
			}

			if(newConfig.GetSampleRate() != MicController.SAMPLE_RATE)
			{
				Debug.LogError("Critical Error: SpeechConfig sampleRate must be set to: " 
				               + this.config.GetSampleRate());
				Application.Quit();
			}

			this.config = newConfig;

			bool isOk = TheDecoder.Reinitialize(newConfig);

			return isOk;
		}

		/// <summary>
		/// Starts SpeechRecognizer
		/// </summary>
		/// <param name="searchName">Search name.</param>
		public void StartRecognizing(string searchName)
		{
			if(IsListening)
			{
				stopRecognizing();
				Debug.Log ("SpeechRecognizer StartRecognizing - IsListening is true, calling stopRecognizing(false)"); 
				return;
			}

			if(MicController.Instance != null){
				MicController.Instance.ResetUtteranceRMSLevels();
			}
			
			this.SetSearch(searchName);
			SpeechRecognizer.worker.BeginWork(this.speechEvent,TheDecoder);
			IsListening = true;
		}

		/// <summary>
		/// Stops SpeechRecognizer. Will raise an OnFinalHypothisesEvent by default
		/// </summary>
		public void stopRecognizing()
		{
			if(!IsListening)
			{
				//Debug.LogWarning("Speech Recognizer stopRecognizing called from " + callingMethod + " - Attempting to stop an already stopped recognizer. Method call ignored.");
				return;
			} 
			else 
			{
				;//Debug.Log ("SpeechRecognizer stopRecognizing called from " + callingMethod + " - will attempt to stop recognizer");
			}

			//Debug.Log("trace");
			IsListening = false;
			if(MicController.Instance != null){
				MicController.Instance.ResetUtteranceRMSLevels();
			}
			SpeechRecognizer.worker.EndWork(TheDecoder);
		}

		/// <summary>
		/// Stops the SpeechRecognizer and turns off the mic if its the only recognizer using it.
		/// Warning turning off the mic is expensive.
		/// </summary>
		/// <param name="raiseFinalEvent">If set to <c>true</c> raise final event.</param>
		public void stopRecognizingAndTurnOffMic()
		{
			stopRecognizing();
			SpeechRecognizer.worker.stopMic();
		}

		#region Search Related
		/// <summary>
		/// Sets the Speech Recognizers search mode. Searches can be added by using the add***model methods
		/// or AddKWSFile, AddKeyphrase, or AddAllphone.
		/// cannot be set while listening is active
		/// </summary>
		/// <returns><c>true</c>, if search was set, <c>false</c> otherwise.</returns>
		/// <param name="searchName">The name of the search you would like to use</param>
		protected bool SetSearch(string searchName)
		{
			if(IsListening)
			{
				Debug.Log("SpeechRecognizer Set Search - Attempting to change search while listening is active. This is a no no.");
				return false;
			}

			//checking if search has already been added to this decoder
			if(!searchNames.Contains(searchName))
			{
				Debug.LogError("Attempted to set a search that does not exist: " + searchName);
				return false;
			}
			
			bool success = PS_Search.ps_set_search(rawDecoder,searchName);
			
			if(!success) return false;
			
			CurrentSearch = searchName;
			return true;
		}
		
		/// <summary>
		/// Removes a search from this Speech Recognizer. Cannot remove the search if it's the current search.
		/// </summary>
		/// <returns><c>true</c>, if search was removed, <c>false</c> otherwise.</returns>
		/// <param name="searchName">Search name.</param>
		public bool RemoveSearch(string searchName)
		{
			if(IsListening)
			{
				Debug.Log("SpeechRecognizer RemoveSearch - Attempting to remove a search while listening. This is not allowed.");
				return false;
			}

			if(searchName == CurrentSearch)
			{
				Debug.Log("Attempting te remove a search that is currently in use. This is not allowed.");
				return false;
			}

			if(!searchNames.Remove(searchName))
			{
				Debug.Log("Attempting to remove a search that does not currently exist. Returning false");
				return false;
			}

			
			return PS_Search.ps_unset_search(rawDecoder,searchName);
		}
		
		/// <summary>
		/// Adds the N grame model to this Speech recognizer. Set the Speech Recognizer to use it with SetSearch.
		/// The model will be read from a file at modelPath. It will error check to see if the
		/// path is valid
		/// </summary>
		/// <returns><c>true</c>, if N grame model was added, <c>false</c> otherwise.</returns>
		/// <param name="modelPath">Model path.</param>
		/// <param name="newSearchName">New search name.</param>
		public bool AddNGrameModel(string modelPath, string newSearchName)
		{
			if(IsListening)
			{
				Debug.Log("SpeechRecognizer AddNGrameModel - Attempting to add a search while listening. This is not allowed");
				return false;
			}

			bool success = PS_Search.ps_set_lm_file(rawDecoder,newSearchName,modelPath);
			
			if(!success) return false;
			
			searchNames.Add(newSearchName);
			return true;
		}
		
		/// <summary>
		/// Adds the FSG model to this SpeechRecognizer. Set the SpeechRecognizer to use it with SetSearch.
		/// The moel will be read from a file at modelPath. It will error check to see if
		/// the path is valid.
		/// </summary>
		/// <returns><c>true</c>, if FSG model was added, <c>false</c> otherwise.</returns>
		/// <param name="model">Model.</param>
		/// <param name="newSearchName">New search name.</param>
		public bool AddFSGModel(FsgModel model, string newSearchName)
		{
			if(IsListening)
			{
				Debug.Log("SpeechRecognizer AddFSGModel - Attempting to add a search while listening. This is not allowed");
				return false;
			}
			
			bool success = PS_Search.ps_set_fsg(rawDecoder,newSearchName,model.RawModel);
			
			if(!success) return false;
			
			searchNames.Add(newSearchName);
			return true;
		}
		
		/// <summary>
		/// Adds the JSGF model to this SpeechRecognizer. Set the SpeechRecognizer to use it with SetSearch.
		/// The moel will be read from a file at modelPath. It will error check to see if
		/// the path is valid.
		/// </summary>
		/// <returns><c>true</c>, if JSGF model was added, <c>false</c> otherwise.</returns>
		/// <param name="modelPath">Model path.</param>
		/// <param name="newSearchName">New search name.</param>
		public bool AddJSGFModel(string modelPath, string newSearchName)
		{
			if(IsListening)
			{
				Debug.Log("SpeechRecognizer AddJFSGModel - Attempting to add a search while listening. This is not allowed");
				return false;
			}
			
			bool success = PS_Search.ps_set_jsgf_file(rawDecoder,newSearchName,modelPath);
			
			if(!success) return false;
			
			searchNames.Add(newSearchName);
			return true;
		}
		
		/// <summary>
		/// Adds a series of keyphrases to spot that are loaded from a file.
		/// Set the SpeechRecogizer to use it with SetSearch.
		/// It will error check to see if the path is valid.
		/// </summary>
		/// <returns><c>true</c>, if KWS file was added, <c>false</c> otherwise.</returns>
		/// <param name="filePath">File path.</param>
		/// <param name="newSearchName">New search name.</param>
		public bool AddKWSFile(string filePath, string newSearchName)
		{
			if(IsListening)
			{
				Debug.Log("SpeechRecognizer AddKWSFile - Attempting to add a search while listening. This is not allowed");
				return false;
			}
			
			if(!System.IO.File.Exists(filePath))
			{
				Debug.LogError("add kws file failed since the file could not be found");
				return false;
			}
			
			bool success = PS_Search.ps_set_kws(rawDecoder,newSearchName,filePath);
			
			if(!success) return false;
			
			searchNames.Add(newSearchName);
			return true;
		}
		
		/// <summary>
		/// Adds a keyphrase for the decoder to spot.
		/// Set the SpeechRecognizer to use it with setsearch
		/// </summary>
		/// <returns><c>true</c>, if keyphrase was added, <c>false</c> otherwise.</returns>
		/// <param name="phrase">Phrase.</param>
		/// <param name="newSearchName">New search name.</param>
		public bool AddKeyphrase(string phrase, string newSearchName)
		{
			if(IsListening)
			{
				Debug.Log("SpeechRecognizer AddKeyphrase - Attempting to add a search while listening. This is not allowed");
				return false;
			}
			
			bool success = PS_Search.ps_set_keyphrase(rawDecoder,newSearchName,phrase);
			
			if(!success) return false;
			
			searchNames.Add(newSearchName);
			return true;
		}
		
		/// <summary>
		/// Adds the allphone model search from a file. Set the SpeechRecognizer to use it with SetSearch.
		/// The moel will be read from a file at modelPath. It will error check to see if
		/// the path is valid.
		/// </summary>
		/// <returns><c>true</c>, if allphone was added, <c>false</c> otherwise.</returns>
		/// <param name="modelPath">Model path.</param>
		/// <param name="newSearchName">New search name.</param>
		public bool AddAllphone(string modelPath, string newSearchName)
		{
			if(IsListening)
			{
				Debug.Log("SpeechRecognizer AddAllphone - Attempting to add a search while listening. This is not allowed");
				return false;
			}
			
			bool success = PS_Search.ps_set_allphone_file(rawDecoder,newSearchName,modelPath);
			
			if(!success) return false;
			
			searchNames.Add(newSearchName);
			return true;
		}

		#endregion

		/// <summary>
		/// Raises the speech changed event. Override this when you want special things to happen
		/// during an event
		/// </summary>
		/// <param name="e">E.</param>
		protected virtual void OnSpeechChanged(SpeechEventArgs e)
		{
			e.SearchMode = CurrentSearch;
			if(SpeechChanged != null)
				SpeechChanged(this,e);
		}

	
		/**
		 * DONT EVER TOUCH THIS WHILE UNITY IS RUNNING
		 * This worker has access and uses the decoder in real time.
		 * modifying this can and probably will break things. dont touch.
		 **/
		private class RecognitionWorker : MonoBehaviour
		{
			public bool IsDecoding {get; private set;}
			public const int CLIP_LENGTH = 3;

			/// <summary>
			/// Speech event. Used by the Speech Recognizer to call SpeechEventHandlers
			/// </summary>
			public delegate void SpeechEvent(SpeechEventArgs e);

			/// <summary>
			/// The info dict. Used to keep track of all the decoders.
			/// </summary>
			private Dictionary<Decoder, RecognizerInfo> infoDict = new Dictionary<Decoder, RecognizerInfo>();

			void Start()
			{
				if(infoDict.Count == 0)
					IsDecoding = false;

				if (MicController.Instance == null)
					MicController.Init(CLIP_LENGTH);

				MicController.Instance.OnNewAudioSamplesFound += UpdateDecoders;
			}

			/**
			 * Main workhorse does the decoding and raises events based on those
			 **/
			void UpdateDecoders(MicController mc, EventArgs e)
			{

				if(!IsDecoding)
				{
					return;
				}

				RecognizerInfo[] infos = new RecognizerInfo[infoDict.Count];
				infoDict.Values.CopyTo(infos,0);

				foreach(RecognizerInfo info in infos)
				{
					if (info.TheDecoder.ProcessPartialRawAudio(mc.AudioSamples, mc.AudioSamplesSize) < 0)
						Debug.LogError("decoder processing error in " + gameObject.name);
					
					Hypothesis hyp = info.TheDecoder.GetPartialHypothesis();

					bool newInSpeech = info.TheDecoder.NewSpeechDetected();
					if(info.InSpeech != newInSpeech)
					{
						info.InSpeech = newInSpeech;
						startOrEndSpeech(newInSpeech, info.RaiseSpeechEvent);
					}

					if(!string.IsNullOrEmpty(hyp.NewSpeech))
						info.RaiseSpeechEvent(new SpeechEventArgs(
							SpeechEventArgs.SpeechEventType.PartialHypothesisFound,
							hyp));
				}

			}
		
			void OnDisable()
			{
				//Debug.LogError("ERROR: " + gameObject.name + " was disabled. This can cause undesirable results");
			}

			void OnDestroy()
			{
				if(IsDecoding)
				{

					IEnumerable<RecognizerInfo> infos = infoDict.Values;

					foreach(RecognizerInfo info in infos)
						info.TheDecoder.EndUtterance();
				}
			}

			/// <summary>
			/// Starts the mic if needed. Adds a decoder and its event to begin decoding.
			/// </summary>
			/// <param name="newSpeechEvent">New speech event.</param>
			/// <param name="newDecoder">New decoder.</param>
			public void BeginWork(SpeechEvent newSpeechEvent, Decoder newDecoder)
			{
				infoDict.Add(newDecoder, new RecognizerInfo(newSpeechEvent,newDecoder,false));
				newDecoder.StartUtterance();

				if(IsDecoding)
					return;

				IsDecoding = true;
			}

			/// <summary>
			/// Stops the mic if there are no decoders left after removal of detracted.
			/// Raises a finalHypothesises event for the removed decoder.
			/// </summary>
			/// <param name="detracted">Detracted.</param>
			/// <param name="raiseFinalEvent"> will raise a finalHypothesisEvent if true</para>
			public void EndWork(Decoder detracted)
			{
				if(!infoDict.ContainsKey(detracted))
				{
					Debug.LogError("Attempting to remove a decoder that was never added to the worker!");
					return;
				}

				
				detracted.EndUtterance();
				Hypothesis hyp = detracted.GetFinalHypothesis();
				//Debug.Log ("before Hyp: " + hyp.FullHypothesis);

				RecognizerInfo info;
				infoDict.TryGetValue(detracted,out info);

          		info.RaiseSpeechEvent(new SpeechEventArgs(
					SpeechEventArgs.SpeechEventType.FinalHypothesisFound,
					hyp));

				infoDict.Remove(detracted);

				if(infoDict.Count == 0)
				{
					IsDecoding = false;
					//Microphone.End(null); //turning off the mic is super expensive!
				}
			}

			/// <summary>
			/// Stops the mic if its safe. that means, not decoding, no listeners
			/// </summary>
			/// <returns><c>true</c>, if mic was stoped, <c>false</c> otherwise.</returns>
			public bool stopMic()
			{
				if(infoDict.Count == 0 && MicController.Instance != null)
				{
					MicController.Instance.UnInit();
					return true;
				}

				return false;
			}

			private void startOrEndSpeech(bool inSpeech, SpeechEvent RaiseSpeechEvent)
			{
				if(inSpeech)
				{
					RaiseSpeechEvent(new SpeechEventArgs(SpeechEventArgs.SpeechEventType.SpeechBegins));
				}
				else
				{
					RaiseSpeechEvent(new SpeechEventArgs(SpeechEventArgs.SpeechEventType.SpeechEnds));
				}
			}


			private class RecognizerInfo
			{
				public SpeechEvent RaiseSpeechEvent {get; private set;}
				public Decoder TheDecoder { get; private set;}
				public bool InSpeech {get; set;}

				public RecognizerInfo(SpeechEvent raiseSpeechEvent, Decoder decoder, bool inSpeech)
				{
					RaiseSpeechEvent = raiseSpeechEvent;
					TheDecoder = decoder;
					InSpeech = inSpeech;
				}
			}
		}

	}


}