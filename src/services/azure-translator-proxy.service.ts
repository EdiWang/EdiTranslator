import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
    providedIn: 'root'
})

export class AzureTranslatorProxyService {
    constructor(private http: HttpClient) { }

    apiEndpoint: string = 'https://edi-translator-api.azurewebsites.net';

    translate(request: TranslationRequest) {
        let url = `${this.apiEndpoint}/api/translation/translate`;
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