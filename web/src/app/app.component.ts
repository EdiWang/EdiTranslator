import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiProvider, LanguageChoice } from './models';
import { AzureTranslatorProxyService } from '../services/azure-translator-proxy.service';
import { Clipboard } from '@angular/cdk/clipboard';
import { TranslationHistory, TranslationHistoryService } from '../services/translation-history.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  sourceForm: FormGroup = new FormGroup({});
  languageList: LanguageChoice[] = [
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
    { Code: 'azure-translator', Name: 'Azure Translator (Text)' },
    { Code: 'aoai', Name: 'Azure Open AI (GPT-4o)' }
  ]

  maxTextLength: number = 5000;
  isBusy: boolean = false;
  translatedText = '';
  errorMessage = '';
  translations: TranslationHistory[] = [];

  constructor(
    private formBuilder: FormBuilder,
    private azureTranslatorProxyService: AzureTranslatorProxyService,
    private clipboard: Clipboard,
    private translationHistoryService: TranslationHistoryService) {
  }

  ngOnInit(): void {
    this.buildForm();
    this.loadTranslations();
  }

  buildForm() {
    this.sourceForm = this.formBuilder.group({
      sourceText: ['', [Validators.required]],
      sourceLanguage: [this.languageList[0].Code, [Validators.required]],
      targetLanguage: [this.languageList[2].Code, [Validators.required]],
      provider: [this.providerList[0].Code, [Validators.required]]
    })
  }

  onTranslate() {
    this.isBusy = true;
    this.errorMessage = '';

    this.azureTranslatorProxyService.translate(
      {
        Content: this.sourceForm?.controls['sourceText']?.value,
        FromLang: this.sourceForm?.controls['sourceLanguage']?.value,
        ToLang: this.sourceForm?.controls['targetLanguage']?.value
      },
      this.sourceForm?.controls['provider']?.value
    ).subscribe(
      {
        next: (response: any) => {
          this.translatedText = response.translatedText;
          this.isBusy = false;

          this.saveTranslation(
            this.languageList.find(l => l.Code === this.sourceForm.controls['sourceLanguage'].value)!,
            this.languageList.find(l => l.Code === this.sourceForm.controls['targetLanguage'].value)!,
            this.sourceForm.controls['sourceText'].value,
            this.translatedText,
            this.providerList.find(p => p.Code === this.sourceForm.controls['provider'].value)!
          );

          this.loadTranslations();
        },
        error: (error) => {
          console.error(error);
          this.errorMessage = 'An error occurred while translating the text.';
          this.isBusy = false;
        }
      }
    );
  }

  swapLanguageSelector() {
    const sourceLanguage = this.sourceForm.controls['sourceLanguage'].value;
    const targetLanguage = this.sourceForm.controls['targetLanguage'].value;
    this.sourceForm.controls['sourceLanguage'].setValue(targetLanguage);
    this.sourceForm.controls['targetLanguage'].setValue(sourceLanguage);
  }

  clear() {
    this.sourceForm.reset();
    this.buildForm();
    this.translatedText = '';
  }

  copyTranslatedText() {
    this.clipboard.copy(this.translatedText);
  }

  loadTranslations(): void {
    this.translations = this.translationHistoryService.listAllTranslations()
      .sort((a, b) => b.Id - a.Id); // Sort by Id in descending order
  }

  saveTranslation(sourceLanguage: LanguageChoice, targetLanguage: LanguageChoice, sourceText: string, translatedText: string, provider: ApiProvider) {
    this.translationHistoryService.saveTranslation(sourceLanguage, targetLanguage, sourceText, translatedText, provider);
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
    console.log(`Translation with ID ${id} has been deleted.`);
  }

  loadTranslation(translation: TranslationHistory): void {
    console.log(translation);

    this.sourceForm.controls['sourceLanguage'].setValue(translation.SourceLanguage.Code);
    this.sourceForm.controls['targetLanguage'].setValue(translation.TargetLanguage.Code);
    this.sourceForm.controls['sourceText'].setValue(translation.SourceText);
    this.sourceForm.controls['provider'].setValue(translation.Provider);
    
    this.translatedText = translation.TranslatedText;
  }
}
