import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { IdentityService } from './identity.service';

@Injectable({
  providedIn: 'root'
})
export class IdentityGuard implements CanActivate {

  constructor(private identity: IdentityService, private router: Router) {}

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
    let canActivate = this.identity.isAuthenticated;

    const roles = next.data['roles'] as string[];
    if (canActivate && roles && roles.length) {
      for (const role of roles) {
        canActivate = this.identity.roles.indexOf(role) > -1;
        if (!canActivate) {
          break;
        }
      }
    }

    if (!canActivate) {
      const returnUrl = state.url || '/';
      this.router.navigate(['/login'], { queryParams: { returnUrl } });
    }

    return canActivate;
  }
}
