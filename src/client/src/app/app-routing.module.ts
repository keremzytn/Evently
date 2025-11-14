import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { EventListComponent } from './features/events/event-list/event-list.component';
import { EventDetailComponent } from './features/events/event-detail/event-detail.component';
import { EventFormComponent } from './features/events/event-form/event-form.component';
import { MyTicketsComponent } from './features/tickets/my-tickets/my-tickets.component';
import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'events' },
  {
    path: 'auth',
    children: [
      { path: 'login', component: LoginComponent },
      { path: 'register', component: RegisterComponent }
    ]
  },
  {
    path: 'events',
    children: [
      { path: '', component: EventListComponent },
      { path: 'new', component: EventFormComponent, canActivate: [AuthGuard] },
      { path: ':id', component: EventDetailComponent }
    ]
  },
  {
    path: 'tickets',
    children: [{ path: 'mine', component: MyTicketsComponent, canActivate: [AuthGuard] }]
  },
  { path: '**', redirectTo: 'events' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
