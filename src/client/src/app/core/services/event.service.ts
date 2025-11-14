import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateEventRequest, EventItem, UpdateEventRequest } from '../models/event.model';

@Injectable({
  providedIn: 'root'
})
export class EventService {
  private readonly baseUrl = `${environment.apiBaseUrl}/events`;

  constructor(private readonly http: HttpClient) {}

  getEvents(): Observable<EventItem[]> {
    return this.http.get<EventItem[]>(this.baseUrl);
  }

  getEventById(id: string): Observable<EventItem> {
    return this.http.get<EventItem>(`${this.baseUrl}/${id}`);
  }

  createEvent(payload: CreateEventRequest): Observable<EventItem> {
    return this.http.post<EventItem>(this.baseUrl, payload);
  }

  updateEvent(id: string, payload: UpdateEventRequest): Observable<EventItem> {
    return this.http.put<EventItem>(`${this.baseUrl}/${id}`, payload);
  }

  deleteEvent(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
