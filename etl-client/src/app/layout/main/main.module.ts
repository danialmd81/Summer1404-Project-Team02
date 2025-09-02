import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {MainComponent} from './main.component';
import {RouterModule} from '@angular/router';
import {mainRoutes} from './main.routes';
import {LandingComponent} from '../../features/landing/landing.component';
import {NavbarComponent} from "./components/navbar/navbar.component";
import {AuthCallbackComponent} from '../../features/auth/auth-callback/auth-callback.component';


@NgModule({
  declarations: [MainComponent],
  imports: [
    CommonModule,
    RouterModule.forChild(mainRoutes),
    LandingComponent,
    NavbarComponent,
    AuthCallbackComponent
  ]
})
export class MainModule {
}
