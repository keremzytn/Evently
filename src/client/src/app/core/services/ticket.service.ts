import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PurchaseTicketRequest, TicketItem } from '../models/ticket.model';

@Injectable({
  providedIn: 'root'
})
export class TicketService {
  private readonly baseUrl = `${environment.apiBaseUrl}/tickets`;

  constructor(private readonly http: HttpClient) {}

  getMyTickets(): Observable<TicketItem[]> {
    return this.http.get<TicketItem[]>(`${this.baseUrl}/my-tickets`);
  }

  purchaseTicket(payload: PurchaseTicketRequest): Observable<{ ticketId: number; ticketCode: string }> {
    return this.http.post<{ ticketId: number; ticketCode: string }>(`${this.baseUrl}/purchase`, payload);
  }
}
