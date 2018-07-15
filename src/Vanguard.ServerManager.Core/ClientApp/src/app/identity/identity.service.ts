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
  expiration_date: number;
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
  tokens?: IdentityToken;
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

export interface IdentityActionResult {
  success: boolean;
  errorMessage?: string;
}

const initialState: IdentityState = {
  tokens: null,
  profile: null
};

@Injectable({
  providedIn: 'root'
})
export class IdentityService {

  private readonly state: BehaviorSubject<IdentityState>;
  private refreshSubscription$: Subscription;

  // TODO: Refactor to server-side friendly version
  private get tokens(): IdentityToken {
    const tokensString = localStorage.getItem(STORAGE_IDENTITY_TOKEN);
    return tokensString == null ? null : JSON.parse(tokensString);
  }

  // TODO: Refactor to server-side friendly version
  private set tokens(token: IdentityToken) {
    const previousToken = this.tokens;
    if (previousToken != null && token.refresh_token == null) {
      token.refresh_token = previousToken.refresh_token;
    }
    localStorage.setItem(STORAGE_IDENTITY_TOKEN, JSON.stringify(token));
  }

  ready$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  state$: Observable<IdentityState>;
  tokens$: Observable<IdentityToken>;
  profile$: Observable<IdentityProfile>;
  authenticated$: Observable<boolean>;

  get isAuthenticated(): boolean {
    const tokens = this.tokens;
    return tokens && tokens.expiration_date > new Date().getTime();
  }

  get profile(): IdentityProfile {
    return this.state.getValue().profile;
  }

  get roles(): string[] {
    const profile = this.profile;
    return profile ? profile.role : [];
  }

  get accessToken(): string {
    const tokens = this.tokens;
    return tokens && tokens.expiration_date > new Date().getTime() ? tokens.access_token : null;
  }

  constructor(private http: HttpClient) {
    this.state = new BehaviorSubject<IdentityState>(initialState);
    this.state$ = this.state.asObservable();
    this.tokens$ = this.state.pipe(
      map(state => state.tokens)
    );
    this.profile$ = this.state.pipe(
      map(state => state.profile)
    );
    this.authenticated$ = this.tokens$.pipe(
      map(tokens => !!tokens)
    );
  }

  init(): Observable<IdentityToken> {
    return this.startupRefresh().pipe(
      tap(() => {
        this.scheduleRefresh();
        this.ready$.next(true);
      })
    );
  }

  async register(registration: IdentityRegistration): Promise<IdentityActionResult> {
    try {
      await this.http.post<any>(`/account/register`, registration).pipe(
        catchError(response => throwError(response))
      ).toPromise();

      return {
        success: true
      };
    } catch (err) {
      return {
        success: false,
        errorMessage: err.error.error_description
      };
    }
  }

  async login(credentials: IdentityCredentials): Promise<IdentityActionResult> {
    try {
      await this.fetchTokens(credentials, 'password').pipe(
        catchError(res => throwError(res)),
        tap(() => {
          this.scheduleRefresh();
          this.ready$.next(true);
        })
      ).toPromise();

      return {
        success: true
      };
    } catch (err) {
      return {
        success: false,
        errorMessage: err.error.error_description
      };
    }
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
      map(state => state.tokens),
      flatMap(token =>
        this.fetchTokens({ refresh_token: token.refresh_token }, 'refresh_token').pipe(
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

  private fetchTokens(data: IdentityCredentials | IdentityRefreshGrant, grantType: string): Observable<any> {
    const payload = Object.assign({}, data, { grant_type: grantType, scope: 'openid offline_access' });
    const params = new URLSearchParams();
    Object.keys(payload).forEach(key => params.append(key, payload[key]));
    return this.http.post<any>(`/connect/token`, params.toString(), oauthRequestOptions).pipe(
      tap((tokens: IdentityToken) => {
        tokens.expiration_date = new Date(new Date().getTime() + tokens.expires_in * 1000).getTime();
        const profile = jwtDecode(tokens.id_token);
        this.tokens = tokens;
        this.updateState({ profile, tokens });
      })
    );
  }

  private startupRefresh(): Observable<IdentityToken> {
    return of(this.tokens).pipe(
      flatMap((tokens: IdentityToken) => {
        if (!tokens) {
          this.updateState(initialState);
          return throwError('No token in Storage');
        }

        const profile = jwtDecode(tokens.id_token) as IdentityProfile;
        this.updateState({ tokens, profile });

        if (tokens.expiration_date > new Date().getTime()) {
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
    this.refreshSubscription$ = this.tokens$
      .pipe(
        first(),
        flatMap(token => interval(token.expires_in / 2 * 1000)),
        flatMap(() => this.refresh())
      )
      .subscribe();
  }

}
