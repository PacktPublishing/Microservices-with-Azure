namespace Contracts
{
    using System.Threading.Tasks;

    using Microsoft.ServiceFabric.Services.Remoting;

    public interface IWeatherService : IService
    {
        Task<WeatherReport> GetReport(string postCode);
    }
}
