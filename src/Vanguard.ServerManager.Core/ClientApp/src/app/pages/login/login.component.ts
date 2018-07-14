import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { IdentityService } from '../../identity/identity.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  private returnUrl: string;

  loginForm: FormGroup;
  errorMessage: string;

  constructor(private identity: IdentityService, private formBuilder: FormBuilder, private router: Router, private route: ActivatedRoute) {
    this.route.queryParams.subscribe(params => {
      this.returnUrl = params['returnUrl'];
    });
  }

  ngOnInit() {
    this.loginForm = this.formBuilder.group({
      username: ['test@test.com', Validators.required],
      password: ['TestTest', Validators.required]
    });
  }

  async onSubmit() {
    if (this.loginForm.valid) {
      const result = await this.identity.login(this.loginForm.value);
      if (result.success) {
        await this.router.navigateByUrl(this.returnUrl || '/');
      } else {
        this.errorMessage = result.errorMessage;
        setTimeout(() => {
          this.errorMessage = '';
        }, 10000);
      }
    }
  }

}
