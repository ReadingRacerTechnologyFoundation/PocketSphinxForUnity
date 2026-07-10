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
    /**
     * This class creates and modifies comand line argument
     * configurations for pocket sphinx.
     * 
     * Created by Rodrigo Cano 9/17/2014
     **/

    class SB_CmdLn
    {
		//int cmd_ln_free_r(cmd_ln_t *cmdln);
		//returns 0 if it was freed completely
		public static int cmd_ln_free_r(
			cmd_ln_t cmdln)
		{
			return cmd_ln_free_r ((IntPtr)cmdln);
		}

		//cmd_ln_t *cmd_ln_parse_file_r(cmd_ln_t *inout_cmdln, /**< In/Out: Previous command-line to update,
		//                                             or NULL to create a new one. */
		//                      arg_t const *defn,    /**< In: Array of argument name definitions*/
		//                      char const *filename,/**< In: A file that contains all
		//                                             the arguments */ 
		//                      int32 strict         /**< In: Fail on duplicate or unknown
		//                                             arguments, or no arguments? */
		//);
		public static cmd_ln_t cmd_ln_parse_file_r(
			cmd_ln_t inout_cmdln,
			arg_t defn,
			string filename,
			bool strict)
		{

			return new cmd_ln_t(cmd_ln_parse_file_r (
				(IntPtr)inout_cmdln,
				(IntPtr)defn,
				filename,
				strict));
		}

		//cmd_ln_t *cmd_ln_parse_r(cmd_ln_t *inout_cmdln, /**< In/Out: Previous command-line to update,
		//                                             or NULL to create a new one. */
		//                 arg_t const *defn,	/**< In: Array of argument name definitions */
		//                 int32 argc,		/**< In: Number of actual arguments -hmm hmmDir would count as 2*/
		//                 char *argv[],		/**< In: Actual arguments */
		//                 int32 strict           /**< In: Fail on duplicate or unknown
		//                                           arguments, or no arguments? */
		//);
		public static cmd_ln_t cmd_ln_parse_r(
			cmd_ln_t inout_cmdln,
			arg_t defn,
			int argc,
			String[] argv,
			bool strict)
		{

			return new cmd_ln_t (cmd_ln_parse_r ((IntPtr)inout_cmdln,
			                                    (IntPtr)defn,
			                                    argc,
			                                    argv,
			                                    strict));
		}

        /**
         * Retrieve a string from a command-line object.
         *
         * The command-line object retains ownership of this string, so you
         * should not attempt to free it manually.
         *
         * @param cmdln Command-line object.
         * @param name the command-line flag to retrieve.
         * @return the string value associated with <tt>name</tt>, or NULL if
         *         <tt>name</tt> does not exist.  You must use
         *         cmd_ln_exists_r() to distinguish between cases where a
         *         value is legitimately NULL and where the corresponding flag
         *         is unknown.
         */
        //char const *cmd_ln_str_r(cmd_ln_t *cmdln, char const *name);
        public static String cmd_ln_str_r(cmd_ln_t cmdln, string name)
        {
            IntPtr retVal = cmd_ln_str_r((IntPtr)cmdln, name);

            return retVal == IntPtr.Zero ? null : 
                Marshal.PtrToStringAnsi(retVal);
        }

        //long cmd_ln_int_r(cmd_ln_t *cmdln, char const *name);
        public static long cmd_ln_int_r(cmd_ln_t cmdln, string name)
        {
            return cmd_ln_int_r((IntPtr)cmdln, name);
        }

        //double cmd_ln_float_r(cmd_ln_t *cmdln, char const *name);
        public static double cmd_ln_float_r(cmd_ln_t cmdln, string name)
        {
            return cmd_ln_float_r((IntPtr)cmdln, name);
        }

        public static bool cmd_ln_bool_r(cmd_ln_t cmdln, string name)
        {
            return cmd_ln_int_r(cmdln, name) != 0;
        }

		/**
         * Set a floating-point number in a command-line object.
         *
         * @param cmdln Command-line object.
         * @param name The command-line flag to set.
         * @param fv double value to set. (its a double but the method is float)
         */
		public static void cmd_ln_set_float_r(
			cmd_ln_t cmdln,
			string name,
			double fv)
		{
			cmd_ln_set_float_r ((IntPtr)cmdln, name, fv);
		}

		/**
         * Set a floating-point number in a command-line object.
         *
         * @param cmdln Command-line object.
         * @param name The command-line flag to set.
         * @param fv Integer value to set.
         */
		public static void cmd_ln_set_int_r(
			cmd_ln_t cmdln,
			string name,
			int iv)
		{
			cmd_ln_set_int_r ((IntPtr)cmdln, name, iv);
		}

		/**
         * Set a string in a command-line object.
         *
         * @param cmdln Command-line object.
         * @param name The command-line flag to set.
         * @param str String value to set.  The command-line object does not
         *            retain ownership of this pointer.
         */
		public static void cmd_ln_set_str_r(
			cmd_ln_t cmdln,
			string name,
			string str)
		{
			cmd_ln_set_str_r ((IntPtr)cmdln, name, str);
		}
		
		public static void cmd_ln_set_boolean(
			cmd_ln_t cmdln,
			string name,
			bool bv)
		{
			cmd_ln_set_boolean_easy ((IntPtr)cmdln, name, bv);
		}

        /**
         * Print a help message listing the valid argument names, and the associated
         * attributes as given in defn. 
         *
         * @param filepath file to output to
         * @param defn Array of argument name definitions.
         * @return 0 on success
         */
		public static int cmd_ln_print_help_easy(
			cmd_ln_t cmdln,
			string filePath,
			arg_t defn)
		{
			if(!System.IO.File.Exists(filePath))
			{
				Debug.LogError("filePath: " + filePath + " could not be found in cmd_ln_print_help_easy");
				return -1;
			}

			return cmd_ln_print_help_easy ((IntPtr)cmdln, filePath, (IntPtr)defn);
		}

        /**
         * Print a help message listing the valid argument names, and the associated
         * attributes as given in defn.
         *
         * @param fp   output stream
         * @param defn Array of argument name definitions.
         */
        public static void cmd_ln_print_help_r(
            cmd_ln_t cmdln,
            FILE fp,
            arg_t defn)
        {
			cmd_ln_print_help_r((IntPtr)cmdln, (IntPtr)fp, (IntPtr)defn);
        }

        /**
        * Does the same as the unwrapped 
        * ps_default_search_args(cmd_ln_t *);
        * but takes a modelDir instead of expecting a defined macro for the modelDir

        * @ModelDir the folder containing the hmm, lmm and dict, as well as grammer etc
        * 
        */
        public static void default_search_args_easy(
            string modelDir,
            cmd_ln_t config)
        {
			default_search_args_easy(modelDir, (IntPtr)config);
        }

        public static cmd_ln_t cmd_ln_init_easy_w(
            string modelDir_hmm,
            string modelDir_lm,
            string modelDir_dict)
        {
			return new cmd_ln_t(cmd_ln_init_easy(
				modelDir_hmm, modelDir_lm, modelDir_dict));
        }

		/**
		 * initializes a basic cmd_ln_t type without any arguments. use this to create new cmd_ln_t's
		 */
		//cmd_ln_t * cmd_ln_init(cmd_ln_t *inout_cmdln, const arg_t *defn, int32 strict, ...)]
		public static cmd_ln_t cmd_ln_init()
		{
			return new cmd_ln_t(cmd_ln_init_no_args_easy());
		}

		#region External Calls
		#if USE_SPHINXALL
		
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int cmd_ln_free_r(
			IntPtr cmdln);
		
		
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr cmd_ln_parse_file_r(
			IntPtr inout_cmdln,
			IntPtr defn,
			[MarshalAs(UnmanagedType.LPStr)] string filename,
			bool strict);
		
		
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr cmd_ln_parse_r(
			IntPtr inout_cmdln,
			IntPtr defn,
			int argc,
			String[] argv,
			bool strict);
		
		//char const *cmd_ln_str_r(cmd_ln_t *cmdln, char const *name);
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr cmd_ln_str_r(
			IntPtr cmdln,
			[MarshalAs(UnmanagedType.LPStr)] string name);
		
		//long cmd_ln_int_r(cmd_ln_t *cmdln, char const *name);
		[DllImport("sphinxall",
		           SetLastError = true,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static long cmd_ln_int_r(
			IntPtr cmdln,
			[MarshalAs(UnmanagedType.LPStr)] string name);
		
		//double cmd_ln_float_r(cmd_ln_t *cmdln, char const *name);
		[DllImport("sphinxall",
		           SetLastError = true,
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static double cmd_ln_float_r(
			IntPtr cmdln,
			[MarshalAs(UnmanagedType.LPStr)] string name);
		
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static void cmd_ln_set_float_r(
			IntPtr cmdln,
			[MarshalAs(UnmanagedType.LPStr)] string name,
			double fv);
		
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static void cmd_ln_set_int_r(
			IntPtr cmdln,
			[MarshalAs(UnmanagedType.LPStr)] string name,
			int iv);
		
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static void cmd_ln_set_str_r(
			IntPtr cmdln,
			[MarshalAs(UnmanagedType.LPStr)] string name,
			[MarshalAs(UnmanagedType.LPStr)] string str);
		
		
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static void cmd_ln_set_boolean_easy(
			IntPtr cmdln,
			[MarshalAs(UnmanagedType.LPStr)] string name,
			bool bv);
		
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int cmd_ln_print_help_easy(
			IntPtr cmdln,
			[MarshalAs(UnmanagedType.LPStr)] string filePath,
			IntPtr defn);
		
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static void cmd_ln_print_help_r(
			IntPtr cmdln,
			IntPtr fp,
			IntPtr defn);
		
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static void default_search_args_easy(
			[MarshalAs(UnmanagedType.LPStr)] string modelDir,
			IntPtr config);
		
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr cmd_ln_init_easy(
			[MarshalAs(UnmanagedType.LPStr)] string modelDir_hmm,
			[MarshalAs(UnmanagedType.LPStr)] string modelDir_lm,
			[MarshalAs(UnmanagedType.LPStr)] string modelDir_dict);
		
		//cmd_ln_t * cmd_ln_init(cmd_ln_t *inout_cmdln, const arg_t *defn, int32 strict, ...)
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr cmd_ln_init(
			IntPtr inout_cmdln,
			IntPtr defn,
			int strict,
			IntPtr makeThisNull);

		//calls cmd_ln_init but with the following args (NULL, ps_args(), 0,NULL)
		[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr cmd_ln_init_no_args_easy();
		
		#elif USE_IOS_INTERNAL
		
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static int cmd_ln_free_r(
			IntPtr cmdln);
		
		
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr cmd_ln_parse_file_r(
			IntPtr inout_cmdln,
			IntPtr defn,
			[MarshalAs(UnmanagedType.LPStr)] string filename,
			bool strict);
		
		
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
			private extern static IntPtr cmd_ln_parse_r(
				IntPtr inout_cmdln,
				IntPtr defn,
				int argc,
				String[] argv,
				bool strict);
			
            //char const *cmd_ln_str_r(cmd_ln_t *cmdln, char const *name);
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr cmd_ln_str_r(
                IntPtr cmdln,
                [MarshalAs(UnmanagedType.LPStr)] string name);

            //long cmd_ln_int_r(cmd_ln_t *cmdln, char const *name);
            [DllImport("__Internal",
                SetLastError = true,
                CallingConvention = CallingConvention.Cdecl)]
            private extern static long cmd_ln_int_r(
                IntPtr cmdln,
                [MarshalAs(UnmanagedType.LPStr)] string name);
            
            //double cmd_ln_float_r(cmd_ln_t *cmdln, char const *name);
            [DllImport("__Internal",
                SetLastError = true,
                CallingConvention = CallingConvention.Cdecl)]
            private extern static double cmd_ln_float_r(
                IntPtr cmdln,
                [MarshalAs(UnmanagedType.LPStr)] string name);

			[DllImport("__Internal",
			           CallingConvention = CallingConvention.Cdecl)]
			private extern static void cmd_ln_set_float_r(
				IntPtr cmdln,
				[MarshalAs(UnmanagedType.LPStr)] string name,
				double fv);
			
			[DllImport("__Internal",
			           CallingConvention = CallingConvention.Cdecl)]
			private extern static void cmd_ln_set_int_r(
				IntPtr cmdln,
				[MarshalAs(UnmanagedType.LPStr)] string name,
				int iv);
			
			[DllImport("__Internal",
			           CallingConvention = CallingConvention.Cdecl)]
			private extern static void cmd_ln_set_str_r(
				IntPtr cmdln,
				[MarshalAs(UnmanagedType.LPStr)] string name,
				[MarshalAs(UnmanagedType.LPStr)] string str);
			
			
			[DllImport("__Internal",
			           CallingConvention = CallingConvention.Cdecl)]
			private extern static void cmd_ln_set_boolean_easy(
				IntPtr cmdln,
				[MarshalAs(UnmanagedType.LPStr)] string name,
				bool bv);

			[DllImport("sphinxall",
		           CallingConvention = CallingConvention.Cdecl)]
			private extern static void cmd_ln_print_help_r(
				IntPtr cmdln,
				IntPtr fp,
				IntPtr defn);

			
			[DllImport("__Internal",
			           CallingConvention = CallingConvention.Cdecl)]
			private extern static int cmd_ln_print_help_easy(
				IntPtr cmdln,
				[MarshalAs(UnmanagedType.LPStr)] string filePath,
				IntPtr defn);

            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static void default_search_args_easy(
                [MarshalAs(UnmanagedType.LPStr)] string modelDir,
                IntPtr config);
            
            [DllImport("__Internal",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr cmd_ln_init_easy(
                [MarshalAs(UnmanagedType.LPStr)] string modelDir_hmm,
                [MarshalAs(UnmanagedType.LPStr)] string modelDir_lm,
                [MarshalAs(UnmanagedType.LPStr)] string modelDir_dict);

		//cmd_ln_t * cmd_ln_init(cmd_ln_t *inout_cmdln, const arg_t *defn, int32 strict, ...)
		//this does not work on iOS64
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr cmd_ln_init(
			IntPtr inout_cmdln,
			IntPtr defn,
			int strict,
			IntPtr makeThisNull);

		//calls cmd_ln_init but with the following args (NULL, ps_args(), 0,NULL)
		[DllImport("__Internal",
		           CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr cmd_ln_init_no_args_easy();

		#else
        //UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

            [DllImport("sphinxbase.dll",
			           CallingConvention = CallingConvention.Cdecl)]
			private extern static int cmd_ln_free_r(
				IntPtr cmdln);


            [DllImport("sphinxbase.dll",
			           CallingConvention = CallingConvention.Cdecl)]
			private extern static IntPtr cmd_ln_parse_file_r(
				IntPtr inout_cmdln,
				IntPtr defn,
				[MarshalAs(UnmanagedType.LPStr)] string filename,
				bool strict);


            [DllImport("sphinxbase.dll",
			           CallingConvention = CallingConvention.Cdecl)]
			private extern static IntPtr cmd_ln_parse_r(
				IntPtr inout_cmdln,
				IntPtr defn,
				int argc,
				String[] argv,
				bool strict);

            //char const *cmd_ln_str_r(cmd_ln_t *cmdln, char const *name);
            [DllImport("sphinxbase.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr cmd_ln_str_r(
                IntPtr cmdln,
                [MarshalAs(UnmanagedType.LPStr)] string name);

            //long cmd_ln_int_r(cmd_ln_t *cmdln, char const *name);
            [DllImport("sphinxbase.dll",
                SetLastError = true,
                CallingConvention = CallingConvention.Cdecl)]
            private extern static long cmd_ln_int_r(
                IntPtr cmdln,
                [MarshalAs(UnmanagedType.LPStr)] string name);
            
            //double cmd_ln_float_r(cmd_ln_t *cmdln, char const *name);
            [DllImport("sphinxbase.dll",
                SetLastError = true,
                CallingConvention = CallingConvention.Cdecl)]
            private extern static double cmd_ln_float_r(
                IntPtr cmdln,
                [MarshalAs(UnmanagedType.LPStr)] string name);

            [DllImport("sphinxbase.dll",
			           CallingConvention = CallingConvention.Cdecl)]
			private extern static void cmd_ln_set_float_r(
				IntPtr cmdln,
				[MarshalAs(UnmanagedType.LPStr)] string name,
				double fv);

            [DllImport("sphinxbase.dll",
			           CallingConvention = CallingConvention.Cdecl)]
			private extern static void cmd_ln_set_int_r(
				IntPtr cmdln,
				[MarshalAs(UnmanagedType.LPStr)] string name,
				int iv);

            [DllImport("sphinxbase.dll",
			           CallingConvention = CallingConvention.Cdecl)]
			private extern static void cmd_ln_set_str_r(
				IntPtr cmdln,
				[MarshalAs(UnmanagedType.LPStr)] string name,
				[MarshalAs(UnmanagedType.LPStr)] string str);


            [DllImport("EasyCallWrapper.dll",
			           CallingConvention = CallingConvention.Cdecl)]
			private extern static void cmd_ln_set_boolean_easy(
				IntPtr cmdln,
				[MarshalAs(UnmanagedType.LPStr)] string name,
				bool bv);

            [DllImport("sphinxbase.dll",
			           CallingConvention = CallingConvention.Cdecl)]
			private extern static int cmd_ln_print_help_easy(
				IntPtr cmdln,
				[MarshalAs(UnmanagedType.LPStr)] string filePath,
				IntPtr defn);

            [DllImport("sphinxbase.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static void cmd_ln_print_help_r(
                IntPtr cmdln,
                IntPtr fp,
                IntPtr defn);

            [DllImport("EasyCallWrapper.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static void default_search_args_easy(
                [MarshalAs(UnmanagedType.LPStr)] string modelDir,
                IntPtr config);
            
            [DllImport("EasyCallWrapper.dll",
                CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr cmd_ln_init_easy(
                [MarshalAs(UnmanagedType.LPStr)] string modelDir_hmm,
                [MarshalAs(UnmanagedType.LPStr)] string modelDir_lm,
                [MarshalAs(UnmanagedType.LPStr)] string modelDir_dict);

			//cmd_ln_t * cmd_ln_init(cmd_ln_t *inout_cmdln, const arg_t *defn, int32 strict, ...)
			[DllImport("sphinxbase.dll",
			           CallingConvention = CallingConvention.Cdecl)]
			private extern static IntPtr cmd_ln_init(
				IntPtr inout_cmdln,
				IntPtr defn,
				int strict,
				IntPtr makeThisNull);

			[DllImport("EasyCallWrapper.dll",
		           CallingConvention = CallingConvention.Cdecl)]
			private extern static IntPtr cmd_ln_init_no_args_easy();
            
		#endif
		#endregion
        
    }
}
