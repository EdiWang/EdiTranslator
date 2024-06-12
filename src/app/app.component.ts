import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LanguageChoice } from './models';
import { AzureTranslatorProxyService } from './azure-translator-proxy.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  sourceForm: FormGroup = new FormGroup({});
  languageList: LanguageChoice[] = [
    { Code: 'zh-Hans', Name: '简体中文 (Simplified Chinese)' },
    { Code: 'en', Name: 'English' }
  ];
  isBusy: boolean = false;
  translatedText = '';

  constructor(
    private formBuilder: FormBuilder,
    private azureTranslatorProxyService: AzureTranslatorProxyService) {
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
    console.log(this.sourceForm?.controls['sourceText']?.value);
    console.log(this.sourceForm?.controls['sourceLanguage']?.value);
    console.log(this.sourceForm?.controls['targetLanguage']?.value);

    this.isBusy = true;
    this.azureTranslatorProxyService.translate({
      Content: this.sourceForm?.controls['sourceText']?.value,
      FromLang: this.sourceForm?.controls['sourceLanguage']?.value,
      ToLang: this.sourceForm?.controls['targetLanguage']?.value
    }).subscribe((response: any) => {
      this.translatedText = response[0].translations[0].text;
      this.isBusy = false;
    });
  }

  clear() {
    this.sourceForm.reset();
    this.buildForm();
    this.translatedText = '';
  }

  copyTranslatedText() {

  }
}
