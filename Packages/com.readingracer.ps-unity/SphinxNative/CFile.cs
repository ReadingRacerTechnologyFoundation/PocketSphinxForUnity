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

    class CFile
    {
		//wrapper for fopen. necssary to ensure the same DLLS Sphinx was compiled with
		//are used when opening a file
		//FILE fopen_rbMode_easy(const char* filePath);
		//returns null FILE when it fails
		public static FILE fopen_rbMode(
			string filePath)
		{
			if(!System.IO.File.Exists(filePath))
			{
				Debug.LogError("fopen_rbMode failed. Could not find " + filePath);
				return FILE.CreateNull();
			}

			return new FILE (fopen_rbMode_easy (filePath));
		}
		
		//wrapper for close.
		//int fclose_easy ( FILE * stream );
		public static int fclose_easy(
			FILE stream)
		{
			return fclose_easy ((IntPtr)stream);
		}
		
		//wrapper for fread.
		//size_t fread_easy ( void * ptr, size_t size, size_t count, FILE * stream );
		//ptr is typically a short[]
		public static int fread_easy(
			IntPtr ptr,
			int size,
			int count,
			FILE stream)
		{
			return fread_easy (ptr, size, count, (IntPtr)stream);
		}
		
		//wrapper for feof
		//int feof_easy ( FILE * stream );
		public static int feof_easy(
			FILE stream)
		{
			return feof_easy ((IntPtr)stream);
		}

		#region External Calls

		#if USE_SPHINXALL
		//FILE* fopen_rbMode_easy(const char* filePath);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr fopen_rbMode_easy(
			[MarshalAs(UnmanagedType.LPStr)] string filePath);
		
		//int fclose_easy ( FILE * stream );
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private  extern static int fclose_easy(
			IntPtr stream);
		
		//size_t fread_easy ( void * ptr, size_t size, size_t count, FILE * stream );
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int fread_easy(
			IntPtr ptr,
			int size,
			int count,
			IntPtr stream);
		
		//int feof_easy ( FILE * stream );
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private  extern static int feof_easy(
			IntPtr stream);
				
		#elif USE_IOS_INTERNAL
		
		//FILE* fopen_rbMode_easy(const char* filePath);
			[DllImport("__Internal",
			           CallingConvention = CallingConvention.Cdecl)]
			private extern static IntPtr fopen_rbMode_easy(
				[MarshalAs(UnmanagedType.LPStr)] string filePath);
			
			//int fclose_easy ( FILE * stream );
			[DllImport("__Internal",
			           CallingConvention = CallingConvention.Cdecl)]
			private  extern static int fclose_easy(
				IntPtr stream);
			
			//size_t fread_easy ( void * ptr, size_t size, size_t count, FILE * stream );
			[DllImport("__Internal",
			           CallingConvention = CallingConvention.Cdecl)]
			private extern static int fread_easy(
				IntPtr ptr,
				int size,
				int count,
				IntPtr stream);
			
			//int feof_easy ( FILE * stream );
			[DllImport("__Internal",
			           CallingConvention = CallingConvention.Cdecl)]
			private extern static int feof_easy(
				IntPtr stream);
    	#else

		    //FILE* fopen_rbMode_easy(const char* filePath);
            [DllImport("EasyCallWrapper.dll",
			            CallingConvention = CallingConvention.Cdecl)]
		    private extern static IntPtr fopen_rbMode_easy(
			    [MarshalAs(UnmanagedType.LPStr)] string filePath);
			
		    //int fclose_easy ( FILE * stream );
            [DllImport("EasyCallWrapper.dll",
			            CallingConvention = CallingConvention.Cdecl)]
		    private  extern static int fclose_easy(
			    IntPtr stream);
			
		    //size_t fread_easy ( void * ptr, size_t size, size_t count, FILE * stream );
            [DllImport("EasyCallWrapper.dll",
			            CallingConvention = CallingConvention.Cdecl)]
		    private extern static int fread_easy(
			    IntPtr ptr,
			    int size,
			    int count,
			    IntPtr stream);
			
		    //int feof_easy ( FILE * stream );
            [DllImport("EasyCallWrapper.dll",
			            CallingConvention = CallingConvention.Cdecl)]
		    private  extern static int feof_easy(
			    IntPtr stream);

	    #endif
		#endregion
        
    }
}
