export type JobStatus = 'Queued' | 'Processing' | 'Completed' | 'Failed';

export interface Job {
  id: string;
  name: string;
  status: JobStatus;
  payload: string;
  result: string | null;
  retryCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateJobRequest {
  name: string;
  payload: string;
}

export interface JobStats {
  total: number;
  queued: number;
  processing: number;
  completed: number;
  failed: number;
}