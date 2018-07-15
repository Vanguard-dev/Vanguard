import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@aspnet/signalr';
// import { MessagePackHubProtocol } from '@aspnet/signalr-protocol-msgpack';

import { IdentityService } from '../identity/identity.service';

@Injectable({
  providedIn: 'root'
})
export class HubListenerService {

  private connections: {[key: string]: { started: boolean, connection: HubConnection }} = {};

  constructor(private identity: IdentityService) {
    identity.ready$.subscribe(ready => {
      console.log('ready state changed', ready);
      if (ready) {
        Object.keys(this.connections).forEach(key => {
          if (!this.connections[key].started) {
            this.connections[key].connection.start().catch(() => {
              this.connections[key].started = false;
            });
            this.connections[key].started = true;
          }
        });
      }
    });
  }

  async registerHub(hubName: string, handler: (connection: HubConnection) => void): Promise<void> {
    if (!!this.connections[hubName]) {
      throw new Error(`A connection has already been built for ${hubName}`);
    }

    const connection = new HubConnectionBuilder()
      .withUrl(`/hubs/${hubName}`, { accessTokenFactory: () => this.identity.accessToken })
      // .withHubProtocol(new MessagePackHubProtocol()) TODO: Fix js client global error and reenable afterwards
      .configureLogging(LogLevel.None)
      .build();

    handler(connection);

    let started: boolean;
    if (this.identity.ready$.getValue() && this.identity.isAuthenticated) {
      await connection.start();
      started = true;
    }

    this.connections[hubName] = {
      started,
      connection
    };
  }

}
