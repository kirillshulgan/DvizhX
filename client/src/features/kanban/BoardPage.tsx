import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { kanbanService } from '../../api/kanbanService';
import type { Board } from '../../types';
import './BoardPage.css';

export const BoardPage = () => {
    const { id } = useParams(); // <--- Читаем ID из URL
    const navigate = useNavigate();
    
    const [board, setBoard] = useState<Board | null>(null);
    const [loading, setLoading] = useState(true);

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

    if (loading) return <div>Загрузка доски...</div>;
    if (!board) return <div>Доска не найдена</div>;

    return (
        <div style={{height: '100vh', display: 'flex', flexDirection: 'column'}}>
            {/* Хедер доски с кнопкой Назад */}
            <div style={{ padding: '10px 20px', background: '#fff', borderBottom: '1px solid #ddd', display: 'flex', alignItems: 'center', gap: '20px' }}>
                <button onClick={() => navigate('/')} style={{cursor:'pointer'}}>
                    ← Назад к списку
                </button>
                <h2>{board.title}</h2>
            </div>

            {/* Контейнер колонок (старый код) */}
            <div className="board-container" style={{height: 'calc(100vh - 60px)'}}>
                {board.columns
                    .sort((a, b) => a.orderIndex - b.orderIndex)
                    .map(column => (
                    <div key={column.id} className="column">
                        <div className="column-header">
                            {column.title} 
                            <span style={{color: '#888', fontSize: '0.8em'}}>{column.cards.length}</span>
                        </div>
                        <div className="card-list">
                            {column.cards.length === 0 && <div style={{padding:10, color:'#999', fontSize:'0.8em'}}>Нет задач</div>}
                            {column.cards
                                .sort((a, b) => a.orderIndex - b.orderIndex)
                                .map(card => (
                                <div key={card.id} className="kanban-card">
                                    <div className="card-title">{card.title}</div>
                                    {card.description && <div className="card-desc">{card.description}</div>}
                                </div>
                            ))}
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
};