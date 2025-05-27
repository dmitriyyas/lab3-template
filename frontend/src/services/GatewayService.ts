import { AxiosResponse } from "axios";
import { API, /*authAPI*/ } from "./api";
import { FlightsResponse } from "../types/flights";
import { deleteJwtToken, deleteUser, getJwtToken, setJwtToken } from "../utils/utils";
import { AuthData, UserCreate, UserInfo } from "../types/users";
import { Privilege, ShortPrivilege } from "../types/privileges";
import { PurchaseRequest, PurchaseResponse } from "../types/tickets";
import { StatsResponse } from "../types/stats";

export class GatewayService {
    public static getFlights(page?: number, size?: number): Promise<AxiosResponse<FlightsResponse>> {
        return API.get('/flights', {params: {page: page, size: size}});
    }

    public static getStats(page?: number, size?: number): Promise<AxiosResponse<StatsResponse>> {
        return API.get('/stats', {params: {page: page, size: size}, headers: {Authorization: getJwtToken()}});
    }

    public static async getToken(code: string): Promise<AxiosResponse<string>> {
        const result = await API.post('/token', {code: code, clientId: 'Audience'});
        setJwtToken(result.data);

        return result;
    }

    public static getUserInfo(): Promise<AxiosResponse<UserInfo>> {
        return API.get('/me', {headers: {Authorization: getJwtToken()}})
    }

    public static login(authData: AuthData): Promise<AxiosResponse<string>> {
        return API.post('/login', authData)
    }

    public static register(userCreate: UserCreate): Promise<AxiosResponse> {
        return API.post('/users', userCreate)
    }

    public static getPrivilege(): Promise<AxiosResponse<Privilege>> {
        return API.get('/privilege', {headers: {Authorization: getJwtToken()}})
    }

    public static buyTicket(purchase: PurchaseRequest): Promise<AxiosResponse<PurchaseResponse>> {
        return API.post('/tickets', purchase, {headers: {Authorization: getJwtToken()}})
    }

    public static cancelTicket(ticketUid: string): Promise<AxiosResponse> {
        return API.delete(`/tickets/${ticketUid}`, {headers: {Authorization: getJwtToken()}})
    }

    public static logout() {
        deleteJwtToken();
        deleteUser();
    }
}
