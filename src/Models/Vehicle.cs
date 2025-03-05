using System;
using AutoGestPro.src.Utils;

namespace AutoGestPro.src.Models
{
    public unsafe struct Vehicle
    {
        public int ID;
        public int ID_Usuario;
        public fixed char Marca[50];
        public fixed char Modelo[50];
        public fixed char Placa[20];

        // Pointers for doubly linked list
        public Vehicle* Next;
        public Vehicle* Previous;

        public Vehicle()
        {
            Next = null;
            Previous = null;
        }

        public Vehicle(int id, int userId, string brand, string model, string licensePlate)
        {
            ID = id;
            ID_Usuario = userId;

            fixed (char* ptr = Marca)
                StringUtils.CopyToFixedBuffer(ptr, brand, 50);

            fixed (char* ptr = Modelo)
                StringUtils.CopyToFixedBuffer(ptr, model, 50);

            fixed (char* ptr = Placa)
                StringUtils.CopyToFixedBuffer(ptr, licensePlate, 20);
            
            Next = null;
            Previous = null;
        }
    }
}
