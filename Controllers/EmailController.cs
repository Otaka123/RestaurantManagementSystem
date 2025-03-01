using Microsoft.AspNetCore.Mvc;
using UsersApp.Services.Email;
using UsersApp.ViewModels;

namespace UsersApp.Controllers
{
    public class EmailController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        private readonly EmailService _emailService;

        public EmailController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult SendEmail()
        {
            return View(new EmailViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(EmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _emailService.SendEmailAsync(model.RecipientEmail, model.Subject, model.Content);
                ViewBag.Message = "Email sent successfully!";
            }
            return View(model);
        }
    }
}

