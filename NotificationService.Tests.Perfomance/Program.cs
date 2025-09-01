using System.Net.Http.Json;
using NBomber.CSharp;
using NotificationService.Tests.Perfomance.Models;

namespace NotificationService.Tests.Perfomance;

class Program
{
    static void Main()
    {
        var baseUrl = "https://localhost:44388/Listing";
        var client = new HttpClient();

        var userListing = new UserListingPairDto
        {
            User = new UserDto 
            { 
                Id = Guid.NewGuid(), 
                Name = "John", 
                LastName = "Doe", 
                Email = "blabla@gmail.com", 
                TelegramId = null 
            },
            Listing = new ListingDto
            {
                Id = Guid.NewGuid(),
                Price = 100000m,
                AreaMeterSqr = 50,
                Rooms = 2,
                Floor = 3,
                IsFurnished = true,
                PetsAllowed = false,
                HasBalcony = true,
                HasAppliances = true,
                Url = "https://example.com",
                CreatedAt = DateTime.UtcNow
            }
        };

        var userListings = new[] { userListing, userListing, userListing };

        var singleScenario = Scenario.Create("NotifySingle", async _ =>
            {
                var response = await client.PostAsJsonAsync(baseUrl + "/notify-single", userListing);
                return response.IsSuccessStatusCode 
                    ? Response.Ok()
                    : Response.Fail();
            })
            .WithLoadSimulations(
                Simulation.KeepConstant(30, TimeSpan.FromSeconds(10))
            );

        var multipleScenario = Scenario.Create("NotifyMultiple", async _ =>
            {
                var response = await client.PostAsJsonAsync(baseUrl + "/notify-multiple", userListings);
                return response.IsSuccessStatusCode 
                    ? Response.Ok()
                    : Response.Fail();
            })
            .WithLoadSimulations(
                Simulation.KeepConstant(20, TimeSpan.FromSeconds(10))
            );

        NBomberRunner
            .RegisterScenarios(singleScenario, multipleScenario)
            .Run();
    }
}