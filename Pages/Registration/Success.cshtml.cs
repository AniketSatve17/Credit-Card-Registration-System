using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace CreditCardRegistration.Pages.Registration
{
    public class SuccessModel : PageModel
    {
        public SuccessData RegistrationData { get; set; }

        public void OnGet()
        {
            if (TempData["RegistrationData"] is string jsonData)
            {
                RegistrationData = JsonConvert.DeserializeObject<SuccessData>(jsonData);
            }
        }

        public class SuccessData
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string CardType { get; set; }
            public string CardNumberLast4 { get; set; }
            public string SubmissionDate { get; set; }
        }
    }
}