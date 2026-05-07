import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { JobService } from '../../services/job.service';

@Component({
  selector: 'app-create-job',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './create-job.component.html',
  styleUrl: './create-job.component.scss'
})
export class CreateJobComponent {
  private readonly fb = inject(FormBuilder);
  private readonly jobService = inject(JobService);
  private readonly router = inject(Router);

  isSubmitting = false;
  error = '';

  form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(256)]],
    payload: ['{}', [Validators.required]]
  });

  get nameControl() { return this.form.controls.name; }
  get payloadControl() { return this.form.controls.payload; }

  readonly examplePayloads = [
    { label: 'Email', value: '{"type": "email", "to": "user@example.com", "subject": "Hello"}' },
    { label: 'Report', value: '{"type": "report", "reportId": "monthly-2024", "format": "pdf"}' },
    { label: 'Data Sync', value: '{"type": "sync", "source": "database", "target": "warehouse"}' },
  ];

  useExample(payload: string): void {
    this.payloadControl.setValue(payload);
    this.formatPayload();
  }

  formatPayload(): void {
    const val = this.payloadControl.value;
    if (!val) return;
    try {
      const formatted = JSON.stringify(JSON.parse(val), null, 2);
      this.payloadControl.setValue(formatted);
    } catch { }
  }

  isPayloadValid(): boolean {
    const val = this.payloadControl.value;
    if (!val) return false;
    try {
      JSON.parse(val);
      return true;
    } catch {
      return false;
    }
  }

  submit(): void {
    if (this.form.invalid || !this.isPayloadValid()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.error = '';

    const { name, payload } = this.form.value;

    this.jobService.createJob({ name: name!, payload: payload! }).subscribe({
      next: job => {
        this.router.navigate(['/jobs', job.id]);
      },
      error: () => {
        this.error = 'Failed to create job. Please try again.';
        this.isSubmitting = false;
      }
    });
  }
}