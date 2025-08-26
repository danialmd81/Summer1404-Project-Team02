import {Injectable} from '@angular/core';
import {HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from '@angular/common/http';
import {Observable} from 'rxjs';

@Injectable()
export class BaseUrlInterceptor implements HttpInterceptor {
  private readonly _baseUrl = "https://192.168.25.175:7238/api";

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!/^https?:\/\//i.test(req.url)) {
      const apiReq = req.clone({url: `${this._baseUrl}${req.url}`});
      return next.handle(apiReq);
    }
    return next.handle(req);
  }

}
