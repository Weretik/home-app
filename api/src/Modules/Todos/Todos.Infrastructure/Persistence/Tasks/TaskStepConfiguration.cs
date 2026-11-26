using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Todos.Domain.Tasks;

namespace Todos.Infrastructure.Persistence.Tasks;

internal sealed class TaskStepConfiguration : IEntityTypeConfiguration<TaskStep>
{
    public void Configure(EntityTypeBuilder<TaskStep> builder)
    {
        builder.ToTable("task_steps");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property<Guid>("TaskId").HasColumnName("task_id");
        builder.Property(s => s.Title).HasColumnName("title").HasMaxLength(TodoTask.MaxTitleLength).IsRequired();
        builder.Property(s => s.IsDone).HasColumnName("is_done");
        builder.Property(s => s.SortOrder).HasColumnName("sort_order");
        builder.HasIndex("TaskId", nameof(TaskStep.SortOrder)).HasDatabaseName("ix_task_steps_task_id_sort_order");
    }
}
