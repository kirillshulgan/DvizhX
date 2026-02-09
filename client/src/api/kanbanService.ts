import apiClient from './client';
import type { Board, BoardColumn, Card } from '../types';

// Временный хак: так как у нас нет страницы выбора Event, 
// мы будем запрашивать список и брать первый попавшийся.
interface EventDto {
    id: string;
    title: string;
    description: string;
    startDate: Date;
    status: number;
    inviteCode: string;
    inviteLink: string;
    myRole: number;
    participantsCount: number;
}

export const kanbanService = {
    // 1. Получить мои события (чтобы узнать ID доски)
    getMyEvents: async () => {
        const response = await apiClient.get<EventDto[]>('/events');
        return response.data;
    },

    // 2. Получить доску целиком
    getBoard: async (eventId: string) => {
        const response = await apiClient.get<Board>(`/kanban/${eventId}`);
        return response.data;
    },

    createEvent: async (title: string, description: string, startDate: string) => {
        const response = await apiClient.post<string>('/events', {
            title,
            description,
            startDate
        });
        return response.data;
    },

    createCard: async (columnId: string, title: string, description?: string) => {
        // Убедись, что CreateCardCommand на бэкенде принимает эти поля
        const response = await apiClient.post('/kanban/cards', {
            columnId,
            title,
            description
        });
        return response.data;
    },

    moveCard: async (cardId: string, targetColumnId: string, newOrderIndex: number) => {
        // У нас уже есть MoveCardCommand на бэкенде
        const response = await apiClient.put('/kanban/cards/move', {
            cardId,
            targetColumnId,
            newOrderIndex
        });
        return response.data;
    }
};
