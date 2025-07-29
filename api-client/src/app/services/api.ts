import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Friend {
  id?: number;
  firstName: string;
  lastName: string;
  phoneNumber: string;
}

export interface PaginatedFriendsResponse {
  friends: Friend[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
  message?: string;
}

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

  // Friends methods
  getFriends(page: number = 1, pageSize: number = 10): Observable<PaginatedFriendsResponse> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    return this.http.get<PaginatedFriendsResponse>(`${this.apiUrl}/Friends`, { params });
  }

  searchFriends(searchTerm: string, page: number = 1, pageSize: number = 10): Observable<PaginatedFriendsResponse> {
    const params = new HttpParams()
      .set('searchTerm', searchTerm)
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    return this.http.get<PaginatedFriendsResponse>(`${this.apiUrl}/Friends/search`, { params });
  }

  getFriend(id: number): Observable<Friend> {
    return this.http.get<Friend>(`${this.apiUrl}/Friends/${id}`);
  }

  addFriend(friend: Friend): Observable<Friend> {
    return this.http.post<Friend>(`${this.apiUrl}/Friends`, friend);
  }

  updateFriend(id: number, friend: Friend): Observable<any> {
    return this.http.put(`${this.apiUrl}/Friends/${id}`, friend);
  }

  deleteFriend(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/Friends/${id}`);
  }
}