import { Component, OnInit, inject, computed } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SignalrService } from './services/signalr.service';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  private readonly signalr = inject(SignalrService);
  private readonly authService = inject(AuthService);

  isConnected = false;
  readonly currentUser = computed(() => this.authService.currentUser());

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.signalr.connect(this.authService.accessToken() ?? '');
    }

    this.signalr.connected$.subscribe(status => this.isConnected = status);
  }

  logout(): void {
    this.signalr.disconnect();
    this.authService.logout();
  }
}