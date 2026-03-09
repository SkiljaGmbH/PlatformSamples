import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { LoginComponent } from './components/login/login.component';
import { authGuard, loginBanGuard } from './guards/auth.guards';

export const routes: Routes = [
    { path: 'home', component: HomeComponent, canActivate: [authGuard] },
    { path: 'login', component: LoginComponent, canActivate: [loginBanGuard] },
    { path: '', redirectTo: '/login', pathMatch: 'full' }
];