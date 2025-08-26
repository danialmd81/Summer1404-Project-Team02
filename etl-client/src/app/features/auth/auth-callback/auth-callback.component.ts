import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {AuthService} from '../auth.service';

@Component({
  selector: 'app-auth-callback',
  standalone: true,
  imports: [],
  templateUrl: './auth-callback.component.html',
  styleUrl: './auth-callback.component.scss'
})
export class AuthCallbackComponent implements OnInit {
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) {
  }

  ngOnInit() {
    const code = this.route.snapshot.queryParamMap.get('code');
    console.log(code);
    if (code) {
      this.authService.exchangeCodeForSession(code, '/dashboard').subscribe({
        // next: () => this.router.navigate(['/dashboard']),
        // error: () => this.router.navigate(['/'])
      });
    } else {
      // this.router.navigate(['/']);
    }
  }
}
