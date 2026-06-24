namespace CalSystem.Domain.Entities;

public class Technician
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;

    private Technician() { }

    public static Technician Create(string name, string email)
    {
        return new Technician
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email
        };
    }
}
