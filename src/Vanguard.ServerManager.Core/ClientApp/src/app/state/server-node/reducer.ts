import { createFeatureSelector, createSelector } from '@ngrx/store';
import { DELETE_SERVER_NODE, DELETE_SERVER_NODE_SUCCESS, FETCH_SERVER_NODES_SUCCESS, ServerNodeAction } from './actions';

export interface ServerNode {
  id?: string;
  name: string;
  publicKey: string;
}

export interface ServerNodeState {
  nodes: ServerNode[];
}

const initialState: ServerNodeState = {
  nodes: []
};

export function serverNodeReducer(state: ServerNodeState = initialState, action: ServerNodeAction): ServerNodeState {
  switch (action.type) {
    case FETCH_SERVER_NODES_SUCCESS:
      return {
        ...state,
        nodes: action.payload
      };
    case DELETE_SERVER_NODE_SUCCESS:
    case DELETE_SERVER_NODE:
      return {
        ...state,
        nodes: state.nodes.splice(state.nodes.findIndex(t => t.id === action.payload.id), 1)
      };
    default:
      return state;
  }
}

export const serverNodeStateSelector = createFeatureSelector('serverNode');

export const serverNodeListSelector = createSelector(
  serverNodeStateSelector,
  (state: ServerNodeState) => state.nodes
);
