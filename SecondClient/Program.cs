using Chat_Project;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Threading.Tasks;

namespace SecondClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Write("Enter your name:");
            var username = Console.ReadLine();
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new ChatService.ChatServiceClient(channel);

            using (var chat = client.Join())
            {
                _ = Task.Run(async () =>
                {
                    while (await chat.ResponseStream.MoveNext())
                    {
                        var response = chat.ResponseStream.Current;
                        Console.WriteLine($"{response.User}:{response.Text}");
                    }
                });




                await chat.RequestStream.WriteAsync(new Message { User = username, Text = $"{username} has joined the chat" });
                string line;
                while ((line = Console.ReadLine()) != null)
                {
                    if (line.ToUpper() == "EXIT")
                    {
                        break;
                    }
                    await chat.RequestStream.WriteAsync(new Message { User = username, Text = line });
                }
                await chat.RequestStream.CompleteAsync();
            }
            Console.WriteLine("He(Her)is leaving");
            await channel.ShutdownAsync();
        }
    }
}
