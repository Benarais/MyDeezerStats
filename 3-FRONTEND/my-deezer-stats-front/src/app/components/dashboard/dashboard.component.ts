// dashboard.component.ts
import { Component, OnInit } from '@angular/core';
import { LoginService } from '../../services/login.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  constructor(
    private loginService: LoginService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (!this.loginService.isAuthenticated()) {
      this.router.navigate(['/login']);
    }
  }
}