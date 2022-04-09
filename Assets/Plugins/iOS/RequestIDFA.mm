#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <AdSupport/AdSupport.h>

extern "C" void _requestIDFA() {
    if (@available(iOS 14.5,*)) {
//        ATTrackingManagerAuthorizationStatus status = [ATTrackingManager trackingAuthorizationStatus];
//        NSLog(@"Helll World!Hello World!Hello World!Hello World!Hello World!Hello World!Hello World!Hello World!Hello World!Hello World!Hello World!Hello World!");
            [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
//                NSLog(@"Hello World!Hello World!Hello World!Hello World!Hello World!Hello World!Hello World!Hello World!Hello World!Hello World!Hello World!Hello World!");
            }
         ];
        
    }
    
    
    
}
