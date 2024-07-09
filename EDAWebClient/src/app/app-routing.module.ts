import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AuthGuard } from './guards/auth-guard.service';
import { LoginBanGuard } from './guards/login-ban.guard';
import { HomeComponent } from './components/home/home.component';
import { LoginComponent } from './components/login/login.component';

export const ROUTES = {
  LOGIN: '/login',
  HOME: '/home'
};

const routes: Routes = [{
  path: 'home',
  component: HomeComponent,
  canActivate: [AuthGuard]
}, {
  path: 'login',
  component: LoginComponent,
  canActivate: [LoginBanGuard]
}, {
  path: '',
  redirectTo: ROUTES.LOGIN,
  pathMatch: 'prefix'
}];

@NgModule({
  imports: [RouterModule.forRoot(routes, { useHash: true })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
