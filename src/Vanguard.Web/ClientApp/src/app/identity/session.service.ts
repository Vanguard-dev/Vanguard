import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, isPlatformServer } from '@angular/common';
import { getSession } from './helpers';

@Injectable()
export class SessionService {

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {
    if (isPlatformBrowser(platformId)) {
      // TODO: Load session from local storage
      console.log('Load from local storage');
    } else if (isPlatformServer(platformId)) {
      // TODO: Load session from injected data
      console.log('Load from injected value', getSession());
    }
  }

}
