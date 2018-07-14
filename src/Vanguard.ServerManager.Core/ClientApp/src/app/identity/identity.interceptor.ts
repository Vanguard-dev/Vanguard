import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { IdentityService } from './identity.service';

@Injectable({
  providedIn: 'root'
})
export class IdentityInterceptor implements HttpInterceptor {

  constructor(private identity: IdentityService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    let finalReq = req;

    if (this.identity.isAuthenticated) {
      const accessToken = this.identity.accessToken;
      if (!accessToken) {
        throw new Error('Missing idToken even though the session is authenticated');
      }

      finalReq = finalReq.clone({
        setHeaders: {
          Authorization: `Bearer ${accessToken}`
        }
      });
    }

    return next.handle(finalReq);
  }

}
