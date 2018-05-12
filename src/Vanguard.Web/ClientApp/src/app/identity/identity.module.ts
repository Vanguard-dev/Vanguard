import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SessionService } from './session.service';

@NgModule({
  imports: [
    CommonModule
  ],
  declarations: [],
  providers: [
    SessionService
  ],
  exports: []
})
export class IdentityModule { }
