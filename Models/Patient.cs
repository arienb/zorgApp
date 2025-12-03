namespace zorgApp.Models;

public class Patient
{
    public string? FirebaseId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public int? Age { get; set; }
    public required string RoomNumber { get; set; }
    public string? UniqueCode { get; set; }

    // Additional profile properties
    public string? CallName { get; set; }
    public string? Hobbies { get; set; }
    public string? Work { get; set; }
    public string? FavoriteFood { get; set; }
    public string? FavoriteFilm { get; set; }
    public string? FavoriteMusic { get; set; }
}