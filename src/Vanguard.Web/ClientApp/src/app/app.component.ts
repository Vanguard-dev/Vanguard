import { Component } from '@angular/core';
import { SessionService } from './identity/session.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {

  constructor(private session: SessionService) {}

}
