import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Job, CreateJobRequest, JobStats } from '../models/job.model';

@Injectable({ providedIn: 'root' })
export class JobService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/jobs';

  createJob(request: CreateJobRequest): Observable<Job> {
    return this.http.post<Job>(this.baseUrl, request);
  }

  getJobs(): Observable<Job[]> {
    return this.http.get<Job[]>(this.baseUrl);
  }

  getJob(id: string): Observable<Job> {
    return this.http.get<Job>(`${this.baseUrl}/${id}`);
  }

  retryJob(id: string): Observable<{ jobId: string; message: string }> {
    return this.http.post<{ jobId: string; message: string }>(`${this.baseUrl}/${id}/retry`, {});
  }

  getStats(jobs: Job[]): JobStats {
    return {
      total: jobs.length,
      queued: jobs.filter(j => j.status === 'Queued').length,
      processing: jobs.filter(j => j.status === 'Processing').length,
      completed: jobs.filter(j => j.status === 'Completed').length,
      failed: jobs.filter(j => j.status === 'Failed').length,
    };
  }
}