using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestLibrary;
using TestLibrary.Models;

namespace TestApp.Controllers
{
    /// <summary>
    /// Controller for managing product operations.
    /// </summary>
    public class ProductController : BaseController
    {
        private readonly ProductService _productService;

        /// <summary>
        /// Initializes a new instance of the ProductController class.
        /// </summary>
        /// <param name="productRepository">The product repository</param>
        public ProductController(IRepository<Product> productRepository)
        {
            _productService = new ProductService(productRepository);
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="name">Product name</param>
        /// <param name="price">Product price</param>
        /// <param name="sku">Product SKU</param>
        /// <param name="category">Product category</param>
        /// <param name="stockQuantity">Initial stock quantity</param>
        /// <returns>The created product</returns>
        public async Task<Product> CreateProductAsync(string name, decimal price, string sku, string category, int stockQuantity)
        {
            LogAction($"Creating product: {name}");
            var product = new Product(name, price, sku)
            {
                Category = category,
                StockQuantity = stockQuantity
            };
            return await _productService.CreateAsync(product);
        }

        /// <summary>
        /// Gets a product by ID.
        /// </summary>
        /// <param name="id">The product ID</param>
        /// <returns>The product if found</returns>
        public async Task<Product> GetProductByIdAsync(int id)
        {
            LogAction($"Getting product by ID: {id}");
            return await _productService.GetByIdAsync(id);
        }

        /// <summary>
        /// Gets all products.
        /// </summary>
        /// <returns>Collection of all products</returns>
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            LogAction("Getting all products");
            return await _productService.GetAllAsync();
        }

        /// <summary>
        /// Gets products by category.
        /// </summary>
        /// <param name="category">The category name</param>
        /// <returns>Collection of products in the category</returns>
        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
        {
            LogAction($"Getting products by category: {category}");
            return await _productService.GetAllAsync(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets products that are in stock.
        /// </summary>
        /// <returns>Collection of products in stock</returns>
        public async Task<IEnumerable<Product>> GetProductsInStockAsync()
        {
            LogAction("Getting products in stock");
            return await _productService.GetAllAsync(p => p.IsInStock());
        }

        /// <summary>
        /// Gets products within a price range.
        /// </summary>
        /// <param name="minPrice">Minimum price</param>
        /// <param name="maxPrice">Maximum price</param>
        /// <returns>Collection of products within the price range</returns>
        public async Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            LogAction($"Getting products between ${minPrice} and ${maxPrice}");
            return await _productService.GetAllAsync(p => p.Price >= minPrice && p.Price <= maxPrice);
        }

        /// <summary>
        /// Updates a product.
        /// </summary>
        /// <param name="id">The product ID</param>
        /// <param name="product">The updated product data</param>
        /// <returns>The updated product</returns>
        public async Task<Product> UpdateProductAsync(int id, Product product)
        {
            LogAction($"Updating product: {id}");
            product.UpdatedAt = DateTime.UtcNow;
            return await _productService.UpdateAsync(id, product);
        }

        /// <summary>
        /// Updates product stock quantity.
        /// </summary>
        /// <param name="id">The product ID</param>
        /// <param name="newQuantity">The new stock quantity</param>
        /// <returns>The updated product</returns>
        public async Task<Product> UpdateStockAsync(int id, int newQuantity)
        {
            LogAction($"Updating stock for product {id} to {newQuantity}");
            var product = await _productService.GetByIdAsync(id);
            product.StockQuantity = newQuantity;
            return await _productService.UpdateAsync(id, product);
        }

        /// <summary>
        /// Deletes a product.
        /// </summary>
        /// <param name="id">The product ID</param>
        public async Task DeleteProductAsync(int id)
        {
            LogAction($"Deleting product: {id}");
            await _productService.DeleteAsync(id);
        }

        /// <summary>
        /// Calculates discounted price for a product.
        /// </summary>
        /// <param name="product">The product</param>
        /// <param name="discountPercentage">Discount percentage</param>
        /// <returns>The discounted price</returns>
        public decimal CalculateDiscountedPrice(Product product, decimal discountPercentage)
        {
            LogAction($"Calculating {discountPercentage}% discount for {product.Name}");
            return product.GetDiscountedPrice(discountPercentage);
        }

        /// <summary>
        /// Gets total inventory value.
        /// </summary>
        /// <returns>Total value of all products in stock</returns>
        public async Task<decimal> GetTotalInventoryValueAsync()
        {
            LogAction("Calculating total inventory value");
            var products = await _productService.GetAllAsync();
            return products.Sum(p => p.CalculateStockValue());
        }

        /// <summary>
        /// Service implementation for product operations.
        /// </summary>
        private class ProductService : Service<Product>
        {
            public ProductService(IRepository<Product> repository) : base(repository)
            {
            }

            public override bool Validate(Product entity)
            {
                return entity != null && entity.IsValid();
            }

            protected override bool ValidateExtended(Product entity, out List<string> errors)
            {
                errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.Name))
                    errors.Add("Product name is required.");

                if (string.IsNullOrWhiteSpace(entity.Sku))
                    errors.Add("SKU is required.");

                if (entity.Price < 0)
                    errors.Add("Price cannot be negative.");

                if (entity.StockQuantity < 0)
                    errors.Add("Stock quantity cannot be negative.");

                if (entity.Name.Length > 100)
                    errors.Add("Product name cannot exceed 100 characters.");

                return errors.Count == 0;
            }

            public override async Task<Product> CreateAsync(Product entity)
            {
                // Check for duplicate SKU
                var existing = await GetAllAsync(p => p.Sku == entity.Sku);
                if (existing.Any())
                    throw new ValidationException($"Product with SKU '{entity.Sku}' already exists.");

                return await base.CreateAsync(entity);
            }
        }
    }
}