export interface ShortPrivilege {
    balance: number;
    status: string;
};

export interface Privilege {
    balance: number;
    status: string;
    history: PrivilegeHistory[];
};

export interface PrivilegeHistory {
    date: string;
    ticketUid: string;
    balanceDiff: number;
    operationType: string;
};