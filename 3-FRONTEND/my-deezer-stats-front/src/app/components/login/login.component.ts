import { Component } from '@angular/core';
import { LoginService } from '../../services/login.service';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-login',
  templateUrl: './login.component.html',
  imports: [FormsModule, CommonModule],
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  email = '';
  password = '';
  errorMessage = '';
  isLoading = false;

  constructor(
    private loginService: LoginService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  login(): void {
    if (!this.isValidForm()) return;

    this.isLoading = true;
    this.errorMessage = '';

    this.loginService.login(this.email, this.password).subscribe({
      next: (response) => {
        if (!response?.token) {
          throw new Error('Réponse invalide de l\'API');
        }
        
        // 1. Stockage du token
        localStorage.setItem('auth_token', response.token); // Utilisez un nom de clé cohérent
        this.password = ''; // Nettoyage sécuritaire
        
        // 2. Récupération de l'URL de redirection
        const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
        

        this.router.navigateByUrl(returnUrl)
          .then(navSuccess => {
            if (!navSuccess) {
              console.error('Échec de la navigation vers', returnUrl);
              this.router.navigate(['/dashboard']); // Fallback
            }
          })
          .catch(err => {
            console.error('Erreur de navigation:', err);
            window.location.href = '/dashboard'; // Fallback ultime
          });
      },
      error: (err) => {
        this.errorMessage = this.getErrorMessage(err);
        this.isLoading = false;
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }

  private isValidForm(): boolean {
    return this.email.includes('@') && this.password.length >= 6;
  }

  private getErrorMessage(err: any): string {
    switch (err.status) {
      case 0: return 'Serveur indisponible';
      case 401: return 'Identifiants invalides';
      case 429: return 'Trop de tentatives';
      default: return 'Erreur technique';
    }
  }
}