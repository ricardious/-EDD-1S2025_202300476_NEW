using AutoGestPro.src.Models;

namespace AutoGestPro.src.Services.Interfaces
{
    public unsafe interface ISparePartService
    {
        SparePart* CreateSparePart(int id, string name, string details, double cost);
        SparePart* Search(int id);
        bool Delete(int id);
        SparePart*[] GetSpareParts();
        string GenerateDot();
    }
}
