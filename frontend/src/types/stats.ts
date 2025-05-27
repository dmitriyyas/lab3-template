export interface Stat {
    id: string;
    method: string;
    path: string;
    statusCode: number;
    durationMs: number;
    timestamp: string;
}

export interface StatsResponse {
    page: number;
    pageSize: number;
    totalElements: number;
    items: Stat[];
}