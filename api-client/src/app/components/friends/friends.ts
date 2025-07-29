import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiService, Friend, PaginatedFriendsResponse } from '../../services/api';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs';

@Component({
  selector: 'app-friends',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './friends.html',
  styleUrl: './friends.css'
})
export class FriendsComponent implements OnInit {
  friends = signal<Friend[]>([]);
  loading = signal(false);
  searchLoading = signal(false);
  
  // Pagination signals
  currentPage = signal(1);
  totalPages = signal(1);
  totalCount = signal(0);
  pageSize = 6;
  
  // Search
  searchTerm = signal('');
  isSearchActive = signal(false);
  
  // Forms
  searchForm: FormGroup;
  addFriendForm: FormGroup;
  
  showAddForm = signal(false);
  editingFriend = signal<Friend | null>(null);
  
  errorMessage = signal('');
  successMessage = signal('');

  constructor(
    private apiService: ApiService,
    private fb: FormBuilder
  ) {
    this.searchForm = this.fb.group({
      search: ['']
    });

    this.addFriendForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      phoneNumber: ['', [Validators.required]]
    });

    this.searchForm.get('search')?.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(searchTerm => {
        this.searchTerm.set(searchTerm);
        
        if (searchTerm && searchTerm.trim()) {
          this.isSearchActive.set(true);
          this.searchLoading.set(true);
          this.currentPage.set(1); // Reset to first page for new search
          return this.apiService.searchFriends(searchTerm.trim(), 1, this.pageSize);
        } else {
          this.isSearchActive.set(false);
          this.currentPage.set(1);
          return this.apiService.getFriends(1, this.pageSize);
        }
      })
    ).subscribe({
      next: (response) => {
        this.handleFriendsResponse(response);
        this.searchLoading.set(false);
      },
      error: (error) => {
        console.error('Search error:', error);
        this.searchLoading.set(false);
        this.errorMessage.set('Failed to search friends');
        this.clearMessages();
      }
    });
  }

  ngOnInit(): void {
    this.loadFriends();
  }

  loadFriends() {
    this.loading.set(true);
    
    const request = this.isSearchActive() 
      ? this.apiService.searchFriends(this.searchTerm(), this.currentPage(), this.pageSize)
      : this.apiService.getFriends(this.currentPage(), this.pageSize);
    
    request.subscribe({
      next: (response) => {
        this.handleFriendsResponse(response);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading friends:', error);
        this.loading.set(false);
        this.errorMessage.set('Failed to load friends');
        this.clearMessages();
      }
    });
  }

  private handleFriendsResponse(response: PaginatedFriendsResponse) {
    this.friends.set(response.friends);
    this.totalCount.set(response.totalCount);
    this.currentPage.set(response.currentPage);
    this.totalPages.set(response.totalPages);
  }

  onPageChange(page: number) {
    if (page < 1 || page > this.totalPages() || page === this.currentPage()) {
      return;
    }
    
    this.currentPage.set(page);
    this.loadFriends();
  }

  clearSearch() {
    this.searchForm.get('search')?.setValue('');
    this.searchTerm.set('');
    this.isSearchActive.set(false);
    this.currentPage.set(1);
    this.loadFriends();
  }

  showAddFriendForm() {
    this.showAddForm.set(true);
    this.editingFriend.set(null);
    this.addFriendForm.reset();
  }

  hideAddForm() {
    this.showAddForm.set(false);
    this.editingFriend.set(null);
    this.addFriendForm.reset();
  }

  editFriend(friend: Friend) {
    this.editingFriend.set(friend);
    this.showAddForm.set(true);
    this.addFriendForm.patchValue({
      firstName: friend.firstName,
      lastName: friend.lastName,
      phoneNumber: friend.phoneNumber
    });
  }

  onSubmitFriend() {
    if (this.addFriendForm.invalid) {
      return;
    }

    const friendData = this.addFriendForm.value;
    const isEditing = this.editingFriend();

    if (isEditing) {
      // Update existing friend
      this.apiService.updateFriend(isEditing.id!, friendData).subscribe({
        next: () => {
          this.successMessage.set('Friend updated successfully!');
          this.hideAddForm();
          this.loadFriends();
          this.clearMessages();
        },
        error: (error) => {
          console.error('Error updating friend:', error);
          this.errorMessage.set('Failed to update friend');
          this.clearMessages();
        }
      });
    } else {
      // Add new friend
      this.apiService.addFriend(friendData).subscribe({
        next: () => {
          this.successMessage.set('Friend added successfully!');
          this.hideAddForm();
          this.loadFriends();
          this.clearMessages();
        },
        error: (error) => {
          console.error('Error adding friend:', error);
          this.errorMessage.set('Failed to add friend');
          this.clearMessages();
        }
      });
    }
  }

  deleteFriend(friend: Friend) {
    if (!friend.id || !confirm(`Are you sure you want to delete ${friend.firstName} ${friend.lastName}?`)) {
      return;
    }

    this.apiService.deleteFriend(friend.id).subscribe({
      next: () => {
        this.successMessage.set('Friend deleted successfully!');
        this.loadFriends();
        this.clearMessages();
      },
      error: (error) => {
        console.error('Error deleting friend:', error);
        this.errorMessage.set('Failed to delete friend');
        this.clearMessages();
      }
    });
  }

  private clearMessages() {
    setTimeout(() => {
      this.errorMessage.set('');
      this.successMessage.set('');
    }, 3000);
  }

  getPaginationPages(): number[] {
    const total = this.totalPages();
    const current = this.currentPage();
    const pages: number[] = [];
    
    // Show max 5 pages at a time
    let start = Math.max(1, current - 2);
    let end = Math.min(total, start + 4);
    
    if (end - start < 4) {
      start = Math.max(1, end - 4);
    }
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  // Pagination info
  getPaginationInfo(): string {
    const start = (this.currentPage() - 1) * this.pageSize + 1;
    const end = Math.min(this.currentPage() * this.pageSize, this.totalCount());
    return `Showing ${start}-${end} of ${this.totalCount()} friends`;
  }
}