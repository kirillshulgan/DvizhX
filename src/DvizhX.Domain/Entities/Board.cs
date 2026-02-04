using DvizhX.Domain.Common;

namespace DvizhX.Domain.Entities
{
    public class Board : BaseEntity
    {
        public Guid EventId { get; set; }
        public Event Event { get; set; }
        public required string Title { get; set; }
        public ICollection<BoardColumn> Columns { get; set; } = [];
    }
}
