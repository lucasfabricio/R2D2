namespace R2D2.Domain.Entities;

public class ListViewColumn
{
    public Guid Id { get; set; }
    public Guid ListViewId { get; set; }
    public string FieldTechnicalName { get; set; } = string.Empty;
    public int Order { get; set; }
    public string? HeaderLabel { get; set; }

    // Navigation property
    public ListView ListView { get; set; } = null!;
}
