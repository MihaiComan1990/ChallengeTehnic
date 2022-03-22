using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;

namespace WebApplication.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public string UserID => (string)TempData[nameof(UserID)];
        public string TempPassword => (string)TempData[nameof(TempPassword)];

        public string Password => (string)TempData[nameof(Password)];

        public string CurrentTime => (string)TempData[nameof(CurrentTime)];
        public string ExpirationTime => (string)TempData[nameof(ExpirationTime)];

        public string LoginResult => (string)TempData[nameof(LoginResult)];


        public void OnGet()
        {
        }

        public IActionResult OnPost([FromForm] string userID,
                                    [FromForm] string password,
                                    [FromForm] string expirationTime,
                                    string answer)
        {
            TempData[nameof(UserID)] = userID;
            TempData[nameof(CurrentTime)] = DateTime.Now.TimeOfDay.ToString();

            if (String.IsNullOrEmpty(expirationTime) == false)
            {
                TempData[nameof(ExpirationTime)] = expirationTime;
            }

            try
            {
                if (answer == "Generate")
                {
                    TempData[nameof(TempPassword)] = PasswordManager.GetTempPassword(userID, 
                                                                                     DateTime.Now, 
                                                                                     out DateTime? newExpireTime);
                    TempData[nameof(ExpirationTime)] = newExpireTime.Value.TimeOfDay.ToString();
                }

                if (answer == "Login")
                {

                    bool loginResult = PasswordManager.LoginUser(userID, password);
                    if (loginResult == true)
                    {
                        TempData[nameof(LoginResult)] = "Succes";
                    }
                    else
                    {
                        TempData[nameof(LoginResult)] = "Login failed";
                    }
                }

            }
            catch (Exception ex)
            {
                TempData[nameof(LoginResult)] = "One or more errors occurred";
            }

            return RedirectToPage("Index");
        }
    }
}
