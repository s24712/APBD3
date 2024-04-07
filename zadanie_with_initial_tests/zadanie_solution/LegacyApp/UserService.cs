using System;

namespace LegacyApp
{
    public class UserService
    {
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!IsValidInput(firstName, lastName, email) || !IsAdult(dateOfBirth))
            {
                return false;
            }

            var client = new ClientRepository().GetById(clientId);
            var user = CreateUser(firstName, lastName, email, dateOfBirth, client);

            SetCreditLimitBasedOnClientType(user, client.Type);
            
            if (IsCreditLimitBelowThreshold(user))
            {
                return false;
            }

            UserDataAccess.AddUser(user);
            return true;
        }

        private bool IsValidInput(string firstName, string lastName, string email)
        {
            return !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && email.Contains("@") && email.Contains(".");
        }

        private bool IsAdult(DateTime dateOfBirth)
        {
            var age = CalculateAge(dateOfBirth);
            return age >= 21;
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
            {
                age--;
            }
            return age;
        }

        private User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, Client client)
        {
            return new User
            {
                FirstName = firstName,
                LastName = lastName,
                EmailAddress = email,
                DateOfBirth = dateOfBirth,
                Client = client
            };
        }

        private void SetCreditLimitBasedOnClientType(User user, string clientType)
        {
            switch (clientType)
            {
                case "VeryImportantClient":
                    user.HasCreditLimit = false;
                    break;
                case "ImportantClient":
                    ApplyCreditLimit(user, multiplier: 2);
                    break;
                default:
                    ApplyCreditLimit(user, multiplier: 1);
                    break;
            }
        }

        private void ApplyCreditLimit(User user, int multiplier)
        {
            var creditService = new UserCreditService();
            int creditLimit = creditService.GetCreditLimit(user.LastName, user.DateOfBirth);
            user.CreditLimit = creditLimit * multiplier;
            user.HasCreditLimit = true;
        }

        private bool IsCreditLimitBelowThreshold(User user)
        {
            return user.HasCreditLimit && user.CreditLimit < 500;
        }
    }
}
