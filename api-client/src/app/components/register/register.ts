import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-register',
  templateUrl: './register.html',
  styleUrls: ['./register.css'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class Register {
  registerForm: FormGroup;
  errorMessage: string = '';
  loading: boolean = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(formGroup: FormGroup) {
    const password = formGroup.get('password')?.value;
    const confirmPassword = formGroup.get('confirmPassword')?.value;
    
    if (password === confirmPassword) {
      return null;
    } else {
      return { passwordMismatch: true };
    }
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const { email, password } = this.registerForm.value;

    this.authService.register(email, password)
      .subscribe({
        next: () => {
          this.loading = false; // Explicitly set loading to false
          console.log('Registration successful, redirecting to login');
          this.router.navigate(['/login']); // Navigate directly to login
        },
        error: (error) => {
          this.loading = false; // Ensure loading is reset on error
          console.error('Registration error:', error);
          
          // Handle different error formats
          if (typeof error.error === 'string') {
            this.errorMessage = error.error;
          } else if (error.error?.message) {
            this.errorMessage = error.error.message;
          } else if (error.status === 400) {
            this.errorMessage = 'Registration failed. Email may already be in use.';
          } else {
            this.errorMessage = 'Registration failed. Please try again.';
          }
        },
        complete: () => {
          this.loading = false; // Also handle completion
        }
      });
  }
}