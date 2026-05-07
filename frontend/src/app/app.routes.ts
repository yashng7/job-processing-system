import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'jobs',
    loadComponent: () =>
      import('./pages/job-list/job-list.component').then(m => m.JobListComponent)
  },
  {
    path: 'jobs/create',
    loadComponent: () =>
      import('./pages/create-job/create-job.component').then(m => m.CreateJobComponent)
  },
  {
    path: 'jobs/:id',
    loadComponent: () =>
      import('./pages/job-detail/job-detail.component').then(m => m.JobDetailComponent)
  },
  { path: '**', redirectTo: 'dashboard' }
];