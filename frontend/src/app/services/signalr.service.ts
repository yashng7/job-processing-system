import { Injectable, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { Job } from '../models/job.model';

@Injectable({ providedIn: 'root' })
export class SignalrService implements OnDestroy {
  private hubConnection!: signalR.HubConnection;

  readonly jobCreated$ = new Subject<Job>();
  readonly jobUpdated$ = new Subject<Job>();
  readonly connected$ = new Subject<boolean>();

  connect(accessToken: string): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/jobs', {
        accessTokenFactory: () => accessToken
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.hubConnection.on('JobCreated', (job: Job) => this.jobCreated$.next(job));
    this.hubConnection.on('JobUpdated', (job: Job) => this.jobUpdated$.next(job));

    this.hubConnection.onreconnecting(() => this.connected$.next(false));
    this.hubConnection.onreconnected(() => this.connected$.next(true));
    this.hubConnection.onclose(() => this.connected$.next(false));

    this.hubConnection.start()
      .then(() => this.connected$.next(true))
      .catch(err => {
        console.warn('SignalR connection failed:', err);
        this.connected$.next(false);
      });
  }

  disconnect(): void {
    this.hubConnection?.stop();
  }

  ngOnDestroy(): void {
    this.disconnect();
  }
}