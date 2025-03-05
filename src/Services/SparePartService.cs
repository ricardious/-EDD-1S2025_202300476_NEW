using System;
using AutoGestPro.src.Models;
using AutoGestPro.src.DataStructures;
using AutoGestPro.src.Services.Interfaces;
using System.Runtime.InteropServices;

namespace AutoGestPro.src.Services
{
    public unsafe class SparePartsService(SparePartsList sparePartsList) : ISparePartService
    {
        private readonly SparePartsList sparePartsList = sparePartsList;
        public event EventHandler DataChanged;

        protected virtual void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        public SparePart* CreateSparePart(int id, string name, string details, double cost)
        {
            SparePart* newSparePart = (SparePart*)Marshal.AllocHGlobal(sizeof(SparePart));
            *newSparePart = new SparePart(id, name, details, cost);
            sparePartsList.Insert(newSparePart);

            OnDataChanged();

            return newSparePart;
        }

        public SparePart* Search(int id)
        {
            return sparePartsList.Search(id);
        }

        public bool Delete(int id)
        {
            return sparePartsList.Delete(id);
        }

        public SparePart*[] GetSpareParts()
        {
            return sparePartsList.GetSpareParts();
        }

        public string GenerateDot()
        {
            return sparePartsList.GenerateDot();
        }
    }
}
