using DvizhX.Application.Common.Interfaces;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Application.Common.Interfaces.Realtime;
using MediatR;

namespace DvizhX.Application.Features.Kanban.Commands.MoveCard
{
    public class MoveCardCommandHandler(
        ICardRepository cardRepository,
        ICurrentUserService currentUserService,
        IKanbanNotifier notifier)
        : IRequestHandler<MoveCardCommand>
    {
        public async Task Handle(MoveCardCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

            // 1. Грузим карту с проверкой прав
            var card = await cardRepository.GetByIdWithColumnAsync(request.CardId, cancellationToken);
            if (card == null) throw new Exception("Card not found");

            var isParticipant = card.Column!.Board!.Event!.Participants.Any(p => p.UserId == userId);
            if (!isParticipant) throw new UnauthorizedAccessException("Access denied");

            var oldColumnId = card.ColumnId;
            var oldIndex = card.OrderIndex;
            var newColumnId = request.TargetColumnId;
            var newIndex = request.NewOrderIndex;
            var eventId = card.Column.Board.EventId;

            // Если ничего не поменялось - выходим
            if (oldColumnId == newColumnId && oldIndex == newIndex) return;

            // === Логика перемещения ===

            // Сценарий 1: Перенос в другую колонку
            if (oldColumnId != newColumnId)
            {
                // A. Убираем из старой колонки (закрываем дырку)
                var oldColumnCards = await cardRepository.GetCardsByColumnIdAsync(oldColumnId, cancellationToken);
                oldColumnCards.Remove(card); // Удаляем из списка в памяти
                NormalizeIndexes(oldColumnCards); // Пересчитываем 0..N

                // B. Добавляем в новую колонку
                var newColumnCards = await cardRepository.GetCardsByColumnIdAsync(newColumnId, cancellationToken);

                // Защита от выхода за границы массива
                if (newIndex > newColumnCards.Count) newIndex = newColumnCards.Count;
                if (newIndex < 0) newIndex = 0;

                card.ColumnId = newColumnId; // Меняем родителя
                newColumnCards.Insert(newIndex, card); // Вставляем в нужную позицию
                NormalizeIndexes(newColumnCards); // Пересчитываем 0..N
            }
            // Сценарий 2: Перемещение внутри той же колонки
            else
            {
                var cards = await cardRepository.GetCardsByColumnIdAsync(oldColumnId, cancellationToken);

                // Удаляем со старой позиции
                cards.Remove(card);

                // Вставляем на новую
                if (newIndex > cards.Count) newIndex = cards.Count;
                if (newIndex < 0) newIndex = 0;

                cards.Insert(newIndex, card);

                NormalizeIndexes(cards);
            }

            // 2. Сохраняем изменения в БД
            // Так как мы загрузили карты через трекинг, EF сам увидит изменения индексов
            await cardRepository.UpdateAsync(card, cancellationToken); // Это вызовет SaveChanges

            // 3. Уведомляем SignalR
            await notifier.CardMovedAsync(eventId, card.Id, newColumnId, newIndex);
        }

        private void NormalizeIndexes(List<Domain.Entities.Card> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].OrderIndex = i;
            }
        }
    }
}
