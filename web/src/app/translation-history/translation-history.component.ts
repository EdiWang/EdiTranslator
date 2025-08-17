import { CommonModule, DatePipe } from '@angular/common';
import { Component, CUSTOM_ELEMENTS_SCHEMA, EventEmitter, OnInit, Output, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { TranslationHistory, TranslationHistoryService } from '../../services/translation-history.service';
import { ApiProvider, LanguageChoice } from '../models';

@Component({
  selector: 'app-translation-history',
  templateUrl: './translation-history.component.html',
  styleUrl: './translation-history.component.css',
  imports: [CommonModule, DatePipe],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class TranslationHistoryComponent implements OnInit, OnDestroy {
  translations: TranslationHistory[] = [];
  isLoading = false;
  error: string | null = null;
  
  @Output() loadTranslation = new EventEmitter<TranslationHistory>();
  
  private destroy$ = new Subject<void>();

  constructor(private translationHistoryService: TranslationHistoryService) {}

  ngOnInit(): void {
    this.loadTranslations();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onLoadTranslation(item: TranslationHistory): void {
    if (!item) {
      console.warn('Translation item is null or undefined');
      return;
    }
    this.loadTranslation.emit(item);
  }

  loadTranslations(): void {
    try {
      this.isLoading = true;
      this.error = null;
      
      this.translations = this.translationHistoryService
        .listAllTranslations()
        .sort((a, b) => b.Id - a.Id);
    } catch (error) {
      this.error = 'Failed to load translations';
      console.error('Error loading translations:', error);
    } finally {
      this.isLoading = false;
    }
  }

  saveTranslation(
    sourceLanguage: LanguageChoice, 
    targetLanguage: LanguageChoice, 
    sourceText: string, 
    translatedText: string, 
    apiProvider: ApiProvider
  ): void {
    if (!sourceText?.trim() || !translatedText?.trim()) {
      console.warn('Cannot save translation with empty text');
      return;
    }

    try {
      this.translationHistoryService.saveTranslation(
        sourceLanguage, 
        targetLanguage, 
        sourceText, 
        translatedText, 
        apiProvider
      );
      this.loadTranslations(); // Refresh the list after saving
    } catch (error) {
      this.error = 'Failed to save translation';
      console.error('Error saving translation:', error);
    }
  }

  getTranslation(id: number): TranslationHistory | undefined {
    if (!id || id <= 0) {
      console.warn('Invalid translation ID');
      return undefined;
    }

    try {
      const translation = this.translationHistoryService.getTranslationById(id);
      return translation;
    } catch (error) {
      console.error('Error getting translation:', error);
      return undefined;
    }
  }

  clearTranslations(): void {
    if (!confirm('Are you sure you want to clear all translations? This action cannot be undone.')) {
      return;
    }

    try {
      this.translationHistoryService.clearAllTranslations();
      this.loadTranslations();
    } catch (error) {
      this.error = 'Failed to clear translations';
      console.error('Error clearing translations:', error);
    }
  }

  deleteTranslation(id: number): void {
    if (!id || id <= 0) {
      console.warn('Invalid translation ID for deletion');
      return;
    }

    if (!confirm('Are you sure you want to delete this translation?')) {
      return;
    }

    try {
      this.translationHistoryService.deleteTranslationById(id);
      this.loadTranslations();
    } catch (error) {
      this.error = 'Failed to delete translation';
      console.error('Error deleting translation:', error);
    }
  }

  retryLoadTranslations(): void {
    this.loadTranslations();
  }
}
