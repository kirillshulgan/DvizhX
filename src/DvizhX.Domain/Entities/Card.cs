using DvizhX.Domain.Common;

namespace DvizhX.Domain.Entities
{
    public class Card : BaseEntity
    {
        public Guid ColumnId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public int OrderIndex { get; set; }

        // Кто назначен на задачу?
        public Guid? AssignedUserId { get; set; }
        public User? AssignedUser { get; set; }
    }
}
