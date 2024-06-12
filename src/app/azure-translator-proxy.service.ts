import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
    providedIn: 'root'
})

export class AzureTranslatorProxyService {
    constructor(private http: HttpClient) { }

    translate(request: TranslationRequest) {
        let url = 'https://translator.ediwang.dev/api/Translation/translate';
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