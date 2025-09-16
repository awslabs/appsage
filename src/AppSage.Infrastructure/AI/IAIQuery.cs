namespace AppSage.Infrastructure.AI
{
    public interface IAIQuery
    {
        string Invoke(string prompt);
        string Invoke(string prompt,AIQueryConfig queryConfig);
    }
}
