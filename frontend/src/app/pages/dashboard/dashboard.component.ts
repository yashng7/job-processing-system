import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { JobService } from '../../services/job.service';
import { SignalrService } from '../../services/signalr.service';
import { Job, JobStats } from '../../models/job.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit, OnDestroy {
  private readonly jobService = inject(JobService);
  private readonly signalr = inject(SignalrService);
  private subs = new Subscription();

  jobs: Job[] = [];
  stats: JobStats = { total: 0, queued: 0, processing: 0, completed: 0, failed: 0 };
  isLoading = true;

  ngOnInit(): void {
    this.loadJobs();

    this.subs.add(
      this.signalr.jobCreated$.subscribe(job => {
        this.jobs = [job, ...this.jobs];
        this.updateStats();
      })
    );

    this.subs.add(
      this.signalr.jobUpdated$.subscribe(updated => {
        this.jobs = this.jobs.map(j => j.id === updated.id ? updated : j);
        this.updateStats();
      })
    );
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  loadJobs(): void {
    this.isLoading = true;
    this.jobService.getJobs().subscribe({
      next: jobs => {
        this.jobs = jobs;
        this.updateStats();
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }

  private updateStats(): void {
    this.stats = this.jobService.getStats(this.jobs);
  }

  get recentJobs(): Job[] {
    return this.jobs.slice(0, 5);
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleString();
  }
}