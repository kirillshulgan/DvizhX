import { useState } from 'react';
import { useNavigate, Link as RouterLink } from 'react-router-dom';
import apiClient from '../../api/client';
import type { AuthResponse } from '../../types';

// MUI Imports
import { Container, Paper, TextField, Button, Typography, Box, Alert, Link } from '@mui/material';
import PersonAddIcon from '@mui/icons-material/PersonAdd';

export const RegisterPage = () => {
    // Состояние формы
    const [formData, setFormData] = useState({
        email: '',
        password: '',
        confirmPassword: '',
        userName: ''
    });
    
    const [error, setError] = useState('');
    const navigate = useNavigate();

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value
        });
    };

    const handleRegister = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');

        if (formData.password !== formData.confirmPassword) {
            setError('Пароли не совпадают');
            return;
        }

        try {
            // Отправляем запрос
            // Убедись, что поля совпадают с RegisterUserCommand на бэкенде
            const response = await apiClient.post<AuthResponse>('/auth/register', {
                email: formData.email,
                userName: formData.userName,
                password: formData.password
            });

            // Если бэкенд сразу возвращает токен:
            if (response.data && response.data.accessToken) {
                localStorage.setItem('token', response.data.accessToken);
                navigate('/'); // Сразу пускаем внутрь
            } else {
                // Если бэкенд не возвращает токен (просто 200 OK), отправляем логиниться
                alert('Регистрация успешна! Теперь войдите.');
                navigate('/login');
            }

        } catch (err: any) {
            console.error(err);
            // Пытаемся достать текст ошибки из ответа сервера
            const message = err.response?.data?.detail || err.response?.data?.title || 'Ошибка регистрации';
            setError(typeof message === 'string' ? message : 'Не удалось зарегистрироваться');
        }
    };

    return (
        <Container component="main" maxWidth="xs">
            <Box sx={{ marginTop: 8, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                <Paper elevation={3} sx={{ padding: 4, display: 'flex', flexDirection: 'column', alignItems: 'center', width: '100%' }}>
                    
                    <Box sx={{ m: 1, bgcolor: 'primary.main', p: 1, borderRadius: '50%', display: 'flex' }}>
                         <PersonAddIcon sx={{ color: 'white' }} />
                    </Box>
                    
                    <Typography component="h1" variant="h5">
                        Регистрация
                    </Typography>

                    <Box component="form" onSubmit={handleRegister} sx={{ mt: 3, width: '100%' }}>
                        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
                        
                         <TextField
                            margin="normal"
                            fullWidth
                            label="Имя пользователя"
                            name="userName"
                            value={formData.userName}
                            onChange={handleChange}
                        />

                        <TextField
                            margin="normal"
                            required
                            fullWidth
                            label="Email адрес"
                            name="email"
                            type="email"
                            value={formData.email}
                            onChange={handleChange}
                        />
                        
                        <TextField
                            margin="normal"
                            required
                            fullWidth
                            label="Пароль"
                            name="password"
                            type="password"
                            value={formData.password}
                            onChange={handleChange}
                        />

                        <TextField
                            margin="normal"
                            required
                            fullWidth
                            label="Подтвердите пароль"
                            name="confirmPassword"
                            type="password"
                            value={formData.confirmPassword}
                            onChange={handleChange}
                        />

                        <Button
                            type="submit"
                            fullWidth
                            variant="contained"
                            sx={{ mt: 3, mb: 2, height: 45 }}
                        >
                            Зарегистрироваться
                        </Button>

                        <Box sx={{ textAlign: 'center' }}>
                            <Link component={RouterLink} to="/login" variant="body2">
                                {"Уже есть аккаунт? Войти"}
                            </Link>
                        </Box>
                    </Box>
                </Paper>
            </Box>
        </Container>
    );
};
