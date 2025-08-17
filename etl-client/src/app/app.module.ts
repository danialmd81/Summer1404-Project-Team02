import {NgModule, APP_INITIALIZER} from '@angular/core';
import {BrowserModule} from '@angular/platform-browser';
import {AppComponent} from './app.component';
import {KeycloakAngularModule, KeycloakService} from 'keycloak-angular';

function initializeKeycloak(keycloak: KeycloakService) {
  return () =>
    keycloak.init({
      config: {
        // this is the url shoud devops give use
        url: 'http://localhost:8000/',
        realm: 'myrealm',
        clientId: 'my-angular-client',
      },
      initOptions: {
        onLoad: 'login-required',
        checkLoginIframe: true,
      },
    });
}

@NgModule({
  declarations: [AppComponent],
  imports: [BrowserModule, KeycloakAngularModule],
  providers: [
    {
      provide: APP_INITIALIZER,
      useFactory: initializeKeycloak,
      deps: [KeycloakService],
    },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {
}
