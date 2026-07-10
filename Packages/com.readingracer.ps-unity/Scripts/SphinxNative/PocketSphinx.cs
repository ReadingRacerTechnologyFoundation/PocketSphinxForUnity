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
#if UNITY_EDITOR_OSX
#define USE_SPHINXALL
#elif UNITY_EDITOR_WIN
#define USE_WIN_DLL
#elif UNITY_ANDROID || UNITY_STANDALONE_OSX
#define USE_SPHINXALL
#elif UNITY_STANDALONE_WIN || UNITY_WP8
#define USE_WIN_DLL
#elif UNITY_IOS
#define USE_IOS_INTERNAL
#else
#define UNSUPPORTED_PLATFORM
#endif

using System;
using System.Runtime.InteropServices;
using UnityEngine;


/**
 * Namespace for access into the Native Sphinx API
 * There be monsters when using these.
 * 
 * sphinxbase, Sphinx, and a C wrapper I made to
 * make calling certain functions easier called EasyCallWrapper
 * are compiled into a single DLL called sphinxall
 * 
 * native methods are defined using IntPtr's whenever a pointer is involved natively.
 * This can lead to confusing code if everything uses an IntPtr but may actually point
 * to very different types.
 * 
 * Instead we use structs to wrap the IntPtrs in proper types. Native extern methods
 * are wrapped as well in publicly defined methods that take care of how to use
 * the struct wrappers.
 * 
 * DO NOT EVER USE THE IntPtr TYPES DIRECTLY FROM THE STRUCT!!!
 * OR ELSE BAD THINGS CAN HAPPEN. USE THE PROPER METHODS!
 * 
 * In order to add or remove a extern method please remember to modify
 * that methods pubilc (non extern) wrapper as well.
 * 
 * Well why not Opaque unsafe types?
 * If this wasn't in Unity who like to throw a fit over those. I would have
 * 
 * Created by Rodrigo Cano 9/17/2014
 **/
namespace Rrtf.Sphinx.SphinxNative
{
	class PocketSphinx
	{
		
		public static arg_t ps_argugments()
		{
			return new arg_t(ps_args());
		}
		
		/**
         * Initialize the decoder from a configuration object.
         *
         * @note The decoder retains ownership of the pointer
         * <code>config</code>, so you must not attempt to free it manually.
         * If you wish to reuse it elsewhere, call cmd_ln_retain() on it.
         *
         * @param config a command-line structure, as created by
         * cmd_ln_parse_r() or cmd_ln_parse_file_r().
         */
		//ps_decoder_t *ps_init(cmd_ln_t *config);
		public static ps_decoder_t ps_initialize(
			cmd_ln_t config)
		{
			return new ps_decoder_t(ps_init((IntPtr)config));
		}
		
		/**
         * Reinitialize the decoder with updated configuration.
         *
         * This function allows you to switch the acoustic model, dictionary,
         * or other configuration without creating an entirely new decoding
         * object.
         *
         * @note The decoder retains ownership of the pointer
         * <code>config</code>, so you must not attempt to free it manually.
         * If you wish to reuse it elsewhere, call cmd_ln_retain() on it.
         *
         * @param ps Decoder.
         * @param config An optional new configuration to use.  If this is
         *               NULL, the previous configuration will be reloaded,
         *               with any changes applied.
         * @return true for success
         */
		//int ps_reinit(ps_decoder_t *ps, cmd_ln_t *config);
		public static bool ps_reinit(
			ps_decoder_t ps,
			cmd_ln_t config)
		{
			return ps_reinit((IntPtr)ps, (IntPtr)config) == 0;
		}
		
		/**
         * Finalize the decoder.
         *
         * This releases all resources associated with the decoder, including
         * any language models or grammars which have been added to it, and
         * the initial configuration object passed to ps_init().
         *
         * @param ps Decoder to be freed.
         * @return true if reference count = 0 after its freed/successful.
         */
		//int ps_free(ps_decoder_t *ps);
		public static bool ps_free(
			ps_decoder_t ps)
		{
			return ps_free((IntPtr)ps) == 0;
		}
		
