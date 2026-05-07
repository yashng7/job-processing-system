import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { JobStatus } from '../../../models/job.model';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './status-badge.component.html'
})
export class StatusBadgeComponent {
  @Input() status: JobStatus = 'Queued';

  get cssClass(): string {
    return `badge badge-${this.status.toLowerCase()}`;
  }

  get icon(): string {
    const icons: Record<JobStatus, string> = {
      Queued: '⏳',
      Processing: '⚙️',
      Completed: '✅',
      Failed: '❌'
    };
    return icons[this.status] || '❓';
  }
}