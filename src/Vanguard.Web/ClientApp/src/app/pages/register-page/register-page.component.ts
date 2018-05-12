import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SessionService } from '../../identity/session.service';

@Component({
  selector: 'app-register-page',
  templateUrl: './register-page.component.html',
  styleUrls: ['./register-page.component.scss']
})
export class RegisterPageComponent implements OnInit {

  registerForm: FormGroup;

  constructor(private formBuilder: FormBuilder, private session: SessionService) { }

  ngOnInit() {
    this.registerForm = this.formBuilder.group({
      email: ['test@test.com', Validators.required],
      password: ['TestTest', Validators.required],
      confirmPassword: ['TestTest', Validators.required]
    });
  }

  async onSubmit() {
    if (this.registerForm.valid) {
      const result = await this.session.register(this.registerForm.value);
      console.log(result);
      // TODO: Check for errors and update error messages
    } else {
      // TODO: Mark form as dirty
    }
  }

}
