import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = 'http://localhost:5158/api';

  constructor(private http: HttpClient) { }

  // Authentication methods
  register(email: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/Authentication/register`, { email, password });
  }

  login(email: string, password: string): Observable<string> {
    return this.http.post(`${this.apiUrl}/Authentication/login`, { email, password }, 
      { responseType: 'text' });
  }

  // User profile methods
  getProfile(userId: string | number): Observable<any> {
    return this.http.get(`${this.apiUrl}/Users/getprofile/${userId}`);
  }

  saveProfile(profile: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/Users/saveprofile`, profile, 
      { responseType: 'text' });
  }

}
