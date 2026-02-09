import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import apiClient from '../../api/client';
import type { AuthResponse } from '../../types';

import { Container, Paper, TextField, Button, Typography, Box, Alert } from '@mui/material';
import LockOutlinedIcon from '@mui/icons-material/LockOutlined';

import { Link as RouterLink } from 'react-router-dom';
import { Link } from '@mui/material';


export const LoginPage = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const navigate = useNavigate();

    const handleLogin = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        
        try {
            console.log('Отправляю запрос на вход...', { email, password });
            
            const response = await apiClient.post<AuthResponse>('/auth/login', {
                email,
                password
            });
            localStorage.setItem('token', response.data.accessToken);
            navigate('/'); 

        } catch (err: any) {
            console.error('Ошибка при входе:', err);
            const message = err.response?.data?.title || err.response?.data || 'Ошибка входа.';
            setError(typeof message === 'string' ? message : JSON.stringify(message));
        }
    };

    return (
        <Container component="main" maxWidth="xs">
            <Box
                sx={{
                    marginTop: 8,
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                }}
            >
                <Paper elevation={3} sx={{ padding: 4, display: 'flex', flexDirection: 'column', alignItems: 'center', width: '100%' }}>
                    <Box sx={{ m: 1, bgcolor: 'secondary.main', p: 1, borderRadius: '50%', display: 'flex' }}>
                         <LockOutlinedIcon sx={{ color: 'white' }} />
                    </Box>
                    <Typography component="h1" variant="h5">
                        Вход в DvizhX
                    </Typography>

                    <Box component="form" onSubmit={handleLogin} sx={{ mt: 1, width: '100%' }}>
                        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
                        
                        <TextField
                            margin="normal"
                            required
                            fullWidth
                            id="email"
                            label="Email адрес"
                            name="email"
                            autoComplete="email"
                            autoFocus
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                        />
                        <TextField
                            margin="normal"
                            required
                            fullWidth
                            name="password"
                            label="Пароль"
                            type="password"
                            id="password"
                            autoComplete="current-password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                        />
                        <Button
                            type="submit"
                            fullWidth
                            variant="contained"
                            sx={{ mt: 3, mb: 2, height: 50 }}
                        >
                            Войти
                        </Button>
                        <Box sx={{ textAlign: 'center' }}>
                            <Link component={RouterLink} to="/register" variant="body2">
                                {"Нет аккаунта? Зарегистрироваться"}
                            </Link>
                        </Box>
                    </Box>
                </Paper>
            </Box>
        </Container>
    );
};