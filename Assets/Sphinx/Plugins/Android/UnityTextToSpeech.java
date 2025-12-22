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
package com.seashells.unitytts;

import android.speech.tts.TextToSpeech;
import android.speech.tts.UtteranceProgressListener;
import android.speech.tts.TextToSpeech.OnInitListener;
import android.util.Log;
import com.unity3d.player.UnityPlayer;
import java.util.HashMap;

public class UnityTextToSpeech extends UtteranceProgressListener implements OnInitListener {
    public static final String DONE_MSG = "DONE";
    public static final String BEGIN_MSG = "BEGIN";
    private TextToSpeech tts;
    private String gameObjectName;
    private String OnFinishedMethodName;
    private HashMap<String, String> map;
    private boolean isInitialized;
    private int skipCallbackCounter;

    public UnityTextToSpeech(String gameObjectName, String UnityMethodName) {
        this.tts = new TextToSpeech(UnityPlayer.currentActivity, this);
        this.tts.setOnUtteranceProgressListener(this);
        this.gameObjectName = gameObjectName;
        this.OnFinishedMethodName = UnityMethodName;
        this.isInitialized = false;
        this.skipCallbackCounter = 0;
        this.map = new HashMap();
    }

    public void setSpeechRate(float rate) {
        this.tts.setSpeechRate(rate);
    }

    public void speak(String words) {
        if(this.isInitialized) {
            if(this.tts.isSpeaking()) {
                this.skipCallbackCounter = 2;
            }

            this.map.put("utteranceId", words);
            this.tts.speak(words, 0, this.map);
        }
    }

    public boolean isSpeaking() {
        return !this.isInitialized?false:this.tts.isSpeaking();
    }

    public void onInit(int status) {
        Log.i("UnityTextToSpeech", "UnityTextToSpeech.OnInit(): " + status);
        this.isInitialized = true;
    }

    public void onStart(String utteranceId) {
        if(this.skipCallbackCounter > 0) {
            --this.skipCallbackCounter;
        } else {
            UnityPlayer.UnitySendMessage(this.gameObjectName, this.OnFinishedMethodName, BEGIN_MSG);
        }
    }

    public void onDone(String utteranceId) {
        if(this.skipCallbackCounter > 0) {
            --this.skipCallbackCounter;
        } else {
            UnityPlayer.UnitySendMessage(this.gameObjectName, this.OnFinishedMethodName, DONE_MSG);
        }
    }

    /** @deprecated */
    @Deprecated
    public void onError(String utteranceId) {
        if(this.skipCallbackCounter > 0) {
            --this.skipCallbackCounter;
        } else {
            UnityPlayer.UnitySendMessage(this.gameObjectName, this.OnFinishedMethodName, DONE_MSG);
        }
    }

    public void releaseTTS() {
        if(this.tts != null) {
            this.tts.stop();
            this.tts.shutdown();
            this.tts = null;
        }
    }
}
