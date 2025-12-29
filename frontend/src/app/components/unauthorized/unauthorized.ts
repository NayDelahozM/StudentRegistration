import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-unauthorized',
  imports: [CommonModule],
  templateUrl: './unauthorized.html',
  styleUrl: './unauthorized.css',
})
export class UnauthorizedComponent implements OnInit {
  errorMessage: string = 'No tienes permiso para acceder a esta página.';
  returnPath: string = '/dashboard';

  constructor(private router: Router, private authService: AuthService) {}

  ngOnInit(): void {
    const navigation = this.router.getCurrentNavigation();
    if (navigation?.extras.state) {
      this.errorMessage = navigation.extras.state['message'] || this.errorMessage;
      this.returnPath = navigation.extras.state['returnPath'] || this.returnPath;
    }
  }

  goBack(): void {
    this.router.navigate([this.returnPath]);
  }

  goToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
