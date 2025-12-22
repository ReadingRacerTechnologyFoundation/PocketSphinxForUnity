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

using System.Collections;
using System.Runtime.InteropServices;
using System;


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
	public class SB_LogMath
	{
		/**
		 * Initialize a log math computation table.
		 * @param base The base B in which computation is to be done.
		 * @param shift Log values are shifted right by this many bits.
		 * @param use_table Whether to use an add table or not
		 * @return The newly created log math table.
		 */
		//logmath_t *logmath_init(float64 base, int shift, int use_table);
		public static logmath_t logmath_initialize(double logBase, int shift, bool use_table)
		{
			return new logmath_t (logmath_init (logBase, shift, use_table ? 1 : 0));
		}

		#region External Calls
		#if USE_SPHINXALL
		
		//logmath_t *logmath_init(float64 base, int shift, int use_table);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr logmath_init(
			double logBase,
			int shift,
			int use_table);
		
		#elif USE_IOS_INTERNAL

			//logmath_t *logmath_init(float64 base, int shift, int use_table);
			[DllImport("__Internal",
           		CallingConvention = CallingConvention.Cdecl)]
			private extern static IntPtr logmath_init(
				double logBase,
				int shift,
				int use_table);
			
		#else
			//UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

			//logmath_t *logmath_init(float64 base, int shift, int use_table);
			[DllImport("sphinxbase.dll",
	   			CallingConvention = CallingConvention.Cdecl)]
			private extern static IntPtr logmath_init(
				double logBase,
				int shift,
				int use_table);
		
		#endif
		#endregion
	}
}

