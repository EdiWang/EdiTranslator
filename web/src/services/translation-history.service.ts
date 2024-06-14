import { Injectable } from '@angular/core';
import { ApiProvider, LanguageChoice } from "../app/models";

export interface TranslationHistory {
    Id: number;
    SourceLanguage: LanguageChoice;
    TargetLanguage: LanguageChoice;
    SourceText: string;
    TranslatedText: string;
    Date: Date;
    Provider?: ApiProvider;
}

@Injectable({
    providedIn: 'root'
})
export class TranslationHistoryService {
    private storageKey = 'translationHistory';

    constructor() { }

    private getHistory(): TranslationHistory[] {
        const historyJson = localStorage.getItem(this.storageKey);
        return historyJson ? JSON.parse(historyJson) : [];
    }

    private saveHistory(history: TranslationHistory[]): void {
        localStorage.setItem(this.storageKey, JSON.stringify(history));
    }

    private generateNewId(history: TranslationHistory[]): number {
        return history.length > 0 ? Math.max(...history.map(h => h.Id)) + 1 : 1;
    }

    saveTranslation(
        sourceLanguage: LanguageChoice, targetLanguage: LanguageChoice,
        sourceText: string, translatedText: string,
        provider: ApiProvider): void {
        const history = this.getHistory();
        const newTranslation: TranslationHistory = {
            Id: this.generateNewId(history),
            SourceLanguage: sourceLanguage,
            TargetLanguage: targetLanguage,
            SourceText: sourceText,
            TranslatedText: translatedText,
            Date: new Date(),
            Provider: provider
        };
        history.push(newTranslation);
        this.saveHistory(history);
    }

    getTranslationById(id: number): TranslationHistory | undefined {
        const history = this.getHistory();
        return history.find(h => h.Id === id);
    }

    listAllTranslations(): TranslationHistory[] {
        return this.getHistory();
    }

    clearAllTranslations(): void {
        localStorage.removeItem(this.storageKey);
    }

    deleteTranslationById(id: number): void {
        let history = this.getHistory();
        history = history.filter(h => h.Id !== id);
        this.saveHistory(history);
    }
}