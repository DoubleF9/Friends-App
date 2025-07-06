import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, catchError, of, tap } from 'rxjs';
import { ApiService } from './api';
import { isPlatformBrowser } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<any>(null);
  public currentUser = this.currentUserSubject.asObservable();

  constructor(
    private apiService: ApiService,
    private router: Router
  ) {
    // Check if user is already logged in
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('token');
      if (token) {
        // Parse token to get user ID and email
        this.parseToken(token);
      }
    }
  }

  register(email: string, password: string): Observable<any> {
    return this.apiService.register(email, password).pipe(
      // Handle successful registration
      tap(() => {
        console.log('Registration successful');
      }),

      catchError((error: HttpErrorResponse) => {
        console.error('Registration error in service:', error);
        
        // JSON parsing error
        if (error.status === 200 && 
            (error.error instanceof SyntaxError || 
             error.message.includes('Http failure during parsing'))) {
          console.log('Registration successful (non-JSON response)');
          return of({ success: true });
        }
        
        throw error;
      })
    );
  }

  login(email: string, password: string): Observable<string> {
    return this.apiService.login(email, password).pipe(
      tap(token => {
        if (typeof window !== 'undefined') {
          localStorage.setItem('token', token as string);
        }
        this.parseToken(token as string);
      }),
      catchError(error => {
        console.error('Login error:', error);
        throw error;
      })
    );
  }

  logout(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('token');
    }
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  isLoggedIn(): boolean {
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('token');
      if (!token) {
        return false;
      }
      
      // Check if token is expired
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        if (payload.exp) {
          const expirationTime = payload.exp * 1000; // Convert to milliseconds
          const currentTime = Date.now();
          
          if (currentTime >= expirationTime) {
            console.log('Token expired in isLoggedIn check');
            this.logout();
            return false;
          }
        }
        return true;
      } catch (e) {
        console.error('Error checking token expiration:', e);
        this.logout();
        return false;
      }
    }
    return false;
  }

  private parseToken(token: string): void {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      console.log('JWT payload:', payload);
      
      // Check if token is expired
      if (payload.exp) {
        const expirationTime = payload.exp * 1000; // Convert to milliseconds
        const currentTime = Date.now();
        
        if (currentTime >= expirationTime) {
          console.log('Token has expired');
          this.logout();
          return;
        }
      }
      
      const userId = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] 
                    || payload['sub'] 
                    || payload['id'] 
                    || payload['nameid'];
                    
      const userEmail = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress']
                       || payload['email'] 
                       || payload['unique_name'];
      
      console.log('Parsed user ID:', userId);
      console.log('Parsed user email:', userEmail);
      
      this.currentUserSubject.next({
        id: userId,
        email: userEmail
      });
    } catch (e) {
      console.error('Error parsing token:', e);
      this.logout();
    }
  }
}