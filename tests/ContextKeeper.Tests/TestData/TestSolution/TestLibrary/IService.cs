using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestLibrary
{
    /// <summary>
    /// Defines the contract for a generic service layer.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface IService<T> where T : class
    {
        /// <summary>
        /// Gets an entity by its identifier with validation.
        /// </summary>
        /// <param name="id">The entity identifier</param>
        /// <returns>The entity if found</returns>
        /// <exception cref="EntityNotFoundException">Thrown when entity is not found</exception>
        Task<T> GetByIdAsync(int id);

        /// <summary>
        /// Gets all entities with optional filtering.
        /// </summary>
        /// <param name="filter">Optional filter predicate</param>
        /// <returns>A collection of entities</returns>
        Task<IEnumerable<T>> GetAllAsync(Func<T, bool>? filter = null);

        /// <summary>
        /// Creates a new entity with validation.
        /// </summary>
        /// <param name="entity">The entity to create</param>
        /// <returns>The created entity</returns>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        Task<T> CreateAsync(T entity);

        /// <summary>
        /// Updates an existing entity with validation.
        /// </summary>
        /// <param name="id">The entity identifier</param>
        /// <param name="entity">The updated entity data</param>
        /// <returns>The updated entity</returns>
        /// <exception cref="EntityNotFoundException">Thrown when entity is not found</exception>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        Task<T> UpdateAsync(int id, T entity);

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="id">The entity identifier</param>
        /// <exception cref="EntityNotFoundException">Thrown when entity is not found</exception>
        Task DeleteAsync(int id);

        /// <summary>
        /// Validates an entity.
        /// </summary>
        /// <param name="entity">The entity to validate</param>
        /// <returns>True if valid; otherwise false</returns>
        bool Validate(T entity);
    }

    /// <summary>
    /// Exception thrown when an entity is not found.
    /// </summary>
    public class EntityNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of EntityNotFoundException.
        /// </summary>
        /// <param name="message">The exception message</param>
        public EntityNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception thrown when validation fails.
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of ValidationException.
        /// </summary>
        /// <param name="message">The exception message</param>
        public ValidationException(string message) : base(message) { }
    }
}