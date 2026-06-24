namespace CalSystem.Application.Orders.Queries.GetOrdersByStatus;

public record OrderDto(
    Guid Id,
    string CustomerName,
    string Equipment,
    string ProblemDescription,
    string Status,
    Guid? TechnicianId,
    DateTime CreatedAt
);
