import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private readonly authService: AuthService) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const shouldAttach = req.url.startsWith(environment.apiBaseUrl);

    if (!shouldAttach) {
      return next.handle(req);
    }

    let headers = req.headers;
    const token = this.authService.token;
    const userId = this.authService.userId;

    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }

    if (userId) {
      headers = headers.set('X-User-Id', userId);
    }

    return next.handle(req.clone({ headers }));
  }
}
