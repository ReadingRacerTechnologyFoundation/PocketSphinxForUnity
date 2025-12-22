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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;


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
namespace Sphinx.SphinxNative
{
	class PS_nbest
	{
        /**
         * Get an iterator over the best hypotheses, optionally within a
         * selected region of the utterance. Iterator is empty now, it must
         * be advanced with ps_nbest_next first. The function may also
         * return a NULL which means that there is no hypothesis available for this
         * utterance.
         *
         * @param ps Decoder.
         */
        //ps_nbest_t *ps_nbest(ps_decoder_t *ps);
        public static ps_nbest_t ps_nbest(
            ps_decoder_t ps)
        {
            return new ps_nbest_t(ps_nbest((IntPtr)ps));
        }

        /**
         * Move an N-best list iterator forward.
         *
         * @param nbest N-best iterator.
         * @return Updated N-best iterator, or NULL if no more hypotheses are
         *         available (iterator is freed ni this case).
         */
        //ps_nbest_t *ps_nbest_next(ps_nbest_t *nbest);
        public static ps_nbest_t ps_nbest_next(ps_nbest_t nbest)
        {
            return new ps_nbest_t(ps_nbest_next((IntPtr)nbest));
        }

        /**
         * Get the hypothesis string from an N-best list iterator.
         *
         * @param nbest N-best iterator.
         * @param out_score Output: Path score for this hypothesis.
         * @return String containing next best hypothesis.
         */
        //char const *ps_nbest_hyp(ps_nbest_t *nbest, int32 *out_score);
        public static String ps_nbest_hyp(ps_nbest_t nbest, ref int out_score)
        {
            IntPtr retVal = ps_nbest_hyp((IntPtr)nbest,ref out_score);

            return retVal == IntPtr.Zero ? null :
                Marshal.PtrToStringAnsi(retVal);
        }

        /**
         * Get the word segmentation from an N-best list iterator.
         *
         * @param nbest N-best iterator.
         * @param out_score Output: Path score for this hypothesis.
         * @return Iterator over the next best hypothesis.
         */
        //ps_seg_t *ps_nbest_seg(ps_nbest_t *nbest);
        public static ps_nbest_t ps_nbest_seg(ps_nbest_t nbest_T)
        {
            return new ps_nbest_t(ps_nbest_seg((IntPtr)nbest_T));
        }

        /**
         * Finish N-best search early, releasing resources.
         *
         * @param nbest N-best iterator.
         */
        //void ps_nbest_free(ps_nbest_t *nbest);
        public static void ps_nbest_free(ps_nbest_t nbest)
        {
            ps_nbest_free((IntPtr)nbest);
        }

		#region External Calls

		#if USE_SPHINXALL
		
		//ps_nbest_t *ps_nbest(ps_decoder_t *ps);
		[DllImport("sphinxall",
		           CharSet=CharSet.Ansi,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_nbest(
			IntPtr ps);
		
		//ps_nbest_t *ps_nbest_next(ps_nbest_t *nbest);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_nbest_next(
			IntPtr nbest);
		
		//char const *ps_nbest_hyp(ps_nbest_t *nbest, int32 *out_score);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_nbest_hyp(
			IntPtr nbest,
			ref int out_score);
		
		//ps_seg_t *ps_nbest_seg(ps_nbest_t *nbest);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_nbest_seg(
			IntPtr nbest);
		
		//void ps_nbest_free(ps_nbest_t *nbest);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static void ps_nbest_free(
			IntPtr nbest);

		#elif USE_IOS_INTERNAL
		
		//ps_nbest_t *ps_nbest(ps_decoder_t *ps);
		[DllImport("__Internal",
		           CharSet=CharSet.Ansi,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_nbest(
			IntPtr ps);
        
            //ps_nbest_t *ps_nbest_next(ps_nbest_t *nbest);
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_nbest_next(
                IntPtr nbest);
            
            //char const *ps_nbest_hyp(ps_nbest_t *nbest, int32 *out_score);
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_nbest_hyp(
                IntPtr nbest,
                ref int out_score);
            
            //ps_seg_t *ps_nbest_seg(ps_nbest_t *nbest);
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_nbest_seg(
                IntPtr nbest);
            
            //void ps_nbest_free(ps_nbest_t *nbest);
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static void ps_nbest_free(
                IntPtr nbest);


			#else
                    //UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

            //ps_nbest_t *ps_nbest(ps_decoder_t *ps);
            [DllImport("pocketsphinx.dll",
                CharSet=CharSet.Ansi,
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_nbest(
                IntPtr ps);
        
            //ps_nbest_t *ps_nbest_next(ps_nbest_t *nbest);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_nbest_next(
                IntPtr nbest);
            
            //char const *ps_nbest_hyp(ps_nbest_t *nbest, int32 *out_score);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_nbest_hyp(
                IntPtr nbest,
                ref int out_score);
            
            //ps_seg_t *ps_nbest_seg(ps_nbest_t *nbest);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_nbest_seg(
                IntPtr nbest);
            
            //void ps_nbest_free(ps_nbest_t *nbest);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static void ps_nbest_free(
                IntPtr nbest);

	    #endif
		#endregion
	}
}