		/**
         * Get the configuration object for this decoder.
         *
         * @return The configuration object for this decoder.  The decoder
         *         retains ownership of this pointer, so you should not
         *         attempt to free it manually.  Use cmd_ln_retain() if you
         *         wish to reuse it elsewhere.
         */
		//cmd_ln_t *ps_get_config(ps_decoder_t *ps);
		public static cmd_ln_t ps_get_config(
			ps_decoder_t ps)
		{
			return new cmd_ln_t(ps_get_config((IntPtr)ps));
		}
		
		/**
         * Get the feature extraction object for this decoder.
         *
         * @return The feature extraction object for this decoder.  The
         *         decoder retains ownership of this pointer, so you should
         *         not attempt to free it manually.  Use fe_retain() if you
         *         wish to reuse it elsewhere.
         */
		//fe_t *ps_get_fe(ps_decoder_t *ps);
		public static fe_t ps_get_fe(
			ps_decoder_t ps)
		{
			return new fe_t(ps_get_fe((IntPtr)ps));
		}
		
		
		/**
         * Get the dynamic feature computation object for this decoder.
         *
         * @return The dynamic feature computation object for this decoder.  The
         *         decoder retains ownership of this pointer, so you should
         *         not attempt to free it manually.  Use feat_retain() if you
         *         wish to reuse it elsewhere.
         */
		//feat_t *ps_get_feat(ps_decoder_t *ps);
		public static feat_t ps_get_feat(
			ps_decoder_t ps)
		{
			return new feat_t (ps_get_feat ((IntPtr)ps));
		}
		
		
		/**
         * Adapt current acoustic model using a linear transform.
         *
         * @param mllr The new transform to use, or NULL to update the existing
         *              transform.  The decoder retains ownership of this pointer,
         *              so you should not attempt to free it manually.  Use
         *              ps_mllr_retain() if you wish to reuse it
         *              elsewhere.
         * @return The updated transform object for this decoder, or
         *         NULL on failure.
         */
		//ps_mllr_t *ps_update_mllr(ps_decoder_t *ps, ps_mllr_t *mllr);
		public static ps_mllr_t ps_update_mllr( ps_decoder_t ps, ps_mllr_t mllr)
		{
			return new ps_mllr_t(ps_update_mllr((IntPtr)ps, (IntPtr)mllr));;
		}
		
		/**
         * Reload the pronunciation dictionary from a file.
         *
         * This function replaces the current pronunciation dictionary with
         * the one stored in dictfile.  This also causes the active search
         * module(s) to be reinitialized, in the same manner as calling
         * ps_add_word() with update=TRUE.
         *
         * @param dictfile Path to dictionary file to load.
         * @param fdictfile Path to filler dictionary to load, or NULL to keep
         *                  the existing filler dictionary.
         * @param format Format of the dictionary file, or NULL to determine
         *               automatically (currently unused,should be NULL)
         */
		//int ps_load_dict(ps_decoder_t *ps, char const *dictfile,
		//                 char const *fdictfile, char const *format);
		public static bool ps_load_dict(ps_decoder_t ps,
		                                string dictfile,
		                                String fdictfile,
		                                String format)
		{
			if(!System.IO.File.Exists(dictfile) || !System.IO.File.Exists(fdictfile))
			{
				Debug.LogError("ps_load_dict failed: could not find file");
				return false;
			}
			
			return ps_load_dict((IntPtr)ps, dictfile, fdictfile, format) == 0;
		}
		
		
		/**
         * Dump the current pronunciation dictionary to a file.
         *
         * This function dumps the current pronunciation dictionary to a tex
         *
         * @param dictfile Path to file where dictionary will be written.
         * @param format Format of the dictionary file, or NULL for the
         *               default (text) format (currently unused, should be NULL)
         */
		//int ps_save_dict(ps_decoder_t *ps, char const *dictfile, char const *format);
		public static bool ps_save_dict(ps_decoder_t ps,
		                                string dictfile,
		                                String format)
		{
			return ps_save_dict((IntPtr)ps, dictfile, format) == 0;
		}
		
		
		/**
        * Add a word to the pronunciation dictionary.
        *
        * This function adds a word to the pronunciation dictionary and the
        * current language model (but, obviously, not to the current FSG if
        * FSG mode is enabled).  If the word is already present in one or the
        * other, it does whatever is necessary to ensure that the word can be
        * recognized.
        *
        * @param word Word string to add.
        * @param phones Whitespace-separated list of phoneme strings
        *               describing pronunciation of <code>word</code>.
        * @param update If TRUE, update the search module (whichever one is
        *               currently active) to recognize the newly added word.
        *               If adding multiple words, it is more efficient to
        *               pass FALSE here in all but the last word.
        * @return The internal ID (>= 0) of the newly added word, or <0 on
        *         failure.
        **/
		//int ps_add_word(ps_decoder_t *ps, char const *word, char const *phones, int update);
		public static int ps_add_word(
			ps_decoder_t ps,
			string word,
			string phones,
			bool update)
		{
			return ps_add_word((IntPtr)ps, word, phones, update ? 1 : 0);
		}
		
