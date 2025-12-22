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
using System.Runtime.InteropServices;

public class iPhoneSpeaker : MonoBehaviour {

	#if UNITY_IPHONE
	[DllImport("__Internal")]
	private static extern void _forceToSpeaker();
	#endif


	//force the iphone to use the loudspeaker
	public static void ForceToSpeaker() {
		#if UNITY_IPHONE && !UNITY_EDITOR
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			_forceToSpeaker();
		}
		#endif
	}


}
