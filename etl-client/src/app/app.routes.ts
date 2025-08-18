import {Routes} from '@angular/router';
import {AuthComponent} from './layout/auth/auth.component';
import {MainComponent} from './layout/main/main.component';

export const appRoutes: Routes = [
  {
    path: '', component: MainComponent, children: []
  },
  {path: 'auth', component: AuthComponent},
]
