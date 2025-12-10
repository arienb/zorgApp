namespace zorgApp.Models
{
    public class Nurse
    {
        public string? FirebaseId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}