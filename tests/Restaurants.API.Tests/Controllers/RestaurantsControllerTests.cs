using FluentAssertions;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Restaurants.Application.Restaurants.Dtos;
using Restaurants.Domain.Entities;
using Restaurants.Domain.Repositories;
using Restaurants.Infrastucture.Seeders;
using System.Net.Http.Json;
using Xunit;

namespace Restaurants.API.Tests.Controllers;

public class RestaurantsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    // Para crear una instancia de la aplicación web para pruebas
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IRestaurantsRepository> _restaurantsRepositoryMock = new ();
    private readonly Mock<IRestaurantSeeder> _restaurantsSeederMock = new();
    public RestaurantsControllerTests(WebApplicationFactory<Program> factory)
    {
        // Agregar un evaluador de políticas falso para evitar problemas de autorización en las pruebas
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
                services.Replace(ServiceDescriptor.Scoped(typeof(IRestaurantsRepository), 
                    _ => _restaurantsRepositoryMock.Object));
                services.Replace(ServiceDescriptor.Scoped(typeof(IRestaurantSeeder),
                    _ => _restaurantsSeederMock.Object));
            });
        });
    }

    [Fact()]
    public async Task GetAll_ForValidRequest_Returns200Ok ()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var results = await client.GetAsync("/api/restaurants?pageNumber=1&pageSize=10");

        // Assert
        results.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact()]
    public async Task GetAll_ForInvalidRequest_Returns400BadRequest ()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var results = await client.GetAsync("/api/restaurants");

        // Assert
        results.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact()]
    public async Task GetById_ForNonExistingId_ShouldReturn404NotFound ()
    {
        // Arrange
        var id = 1234;
        _restaurantsRepositoryMock.Setup(m => m.GetByIdAsync(id)).ReturnsAsync((Restaurant?)null);
        var client = _factory.CreateClient();

        // Act
        var results = await client.GetAsync($"/api/restaurants/{id}");

        // Assert
        results.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact()]
    public async Task GetById_ForExistingId_ShouldReturn200Ok ()
    {
        // Arrange
        var id = 25;
        var restaurant = new Restaurant ()
        {
            Id = id, 
            Name = "Test Restaurant",
            Description = "A test restaurant for unit testing",
        };
        _restaurantsRepositoryMock.Setup(m => m.GetByIdAsync(id)).ReturnsAsync(restaurant);
        var client = _factory.CreateClient();

        // Act
        var results = await client.GetAsync($"/api/restaurants/{id}");
        var restaurantDto = await results.Content.ReadFromJsonAsync<RestaurantDto>();

        // Assert
        results.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        restaurantDto.Should().NotBeNull();
        restaurantDto.Name.Should().Be("Test Restaurant");
        restaurantDto.Description.Should().Be("A test restaurant for unit testing");
    }
}