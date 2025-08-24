import {Routes} from '@angular/router';
import {LandingComponent} from '../../features/landing/landing.component';
import {MainComponent} from './main.component';

export const mainRoutes: Routes = [{
  path: '',
  component: MainComponent,
  children: [
    {
      path: '',
      component: LandingComponent
    }
  ]
}]
