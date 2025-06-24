using System;

namespace TestLibrary.Models
{
    /// <summary>
    /// Represents a user in the system.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's full name.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date when the user was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date when the user was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets whether the user is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the user's role.
        /// </summary>
        public UserRole Role { get; set; } = UserRole.User;

        /// <summary>
        /// Initializes a new instance of the User class.
        /// </summary>
        public User()
        {
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the User class with specified values.
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="email">The email address</param>
        /// <param name="fullName">The full name</param>
        public User(string username, string email, string fullName) : this()
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
        }

        /// <summary>
        /// Returns a string representation of the user.
        /// </summary>
        /// <returns>A string containing the username and email</returns>
        public override string ToString()
        {
            return $"{Username} ({Email})";
        }

        /// <summary>
        /// Validates whether the user data is valid.
        /// </summary>
        /// <returns>True if valid; otherwise false</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   Email.Contains("@") &&
                   !string.IsNullOrWhiteSpace(FullName);
        }
    }

    /// <summary>
    /// Defines user roles in the system.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Regular user with basic permissions.
        /// </summary>
        User = 0,

        /// <summary>
        /// Moderator with elevated permissions.
        /// </summary>
        Moderator = 1,

        /// <summary>
        /// Administrator with full permissions.
        /// </summary>
        Administrator = 2
    }
}