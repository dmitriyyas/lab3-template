import { User } from "../types/users";

export const setJwtToken = (token: string) => {
    localStorage.setItem('token', token);
};

export const deleteJwtToken = () => {
    localStorage.removeItem('token');
}

export const getJwtToken = () => {
    return `Bearer ${localStorage.getItem('token')}`;
};

export const getUser = (): User | undefined => {
    let str = localStorage.getItem('user');
    let user: User | undefined = str? JSON.parse(str) : undefined;
    return user;
};

export const deleteUser = () => {
    localStorage.removeItem('user');
}

export const setUser = (user: User) => {
    localStorage.setItem('user', JSON.stringify(user));
}

export const getSearchParams = (page: number): [number, number] => {
    const take = 5;
    const skip = take * (page - 1);
    return [skip, take];
}

export const parseToCustomFormat = (dateString: string): string => {
    const date = new Date(dateString);
    if (isNaN(date.getDate()))
        return '';
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear().toString();
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');

    return `${day}.${month}.${year} ${hours}:${minutes}`;
}

export const parseToISOString = (customFormatString: string): string | undefined => {
    try {
        const [datePart, timePart] = customFormatString.split(' ');
        const [day, month, year] = datePart.split('.');
        const [hours, minutes] = timePart.split(':');

        const isoString = `${year}-${month}-${day}T${hours}:${minutes}:00.000Z`;
        return new Date(isoString).toISOString();
    } catch {
        return undefined;
    }
}
