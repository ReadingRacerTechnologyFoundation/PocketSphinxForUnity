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
    /**
     * A c FILE type
     **/
    public struct FILE
	{
		private IntPtr ptr;

		public FILE(IntPtr ptr)
		{
			this.ptr = ptr;
		}

		public static explicit operator IntPtr(FILE f)
		{
			return f.ptr;
		}

		public static FILE CreateNull()
		{
			return new FILE (IntPtr.Zero);
		}
		
		public bool IsNull ()
		{
			return ptr == IntPtr.Zero;
		}
    };

    /**
     * Use ps_arguments to generate.
     * dont get or set unless you know what you are doing
     **/
    public struct arg_t
    {
		private IntPtr ptr;
		
		public arg_t(IntPtr ptr)
		{
			this.ptr = ptr;
		}
		
		public static explicit operator IntPtr(arg_t f)
		{
			return f.ptr;
		}
		
		public static arg_t CreateNull()
		{
			return new arg_t (IntPtr.Zero);
		}
		
		public bool IsNull ()
		{
			return ptr == IntPtr.Zero;
		}
    };

    /**
    * Stores the command line arguments. This is what is edited/created
    * remeber to free using cmd_ln_free_r
    * dont get ore set unless you know what you are doing.
    **/
    public struct cmd_ln_t
    {
		private IntPtr ptr;
		
		public cmd_ln_t(IntPtr ptr)
		{
			this.ptr = ptr;
		}
		
		public static explicit operator IntPtr(cmd_ln_t f)
		{
			return f.ptr;
		}
		
		public static cmd_ln_t CreateNull()
		{
			return new cmd_ln_t (IntPtr.Zero);
		}
		
		public bool IsNull ()
		{
			return ptr == IntPtr.Zero;
		}
    };

    public struct ps_decoder_t
    {
		private IntPtr ptr;
		
		public ps_decoder_t(IntPtr ptr)
		{
			this.ptr = ptr;
		}
		
		public static explicit operator IntPtr(ps_decoder_t f)
		{
			return f.ptr;
		}
		
		public static ps_decoder_t CreateNull()
		{
			return new ps_decoder_t (IntPtr.Zero);
		}
		
		public bool IsNull ()
		{
			return ptr == IntPtr.Zero;
		}
    };

    public struct ngram_model_t
    {
		private IntPtr ptr;
		
		public ngram_model_t(IntPtr ptr)
		{
			this.ptr = ptr;
		}
		
		public static explicit operator IntPtr(ngram_model_t f)
		{
			return f.ptr;
		}
		
		public static ngram_model_t CreateNull()
		{
			return new ngram_model_t (IntPtr.Zero);
		}
		
		public bool IsNull ()
		{
			return ptr == IntPtr.Zero;
		}
    };

    public struct fsg_model_t
    {
		private IntPtr ptr;
		
		public fsg_model_t(IntPtr ptr)
		{
			this.ptr = ptr;
		}
		
		public static explicit operator IntPtr(fsg_model_t f)
		{
			return f.ptr;
		}
		
		public static fsg_model_t CreateNull()
		{
			return new fsg_model_t (IntPtr.Zero);
		}
		
		public bool IsNull ()
		{
			return ptr == IntPtr.Zero;
		}
    };

    public struct fe_t
    {
		private IntPtr ptr;
		
		public fe_t(IntPtr ptr)
		{
			this.ptr = ptr;
		}
		
		public static explicit operator IntPtr(fe_t f)
		{
			return f.ptr;
		}
		
		public static fe_t CreateNull()
		{
			return new fe_t (IntPtr.Zero);
		}
		
		public bool IsNull ()
		{
			return ptr == IntPtr.Zero;
		}
    };

    public struct feat_t
    {
		private IntPtr ptr;
		
		public feat_t(IntPtr ptr)
		{
			this.ptr = ptr;
		}
		
		public static explicit operator IntPtr(feat_t f)
		{
			return f.ptr;
		}
		
		public static feat_t CreateNull()
		{
			return new feat_t (IntPtr.Zero);
		}
		
		public bool IsNull ()
		{
			return ptr == IntPtr.Zero;
		}
    };

    public struct ps_mllr_t
    {
		private IntPtr ptr;
		
		public ps_mllr_t(IntPtr ptr)
		{
			this.ptr = ptr;
		}
		
		public static explicit operator IntPtr(ps_mllr_t f)
		{
			return f.ptr;
		}
		
		public static ps_mllr_t CreateNull()
		{
			return new ps_mllr_t (IntPtr.Zero);
		}
		
		public bool IsNull ()
		{
			return ptr == IntPtr.Zero;
		}
    };

    public struct ps_seg_t
    {
        private IntPtr ptr;
		
		public ps_seg_t(IntPtr ptr)
		{
			this.ptr = ptr;
		}
		
		public static explicit operator IntPtr(ps_seg_t f)
		{
			return f.ptr;
		}

        public static ps_seg_t CreateNull()
		{
            return new ps_seg_t(IntPtr.Zero);
		}
		
		public bool IsNull ()
		{
			return ptr == IntPtr.Zero;
		}
    };

    public struct ps_nbest_t
    {
        private IntPtr ptr;

        public ps_nbest_t(IntPtr ptr)
		{
			this.ptr = ptr;
		}

        public static explicit operator IntPtr(ps_nbest_t f)
		{
			return f.ptr;
		}

        public static ps_nbest_t CreateNull()
		{
            return new ps_nbest_t (IntPtr.Zero);
		}
		
		public bool IsNull ()
		{
			return ptr == IntPtr.Zero;
		}
    }

    public struct ps_search_iter_t
    {
        private IntPtr ptr;

        public ps_search_iter_t(IntPtr ptr)
		{
			this.ptr = ptr;
		}

        public static explicit operator IntPtr(ps_search_iter_t f)
		{
			return f.ptr;
		}

        public static ps_search_iter_t CreateNull()
		{
            return new ps_search_iter_t (IntPtr.Zero);
		}
		
		public bool IsNull ()
		{
			return ptr == IntPtr.Zero;
		}
    }

	public struct logmath_t
	{
		private IntPtr ptr;
		
		public logmath_t(IntPtr ptr)
		{
			this.ptr = ptr;
		}
		
		public static explicit operator IntPtr(logmath_t f)
		{
			return f.ptr;
		}
		
		public static logmath_t CreateNull()
		{
			return new logmath_t (IntPtr.Zero);
		}
		
		public bool IsNull ()
		{
			return ptr == IntPtr.Zero;
		}
	}

	public class fsg_builder_t
	{
		private IntPtr ptr;

		private bool isCreatedLocally;
		private FsgBuilder fsgBuilder;
		
		public fsg_builder_t(IntPtr ptr)
		{
			this.ptr = ptr;
			this.isCreatedLocally = false;
		}

		private fsg_builder_t(IntPtr ptr, FsgBuilder builder, bool createdLocally)
		{
			this.ptr = ptr;
			this.fsgBuilder = builder;
			this.isCreatedLocally = createdLocally;
		}
		
		public static explicit operator IntPtr(fsg_builder_t f)
		{
			return f.ptr;
		}
		
		public static fsg_builder_t CreateNull()
		{
			return new fsg_builder_t (IntPtr.Zero);
		}
		
		public bool IsNull ()
		{
			return ptr == IntPtr.Zero;
		}

		//deep copies values
		public static fsg_builder_t CreateBuilder(int lines, int[] wordsPerLine, String words)
		{
			FsgBuilder builder;
			builder.Lines = lines;

			builder.wordsPerLine = Marshal.AllocHGlobal(sizeof(int)*wordsPerLine.Length);
			Marshal.Copy(wordsPerLine,0,builder.wordsPerLine,wordsPerLine.Length);

			if(words != null)
				builder.words = Marshal.StringToHGlobalAnsi(words);
			else
				builder.words = IntPtr.Zero;

			IntPtr tempPtr = Marshal.AllocHGlobal(Marshal.SizeOf(builder));
			Marshal.StructureToPtr(builder,tempPtr,false);

			return new fsg_builder_t(tempPtr,builder ,true);
		}

		~fsg_builder_t()
		{
			if(!this.isCreatedLocally)
				return;

			#pragma warning disable 0472
			if(fsgBuilder.wordsPerLine != null && fsgBuilder.wordsPerLine != IntPtr.Zero)
				Marshal.FreeHGlobal(fsgBuilder.wordsPerLine);

			if(fsgBuilder.words != null && fsgBuilder.words != IntPtr.Zero)
				Marshal.FreeHGlobal(fsgBuilder.words);
			#pragma warning restore 0472

			Marshal.FreeHGlobal(ptr);
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct FsgBuilder
		{
			public int Lines;
			public IntPtr wordsPerLine;
			public IntPtr words;
		}

	}
}
