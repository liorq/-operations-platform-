import { DOCUMENT, DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { CalculateResponse, InputType, Operation, OperationKind } from '../models/models';
import { CalculatorService } from '../services/calculator.service';

interface OperationGroup {
  title: string;
  operations: Operation[];
}

const HINTS: Record<InputType, string> = {
  Number: 'הזן מספרים',
  Text: 'הזן טקסט',
  Coordinates: 'א\' = קו רוחב (-90 עד 90), ב\' = קו אורך (-180 עד 180). למשל 32.08 ו-34.78',
};

const GROUPS: { kind: OperationKind; title: string }[] = [
  { kind: 'Expression', title: 'חשבון' },
  { kind: 'Text', title: 'טקסט' },
  { kind: 'Http', title: 'נתונים חיים' },
];

const GLYPHS: Record<string, string> = {
  add: '+',
  subtract: '−',
  multiply: '×',
  divide: '÷',
  power: '^',
  hypotenuse: '√',
  concat: '‖',
  levenshtein: 'Δ',
  anagram: '≟',
  weather: '°C',
};
const DEFAULT_GLYPH = '·';

const ARROW_STEPS: Record<string, number> = { ArrowLeft: 1, ArrowRight: -1 };
const ENTER_KEY = 'Enter';
const MONTH_SEGMENTS = 14;
const DARK_THEME = 'ink';
const UTC_SUFFIX = 'Z';

/** Field A, an operation, field B and the result, with the month usage and the last runs below. */
@Component({
  selector: 'app-calculator',
  imports: [FormsModule, DatePipe],
  templateUrl: './calculator.component.html',
  styleUrl: './calculator.component.css',
  host: { '(document:keydown)': 'onKeydown($event)' },
})
export class CalculatorComponent implements OnInit {
  private readonly service: CalculatorService = inject(CalculatorService);
  private readonly root: HTMLElement = inject(DOCUMENT).documentElement;

  protected readonly operations = signal<Operation[]>([]);
  protected readonly a = signal<string>('');
  protected readonly b = signal<string>('');
  protected readonly selectedKey = signal<string>('');
  protected readonly response = signal<CalculateResponse | null>(null);
  protected readonly errorMessage = signal<string>('');
  protected readonly isBusy = signal<boolean>(false);

  protected readonly expressionKind: OperationKind = 'Expression';

  protected readonly glyph = computed<string>(() => GLYPHS[this.selectedKey()] ?? DEFAULT_GLYPH);

  private readonly selectedIndex = computed<number>(() =>
    this.operations().findIndex((item: Operation): boolean => item.key === this.selectedKey()),
  );

  protected readonly hint = computed<string>(() => {
    const operation: Operation | undefined = this.operations()[this.selectedIndex()];

    return operation ? (operation.description ?? HINTS[operation.inputType]) : '';
  });

  protected readonly groups = computed<OperationGroup[]>(() =>
    GROUPS.map((group): OperationGroup => ({
      title: group.title,
      operations: this.operations().filter((item: Operation): boolean => item.kind === group.kind),
    })).filter((group: OperationGroup): boolean => group.operations.length > 0),
  );

  protected readonly segments = computed<boolean[]>(() => {
    const used: number = this.response()?.countThisMonth ?? 0;

    return Array.from({ length: MONTH_SEGMENTS }, (_: unknown, index: number): boolean => index < used);
  });

  ngOnInit(): void {
    this.service.getOperations().subscribe({
      next: (operations: Operation[]): void => {
        this.operations.set(operations);
        this.selectedKey.set(operations[0]?.key ?? '');
      },
      error: (error: unknown): void => this.errorMessage.set(this.service.getErrorMessage(error)),
    });
  }

  protected calculate(): void {
    if (this.isBusy() || !this.selectedKey()) {
      return;
    }

    this.isBusy.set(true);
    this.errorMessage.set('');

    this.service
      .calculate({ a: this.a(), b: this.b(), operation: this.selectedKey() })
      .pipe(finalize((): void => this.isBusy.set(false)))
      .subscribe({
        next: (response: CalculateResponse): void => this.response.set(response),
        error: (error: unknown): void => {
          this.response.set(null);
          this.errorMessage.set(this.service.getErrorMessage(error));
        },
      });
  }

  protected select(key: string): void {
    this.selectedKey.set(key);
    this.response.set(null);
    this.errorMessage.set('');
  }

  protected toggleTheme(): void {
    this.root.dataset['theme'] = this.root.dataset['theme'] === DARK_THEME ? '' : DARK_THEME;
  }

  protected onKeydown(event: KeyboardEvent): void {
    if (event.key === ENTER_KEY && !(event.target instanceof HTMLButtonElement)) {
      this.calculate();
      return;
    }

    const step: number | undefined = ARROW_STEPS[event.key];

    if (step && !(event.target instanceof HTMLInputElement)) {
      const next: Operation | undefined = this.operations()[this.selectedIndex() + step];

      if (next) {
        this.select(next.key);
      }
    }
  }

  /** The server stores UTC but SQL Server hands it back without a "Z", so one is added before showing local time. */
  protected toDate(value: string): Date {
    return new Date(value.endsWith(UTC_SUFFIX) ? value : value + UTC_SUFFIX);
  }
}
