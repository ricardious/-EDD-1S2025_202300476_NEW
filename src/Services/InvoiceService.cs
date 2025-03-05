using AutoGestPro.src.DataStructures;
using AutoGestPro.src.Models;
using AutoGestPro.src.Services.Interfaces;
using System;

namespace AutoGestPro.src.Services
{
    public unsafe class InvoiceService(InvoiceStack invoiceStack) : IInvoiceService
    {
        private readonly InvoiceStack invoiceStack = invoiceStack;

        public event EventHandler DataChanged;

        protected virtual void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        public unsafe Invoice* Push(Invoice* newInvoice)
        {
            try
            {
                invoiceStack.Push(newInvoice);
                OnDataChanged();
                return newInvoice;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public unsafe Invoice* Pop()
        {
            try
            {
                Invoice* invoice = invoiceStack.Pop();
                OnDataChanged();
                return invoice;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public Invoice* Peek()
        {
            return invoiceStack.Peek();
        }

        public Invoice*[] GetAllInvoices()
        {
            return invoiceStack.GetAllInvoicePointers();
        }

        public string GenerateDot()
        {
            return invoiceStack.GenerateDot();
        }
    }
}
