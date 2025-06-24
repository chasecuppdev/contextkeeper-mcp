using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestLibrary
{
    /// <summary>
    /// A generic service implementation with business logic.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public abstract class Service<T> : IService<T> where T : class
    {
        private readonly IRepository<T> _repository;

        /// <summary>
        /// Initializes a new instance of the Service class.
        /// </summary>
        /// <param name="repository">The repository instance</param>
        protected Service(IRepository<T> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <inheritdoc/>
        public virtual async Task<T> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new EntityNotFoundException($"Entity with ID {id} not found.");
            
            return entity;
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<T>> GetAllAsync(Func<T, bool>? filter = null)
        {
            var entities = await _repository.GetAllAsync();
            return filter != null ? entities.Where(filter) : entities;
        }

        /// <inheritdoc/>
        public virtual async Task<T> CreateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (!Validate(entity))
                throw new ValidationException("Entity validation failed.");

            return await _repository.AddAsync(entity);
        }

        /// <inheritdoc/>
        public virtual async Task<T> UpdateAsync(int id, T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var exists = await _repository.ExistsAsync(id);
            if (!exists)
                throw new EntityNotFoundException($"Entity with ID {id} not found.");

            if (!Validate(entity))
                throw new ValidationException("Entity validation failed.");

            await _repository.UpdateAsync(entity);
            return entity;
        }

        /// <inheritdoc/>
        public virtual async Task DeleteAsync(int id)
        {
            var exists = await _repository.ExistsAsync(id);
            if (!exists)
                throw new EntityNotFoundException($"Entity with ID {id} not found.");

            await _repository.DeleteAsync(id);
        }

        /// <inheritdoc/>
        public abstract bool Validate(T entity);

        /// <summary>
        /// Gets the underlying repository.
        /// </summary>
        protected IRepository<T> Repository => _repository;

        /// <summary>
        /// Performs additional validation logic.
        /// </summary>
        /// <param name="entity">The entity to validate</param>
        /// <param name="errors">Collection to populate with validation errors</param>
        /// <returns>True if validation passes; otherwise false</returns>
        protected virtual bool ValidateExtended(T entity, out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}