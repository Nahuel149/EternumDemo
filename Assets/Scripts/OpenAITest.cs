using UnityEngine;
using OpenAI;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

public class OpenAITest : MonoBehaviour
{
    private OpenAIApi openai;

    void Start()
    {
        openai = new OpenAIApi(); // Automatically loads credentials from auth.json
        SendRequest();
        // Uncomment the line below to test streaming requests
        // SendStreamRequest();
    }

    private async void SendRequest()
    {
        var req = new CreateChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Role = "user",
                    Content = "Hello!"
                }
            }
        };

        try
        {
            var res = await openai.CreateChatCompletion(req);
            Debug.Log("Response: " + res.Choices[0].Message.Content);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error: " + e.Message);
        }
    }

    private void SendStreamRequest()
    {
        var req = new CreateChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Role = "user",
                    Content = "Write a 100 word long short story in La Fontaine style."
                }
            },
            Temperature = 0.7f,
        };

        openai.CreateChatCompletionAsync(req,
            (responses) =>
            {
                var result = string.Join("", responses.Select(response => response.Choices[0].Delta.Content));
                Debug.Log(result);
            },
            () =>
            {
                Debug.Log("completed");
            },
            new CancellationTokenSource()
        );
    }
}