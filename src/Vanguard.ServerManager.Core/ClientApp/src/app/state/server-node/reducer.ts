import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ServerNodeAction } from './actions';

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
    default:
      return state;
  }
}

export const serverNodeStateSelector = createFeatureSelector('serverNode');

export const serverNodeListSelector = createSelector(
  serverNodeStateSelector,
  (state: ServerNodeState) => state.nodes
);
