using DvizhX.Application.Common.Interfaces;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Application.Common.Interfaces.Realtime;
using MediatR;

namespace DvizhX.Application.Features.Kanban.Commands.UpdateCard
{
    public class UpdateCardCommandHandler(
        ICardRepository cardRepository,
        ICurrentUserService currentUserService,
        IKanbanNotifier notifier)
        : IRequestHandler<UpdateCardCommand>
    {
        public async Task Handle(UpdateCardCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

            // Грузим с колонкой, чтобы проверить права через Event
            var card = await cardRepository.GetByIdWithColumnAsync(request.CardId, cancellationToken);
            if (card == null) throw new KeyNotFoundException("Card not found");

            var isParticipant = card.Column!.Board!.Event!.Participants.Any(p => p.UserId == userId);
            if (!isParticipant) throw new UnauthorizedAccessException();

            // Обновляем поля
            card.Title = request.Title;
            card.Description = request.Description;

            await cardRepository.UpdateAsync(card, cancellationToken);

            // Уведомляем фронтенд
            await notifier.CardUpdatedAsync(card.Column.Board.EventId, card.Id, card.Title, card.Description);
        }
    }
}
