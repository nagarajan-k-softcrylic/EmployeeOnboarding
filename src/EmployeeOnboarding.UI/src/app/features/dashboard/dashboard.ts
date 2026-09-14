import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import Chart from 'chart.js/auto';
import { EmployeeService } from '../../core/services/employee.service';
import { Employee } from '../../core/models/employee.model';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, MatCardModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements AfterViewInit, OnDestroy {
  private readonly employeeService = inject(EmployeeService);
  private chart?: Chart;

  @ViewChild('departmentChart') private readonly chartRef!: ElementRef<HTMLCanvasElement>;

  protected readonly totalEmployees = signal(0);
  protected readonly recentJoiners = signal(0);
  protected readonly departments = signal(0);

  ngAfterViewInit(): void {
    this.employeeService.getAll().subscribe({
      next: (employees) => this.buildDashboard(employees),
      error: () => this.buildDashboard([])
    });
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }

  private buildDashboard(employees: Employee[]): void {
    this.totalEmployees.set(employees.length);

    const thirtyDaysAgo = new Date();
    thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);
    this.recentJoiners.set(
      employees.filter((e) => new Date(e.dateOfJoining) >= thirtyDaysAgo).length
    );

    const departmentCounts = new Map<string, number>();
    for (const employee of employees) {
      departmentCounts.set(employee.department, (departmentCounts.get(employee.department) ?? 0) + 1);
    }
    this.departments.set(departmentCounts.size);

    if (this.chartRef?.nativeElement) {
      this.chart = new Chart(this.chartRef.nativeElement, {
        type: 'bar',
        data: {
          labels: Array.from(departmentCounts.keys()),
          datasets: [
            {
              label: 'Employees by Department',
              data: Array.from(departmentCounts.values()),
              backgroundColor: '#3f51b5'
            }
          ]
        },
        options: {
          responsive: true,
          plugins: { legend: { display: false } },
          scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } }
        }
      });
    }
  }
}
