import {NgModule} from "@angular/core";
import {BrowserModule} from "@angular/platform-browser";
import {AppComponent} from './app.component';
import {provideAnimationsAsync} from '@angular/platform-browser/animations/async';
import {providePrimeNG} from 'primeng/config';
import Aura from '@primeuix/themes/aura';


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
