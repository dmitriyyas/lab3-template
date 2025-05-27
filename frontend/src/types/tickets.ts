import { ShortPrivilege } from "./privileges";

export interface Ticket {
    ticketUid: string;
    flightNumber: string;
    fromAirport: string;
    toAirport: string;
    date: string;
    price: number;
    status: string;
};

export interface PurchaseRequest {
    flightNumber: string;
    price: number;
    paidFromBalance: boolean;
};

export interface PurchaseResponse {
    ticketUid: string;
    flightNumber: string;
    fromAirport: string;
    date: string;
    price: number;
    paidByMoney: number;
    paidByBonuses: number;
    status: string;
    privilege: ShortPrivilege | null;
};