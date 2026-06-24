namespace CalSystem.Web.Models;

public class OrderDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Equipment { get; set; } = string.Empty;
    public string ProblemDescription { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? TechnicianId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
}
