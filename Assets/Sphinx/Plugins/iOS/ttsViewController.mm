#import "ttsViewController.h"

NSString *const TTS_BEGIN_MSG = @"BEGIN";
NSString *const TTS_DONE_MSG = @"DONE";

@implementation ttsViewController
@synthesize synthesizer;
- (AVSpeechSynthesizer *)synthesizer {
    if (synthesizer == nil) {
        synthesizer = [[AVSpeechSynthesizer alloc] init];
    }
    return synthesizer;
}

@synthesize gameObjectName;
@synthesize functionName;
@synthesize speechRate;

- (void) convertTextToSpeech:(NSString *)text
{
    if(self.synthesizer.isSpeaking)
        [self.synthesizer stopSpeakingAtBoundary:AVSpeechBoundaryImmediate];
    
    AVSpeechUtterance *utterance = [[AVSpeechUtterance alloc] initWithString:text];
    utterance.rate = self.speechRate;
    utterance.preUtteranceDelay = 0;
    utterance.postUtteranceDelay = 0;
    utterance.voice = [AVSpeechSynthesisVoice voiceWithLanguage:@"en-us"];
    [self.synthesizer speakUtterance:utterance];
}

- (void) setDelegate {
    self.synthesizer.delegate = self;
}


-(void)speechSynthesizer:(AVSpeechSynthesizer *)synthesizer didFinishSpeechUtterance:(AVSpeechUtterance *)utterance
{
    UnitySendMessage([self.gameObjectName UTF8String], [self.functionName UTF8String], [TTS_DONE_MSG UTF8String]);
}

-(void)speechSynthesizer:(AVSpeechSynthesizer *)synthesizer didStartSpeechUtterance:(AVSpeechUtterance *)utterance
{
    UnitySendMessage([self.gameObjectName UTF8String], [self.functionName UTF8String], [TTS_BEGIN_MSG UTF8String]);
}

@end

extern "C" {
    void initTTS(float speechRate, char* gameObjectName, char* callbackMethodName);
    void TextToSpeech(char* text);
}

static ttsViewController *tts = nil;

void initTTS(float speechRate, char* gameObjectName, char* callbackMethodName)
{
    if(tts != nil) return;
    
    tts = [ttsViewController alloc];
    [tts setDelegate];
    
    if(speechRate <= 0) speechRate = .00001f;
    tts.speechRate = speechRate;
    
    //finds the ios version and slows the speechrate down if on iOS 9+
    NSString *ver = [[UIDevice currentDevice] systemVersion];
    int vNumber = [[ver componentsSeparatedByString:@"."][0] intValue];
    if(vNumber > 8) tts.speechRate = .435f;
    
    
    tts.gameObjectName = (gameObjectName != nil) ? [NSString stringWithUTF8String: gameObjectName] : @"";
    tts.functionName = (callbackMethodName != nil) ? [NSString stringWithUTF8String: callbackMethodName] : @"";
}

void TextToSpeech(char* text)
{
    if(tts == nil)
        return;
    
    NSString *sName = (text != nil) ? [NSString stringWithUTF8String: text] : [NSString stringWithUTF8String: ""];
    [tts convertTextToSpeech:sName];
}

