#import <KakaoOpenSDK/KakaoOpenSDK.h>

extern "C" {
    void _sendKakaoLogin()
    {
        // Close old session
        if ( ! [[KOSession sharedSession] isOpen] ) {
            NSLog(@"in isOpen condition");
            [[KOSession sharedSession] close];
            NSLog(@"Old session closed");
        }

        // session open with completion handler
        [[KOSession sharedSession] openWithCompletionHandler:^(NSError *error) {
            if (error) {
                NSLog(@"login failed. - error: %@", error);
            }
            else {
                NSLog(@"login succeeded.");
            }
            
            // get user info
            [KOSessionTask userMeTaskWithCompletion:^(NSError * _Nullable errosr, KOUserMe * _Nullable me) {
                if (error){
                    NSLog(@"get user info failed. - error: %@", error);
                    UnitySendMessage("LoginManager", "SaveKakaoUserID", [me.ID UTF8String]);
                    UnitySendMessage("LoginManager", "SaveKakaoUserEmail", [me.account.email UTF8String]);
                } else {
                    NSLog(@"get user info. - user info: %@", me);
                    UnitySendMessage("LoginManager", "SaveKakaoUserID", [me.ID UTF8String]);
                    UnitySendMessage("LoginManager", "SaveKakaoUserEmail", [me.account.email UTF8String]);
                }
            }];
        }];
    }
}
