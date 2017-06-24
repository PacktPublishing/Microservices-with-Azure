namespace AgeCalculatorService.Controllers
{
    using System;
    using System.Web.Http;

    public class AgeController : ApiController
    {
        public string Get(DateTime dateOfBirth)
        {
            return $"You are {DateTime.UtcNow.Year - dateOfBirth.Year} years old.";
        }
    }
}