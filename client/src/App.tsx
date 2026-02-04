import type { JSX } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { LoginPage } from './features/auth/LoginPage';
import { BoardPage } from './features/kanban/BoardPage';
import { EventListPage } from './features/events/EventListPage';

// Защищенный маршрут (проверяет наличие токена)
const PrivateRoute = ({ children }: { children: JSX.Element}) => {
    const token = localStorage.getItem('token');
    return token ? children : <Navigate to="/login" />;
};

function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/login" element={<LoginPage />} />
                
                {/* Главная - теперь Список событий */}
                <Route path="/" element={
                    <PrivateRoute>
                        <EventListPage />
                    </PrivateRoute>
                } />

                {/* Страница конкретной доски (параметр :boardId) */}
                <Route path="/board/:id" element={
                    <PrivateRoute>
                        <BoardPage />
                    </PrivateRoute>
                } />
            </Routes>
        </BrowserRouter>
    );
}

export default App;