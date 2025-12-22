
extern "C" {
    void RROpenURL(char* url);
}

void RROpenURL(char* url){
    if(url == nil){
        NSLog(@"url is empty. error");
        return;
    }
    
    NSString *sms = [NSString stringWithUTF8String: url];
    NSString *finalurl = [sms stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
    [[UIApplication sharedApplication] openURL:[NSURL URLWithString:finalurl]];
}

