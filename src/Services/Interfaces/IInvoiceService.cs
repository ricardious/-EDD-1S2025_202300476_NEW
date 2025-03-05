using AutoGestPro.src.Models;

namespace AutoGestPro.src.Services.Interfaces
{
    public unsafe interface IInvoiceService
    {
        unsafe Invoice* Push(Invoice* newInvoice);
        Invoice* Pop();
        Invoice* Peek();
        Invoice*[] GetAllInvoices();
        string GenerateDot();
    }
}
