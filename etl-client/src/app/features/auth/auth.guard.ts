import {Injectable} from '@angular/core';
import {CanMatch, Route, Router, UrlSegment, UrlTree} from '@angular/router';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';
import {AuthService} from './auth.service';

@Injectable({providedIn: 'root'})
export class AuthGuard implements CanMatch {
  constructor(private authService: AuthService, private router: Router) {
  }

  canMatch(
    route: Route,
    segments: UrlSegment[]
  ): Observable<boolean | UrlTree> {
    return this.authService.checkAuth().pipe(
      map(isAuth => {
        if (!isAuth) {
          return this.router.createUrlTree(['/']);
        }
        return true;
      })
    );
  }
}
