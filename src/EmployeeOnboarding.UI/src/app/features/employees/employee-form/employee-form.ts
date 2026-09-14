import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { EmployeeService } from '../../../core/services/employee.service';

@Component({
  selector: 'app-employee-form',
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './employee-form.html',
  styleUrl: './employee-form.scss'
})
export class EmployeeForm {
  private readonly fb = inject(FormBuilder);
  private readonly employeeService = inject(EmployeeService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly isEditMode = signal(false);
  protected readonly employeeId = signal<number | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    employeeCode: ['', [Validators.required]],
    name: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', [Validators.required]],
    department: ['', [Validators.required]],
    designation: ['', [Validators.required]],
    dateOfJoining: ['', [Validators.required]]
  });

  constructor() {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const id = Number(idParam);
      this.isEditMode.set(true);
      this.employeeId.set(id);
      this.form.controls.employeeCode.disable();

      this.employeeService.getById(id).subscribe((employee) => {
        this.form.patchValue({
          employeeCode: employee.employeeCode,
          name: employee.name,
          email: employee.email,
          phone: employee.phone,
          department: employee.department,
          designation: employee.designation,
          dateOfJoining: employee.dateOfJoining?.substring(0, 10)
        });
      });
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();

    if (this.isEditMode() && this.employeeId()) {
      this.employeeService.update(this.employeeId()!, value).subscribe({
        next: () => {
          this.snackBar.open('Employee updated successfully.', 'Close', { duration: 3000 });
          this.router.navigate(['/employees', this.employeeId()]);
        }
      });
    } else {
      this.employeeService.create(value).subscribe({
        next: (created) => {
          this.snackBar.open('Employee created successfully.', 'Close', { duration: 3000 });
          this.router.navigate(['/employees', created.id]);
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/employees']);
  }
}
