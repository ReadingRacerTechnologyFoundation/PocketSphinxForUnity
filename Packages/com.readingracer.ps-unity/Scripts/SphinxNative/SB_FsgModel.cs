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
using System.Runtime.InteropServices;
using System;
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
	public class SB_FsgModel
	{
		/**
		 * Read a word FSG from the given file and return a pointer to the structure
		 * created.  Return NULL if any error occurred.
		 * 
		 * File format:
		 * 
		 * <pre>
		 *   Any number of comment lines; ignored
		 *   FSG_BEGIN [<fsgname>]
		 *   N <#states>
		 *   S <start-state ID>
		 *   F <final-state ID>
		 *   T <from-state> <to-state> <prob> [<word-string>]
		 *   T ...
		 *   ... (any number of state transitions)
		 *   FSG_END
		 *   Any number of comment lines; ignored
		 * </pre>
		 * 
		 * The FSG spec begins with the line containing the keyword FSG_BEGIN.
		 * It has an optional fsg name string.  If not present, the FSG has the empty
		 * string as its name.
		 * 
		 * Following the FSG_BEGIN declaration is the number of states, the start
		 * state, and the final state, each on a separate line.  States are numbered
		 * in the range [0 .. <numberofstate>-1].
		 * 
		 * These are followed by all the state transitions, each on a separate line,
		 * and terminated by the FSG_END line.  A state transition has the given
		 * probability of being taken, and emits the given word.  The word emission
		 * is optional; if word-string omitted, it is an epsilon or null transition.
		 * 
		 * Comments can also be embedded within the FSG body proper (i.e. between
		 * FSG_BEGIN and FSG_END): any line with a # character in col 1 is treated
		 * as a comment line.
		 * 
		 * @lw language model weight
		 * 
		 * Return value: a new fsg_model_t structure if the file is successfully
		 * read, NULL otherwise.
		 */
		//fsg_model_t *fsg_model_readfile(const char *file, logmath_t *lmath, float32 lw);
		public static fsg_model_t fsg_model_readfile(string filePath, logmath_t lmath, float lw)
		{
			if (!System.IO.File.Exists (filePath))
			{
				Debug.LogError("fsg_model_readfile Failed. Could not find " + filePath);
				return fsg_model_t.CreateNull();
			}

			return new fsg_model_t (fsg_model_readfile (filePath, (IntPtr)lmath, lw));
		}

		/// <summary>
		/// Worsk the same as read file cept you can create a builder in C#. Builder is just the string the
		/// file would normally hold
		/// </summary>
		/// <returns>The build struct.</returns>
		/// <param name="build">Build.</param>
		/// <param name="lmath">Lmath.</param>
		/// <param name="lw">Lw.</param>
		//fsg_model_t* fsg_model_fromBuildStruct_easy(fsg_builder_t *build, logmath_t * lmath, float32 lw);
		public static fsg_model_t fsg_model_fromBuildStruct(fsg_builder_t build, logmath_t lmath, float lw)
		{
			return new fsg_model_t(fsg_model_fromBuildStruct_easy( (IntPtr) build, (IntPtr) lmath, lw));
		}

		#region External Calls
		#if USE_SPHINXALL
		
		//fsg_model_t *fsg_model_readfile(const char *file, logmath_t *lmath, float32 lw);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr fsg_model_readfile(
			[MarshalAs(UnmanagedType.LPStr)] string filePath,
			IntPtr lmath,
			float lw);
		
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr fsg_model_fromBuildStruct_easy(
			IntPtr build, 
			IntPtr lmath, 
			float lw);

		#elif USE_IOS_INTERNAL
		
		//fsg_model_t *fsg_model_readfile(const char *file, logmath_t *lmath, float32 lw);
		[DllImport("__Internal",
   			CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr fsg_model_readfile(
			[MarshalAs(UnmanagedType.LPStr)] string filePath,
			IntPtr lmath,
			float lw);

		//fsg_model_t* fsg_model_fromBuildStruct_easy(fsg_builder_t *build, logmath_t * lmath, float32 lw);
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr fsg_model_fromBuildStruct_easy(
			IntPtr build, 
			IntPtr lmath, 
			float lw);
		
		#else
		//UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

		//fsg_model_t *fsg_model_readfile(const char *file, logmath_t *lmath, float32 lw);
		[DllImport("sphinxbase.dll",
   			CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr fsg_model_readfile(
			[MarshalAs(UnmanagedType.LPStr)] string filePath,
			IntPtr lmath,
			float lw);

		//fsg_model_t* fsg_model_fromBuildStruct_easy(fsg_builder_t *build, logmath_t * lmath, float32 lw);
		[DllImport("sphinxbase.dll",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr fsg_model_fromBuildStruct_easy(
			IntPtr build, 
			IntPtr lmath, 
			float lw);
		
		#endif
		#endregion
	}
}