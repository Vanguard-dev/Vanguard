import { NgModule, Optional, SkipSelf } from '@angular/core';

@NgModule({})
export class VanguardIdentityModule {
  constructor(@Optional() @SkipSelf() parentModule: VanguardIdentityModule) {
    if (parentModule) {
      throw new Error('VanguardIdentityModule is already loaded. Import it in the root module only.');
    }
  }
}
