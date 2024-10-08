import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';

import {
  provideFASTDesignSystem,
  fastCard,
  fastButton,
  fastTextField,
  fastTextArea,
  fastSelect,
  fastOption,
  fastProgress
} from '@microsoft/fast-components';

provideFASTDesignSystem().register(
  fastCard(),
  fastButton(),
  fastTextField(),
  fastTextArea(),
  fastSelect(),
  fastOption(),
  fastProgress()
);

platformBrowserDynamic().bootstrapModule(AppModule, {
  ngZoneEventCoalescing: true
})
  .catch(err => console.error(err));
