import { CommonModule, DatePipe } from '@angular/common';
import { Component, CUSTOM_ELEMENTS_SCHEMA, EventEmitter, OnInit, Output } from '@angular/core';
import { TranslationHistory, TranslationHistoryService } from '../../services/translation-history.service';
import { ApiProvider, LanguageChoice } from '../models';

@Component({
  selector: 'app-translation-history',
  templateUrl: './translation-history.component.html',
  styleUrl: './translation-history.component.css',
  imports: [CommonModule, DatePipe],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class TranslationHistoryComponent implements OnInit {
  translations: TranslationHistory[] = [];
  @Output() loadTranslation: EventEmitter<any> = new EventEmitter();

  constructor(private translationHistoryService: TranslationHistoryService) {
  }

  ngOnInit(): void {
    this.loadTranslations();
  }

  onloadTranslation(item: TranslationHistory) {
    this.loadTranslation.emit(item);
  }

  loadTranslations(): void {
    this.translations = this.translationHistoryService.listAllTranslations()
      .sort((a, b) => b.Id - a.Id); // Sort by Id in descending order
  }

  saveTranslation(sourceLanguage: LanguageChoice, targetLanguage: LanguageChoice, sourceText: string, translatedText: string, apiProvider: ApiProvider) {
    this.translationHistoryService.saveTranslation(sourceLanguage, targetLanguage, sourceText, translatedText, apiProvider);
  }

  getTranslation(id: number) {
    const translation = this.translationHistoryService.getTranslationById(id);
    console.log(translation);
  }

  clearTranslations() {
    this.translationHistoryService.clearAllTranslations();
    this.loadTranslations();
  }

  deleteTranslation(id: number): void {
    this.translationHistoryService.deleteTranslationById(id);
    this.loadTranslations();
  }
}
