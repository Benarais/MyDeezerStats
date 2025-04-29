import { Component } from '@angular/core';
import { LoginService, SignInResponse } from '../../services/login.service';
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
  successMessage = '';
  isLoading = false;
  isSignUp: boolean = false;

  constructor(
    private loginService: LoginService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Vérification si l'utilisateur est déjà connecté
    if (this.loginService.isAuthenticated()) {
      this.router.navigate(['/dashboard']); // Redirection vers /dashboard si déjà connecté
    }
  }

    // Bascule entre le mode login et le mode sign-up
  toggleAuthMode() {
      this.isSignUp = !this.isSignUp;
  }

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

  // Méthode pour la création de compte
signUp() {
  this.isLoading = true;
  this.errorMessage = '';
  this.successMessage = '';

  this.loginService.signUp(this.email, this.password).subscribe(
    (response: boolean) => {
      this.isLoading = false;
      if (response) {
        this.successMessage = 'Votre compte a été créé avec succès ! Vous pouvez maintenant vous connecter.';
        // Par exemple : rediriger après quelques secondes
        // this.router.navigate(['/login']);
      } else {
        // Normalement ça ne devrait pas arriver si API est bien faite, mais au cas où :
        this.errorMessage = 'Erreur inattendue lors de la création du compte.';
      }
    },
    (error: any) => {
      this.isLoading = false;

      if (error.status === 409) {
        this.errorMessage = 'Ce compte existe déjà. Essayez de vous connecter.';
      } else if (error.status === 500) {
        this.errorMessage = 'Erreur serveur. Veuillez réessayer plus tard.';
      } else {
        this.errorMessage = 'Erreur inconnue. Veuillez réessayer.';
      }
    }
  );
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