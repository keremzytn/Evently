import { Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { EventService } from '../../../core/services/event.service';

@Component({
  selector: 'app-event-form',
  templateUrl: './event-form.component.html',
  styleUrls: ['./event-form.component.scss']
})
export class EventFormComponent {
  loading = false;
  error?: string;

  private readonly fb = inject(FormBuilder);

  form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.minLength(3)]],
    description: ['', [Validators.required, Validators.minLength(10)]],
    location: ['', Validators.required],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
    totalTickets: [50, [Validators.required, Validators.min(1)]],
    price: [250, [Validators.required, Validators.min(0)]],
    imageUrl: [''],
    category: ['', Validators.required]
  });

  constructor(
    private readonly eventService: EventService,
    private readonly router: Router
  ) {}

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error = undefined;

    this.eventService.createEvent(this.form.getRawValue()).subscribe({
      next: event => {
        this.loading = false;
        this.router.navigate(['/events', event.id]);
      },
      error: err => {
        this.loading = false;
        this.error = err?.error?.message ?? 'Etkinlik oluşturulamadı.';
      }
    });
  }
}
