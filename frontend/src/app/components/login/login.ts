import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LoginRequest } from '../../models/auth.interface';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class LoginComponent implements OnInit {
  loginData: LoginRequest = {
    username: '',
    password: ''
  };
  errorMessage = '';
  loading = false;
  infoMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Check if user was redirected with a message
    const navigation = this.router.getCurrentNavigation();
    if (navigation?.extras.state) {
      this.infoMessage = navigation.extras.state['message'] || '';
    }
  }

  onSubmit(): void {
    this.errorMessage = '';
    this.infoMessage = '';
    this.loading = true;

    this.authService.login(this.loginData).subscribe({
      next: () => {
        this.loading = false;

        // Try to redirect to the return URL if available
        const navigation = this.router.getCurrentNavigation();
        const returnUrl = navigation?.extras?.state?.['returnUrl'] || '/dashboard';

        this.router.navigate([returnUrl]);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Error al iniciar sesión. Verifica tus credenciales.';
      }
    });
  }
}
