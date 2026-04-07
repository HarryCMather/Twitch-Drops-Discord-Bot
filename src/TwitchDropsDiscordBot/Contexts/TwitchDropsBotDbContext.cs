using Microsoft.EntityFrameworkCore;
using TwitchDropsDiscordBot.Models.Entities;

namespace TwitchDropsDiscordBot.Contexts;

public sealed class TwitchDropsBotDbContext : DbContext
{
    public TwitchDropsBotDbContext(DbContextOptions<TwitchDropsBotDbContext> dbContextOptions) : base(dbContextOptions)
    {
    }

    public DbSet<Game> Games { get; set; }

    public DbSet<DropOwner> DropOwners { get; set; }

    public DbSet<Drop> Drops { get; set; }

    public DbSet<TimeBasedDrop> TimeBasedDrops { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>()
                    .HasKey(game => game.Id);

        modelBuilder.Entity<Game>()
                    .Property(game => game.Id)
                    .ValueGeneratedOnAdd();

        modelBuilder.Entity<Game>()
                    .Property(game => game.Name)
                    .IsRequired()
                    .HasMaxLength(64);

        modelBuilder.Entity<Game>()
                    .Property(game => game.ShouldAlert)
                    .IsRequired();

        modelBuilder.Entity<Game>()
                    .HasIndex(game => game.Name)
                    .IsUnique();


        modelBuilder.Entity<DropOwner>()
                    .HasKey(dropOwner => dropOwner.Id);

        modelBuilder.Entity<DropOwner>()
                    .Property(dropOwner => dropOwner.Id)
                    .ValueGeneratedOnAdd();

        modelBuilder.Entity<DropOwner>()
                    .Property(dropOwner => dropOwner.Name)
                    .IsRequired()
                    .HasMaxLength(64);

        modelBuilder.Entity<DropOwner>()
                    .HasIndex(dropOwner => dropOwner.Name)
                    .IsUnique();


        modelBuilder.Entity<Drop>()
                    .HasKey(drop => drop.Id);

        modelBuilder.Entity<Drop>()
                    .Property(drop => drop.Id)
                    .ValueGeneratedNever();

        modelBuilder.Entity<Drop>()
                    .Property(drop => drop.Name)
                    .IsRequired();

        modelBuilder.Entity<Drop>()
                    .Property(drop => drop.Description)
                    .IsRequired();

        modelBuilder.Entity<Drop>()
                    .HasOne<DropOwner>()
                    .WithMany()
                    .HasForeignKey("drop_owner_id")
                    .IsRequired();

        modelBuilder.Entity<Drop>()
                    .HasOne<Game>()
                    .WithMany()
                    .HasForeignKey("game_id")
                    .IsRequired();


        modelBuilder.Entity<TimeBasedDrop>()
                    .HasKey(timeBasedDrop => timeBasedDrop.Id);

        modelBuilder.Entity<TimeBasedDrop>()
                    .Property(timeBasedDrop => timeBasedDrop.Id)
                    .ValueGeneratedNever();

        modelBuilder.Entity<TimeBasedDrop>()
                    .Property(timeBasedDrop => timeBasedDrop.Name)
                    .IsRequired();

        modelBuilder.Entity<TimeBasedDrop>()
                    .HasOne<Drop>()
                    .WithMany()
                    .HasForeignKey("parent_drop_id")
                    .IsRequired();
    }
}
