import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiProvider, LanguageChoice } from './models';
import { AzureTranslatorProxyService } from '../services/azure-translator-proxy.service';
import { Clipboard } from '@angular/cdk/clipboard';
import { TranslationHistory } from '../services/translation-history.service';
import { TranslationHistoryComponent } from './translation-history/translation-history.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  sourceForm: FormGroup = new FormGroup({});
  @ViewChild("translationHistory") translationHistoryComponent!: TranslationHistoryComponent;

  languageList: LanguageChoice[] = [
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

  providerList: ApiProvider[] = [
    { Name: 'Azure Translator (Text)', ApiRoute: 'azure-translator' },
    { Name: 'Azure Open AI (GPT-4o)', ApiRoute: 'aoai/gpt-4o' },
    { Name: 'Azure Open AI (GPT-3.5 Turbo)', ApiRoute: 'aoai/gpt-35-turbo' }
  ]

  maxTextLength: number = 5000;
  isBusy: boolean = false;
  translatedText = '';
  errorMessage = '';
  translations: TranslationHistory[] = [];

  constructor(
    private formBuilder: FormBuilder,
    private azureTranslatorProxyService: AzureTranslatorProxyService,
    private clipboard: Clipboard) {
  }

  ngOnInit(): void {
    this.buildForm();
    this.loadUserSelection();
  }

  buildForm() {
    this.sourceForm = this.formBuilder.group({
      sourceText: ['', [Validators.required]],
      sourceLanguage: [this.languageList[0].Code],
      targetLanguage: [this.languageList[3].Code, [Validators.required]],
      apiProvider: [this.providerList[0].ApiRoute, [Validators.required]]
    })
  }

  handleKeyDown(event: KeyboardEvent) {
    if (event.key === 'Enter') {
      if (event.ctrlKey) {
        this.onTranslate();
        return;
      }
    }
  }

  onTranslate() {
    this.isBusy = true;
    this.errorMessage = '';

    this.saveUserSelection();

    this.azureTranslatorProxyService.translate(
      {
        Content: this.sourceForm?.controls['sourceText']?.value,
        FromLang: this.sourceForm?.controls['sourceLanguage']?.value,
        ToLang: this.sourceForm?.controls['targetLanguage']?.value
      },
      this.sourceForm?.controls['apiProvider']?.value
    ).subscribe(
      {
        next: (response: any) => {
          this.translatedText = response.translatedText;
          this.isBusy = false;

          this.translationHistoryComponent.saveTranslation(
            this.languageList.find(l => l.Code === this.sourceForm.controls['sourceLanguage'].value)!,
            this.languageList.find(l => l.Code === this.sourceForm.controls['targetLanguage'].value)!,
            this.sourceForm.controls['sourceText'].value,
            this.translatedText,
            this.providerList.find(p => p.ApiRoute === this.sourceForm.controls['apiProvider'].value)!
          );

          this.translationHistoryComponent.loadTranslations();
        },
        error: (error) => {
          console.error(error);
          this.errorMessage = 'An error occurred while translating the text.';
          this.isBusy = false;
        }
      }
    );
  }

  saveUserSelection() {
    localStorage.setItem('sourceLanguage', this.sourceForm.controls['sourceLanguage'].value);
    localStorage.setItem('targetLanguage', this.sourceForm.controls['targetLanguage'].value);
    localStorage.setItem('apiProvider', this.sourceForm.controls['apiProvider'].value);
  }

  loadUserSelection() {
    const sourceLanguage = localStorage.getItem('sourceLanguage');
    const targetLanguage = localStorage.getItem('targetLanguage');
    const apiProvider = localStorage.getItem('apiProvider');

    if (sourceLanguage && targetLanguage) {
      this.sourceForm.controls['sourceLanguage'].setValue(sourceLanguage);
      this.sourceForm.controls['targetLanguage'].setValue(targetLanguage);
      this.sourceForm.controls['apiProvider'].setValue(apiProvider);
    }
  }

  swapLanguageSelector() {
    const sourceLanguage = this.sourceForm.controls['sourceLanguage'].value;
    const targetLanguage = this.sourceForm.controls['targetLanguage'].value;
    this.sourceForm.controls['sourceLanguage'].setValue(targetLanguage);
    this.sourceForm.controls['targetLanguage'].setValue(sourceLanguage);
  }

  clear() {
    this.sourceForm.controls['sourceText'].setValue('');
    this.translatedText = '';
  }

  copyTranslatedText() {
    this.clipboard.copy(this.translatedText);
  }

  loadTranslationToForm(translation: TranslationHistory): void {
    this.sourceForm.controls['sourceLanguage'].setValue(translation.SourceLanguage.Code);
    this.sourceForm.controls['targetLanguage'].setValue(translation.TargetLanguage.Code);
    this.sourceForm.controls['sourceText'].setValue(translation.SourceText);
    this.sourceForm.controls['apiProvider'].setValue(translation.Provider.ApiRoute);

    this.translatedText = translation.TranslatedText;
  }
}
