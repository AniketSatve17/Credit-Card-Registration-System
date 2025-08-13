using CreditCardRegistration.Models;
using CreditCardRegistration.Pages;

namespace CreditCardRegistration.Extensions
{
    public static class RegistrationInputExtensions
    {
        public static User ToUserEntity(this RegisterModel.RegistrationInput input)
        {
            return new User
            {
                FirstName = input.FirstName,
                LastName = input.LastName,
                Email = input.Email,
                PhoneNumber = input.PhoneNumber,
                DateOfBirth = input.DateOfBirth,
                Gender = input.Gender
                // Add other property mappings as needed
            };
        }
    }
}