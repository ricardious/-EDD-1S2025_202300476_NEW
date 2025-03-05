using System;
using AutoGestPro.src.Utils;

namespace AutoGestPro.src.Models
{
    public unsafe struct SparePart
    {
        public int ID;
        public fixed char Repuesto[50];
        public fixed char Detalles[100];
        public double Costo;

        // Pointer for circular linked list
        public SparePart* Next;

        public SparePart()
        {
            Next = null;
        }

        public SparePart(int id, string name, string details, double cost)
        {
            ID = id;
            Costo = cost;

            // Copy the string safely
            fixed (char* namePtr = Repuesto)
                StringUtils.CopyToFixedBuffer(namePtr, name, 50);

            fixed (char* detailsPtr = Detalles)
                StringUtils.CopyToFixedBuffer(detailsPtr, details, 100);

            Next = null;
        }
    }
}
