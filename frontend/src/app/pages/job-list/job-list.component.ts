import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { JobService } from '../../services/job.service';
import { SignalrService } from '../../services/signalr.service';
import { Job } from '../../models/job.model';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-job-list',
  standalone: true,
  imports: [CommonModule, RouterLink, StatusBadgeComponent],
  templateUrl: './job-list.component.html',
  styleUrl: './job-list.component.scss'
})
export class JobListComponent implements OnInit, OnDestroy {
  private readonly jobService = inject(JobService);
  private readonly signalr = inject(SignalrService);
  private subs = new Subscription();

  jobs: Job[] = [];
  isLoading = true;
  error = '';

  ngOnInit(): void {
    this.loadJobs();

    this.subs.add(
      this.signalr.jobCreated$.subscribe(job => {
        this.jobs = [job, ...this.jobs];
      })
    );

    this.subs.add(
      this.signalr.jobUpdated$.subscribe(updated => {
        this.jobs = this.jobs.map(j => j.id === updated.id ? updated : j);
      })
    );
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  loadJobs(): void {
    this.isLoading = true;
    this.error = '';
    this.jobService.getJobs().subscribe({
      next: jobs => {
        this.jobs = jobs;
        this.isLoading = false;
      },
      error: () => {
        this.error = 'Failed to load jobs. Please try again.';
        this.isLoading = false;
      }
    });
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleString();
  }

  trackById(_: number, job: Job): string {
    return job.id;
  }
}