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
import {HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {BaseUrlInterceptor} from './features/auth/interceptors/base-url.interceptor';
import {CredentialsInterceptor} from './features/auth/interceptors/credentials.interceptor';


@NgModule({
  bootstrap: [AppComponent],
  declarations: [AppComponent],
  imports: [BrowserModule, RouterModule.forRoot(appRoutes), MainModule, DashboardModule],
  providers: [provideAnimationsAsync(),
    providePrimeNG({
      theme: {
        preset: CustomPreset
      }
    }), provideHttpClient(withInterceptorsFromDi()),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: BaseUrlInterceptor,
      multi: true
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: CredentialsInterceptor,
      multi: true
    }]
})
export class AppModule {
}
