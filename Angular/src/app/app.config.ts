import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { providePrimeNG } from 'primeng/config';
import { MessageService } from 'primeng/api';
import { provideHttpClient } from '@angular/common/http';
import Aura from '@primeng/themes/aura';
import Material from '@primeng/themes/material';
import Lara from '@primeng/themes/lara'

import { routes } from './app.routes';
import MyPreset from '../theme';

export const appConfig: ApplicationConfig = {
  providers: [
    MessageService,
    provideHttpClient(),
    provideZoneChangeDetection({ eventCoalescing: true }), 
    provideRouter(routes),
    provideAnimationsAsync(),
    providePrimeNG({
        theme: {
            preset: MyPreset
        }
    })]
};
