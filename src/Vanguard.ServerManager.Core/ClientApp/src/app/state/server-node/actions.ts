import { Action } from '@ngrx/store';

import { createActionType } from '../utils';

export const DELETE_SERVER_NODE = createActionType('DELETE_SERVER_NODE');
export const DELETE_SERVER_NODE_SUCCESS = createActionType('DELETE_SERVER_NODE_SUCCESS');
export const DELETE_SERVER_NODE_FAILURE = createActionType('DELETE_SERVER_NODE_FAILURE');

export class DeleteServerNode implements Action {
  readonly type = DELETE_SERVER_NODE;

  constructor(public payload: string) {}
}

export class DeleteServerNodeSuccess implements Action {
  readonly type = DELETE_SERVER_NODE_SUCCESS;
}

export class DeleteServerNodeFailure implements Action {
  readonly type = DELETE_SERVER_NODE_FAILURE;

  constructor(public payload: string) {}
}

export type ServerNodeAction = DeleteServerNode | DeleteServerNodeSuccess | DeleteServerNodeFailure;
