using Core.Entities.Models;

namespace Application.Data.Interfaces;

public interface IEmailProviderService
{
    Task PrepareAndSendEmail(MailModel mailRequest);
}