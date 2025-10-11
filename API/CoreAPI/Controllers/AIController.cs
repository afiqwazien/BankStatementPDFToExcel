using Microsoft.AspNetCore.Mvc;
using YourNamespace.Services;
using OpenAI.Chat;
using OpenAI;
using System;
using System.ClientModel;

namespace BankStatementPDFToExcel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiController : ControllerBase
    {
        [HttpPost("testAI")]
        public async Task<IActionResult> TestAI(string input)
        {
            // Initialize the OpenAI client with your API key and URI
            // var client = new OpenAIClient("sk-2d01b75f178d4928b948b5bdae372e66", "https://api.deepseek.com");

            // Initialize the OpenAI client with your API key
            // var client = new OpenAIClient("sk-2d01b75f178d4928b948b5bdae372e66");

            // // Example 1: Generate text using a specific model
            // var prompt = "Write a short story about a character who discovers a hidden world.";
            // var response = await client.  Completions.CreateCompletionAsync(
            //     new CompletionRequest
            //     {
            //         Model = "text-davinci-003",
            //         Prompt = prompt,
            //         MaxTokens = 2048,
            //         Temperature = 0.7,
            //     });
            // Console.WriteLine("Generated text:");
            // Console.WriteLine(response.Choices[0].Text);


            // OpenAIClient client = new OpenAIClient(new ApiKeyCredential("sk-2d01b75f178d4928b948b5bdae372e66"), new OpenAIClientOptions()
            // {
            //     Endpoint = new Uri("https://api.deepseek.com"),
            // });

            ChatClient client = new ChatClient("deepseek-chat",new ApiKeyCredential("sk-2d01b75f178d4928b948b5bdae372e66"), new OpenAIClientOptions()
            {
                Endpoint = new Uri("https://api.deepseek.com"),
            });
            
            ChatCompletion completion = await client.CompleteChatAsync("Say 'this is a test.'");

            Console.WriteLine($"[ASSISTANT]: {completion.Content[0].Text}");

            return Ok();
        }


    }

}