		/** 
         * Lookup for the word in the dictionary and return phone transcription
         * for it.
         *
         * @param ps Pocketsphinx decoder
         * @param word Word to look for
         *
         * @return Whitespace-spearated phone string describing the pronunciation of the <code>word</code>
         *         or NULL if word is not present in the dictionary. The string is
         *         allocated and must be freed by the user.
         */
		//char *ps_lookup_word(ps_decoder_t *ps, const char *word);
		public static String ps_lookup_word(
			ps_decoder_t ps,
			string word)
		{
			IntPtr returnWord = ps_lookup_word((IntPtr)ps, word);
			
			if (returnWord == IntPtr.Zero)
				return null;
			else
				return Marshal.PtrToStringAnsi(returnWord);
		}
		
		/**
         * Decode a raw audio stream.
         *
         * No headers are recognized in this files.  The configuration
         * parameters <tt>-samprate</tt> and <tt>-input_endian</tt> are used
         * to determine the sampling rate and endianness of the stream,
         * respectively.  Audio is always assumed to be 16-bit signed PCM.
         *
         * @param ps Decoder.
         * @param rawfh Previously opened file stream.
         * @param maxsamps Maximum number of samples to read from rawfh, or -1
         *                 to read until end-of-file.
         * @return Number of samples of audio.
         */
		//int ps_decode_raw(ps_decoder_t *ps, FILE *rawfh, long maxsamps);
		public static int ps_decode_raw(
			ps_decoder_t ps,
			FILE rawfh,
			int maxsamps)
		{
			return ps_decode_raw((IntPtr)ps, (IntPtr)rawfh, maxsamps);
		}
		
		/**
         * Start processing of the stream of speech. Channel parameters like
         * noise-level are maintained for the stream and reused among utterances.
         * Times returned in segment iterators are also stream-wide.
         *
         * @return true for success
         */
		//int ps_start_stream(ps_decoder_t *ps);
		public static bool ps_start_stream(
			ps_decoder_t ps)
		{
			return ps_start_stream((IntPtr)ps) == 0;
		}
		
		
		/**
         * The same as the ps_decode_raw but handles opening and closing the file
         * inside the function body
         * only requires the file path
         **/
		public static int ps_decode_raw_easy( ps_decoder_t ps, string filePath,
		                                     int maxsamps)
		{
			if(!System.IO.Directory.Exists(filePath))
			{
				Debug.LogError("ps_decode_raw_easy failed.  Could not find " + filePath);
				return -1;
			}
			
			return ps_decode_raw_easy((IntPtr)ps, filePath, maxsamps);
		}
		
