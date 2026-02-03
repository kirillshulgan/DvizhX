using DvizhX.Domain.Common;

namespace DvizhX.Domain.Entities
{
    public class BoardColumn : BaseEntity
    {
        public Guid BoardId { get; set; }
        public required string Title { get; set; }
        public int OrderIndex { get; set; }
        public ICollection<Card> Cards { get; set; } = [];
    }
}
