using System;

namespace TestApp.Controllers
{
    /// <summary>
    /// Base controller class providing common functionality.
    /// </summary>
    public abstract class BaseController
    {
        /// <summary>
        /// Gets or sets whether logging is enabled.
        /// </summary>
        protected bool LoggingEnabled { get; set; } = true;

        /// <summary>
        /// Gets the controller name.
        /// </summary>
        protected virtual string ControllerName => GetType().Name;

        /// <summary>
        /// Logs an action to the console.
        /// </summary>
        /// <param name="action">The action description</param>
        protected void LogAction(string action)
        {
            if (LoggingEnabled)
            {
                Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {ControllerName}: {action}");
            }
        }

        /// <summary>
        /// Logs an error to the console.
        /// </summary>
        /// <param name="error">The error message</param>
        /// <param name="exception">Optional exception details</param>
        protected void LogError(string error, Exception? exception = null)
        {
            if (LoggingEnabled)
            {
                Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {ControllerName} ERROR: {error}");
                if (exception != null)
                {
                    Console.WriteLine($"  Exception: {exception.GetType().Name} - {exception.Message}");
                }
            }
        }

        /// <summary>
        /// Validates input parameters.
        /// </summary>
        /// <param name="parameterName">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        protected void ValidateNotNull(string parameterName, object? value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        /// <summary>
        /// Validates string input parameters.
        /// </summary>
        /// <param name="parameterName">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <exception cref="ArgumentException">Thrown when value is null or whitespace</exception>
        protected void ValidateNotEmpty(string parameterName, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{parameterName} cannot be null or empty.", parameterName);
            }
        }

        /// <summary>
        /// Executes an action with error handling.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="action">The action to execute</param>
        /// <param name="errorMessage">Error message to log on failure</param>
        /// <returns>The action result</returns>
        protected T ExecuteWithErrorHandling<T>(Func<T> action, string errorMessage)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                LogError(errorMessage, ex);
                throw;
            }
        }
    }
}