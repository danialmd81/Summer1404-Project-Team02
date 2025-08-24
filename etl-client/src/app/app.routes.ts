import {Routes} from '@angular/router';
import {DashboardComponent} from './layout/dashboard/dashboard.component';
import {MainComponent} from './layout/main/main.component';

export const appRoutes: Routes = [
  {
    path: '', component: MainComponent,
    loadChildren: () => import("./layout/main/main.module").then(m => m.MainModule)
  },
  {
    path: 'dashboard', loadChildren: () => import("./layout/dashboard/dashboard.module").then(m => m.DashboardModule)
  },
]
