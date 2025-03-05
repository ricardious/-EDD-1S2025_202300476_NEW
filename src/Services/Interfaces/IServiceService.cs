using AutoGestPro.src.Models;

namespace AutoGestPro.src.Services.Interfaces
{
    public unsafe interface IServiceQueue
    {
        void Enqueue(Service* newService);
        Service* Dequeue();
        Service* Search(int id);
        Service[] GetAllServices();
        Service*[] GetAllServicePointers();
        string GenerateDot();
    }
}
