import { Clipboard } from '@angular/cdk/clipboard';

import { Component, CUSTOM_ELEMENTS_SCHEMA, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, takeUntil, timer, switchMap, catchError, EMPTY } from 'rxjs';
import { AzureTranslatorProxyService } from '../services/azure-translator-proxy.service';
import { KeepAliveService } from '../services/keep-alive-service';
import { TranslationHistory } from '../services/translation-history.service';
import { ApiProvider, LanguageChoice, TranslationRequest } from './models';
import { TranslationHistoryComponent } from './translation-history/translation-history.component';

interface FormControls {
  sourceText: string;
  sourceLanguage: string;
  targetLanguage: string;
  apiProvider: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  imports: [FormsModule, ReactiveFormsModule, TranslationHistoryComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class AppComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private readonly formBuilder = inject(FormBuilder);
  private readonly keepAliveService = inject(KeepAliveService);
  private readonly azureTranslatorProxyService = inject(AzureTranslatorProxyService);
  private readonly clipboard = inject(Clipboard);

  // Constants
  private static readonly STORAGE_KEYS = {
    SOURCE_LANGUAGE: 'sourceLanguage',
    TARGET_LANGUAGE: 'targetLanguage',
    API_PROVIDER: 'apiProvider'
  } as const;

  private static readonly KEEP_ALIVE_INTERVAL = 60 * 1000; // 1 minute
  private static readonly DEFAULT_LANGUAGE_INDICES = {
    SOURCE: 0,
    TARGET: 3
  } as const;

  readonly maxTextLength = 5000;
  readonly languageList: readonly LanguageChoice[] = [
    { Code: 'auto-detect', Name: 'Auto-Detect' },
    { Code: 'zh-Hans', Name: '简体中文 (Simplified Chinese)' },
    { Code: 'zh-Hant', Name: '繁體中文 (Traditional Chinese)' },
    { Code: 'en-US', Name: 'English (United States)' },
    { Code: 'ar', Name: 'العربية (Arabic)' },
    { Code: 'de', Name: 'Deutsch (German)' },
    { Code: 'es', Name: 'Español (Spanish)' },
    { Code: 'fr', Name: 'Français (French)' },
    { Code: 'hi', Name: 'हिन्दी (Hindi)' },
    { Code: 'id', Name: 'Bahasa Indonesia (Indonesian)' },
    { Code: 'it', Name: 'Italiano (Italian)' },
    { Code: 'ja', Name: '日本語 (Japanese)' },
    { Code: 'ko', Name: '한국어 (Korean)' },
    { Code: 'nl', Name: 'Nederlands (Dutch)' },
    { Code: 'pl', Name: 'Polski (Polish)' },
    { Code: 'pt', Name: 'Português (Portuguese)' },
    { Code: 'ru', Name: 'Русский (Russian)' },
    { Code: 'th', Name: 'ไทย (Thai)' },
    { Code: 'tr', Name: 'Türkçe (Turkish)' },
    { Code: 'vi', Name: 'Tiếng Việt (Vietnamese)' }
  ];

  readonly providerList: readonly ApiProvider[] = [
    { Name: 'Azure Translator', ApiRoute: 'azure-translator' },
    { Name: 'GPT-4.1 (Azure)', ApiRoute: 'aoai/gpt-4.1' },
    { Name: 'GPT-4.1-mini (Azure)', ApiRoute: 'aoai/gpt-4.1-mini' },
    { Name: 'GPT-5-mini (Azure)', ApiRoute: 'aoai/gpt-5-mini' },
  ];

  sourceForm!: FormGroup;
  @ViewChild("translationHistory") translationHistoryComponent!: TranslationHistoryComponent;

  isBusy = false;
  translatedText = '';
  errorMessage = '';
  translations: TranslationHistory[] = [];

  ngOnInit(): void {
    this.initializeComponent();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeComponent(): void {
    this.buildForm();
    this.loadUserSelection();
    this.startKeepAlive();
  }

  private buildForm(): void {
    this.sourceForm = this.formBuilder.group({
      sourceText: ['', [Validators.required]],
      sourceLanguage: [this.languageList[AppComponent.DEFAULT_LANGUAGE_INDICES.SOURCE].Code],
      targetLanguage: [this.languageList[AppComponent.DEFAULT_LANGUAGE_INDICES.TARGET].Code, [Validators.required]],
      apiProvider: [this.providerList[0].ApiRoute, [Validators.required]]
    });
  }

  private startKeepAlive(): void {
    timer(0, AppComponent.KEEP_ALIVE_INTERVAL)
      .pipe(
        switchMap(() => this.keepAliveService.keepSessionAlive()),
        catchError((error) => {
          console.error('Error keeping session alive', error);
          return EMPTY;
        }),
        takeUntil(this.destroy$)
      )
      .subscribe((response) => {
        console.log('Session kept alive', response);
      });
  }

  handleKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && event.ctrlKey) {
      event.preventDefault();
      this.onTranslate();
    }
  }

