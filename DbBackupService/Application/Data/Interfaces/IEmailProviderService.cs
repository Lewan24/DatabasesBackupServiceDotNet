namespace Application.Data.Interfaces;

public interface IEmailProviderService
{
    Task SendMail();
}