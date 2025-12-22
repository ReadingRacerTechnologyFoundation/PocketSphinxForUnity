//
//  LocaleInfo.m
//  Unity-iPhone
//
//  Created by rodrigo cano on 9/4/15.
//
//

#import <Foundation/Foundation.h>



extern "C"
{
    char* copyString(const char* string)
    {
        if(string == NULL)
            return NULL;
        
        char* res = (char*)malloc(strlen(string) + 1);
        strcpy(res, string);
        
        return res;
    }
    
    char* Country()
    {
        NSLocale *locale = [NSLocale currentLocale];
        NSString *langAndRegion = [locale localeIdentifier];
       // NSString *country = [locale displayNameForKey:NSLocaleIdentifier value:[locale localeIdentifier]];
        NSString *country = [[langAndRegion componentsSeparatedByString:@"_"] objectAtIndex:1];
        country = [country stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]];
        
        return copyString([country UTF8String]);
    }
}
