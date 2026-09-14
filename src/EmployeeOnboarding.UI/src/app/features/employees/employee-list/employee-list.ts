import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { EmployeeService } from '../../../core/services/employee.service';
import { Employee } from '../../../core/models/employee.model';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-employee-list',
  imports: [CommonModule, RouterLink, MatTableModule, MatButtonModule, MatIconModule, MatCardModule],
  templateUrl: './employee-list.html',
  styleUrl: './employee-list.scss'
})
export class EmployeeList {
  private readonly employeeService = inject(EmployeeService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  protected readonly employees = signal<Employee[]>([]);
  protected readonly displayedColumns = ['employeeCode', 'name', 'email', 'department', 'designation', 'dateOfJoining', 'actions'];

  constructor() {
    this.load();
  }

  private load(): void {
    this.employeeService.getAll().subscribe({
      next: (data) => this.employees.set(data)
    });
  }

  delete(employee: Employee): void {
    const dialogRef = this.dialog.open(ConfirmDialog, {
      data: { message: `Delete employee "${employee.name}"?` }
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }
      this.employeeService.delete(employee.id).subscribe({
        next: () => {
          this.snackBar.open('Employee deleted successfully.', 'Close', { duration: 3000 });
          this.load();
        }
      });
    });
  }
}
