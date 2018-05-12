import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, isPlatformServer } from '@angular/common';
import { getSession } from './helpers';
import { HttpClient } from '@angular/common/http';

export interface SessionActionResult {
  succeeded: boolean;
  errors?: {[key: string]: string[]};
}

export interface RegisterModel {
  email: string;
  password: string;
  confirmPassword: string;
}

export interface LoginModel {
  email: string;
  password: string;
}

@Injectable()
export class SessionService {

  constructor(@Inject(PLATFORM_ID) private platformId: Object, private http: HttpClient) {
    if (isPlatformBrowser(platformId)) {
      // TODO: Load session from local storage
      console.log('Load from local storage');
    } else if (isPlatformServer(platformId)) {
      // TODO: Load session from injected data
      console.log('Load from injected value', getSession());
    }
  }

  async register(model: RegisterModel): Promise<SessionActionResult> {
    try {
      await this.http.post('/api/account/register', model, {
        observe: 'response',
        withCredentials: true
      }).toPromise();
      return {
        succeeded: true
      };
    } catch (err) {
      if (err.status === 400) {
        return {
          succeeded: false,
          errors: err.error
        };
      }

      return {
        succeeded: false,
        errors: {
          'Unknown': ['']
        }
      };
    }
  }

  async login(credentials: LoginModel): Promise<SessionActionResult> {
    try {
      await this.http.post('/api/account/login', credentials, {
        observe: 'response',
        withCredentials: true
      }).toPromise();
      // TODO: Fetch profile and store session
      return {
        succeeded: true
      };
    } catch (err) {
      if (err.status === 400) {
        return {
          succeeded: false,
          errors: err.error
        };
      }

      return {
        succeeded: false,
        errors: {
          'Unknown': ['']
        }
      };
    }
  }

}
