import { Component, OnInit, ChangeDetectorRef, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth';
import { ApiService } from '../../services/api';
import { Router } from '@angular/router';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class Profile implements OnInit {
  profileForm: FormGroup;
  currentUser: any = null;
  loading = signal(false);
  successMessage: string = '';
  errorMessage: string = '';

  constructor(
    private authService: AuthService,
    private apiService: ApiService,
    private fb: FormBuilder,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.profileForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      dateOfBirth: [''],
      profilePicture: ['']
    });
  }

  ngOnInit(): void {
    // Subscribe to current user
    this.authService.currentUser.subscribe(user => {
      console.log('Current user updated:', user);
      this.currentUser = user;
      if (user && user.id) {
        this.loadProfile();
      } else {
        console.log('No user or user ID available');
        this.loading.set(false);
        // Force UI update
        setTimeout(() => this.cdr.detectChanges(), 0);
      }
    });
  }

  loadProfile(): void {
    if (this.currentUser?.id) {
      this.loading.set(true);
      console.log('Loading profile for user ID:', this.currentUser.id);
      this.apiService.getProfile(this.currentUser.id).subscribe({
        next: (profile) => {
          console.log('Profile loaded successfully:', profile);
          // Map API field names to form field names
          const mappedProfile = {
            firstName: profile.firstName,
            lastName: profile.lastName,
            dateOfBirth: profile.birthDate ? profile.birthDate.split('T')[0] : '', // Convert to date format
            profilePicture: profile.photoUrl
          };
          console.log('Mapped profile for form:', mappedProfile);
          this.profileForm.patchValue(mappedProfile);
          this.loading.set(false);
          console.log('Loading set to false:', this.loading());
          // Force UI update
          setTimeout(() => this.cdr.detectChanges(), 0);
          console.log('Change detection triggered');
        },
        error: (error) => {
          console.error('Error loading profile:', error);
          console.error('Error status:', error.status);
          console.error('Error message:', error.message);
          // Don't show error for 404 - just means no profile exists yet
          if (error.status === 404) {
            console.log('No profile found, starting with empty form');
          } else {
            this.errorMessage = 'Failed to load profile data.';
          }
          this.loading.set(false);
          this.cdr.detectChanges();
        }
      });
    } else {
      console.error('No current user or user ID found');
      this.loading.set(false);
      // Force UI update
      setTimeout(() => this.cdr.detectChanges(), 0);
    }
  }

  onSubmit(): void {
    if (this.profileForm.invalid) {
      return;
    }

    this.loading.set(true);
    this.successMessage = '';
    this.errorMessage = '';

    const profileData = {
      id: this.currentUser?.id,
      firstName: this.profileForm.value.firstName,
      lastName: this.profileForm.value.lastName,
      birthDate: this.profileForm.value.dateOfBirth,
      photoUrl: this.profileForm.value.profilePicture
    };

    console.log('Saving profile data:', profileData);

    this.apiService.saveProfile(profileData).subscribe({
      next: () => {
        this.successMessage = 'Profile updated successfully!';
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error saving profile:', error);
        this.errorMessage = 'Failed to update profile. Please try again.';
        this.loading.set(false);
      }
    });
  }

  logout(): void {
    this.authService.logout();
  }
}
