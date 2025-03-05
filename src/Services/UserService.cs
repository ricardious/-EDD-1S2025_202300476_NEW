using System;
using System.Runtime.InteropServices;
using AutoGestPro.src.Models;
using AutoGestPro.src.DataStructures;
using AutoGestPro.src.Services.Interfaces;
using AutoGestPro.src.Utils;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace AutoGestPro.src.Services
{
    public unsafe class UserService(UserList userList, int startId = 1) : IUserService
    {
        private readonly UserList userList = userList;
        public event EventHandler DataChanged;
        private int nextUserId = startId;

        protected virtual void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        public User* CreateUser(int id, string nombres, string apellidos, string correo, string contrasenia)
        {
            User* newUser = (User*)Marshal.AllocHGlobal(sizeof(User));
            *newUser = new User(id, nombres, apellidos, correo, contrasenia);
            userList.Insert(newUser);

            // Update nextUserId if needed
            if (id >= nextUserId)
                nextUserId = id + 1;

            OnDataChanged();

            return newUser;
        }

        public User* GetUserById(int id)
        {
            return userList.Search(id);
        }

        public User* GetUserByEmail(string email)
        {
            return userList.SearchByEmail(email);
        }

        public bool UpdateUser(int id, User* updatedUser)
        {
            return userList.Update(id, updatedUser);
        }




        public bool DeleteUser(int id)
        {
            return userList.Delete(id);
        }

        public User*[] GetAllUsers()
        {
            return userList.GetAllUsers();
        }

        public void LoadUsersFromJson(Newtonsoft.Json.Linq.JArray usersArray)
        {
            foreach (Newtonsoft.Json.Linq.JObject obj in usersArray.Cast<JObject>())
            {
                int id = (int)obj["ID"];
                string nombres = (string)obj["Nombres"];
                string apellidos = (string)obj["Apellidos"];
                string correo = (string)obj["Correo"];
                string contrasenia = (string)obj["Contrasenia"];

                if (userList.Search(id) == null)
                {
                    User* usuario = (User*)Marshal.AllocHGlobal(sizeof(User));
                    *usuario = new User(id, nombres, apellidos, correo, contrasenia);
                    userList.Insert(usuario);

                    // Update nextUserId if needed
                    if (id >= nextUserId)
                        nextUserId = id + 1;
                }
            }
        }

        public string GenerateDot()
        {
            return userList.GenerateDot();
        }

        
    }
}