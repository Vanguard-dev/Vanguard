import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { IdentityService } from 'vanguard-identity';

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
      const result = await this.identity.login(this.loginForm.value).toPromise();
      console.log(result);
      if (result.succeeded) {
        await this.router.navigateByUrl(this.returnUrl || '/');
      } else {
        // TODO: Display error based on backend return
        this.errorMessage = 'Tarkista käyttäjätunnus ja salasana.';
        setTimeout(() => {
          this.errorMessage = '';
        }, 10000);
      }
    }
  }

}
