import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { kanbanService } from '../../api/kanbanService';
import type { Board } from '../../types';
import './BoardPage.css';
import { DragDropContext, Droppable, Draggable, type DropResult } from '@hello-pangea/dnd';

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

    const onDragEnd = async (result: DropResult) => {
        const { destination, source, draggableId } = result;

        // 1. Если бросили мимо или туда же, где была
        if (!destination) return;
        if (
            destination.droppableId === source.droppableId &&
            destination.index === source.index
        ) {
            return;
        }

        // 2. Оптимистичное обновление UI (чтобы не ждать ответа сервера)
        // Нам нужно клонировать состояние board и изменить его
        const newBoard = { ...board! };
        
        const sourceColIndex = newBoard.columns.findIndex(c => c.id === source.droppableId);
        const destColIndex = newBoard.columns.findIndex(c => c.id === destination.droppableId);
        
        const sourceCol = newBoard.columns[sourceColIndex];
        const destCol = newBoard.columns[destColIndex];

        // Достаем карту из старой колонки
        const [movedCard] = sourceCol.cards.splice(source.index, 1);
        
        // Обновляем ID колонки у карты (если перенесли в другую)
        movedCard.columnId = destCol.id;
        
        // Вставляем в новую
        destCol.cards.splice(destination.index, 0, movedCard);

        // Обновляем стейт сразу
        setBoard(newBoard);

        // 3. Отправляем запрос на сервер
        try {
            await kanbanService.moveCard(draggableId, destination.droppableId, destination.index);
        } catch (error) {
            console.error("Failed to move card", error);
            // Тут по-хорошему надо откатить изменения (loadBoard), если сервер вернул ошибку
            alert("Ошибка синхронизации! Обновите страницу.");
            loadBoard(id!);
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
            <DragDropContext onDragEnd={onDragEnd}>
                <Box sx={{ 
                    flexGrow: 1, 
                    display: 'flex', 
                    overflowX: 'auto', 
                    p: 3, 
                    gap: 3,
                    alignItems: 'flex-start' 
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

                            {/* Droppable Зона - Это список карточек */}
                            <Droppable droppableId={column.id}>
                                {(provided) => (
                                    <Stack
                                        spacing={1}
                                        sx={{ overflowY: 'auto', flexGrow: 1, minHeight: 10 }} // minHeight нужен, чтобы можно было бросить в пустую колонку
                                        ref={provided.innerRef}
                                        {...provided.droppableProps}
                                    >
                                        {column.cards
                                            // .sort мы здесь УБИРАЕМ, так как порядок должен соответствовать индексам в массиве cards,
                                            // которые мы мутируем при DnD. Если оставить sort по OrderIndex,
                                            // карточка будет "прыгать" назад до ответа сервера.
                                            // Сортировка должна происходить один раз при загрузке (useEffect).
                                            .map((card, index) => (
                                            
                                            <Draggable key={card.id} draggableId={card.id} index={index}>
                                                {(provided, snapshot) => (
                                                    <Card
                                                        ref={provided.innerRef}
                                                        {...provided.draggableProps}
                                                        {...provided.dragHandleProps}
                                                        sx={{ 
                                                            cursor: 'grab',
                                                            bgcolor: snapshot.isDragging ? '#e3f2fd' : 'white', // Подсветка при перетаскивании
                                                            boxShadow: snapshot.isDragging ? 3 : 1,
                                                            transition: 'background-color 0.2s, box-shadow 0.2s',
                                                            // Обязательно сохраняем стили от библиотеки (трансформации)
                                                            ...provided.draggableProps.style 
                                                        }}
                                                    >
                                                        <CardContent sx={{ p: 1.5, '&:last-child': { pb: 1.5 } }}>
                                                            <Typography variant="body1" fontWeight={500}>
                                                                {card.title}
                                                            </Typography>
                                                            {card.description && (
                                                                <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 0.5 }}>
                                                                    {card.description}
                                                                </Typography>
                                                            )}
                                                        </CardContent>
                                                    </Card>
                                                )}
                                            </Draggable>

                                        ))}
                                        {provided.placeholder} {/* Заглушка, растягивающая список во время драга */}
                                    </Stack>
                                )}
                            </Droppable>
                            
                            {/* Кнопка "Добавить карточку" */}
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
            </DragDropContext>
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