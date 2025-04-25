namespace Zift.SharedFixture.Models;

public static class Catalog
{
    public static ICollection<Category> Categories =>
    [
        new Category
        {
            Id = Guid.NewGuid(),
            Name = "Electronics",
            Products =
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Smartphone",
                    Price = 999.99m,
                    Reviews =
                    {
                        new Review { Id = Guid.NewGuid(), Author = new User { Name = "John Doe", Email = "john.doe@example.com" }, Content = "Great phone!", Rating = 5 },
                        new Review { Id = Guid.NewGuid(), Author = new User { Name = "Jane Smith", Email = "jane.smith@example.com" }, Content = "Good value for the price.", Rating = 4 }
                    }
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Laptop",
                    Price = 1299.99m,
                    Reviews =
                    {
                        new Review { Id = Guid.NewGuid(), Author = new User { Name = "Alice Johnson", Email = "alice.johnson@example.com" }, Content = "Very fast and reliable.", Rating = 5 },
                        new Review { Id = Guid.NewGuid(), Author = new User { Name = "Bob Brown", Email = "bob.brown@example.com" }, Content = "Battery life could be better.", Rating = 3 }
                    }
                }
            }
        },
        new Category
        {
            Id = Guid.NewGuid(),
            Name = "Home Appliances",
            Products =
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Refrigerator",
                    Price = 599.99m,
                    Reviews =
                    {
                        new Review { Id = Guid.NewGuid(), Author = new User { Name = "Charlie Davis", Email = "charlie.davis@example.com" }, Content = "Spacious and energy-efficient.", Rating = 4 }
                    }
                }
            }
        },
        new Category
        {
            Id = Guid.NewGuid(),
            Name = "Clothing",
            Products =
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "T-Shirt",
                    Price = 19.99m,
                    Reviews =
                    {
                        new Review { Id = Guid.NewGuid(), Author = new User { Name = "Dave Lee", Email = "dave.lee@example.com" }, Content = "Comfortable and fits well.", Rating = 5 },
                        new Review { Id = Guid.NewGuid(), Author = new User { Name = "Emma Stone", Email = "emma.stone@example.com" }, Content = "Loved the color options.", Rating = 4 }
                    }
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Jeans",
                    Price = 49.99m,
                    Reviews =
                    {
                        new Review { Id = Guid.NewGuid(), Author = new User { Name = "Ryan Gosling", Email = "ryan.gosling@example.com" }, Content = "Great quality but runs a bit tight.", Rating = 4 },
                        new Review { Id = Guid.NewGuid(), Author = new User { Name = "Sophia Turner", Email = "sophia.turner@example.com" }, Content = "Perfect fit and very comfortable.", Rating = 5 }
                    }
                }
            }
        },
        new Category
        {
            Id = Guid.NewGuid(),
            Name = "Books",
            Products =
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "The Great Gatsby",
                    Price = 15.99m,
                    Reviews =
                    {
                        new Review { Id = Guid.NewGuid(), Author = new User { Name = "John Green", Email = "john.green@example.com" }, Content = "A timeless classic.", Rating = 5 }
                    }
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "1984",
                    Price = 12.99m,
                    Reviews =
                    {
                        new Review { Id = Guid.NewGuid(), Author = new User { Name = "George Orwell", Email = "george.orwell@example.com" }, Content = "More relevant today than ever.", Rating = 5 },
                        new Review { Id = Guid.NewGuid(), Author = new User { Name = "Aldous Huxley", Email = "aldous.huxley@example.com" }, Content = "A chilling dystopian tale.", Rating = 5 }
                    }
                }
            }
        }
    ];
}
