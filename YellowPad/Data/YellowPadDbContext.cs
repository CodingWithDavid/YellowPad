using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using YellowPad.Models;

namespace YellowPad.Data;

public sealed class YellowPadDbContext(DbContextOptions<YellowPadDbContext> options)
    : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

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
        item.Property(todo => todo.Title).IsRequired().HasMaxLength(500);
        item.Property(todo => todo.Color).HasDefaultValue(TaskColor.Default);
        item.Property(todo => todo.CreatedUtc).HasConversion(utcConverter);
        item.Property(todo => todo.CompletedUtc).HasConversion(nullableUtcConverter);
        item.Property(todo => todo.UpdatedUtc).HasConversion(nullableUtcConverter);
        item.HasIndex(todo => new { todo.IsCompleted, todo.SortOrder });
    }
}
