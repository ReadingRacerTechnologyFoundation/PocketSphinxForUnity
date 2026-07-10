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
using UnityEngine;
using System.Collections;

namespace Rrtf.Sphinx
{
	using SphinxNative;
	public class LogMath
	{
		public readonly logmath_t RawLogMath;

		public LogMath()
		{
			RawLogMath = SB_LogMath.logmath_initialize(1.0001f, 0, false);
		}

		public LogMath(float logBase, int shift, bool use_table)
		{
			RawLogMath = SB_LogMath.logmath_initialize(logBase, shift, use_table);
		}

		//TODO needs a destructor to deallocate memory
	}
}
