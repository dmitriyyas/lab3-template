import axios from "axios";

const url = 'http://localhost:8080/api/v1';

export const API = axios.create({ baseURL: url });
//export const authAPI = axios.create({ baseURL: 'http://localhost:8090/api/v1' });
