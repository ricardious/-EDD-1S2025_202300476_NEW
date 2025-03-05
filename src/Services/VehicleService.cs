using System;
using System.Runtime.InteropServices;
using AutoGestPro.src.Models;
using AutoGestPro.src.DataStructures;
using AutoGestPro.src.Services.Interfaces;
using Newtonsoft.Json.Linq;



namespace AutoGestPro.src.Services
{


    public unsafe class VehicleService(VehicleList vehicleList, int startId = 1) : IVehicleService
    {
        private readonly VehicleList vehicleList = vehicleList;
        public event EventHandler DataChanged;
        private int nextVehicleId = startId;

        protected virtual void OnVehiclesChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        public Vehicle* CreateVehicle(int id, int userId, string marca, string modelo, string placa)
        {

            Vehicle* newVehicle = (Vehicle*)Marshal.AllocHGlobal(sizeof(Vehicle));
            *newVehicle = new Vehicle(id, userId, marca, modelo, placa);
            vehicleList.Insert(newVehicle);

            // Update nextVehicleId if needed
            if (id >= nextVehicleId)
                nextVehicleId = id + 1;

            OnVehiclesChanged();

            return newVehicle;
        }

        public Vehicle* GetVehicleById(int id)
        {
            return vehicleList.Search(id);
        }

        public Vehicle*[] GetVehiclesByUserId(int userId)
        {
            return vehicleList.SearchByUserId(userId);
        }

        public bool UpdateVehicle(int id, int userId, string marca, string modelo, string placa)
        {
            Vehicle* vehiculo = vehicleList.Search(id);
            if (vehiculo == null) return false;

            *vehiculo = new Vehicle(id, userId, marca, modelo, placa);
            return true;
        }

        public bool DeleteVehicle(int id)
        {
            return vehicleList.Delete(id);
        }

        public Vehicle*[] GetAllVehicles()
        {
            return vehicleList.GetVehicles();
        }

        public void LoadVehiclesFromJson(JArray vehiclesArray)
        {
            foreach (JObject obj in vehiclesArray)
            {
                int id = (int)obj["ID"];
                int userId = (int)obj["ID_Usuario"];
                string marca = (string)obj["Marca"];
                string modelo = (string)obj["Modelo"];
                string placa = (string)obj["Placa"];

                if (vehicleList.Search(id) == null)
                {
                    Vehicle* vehiculo = (Vehicle*)Marshal.AllocHGlobal(sizeof(Vehicle));
                    *vehiculo = new Vehicle(id, userId, marca, modelo, placa);
                    vehicleList.Insert(vehiculo);

                    // Actualizar nextVehicleId si es necesario
                    if (id >= nextVehicleId)
                        nextVehicleId = id + 1;
                }
            }
        }

        public string GenerateDot()
        {
            return vehicleList.GenerateDot();
        }
    }
}