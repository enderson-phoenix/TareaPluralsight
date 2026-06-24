using CalSystem.Domain.Enums;

namespace CalSystem.Domain.Entities;

/// <summary>
/// Represents a technical service order in the CalSystem calibration management system.
/// </summary>
public class ServiceOrder
{
    /// <summary>Gets the unique identifier of the service order.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the name of the customer who requested the service.</summary>
    public string CustomerName { get; private set; } = default!;

    /// <summary>Gets the name or model of the equipment to be calibrated or serviced.</summary>
    public string Equipment { get; private set; } = default!;

    /// <summary>Gets the description of the problem reported by the customer.</summary>
    public string ProblemDescription { get; private set; } = default!;

    /// <summary>Gets the current status of the service order.</summary>
    public OrderStatus Status { get; private set; }

    /// <summary>Gets the identifier of the technician assigned to this order, if any.</summary>
    public Guid? TechnicianId { get; private set; }

    /// <summary>Gets the UTC timestamp when the order was created.</summary>
    public DateTime CreatedAt { get; private set; }

    private ServiceOrder() { }

    /// <summary>
    /// Creates a new service order with Pending status.
    /// </summary>
    /// <param name="customerName">Full name of the customer requesting service.</param>
    /// <param name="equipment">Name or model of the equipment requiring service.</param>
    /// <param name="problemDescription">Detailed description of the reported issue.</param>
    /// <returns>A new <see cref="ServiceOrder"/> instance in Pending status.</returns>
    public static ServiceOrder Create(string customerName, string equipment, string problemDescription)
    {
        return new ServiceOrder
        {
            Id = Guid.NewGuid(),
            CustomerName = customerName,
            Equipment = equipment,
            ProblemDescription = problemDescription,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Assigns a technician to this order and transitions status to InProgress.
    /// </summary>
    /// <param name="technicianId">Identifier of the technician being assigned.</param>
    public void AssignTechnician(Guid technicianId)
    {
        TechnicianId = technicianId;
        Status = OrderStatus.InProgress;
    }

    /// <summary>
    /// Closes the service order, marking the service as completed.
    /// </summary>
    public void Close()
    {
        Status = OrderStatus.Closed;
    }
}
