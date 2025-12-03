import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { ProfileComponent } from './profile/profile.component';
import { ChangePasswordComponent } from './change-password/change-password.component';
import { RoleGuard } from '../core/guards/role.guard';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  
  {
    path: 'profile',
    component: ProfileComponent, canActivate: [RoleGuard], data: { roles: ['SV', 'GV', 'ADMIN'] }
  },

  { path: 'change-password', component: ChangePasswordComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthRoutingModule { }
