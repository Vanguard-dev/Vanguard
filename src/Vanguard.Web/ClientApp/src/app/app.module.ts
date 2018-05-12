import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { PlaceholderComponent } from './placeholder/placeholder.component';
import { IdentityModule } from './identity/identity.module';

@NgModule({
  declarations: [
    AppComponent,
    PlaceholderComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    IdentityModule,
    RouterModule.forRoot([
      { path: '', component: PlaceholderComponent, pathMatch: 'full' },
    ])
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
