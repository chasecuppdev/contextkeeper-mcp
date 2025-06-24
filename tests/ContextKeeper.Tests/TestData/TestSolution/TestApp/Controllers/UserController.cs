using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestLibrary;
using TestLibrary.Models;

namespace TestApp.Controllers
{
    /// <summary>
    /// Controller for managing user operations.
    /// </summary>
    public class UserController : BaseController
    {
        private readonly UserService _userService;

        /// <summary>
        /// Initializes a new instance of the UserController class.
        /// </summary>
        /// <param name="userRepository">The user repository</param>
        public UserController(IRepository<User> userRepository)
        {
            _userService = new UserService(userRepository);
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="email">The email address</param>
        /// <param name="fullName">The full name</param>
        /// <returns>The created user</returns>
        public async Task<User> CreateUserAsync(string username, string email, string fullName)
        {
            LogAction($"Creating user: {username}");
            var user = new User(username, email, fullName);
            return await _userService.CreateAsync(user);
        }

        /// <summary>
        /// Gets a user by ID.
        /// </summary>
        /// <param name="id">The user ID</param>
        /// <returns>The user if found</returns>
        public async Task<User> GetUserByIdAsync(int id)
        {
            LogAction($"Getting user by ID: {id}");
            return await _userService.GetByIdAsync(id);
        }

        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns>Collection of all users</returns>
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            LogAction("Getting all users");
            return await _userService.GetAllAsync();
        }

        /// <summary>
        /// Gets active users.
        /// </summary>
        /// <returns>Collection of active users</returns>
        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            LogAction("Getting active users");
            return await _userService.GetAllAsync(u => u.IsActive);
        }

        /// <summary>
        /// Gets users by role.
        /// </summary>
        /// <param name="role">The user role</param>
        /// <returns>Collection of users with the specified role</returns>
        public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
        {
            LogAction($"Getting users by role: {role}");
            return await _userService.GetAllAsync(u => u.Role == role);
        }

        /// <summary>
        /// Updates a user.
        /// </summary>
        /// <param name="id">The user ID</param>
        /// <param name="user">The updated user data</param>
        /// <returns>The updated user</returns>
        public async Task<User> UpdateUserAsync(int id, User user)
        {
            LogAction($"Updating user: {id}");
            user.UpdatedAt = DateTime.UtcNow;
            return await _userService.UpdateAsync(id, user);
        }

        /// <summary>
        /// Deletes a user.
        /// </summary>
        /// <param name="id">The user ID</param>
        public async Task DeleteUserAsync(int id)
        {
            LogAction($"Deleting user: {id}");
            await _userService.DeleteAsync(id);
        }

        /// <summary>
        /// Activates a user.
        /// </summary>
        /// <param name="id">The user ID</param>
        /// <returns>The activated user</returns>
        public async Task<User> ActivateUserAsync(int id)
        {
            LogAction($"Activating user: {id}");
            var user = await _userService.GetByIdAsync(id);
            user.IsActive = true;
            return await _userService.UpdateAsync(id, user);
        }

        /// <summary>
        /// Deactivates a user.
        /// </summary>
        /// <param name="id">The user ID</param>
        /// <returns>The deactivated user</returns>
        public async Task<User> DeactivateUserAsync(int id)
        {
            LogAction($"Deactivating user: {id}");
            var user = await _userService.GetByIdAsync(id);
            user.IsActive = false;
            return await _userService.UpdateAsync(id, user);
        }

        /// <summary>
        /// Service implementation for user operations.
        /// </summary>
        private class UserService : Service<User>
        {
            public UserService(IRepository<User> repository) : base(repository)
            {
            }

            public override bool Validate(User entity)
            {
                return entity != null && entity.IsValid();
            }

            protected override bool ValidateExtended(User entity, out List<string> errors)
            {
                errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.Username))
                    errors.Add("Username is required.");

                if (string.IsNullOrWhiteSpace(entity.Email))
                    errors.Add("Email is required.");

                if (!entity.Email.Contains("@"))
                    errors.Add("Email must be valid.");

                if (entity.Username.Length < 3)
                    errors.Add("Username must be at least 3 characters.");

                return errors.Count == 0;
            }
        }
    }
}