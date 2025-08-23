import {Component} from '@angular/core';
import {TopbarComponent} from './components/topbar/topbar.component';
import {SidebarComponent} from './components/sidebar/sidebar.component';
import {RouterOutlet} from '@angular/router';
import {UsersComponent} from '../../features/users/users.component';

@Component({
  selector: 'app-main',
  standalone: true,
  imports: [
    TopbarComponent,
    SidebarComponent,
    RouterOutlet,
    UsersComponent
  ],
  templateUrl: './main.component.html',
  styleUrl: './main.component.scss'
})
export class MainComponent {
}
