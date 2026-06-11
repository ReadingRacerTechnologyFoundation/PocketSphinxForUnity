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
 * are wrapped as well in `ly defined methods that take care of how to use
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
	class PS_Segmentation
	{

        /**
         * Get an iterator over the word segmentation for the best hypothesis.
         *
         * @param ps Decoder.
         * @return Iterator over the best hypothesis at this point in
         *         decoding.  NULL if no hypothesis is available.
         */
        //ps_seg_t* ps_seg_iter(ps_decoder_t* ps);
        public static ps_seg_t ps_seg_iter(ps_decoder_t ps)
        {
            return new ps_seg_t(ps_seg_iter((IntPtr)ps));
        }

        /**
         * Get the next segment in a word segmentation.
         *
         * @param seg Segment iterator.
         * @return Updated iterator with the next segment.  NULL at end of
         *         utterance (the iterator will be freed in this case).
         */
        //ps_seg_t *ps_seg_next(ps_seg_t *seg);
        public static ps_seg_t ps_seg_next(ps_seg_t seg)
        {
            return new ps_seg_t((IntPtr)seg);
        }

        /**
         * Get word string from a segmentation iterator.
         *
         * @param seg Segment iterator.
         * @return Read-only string giving string name of this segment.  This
         * is only valid until the next call to ps_seg_next().
         */
        //char const *ps_seg_word(ps_seg_t *seg);
        public static string ps_seg_word(ps_seg_t seg)
        {
            return Marshal.PtrToStringAnsi(ps_seg_word((IntPtr)seg));
        }

        /**
         * Get inclusive start and end frames from a segmentation iterator.
         *
         * @note These frame numbers are inclusive, i.e. the end frame refers
         * to the last frame in which the given word or other segment was
         * active.  Therefore, the actual duration is *out_ef - *out_sf + 1.
         *
         * @param seg Segment iterator.
         * @param out_sf Output: First frame index in segment.
         * @param out_sf Output: Last frame index in segment.
         */
        //void ps_seg_frames(ps_seg_t *seg, int *out_sf, int *out_ef);
        public static void ps_seg_frames(ps_seg_t seg, ref int out_sf, ref int out_ef)
        {
            ps_seg_frames((IntPtr)seg, ref out_sf, ref out_ef);
        }

        /**
         * Get language, acoustic, and posterior probabilities from a
         * segmentation iterator.
         *
         * @note Unless the -bestpath option is enabled, this function will
         * always return zero (corresponding to a posterior probability of
         * 1.0).  Even if -bestpath is enabled, it will also return zero when
         * called on a partial result.  Ongoing research into effective
         * confidence annotation for partial hypotheses may result in these
         * restrictions being lifted in future versions.
         *
         * @param out_ascr Output: acoustic model score for this segment.
         * @param out_lscr Output: language model score for this segment.
         * @param out_lback Output: language model backoff mode for this
         *                  segment (i.e. the number of words used in
         *                  calculating lscr).  This field is, of course, only
         *                  meaningful for N-Gram models.
         * @return Log posterior probability of current segment.  Log is
         *         expressed in the log-base used in the decoder.  To convert
         *         to linear floating-point, use logmath_exp(ps_get_logmath(),
         *         pprob).
         */
        //int32 ps_seg_prob(ps_seg_t *seg, int32 *out_ascr, int32 *out_lscr, int32 *out_lback);
        public static int ps_seg_prob(ps_seg_t seg,
            ref int out_ascr,
            ref int out_lscr,
            ref int out_lback)
        {
            return ps_seg_prob((IntPtr)seg, ref out_ascr, ref out_lscr, ref out_lback);
        }

        /**
         * Finish iterating over a word segmentation early, freeing resources.
         */
        //void ps_seg_free(ps_seg_t *seg);
        public static void ps_seg_free(ps_seg_t seg)
        {
            ps_seg_free((IntPtr)seg);
        }

		#region External Calls
		#if USE_SPHINXALL
		
		//ps_seg_t* ps_seg_iter(ps_decoder_t* ps);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_seg_iter(
			IntPtr ps);  
		
		//ps_seg_t* ps_seg_next(ps_seg_t* seg);  
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_seg_next(
			IntPtr seg);
		
		//char const *ps_seg_word(ps_seg_t *seg);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_seg_word(
			IntPtr seg);
		
		//void ps_seg_frames(ps_seg_t *seg, int *out_sf, int *out_ef);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static void ps_seg_frames(
			IntPtr seg,
			ref int out_sf,
			ref int out_ef);
		
		//int32 ps_seg_prob(ps_seg_t *seg, int32 *out_ascr, int32 *out_lscr, int32 *out_lback);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int ps_seg_prob(
			IntPtr seg,
			ref int out_ascr,
			ref int out_lscr,
			ref int out_lback);
		
		//void ps_seg_free(ps_seg_t *seg);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static void ps_seg_free(
			IntPtr seg);
		
		#elif USE_IOS_INTERNAL
		
		//ps_seg_t* ps_seg_iter(ps_decoder_t* ps);
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr ps_seg_iter(
			IntPtr ps);
		
		//ps_seg_t* ps_seg_next(ps_seg_t* seg);  
		[DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_seg_next(
                IntPtr seg);

            //char const *ps_seg_word(ps_seg_t *seg);
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_seg_word(
                IntPtr seg);

            //void ps_seg_frames(ps_seg_t *seg, int *out_sf, int *out_ef);
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static void ps_seg_frames(
                IntPtr seg,
                ref int out_sf,
                ref int out_ef);

            //int32 ps_seg_prob(ps_seg_t *seg, int32 *out_ascr, int32 *out_lscr, int32 *out_lback);
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_seg_prob(
                IntPtr seg,
                ref int out_ascr,
                ref int out_lscr,
                ref int out_lback);

            //void ps_seg_free(ps_seg_t *seg);
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static void ps_seg_free(
                IntPtr seg);


            
#else
        //ps_seg_t* ps_seg_iter(ps_decoder_t* ps);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_seg_iter(
                IntPtr ps);

            //ps_seg_t* ps_seg_next(ps_seg_t* seg);  
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_seg_next(
                IntPtr seg);
            

            //char const *ps_seg_word(ps_seg_t *seg);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr ps_seg_word(
                IntPtr seg);

            //void ps_seg_frames(ps_seg_t *seg, int *out_sf, int *out_ef);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static void ps_seg_frames(
                IntPtr seg,
                ref int out_sf,
                ref int out_ef);

            //int32 ps_seg_prob(ps_seg_t *seg, int32 *out_ascr, int32 *out_lscr, int32 *out_lback);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static int ps_seg_prob(
                IntPtr seg,
                ref int out_ascr,
                ref int out_lscr,
                ref int out_lback);

            //void ps_seg_free(ps_seg_t *seg);
            [DllImport("pocketsphinx.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static void ps_seg_free(
                IntPtr seg);
            
        #endif
		#endregion
	}
}
