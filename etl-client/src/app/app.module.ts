import {NgModule} from "@angular/core";
import {BrowserModule} from "@angular/platform-browser";
import {AppComponent} from './app.component';
import {provideAnimationsAsync} from '@angular/platform-browser/animations/async';
import {providePrimeNG} from 'primeng/config';
import Aura from '@primeuix/themes/aura';
import {RouterModule} from '@angular/router';
import {appRoutes} from './app.routes';
import {AuthComponent} from './layout/auth/auth.component';
import {TopbarComponent} from './layout/main/components/topbar/topbar.component';
import {SidebarComponent} from './layout/main/components/sidebar/sidebar.component';


@NgModule({
  declarations: [AppComponent, AuthComponent],
  bootstrap: [AppComponent],
  imports: [BrowserModule, RouterModule.forRoot(appRoutes), TopbarComponent, SidebarComponent],
  providers: [provideAnimationsAsync(),
    providePrimeNG({
      theme: {
        preset: Aura
      }
    })]
})
export class AppModule {
}
