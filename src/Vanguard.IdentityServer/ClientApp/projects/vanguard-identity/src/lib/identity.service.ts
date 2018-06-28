import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { catchError, first, flatMap, map, tap } from 'rxjs/operators';
import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Subscription } from 'rxjs/Subscription';
import { throwError } from 'rxjs/internal/observable/throwError';
import { of } from 'rxjs/internal/observable/of';
import { interval } from 'rxjs/internal/observable/interval';

function jwtDecode(token: string): any {
  const encodedDataPart = token.split('.')[1]
    .replace('-', '+')
    .replace('_', '/');

  return JSON.parse(atob(encodedDataPart));
}

const STORAGE_IDENTITY_TOKEN = 'VANGUARD_IDENTITY_TOKEN';

const oauthRequestOptions = { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } };

export interface IdentityToken {
  access_token: string;
  refresh_token: string;
  id_token: string;
  expires_in: number;
  token_type: string;
  expiration_date: string;
}

export interface IdentityProfile {
  sub: string;
  jti: string;
  useage: string;
  at_hash: string;
  nbf: number;
  exp: number;
  iat: number;
  iss: string;

  unique_name: string;
  email_confirmed: boolean;
  role: string[];
}

export interface IdentityState {
  token?: IdentityToken;
  profile?: IdentityProfile;
}

export interface IdentityRefreshGrant {
  refresh_token: string;
}

export interface IdentityCredentials {
  username: string;
  password: string;
}

export interface IdentityRegistration {
  userName: string;
  password: string;
  confirmPassword: string;
}

const initialState: IdentityState = {
  token: null,
  profile: null
};

@Injectable({
  providedIn: 'root'
})
export class IdentityService {

  private readonly state: BehaviorSubject<IdentityState>;
  private refreshSubscription$: Subscription;

  // TODO: Refactor to server-side friendly version
  private get token(): IdentityToken {
    const tokenString = localStorage.getItem(STORAGE_IDENTITY_TOKEN);
    return tokenString == null ? null : JSON.parse(tokenString);
  }

  // TODO: Refactor to server-side friendly version
  private set token(token: IdentityToken) {
    const previousToken = this.token;
    if (previousToken != null && token.refresh_token == null) {
      token.refresh_token = previousToken.refresh_token;
    }
    localStorage.setItem(STORAGE_IDENTITY_TOKEN, JSON.stringify(token));
  }

  state$: Observable<IdentityState>;
  token$: Observable<IdentityToken>;
  profile$: Observable<IdentityProfile>;
  authenticated$: Observable<boolean>;

  constructor(private http: HttpClient) {
    this.state = new BehaviorSubject<IdentityState>(initialState);
    this.state$ = this.state.asObservable();
    this.token$ = this.state.pipe(
      map(state => state.token)
    );
    this.profile$ = this.state.pipe(
      map(state => state.profile)
    );
    this.authenticated$ = this.token$.pipe(
      map(tokens => !!tokens)
    );
  }

  init(): Observable<IdentityToken> {
    return this.startupRefresh().pipe(
      tap(() => this.scheduleRefresh())
    );
  }

  register(registration: IdentityRegistration): Observable<any> {
    return this.http.post<any>(`/account/register`, registration).pipe(
      catchError(response => throwError(response))
    );
  }

  login(credentials: IdentityCredentials): Observable<any> {
    console.log(credentials);
    return this.fetchToken(credentials, 'password').pipe(
      catchError(response => throwError(response)),
      tap(response => this.scheduleRefresh())
    );
  }

  logout(): void {
    this.updateState(initialState);
    if (this.refreshSubscription$) {
      this.refreshSubscription$.unsubscribe();
    }
    this.removeToken();
  }

  refresh(): Observable<IdentityToken> {
    return this.state.pipe(
      first(),
      map(state => state.token),
      flatMap(token =>
        this.fetchToken({ refresh_token: token.refresh_token }, 'refresh_token').pipe(
          catchError(() => throwError('Session expired'))
        )
      )
    );
  }

  private updateState(newState: IdentityState): void {
    const previousState = this.state.getValue();
    this.state.next(Object.assign({}, previousState, newState));
  }

  private removeToken(): void {
    localStorage.removeItem(STORAGE_IDENTITY_TOKEN);
  }

  private fetchToken(data: IdentityCredentials | IdentityRefreshGrant, grantType: string): Observable<any> {
    const payload = Object.assign({}, data, { grant_type: grantType, scope: 'openid offline_access' });
    const params = new URLSearchParams();
    Object.keys(payload).forEach(key => params.append(key, payload[key]));
    return this.http.post<any>(`/connect/token`, params.toString(), oauthRequestOptions).pipe(
      tap((token: IdentityToken) => {
        token.expiration_date = new Date(new Date().getTime() + token.expires_in * 1000).getTime().toString();
        const profile = jwtDecode(token.id_token);
        this.token = token;
        this.updateState({ profile, token });
      })
    );
  }

  private startupRefresh(): Observable<IdentityToken> {
    return of(this.token).pipe(
      flatMap((token: IdentityToken) => {
        if (!token) {
          this.updateState(initialState);
          return throwError('No token in Storage');
        }

        const profile = jwtDecode(token.id_token) as IdentityProfile;
        this.updateState({ token, profile });

        if (+token.expiration_date > new Date().getTime()) {
          this.updateState(this.state.getValue());
        }

        return this.refresh();
      }),
      catchError(error => {
        this.logout();
        return throwError(error);
      })
    );
  }

  private scheduleRefresh(): void {
    this.refreshSubscription$ = this.token$
      .pipe(
        first(),
        flatMap(token => interval(token.expires_in / 2 * 1000)),
        flatMap(() => this.refresh())
      )
      .subscribe();
  }

}
