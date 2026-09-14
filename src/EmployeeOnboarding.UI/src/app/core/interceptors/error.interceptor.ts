import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);
  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let message = 'An unexpected error occurred.';

      if (error.status === 401) {
        message = 'Session expired. Please log in again.';
        authService.logout();
        router.navigate(['/login']);
      } else if (error.status === 0) {
        message = 'Unable to reach the server. Please try again later.';
      } else if (error.error?.message) {
        message = error.error.message;
      } else if (error.error?.errors) {
        message = Object.values(error.error.errors).flat().join(' ');
      } else if (error.status === 404) {
        message = 'The requested resource was not found.';
      }

      snackBar.open(message, 'Close', { duration: 5000, panelClass: 'error-snackbar' });
      return throwError(() => error);
    })
  );
};
