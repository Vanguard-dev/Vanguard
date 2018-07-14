import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { HttpClient } from '@angular/common/http';
import { defer, Observable, of } from 'rxjs';
import { Action } from '@ngrx/store';
import {
  DELETE_SERVER_NODE,
  DeleteServerNodeFailure,
  DeleteServerNodeSuccess,
  FETCH_SERVER_NODES,
  FetchServerNodes, FetchServerNodesFailure,
  FetchServerNodesSuccess
} from './actions';
import { catchError, map, mergeMap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ServerNode } from './reducer';

@Injectable()
export class ServerNodeEffects {

  @Effect()
  fetch$: Observable<Action> = this.actions$.pipe(
    ofType(FETCH_SERVER_NODES),
    mergeMap(() => this.http.get<ServerNode[]>(`${environment.apiRootUri}/api/node`).pipe(
      map(result => new FetchServerNodesSuccess(result)),
      catchError(err => of(new FetchServerNodesFailure(err)))
    ))
  );

  @Effect()
  delete$: Observable<Action> = this.actions$.pipe(
    ofType(DELETE_SERVER_NODE),
    mergeMap((payload: ServerNode) => this.http.delete(`${environment.apiRootUri}/api/node/${payload.id}`).pipe(
      map(() => new DeleteServerNodeSuccess(payload)),
      catchError(err => of(new DeleteServerNodeFailure(err)))
    ))
  );

  @Effect()
  init$: Observable<Action> = defer(() => of(new FetchServerNodes()));

  constructor(private actions$: Actions, private http: HttpClient) {}

}
