import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { PlaceholderComponent } from './placeholder/placeholder.component';
import { IdentityModule } from './identity/identity.module';
import { RegisterPageComponent } from './pages/register-page/register-page.component';
import { LoginPageComponent } from './pages/login-page/login-page.component';

@NgModule({
  declarations: [
    AppComponent,
    PlaceholderComponent,
    RegisterPageComponent,
    LoginPageComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    ReactiveFormsModule,
    IdentityModule,
    RouterModule.forRoot([
      { path: '', component: PlaceholderComponent, pathMatch: 'full' },
      { path: 'register', component: RegisterPageComponent },
      { path: 'login', component: LoginPageComponent }
    ])
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
