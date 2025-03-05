using System;
using AutoGestPro.src.DataStructures;
using AutoGestPro.src.Models;
using AutoGestPro.src.Services.Interfaces;
using System.Runtime.InteropServices;

namespace AutoGestPro.src.Services
{
    public unsafe class ServiceQueueService(ServiceQueue serviceQueue) : IServiceQueue
    {
        private readonly ServiceQueue serviceQueue = serviceQueue;
        public event EventHandler DataChanged;

        protected virtual void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Enqueue(Service* newService)
        {
            serviceQueue.Enqueue(newService);
        }

        public Service* Dequeue()
        {
            return serviceQueue.Dequeue();
        }

        public Service* Search(int id)
        {
            return serviceQueue.Search(id);
        }

        public Service[] GetAllServices()
        {
            return serviceQueue.GetAllServices();
        }

        public Service*[] GetAllServicePointers()
        {
            return serviceQueue.GetAllServicePointers();
        }

        public string GenerateDot()
        {
            return serviceQueue.GenerateDot();
        }
    }
}
