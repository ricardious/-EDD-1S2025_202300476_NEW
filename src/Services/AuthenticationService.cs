using System;
using System.Collections.Generic;
using AutoGestPro.src.Models;

namespace AutoGestPro.src.Services
{
    /// <summary>
    /// Provides authentication services for the application.
    /// </summary>
    public class AuthenticationService
    {
        // Hardcoded admin credentials (for demonstration purposes only)
        private const string AdminEmail = "root@gmail.com";
        private const string AdminPassword = "root123";

        /// <summary>
        /// Authenticates a user by verifying their email and password.
        /// </summary>
        /// <param name="email">The email of the user attempting to authenticate.</param>
        /// <param name="password">The password provided by the user.</param>
        /// <returns>True if authentication is successful, otherwise false.</returns>
        public static bool Authenticate(string email, string password)
        {
            // Ensure email and password are not null or empty
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return false;

            // Check if the provided credentials match the hardcoded admin credentials
            return email.Equals(AdminEmail) && password.Equals(AdminPassword);
        }
    }
}
