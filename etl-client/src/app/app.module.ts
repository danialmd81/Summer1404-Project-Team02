import {NgModule} from "@angular/core";
import {BrowserModule} from "@angular/platform-browser";
import {AppComponent} from './app.component';
import {provideAnimationsAsync} from '@angular/platform-browser/animations/async';
import {providePrimeNG} from 'primeng/config';
import Aura from '@primeuix/themes/aura';
import {ButtonDemoComponent} from '../button-demo/button-demo.component';

@NgModule({
  declarations: [AppComponent,],
  bootstrap: [AppComponent],
  imports: [BrowserModule, ButtonDemoComponent],
  providers: [provideAnimationsAsync(),
    providePrimeNG({
      theme: {
        preset: Aura
      }
    })]
})
export class AppModule {
}