		/**
         * Start utterance processing.
         *
         * This function should be called before any utterance data is passed
         * to the decoder.  It marks the start of a new utterance and
         * reinitializes internal data structures.
         *
         * @param ps Decoder to be started.
         * @return true for success
         */
		//int ps_start_utt(ps_decoder_t *ps,);
		public static bool ps_start_utt(ps_decoder_t ps)
		{
			return ps_start_utt((IntPtr)ps) >= 0;
		}
		
		/**
         * Decode raw audio data.
         *
         * @param ps Decoder.
         * @param no_search If non-zero, perform feature extraction but don't
         *                  do any recognition yet.  This may be necessary if
         *                  your processor has trouble doing recognition in
         *                  real-time.
         * @param full_utt If non-zero, this block of data is a full utterance
         *                 worth of data.  This may allow the recognizer to
         *                 produce more accurate results.
         * @return Number of frames of data searched, or <0 for error.
         */
		//int ps_process_raw(ps_decoder_t *ps, int16 const *data,
		//                   size_t n_samples,
		//                   int no_search,
		//                   int full_utt);
		public static int ps_process_raw(
			ps_decoder_t ps,
			short[] data,
			uint n_samples,
			bool no_search,
			bool full_utt)
		{
			return ps_process_raw((IntPtr)ps, data, n_samples, no_search ? 1 : 0, full_utt ? 1 : 0);
		}
		
		/**
             * Get the number of frames of data searched.
             *
             * Note that there is a delay between this and the number of frames of
             * audio which have been input to the system.  This is due to the fact
             * that acoustic features are computed using a sliding window of
             * audio, and dynamic features are computed over a sliding window of
             * acoustic features.
             *
             * @param ps Decoder.
             * @return Number of frames of speech data which have been recognized
             * so far.
             */
		//int ps_get_n_frames(ps_decoder_t *ps);
		public static int ps_get_n_frames(ps_decoder_t ps)
		{
			return ps_get_n_frames((IntPtr)ps);
		}
		
		/**
         * End utterance processing.
         *
         * @param ps Decoder.
         * @return true for success
         */
		//int ps_end_utt(ps_decoder_t *ps);
		public static bool ps_end_utt(
			ps_decoder_t ps)
		{
			return ps_end_utt((IntPtr)ps) >= 0;
		}
		
		/**
         * Get hypothesis string and path score.
         * 
         * REMEMBER TO USE ps_set_search before or BOOM CRASH!
         *
         * @param ps Decoder.
         * @param out_best_score Output: path score corresponding to returned string.
         * @return String containing best hypothesis at this point in
         *         decoding.  NULL if no hypothesis is available.
         */
		//char const *ps_get_hyp(ps_decoder_t *ps, int32 *out_best_score)
		public static String ps_get_hyp(ps_decoder_t ps,
		                                ref int out_best_score)
		{
			IntPtr hyp = ps_get_hyp((IntPtr)ps, ref out_best_score);

			if (hyp == IntPtr.Zero)
				return null;
			else
			{
				return Marshal.PtrToStringAnsi(hyp);
			}
		}
		
		/**
         * Get posterior probability.
         *
         * @note Unless the -bestpath option is enabled, this function will
         * always return zero (corresponding to a posterior probability of
         * 1.0).  Even if -bestpath is enabled, it will also return zero when
         * called on a partial result.  Ongoing research into effective
         * confidence annotation for partial hypotheses may result in these
         * restrictions being lifted in future versions.
         *
         * @param ps Decoder.
         * @return Posterior probability of the best hypothesis.
         */
		//int32 ps_get_prob(ps_decoder_t *ps);
		public static int ps_get_prob(ps_decoder_t ps)
		{
			return ps_get_prob((IntPtr)ps);
		}
		
		/**
         * Checks if the last feed audio buffer contained speech
         *
         * @param ps Decoder.
         * @return 1 if last buffer contained speech, 0 - otherwise
         */
		//uint8 ps_get_in_speech(ps_decoder_t* ps);
		public static bool ps_get_in_speech(ps_decoder_t ps)
		{
			return ps_get_in_speech((IntPtr)ps) == 1;
		}
		
