import { provideHttpClient } from '@angular/common/http';
import { bootstrapApplication } from '@angular/platform-browser';
import { CalculatorComponent } from './app/calculator/calculator.component';

bootstrapApplication(CalculatorComponent, { providers: [provideHttpClient()] });
