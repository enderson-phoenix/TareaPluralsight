namespace CalSystem.Web.Models;

public class CreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string Equipment { get; set; } = string.Empty;
    public string ProblemDescription { get; set; } = string.Empty;
}
