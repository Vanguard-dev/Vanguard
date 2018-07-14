import { routerReducer, RouterReducerState } from '@ngrx/router-store';
import { ActionReducerMap, MetaReducer } from '@ngrx/store';
import { storeFreeze } from 'ngrx-store-freeze';

import { environment } from '../../environments/environment';

import { RouterStateUrl } from './utils';
import { serverNodeReducer, ServerNodeState } from './server-node/reducer';

export interface AppState {
  router: RouterReducerState<RouterStateUrl>;
  serverNode: ServerNodeState;
}

export const appReducer: ActionReducerMap<AppState> = {
  router: routerReducer,
  serverNode: serverNodeReducer
};

export const appMetaReducers: MetaReducer<AppState>[] = !environment.production
  ? [storeFreeze]
  : [];