		#region External Calls
		
		#if USE_SPHINXALL
		
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_args();
		
		//ps_decoder_t *ps_init(cmd_ln_t *config);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_init(
			IntPtr config);
		
		//int ps_reinit(ps_decoder_t *ps, cmd_ln_t *config);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_reinit(
			IntPtr ps,
			IntPtr config);
		
		//int ps_free(ps_decoder_t *ps);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_free(
			IntPtr ps);
		
		//cmd_ln_t *ps_get_config(ps_decoder_t *ps);
		[DllImport("sphinxall",
		           SetLastError = true,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_get_config(
			IntPtr ps);
		
		//fe_t *ps_get_fe(ps_decoder_t *ps);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_get_fe(
			IntPtr ps);
		
		//feat_t *ps_get_feat(ps_decoder_t *ps);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_get_feat(
			IntPtr ps);
		
		//ps_mllr_t *ps_update_mllr(ps_decoder_t *ps, ps_mllr_t *mllr);
		[DllImport("sphinxall",
		           SetLastError = true,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_update_mllr(
			IntPtr ps,
			IntPtr mllr);
		
		//int ps_load_dict(ps_decoder_t *ps, char const *dictfile,
		//                 char const *fdictfile, char const *format);
		[DllImport("sphinxall",
		           CharSet=CharSet.Ansi,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_load_dict(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string dictfile,
			String fdictfile,
			String format);
		
		//int ps_save_dict(ps_decoder_t *ps, char const *dictfile, char const *format);
		[DllImport("sphinxall",
		           CharSet=CharSet.Ansi,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_save_dict(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string dictfile,
			String format);
		
		//int ps_add_word(ps_decoder_t *ps, char const *word, char const *phones, int update);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_add_word(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string word,
			[MarshalAs(UnmanagedType.LPStr)] string phones,
			int update);
		
		//char *ps_lookup_word(ps_decoder_t *ps, const char *word);
		[DllImport("sphinxall",
		           SetLastError = true,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_lookup_word(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string word);
		
		//int ps_decode_raw(ps_decoder_t *ps, FILE *rawfh, long maxsamps);
		[DllImport("sphinxall",
		           CharSet=CharSet.Ansi,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_decode_raw(
			IntPtr ps,
			IntPtr rawfh,
			int maxsamps);
		
		//int ps_start_stream(ps_decoder_t *ps);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_start_stream(
			IntPtr ps);
		
		[DllImport("sphinxall",
		           CharSet=CharSet.Ansi,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_decode_raw_easy(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string rawfhDir,
			int maxsamps);
		
		//int ps_start_utt(ps_decoder_t *ps);
		[DllImport("sphinxall",
		           CharSet=CharSet.Ansi,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_start_utt(
			IntPtr ps);
		
		
		//int ps_process_raw(ps_decoder_t *ps, int16 const *data,
		//                   size_t n_samples,
		//                   int no_search,
		//                   int full_utt);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_process_raw(
			IntPtr ps,
			short[] data,
			uint n_samples,
			int no_search,
			int full_utt);
		
		//int ps_get_n_frames(ps_decoder_t *ps);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_get_n_frames(
			IntPtr ps);
		
		//int ps_end_utt(ps_decoder_t *ps);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_end_utt(
			IntPtr ps);
		
		//char const *ps_get_hyp(ps_decoder_t *ps, int32 *out_best_score);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_get_hyp(
			IntPtr ps,
			ref int out_best_score);
		
		//int32 ps_get_prob(ps_decoder_t *ps);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_get_prob(
			IntPtr ps);
		
		//uint8 ps_get_in_speech(ps_decoder_t* ps);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static byte ps_get_in_speech(
			IntPtr ps);
		
		#elif USE_IOS_INTERNAL
		
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_args();
		
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_init(
			IntPtr config);
		
		//int ps_reinit(ps_decoder_t *ps, cmd_ln_t *config);
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_reinit(
			IntPtr ps,
			IntPtr config);
		
		//int ps_free(ps_decoder_t *ps);
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_free(
			IntPtr ps);
		
		//cmd_ln_t *ps_get_config(ps_decoder_t *ps);
		[DllImport("__Internal",
		           SetLastError = true,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_get_config(
			IntPtr ps);
		
		//fe_t *ps_get_fe(ps_decoder_t *ps);
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_get_fe(
			IntPtr ps);
		
		//feat_t *ps_get_feat(ps_decoder_t *ps);
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_get_feat(
			IntPtr ps);
		
		//ps_mllr_t *ps_update_mllr(ps_decoder_t *ps, ps_mllr_t *mllr);
		[DllImport("__Internal",
		           SetLastError = true,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_update_mllr(
			IntPtr ps,
			IntPtr mllr);
		
		//int ps_load_dict(ps_decoder_t *ps, char const *dictfile,
		//                 char const *fdictfile, char const *format);
		[DllImport("__Internal",
		           CharSet=CharSet.Ansi,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_load_dict(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string dictfile,
			String fdictfile,
			String format);
		
		//int ps_save_dict(ps_decoder_t *ps, char const *dictfile, char const *format);
		[DllImport("__Internal",
		           CharSet=CharSet.Ansi,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_save_dict(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string dictfile,
			String format);
		
		//int ps_add_word(ps_decoder_t *ps, char const *word, char const *phones, int update);
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_add_word(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string word,
			[MarshalAs(UnmanagedType.LPStr)] string phones,
			int update);
		
		//char *ps_lookup_word(ps_decoder_t *ps, const char *word);
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_lookup_word(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string word);
		
		//int ps_decode_raw(ps_decoder_t *ps, FILE *rawfh long maxsamps);
		[DllImport("__Internal",
		           CharSet=CharSet.Ansi,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_decode_raw(
			IntPtr ps,
			IntPtr rawfh,
			int maxsamps);
		
		//int ps_start_stream(ps_decoder_t *ps);
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_start_stream(
			IntPtr ps);
		
		[DllImport("__Internal",
		           CharSet=CharSet.Ansi,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_decode_raw_easy(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string rawfhDir,
			int maxsamps);
		
		//int ps_start_utt(ps_decoder_t *ps);
		[DllImport("__Internal",
		           CharSet=CharSet.Ansi,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_start_utt(IntPtr ps);
		
		//int ps_process_raw(ps_decoder_t *ps, int16 const *data,
		//                   size_t n_samples,
		//                   int no_search,
		//                   int full_utt);
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_process_raw(
			IntPtr ps,
			short[] data,
			uint n_samples,
			int no_search,
			int full_utt);
		
		//int ps_get_n_frames(ps_decoder_t *ps);
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_get_n_frames(
			IntPtr ps);
		
		//int ps_end_utt(ps_decoder_t *ps);
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_end_utt(
			IntPtr ps);
		
		//char const *ps_get_hyp(ps_decoder_t *ps, int32 *out_best_score);
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_get_hyp(
			IntPtr ps,
			ref int out_best_score);
		
		//int32 ps_get_prob(ps_decoder_t *ps);
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_get_prob(IntPtr ps);      
		
		//uint8 ps_get_in_speech(ps_decoder_t* ps);
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static byte ps_get_in_speech(
			IntPtr ps);
		
		#else
		//UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		
		[DllImport("pocketsphinx.dll",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_args();
		
		//ps_decoder_t *ps_init(cmd_ln_t *config);
		[DllImport("pocketsphinx.dll",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_init(
			IntPtr config);
		
		//int ps_reinit(ps_decoder_t *ps, cmd_ln_t *config);
		[DllImport("pocketsphinx.dll",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_reinit(
			IntPtr ps,
			IntPtr config);
		
		//int ps_free(ps_decoder_t *ps);
		[DllImport("pocketsphinx.dll",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_free(
			IntPtr ps);
		
		//cmd_ln_t *ps_get_config(ps_decoder_t *ps);
		[DllImport("pocketsphinx.dll",
		           SetLastError = true,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_get_config(
			IntPtr ps);
		
		//fe_t *ps_get_fe(ps_decoder_t *ps);
		[DllImport("pocketsphinx.dll",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_get_fe(
			IntPtr ps);
		
		//feat_t *ps_get_feat(ps_decoder_t *ps);
		[DllImport("pocketsphinx.dll",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_get_feat(
			IntPtr ps);
		
		//ps_mllr_t *ps_update_mllr(ps_decoder_t *ps, ps_mllr_t *mllr);
		[DllImport("pocketsphinx.dll",
		           SetLastError = true,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_update_mllr(
			IntPtr ps,
			IntPtr mllr);
		
		//int ps_load_dict(ps_decoder_t *ps, char const *dictfile,
		//                 char const *fdictfile, char const *format);
		[DllImport("pocketsphinx.dll",
		           CharSet = CharSet.Ansi,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_load_dict(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string dictfile,
			String fdictfile,
			String format);
		
		//int ps_save_dict(ps_decoder_t *ps, char const *dictfile, char const *format);
		[DllImport("pocketsphinx.dll",
		           CharSet = CharSet.Ansi,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_save_dict(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string dictfile,
			String format);
		
		//int ps_add_word(ps_decoder_t *ps, char const *word, char const *phones, int update);
		[DllImport("pocketsphinx.dll",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_add_word(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string word,
			[MarshalAs(UnmanagedType.LPStr)] string phones,
			int update);
		
		//char *ps_lookup_word(ps_decoder_t *ps, const char *word);
		[DllImport("pocketsphinx.dll",
		           SetLastError = true,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_lookup_word(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string word);
		
		
		//int ps_decode_raw(ps_decoder_t *ps, FILE *rawfh, long maxsamps);
		[DllImport("pocketsphinx.dll",
		           CharSet = CharSet.Ansi,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_decode_raw(
			IntPtr ps,
			IntPtr rawfh,
			int maxsamps);
		
		//int ps_start_stream(ps_decoder_t *ps);
		[DllImport("pocketsphinx.dll",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_start_stream(
			IntPtr ps);
		
		[DllImport("EasyCallWrapper.dll",
		           CharSet = CharSet.Ansi,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_decode_raw_easy(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string rawfhDir,
			int maxsamps);
		
		//int ps_start_utt(ps_decoder_t *ps);
		[DllImport("pocketsphinx.dll",
		           CharSet = CharSet.Ansi,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_start_utt(IntPtr ps);
		
		//int ps_process_raw(ps_decoder_t *ps, int16 const *data,
		//                   size_t n_samples,
		//                   int no_search,
		//                   int full_utt);
		[DllImport("pocketsphinx.dll",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_process_raw(
			IntPtr ps,
			short[] data,
			uint n_samples,
			int no_search,
			int full_utt);
		
		//int ps_get_n_frames(ps_decoder_t *ps);
		[DllImport("pocketsphinx.dll",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_get_n_frames(
			IntPtr ps);
		
		//int ps_end_utt(ps_decoder_t *ps);
		[DllImport("pocketsphinx.dll",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_end_utt(
			IntPtr ps);
		
		//char const *ps_get_hyp(ps_decoder_t *ps, int32 *out_best_score);
		[DllImport("pocketsphinx.dll",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_get_hyp(
			IntPtr ps,
			ref int out_best_score);
		
		//int32 ps_get_prob(ps_decoder_t *ps);
		[DllImport("pocketsphinx.dll",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_get_prob(IntPtr ps);
		
		//uint8 ps_get_in_speech(ps_decoder_t* ps);
		[DllImport("pocketsphinx.dll",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static byte ps_get_in_speech(
			IntPtr ps);
		
		#endif
		#endregion
	}
}
