using System;

namespace AutoGestPro.src.Models
{
    public unsafe struct Invoice
    {
        public int ID;
        public int ID_Orden;
        public double Total;
        
        // Pointer for stack
        public Invoice* Next;

        public Invoice()
        {
            Next = null;
        }

        public Invoice(int id, int orderId, double total)
        {
            ID = id;
            ID_Orden = orderId;
            Total = total;
            Next = null;
        }
    }
}
