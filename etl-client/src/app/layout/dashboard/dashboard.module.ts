import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {DashboardComponent} from './dashboard.component';
import {SidebarComponent} from './components/sidebar/sidebar.component';
import {TopbarComponent} from './components/topbar/topbar.component';
import {dashboardRoutes} from './dashboard.routes';
import {RouterModule} from '@angular/router';


@NgModule({
  declarations: [DashboardComponent],
  imports: [
    CommonModule,
    RouterModule.forChild(dashboardRoutes),
    SidebarComponent,
    TopbarComponent,
  ]
})
export class DashboardModule {
}
