import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Navbar } from './components/navbar/navbar';
import { AuthService } from './services/auth';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  standalone: true,
  imports: [RouterOutlet, CommonModule, Navbar]
})
export class App implements OnInit {
  title = 'API Client App';
  isLoggedIn = false;

  constructor(private authService: AuthService) {}

  ngOnInit() {
    // Subscribe to auth status changes
    this.authService.currentUser.subscribe(user => {
      this.isLoggedIn = !!user;
    });
  }
}