using System;
using AutoGestPro.src.Models;
using Newtonsoft.Json.Linq;

namespace AutoGestPro.src.Services.Interfaces
{
    public unsafe interface IUserService
    {
        User* CreateUser(int id, string nombres, string apellidos, string correo, string contrasenia);
        User* GetUserById(int id);
        User* GetUserByEmail(string email);
        bool UpdateUser(int id, User* updateData);
        bool DeleteUser(int id);
        User*[] GetAllUsers();
        void LoadUsersFromJson(JArray usersArray);
        string GenerateDot();
    }
}