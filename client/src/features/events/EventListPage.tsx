import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { kanbanService } from '../../api/kanbanService';
import './EventList.css'; 

export const EventListPage = () => {
    const [events, setEvents] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);
    const navigate = useNavigate();
    
    // Состояние: показываем форму или нет
    const [isCreating, setIsCreating] = useState(false);
    
    const [newEvent, setNewEvent] = useState({ title: '', description: '', date: '', time: '' });

    useEffect(() => {
        loadEvents();
    }, []);

    const loadEvents = async () => {
        try {
            setLoading(true);
            const data = await kanbanService.getMyEvents();
            setEvents(data);
        } catch (error) {
            console.error(error);
            alert('Ошибка загрузки списка событий');
        } finally {
            setLoading(false);
        }
    };

    const handleSubmitCreate = async () => {
        if (!newEvent.title || !newEvent.description || !newEvent.date || !newEvent.time) {
            alert("Заполните все поля!");
            return;
        }

        const combinedDate = new Date(`${newEvent.date}T${newEvent.time}`);
        
        try {
            await kanbanService.createEvent(newEvent.title, newEvent.description, combinedDate.toISOString());
            setIsCreating(false);
            setNewEvent({ title: '', description: '', date: '', time: '' });
            await loadEvents();
        } catch (error) {
            alert("Ошибка создания");
        }
    };

    const handleEventClick = (id: string) => {
        if (!id) {
            alert("У этого события нет доски (ошибка данных)");
            return;
        }
        navigate(`/board/${id}`); // Исправил: переходим по boardId, а не eventId
    };

    return (
        <div className="event-list-container">
            <header className="event-list-header">
                <h1>Мои События 📅</h1>
                
                {/* ЛОГИКА ОТОБРАЖЕНИЯ: Если НЕ создаем - кнопка, Если создаем - ничего (форма будет ниже) */}
                {!isCreating && (
                    <button className="create-btn" onClick={() => setIsCreating(true)}>
                        + Создать событие
                    </button>
                )}
            </header>

            {/* БЛОК ФОРМЫ (Вставлен прямо в верстку) */}
            {isCreating && (
                <div style={{ background: '#f9f9f9', padding: 15, borderRadius: 8, marginBottom: 20, border: '1px solid #ddd' }}>
                    <h3>Новое событие</h3>
                    <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap', marginBottom: 10 }}>
                        <input 
                            type="text" 
                            placeholder="Название" 
                            value={newEvent.title}
                            onChange={e => setNewEvent({...newEvent, title: e.target.value})}
                            style={{ padding: 8, flex: 1 }}
                        />
                        <input 
                            type="text" 
                            placeholder="Описание" 
                            value={newEvent.description}
                            onChange={e => setNewEvent({...newEvent, description: e.target.value})}
                            style={{ padding: 8, flex: 1 }}
                        />
                        <input 
                            type="date" 
                            value={newEvent.date}
                            onChange={e => setNewEvent({...newEvent, date: e.target.value})}
                            style={{ padding: 8 }}
                        />
                        <input 
                            type="time" 
                            value={newEvent.time}
                            onChange={e => setNewEvent({...newEvent, time: e.target.value})}
                            style={{ padding: 8 }}
                        />
                    </div>
                    <div style={{ display: 'flex', gap: 10 }}>
                        <button 
                            onClick={handleSubmitCreate} 
                            style={{ background: '#28a745', color: 'white', padding: '8px 16px', border: 'none', borderRadius: 4, cursor: 'pointer'}}
                        >
                            Сохранить
                        </button>
                        <button 
                            onClick={() => setIsCreating(false)} 
                            style={{ background: '#6c757d', color: 'white', padding: '8px 16px', border: 'none', borderRadius: 4, cursor: 'pointer'}}
                        >
                            Отмена
                        </button>
                    </div>
                </div>
            )}

            {loading ? (
                <div>Загрузка...</div>
            ) : (
                <div className="events-grid">
                    {events.length === 0 ? (
                        <div className="empty-state">
                            Событий пока нет. Создайте первое!
                        </div>
                    ) : (
                        events.map(evt => (
                            <div 
                                key={evt.id} 
                                className="event-card"
                                // ВАЖНО: Убедись, что бэкенд возвращает evt.boardId, иначе переход не сработает
                                onClick={() => handleEventClick(evt.id)} 
                            >
                                <h3>{evt.title}</h3>
                                <p>{evt.description || 'Нет описания'}</p>
                                {/* Форматируем дату для красоты */}
                                <p style={{fontSize: '0.8em', color: '#666'}}>
                                    {evt.startDate ? new Date(evt.startDate).toLocaleString() : ''}
                                </p>
                            </div>
                        ))
                    )}
                </div>
            )}
        </div>
    );
};
