import { Component, OnInit } from '@angular/core';
import { TicketService } from '../../../core/services/ticket.service';
import { TicketItem } from '../../../core/models/ticket.model';

@Component({
  selector: 'app-my-tickets',
  templateUrl: './my-tickets.component.html',
  styleUrls: ['./my-tickets.component.scss']
})
export class MyTicketsComponent implements OnInit {
  tickets: TicketItem[] = [];
  loading = false;
  error?: string;

  constructor(private readonly ticketService: TicketService) {}

  ngOnInit(): void {
    this.loadTickets();
  }

  refresh(): void {
    this.loadTickets();
  }

  trackByTicket(_: number, ticket: TicketItem): number {
    return ticket.id;
  }

  private loadTickets(): void {
    this.loading = true;
    this.error = undefined;

    this.ticketService.getMyTickets().subscribe({
      next: tickets => {
        this.tickets = tickets;
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.error = err?.error?.message ?? 'Biletler alınamadı.';
      }
    });
  }
}
