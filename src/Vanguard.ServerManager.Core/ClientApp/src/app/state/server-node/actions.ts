import { Action } from '@ngrx/store';

import { createActionType } from '../utils';
import { ServerNode } from './reducer';

export const FETCH_SERVER_NODES = createActionType('FETCH_SERVER_NODES');
export const FETCH_SERVER_NODES_SUCCESS = createActionType('FETCH_SERVER_NODES_SUCCESS');
export const FETCH_SERVER_NODES_FAILURE = createActionType('FETCH_SERVER_NODES_FAILURE');
export const DELETE_SERVER_NODE = createActionType('DELETE_SERVER_NODE');
export const DELETE_SERVER_NODE_SUCCESS = createActionType('DELETE_SERVER_NODE_SUCCESS');
export const DELETE_SERVER_NODE_FAILURE = createActionType('DELETE_SERVER_NODE_FAILURE');

export class FetchServerNodes implements Action {
  readonly type = FETCH_SERVER_NODES;
}

export class FetchServerNodesSuccess implements Action {
  readonly type = FETCH_SERVER_NODES_SUCCESS;

  constructor(public payload: ServerNode[]) {}
}

export class FetchServerNodesFailure implements Action {
  readonly type = FETCH_SERVER_NODES_FAILURE;

  constructor(public payload: string) {}
}

export class DeleteServerNode implements Action {
  readonly type = DELETE_SERVER_NODE;

  constructor(public payload: ServerNode) {}
}

export class DeleteServerNodeSuccess implements Action {
  readonly type = DELETE_SERVER_NODE_SUCCESS;

  constructor(public payload: ServerNode) {}
}

export class DeleteServerNodeFailure implements Action {
  readonly type = DELETE_SERVER_NODE_FAILURE;

  constructor(public payload: string) {}
}

export type ServerNodeAction = FetchServerNodes |
  FetchServerNodesSuccess |
  FetchServerNodesFailure |
  DeleteServerNode |
  DeleteServerNodeSuccess |
  DeleteServerNodeFailure;
