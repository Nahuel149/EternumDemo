using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using OpenAI;

namespace Backend
{
    public class OpenAIBackendApi : IDisposable
    {
        private readonly string baseUrl;
        private readonly HttpClient httpClient;

        public OpenAIBackendApi()
        {
            this.baseUrl = ServerConfig.GetServerURL();
            this.httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<ChatCompletionResponse> CreateChatCompletion(CreateChatCompletionRequest request)
        {
            try
            {
                if (request.Messages == null || request.Messages.Count == 0)
                {
                    throw new ArgumentException("Messages cannot be null or empty");
                }

                var requestData = new
                {
                    model = request.Model ?? "gpt-4o-mini",
                    messages = request.Messages.Select(m => new
                    {
                        role = m.Role.ToLower(),
                        content = m.Content
                    }).ToList(),
                    temperature = request.Temperature
                };

                var json = JsonConvert.SerializeObject(requestData, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                });
                Debug.Log($"Chat Request:\n{json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{baseUrl}/api/chat", content);
                var responseString = await response.Content.ReadAsStringAsync();

                Debug.Log($"Chat Response Status: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    Debug.Log($"Chat Response Content (truncated):\n{responseString.Substring(0, Math.Min(500, responseString.Length))}...");
                }
                else
                {
                    Debug.LogError($"Chat Error Response:\n{responseString}");
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API Error: {response.StatusCode} - {responseString}");
                }

                return JsonConvert.DeserializeObject<ChatCompletionResponse>(responseString);
            }
            catch (Exception e)
            {
                Debug.LogError($"Chat API Error:\n{e.Message}\nStack Trace:\n{e.StackTrace}");
                throw;
            }
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }

    [System.Serializable]
    public class ChatCompletionResponse
    {
        public List<Choice> Choices { get; set; }
        public string Id { get; set; }
        public string Object { get; set; }
        public long Created { get; set; }
        public string Model { get; set; }
        public Usage Usage { get; set; }
    }

    [System.Serializable]
    public class Choice
    {
        public ChatMessage Message { get; set; }
        public int Index { get; set; }
        public string FinishReason { get; set; }
    }

    [System.Serializable]
    public class ChatMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

    [System.Serializable]
    public class Usage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }

    [System.Serializable]
    public class CreateChatCompletionRequest
    {
        public string Model { get; set; } = "gpt-4o-mini";
        public List<ChatMessage> Messages { get; set; }
        public float Temperature { get; set; } = 0.7f;
        public int? MaxTokens { get; set; }
        public float? TopP { get; set; }
        public float? FrequencyPenalty { get; set; }
        public float? PresencePenalty { get; set; }
    }
}