import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { DetailComponent } from './components/detail/detail.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { 
    path: 'dashboard', 
    component: DashboardComponent,
    canActivate: [authGuard] // Protection par JWT
  },
  { 
    path: 'detail/:type',  // type peut être 'album', 'artist' ou 'track'
    component: DetailComponent,
    canActivate: [authGuard], // Protection par JWT
    data: { 
      requiresAuth: true 
    }
  },
  { 
    path: 'login', 
    component: LoginComponent 
  },
  { 
    path: '', 
    redirectTo: 'login', 
    pathMatch: 'full' 
  },
  { 
    path: '**', 
    redirectTo: 'login' // Redirection vers login pour les routes inconnues
  }
];