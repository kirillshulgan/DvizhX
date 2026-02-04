using DvizhX.Domain.Common;

namespace DvizhX.Domain.Entities
{
    public class Card : BaseEntity
    {
        public Guid ColumnId { get; set; }

        // Чтобы не зациклило при сериализации, если вдруг вернешь сущность
        [System.Text.Json.Serialization.JsonIgnore]
        public BoardColumn? Column { get; set; }

        public required string Title { get; set; }
        public string? Description { get; set; }
        public int OrderIndex { get; set; }

        // Кто назначен на задачу?
        public Guid? AssignedUserId { get; set; }
        public User? AssignedUser { get; set; }
    }
}
