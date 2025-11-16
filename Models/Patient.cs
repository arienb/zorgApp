namespace zorgApp.Models;

public class Patient
{
    public string? FirebaseId { get; set; }
    public string? Name { get; set; }        // "name"
    public string? Email { get; set; }       // "email"
    public int? Age { get; set; }            // "age"
    public string? RoomNumber { get; set; }  // "roomNumber"
    public string? Condition { get; set; }   // "condition"
}