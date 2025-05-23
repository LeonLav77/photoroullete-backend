using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vjezba.Model;

namespace Vjezba.DAL
{
    public class ClientManagerDbContext : IdentityDbContext<AppUser>
    {
        protected ClientManagerDbContext() { }
        
        public ClientManagerDbContext(DbContextOptions<ClientManagerDbContext> options) : base(options)
        { }

        // DbSets for game entities
        public DbSet<Game> Games { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Lobby> Lobbies { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Game entity
            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')"); // SQLite syntax
                
                // Configure relationships
                entity.HasMany(e => e.PlayersCollection)
                      .WithOne(p => p.Game)
                      .HasForeignKey(p => p.GameId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.RoundsCollection)
                      .WithOne(r => r.Game)
                      .HasForeignKey(r => r.GameId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Player entity
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ConnectionId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ImagesJson).HasDefaultValue("[]");
                entity.Property(e => e.IsReady).HasDefaultValue(false);
            });

            // Configure Round entity
            modelBuilder.Entity<Round>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Image).IsRequired();
                entity.Property(e => e.CorrectAnswer).HasMaxLength(200);
                
                // Configure relationships
                entity.HasMany(e => e.AnswersCollection)
                      .WithOne(a => a.Round)
                      .HasForeignKey(a => a.RoundId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Answer entity
            modelBuilder.Entity<Answer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Player).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PlayersAnswer).HasMaxLength(200);
                entity.Property(e => e.TimeRemaining).HasDefaultValue(0);
                entity.Property(e => e.Score).HasDefaultValue(0);
            });

            // Configure Lobby entity
            modelBuilder.Entity<Lobby>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')"); // SQLite syntax
            });

            // Create indexes for better performance
            modelBuilder.Entity<Game>()
                .HasIndex(g => g.Code)
                .IsUnique();

            modelBuilder.Entity<Lobby>()
                .HasIndex(l => l.Code)
                .IsUnique();

            modelBuilder.Entity<Player>()
                .HasIndex(p => p.ConnectionId);
        }
    }
}