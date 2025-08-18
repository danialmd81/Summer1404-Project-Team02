import { Component } from '@angular/core';
import {TopbarComponent} from './components/topbar/topbar.component';
import {SidebarComponent} from './components/sidebar/sidebar.component';
import {RouterOutlet} from '@angular/router';

@Component({
  selector: 'app-main',
  standalone: true,
  imports: [
    TopbarComponent,
    SidebarComponent,
    RouterOutlet
  ],
  templateUrl: './main.component.html',
  styleUrl: './main.component.scss'
})
export class MainComponent {
}
