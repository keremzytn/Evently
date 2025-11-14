import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { EventService } from '../../../core/services/event.service';
import { EventItem } from '../../../core/models/event.model';
import { TicketService } from '../../../core/services/ticket.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-event-detail',
  templateUrl: './event-detail.component.html',
  styleUrls: ['./event-detail.component.scss']
})
export class EventDetailComponent implements OnInit {
  event?: EventItem;
  loading = false;
  error?: string;
  ticketMessage?: string;
  ticketError?: string;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly eventService: EventService,
    private readonly ticketService: TicketService,
    public readonly authService: AuthService
  ) {}

  ngOnInit(): void {
    const eventId = this.route.snapshot.paramMap.get('id');

    if (!eventId) {
      this.router.navigate(['/events']);
      return;
    }

    this.fetchEvent(eventId);
  }

  purchaseTicket(): void {
    if (!this.event) {
      return;
    }

    this.ticketMessage = undefined;
    this.ticketError = undefined;

    this.ticketService.purchaseTicket({ eventId: this.event.id, price: this.event.price }).subscribe({
      next: response => {
        this.ticketMessage = `Biletiniz hazır! Kod: ${response.ticketCode}`;
      },
      error: err => {
        this.ticketError = err?.error?.message ?? 'Bilet satın alınırken bir hata oluştu.';
      }
    });
  }

  private fetchEvent(id: string): void {
    this.loading = true;
    this.error = undefined;

    this.eventService.getEventById(id).subscribe({
      next: event => {
        this.event = event;
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.error = err?.error?.message ?? 'Etkinlik getirilemedi.';
      }
    });
  }
}
