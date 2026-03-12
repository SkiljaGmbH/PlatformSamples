import { HttpErrorResponse, HttpEvent, HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { map, switchMap, take, tap } from 'rxjs/operators';
import { SnackTypes } from '../models/snack-bar.model';
import { AuthService } from '../services/auth.service';
import { SnackBarService } from '../services/snack-bar.service';


export const urlCleanupInterceptor: HttpInterceptorFn = (req, next) => {
    const removeDoubleSlashes = (url: string): string => {
        return url.replace(/([^:]\/)\/+/g, '$1');
    };

    const dupReq = req.clone({ url: removeDoubleSlashes(req.url) });
    return next(dupReq);
};


export const jwtInterceptor: HttpInterceptorFn = (req, next) => {

    if (req.headers.has('skipAuth')) {
        return next(req);
    }

    const auth = inject(AuthService);
    const snack = inject(SnackBarService);

    const handleError = (err: any) => {
        if (err instanceof HttpErrorResponse) {
            if (err.error) {
                snack.show({
                    type: SnackTypes.ERROR,
                    message: err.error.error_description || err.message || err.name
                });
            }
            if (err.status === 401) {
                auth.logout();
            }
        }
    };

    return auth.getAuthorizationToken().pipe(
        take(1),
        map((token) => {
            if (token) {
                return req.clone({
                    setHeaders: { Authorization: 'bearer ' + token }
                });
            }
            return req;
        }),
        switchMap((clonedReq) => next(clonedReq).pipe(
            tap({
                next: (event: HttpEvent<any>) => {
                    if (event instanceof HttpResponse) {

                    }
                },
                error: (err: any) => handleError(err)
            })
        ))
    );
};