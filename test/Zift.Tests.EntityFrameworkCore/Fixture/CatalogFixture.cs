namespace Zift.Fixture;

public static class CatalogFixture
{
    public static IReadOnlyList<Category> Create()
    {
        return
        [
            new Category
            {
                Id = Ids.Categories.Electronics,
                Name = "Electronics",
                Products =
                {
                    new Product
                    {
                        Id = Ids.Products.Smartphone,
                        Name = "Smartphone",
                        Price = 999.99m,
                        Reviews =
                        {
                            new Review
                            {
                                Id = Ids.Reviews.Smartphone_Positive,
                                Content = "Great phone!",
                                Rating = 5,
                                DatePosted = new DateTime(2024, 1, 10),
                                Author = new User
                                {
                                    Name = "John Doe",
                                    Email = "john.doe@example.com"
                                }
                            },
                            new Review
                            {
                                Id = Ids.Reviews.Smartphone_Neutral,
                                Content = "Good value for the price.",
                                Rating = 4,
                                DatePosted = new DateTime(2024, 1, 12),
                                Author = new User
                                {
                                    Name = "Jane Smith",
                                    Email = "jane.smith@example.com"
                                }
                            }
                        }
                    },
                    new Product
                    {
                        Id = Ids.Products.Laptop,
                        Name = "Laptop",
                        Price = 1299.99m,
                        Reviews =
                        {
                            new Review
                            {
                                Id = Ids.Reviews.Laptop_Positive,
                                Content = "Very fast and reliable.",
                                Rating = 5,
                                DatePosted = new DateTime(2024, 2, 5),
                                Author = new User
                                {
                                    Name = "Alice Johnson",
                                    Email = "alice.johnson@example.com"
                                }
                            },
                            new Review
                            {
                                Id = Ids.Reviews.Laptop_Negative,
                                Content = "Battery life could be better.",
                                Rating = 3,
                                DatePosted = new DateTime(2024, 2, 8),
                                Author = new User
                                {
                                    Name = "Bob Brown",
                                    Email = "bob.brown@example.com"
                                }
                            },
                            new Review
                            {
                                Id = Ids.Reviews.Laptop_Anonymous,
                                Content = "Anonymous review.",
                                Rating = 2,
                                DatePosted = new DateTime(2024, 3, 1),
                                Author = null
                            }
                        }
                    }
                }
            },
            new Category
            {
                Id = Ids.Categories.Books,
                Name = "Books",
                Products =
                {
                    new Product
                    {
                        Id = Ids.Products.GreatGatsby,
                        Name = "The Great Gatsby",
                        Price = 15.99m,
                        Reviews =
                        {
                            new Review
                            {
                                Id = Ids.Reviews.Book_Positive,
                                Content = "A timeless classic.",
                                Rating = 5,
                                DatePosted = new DateTime(2023, 11, 20),
                                Author = new User
                                {
                                    Name = "John Green",
                                    Email = "john.green@example.com"
                                }
                            }
                        }
                    },
                    new Product
                    {
                        Id = Ids.Products.NineteenEightyFour,
                        Name = "1984",
                        Price = 12.99m,
                        Reviews =
                        {
                            new Review
                            {
                                Id = Ids.Reviews.Book_NullRating,
                                Content = "More relevant today than ever.",
                                Rating = null,
                                DatePosted = new DateTime(2023, 12, 1),
                                Author = new User
                                {
                                    Name = "George Orwell",
                                    Email = "george.orwell@example.com"
                                }
                            }
                        }
                    }
                }
            }
        ];
    }

    public static class Ids
    {
        public static class Categories
        {
            public static readonly Guid Electronics =
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

            public static readonly Guid Books =
                Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        }

        public static class Products
        {
            public static readonly Guid Smartphone =
                Guid.Parse("11111111-1111-1111-1111-111111111111");

            public static readonly Guid Laptop =
                Guid.Parse("22222222-2222-2222-2222-222222222222");

            public static readonly Guid GreatGatsby =
                Guid.Parse("33333333-3333-3333-3333-333333333333");

            public static readonly Guid NineteenEightyFour =
                Guid.Parse("44444444-4444-4444-4444-444444444444");
        }

        public static class Reviews
        {
            public static readonly Guid Smartphone_Positive =
                Guid.Parse("aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa");

            public static readonly Guid Smartphone_Neutral =
                Guid.Parse("bbbbbbbb-1111-1111-1111-bbbbbbbbbbbb");

            public static readonly Guid Laptop_Positive =
                Guid.Parse("cccccccc-2222-2222-2222-cccccccccccc");

            public static readonly Guid Laptop_Negative =
                Guid.Parse("dddddddd-2222-2222-2222-dddddddddddd");

            public static readonly Guid Laptop_Anonymous =
                Guid.Parse("eeeeeeee-2222-2222-2222-eeeeeeeeeeee");

            public static readonly Guid Book_Positive =
                Guid.Parse("eeeeeeee-3333-3333-3333-eeeeeeeeeeee");

            public static readonly Guid Book_NullRating =
                Guid.Parse("ffffffff-4444-4444-4444-ffffffffffff");
        }
    }
}
