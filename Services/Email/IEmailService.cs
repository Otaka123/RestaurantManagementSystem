﻿namespace UsersApp.Services.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string recipientEmail, string subject, string content);
    }
}
