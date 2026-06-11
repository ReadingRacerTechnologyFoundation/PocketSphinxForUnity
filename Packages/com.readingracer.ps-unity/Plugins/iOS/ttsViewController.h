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
#import <AVFoundation/AVFoundation.h>

@interface ttsViewController : UIViewController <AVSpeechSynthesizerDelegate> {
    AVSpeechSynthesizer *synthesizer;
    
    float speechRate;
    //used for callbacks
    NSString* gameObjectName;
    NSString* functionName;
}
@property (strong, nonatomic) AVSpeechSynthesizer *synthesizer;

@property (retain) NSString* gameObjectName;
@property (retain) NSString* functionName;
@property (nonatomic) float speechRate;

@end

