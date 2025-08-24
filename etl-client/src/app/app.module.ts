import {NgModule} from "@angular/core";
import {BrowserModule} from "@angular/platform-browser";
import {AppComponent} from './app.component';
import {provideAnimationsAsync} from '@angular/platform-browser/animations/async';
import {providePrimeNG} from 'primeng/config';
import {RouterModule} from '@angular/router';
import {appRoutes} from './app.routes';
import {CustomPreset} from './theme/mypreset';
import {MainModule} from './layout/main/main.module';
import {DashboardModule} from './layout/dashboard/dashboard.module';


@NgModule({
  bootstrap: [AppComponent],
  declarations: [AppComponent],
  imports: [BrowserModule, RouterModule.forRoot(appRoutes), MainModule, DashboardModule],
  providers: [provideAnimationsAsync(),
    providePrimeNG({
      theme: {
        preset: CustomPreset
      }
    })]
})
export class AppModule {
}
