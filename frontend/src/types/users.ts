import { ShortPrivilege } from "./privileges";
import { Ticket } from "./tickets";

export interface User {
    id: number;
    username: string;
    name: string;
    surname: string;
    fathername: string;
    phone: string;
    email: string;
    role: string;
};

export interface UserInfo {
    user: User;
    tickets: Ticket[];
    privilege: ShortPrivilege | null;
};

export interface AuthData {
    username: string;
    password: string;
    redirectUri: string;
    clientId: string;
    scope: string;
    state: string;
};

export interface UserCreate{
    username: string;
    password: string;
    name: string;
    surname: string;
    fathername: string;
    phone: string;
    email: string;
};