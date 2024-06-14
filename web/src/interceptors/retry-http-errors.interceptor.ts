import { Injectable } from '@angular/core';
import {
    HttpRequest,
    HttpHandler,
    HttpEvent,
    HttpInterceptor,
    HttpErrorResponse,
} from '@angular/common/http';
import { Observable, retry, timer } from 'rxjs';

@Injectable()
export class RetryInterceptor implements HttpInterceptor {
    constructor() { }

    intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
        return next.handle(request).pipe(retry({ count: 2, delay: this.shouldRetry }));
    }

    shouldRetry(error: HttpErrorResponse) {
        if (error.status >= 500) {
            console.log('Retrying request in 1 second');
            return timer(1000);
        }
        throw error;
    }
}
