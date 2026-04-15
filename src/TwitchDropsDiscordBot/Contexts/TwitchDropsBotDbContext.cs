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
        ConfigureGameEntity(modelBuilder);
        ConfigureDropOwnerEntity(modelBuilder);
        ConfigureDropEntity(modelBuilder);
        ConfigureTimeBasedDropEntity(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private static void ConfigureGameEntity(ModelBuilder modelBuilder)
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
    }

    private static void ConfigureDropOwnerEntity(ModelBuilder modelBuilder)
    {
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
    }

    private static void ConfigureDropEntity(ModelBuilder modelBuilder)
    {
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

        // Opting to manage these separately:
        modelBuilder.Entity<Drop>()
                    .Ignore(drop => drop.TimeBasedDrops)
                    .Ignore(drop => drop.GameName)
                    .Ignore(drop => drop.Owner)
                    .Ignore(drop => drop.Status);

        modelBuilder.Entity<Drop>()
                    .HasOne<DropOwner>()
                    .WithMany()
                    .HasForeignKey(drop => drop.DropOwnerId)
                    .IsRequired();

        modelBuilder.Entity<Drop>()
                    .HasOne<Game>()
                    .WithMany()
                    .HasForeignKey(drop => drop.GameId)
                    .IsRequired();
    }

    private static void ConfigureTimeBasedDropEntity(ModelBuilder modelBuilder)
    {
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
                    .HasForeignKey(timeBasedDrop => timeBasedDrop.ParentDropId)
                    .IsRequired();
    }
}
