using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using zorgApp.Models;

namespace zorgApp.Services
{
    public class FirebaseService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private const string FirebaseUrl = "https://zorgapp-316e8-default-rtdb.europe-west1.firebasedatabase.app";
        private const string DiaryItemsNode = "diaryItems";

        public FirebaseService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(FirebaseUrl)
            };
        }

        public async Task<List<DiaryItem>> GetDiaryItemsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"/{DiaryItemsNode}.json");

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Firebase GetDiaryItems Error: {response.StatusCode}");
                    return new List<DiaryItem>();
                }

                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content) || content == "null")
                {
                    return new List<DiaryItem>();
                }

                var items = JsonSerializer.Deserialize<Dictionary<string, DiaryItem>>(content);

                if (items == null)
                {
                    return new List<DiaryItem>();
                }

                return items.Select(kvp => new DiaryItem
                {
                    Id = kvp.Key,
                    Title = kvp.Value.Title,
                    Description = kvp.Value.Description,
                    Timestamp = kvp.Value.Timestamp,
                    CreatedBy = kvp.Value.CreatedBy
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase GetDiaryItems Error: {ex.Message}");
                return new List<DiaryItem>();
            }
        }

        public async Task<string> AddDiaryItemAsync(DiaryItem item)
        {
            try
            {
                var itemToAdd = new
                {
                    item.Title,
                    item.Description,
                    item.Timestamp,
                    item.CreatedBy
                };

                var response = await _httpClient.PostAsJsonAsync($"/{DiaryItemsNode}.json", itemToAdd);
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
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content) || content == "null")
                {
                    return null;
                }

                var item = JsonSerializer.Deserialize<DiaryItem>(content);

                if (item != null)
                {
                    item.Id = id;
                }

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

        private class FirebasePostResponse
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}
