import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
    providedIn: 'root'
})

export class AzureTranslatorProxyService {
    constructor(private http: HttpClient) { }

    translate(request: TranslationRequest, route: string) {
        let url = `/api/translation/${route}`;

        if (request.FromLang === 'auto-detect') { 
            request.FromLang = null;
        }

        return this.http.post(url, request);
    }
}

export interface TranslationRequest {
    Content: string;
    FromLang: string | null;
    ToLang: string;
}
