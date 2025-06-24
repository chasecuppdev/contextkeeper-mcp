using System;
using System.Collections.Generic;

namespace TestLibrary.Models
{
    /// <summary>
    /// Represents a product in the system.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Gets or sets the product identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the product name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the product description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the product price.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the product SKU (Stock Keeping Unit).
        /// </summary>
        public string Sku { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the quantity in stock.
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Gets or sets the product category.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the product tags.
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets whether the product is available.
        /// </summary>
        public bool IsAvailable { get; set; } = true;

        /// <summary>
        /// Gets or sets the date when the product was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date when the product was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the Product class.
        /// </summary>
        public Product()
        {
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the Product class with specified values.
        /// </summary>
        /// <param name="name">The product name</param>
        /// <param name="price">The product price</param>
        /// <param name="sku">The product SKU</param>
        public Product(string name, decimal price, string sku) : this()
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Price = price;
            Sku = sku ?? throw new ArgumentNullException(nameof(sku));
        }

        /// <summary>
        /// Calculates the total value of stock.
        /// </summary>
        /// <returns>The total value (price * quantity)</returns>
        public decimal CalculateStockValue()
        {
            return Price * StockQuantity;
        }

        /// <summary>
        /// Checks if the product is in stock.
        /// </summary>
        /// <returns>True if stock quantity is greater than zero; otherwise false</returns>
        public bool IsInStock()
        {
            return StockQuantity > 0 && IsAvailable;
        }

        /// <summary>
        /// Applies a discount to the product price.
        /// </summary>
        /// <param name="discountPercentage">The discount percentage (0-100)</param>
        /// <returns>The discounted price</returns>
        public decimal GetDiscountedPrice(decimal discountPercentage)
        {
            if (discountPercentage < 0 || discountPercentage > 100)
                throw new ArgumentOutOfRangeException(nameof(discountPercentage), "Discount must be between 0 and 100.");

            return Price * (1 - discountPercentage / 100);
        }

        /// <summary>
        /// Returns a string representation of the product.
        /// </summary>
        /// <returns>A string containing the product name and SKU</returns>
        public override string ToString()
        {
            return $"{Name} (SKU: {Sku})";
        }

        /// <summary>
        /// Validates whether the product data is valid.
        /// </summary>
        /// <returns>True if valid; otherwise false</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   !string.IsNullOrWhiteSpace(Sku) &&
                   Price >= 0 &&
                   StockQuantity >= 0;
        }

        /// <summary>
        /// Creates a copy of the product.
        /// </summary>
        /// <returns>A new Product instance with the same values</returns>
        public Product Clone()
        {
            return new Product
            {
                Id = Id,
                Name = Name,
                Description = Description,
                Price = Price,
                Sku = Sku,
                StockQuantity = StockQuantity,
                Category = Category,
                Tags = new List<string>(Tags),
                IsAvailable = IsAvailable,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt
            };
        }
    }
}