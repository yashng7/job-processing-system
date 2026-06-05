import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'jobs',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/job-list/job-list.component').then(m => m.JobListComponent)
  },
  {
    path: 'jobs/create',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/create-job/create-job.component').then(m => m.CreateJobComponent)
  },
  {
    path: 'jobs/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/job-detail/job-detail.component').then(m => m.JobDetailComponent)
  },
  { path: '**', redirectTo: 'dashboard' }
];