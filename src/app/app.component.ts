import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LanguageChoice } from './models';
import { AzureTranslatorProxyService } from '../services/azure-translator-proxy.service';
import { Clipboard } from '@angular/cdk/clipboard';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  sourceForm: FormGroup = new FormGroup({});
  languageList: LanguageChoice[] = [
    { Code: 'zh-Hans', Name: '简体中文 (Simplified Chinese)' },
    { Code: 'en', Name: 'English' },
    { Code: 'ja', Name: '日本語 (Japanese)' }
  ];

  isBusy: boolean = false;
  translatedText = '';
  errorMessage = '';

  constructor(
    private formBuilder: FormBuilder,
    private azureTranslatorProxyService: AzureTranslatorProxyService,
    private clipboard: Clipboard) {
  }

  ngOnInit(): void {
    this.buildForm();
  }

  buildForm() {
    this.sourceForm = this.formBuilder.group({
      sourceText: ['', [Validators.required]],
      sourceLanguage: [this.languageList[0].Code, [Validators.required]],
      targetLanguage: [this.languageList[1].Code, [Validators.required]]
    })
  }

  onTranslate() {
    this.isBusy = true;
    this.errorMessage = '';
    
    this.azureTranslatorProxyService.translate({
      Content: this.sourceForm?.controls['sourceText']?.value,
      FromLang: this.sourceForm?.controls['sourceLanguage']?.value,
      ToLang: this.sourceForm?.controls['targetLanguage']?.value
    }).subscribe(
      {
        next: (response: any) => {
          this.translatedText = response[0].translations[0].text;
          this.isBusy = false;
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
}
