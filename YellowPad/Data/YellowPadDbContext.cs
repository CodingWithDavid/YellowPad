using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using YellowPad.Models;

namespace YellowPad.Data;

public sealed class YellowPadDbContext(DbContextOptions<YellowPadDbContext> options)
    : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<TodoTab> TodoTabs => Set<TodoTab>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            value => value,
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
        var nullableUtcConverter = new ValueConverter<DateTime?, DateTime?>(
            value => value,
            value => value.HasValue
                ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                : value);

        var item = modelBuilder.Entity<TodoItem>();
        item.HasOne<TodoTab>()
            .WithMany()
            .HasForeignKey(todo => todo.TabId)
            .OnDelete(DeleteBehavior.Cascade);
        item.Property(todo => todo.Title).IsRequired().HasMaxLength(500);
        item.Property(todo => todo.Color).HasDefaultValue(TaskColor.Default);
        item.Property(todo => todo.CreatedUtc).HasConversion(utcConverter);
        item.Property(todo => todo.CompletedUtc).HasConversion(nullableUtcConverter);
        item.Property(todo => todo.UpdatedUtc).HasConversion(nullableUtcConverter);
        item.HasIndex(todo => new { todo.IsCompleted, todo.SortOrder });

        var tab = modelBuilder.Entity<TodoTab>();
        tab.Property(value => value.Name).IsRequired().HasMaxLength(100);
        tab.Property(value => value.CreatedUtc).HasConversion(utcConverter);
        tab.HasIndex(value => value.SortOrder);
    }
}
