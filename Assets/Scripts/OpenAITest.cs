using UnityEngine;
using System.Collections.Generic;
using Backend;

public class OpenAITest : MonoBehaviour
{
    private OpenAIBackendApi openai;

    void Start()
    {
        openai = new OpenAIBackendApi();
        SendRequest();
    }

    private async void SendRequest()
    {
        var req = new CreateChatCompletionRequest
        {
            Model = "gpt-4o-mini",
            Messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Role = "user",
                    Content = "Hello!"
                }
            },
            Temperature = 0.7f
        };

        try
        {
            var res = await openai.CreateChatCompletion(req);
            Debug.Log("Response: " + res.Choices[0].Message.Content);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error: " + e.Message);
            Debug.LogError("Stack Trace: " + e.StackTrace);
        }
    }

    private void OnDestroy()
    {
        openai?.Dispose();
    }
}