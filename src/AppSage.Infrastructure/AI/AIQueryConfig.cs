namespace AppSage.Infrastructure.AI
{
    public class AIQueryConfig
    {
        public string ModelId { get; set; }
        public int MaxTokens { get; set; }
        public double Temperature { get; set; }
        public double TopP { get; set; }
        public double TopK { get; set; }
    }
}
