using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SmartShop.API.Controllers;
using SmartShop.API.Data;
using SmartShop.API.DTOs;
using SmartShop.API.Entities;
using Xunit;

namespace SmartShop.Tests;

public class ReportsControllerTests
{
    private SmartShopDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<SmartShopDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new SmartShopDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task GetReviewReport_ReturnsAllProducts_IncludingProductsWithZeroReviews()
    {
        // Arrange
        using var context = CreateInMemoryDbContext("TestDb_Report_ZeroReviews");

        // Seed 2 products: 1 with reviews, 1 without
        var product1 = new Product
        {
            ProductId = 10,
            ProductName = "Test Product 1",
            Category = "Phone",
            Price = 10000000m,
            Features = "Fast, Sleek"
        };
        var product2 = new Product
        {
            ProductId = 20,
            ProductName = "Test Product 2 (No reviews)",
            Category = "Laptop",
            Price = 20000000m,
            Features = "Lightweight"
        };

        context.Products.AddRange(product1, product2);

        // Add 3 reviews for product 1: 2 Positive (5, 4 stars), 1 Negative (2 stars)
        context.Reviews.AddRange(
            new Review
            {
                ProductId = 10,
                CustomerName = "Alice",
                Content = "Great device",
                Rating = 5,
                Sentiment = "Positive",
                Score = 0.95,
                Summary = "Great"
            },
            new Review
            {
                ProductId = 10,
                CustomerName = "Bob",
                Content = "Very nice",
                Rating = 4,
                Sentiment = "Positive",
                Score = 0.88,
                Summary = "Nice"
            },
            new Review
            {
                ProductId = 10,
                CustomerName = "Charlie",
                Content = "Battery issues",
                Rating = 2,
                Sentiment = "Negative",
                Score = 0.75,
                Summary = "Battery poor"
            }
        );

        await context.SaveChangesAsync();

        var controller = new ReportsController(context, NullLogger<ReportsController>.Instance);

        // Act
        var actionResult = await controller.GetReviewReport(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var report = Assert.IsAssignableFrom<List<ReviewReportItem>>(okResult.Value);

        // Must contain all products sorted by ProductId
        Assert.True(report.Count >= 2);

        var row1 = report.FirstOrDefault(r => r.ProductName == "Test Product 1");
        Assert.NotNull(row1);
        Assert.Equal(3, row1.TotalReviews);
        Assert.Equal(2, row1.Positive);
        Assert.Equal(1, row1.Negative);
        Assert.Equal(0, row1.Neutral);
        // Average rating = (5 + 4 + 2) / 3 = 11 / 3 = 3.666... -> 3.7 rounded to 1 decimal place
        Assert.Equal(3.7, row1.AverageRating);

        var row2 = report.FirstOrDefault(r => r.ProductName == "Test Product 2 (No reviews)");
        Assert.NotNull(row2);
        Assert.Equal(0, row2.TotalReviews);
        Assert.Equal(0, row2.Positive);
        Assert.Equal(0, row2.Negative);
        Assert.Equal(0, row2.Neutral);
        // Products without reviews must have 0.0 average rating
        Assert.Equal(0.0, row2.AverageRating);
    }
}
