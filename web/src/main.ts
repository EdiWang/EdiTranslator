import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';

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

provideFluentDesignSystem().register(
  fluentCard(),
  fluentButton(),
  fluentTextField(),
  fluentTextArea(),
  fluentSelect(),
  fluentOption(),
  fluentProgress()
);

platformBrowserDynamic().bootstrapModule(AppModule, {
  ngZoneEventCoalescing: true
})
  .catch(err => console.error(err));
