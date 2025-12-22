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


/* Win32/WinCE DLL gunk */
#if (defined(_WIN32) || defined(_WIN32_WCE)) && !defined(__MINGW32__) && !defined(__CYGWIN__) && !defined(__WINSCW__) && !defined(__SYMBIAN32__)
#ifdef EASYCALLWRAPPER_EXPORTS /* Visual Studio */
#define EASYCALLWRAPPER_EXPORT __declspec(dllexport)
#else
#define EASYCALLWRAPPER_EXPORT __declspec(dllimport)
#endif
#else /* !_WIN32 */
#define EASYCALLWRAPPER_EXPORT
#endif

#ifndef sphinxall_EasyCallWrapper_h
#define sphinxall_EasyCallWrapper_h

#include <pocketsphinx.h>

//an easyiest and simlpest way to config cmd_ln_t. 
EASYCALLWRAPPER_EXPORT
cmd_ln_t* cmd_ln_init_easy(const char* modelDir_hmm,
                           const char* modelDir_lm, const char* modelDir_dict);

//calls cmd_ln_init with null arguments and ps_args()s
EASYCALLWRAPPER_EXPORT
cmd_ln_t* cmd_ln_init_no_args_easy();

/**
* Does the same as the unwrapped 
* ps_default_search_args(cmd_ln_t *);
* but takes a modelDir instead of expecting a defined macro for the modelDir

* @ModelDir the folder containing the hmm, lmm and dict, as well as grammer etc
* 
* returns 0 if successful
*/
EASYCALLWRAPPER_EXPORT
void default_search_args_easy(const char* modelDir, cmd_ln_t* config);

//sets a boolean. wrapper since default is a macro
EASYCALLWRAPPER_EXPORT
void cmd_ln_set_boolean_easy(cmd_ln_t *cmdln, char const *name, int bv);

/**
 * Print a help message listing the valid argument names, and the associated
 * attributes as given in defn.
 * return 0 if successfull.
 */
EASYCALLWRAPPER_EXPORT
int cmd_ln_print_help_easy (cmd_ln_t *cmdln,
                          const char*,	   /**< In: path of File to which to print to */
			  const arg_t *defn /**< In: Array of argument name definitions */
	);

/**
 * Decode a raw audio stream.
 *
 * No headers are recognized in this files.  The configuration
 * parameters <tt>-samprate</tt> and <tt>-input_endian</tt> are used
 * to determine the sampling rate and endianness of the stream,
 * respectively.  Audio is always assumed to be 16-bit signed PCM.
 *
 * @param ps Decoder.
 * @param rawfh directory of the raw file. automatically opened and closed
 * @param uttid Utterance ID (or NULL to generate automatically).
 * @param maxsamps Maximum number of samples to read from rawfh, or -1
 *                 to read until end-of-file.
 * @return Number of samples of audio. <0 if failed
 */
EASYCALLWRAPPER_EXPORT
int ps_decode_raw_easy(ps_decoder_t *ps, const char* rawfhDir,
                  char const *uttid, long maxsamps);

//wrapper for fopen. necssary to ensure the same DLLS pocketsphinx was compiled with
//are used when opening a file
EASYCALLWRAPPER_EXPORT
FILE* fopen_rbMode_easy(const char* filePath);

//wrapper for close.
//necssary to ensure the same DLLS pocketsphinx was compiled with
//are used when closing a file
EASYCALLWRAPPER_EXPORT
int fclose_easy ( FILE * stream );

//wrapper for fread.
//necssary to ensure the same DLLS pocketsphinx was compiled with
//are used when reading a file
EASYCALLWRAPPER_EXPORT
size_t fread_easy ( void * ptr, size_t size, size_t count, FILE * stream );

//wrapper for feof
EASYCALLWRAPPER_EXPORT
int feof_easy ( FILE * stream );

//stupid wrapper since iOS wont allow you to have 2 different wrappers for one function in c#
EASYCALLWRAPPER_EXPORT
char const *ps_get_hyp_easy(ps_decoder_t *ps, int32 *out_best_score,char const **out_uttid);

#endif

