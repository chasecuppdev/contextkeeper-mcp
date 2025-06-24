using System;
using System.Threading.Tasks;
using TestLibrary;
using TestLibrary.Models;
using TestApp.Controllers;

namespace TestApp
{
    /// <summary>
    /// The main application entry point.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main method that starts the application.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Exit code</returns>
        public static async Task<int> Main(string[] args)
        {
            Console.WriteLine("Test Application Starting...");

            try
            {
                // Initialize repositories
                var userRepository = new Repository<User>(u => u.Id);
                var productRepository = new Repository<Product>(p => p.Id);

                // Initialize controllers
                var userController = new UserController(userRepository);
                var productController = new ProductController(productRepository);

                // Demo user operations
                await DemoUserOperations(userController);

                // Demo product operations
                await DemoProductOperations(productController);

                Console.WriteLine("\nApplication completed successfully!");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                return 1;
            }
        }

        /// <summary>
        /// Demonstrates user-related operations.
        /// </summary>
        /// <param name="controller">The user controller</param>
        private static async Task DemoUserOperations(UserController controller)
        {
            Console.WriteLine("\n--- User Operations Demo ---");

            // Create users
            var user1 = await controller.CreateUserAsync("john_doe", "john@example.com", "John Doe");
            Console.WriteLine($"Created user: {user1}");

            var user2 = await controller.CreateUserAsync("jane_smith", "jane@example.com", "Jane Smith");
            Console.WriteLine($"Created user: {user2}");

            // List all users
            var users = await controller.GetAllUsersAsync();
            Console.WriteLine($"\nTotal users: {users.Count()}");

            // Get user by ID
            var foundUser = await controller.GetUserByIdAsync(user1.Id);
            Console.WriteLine($"\nFound user by ID: {foundUser}");

            // Update user
            user1.Role = UserRole.Administrator;
            var updated = await controller.UpdateUserAsync(user1.Id, user1);
            Console.WriteLine($"\nUpdated user role to: {updated.Role}");

            // Delete user
            await controller.DeleteUserAsync(user2.Id);
            Console.WriteLine($"\nDeleted user: {user2.Username}");
        }

        /// <summary>
        /// Demonstrates product-related operations.
        /// </summary>
        /// <param name="controller">The product controller</param>
        private static async Task DemoProductOperations(ProductController controller)
        {
            Console.WriteLine("\n--- Product Operations Demo ---");

            // Create products
            var product1 = await controller.CreateProductAsync("Laptop", 999.99m, "TECH-001", "Electronics", 10);
            Console.WriteLine($"Created product: {product1}");

            var product2 = await controller.CreateProductAsync("Mouse", 29.99m, "TECH-002", "Electronics", 50);
            Console.WriteLine($"Created product: {product2}");

            // List all products
            var products = await controller.GetAllProductsAsync();
            Console.WriteLine($"\nTotal products: {products.Count()}");

            // Get products by category
            var electronics = await controller.GetProductsByCategoryAsync("Electronics");
            Console.WriteLine($"\nProducts in Electronics category: {electronics.Count()}");

            // Check stock
            var inStock = await controller.GetProductsInStockAsync();
            Console.WriteLine($"\nProducts in stock: {inStock.Count()}");

            // Apply discount
            var discountedPrice = controller.CalculateDiscountedPrice(product1, 10);
            Console.WriteLine($"\n{product1.Name} with 10% discount: ${discountedPrice:F2}");

            // Update stock
            await controller.UpdateStockAsync(product1.Id, 5);
            Console.WriteLine($"\nUpdated {product1.Name} stock to 5 units");
        }

        /// <summary>
        /// Initializes the application configuration.
        /// </summary>
        /// <returns>Configuration settings</returns>
        public static AppConfiguration InitializeConfiguration()
        {
            return new AppConfiguration
            {
                AppName = "Test Application",
                Version = "1.0.0",
                MaxUsers = 100,
                MaxProducts = 1000
            };
        }
    }

    /// <summary>
    /// Application configuration settings.
    /// </summary>
    public class AppConfiguration
    {
        /// <summary>
        /// Gets or sets the application name.
        /// </summary>
        public string AppName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the application version.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximum number of users.
        /// </summary>
        public int MaxUsers { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of products.
        /// </summary>
        public int MaxProducts { get; set; }
    }
}