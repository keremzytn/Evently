export interface TicketItem {
  id: number;
  eventId: string;
  ticketCode: string;
  price: number;
  status: string;
  purchasedAt: string;
  usedAt?: string | null;
}

export interface PurchaseTicketRequest {
  eventId: string;
  price: number;
}
