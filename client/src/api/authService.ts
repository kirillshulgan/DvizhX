import apiClient from './client';
import type { AuthResponse } from '../types';

export const authService = {
    login: async (data: any) => {
        const response = await apiClient.post<AuthResponse>('/auth/login', data);
        return response.data;
    },

    register: async (data: any) => {
        // Предполагаем, что бэкенд на /auth/register возвращает токен, как и при логине.
        // Если бэкенд возвращает просто 200 OK, то логику придется чуть изменить (редирект на логин).
        const response = await apiClient.post<AuthResponse>('/auth/register', data);
        return response.data;
    }
};
