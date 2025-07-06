import { Routes } from '@angular/router';
import { Login } from './components/login/login';
import { Register } from './components/register/register';
import { Profile } from './components/profile/profile';
import { AuthGuard } from './guards/auth-guard';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'profile', component: Profile, canActivate: [AuthGuard] },
  { path: '**', redirectTo: '/login' }
];