export interface EventItem {
  id: string;
  title: string;
  description: string;
  location: string;
  startDate: string;
  endDate: string;
  totalTickets: number;
  availableTickets: number;
  price: number;
  organizerId: string;
  imageUrl?: string | null;
  category: string;
}

export interface CreateEventRequest {
  title: string;
  description: string;
  location: string;
  startDate: string;
  endDate: string;
  totalTickets: number;
  price: number;
  imageUrl?: string | null;
  category: string;
}

export type UpdateEventRequest = Partial<CreateEventRequest>;
