import { useState } from 'react';
import apiClient from '../../api/client';
import type { AuthResponse } from '../../types';
import { useNavigate } from 'react-router-dom';

export const LoginPage = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const navigate = useNavigate();

    const handleLogin = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(''); // Сброс старых ошибок
        
        try {
            console.log('Отправляю запрос на вход...', { email, password });
            
            const response = await apiClient.post<AuthResponse>('/auth/login', {
                email,
                password
            });

            console.log('Ответ сервера:', response.data);

            // ПРОВЕРКА: Как называется поле с токеном?
            const token = response.data.accessToken; 
            
            if (!token) {
                console.error('ТОКЕН НЕ ПРИШЕЛ! Структура ответа не совпадает с ожидаемой.');
                setError('Ошибка: Сервер не вернул токен.');
                return;
            }

            console.log('Токен получен, сохраняю в localStorage...');
            localStorage.setItem('token', token);
            
            console.log('Перехожу на главную...');
            navigate('/'); 

        } catch (err: any) {
            console.error('Ошибка при входе:', err);
            // Если сервер вернул текст ошибки, покажем его
            const message = err.response?.data?.title || err.response?.data || 'Ошибка входа.';
            setError(typeof message === 'string' ? message : JSON.stringify(message));
        }
    };

    return (
        <div style={{ display: 'flex', justifyContent: 'center', marginTop: '50px' }}>
            <form onSubmit={handleLogin} style={{ display: 'flex', flexDirection: 'column', width: '300px', gap: '10px' }}>
                <h2>Вход в DvizhX</h2>
                {error && <div style={{ color: 'red' }}>{error}</div>}
                
                <input 
                    type="email" 
                    placeholder="Email" 
                    value={email}
                    onChange={e => setEmail(e.target.value)}
                    required
                    style={{ padding: '8px' }}
                />
                <input 
                    type="password" 
                    placeholder="Password" 
                    value={password}
                    onChange={e => setPassword(e.target.value)}
                    required
                    style={{ padding: '8px' }}
                />
                <button type="submit" style={{ padding: '10px', cursor: 'pointer' }}>
                    Войти
                </button>
            </form>
        </div>
    );
};