  onTranslate(): void {
    if (this.sourceForm.invalid || this.isBusy) {
      return;
    }

    this.isBusy = true;
    this.errorMessage = '';
    this.saveUserSelection();

    const formValue = this.sourceForm.value as FormControls;
    const translationRequest: TranslationRequest = {
      Content: formValue.sourceText,
      FromLang: formValue.sourceLanguage,
      ToLang: formValue.targetLanguage
    };

    this.azureTranslatorProxyService
      .translate(translationRequest, formValue.apiProvider)
      .pipe(
        takeUntil(this.destroy$),
        catchError((error) => {
          console.error('Translation error:', error);
          this.errorMessage = 'An error occurred while translating the text.';
          this.isBusy = false;
          return EMPTY;
        })
      )
      .subscribe((response) => {
        this.handleTranslationSuccess(response, formValue);
      });
  }

  private handleTranslationSuccess(response: any, formValue: FormControls): void {
    this.translatedText = response.translatedText;
    this.isBusy = false;

    const sourceLanguage = this.findLanguageByCode(formValue.sourceLanguage);
    const targetLanguage = this.findLanguageByCode(formValue.targetLanguage);
    const provider = this.findProviderByRoute(formValue.apiProvider);

    if (sourceLanguage && targetLanguage && provider) {
      this.translationHistoryComponent.saveTranslation(
        sourceLanguage,
        targetLanguage,
        formValue.sourceText,
        this.translatedText,
        provider
      );
      this.translationHistoryComponent.loadTranslations();
    }
  }

  private findLanguageByCode(code: string): LanguageChoice | undefined {
    return this.languageList.find(l => l.Code === code);
  }

  private findProviderByRoute(route: string): ApiProvider | undefined {
    return this.providerList.find(p => p.ApiRoute === route);
  }

  private saveUserSelection(): void {
    const formValue = this.sourceForm.value as FormControls;
    
    try {
      localStorage.setItem(AppComponent.STORAGE_KEYS.SOURCE_LANGUAGE, formValue.sourceLanguage);
      localStorage.setItem(AppComponent.STORAGE_KEYS.TARGET_LANGUAGE, formValue.targetLanguage);
      localStorage.setItem(AppComponent.STORAGE_KEYS.API_PROVIDER, formValue.apiProvider);
    } catch (error) {
      console.warn('Failed to save user selection to localStorage:', error);
    }
  }

  private loadUserSelection(): void {
    try {
      const sourceLanguage = localStorage.getItem(AppComponent.STORAGE_KEYS.SOURCE_LANGUAGE);
      const targetLanguage = localStorage.getItem(AppComponent.STORAGE_KEYS.TARGET_LANGUAGE);
      const apiProvider = localStorage.getItem(AppComponent.STORAGE_KEYS.API_PROVIDER);

      if (sourceLanguage && targetLanguage && apiProvider) {
        this.sourceForm.patchValue({
          sourceLanguage,
          targetLanguage,
          apiProvider
        });
      }
    } catch (error) {
      console.warn('Failed to load user selection from localStorage:', error);
    }
  }

  swapLanguageSelector(): void {
    const currentValues = this.sourceForm.value as FormControls;
    
    // Don't swap if source is auto-detect
    if (currentValues.sourceLanguage === 'auto-detect') {
      return;
    }

    this.sourceForm.patchValue({
      sourceLanguage: currentValues.targetLanguage,
      targetLanguage: currentValues.sourceLanguage
    });
  }

  clear(): void {
    this.sourceForm.patchValue({ sourceText: '' });
    this.translatedText = '';
    this.errorMessage = '';
  }

  copyTranslatedText(): void {
    if (this.translatedText) {
      this.clipboard.copy(this.translatedText);
    }
  }

  loadTranslationToForm(translation: TranslationHistory): void {
    this.sourceForm.patchValue({
      sourceLanguage: translation.SourceLanguage.Code,
      targetLanguage: translation.TargetLanguage.Code,
      sourceText: translation.SourceText,
      apiProvider: translation.Provider.ApiRoute
    });

    this.translatedText = translation.TranslatedText;
    this.errorMessage = '';
  }
}
