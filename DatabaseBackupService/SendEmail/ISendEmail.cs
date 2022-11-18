namespace SendEmail
{
    public interface ISendEmail
    {
        Task<bool> SendMessage(string subject, string body);
    }
}
