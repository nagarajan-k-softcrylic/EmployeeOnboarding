import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private readonly _isLoading = signal<boolean>(false);
  private requestCount = 0;
  readonly isLoading = this._isLoading.asReadonly();

  show(): void {
    this.requestCount++;
    this._isLoading.set(true);
  }

  hide(): void {
    this.requestCount = Math.max(0, this.requestCount - 1);
    if (this.requestCount === 0) {
      this._isLoading.set(false);
    }
  }
}
