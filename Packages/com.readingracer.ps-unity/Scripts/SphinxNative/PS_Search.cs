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
	class PS_Search
	{

        /**
         * Actives search with the provided name.
         *
         * Activates search with the provided name. The search must be added before
         * using either ps_set_fsg(), ps_set_lm() or ps_set_kws().
         *
         * @return true on success
         */
        //int ps_set_search(ps_decoder_t *ps, const char *name);
        public static bool ps_set_search(ps_decoder_t ps, string name)
        {
            return ps_set_search((IntPtr)ps, name) == 0;
        }

        /**
         * Returns name of curent search in decoder
         *
         * @see ps_set_search
         */
        //const char* ps_get_search(ps_decoder_t *ps);
        public static string ps_get_search(ps_decoder_t ps)
        {
            return Marshal.PtrToStringAnsi(ps_get_search((IntPtr)ps));
        }

        /**
         * Unsets the search and releases related resources.
         *
         * Unsets the search previously added with
         * using either ps_set_fsg(), ps_set_lm() or ps_set_kws().
         *
         * @see ps_set_fsg
         * @see ps_set_lm
         * @see ps_set_kws
         */
        //int ps_unset_search(ps_decoder_t *ps, const char *name);
        public static bool ps_unset_search(ps_decoder_t ps, string name)
        {
            return ps_unset_search((IntPtr)ps, name) == 0;
        }

        /**
         * Returns iterator over current searches 
         *
         * @see ps_set_search
         */
        //ps_search_iter_t* ps_search_iter(ps_decoder_t* ps);
        public static ps_search_iter_t ps_search_iter(ps_decoder_t ps)
        {
            return new ps_search_iter_t(ps_search_iter((IntPtr)ps));
        }

        /**
         * Updates search iterator to point to the next position.
         * 
         * This function automatically frees the iterator object upon reaching
         * the final entry.
         * @see ps_set_search
         */
        //ps_search_iter_t *ps_search_iter_next(ps_search_iter_t *itor);
        public static ps_search_iter_t ps_search_iter_next(ps_search_iter_t itor)
        {
            return new ps_search_iter_t(ps_search_iter_next((IntPtr)itor));
        }

        /**
         * Retrieves the name of the search the iterator points to.
         *
         * @see ps_set_search
         */
        //const char* ps_search_iter_val(ps_search_iter_t *itor);
        public static String ps_search_iter_val(ps_search_iter_t itor)
        {
            IntPtr retVal = ps_search_iter_val((IntPtr)itor);

            return retVal == IntPtr.Zero ? null :
                Marshal.PtrToStringAnsi(retVal);
        }

        /**
         * Delete an unfinished search iterator
         *
         * @see ps_set_search
         */
        //void ps_search_iter_free(ps_search_iter_t* itor);
        public static void ps_search_iter_free(ps_search_iter_t itor)
        {
            ps_search_iter_free((IntPtr)itor);
        }

        /**
         * Get the language model set object for this decoder.
         *
         * If N-Gram decoding is not enabled, this will return NULL.  You will
         * need to enable it using ps_set_lmset().
         *
         * @return The language model set object for this decoder.  The
         *         decoder retains ownership of this pointer, so you should
         *         not attempt to free it manually.  Use ngram_model_retain()
         *         if you wish to reuse it elsewhere.
         */
        //ngram_model_t *ps_get_lm(ps_decoder_t *ps, const char *name);
        public static ngram_model_t ps_get_lm(ps_decoder_t ps, string name)
        {
            return new ngram_model_t(ps_get_lm((IntPtr)ps, name));
        }

        /**
         * Adds new search based on N-gram language model.
         *
         * Associates N-gram search with the provided name. The search can be activated
         * using ps_set_search().
         *
         * @see ps_set_search.
         */
        //int ps_set_lm(ps_decoder_t *ps, const char *name, ngram_model_t *lm);
        public static bool ps_set_lm(ps_decoder_t ps, string name, ngram_model_t lm)
        {
            return ps_set_lm((IntPtr)ps, name, (IntPtr)lm) == 0;
        }

        /**
         * Adds new search based on N-gram language model.
         *
         * Convenient method to load N-gram model and create a search.
         * 
         * @see ps_set_lm
         */
        //int ps_set_lm_file(ps_decoder_t *ps, const char *name, const char *path);
        public static bool ps_set_lm_file(ps_decoder_t ps, string name, string path)
        {
			if(!System.IO.File.Exists(path))
			{
				Debug.LogError("ps_set_lm_file failed. Could not find " + path);
				return false;
			}

            return ps_set_lm_file((IntPtr)ps, name, path) == 0;
        }

        /**
         * Get the finite-state grammar set object for this decoder.
         *
         * If FSG decoding is not enabled, this returns NULL.  Call
         * ps_set_fsgset() to enable it.
         *
         * @return The current FSG set object for this decoder, or
         *         NULL if none is available.
         */
        //fsg_model_t *ps_get_fsg(ps_decoder_t *ps, const char *name);
        public static fsg_model_t ps_get_fsg(ps_decoder_t ps, string name)
        {
            return new fsg_model_t(ps_get_fsg((IntPtr)ps, name));
        }


        /**
         * Adds new search based on finite state grammar.
         *
         * Associates FSG search with the provided name. The search can be activated
         * using ps_set_search().
         *
         * @see ps_set_search
         */
        //int ps_set_fsg(ps_decoder_t *ps, const char *name, fsg_model_t *fsg);
        public static bool ps_set_fsg(ps_decoder_t ps, string name, fsg_model_t fsg)
        {
            return ps_set_fsg((IntPtr)ps, name, (IntPtr)fsg) == 0;
        }

        /**
         * Adds new search using JSGF model.
         *
         * Convenient method to load JSGF model and create a search.
         *
         * @see ps_set_fsg
         */
        public static bool ps_set_jsgf_file(ps_decoder_t ps, string name, string path)
        {
			if (!System.IO.File.Exists (path))
			{
				Debug.LogError("ps_set_jsgf_file failed. Could not find " + path);
				return false;
			}

            return ps_set_jsgf_file((IntPtr)ps, name, path) == 0;
        }

        /**
         * Get the current Key phrase to spot
         *
         * If KWS is not enabled, this returns NULL. Call
         * ps_update_kws() to enable it.
         *
         * @return The current keyphrase to spot
         */
        //const char* ps_get_kws(ps_decoder_t *ps, const char *name);
        public static String ps_get_kws(ps_decoder_t ps, string name)
        {
            IntPtr kwptr = ps_get_kws((IntPtr)ps, name);
            if (kwptr == IntPtr.Zero)
                return null;
            else
                return Marshal.PtrToStringAnsi(kwptr);
        }

        /**
         * Adds keywords from a file to spotting
         *
         * Associates KWS search with the provided name. The search can be activated
         * using ps_set_search().
         *
         * @see ps_set_search
         */
        //int ps_set_kws(ps_decoder_t *ps, const char *name, const char *keyfile);
        public static bool ps_set_kws(ps_decoder_t ps, string name, string keyfile)
        {
            return ps_set_kws((IntPtr)ps, name, keyfile) == 0;
        }

        /**
         * Adds new keyword to spot
         *
         * Associates KWS search with the provided name. The search can be activated
         * using ps_set_search().
         *
         * @see ps_set_search
         */
        //int ps_set_keyphrase(ps_decoder_t *ps, const char *name, const char *keyphrase);
        public static bool ps_set_keyphrase(ps_decoder_t ps, string name, string keyphrase)
        {
            return ps_set_keyphrase((IntPtr)ps, name, keyphrase) == 0;
        }

        /**
         * Adds new search based on phone N-gram language model.
         *
         * Associates N-gram search with the provided name. The search can be activated
         * using ps_set_search().
         *
         * @see ps_set_search.
         */
        //int ps_set_allphone(ps_decoder_t *ps, const char *name, ngram_model_t *lm);
        public static bool ps_set_allphone(ps_decoder_t ps, string name, ngram_model_t lm)
        {
            return ps_set_allphone((IntPtr)ps, name, (IntPtr)lm) == 0;
        }


        /**
         * Adds new search based on phone N-gram language model.
         *
         * Convenient method to load N-gram model and create a search.
         * 
         * @see ps_set_allphone
         */
        //int ps_set_allphone_file(ps_decoder_t *ps, const char *name, const char *path);
        public static bool ps_set_allphone_file(ps_decoder_t ps, string name, string path)
        {
			if(!System.IO.File.Exists(path))
			{
				Debug.LogError("ps_set_allphone_file failed. Could not find " + path);
				return false;
			}

            return ps_set_allphone_file((IntPtr)ps, name, path) == 0;
        }

		#region External Calls

		#if USE_SPHINXALL
		
		//int ps_set_search(ps_decoder_t *ps, const char *name);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_set_search(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string name);
		
		//const char* ps_get_search(ps_decoder_t *ps);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_get_search(
			IntPtr ps);
		
		//int ps_unset_search(ps_decoder_t *ps, const char *name);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_unset_search(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string name);
		
		//ps_search_iter_t* ps_search_iter(ps_decoder_t* ps);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_search_iter(
			IntPtr ps);
		
		//ps_search_iter_t *ps_search_iter_next(ps_search_iter_t *itor);
		[DllImport("sphinxall",
		           SetLastError = true,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_search_iter_next(
			IntPtr itor);
		
		//const char* ps_search_iter_val(ps_search_iter_t *itor);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_search_iter_val(
			IntPtr itor);
		
		//void ps_search_iter_free(ps_search_iter_t* itor);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static void ps_search_iter_free(
			IntPtr itor);
		
		//ngram_model_t *ps_get_lm(ps_decoder_t *ps, const char *name);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_get_lm(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string name);
		
		//int ps_set_lm(ps_decoder_t *ps, const char *name, ngram_model_t *lm);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_set_lm(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string name,
			IntPtr lm);
		
		//int ps_set_lm_file(ps_decoder_t *ps, const char *name, const char *path);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_set_lm_file(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string name,
			[MarshalAs(UnmanagedType.LPStr)] string path);
		
		//fsg_model_t *ps_get_fsg(ps_decoder_t *ps, const char *name);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_get_fsg(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string name);
		
		//int ps_set_fsg(ps_decoder_t *ps, const char *name, fsg_model_t *fsg);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_set_fsg(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string name,
			IntPtr fsg);
		
		//int ps_set_jsgf_file(ps_decoder_t *ps, const char *name, const char *path);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_set_jsgf_file(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string name,
			[MarshalAs(UnmanagedType.LPStr)] string path);
		
		//const char* ps_get_kws(ps_decoder_t *ps, const char *name);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_get_kws(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string name);
		
		//int ps_set_kws(ps_decoder_t *ps, const char *name, const char *keyfile);
		[DllImport("sphinxall",
		           SetLastError = true,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_set_kws(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string name,
			[MarshalAs(UnmanagedType.LPStr)] string keyfile);
		
		//int ps_set_keyphrase(ps_decoder_t *ps, const char *name, const char *keyphrase);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_set_keyphrase(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string name,
			[MarshalAs(UnmanagedType.LPStr)] string keyphrase);
		
		//int ps_set_allphone(ps_decoder_t *ps, const char *name, ngram_model_t *lm);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_set_allphone(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string name,
			IntPtr lm);
		
		//int ps_set_allphone_file(ps_decoder_t *ps, const char *name, const char *path);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_set_allphone_file(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string name,
			[MarshalAs(UnmanagedType.LPStr)] string path);
		
		#elif USE_IOS_INTERNAL
		
		//int ps_set_search(ps_decoder_t *ps, const char *name);
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_set_search(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string name);
		
		//const char* ps_get_search(ps_decoder_t *ps);
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_get_search(
			IntPtr ps);
		
		//int ps_unset_search(ps_decoder_t *ps, const char *name);
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_unset_search(
			IntPtr ps,
			[MarshalAs(UnmanagedType.LPStr)] string name);
		
		//ps_search_iter_t* ps_search_iter(ps_decoder_t* ps);
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_search_iter(
			IntPtr ps);
		
		//ps_search_iter_t *ps_search_iter_next(ps_search_iter_t *itor);
		[DllImport("__Internal",
		           SetLastError = true,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_search_iter_next(
                IntPtr itor);

            //const char* ps_search_iter_val(ps_search_iter_t *itor);
            [DllImport("__Internal",
            CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_search_iter_val(
                IntPtr itor);

            //void ps_search_iter_free(ps_search_iter_t* itor);
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static void ps_search_iter_free(
                IntPtr itor);

        //ngram_model_t *ps_get_lm(ps_decoder_t *ps, const char *name);
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_get_lm(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name);
            
            //int ps_set_lm(ps_decoder_t *ps, const char *name, ngram_model_t *lm);
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_set_lm(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                IntPtr lm);
            
            //int ps_set_lm_file(ps_decoder_t *ps, const char *name, const char *path);
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_set_lm_file(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                [MarshalAs(UnmanagedType.LPStr)] string path);
            
            //fsg_model_t *ps_get_fsg(ps_decoder_t *ps, const char *name);
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_get_fsg(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name);
            
            //int ps_set_fsg(ps_decoder_t *ps, const char *name, fsg_model_t *fsg);
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_set_fsg(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                IntPtr fsg);
            
            //int ps_set_jsgf_file(ps_decoder_t *ps, const char *name, const char *path);
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_set_jsgf_file(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                [MarshalAs(UnmanagedType.LPStr)] string path);
            
            //const char* ps_get_kws(ps_decoder_t *ps, const char *name);
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_get_kws(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name);
            
            //int ps_set_kws(ps_decoder_t *ps, const char *name, const char *keyfile);
            [DllImport("__Internal",
                SetLastError = true,
                CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_set_kws(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                [MarshalAs(UnmanagedType.LPStr)] string keyfile);
        
            //int ps_set_keyphrase(ps_decoder_t *ps, const char *name, const char *keyphrase);
            [DllImport("__Internal",
              CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_set_keyphrase(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                [MarshalAs(UnmanagedType.LPStr)] string keyphrase);

            //int ps_set_allphone(ps_decoder_t *ps, const char *name, ngram_model_t *lm);
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_set_allphone(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                IntPtr lm);
            
            //int ps_set_allphone_file(ps_decoder_t *ps, const char *name, const char *path);
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_set_allphone_file(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                [MarshalAs(UnmanagedType.LPStr)] string path);
		#else

        //int ps_set_search(ps_decoder_t *ps, const char *name);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_set_search(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name);

            //const char* ps_get_search(ps_decoder_t *ps);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_get_search(
                IntPtr ps);

            //int ps_unset_search(ps_decoder_t *ps, const char *name);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_unset_search(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name);

            //ps_search_iter_t* ps_search_iter(ps_decoder_t* ps);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_search_iter(
                IntPtr ps);

            //ps_search_iter_t *ps_search_iter_next(ps_search_iter_t *itor);
            [DllImport("pocketsphinx.dll",
                SetLastError = true,
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_search_iter_next(
                IntPtr itor);

            //const char* ps_search_iter_val(ps_search_iter_t *itor);
            [DllImport("pocketsphinx.dll",
            CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_search_iter_val(
                IntPtr itor);

            //void ps_search_iter_free(ps_search_iter_t* itor);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static void ps_search_iter_free(
                IntPtr itor);

            //ngram_model_t *ps_get_lm(ps_decoder_t *ps, const char *name);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_get_lm(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name);

            //int ps_set_lm(ps_decoder_t *ps, const char *name, ngram_model_t *lm);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_set_lm(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                IntPtr lm);

            //int ps_set_lm_file(ps_decoder_t *ps, const char *name, const char *path);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_set_lm_file(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                [MarshalAs(UnmanagedType.LPStr)] string path);

            //fsg_model_t *ps_get_fsg(ps_decoder_t *ps, const char *name);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_get_fsg(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name);

            //int ps_set_fsg(ps_decoder_t *ps, const char *name, fsg_model_t *fsg);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_set_fsg(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                IntPtr fsg);

            //int ps_set_jsgf_file(ps_decoder_t *ps, const char *name, const char *path);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_set_jsgf_file(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                [MarshalAs(UnmanagedType.LPStr)] string path);

            //const char* ps_get_kws(ps_decoder_t *ps, const char *name);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_get_kws(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name);

            //int ps_set_kws(ps_decoder_t *ps, const char *name, const char *keyfile);
            [DllImport("pocketsphinx.dll",
                SetLastError = true,
                CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_set_kws(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                [MarshalAs(UnmanagedType.LPStr)] string keyfile);

            //int ps_set_keyphrase(ps_decoder_t *ps, const char *name, const char *keyphrase);
            [DllImport("pocketsphinx.dll",
              CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_set_keyphrase(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                [MarshalAs(UnmanagedType.LPStr)] string keyphrase);

            //int ps_set_allphone(ps_decoder_t *ps, const char *name, ngram_model_t *lm);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_set_allphone(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                IntPtr lm);

            //int ps_set_allphone_file(ps_decoder_t *ps, const char *name, const char *path);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_set_allphone_file(
                IntPtr ps,
                [MarshalAs(UnmanagedType.LPStr)] string name,
                [MarshalAs(UnmanagedType.LPStr)] string path);
            
        #endif
		#endregion
    }
}
