using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestLibrary
{
    /// <summary>
    /// Defines the contract for a generic repository pattern.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets an entity by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier</param>
        /// <returns>The entity if found; otherwise null</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns>A collection of all entities</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Adds a new entity.
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <returns>The added entity</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity">The entity to update</param>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Checks if an entity exists.
        /// </summary>
        /// <param name="id">The entity identifier</param>
        /// <returns>True if the entity exists; otherwise false</returns>
        Task<bool> ExistsAsync(int id);
    }
}