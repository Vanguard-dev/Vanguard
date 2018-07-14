import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { Action } from '@ngrx/store';
import { DELETE_SERVER_NODE, DeleteServerNodeFailure, DeleteServerNodeSuccess } from './actions';
import { catchError, map, mergeMap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

@Injectable()
export class ServerNodeEffects {

  @Effect()
  delete$: Observable<Action> = this.actions$.pipe(
    ofType(DELETE_SERVER_NODE),
    mergeMap((payload: string) => this.http.delete(`${environment.apiRootUri}/api/servernode/${payload}`).pipe(
      map(() => new DeleteServerNodeSuccess()),
      catchError(err => of(new DeleteServerNodeFailure(err)))
    ))
  );

  constructor(private actions$: Actions, private http: HttpClient) {}

}
