import {Component} from '@angular/core';
import {Button} from "primeng/button";
import {AuthService} from '../auth.service';
import {ProgressSpinner} from 'primeng/progressspinner';

@Component({
  selector: 'app-sign-in-btn',
  standalone: true,
  imports: [
    Button,
    ProgressSpinner
  ],
  templateUrl: './sign-in-btn.component.html',
  styleUrl: './sign-in-btn.component.scss'
})
export class SignInBtnComponent {
  loading = false;

  constructor(private authService: AuthService) {
  }

  login() {
    this.loading = true;
    this.authService.getLoginUrl().subscribe({
      next: ({redirectUrl}) => window.location.href = redirectUrl,
      error: () => this.loading = false
    });
  }
}
