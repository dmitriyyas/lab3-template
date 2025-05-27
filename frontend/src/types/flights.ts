export interface Flight {
    flightNumber: string;
    fromAirport: string;
    toAirport: string;
    date: string;
    price: number;
}

export interface FlightsResponse {
    page: number;
    pageSize: number;
    totalElements: number;
    items: Flight[];
}