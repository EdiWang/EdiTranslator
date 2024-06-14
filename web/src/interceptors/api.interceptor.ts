import { HttpInterceptor } from "@angular/common/http";

export class TranslatorApiInterceptor implements HttpInterceptor {
    constructor() { }
    intercept(request: any, next: any) {
        const clientName = 'edi-translator-web';
        request = request.clone({
            setHeaders: {
                'x-client-name': clientName
            }
        });
        return next.handle(request);
    }
}
