import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { ClarityModule } from '@clr/angular';

import { StoreModule } from '@ngrx/store';
import { RouterStateSerializer, StoreRouterConnectingModule } from '@ngrx/router-store';
import { StoreDevtoolsModule } from '@ngrx/store-devtools';
import { EffectsModule } from '@ngrx/effects';

import { environment } from '../environments/environment';

import { appMetaReducers, appReducer } from './state/app.reducer';

import { AppComponent } from './app.component';
import { LoginComponent } from './pages/login/login.component';
import { PageNotFoundComponent } from './pages/page-not-found/page-not-found.component';
import { AppRoutingModule } from './app-routing.module';
import { ReactiveFormsModule } from '@angular/forms';
import { RegisterComponent } from './pages/register/register.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { CustomRouteSerializer } from './state/utils';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    PageNotFoundComponent,
    RegisterComponent,
    DashboardComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    ClarityModule,
    BrowserAnimationsModule,
    ReactiveFormsModule,

    StoreModule.forRoot(appReducer, {
      metaReducers: appMetaReducers
    }),
    StoreRouterConnectingModule.forRoot(),
    EffectsModule.forRoot([

    ]),
    !environment.production ? StoreDevtoolsModule.instrument() : [],

    AppRoutingModule
  ],
  providers: [
    {
      provide: RouterStateSerializer,
      useClass: CustomRouteSerializer
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
