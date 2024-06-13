import { Injectable } from '@angular/core';
import { LanguageChoice } from "../app/models";

export interface TranslationHistory {
    Id: number;
    SourceLanguage: LanguageChoice;
    TargetLanguage: LanguageChoice;
    SourceText: string;
    TranslatedText: string;
    Date: Date;
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

    saveTranslation(sourceLanguage: LanguageChoice, targetLanguage: LanguageChoice, sourceText: string, translatedText: string): void {
        const history = this.getHistory();
        const newTranslation: TranslationHistory = {
            Id: this.generateNewId(history),
            SourceLanguage: sourceLanguage,
            TargetLanguage: targetLanguage,
            SourceText: sourceText,
            TranslatedText: translatedText,
            Date: new Date()
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
}