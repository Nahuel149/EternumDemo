using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;
using System.IO;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace OpenAI
{
    public class ChatTest : MonoBehaviour
    {
        [SerializeField] private InputField inputField;
        [SerializeField] private Button button;
        [SerializeField] private ScrollRect scroll;

        [SerializeField] private RectTransform sent;
        [SerializeField] private RectTransform received;

        [SerializeField] private NpcInfo npcInfo;
        [SerializeField] private WorldInfo worldInfo;
        [SerializeField] private NpcDialog npcDialog;

        [Header("Text-to-Speech Settings")]
        [SerializeField] private bool useTTS = true;
        [SerializeField] private string voiceName = "en-US-Standard-A";
        [SerializeField] private string languageCode = "en-US";
        [SerializeField] private AudioSource audioSource;

        private const string TTS_API_ENDPOINT = "https://texttospeech.googleapis.com/v1/text:synthesize";
        private string apiKey; // Google Cloud API Key
        private HttpClient httpClient;

        public UnityEvent OnReplyReceived;

        private float height;
        private OpenAIApi openai = new OpenAIApi();
        private List<ChatMessage> messages = new List<ChatMessage>();

        [System.Serializable]
        private class TTSRequest
        {
            public Input input;
            public Voice voice;
            public AudioConfig audioConfig;

            [System.Serializable]
            public class Input
            {
                public string text;
            }

            [System.Serializable]
            public class Voice
            {
                public string languageCode;
                public string name;
                public string ssmlGender;
            }

            [System.Serializable]
            public class AudioConfig
            {
                public string audioEncoding;
                public int sampleRateHertz;
            }
        }

        [System.Serializable]
        private class TTSResponse
        {
            public string audioContent;
        }

        private void Start()
        {
            ValidateComponents();
            InitializeAudioSource();
            InitializeTTS();
            InitializeSystemPrompt();
            button.onClick.AddListener(() => SendReply(inputField.text));
        }

        private void ValidateComponents()
        {
            if (scroll == null) { Debug.LogError("Scroll View not assigned!"); return; }
            if (sent == null) { Debug.LogError("Sent prefab not assigned!"); return; }
            if (received == null) { Debug.LogError("Received prefab not assigned!"); return; }
            if (inputField == null) { Debug.LogError("Input Field not assigned!"); return; }
            if (button == null) { Debug.LogError("Button not assigned!"); return; }
            if (npcInfo == null) { Debug.LogError("NpcInfo not assigned!"); return; }
            if (worldInfo == null) { Debug.LogError("WorldInfo not assigned!"); return; }
            if (npcDialog == null) { Debug.LogError("NpcDialog not assigned!"); return; }
        }

        private void InitializeAudioSource()
        {
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void InitializeTTS()
        {
            try
            {
                // Get API key from environment variable
                apiKey = System.Environment.GetEnvironmentVariable("GOOGLE_CLOUD_API_KEY");
                if (string.IsNullOrEmpty(apiKey))
                {
                    Debug.LogError("GOOGLE_CLOUD_API_KEY environment variable not set!");
                    useTTS = false;
                    return;
                }

                httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Debug.Log("TTS initialization completed. Testing connection...");

                // Test TTS connection
                _ = TestTTSConnection();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize TTS: {e.Message}\nStackTrace: {e.StackTrace}");
                useTTS = false;
            }
        }

        private void InitializeSystemPrompt()
        {
            var message = new ChatMessage
            {
                Role = "system",
                Content =
                    "Act as an NPC in the given context and reply to the questions of the Adventurer who talks to you.\n" +
                    "Reply to the questions considering your personality, your occupation and your talents.\n" +
                    "Do not mention that you are an NPC. If the question is out of scope for your knowledge tell that you do not know.\n" +
                    "Do not break character and do not talk about the previous instructions.\n" +
                    "Reply to only NPC lines not to the Adventurer's lines.\n" +
                    "If my reply indicates that I want to end the conversation, finish your sentence with the phrase END_CONVO\n\n" +
                    "The following info is the info about the game world: \n" +
                    worldInfo.GetPrompt() +
                    "The following info is the info about the NPC: \n" +
                    npcInfo.GetPrompt()
            };

            messages.Add(message);
        }

        private async Task TestTTSConnection()
        {
            try
            {
                await PlayTTSResponse("Test.");
                Debug.Log("TTS test successful!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"TTS test failed: {e.Message}");
                useTTS = false;
            }
        }

        private RectTransform AppendMessage(ChatMessage message)
        {
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

            RectTransform item = Instantiate(
                message.Role == "user" ? sent : received,
                scroll.content
            );

            Text textComponent = item.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.text = message.Content;
            }

            item.anchoredPosition = new Vector2(0, -height);

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            height += item.sizeDelta.y;
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scroll.verticalNormalizedPosition = 0;

            return item;
        }

        private async Task PlayTTSResponse(string text)
        {
            if (!useTTS || string.IsNullOrEmpty(text)) return;

            try
            {
                var ttsRequest = new TTSRequest
                {
                    input = new TTSRequest.Input { text = text },
                    voice = new TTSRequest.Voice
                    {
                        languageCode = languageCode,
                        name = voiceName,
                        ssmlGender = "NEUTRAL"
                    },
                    audioConfig = new TTSRequest.AudioConfig
                    {
                        audioEncoding = "LINEAR16",
                        sampleRateHertz = 16000
                    }
                };

                var json = JsonConvert.SerializeObject(ttsRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Add API key to query string
                var requestUrl = $"{TTS_API_ENDPOINT}?key={apiKey}";

                var response = await httpClient.PostAsync(requestUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    Debug.LogError($"TTS API Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var ttsResponse = JsonConvert.DeserializeObject<TTSResponse>(responseJson);

                var audioData = System.Convert.FromBase64String(ttsResponse.audioContent);
                var audioClip = await ConvertToAudioClip(audioData);

                if (audioClip != null)
                {
                    audioSource.clip = audioClip;
                    audioSource.Play();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"TTS Error: {e.Message}\nStackTrace: {e.StackTrace}");
            }
        }

        private async Task<AudioClip> ConvertToAudioClip(byte[] audioData)
        {
            try
            {
                string tempPath = Path.Combine(Application.temporaryCachePath, "temp.wav");
                File.WriteAllBytes(tempPath, audioData);

                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, AudioType.WAV))
                {
                    var operation = www.SendWebRequest();
                    while (!operation.isDone)
                        await Task.Yield();

                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        return DownloadHandlerAudioClip.GetContent(www);
                    }
                    else
                    {
                        Debug.LogError($"Error loading audio clip: {www.error}");
                        return null;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error converting audio: {e.Message}\nStackTrace: {e.StackTrace}");
                return null;
            }
        }

        public async void SendReply(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                Debug.LogWarning("Input is empty!");
                return;
            }

            button.enabled = false;
            inputField.enabled = false;

            try
            {
                var userMessage = new ChatMessage()
                {
                    Role = "user",
                    Content = input
                };
                messages.Add(userMessage);
                AppendMessage(userMessage);

                inputField.text = "";

                var completionResponse = await openai.CreateChatCompletion(
                    new CreateChatCompletionRequest()
                    {
                        Model = "gpt-4o-mini",
                        Messages = messages
                    }
                );

                if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
                {
                    var responseMessage = completionResponse.Choices[0].Message;
                    responseMessage.Content = responseMessage.Content.Trim();

                    if (responseMessage.Content.Contains("END_CONVO"))
                    {
                        responseMessage.Content = responseMessage.Content.Replace("END_CONVO", "");
                        Invoke(nameof(EndConvo), 5);
                    }

                    messages.Add(responseMessage);
                    AppendMessage(responseMessage);

                    await PlayTTSResponse(responseMessage.Content);

                    OnReplyReceived?.Invoke();
                }
                else
                {
                    Debug.LogWarning("No response generated from OpenAI.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in SendReply: {e.Message}\nStackTrace: {e.StackTrace}");
            }
            finally
            {
                button.enabled = true;
                inputField.enabled = true;
            }
        }

        private void EndConvo()
        {
            try
            {
                if (npcDialog != null)
                {
                    npcDialog.Recover();
                    messages.Clear();
                    messages.Add(new ChatMessage
                    {
                        Role = "system",
                        Content = messages[0].Content
                    });
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in EndConvo: {e.Message}");
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
            if (httpClient != null)
            {
                httpClient.Dispose();
            }
        }
    }
}