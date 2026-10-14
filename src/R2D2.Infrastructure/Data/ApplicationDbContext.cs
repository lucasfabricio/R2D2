using Microsoft.EntityFrameworkCore;
using R2D2.Domain.Entities;

namespace R2D2.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<DynamicField> DynamicFields => Set<DynamicField>();
    public DbSet<DynamicList> DynamicLists => Set<DynamicList>();
    public DbSet<DynamicForm> DynamicForms => Set<DynamicForm>();
    public DbSet<FormField> FormFields => Set<FormField>();
    public DbSet<BusinessRule> BusinessRules => Set<BusinessRule>();
    public DbSet<Workflow> Workflows => Set<Workflow>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<TableData> TableData => Set<TableData>();
    public DbSet<FieldValue> FieldValues => Set<FieldValue>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ListView> ListViews => Set<ListView>();
    public DbSet<ListViewColumn> ListViewColumns => Set<ListViewColumn>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // DynamicField configuration
        modelBuilder.Entity<DynamicField>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TechnicalName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Label).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RelatedFieldTechnicalName).HasMaxLength(100);

            entity.HasOne(e => e.List)
                .WithMany(l => l.Fields)
                .HasForeignKey(e => e.ListId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RelatedList)
                .WithMany()
                .HasForeignKey(e => e.RelatedListId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(e => new { e.ListId, e.TechnicalName }).IsUnique();
        });

        // DynamicList configuration
        modelBuilder.Entity<DynamicList>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // DynamicForm configuration
        modelBuilder.Entity<DynamicForm>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(e => e.Workflow)
                .WithMany()
                .HasForeignKey(e => e.WorkflowId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // FormField configuration
        modelBuilder.Entity<FormField>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Label).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CascadeFilterField).HasMaxLength(100);

            entity.HasOne(e => e.Form)
                .WithMany(f => f.Fields)
                .HasForeignKey(e => e.FormId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.LinkedList)
                .WithMany()
                .HasForeignKey(e => e.LinkedListId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.LinkedField)
                .WithMany()
                .HasForeignKey(e => e.LinkedFieldId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.ParentFormField)
                .WithMany(e => e.ChildFormFields)
                .HasForeignKey(e => e.ParentFormFieldId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // BusinessRule configuration
        modelBuilder.Entity<BusinessRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ConfigurationJson).HasColumnType("nvarchar(max)");
        });

        // Workflow configuration
        modelBuilder.Entity<Workflow>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.FlowJson).HasColumnType("nvarchar(max)");
        });

        // WorkflowStep configuration
        modelBuilder.Entity<WorkflowStep>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Workflow)
                .WithMany(w => w.Steps)
                .HasForeignKey(e => e.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Rule)
                .WithMany(r => r.WorkflowSteps)
                .HasForeignKey(e => e.RuleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TableData configuration
        modelBuilder.Entity<TableData>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.List)
                .WithMany(l => l.TableData)
                .HasForeignKey(e => e.ListId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // FieldValue configuration
        modelBuilder.Entity<FieldValue>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Value).HasColumnType("nvarchar(max)");
            
            entity.HasOne(e => e.TableData)
                .WithMany(t => t.Values)
                .HasForeignKey(e => e.TableDataId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Field)
                .WithMany(f => f.FieldValues)
                .HasForeignKey(e => e.FieldId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(e => new { e.TableDataId, e.FieldId }).IsUnique();
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);

            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // ListView configuration
        modelBuilder.Entity<ListView>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DisplayName).HasMaxLength(200);

            entity.HasOne(e => e.List)
                .WithMany()
                .HasForeignKey(e => e.ListId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CreateForm)
                .WithMany()
                .HasForeignKey(e => e.CreateFormId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.UpdateForm)
                .WithMany()
                .HasForeignKey(e => e.UpdateFormId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ListViewColumn configuration
        modelBuilder.Entity<ListViewColumn>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FieldTechnicalName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.HeaderLabel).HasMaxLength(200);

            entity.HasOne(e => e.ListView)
                .WithMany(lv => lv.Columns)
                .HasForeignKey(e => e.ListViewId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
