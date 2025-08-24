import {Route} from '@angular/router';
import {DashboardComponent} from './dashboard.component';
import {UsersComponent} from '../../features/users/users.component';

export const dashboardRoutes: Route[] = [
  {
    path: "",
    component: DashboardComponent,
    children: [
      {
        path: "users",
        component: UsersComponent,
      }
    ]
  }
]
