import {Injectable} from '@angular/core';
import {BehaviorSubject, catchError, map, Observable, of} from 'rxjs';
import {HttpClient} from '@angular/common/http';

export interface User {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private _userSubject = new BehaviorSubject<User | null>(null)
  public user$ = this._userSubject.asObservable();

  constructor(private http: HttpClient) {
  }

  public checkAuth() {
    return this.http.get<User>("/User/profile").pipe(
      map((res: User) => {
        this._userSubject.next(res)
        return true;
      }),
      catchError(() => {
        this._userSubject.next(null);
        return of(false);
      })
    )
  }

  public get user() {
    return this._userSubject.value;
  }

  public logout() {
    return this.http.post<void>("/logout", null, {})
  }

  public getLoginUrl(): Observable<{ redirectUrl: string }> {
    return this.http.get<{ redirectUrl: string }>('/auth/login?redirectPath=/auth/callback');
  }

  public exchangeCodeForSession(code: string, redirectPath: string) {
    return this.http.post('/auth/login-callback', {
      code,
      redirectPath
    });
  }
}
