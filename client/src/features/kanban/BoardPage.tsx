import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { kanbanService } from '../../api/kanbanService';
import type { Board } from '../../types';
import './BoardPage.css';

import { 
    Box, Typography, Paper, IconButton, AppBar, Toolbar, Button, 
    Card, CardContent, Chip, Stack,
    Dialog, DialogTitle, DialogContent, DialogActions, TextField 
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import AddIcon from '@mui/icons-material/Add';
// import MoreHorizIcon from '@mui/icons-material/MoreHoriz'; // Иконка "меню" карточки (на будущее)

export const BoardPage = () => {
    const { id } = useParams(); // <--- Читаем ID из URL
    const navigate = useNavigate();
    const [board, setBoard] = useState<Board | null>(null);
    const [loading, setLoading] = useState(true);

    const [isDialogOpen, setDialogOpen] = useState(false);
    const [targetColumnId, setTargetColumnId] = useState<string | null>(null);
    const [newCard, setNewCard] = useState({ title: '', description: '' });

    useEffect(() => {
        if (id) {
            loadBoard(id);
        }
    }, [id]);

    const loadBoard = async (id: string) => {
        try {
            setLoading(true);
            const data = await kanbanService.getBoard(id);
            setBoard(data);
        } catch (err) {
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    // Открыть окно создания для конкретной колонки
    const handleOpenCreateDialog = (columnId: string) => {
        setTargetColumnId(columnId);
        setNewCard({ title: '', description: '' }); // Чистим форму
        setDialogOpen(true);
    };

    // Отправить данные на сервер
    const handleCreateCard = async () => {
        if (!targetColumnId || !newCard.title) return;

        try {
            await kanbanService.createCard(targetColumnId, newCard.title, newCard.description);
            setDialogOpen(false);
            
            // Перезагружаем доску, чтобы увидеть новую карту
            // (В идеале лучше обновлять стейт локально, но пока так проще)
            if (id) loadBoard(id); 
            
        } catch (error) {
            console.error(error);
            alert("Ошибка при создании карточки");
        }
    };

    if (loading) return <div>Загрузка доски...</div>;
    if (!board) return <div>Доска не найдена</div>;

    return (
        <Box sx={{ height: '100vh', display: 'flex', flexDirection: 'column', bgcolor: '#f4f5f7' }}>
            {/* Шапка доски */}
            <AppBar position="static" color="default" elevation={1}>
                <Toolbar>
                    <IconButton edge="start" color="inherit" onClick={() => navigate('/')} sx={{ mr: 2 }}>
                        <ArrowBackIcon />
                    </IconButton>
                    <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
                        {board.title}
                    </Typography>
                    {/* Сюда можно добавить кнопку "Пригласить" */}
                </Toolbar>
            </AppBar>

            {/* Область колонок (Горизонтальный скролл) */}
            <Box sx={{ 
                flexGrow: 1, 
                display: 'flex', 
                overflowX: 'auto', 
                p: 3, 
                gap: 3,
                alignItems: 'flex-start' // Чтобы колонки не растягивались на всю высоту
            }}>
                {board.columns
                    .sort((a, b) => a.orderIndex - b.orderIndex)
                    .map(column => (
                    <Paper 
                        key={column.id} 
                        elevation={0}
                        sx={{ 
                            minWidth: 300, 
                            maxWidth: 300, 
                            bgcolor: '#ebecf0', 
                            p: 2,
                            maxHeight: '100%',
                            display: 'flex',
                            flexDirection: 'column'
                        }}
                    >
                        {/* Заголовок колонки */}
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2, alignItems: 'center' }}>
                            <Typography variant="subtitle1" fontWeight="bold">
                                {column.title}
                            </Typography>
                            <Chip label={column.cards.length} size="small" />
                        </Box>

                        {/* Список карточек */}
                        <Stack spacing={1} sx={{ overflowY: 'auto', flexGrow: 1 }}>
                            {column.cards
                                .sort((a, b) => a.orderIndex - b.orderIndex)
                                .map(card => (
                                <Card key={card.id} sx={{ '&:hover': { bgcolor: '#f9f9f9', cursor: 'pointer' } }}>
                                    <CardContent sx={{ p: 1.5, '&:last-child': { pb: 1.5 } }}>
                                        <Typography variant="body1" fontWeight={500}>
                                            {card.title}
                                        </Typography>
                                        {card.description && (
                                            <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 0.5 }}>
                                                {card.description}
                                            </Typography>
                                        )}
                                        {/* Если назначен юзер - можно показать аватарку тут */}
                                    </CardContent>
                                </Card>
                            ))}
                        </Stack>
                        
                        {/* Кнопка "Добавить карточку" (визуальная заглушка пока) */}
                        <Button 
                            startIcon={<AddIcon />}
                            fullWidth 
                            sx={{ mt: 1, color: '#5e6c84', justifyContent: 'flex-start', textTransform: 'none' }}
                            onClick={() => handleOpenCreateDialog(column.id)}
                        >
                            Добавить карточку
                        </Button>
                    </Paper>
                ))}
            </Box>

            <Dialog open={isDialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
                <DialogTitle>Новая задача</DialogTitle>
                <DialogContent>
                    <TextField
                        autoFocus
                        margin="dense"
                        label="Заголовок задачи"
                        fullWidth
                        variant="outlined"
                        value={newCard.title}
                        onChange={(e) => setNewCard({ ...newCard, title: e.target.value })}
                        // Чтобы можно было отправить по Enter
                        onKeyDown={(e) => {
                            if (e.key === 'Enter') handleCreateCard();
                        }}
                    />
                    <TextField
                        margin="dense"
                        label="Описание (необязательно)"
                        fullWidth
                        multiline
                        rows={3}
                        variant="outlined"
                        value={newCard.description}
                        onChange={(e) => setNewCard({ ...newCard, description: e.target.value })}
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setDialogOpen(false)} color="inherit">
                        Отмена
                    </Button>
                    <Button onClick={handleCreateCard} variant="contained" disabled={!newCard.title}>
                        Создать
                    </Button>
                </DialogActions>
            </Dialog>

        </Box>
    );
};