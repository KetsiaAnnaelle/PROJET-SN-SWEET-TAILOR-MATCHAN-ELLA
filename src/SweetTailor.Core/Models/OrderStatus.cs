namespace SweetTailor.Core.Models;

public enum OrderStatus
{
    Recue,
    Couture,
    Essayage,
    Prete,
    Livree
}

public enum StepState
{
    Done,
    Now,
    Todo
}

public class OrderStep
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public StepState State { get; set; } = StepState.Todo;
}
