import { HttpInterceptor } from "@angular/common/http";

export class AuthInterceptor implements HttpInterceptor {
    constructor() { }
    intercept(request: any, next: any) {
        const clientName = 'edi-translator-web';
        request = request.clone({
            setHeaders: {
                'x-client-name': clientName,
                'x-api-key': '9fd6ebbc-7eed-42b4-86a8-fbfe157b8e23'
            }
        });
        return next.handle(request);
    }
}