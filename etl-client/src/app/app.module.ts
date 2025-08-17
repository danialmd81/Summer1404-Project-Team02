import {NgModule} from "@angular/core";
import {BrowserModule} from "@angular/platform-browser";
import {AppComponent} from './app.component';
import {KeycloakAngularModule, KeycloakService} from 'keycloak-angular';
import {provideAnimationsAsync} from '@angular/platform-browser/animations/async';
import {providePrimeNG} from 'primeng/config';
import Aura from '@primeuix/themes/aura';

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
  declarations: [AppComponent,],
  bootstrap: [AppComponent],
  imports: [BrowserModule],
  providers: [provideAnimationsAsync(),
    providePrimeNG({
      theme: {
        preset: Aura
      }
    })]
})
export class AppModule {
}
