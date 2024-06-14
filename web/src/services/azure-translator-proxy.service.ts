import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
    providedIn: 'root'
})

export class AzureTranslatorProxyService {
    constructor(private http: HttpClient) { }

    translate(request: TranslationRequest, provider: string) {
        let url = `/api/translation/translate`;

        if (provider === 'aoai-gpt4o') {
            url += `/oai`;
        }

        return this.http.post(url, request);
    }
}

export interface TranslationRequest {
    Content: string;
    FromLang: string;
    ToLang: string;
}

// export interface TranslationResponse {
//     Translations: TranslationResult[];
// }

// export interface TranslationResult {
//     Text: string;
//     To: string;
// }