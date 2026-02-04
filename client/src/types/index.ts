export interface User {
    id: string;
    email: string;
    firstName?: string;
    lastName?: string;
}

export interface Card {
    id: string;
    title: string;
    description: string;
    columnId: string;
    orderIndex: number;
    assignedUserId?: string;
}

export interface BoardColumn {
    id: string;
    title: string;
    orderIndex: number;
    cards: Card[];
}

export interface Board {
    id: string;
    title: string;
    columns: BoardColumn[];
}

export interface AuthResponse {
    accessToken: string;
    refreshToken: string;
}