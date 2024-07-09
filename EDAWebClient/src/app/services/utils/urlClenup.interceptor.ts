import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class UrlCleanupInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const dupReq = req.clone({ url: this.removeDoubleSlashes(req.url) });
    return next.handle(dupReq);
  }

  private removeDoubleSlashes(entryUrl: string): string {
    return entryUrl.replace(/([^:]\/)\/+/g, '$1');
  }
}