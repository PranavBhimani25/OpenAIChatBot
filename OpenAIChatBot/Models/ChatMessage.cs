namespace OpenAIChatBot.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string UserMessage { get; set; }
        public string BotReply { get; set; }
    }
}
