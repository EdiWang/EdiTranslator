export interface LanguageChoice {
    Code: string;
    Name: string;
}

export interface ApiProvider {
    Name: string;
    ApiRoute: string;
}

export interface TranslationRequest {
    Content: string;
    FromLang: string;
    ToLang: string;
}