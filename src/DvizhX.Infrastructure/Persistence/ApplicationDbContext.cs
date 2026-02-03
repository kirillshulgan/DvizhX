using DvizhX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DvizhX.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventParticipant> Participants { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<BoardColumn> Columns { get; set; }
        public DbSet<Card> Cards { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Применяем все конфигурации из текущей сборки (если будешь использовать IEntityTypeConfiguration)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Пример явной настройки (Fluent API)
            modelBuilder.Entity<EventParticipant>()
                .HasOne(ep => ep.Event)
                .WithMany(e => e.Participants)
                .HasForeignKey(ep => ep.EventId)
                .OnDelete(DeleteBehavior.Cascade); // Удалил ивент -> удалил участников

            modelBuilder.Entity<Event>()
               .HasIndex(e => e.InviteCode)
               .IsUnique(); // Инвайт код уникален
        }
    }
}
