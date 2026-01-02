using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using zorgApp.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace zorgApp.Services
{
    public class FirebaseService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        private const string FirebaseUrl =
            "https://zorgapp-316e8-default-rtdb.europe-west1.firebasedatabase.app/";

        private const string BaseNode = "zorgApp";
        private const string DepartmentsNode = $"{BaseNode}/departments";
        private const string PatientsNode = $"{BaseNode}/patients";

        public FirebaseService()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(FirebaseUrl) };

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // -------------------------------------- 
        //  DEPARTMENTS / NURSES CRUD
        // -------------------------------------- 

        public async Task<List<Nurse>> GetDepartmentsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"/{DepartmentsNode}.json");
                if (!response.IsSuccessStatusCode)
                    return new List<Nurse>();

                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content) || content == "null")
                    return new List<Nurse>();

                var departments =
                    JsonSerializer.Deserialize<Dictionary<string, Nurse>>(
                        content,
                        _jsonOptions
                    );
                if (departments == null)
                    return new List<Nurse>();

                var list = new List<Nurse>();
                foreach (var kvp in departments)
                {
                    var dept = kvp.Value;
                    dept.FirebaseId = kvp.Key;
                    list.Add(dept);
                }
                return list;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase GetDepartments Error: {ex.Message}");
                return new List<Nurse>();
            }
        }

        public async Task<Nurse?> GetDepartmentByNameAsync(string departmentName)
        {
            try
            {
                var allDepartments = await GetDepartmentsAsync();
                return allDepartments.FirstOrDefault(d => 
                    d.DepartmentName?.Equals(departmentName, StringComparison.OrdinalIgnoreCase) == true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase GetDepartmentByName Error: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> AddDepartmentAsync(Nurse nurse)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"AddDepartmentAsync called for: {nurse.DepartmentName}");
                
                var departmentData = new
                {
                    nurse.DepartmentName,
                    nurse.Password,
                    nurse.CreatedAt
                };

                var json = JsonSerializer.Serialize(departmentData);
                System.Diagnostics.Debug.WriteLine($"JSON to send: {json}");

                var response = await _httpClient.PostAsJsonAsync($"/{DepartmentsNode}.json", departmentData);
                
                System.Diagnostics.Debug.WriteLine($"Response status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Error response: {errorContent}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Success response: {content}");
                
                var result = JsonSerializer.Deserialize<FirebasePostResponse>(content);
                return result?.Name;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase AddDepartment Error: {ex}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw om de exception in ViewModel op te vangen
            }
        }

        public async Task<bool> ValidateDepartmentCredentialsAsync(string departmentName, string password)
        {
            try
            {
                var department = await GetDepartmentByNameAsync(departmentName);
                return department != null && department.Password == password;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase ValidateDepartmentCredentials Error: {ex.Message}");
                return false;
            }
        }

        // -------------------------------------- 
        //  PATIENTS CRUD (per afdeling)
        // -------------------------------------- 

        public async Task<List<Patient>> GetPatientsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"/{PatientsNode}.json");
                if (!response.IsSuccessStatusCode)
                    return new List<Patient>();

                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content) || content == "null")
                    return new List<Patient>();

                var patients =
                    JsonSerializer.Deserialize<Dictionary<string, Patient>>(
                        content,
                        _jsonOptions
                    );
                if (patients == null)
                    return new List<Patient>();

                var list = new List<Patient>();
                foreach (var kvp in patients)
                {
                    var p = kvp.Value;
                    p.FirebaseId = kvp.Key;
                    list.Add(p);
                }
                return list;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase GetPatients Error: {ex.Message}");
                return new List<Patient>();
            }
        }

        public async Task<List<Patient>> GetPatientsByDepartmentAsync(string departmentName)
        {
            try
            {
                var allPatients = await GetPatientsAsync();
                return allPatients.Where(p => 
                    p.DepartmentName?.Equals(departmentName, StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase GetPatientsByDepartment Error: {ex.Message}");
                return new List<Patient>();
            }
        }

        public async Task<Patient?> GetPatientByUniqueCodeAsync(string uniqueCode)
        {
            try
            {
                var allPatients = await GetPatientsAsync();
                return allPatients.FirstOrDefault(p => 
                    p.UniqueCode?.Equals(uniqueCode, StringComparison.OrdinalIgnoreCase) == true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase GetPatientByUniqueCode Error: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> AddPatientAsync(Patient patient)
        {
            try
            {
                if (string.IsNullOrEmpty(patient.UniqueCode))
                {
                    patient.UniqueCode = GenerateUniqueCode();
                }

                var patientData = new
                {
                    patient.Name,
                    patient.Email,
                    patient.Age,
                    patient.RoomNumber,
                    patient.UniqueCode,
                    patient.ProfileImageUrl,
                    patient.DepartmentName
                };

                var response = await _httpClient.PostAsJsonAsync($"/{PatientsNode}.json", patientData);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FirebasePostResponse>(content);
                return result?.Name;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase AddPatient Error: {ex.Message}");
                return null;
            }
        }

        // -------------------------------------- 
        //  PATIENT INFO UPDATEN & DELETEN (+PDF)
        // -------------------------------------- 
        public async Task UpdatePatientAsync(string id, Patient patient)
        {
            try
            {
                var toUpdate = new
                {
                    patient.Name,
                    patient.Email,
                    patient.Age,
                    patient.RoomNumber,
                    patient.UniqueCode,
                    patient.ProfileImageUrl,
                    patient.DepartmentName
                };

                var json = JsonSerializer.Serialize(toUpdate);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"/{PatientsNode}/{id}.json", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase UpdatePatient Error: {ex.Message}");
                throw;
            }
        }

        public async Task UpdatePatientProfileAsync(string id, Patient patient, Stream? imageStream = null, string? fileName = null) 
        {
            try
            {
                // Upload profile image if provided
                if (imageStream != null && !string.IsNullOrEmpty(fileName))
                {
                    var imageUrl = await UploadImageAsync(imageStream, $"profiles/{fileName}");
                    patient.ProfileImageUrl = imageUrl;
                }

                // ⚠️ KRITIEKE FIX: Gebruik PATCH in plaats van PUT om alleen specifieke velden te updaten
                // PUT vervangt het hele object en verwijdert nested data zoals diaryItems!
                var toUpdate = new Dictionary<string, object?>
                {
                    { "email", patient.Email },
                    { "age", patient.Age },
                    { "callName", patient.CallName },
                    { "hobbies", patient.Hobbies },
                    { "work", patient.Work },
                    { "favoriteFood", patient.FavoriteFood },
                    { "favoriteFilm", patient.FavoriteFilm },
                    { "favoriteMusic", patient.FavoriteMusic },
                    { "profileImageUrl", patient.ProfileImageUrl }
                };

                var json = JsonSerializer.Serialize(toUpdate);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // ✅ GEBRUIK PATCH IN PLAATS VAN PUT
                var response = await _httpClient.PatchAsync($"/{PatientsNode}/{id}.json", content);
                response.EnsureSuccessStatusCode();
                
                System.Diagnostics.Debug.WriteLine("✅ Patient profile updated successfully with PATCH!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase UpdatePatientProfile Error: {ex.Message}");
                throw;
            }
        }

        public async Task DeletePatientAsync(string id)
        {
            try
            {
                // Delete patient and all their diary items
                var response = await _httpClient.DeleteAsync($"/{PatientsNode}/{id}.json");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase DeletePatient Error: {ex.Message}");
                throw;
            }
        }

        public async Task ExportDiaryPdfAndSendEmailAsync(string patientId)
        {
            var url = "https://europe-west1-zorgapp-316e8.cloudfunctions.net/exportDiaryPdf";

            var payload = new Dictionary<string, string>
            {
                { "patientId", patientId }
            };
            var json = JsonSerializer.Serialize(payload);

            using var client = new HttpClient();
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);

            var result = await response.Content.ReadAsStringAsync();
            Debug.WriteLine("HTTP RESPONSE = " + await response.Content.ReadAsStringAsync());

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine("WARNING: Function returned non-success status: " + response.StatusCode);
            }
        }

        // -------------------------------------- 
        //  DIARY ITEMS (nested under patients)
        // -------------------------------------- 

        public async Task<List<DiaryItem>> GetDiaryItemsAsync(string patientId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/{PatientsNode}/{patientId}/diaryItems.json");
                if (!response.IsSuccessStatusCode)
                    return new List<DiaryItem>();

                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content) || content == "null")
                    return new List<DiaryItem>();

                var items =
                    JsonSerializer.Deserialize<Dictionary<string, DiaryItem>>(
                        content,
                        _jsonOptions
                    );
                if (items == null) return new List<DiaryItem>();

                var result = new List<DiaryItem>();
                foreach (var kvp in items)
                {
                    var item = kvp.Value;
                    item.Id = kvp.Key;
                    item.PatientId = patientId;
                    result.Add(item);
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase GetDiaryItems Error: {ex.Message}");
                return new List<DiaryItem>();
            }
        }

        public async Task<string> AddDiaryItemAsync(
            string patientId,
            DiaryItem item,
            Stream? imageStream = null,
            string? fileName = null
        )
        {
            try
            {
                if (imageStream != null && !string.IsNullOrEmpty(fileName))
                {
                    var imageUrl = await UploadImageAsync(imageStream, fileName);
                    item.ImageUrl = imageUrl;
                }

                var itemToAdd = new
                {
                    item.Title,
                    item.Description,
                    item.ImageUrl,
                    item.Timestamp,
                    item.CreatedBy
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"/{PatientsNode}/{patientId}/diaryItems.json",
                    itemToAdd
                );
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FirebasePostResponse>(content);
                return result?.Name ?? string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase AddDiaryItem Error: {ex.Message}");
                throw;
            }
        }

        public async Task<DiaryItem?> GetDiaryItemByIdAsync(string patientId, string itemId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/{PatientsNode}/{patientId}/diaryItems/{itemId}.json");
                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content) || content == "null")
                    return null;

                var item = JsonSerializer.Deserialize<DiaryItem>(content, _jsonOptions);
                if (item != null)
                {
                    item.Id = itemId;
                    item.PatientId = patientId;
                }

                return item;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase GetDiaryItemById Error: {ex.Message}");
                return null;
            }
        }

        public async Task UpdateDiaryItemAsync(string patientId, DiaryItem item)
        {
            try
            {
                var itemToUpdate = new
                {
                    item.Title,
                    item.Description,
                    item.Timestamp,
                    item.CreatedBy,
                    item.ImageUrl
                };

                var json = JsonSerializer.Serialize(itemToUpdate);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"/{PatientsNode}/{patientId}/diaryItems/{item.Id}.json", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase UpdateDiaryItem Error: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteDiaryItemAsync(string patientId, string itemId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/{PatientsNode}/{patientId}/diaryItems/{itemId}.json");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase DeleteDiaryItem Error: {ex.Message}");
                throw;
            }
        }

        // -------------------------------------- 
        //  IMAGES
        // -------------------------------------- 
        public async Task<string?> UploadImageAsync(Stream imageStream, string fileName)
        {
            try
            {
                var storageUrl =
                    $"https://firebasestorage.googleapis.com/v0/b/zorgapp-316e8.firebasestorage.app/o/{Uri.EscapeDataString(fileName)}?uploadType=media";

                var content = new StreamContent(imageStream);
                content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                var response = await _httpClient.PostAsync(storageUrl, content);
                response.EnsureSuccessStatusCode();

                return
                    $"https://firebasestorage.googleapis.com/v0/b/zorgapp-316e8.firebasestorage.app/o/{Uri.EscapeDataString(fileName)}?alt=media";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UploadImageAsync Error: {ex.Message}");
                return null;
            }
        }


        // -------------------------------------- 
        //  NOTIFICATIONS (MEERDERE PER PATIËNT)
        // -------------------------------------- 
        public async Task<string> AddNotificationAsync(Notification notification)
        {
            try
            {
                var notificationData = new
                {
                    notification.PatientId,
                    notification.Message,
                    notification.Timestamp,
                    notification.IsRead,
                    notification.PatientFamilyDeviceToken
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"/{PatientsNode}/{notification.PatientId}/notifications.json",
                    notificationData
                );
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FirebasePostResponse>(content);
                return result?.Name ?? string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase AddNotification Error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Notification>> GetNotificationsAsync(string patientId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/{PatientsNode}/{patientId}/notifications.json");
                if (!response.IsSuccessStatusCode)
                    return new List<Notification>();

                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content) || content == "null")
                    return new List<Notification>();

                var notifications = JsonSerializer.Deserialize<Dictionary<string, Notification>>(
                    content,
                    _jsonOptions
                );
                if (notifications == null)
                    return new List<Notification>();

                var result = new List<Notification>();
                foreach (var kvp in notifications)
                {
                    var notification = kvp.Value;
                    notification.Id = kvp.Key;
                    result.Add(notification);
                }

                // Sorteer op timestamp (nieuwste eerst)
                return result.OrderByDescending(n => n.Timestamp).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase GetNotifications Error: {ex.Message}");
                return new List<Notification>();
            }
        }

        public async Task MarkNotificationAsReadAsync(string patientId, string notificationId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"MarkNotificationAsReadAsync called - PatientId: {patientId}, NotificationId: {notificationId}");
                
                // Eerst de notificatie ophalen
                var getResponse = await _httpClient.GetAsync($"/{PatientsNode}/{patientId}/notifications/{notificationId}.json");
                
                if (!getResponse.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to get notification: {getResponse.StatusCode}");
                    return;
                }

                var content = await getResponse.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Current notification data: {content}");
                
                var notification = JsonSerializer.Deserialize<Notification>(content, _jsonOptions);
                
                if (notification == null)
                {
                    System.Diagnostics.Debug.WriteLine("Notification is null after deserialization");
                    return;
                }

                // Update IsRead naar true
                notification.IsRead = true;
                notification.Id = notificationId; // Zorg dat ID behouden blijft
                
                // Hele notificatie terugschrijven met PUT
                var updatedNotificationData = new
                {
                    notification.PatientId,
                    notification.Message,
                    notification.Timestamp,
                    IsRead = true, // Expliciet true
                    notification.PatientFamilyDeviceToken
                };

                var json = JsonSerializer.Serialize(updatedNotificationData);
                System.Diagnostics.Debug.WriteLine($"Updated notification JSON: {json}");
                
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                
                var putResponse = await _httpClient.PutAsync(
                    $"/{PatientsNode}/{patientId}/notifications/{notificationId}.json",
                    stringContent
                );
                
                System.Diagnostics.Debug.WriteLine($"PUT response status: {putResponse.StatusCode}");
                
                if (!putResponse.IsSuccessStatusCode)
                {
                    var errorContent = await putResponse.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"PUT error response: {errorContent}");
                }
                
                putResponse.EnsureSuccessStatusCode();
                
                System.Diagnostics.Debug.WriteLine("✅ Notification marked as read successfully!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase MarkNotificationAsRead Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task DeleteNotificationAsync(string patientId, string notificationId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(
                    $"/{PatientsNode}/{patientId}/notifications/{notificationId}.json"
                );
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase DeleteNotification Error: {ex.Message}");
                throw;
            }
        }

        /* pushnotification (latere implementatie)
        public async Task SendPushNotificationAsync(Notification notification)
        {
            var payload = new
            {
                to = notification.PatientFamilyDeviceToken,
                notification = new
                {
                    title = "Nieuwe notificatie",
                    body = notification.Message
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("key", "YOUR_FCM_SERVER_KEY");

            var response = await _httpClient.PostAsync("https://fcm.googleapis.com/fcm/send", content);
            response.EnsureSuccessStatusCode();
        }
        */
        // -------------------------------------- 
        //  HELPER METHODS
        // -------------------------------------- 
        private string GenerateUniqueCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public string GeneratePassword()
        {
            const string chars = "0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private class FirebasePostResponse
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}