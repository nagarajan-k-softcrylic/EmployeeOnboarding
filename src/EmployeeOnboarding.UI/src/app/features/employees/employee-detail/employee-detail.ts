import { Component, ElementRef, ViewChild, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { EmployeeService } from '../../../core/services/employee.service';
import { Employee } from '../../../core/models/employee.model';

@Component({
  selector: 'app-employee-detail',
  imports: [CommonModule, RouterLink, MatCardModule, MatButtonModule, MatIconModule],
  templateUrl: './employee-detail.html',
  styleUrl: './employee-detail.scss'
})
export class EmployeeDetail {
  private readonly employeeService = inject(EmployeeService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly employee = signal<Employee | null>(null);
  protected readonly uploading = signal(false);

  @ViewChild('fileInput') private readonly fileInput!: ElementRef<HTMLInputElement>;

  private readonly employeeId = Number(this.route.snapshot.paramMap.get('id'));

  constructor() {
    this.load();
  }

  private load(): void {
    this.employeeService.getById(this.employeeId).subscribe((employee) => this.employee.set(employee));
  }

  triggerUpload(): void {
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    this.uploading.set(true);
    this.employeeService.uploadResume(this.employeeId, file).subscribe({
      next: () => {
        this.snackBar.open('Resume uploaded successfully.', 'Close', { duration: 3000 });
        this.uploading.set(false);
        this.load();
      },
      error: () => this.uploading.set(false)
    });
  }

  downloadResume(): void {
    this.employeeService.downloadResume(this.employeeId).subscribe((blob) => {
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `resume-${this.employee()?.employeeCode ?? this.employeeId}.pdf`;
      link.click();
      window.URL.revokeObjectURL(url);
    });
  }

  downloadWelcomeLetter(): void {
    this.employeeService.downloadWelcomeLetter(this.employeeId).subscribe((blob) => {
      const url = window.URL.createObjectURL(blob);
      window.open(url, '_blank');
      setTimeout(() => window.URL.revokeObjectURL(url), 60_000);
    });
  }

  goBack(): void {
    this.router.navigate(['/employees']);
  }
}
