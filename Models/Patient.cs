namespace zorgApp.Models;

public class Patient
{
    public string? FirebaseId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public int? Age { get; set; }
    public string? RoomNumber { get; set; }
    public string? UniqueCode { get; set; } 
}