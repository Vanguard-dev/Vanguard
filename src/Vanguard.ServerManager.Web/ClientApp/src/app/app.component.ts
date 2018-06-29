import { Component, OnInit } from '@angular/core';
import { IdentityService } from './identity/identity.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {

  constructor(private identity: IdentityService, private router: Router) {}

  ngOnInit() {
    this.identity.init()
      .subscribe(
        () => console.log('Startup success'),
        error => console.warn(error)
      );
  }

  async onLogout() {
    this.identity.logout();
    await this.router.navigateByUrl('/');
  }

}
