using System;
using System.Collections.Generic;
using AutoGestPro.src.Models;
using AutoGestPro.src.Utils;

namespace AutoGestPro.src.DataStructures
{
    /// <summary>
    /// Implements a linked list data structure for managing User objects.
    /// This class uses unsafe code to handle pointers for dynamic memory management.
    /// </summary>
    public unsafe class UserList
    {
        /// <summary>
        /// Pointer to the head (first element) of the user list.
        /// </summary>
        private User* head;

        /// <summary>
        /// Initializes an empty user list.
        /// </summary>
        public UserList()
        {
            head = null;
        }

        /// <summary>
        /// Inserts a new user at the end of the list.
        /// </summary>
        /// <param name="newUser">Pointer to the user to be added.</param>
        public void Insert(User* newUser)
        {
            if (head == null)
            {
                head = newUser;
                return;
            }

            User* current = head;
            while (current->Next != null)
            {
                current = current->Next;
            }

            current->Next = newUser;
        }

        /// <summary>
        /// Searches for a user by their unique ID.
        /// </summary>
        /// <param name="id">The ID of the user to find.</param>
        /// <returns>Pointer to the user if found, otherwise null.</returns>
        public User* Search(int id)
        {
            User* current = head;
            while (current != null)
            {
                if (current->ID == id)
                {
                    return current;
                }
                current = current->Next;
            }
            return null;
        }

        /// <summary>
        /// Searches for a user by their email address.
        /// </summary>
        /// <param name="email">The email to search for.</param>
        /// <returns>Pointer to the user if found, otherwise null.</returns>
        public User* SearchByEmail(string email)
        {
            User* current = head;
            while (current != null)
            {
                string currentEmail = new(current->Correo);
                if (currentEmail == email)
                {
                    return current;
                }
                current = current->Next;
            }
            return null;
        }

        /// <summary>
        /// Deletes a user from the list by their unique ID.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>True if the user was successfully deleted, otherwise false.</returns>
        public bool Delete(int id)
        {
            if (head == null)
                return false;

            if (head->ID == id)
            {
                head = head->Next;
                return true;
            }

            User* current = head;
            while (current->Next != null)
            {
                if (current->Next->ID == id)
                {
                    current->Next = current->Next->Next;
                    return true;
                }
                current = current->Next;
            }

            return false;
        }

        /// <summary>
        /// Updates an existing user's information in the list.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="updatedUser">Pointer to a User object containing the updated information.</param>
        /// <returns>True if the user was found and updated, otherwise false.</returns>
        public bool Update(int id, User* updatedUser)
        {

            // Validate inputs
            if (updatedUser == null)
            {
                return false;
            }
            
            User* userToUpdate = Search(id);

            if (userToUpdate == null)
            {
                return false; // User not found
            }

            // Update individual string fields
            StringUtils.CopyToFixedBuffer(userToUpdate->Nombres, new string(updatedUser->Nombres), 50);
            StringUtils.CopyToFixedBuffer(userToUpdate->Apellidos, new string(updatedUser->Apellidos), 50);
            StringUtils.CopyToFixedBuffer(userToUpdate->Correo, new string(updatedUser->Correo), 100);
            StringUtils.CopyToFixedBuffer(userToUpdate->Contrasenia, new string(updatedUser->Contrasenia), 50);

            return true;
        }

        /// <summary>
        /// Gets all users in the list.
        /// </summary>
        /// <returns>An array containing pointers to all users in the list.</returns>
        public User*[] GetAllUsers()
        {
            // First count how many users we have
            int count = 0;
            User* current = head;
            while (current != null)
            {
                count++;
                current = current->Next;
            }

            // Create an array of the right size
            User*[] users = new User*[count];

            // Fill the array
            current = head;
            for (int i = 0; i < count; i++)
            {
                users[i] = current;
                current = current->Next;
            }

            return users;
        }

        /// <summary>
        /// Generates a Graphviz DOT representation of the linked list.
        /// </summary>
        /// <returns>A string representing the DOT format visualization of the user list.</returns>
        public string GenerateDot()
        {
            string dot = "digraph UserList {\n";
            dot += "rankdir=LR;\n";
            dot += "node [shape=record];\n";

            User* current = head;
            int i = 0;

            while (current != null)
            {
                string firstName = new(current->Nombres);
                string lastName = new(current->Apellidos);
                string email = new(current->Correo);

                dot += $"node{i} [label=\"ID: {current->ID}| Name: {firstName} {lastName}| Email: {email}\"];\n";

                if (current->Next != null)
                {
                    dot += $"node{i} -> node{i + 1};\n";
                }

                current = current->Next;
                i++;
            }

            dot += "}";
            return dot;
        }
    }
}
