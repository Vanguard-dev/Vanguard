import { NgModule, Optional, SkipSelf } from '@angular/core';
import { HTTP_INTERCEPTORS } from '@angular/common/http';

import { IdentityInterceptor } from './identity.interceptor';

@NgModule({
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: IdentityInterceptor,
      multi: true
    }
  ]
})
export class VanguardIdentityModule {
  constructor(@Optional() @SkipSelf() parentModule: VanguardIdentityModule) {
    if (parentModule) {
      throw new Error('VanguardIdentityModule is already loaded. Import it in the root module only.');
    }
  }
}
