using System;
using AutoGestPro.src.Models;

namespace AutoGestPro.src.DataStructures
{
    public unsafe class ServiceQueue
    {
        private Service* front;
        private Service* rear;

        public ServiceQueue()
        {
            front = null;
            rear = null;
        }

        public void Enqueue(Service* newService)
        {
            if (front == null)
            {
                front = newService;
                rear = newService;
                return;
            }

            rear->Next = newService;
            rear = newService;
        }

        public Service* Dequeue()
        {
            if (front == null)
                return null;

            Service* service = front;
            front = front->Next;

            if (front == null)
                rear = null;

            service->Next = null;
            return service;
        }

        public Service* Search(int id)
        {
            Service* current = front;
            while (current != null)
            {
                if (current->ID == id)
                {
                    return current;
                }
                current = current->Next;
            }
            return null;
        }

        /// <summary>
        /// Gets all services in the queue as an array.
        /// </summary>
        /// <returns>Array of Service structures</returns>
        public Service[] GetAllServices()
        {
            // Count the number of services in the queue
            int count = 0;
            Service* current = front;

            while (current != null)
            {
                count++;
                current = current->Next;
            }

            // Create an array to hold the services
            Service[] services = new Service[count];

            // Fill the array with copies of the services
            current = front;
            for (int i = 0; i < count; i++)
            {
                services[i] = *current; // Copy the service struct
                current = current->Next;
            }

            return services;
        }


        /// <summary>
        /// Gets all service pointers in the queue.
        /// </summary>
        /// <returns>Array of Service pointers</returns>
        public Service*[] GetAllServicePointers()
        {
            // Count the number of services
            int count = 0;
            Service* current = front;

            while (current != null)
            {
                count++;
                current = current->Next;
            }

            // Create an array of pointers
            Service*[] pointers = new Service*[count];

            // Fill the array with the pointers
            current = front;
            for (int i = 0; i < count; i++)
            {
                pointers[i] = current;
                current = current->Next;
            }

            return pointers;
        }



        public string GenerateDot()
        {
            string dot = "digraph ServiceQueue {\n";
            dot += "rankdir=LR;\n";
            dot += "node [shape=record];\n";
            dot += "label=\"Service Queue\";\n";

            Service* current = front;
            int i = 0;

            while (current != null)
            {
                dot += $"node{i} [label=\"ID: {current->ID}| Vehicle: {current->Id_Vehiculo}| Part: {current->Id_Repuesto}| Cost: {current->Costo:C}\"];\n";

                if (current->Next != null)
                {
                    dot += $"node{i} -> node{i + 1};\n";
                }

                current = current->Next;
                i++;
            }

            dot += "}";
            return dot;
        }
    }
}
