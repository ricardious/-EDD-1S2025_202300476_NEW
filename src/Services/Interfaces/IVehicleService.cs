using System;
using AutoGestPro.src.Models;
using Newtonsoft.Json.Linq;

namespace AutoGestPro.src.Services.Interfaces
{
    public unsafe interface IVehicleService
    {
        // Crear un vehículo
        Vehicle* CreateVehicle(int id, int userId, string marca, string modelo, string placa);
        
        // Obtener un vehículo por su ID
        Vehicle* GetVehicleById(int id);
        
        // Obtener vehículos por ID de usuario
        Vehicle*[] GetVehiclesByUserId(int userId);
        
        // Actualizar un vehículo existente
        bool UpdateVehicle(int id, int userId, string marca, string modelo, string placa);
        
        // Eliminar un vehículo
        bool DeleteVehicle(int id);
        
        // Obtener todos los vehículos
        Vehicle*[] GetAllVehicles();
        
        // Cargar vehículos desde un array JSON
        void LoadVehiclesFromJson(JArray vehiclesArray);
        
        // Generar representación DOT para visualización
        string GenerateDot();
    }
}