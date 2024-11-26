import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';



import {
  provideFluentDesignSystem,
  fluentCard,
  fluentButton,
  fluentTextField,
  fluentTextArea,
  fluentSelect,
  fluentOption,
  fluentProgress
} from '@fluentui/web-components';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { TranslatorApiInterceptor } from './interceptors/api.interceptor';
import { RetryInterceptor } from './interceptors/retry-http-errors.interceptor';
import { BrowserModule, bootstrapApplication } from '@angular/platform-browser';
import { AppRoutingModule } from './app/app-routing.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ClipboardModule } from '@angular/cdk/clipboard';
import { AppComponent } from './app/app.component';
import { importProvidersFrom } from '@angular/core';

provideFluentDesignSystem().register(
  fluentCard(),
  fluentButton(),
  fluentTextField(),
  fluentTextArea(),
  fluentSelect(),
  fluentOption(),
  fluentProgress()
);

bootstrapApplication(AppComponent, {
    providers: [
        importProvidersFrom(BrowserModule, AppRoutingModule, FormsModule, ReactiveFormsModule, ClipboardModule),
        { provide: HTTP_INTERCEPTORS, useClass: TranslatorApiInterceptor, multi: true },
        { provide: HTTP_INTERCEPTORS, useClass: RetryInterceptor, multi: true },
        provideHttpClient(withInterceptorsFromDi())
    ]
})
  .catch(err => console.error(err));
