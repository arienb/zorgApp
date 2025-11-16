using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using zorgApp.Models;

namespace zorgApp.Services
{
    public class FirebaseService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        private const string FirebaseUrl =
            "https://zorgapp-316e8-default-rtdb.europe-west1.firebasedatabase.app/";

        private const string BaseNode = "zorgApp";
        private const string DiaryItemsNode = $"{BaseNode}/diaryItems";
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
        //  DIARY ITEMS (unchanged)
        // -------------------------------------- 

        public async Task<List<DiaryItem>> GetDiaryItemsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"/{DiaryItemsNode}.json");
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
                    $"/{DiaryItemsNode}.json",
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

        public async Task<DiaryItem?> GetDiaryItemByIdAsync(string id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/{DiaryItemsNode}/{id}.json");
                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content) || content == "null")
                    return null;

                var item = JsonSerializer.Deserialize<DiaryItem>(content, _jsonOptions);
                if (item != null)
                    item.Id = id;

                return item;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase GetDiaryItemById Error: {ex.Message}");
                return null;
            }
        }

        public async Task UpdateDiaryItemAsync(string id, DiaryItem item)
        {
            try
            {
                var itemToUpdate = new
                {
                    item.Title,
                    item.Description,
                    item.Timestamp,
                    item.CreatedBy
                };

                var json = JsonSerializer.Serialize(itemToUpdate);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"/{DiaryItemsNode}/{id}.json", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase UpdateDiaryItem Error: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteDiaryItemAsync(string id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/{DiaryItemsNode}/{id}.json");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase DeleteDiaryItem Error: {ex.Message}");
                throw;
            }
        }

        // -------------------------------------- 
        //  PATIENTS CRUD
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
                    p.FirebaseId = kvp.Key; // 👈 Add this field to your Patient model
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

        public async Task<string?> AddPatientAsync(Patient patient)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"/{PatientsNode}.json", patient);
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

        public async Task UpdatePatientAsync(string id, Patient patient)
        {
            try
            {
                // We'll update only some fields
                var toUpdate = new
                {
                    patient.Name,
                    patient.Email
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

        public async Task DeletePatientAsync(string id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/{PatientsNode}/{id}.json");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase DeletePatient Error: {ex.Message}");
                throw;
            }
        }

        // -------------------------------------- 
        //  IMAGES (unchanged)
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

        private class FirebasePostResponse
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}