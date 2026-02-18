using DvizhX.Application.Common.Interfaces;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Application.Common.Interfaces.Realtime;
using MediatR;

namespace DvizhX.Application.Features.Kanban.Commands.MoveCard
{
    public class MoveCardCommandHandler(
        ICardRepository cardRepository,
        ICurrentUserService currentUserService,
        IKanbanNotifier notifier,
        IDeviceTokenRepository deviceTokenRepository,
        INotificationService notificationService,
        IEventRepository eventRepository,
        IUserRepository userRepository)
        : IRequestHandler<MoveCardCommand>
    {
        public async Task Handle(MoveCardCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

            // 1. Грузим карту с проверкой прав
            var card = await cardRepository.GetByIdWithColumnAsync(request.CardId, cancellationToken);
            if (card == null) throw new Exception("Card not found");

            var eventEntity = card.Column!.Board!.Event; // Ссылка на Event
            if (eventEntity == null) throw new Exception("Event not found for board");

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

            // 4. --- PUSH NOTIFICATIONS ---
            // Оборачиваем в try-catch, чтобы не ломать основной процесс из-за ошибок Firebase
            try
            {
                // Находим всех участников события, кроме текущего юзера
                var participantIds = eventEntity.Participants
                    .Where(p => p.UserId != userId)
                    .Select(p => p.UserId)
                    .ToList();

                if (participantIds.Any())
                {
                    // Получаем FCM токены этих пользователей
                    var tokens = await deviceTokenRepository.GetTokensByUserIdsAsync(participantIds, cancellationToken);

                    if (tokens.Any())
                    {
                        // Формируем текст уведомления
                        // Можно добавить имя юзера, если оно есть в currentUserService (Name claim)
                        var currentUser = await userRepository.GetByIdAsync(currentUserService.UserId.Value);

                        var title = "Задача перемещена";
                        var body = $"{currentUser.Username} переместил карточку '{card.Title}'";

                        // Отправляем пуш
                        await notificationService.SendMulticastAsync(tokens, title, body);
                    }
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку отправки пуша, но не падаем
                Console.WriteLine($"Failed to send push notification: {ex.Message}");
            }
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
