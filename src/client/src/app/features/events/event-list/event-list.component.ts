import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Subscription } from 'rxjs';
import { EventService } from '../../../core/services/event.service';
import { EventItem } from '../../../core/models/event.model';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-event-list',
  templateUrl: './event-list.component.html',
  styleUrls: ['./event-list.component.scss']
})
export class EventListComponent implements OnInit, OnDestroy {
  events: EventItem[] = [];
  filteredEvents: EventItem[] = [];
  loading = false;
  error?: string;

  search = new FormControl<string>('', { nonNullable: true });
  private subscriptions = new Subscription();

  constructor(
    private readonly eventService: EventService,
    public readonly authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadEvents();

    this.subscriptions.add(
      this.search.valueChanges.subscribe(value => {
        this.applyFilter(value ?? '');
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  refresh(): void {
    this.search.setValue('', { emitEvent: false });
    this.loadEvents();
  }

  trackByEvent(_: number, event: EventItem): string {
    return event.id;
  }

  private loadEvents(): void {
    this.loading = true;
    this.error = undefined;

    this.eventService.getEvents().subscribe({
      next: events => {
        this.events = events;
        this.applyFilter(this.search.value ?? '');
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.error = err?.error?.message ?? 'Etkinlikler getirilirken bir hata oluştu.';
      }
    });
  }

  private applyFilter(term: string): void {
    const keyword = term.trim().toLowerCase();
    if (!keyword) {
      this.filteredEvents = [...this.events];
      return;
    }

    this.filteredEvents = this.events.filter(ev =>
      [ev.title, ev.location, ev.category].some(field => field.toLowerCase().includes(keyword))
    );
  }
}
