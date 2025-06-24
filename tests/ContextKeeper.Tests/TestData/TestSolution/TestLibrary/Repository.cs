using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestLibrary
{
    /// <summary>
    /// A generic in-memory repository implementation.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly Dictionary<int, T> _entities = new Dictionary<int, T>();
        private readonly Func<T, int> _idSelector;
        private int _nextId = 1;

        /// <summary>
        /// Initializes a new instance of the Repository class.
        /// </summary>
        /// <param name="idSelector">Function to extract ID from entity</param>
        public Repository(Func<T, int> idSelector)
        {
            _idSelector = idSelector ?? throw new ArgumentNullException(nameof(idSelector));
        }

        /// <inheritdoc/>
        public virtual Task<T?> GetByIdAsync(int id)
        {
            _entities.TryGetValue(id, out var entity);
            return Task.FromResult(entity);
        }

        /// <inheritdoc/>
        public virtual Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<T>>(_entities.Values.ToList());
        }

        /// <inheritdoc/>
        public virtual Task<T> AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var id = _nextId++;
            _entities[id] = entity;
            return Task.FromResult(entity);
        }

        /// <inheritdoc/>
        public virtual Task UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var id = _idSelector(entity);
            if (!_entities.ContainsKey(id))
                throw new InvalidOperationException($"Entity with ID {id} not found.");

            _entities[id] = entity;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task DeleteAsync(int id)
        {
            _entities.Remove(id);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task<bool> ExistsAsync(int id)
        {
            return Task.FromResult(_entities.ContainsKey(id));
        }

        /// <summary>
        /// Gets the total count of entities.
        /// </summary>
        public int Count => _entities.Count;

        /// <summary>
        /// Clears all entities from the repository.
        /// </summary>
        protected void Clear()
        {
            _entities.Clear();
            _nextId = 1;
        }
    }
}