import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse } from '@angular/common/http';
import { Injectable, Injector } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/do';
import { map, switchMap, take, tap } from 'rxjs/operators';
import { SnackTypes } from '../../models/snack-bar.model';
import { AuthService } from '../auth.service';
import { SnackBarService } from './../snack-bar.service';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  private snack; SnackBarService;
  constructor(
    private injector: Injector
  ) {

    this.snack = injector.get(SnackBarService);
  }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!request.headers.has('skipAuth')) {
      const auth = this.injector.get(AuthService);
      return auth.getAuthorizationToken().pipe(
        take(1),
        map((token) => {
          if (token) {
            request = request.clone({ setHeaders: { Authorization: 'bearer ' + token } });
          }
          return request;
        }),
        switchMap((req => next.handle(req))),
        tap({
          next: (event: HttpEvent<any>) => {
            if (event instanceof HttpResponse) {
              // do stuff with response
            }
          },
          error: (err: any) => this.handleError(auth, err)
        })
      );
    } else {
      return next.handle(request);
    }
  }

  private handleError(auth: AuthService, err: any) {
    if (err instanceof HttpErrorResponse) {
      if (err.error) {
        this.snack.snackBarSubject.next({
          type: SnackTypes.ERROR,
          message: err.error.error_description || err.message || err.name
        });
      }
      if (err.status === 401) {
        auth.logout();
      }
    }
  }
}
