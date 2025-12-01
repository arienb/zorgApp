namespace zorgApp.Models;

public class Patient
{
    public string? FirebaseId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public int? Age { get; set; }
    public required string RoomNumber { get; set; }
    public string? UniqueCode { get; set; } 
}