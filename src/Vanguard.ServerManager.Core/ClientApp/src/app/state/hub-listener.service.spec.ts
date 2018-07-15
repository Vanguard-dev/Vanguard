import { TestBed, inject } from '@angular/core/testing';

import { HubListenerService } from './hub-listener.service';

describe('HubListenerService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [HubListenerService]
    });
  });

  it('should be created', inject([HubListenerService], (service: HubListenerService) => {
    expect(service).toBeTruthy();
  }));
});
