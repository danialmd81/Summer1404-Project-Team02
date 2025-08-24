import {Component} from '@angular/core';
import {UserProfileBtnComponent} from './components/user-profile-btn/user-profile-btn.component';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [
    UserProfileBtnComponent
  ],
  templateUrl: './topbar.component.html',
  styleUrl: './topbar.component.scss'
})
export class TopbarComponent {


}
