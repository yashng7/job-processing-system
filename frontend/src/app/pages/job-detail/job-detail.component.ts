import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { JobService } from '../../services/job.service';
import { SignalrService } from '../../services/signalr.service';
import { Job } from '../../models/job.model';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-job-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, StatusBadgeComponent],
  templateUrl: './job-detail.component.html',
  styleUrl: './job-detail.component.scss'
})
export class JobDetailComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly jobService = inject(JobService);
  private readonly signalr = inject(SignalrService);
  private subs = new Subscription();

  job: Job | null = null;
  isLoading = true;
  isRetrying = false;
  error = '';
  successMessage = '';

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/jobs']);
      return;
    }

    this.loadJob(id);

    this.subs.add(
      this.signalr.jobUpdated$.subscribe(updated => {
        if (this.job && updated.id === this.job.id) {
          this.job = updated;
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  loadJob(id: string): void {
    this.isLoading = true;
    this.jobService.getJob(id).subscribe({
      next: job => {
        this.job = job;
        this.isLoading = false;
      },
      error: () => {
        this.error = 'Job not found.';
        this.isLoading = false;
      }
    });
  }

  retryJob(): void {
    if (!this.job) return;
    this.isRetrying = true;
    this.successMessage = '';
    this.error = '';

    this.jobService.retryJob(this.job.id).subscribe({
      next: res => {
        this.successMessage = res.message;
        this.isRetrying = false;
        if (this.job) {
          this.job = { ...this.job, status: 'Queued', retryCount: this.job.retryCount };
        }
      },
      error: () => {
        this.error = 'Failed to retry job.';
        this.isRetrying = false;
      }
    });
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleString();
  }

  formatJson(str: string): string {
    try {
      return JSON.stringify(JSON.parse(str), null, 2);
    } catch {
      return str;
    }
  }
}