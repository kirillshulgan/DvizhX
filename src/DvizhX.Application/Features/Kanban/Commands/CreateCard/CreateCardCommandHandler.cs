using DvizhX.Application.Common.Interfaces;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Application.Common.Interfaces.Realtime;
using DvizhX.Application.Features.Kanban.Common;
using DvizhX.Domain.Entities;
using MediatR;

namespace DvizhX.Application.Features.Kanban.Commands.CreateCard
{
    public class CreateCardCommandHandler(
        ICardRepository cardRepository,
        ICurrentUserService currentUserService,
        IKanbanNotifier notifier,
        INotificationService firebaseService,
        IDeviceTokenRepository deviceTokenRepository) : IRequestHandler<CreateCardCommand, CardDto>
    {
        public async Task<CardDto> Handle(CreateCardCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

            // 1. Проверяем доступ через метод репозитория
            // Важно: Предполагаем, что GetColumnWithHierarchyAsync подгружает Board.Event.Participants
            var column = await cardRepository.GetColumnWithHierarchyAsync(request.ColumnId, cancellationToken);

            if (column == null) throw new Exception("Column not found");

            var isParticipant = column.Board.Event.Participants.Any(p => p.UserId == userId);
            if (!isParticipant) throw new UnauthorizedAccessException("Access denied");

            // 2. Вычисляем OrderIndex через метод репозитория
            var maxIndex = await cardRepository.GetMaxOrderIndexAsync(request.ColumnId, cancellationToken);

            // 3. Создаем карточку
            var card = new Card
            {
                ColumnId = request.ColumnId,
                Title = request.Title,
                Description = request.Description,
                OrderIndex = maxIndex + 1,
                AssignedUserId = null
            };

            // Сохраняем через репозиторий
            await cardRepository.AddAsync(card, cancellationToken);

            // 4. Формируем DTO
            var cardDto = new CardDto(
                card.Id,
                card.Title,
                card.Description,
                card.OrderIndex,
                null,
                null
            );

            // 5. Уведомляем Realtime (SignalR)
            var eventId = column.Board.EventId;
            await notifier.CardCreatedAsync(eventId, cardDto);

            // 6. 🔥 Уведомляем Push (Firebase) 🔥
            try
            {
                // Получаем ID всех участников, кроме меня
                var recipientIds = column.Board.Event.Participants
                    .Where(p => p.UserId != userId)
                    .Select(p => p.UserId)
                    .ToList();

                if (recipientIds.Any())
                {
                    // Достаем токены этих пользователей
                    var tokens = await deviceTokenRepository.GetTokensByUserIdsAsync(recipientIds);

                    if (tokens.Any())
                    {
                        await firebaseService.SendMulticastAsync(
                            tokens,
                            "Новая карточка 🆕",
                            $"В колонке '{column.Title}' добавлена задача: {card.Title}"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                // Логируем, но не выбрасываем исключение, чтобы не откатывать транзакцию создания карточки
                // _logger.LogError(ex, "Failed to send push notification");
            }

            return cardDto;
        }
    }
}
