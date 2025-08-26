import { Component } from '@angular/core';
import {RouterLink} from '@angular/router';
import {Button} from 'primeng/button';
import {SignInBtnComponent} from '../../../../features/auth/sign-in-btn/sign-in-btn.component';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    RouterLink,
    Button,
    SignInBtnComponent
  ],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss'
})
export class NavbarComponent {

}
