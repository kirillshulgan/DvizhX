import type { JSX } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { LoginPage } from './features/auth/LoginPage';
import { BoardPage } from './features/kanban/BoardPage';
import { EventListPage } from './features/events/EventListPage';
import { RegisterPage } from './features/auth/RegisterPage';

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

                <Route path="/register" element={<RegisterPage />} />
                
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