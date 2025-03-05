using AutoGestPro.src.Utils; // Import the utilities

namespace AutoGestPro.src.Models
{
    public unsafe struct Service
    {
        public int ID;
        public int Id_Repuesto;
        public int Id_Vehiculo;
        public fixed char Detalles[100];
        public double Costo;

        // Pointer for queue
        public Service* Next;

        public Service()
        {
            Next = null;
        }

        public Service(int id, int sparePartId, int vehicleId, string details, double cost)
        {
            ID = id;
            Id_Repuesto = sparePartId;
            Id_Vehiculo = vehicleId;
            Costo = cost;
            Next = null;

            // Copy details safely
            fixed (char* detailsPtr = Detalles)
                StringUtils.CopyToFixedBuffer(detailsPtr, details, 100);
        }
    }
}
