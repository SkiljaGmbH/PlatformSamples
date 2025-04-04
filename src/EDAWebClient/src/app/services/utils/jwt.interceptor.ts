import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/do';
import { SnackBarService } from '../snack-bar.service';
import {SnackTypes} from '../../models/snack-bar.model';
import {StorageService} from './storage.service';
import {AuthService} from '../auth.service';

export class JwtInterceptor implements HttpInterceptor {
    constructor(
        private snackBarService: SnackBarService,
        private storageService: StorageService,
        private authService: AuthService
    ) {}

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const token = this.authService.getAuthorizationToken();
        if (token) {
          request = request.clone({
            setHeaders: {Authorization: 'bearer ' + token }
          });
        }
        return next.handle(request).do((event: HttpEvent<any>) => {
            if (event instanceof HttpResponse) {
                // do stuff with response
            }
        }, (err: any) => {
            if (err instanceof HttpErrorResponse) {
              if (err.error) {
                this.snackBarService.snackBarSubject.next({type: SnackTypes.ERROR,
                  message: err.error.error_description || err.message || err.name});
              }
              if (err.status === 401) {
                this.authService.logout();
              }
            }
        });
    }
}
