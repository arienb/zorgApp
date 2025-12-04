using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace zorgApp.Models
{
    public class Patient : INotifyPropertyChanged
    {
        private string? _profileImageUrl;

        public string? FirebaseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Age { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string UniqueCode { get; set; } = string.Empty;
        
        // Profile fields
        public string? CallName { get; set; }
        public string? Hobbies { get; set; }
        public string? FavoriteFood { get; set; }
        public string? FavoriteFilm { get; set; }
        public string? FavoriteMusic { get; set; }
        public string? Work { get; set; }
        
        public string? ProfileImageUrl
        {
            get => _profileImageUrl;
            set
            {
                if (_profileImageUrl != value)
                {
                    _profileImageUrl = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}