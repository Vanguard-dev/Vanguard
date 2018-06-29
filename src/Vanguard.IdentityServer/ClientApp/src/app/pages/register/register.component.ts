import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { IdentityService } from '../../../../projects/vanguard-identity/src/lib/identity.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {

  registerForm: FormGroup;

  constructor(private formBuilder: FormBuilder, private identity: IdentityService) { }

  ngOnInit() {
    this.registerForm = this.formBuilder.group({
      username: ['test@test.com', Validators.required],
      password: ['TestTest', Validators.required],
      confirmPassword: ['TestTest', Validators.required]
    });
  }

  async onSubmit() {
    if (this.registerForm.valid) {
      const result = await this.identity.register(this.registerForm.value);
      console.log(result);
      // TODO: Check for errors and update error messages
    } else {
      // TODO: Mark form as dirty
    }
  }

}
