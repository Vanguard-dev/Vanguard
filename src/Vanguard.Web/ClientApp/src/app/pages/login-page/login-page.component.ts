import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SessionService } from '../../identity/session.service';

@Component({
  selector: 'app-login-page',
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.scss']
})
export class LoginPageComponent implements OnInit {

  loginForm: FormGroup;

  constructor(private formBuilder: FormBuilder, private session: SessionService) { }

  ngOnInit() {
    this.loginForm = this.formBuilder.group({
      email: ['test@test.com', Validators.required],
      password: ['TestTest', Validators.required]
    });
  }

  async onSubmit() {
    if (this.loginForm.valid) {
      const result = await this.session.login(this.loginForm.value);
      console.log(result);
      // TODO: Check for errors and update error messages
    } else {
      // TODO: Mark form as dirty
    }
  }

}
