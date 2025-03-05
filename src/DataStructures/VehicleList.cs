using System;
using System.Collections.Generic;
using AutoGestPro.src.Models;

namespace AutoGestPro.src.DataStructures
{
    public unsafe class VehicleList
    {
        private Vehicle* head;
        private Vehicle* tail;

        public VehicleList()
        {
            head = null;
            tail = null;
        }

        public void Insert(Vehicle* newVehicle)
        {
            if (head == null)
            {
                head = newVehicle;
                tail = newVehicle;
                return;
            }

            tail->Next = newVehicle;
            newVehicle->Previous = tail;
            tail = newVehicle;
        }

        public Vehicle* Search(int id)
        {
            Vehicle* current = head;
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

        public Vehicle* SearchByPlate(string plate)
        {
            Vehicle* current = head;
            while (current != null)
            {
                string currentPlate = new(current->Placa);

                if (currentPlate == plate)
                {
                    return current;
                }
                current = current->Next;
            }
            return null;
        }

        public Vehicle*[] SearchByUserId(int userId)
        {
            int count = 0;
            Vehicle* current = head;

            while (current != null)
            {
                if (current->ID_Usuario == userId)
                {
                    count++;
                }
                current = current->Next;
            }

            Vehicle*[] vehicles = new Vehicle*[count];
            current = head;
            int i = 0;
            while (current != null)
            {
                if (current->ID_Usuario == userId)
                {
                    vehicles[i++] = current;
                }
                current = current->Next;
            }
            return vehicles;
        }

        public bool Delete(int id)
        {
            if (head == null)
                return false;

            if (head->ID == id)
            {
                if (head == tail)
                {
                    head = null;
                    tail = null;
                }
                else
                {
                    head = head->Next;
                    head->Previous = null;
                }
                return true;
            }

            if (tail->ID == id)
            {
                tail = tail->Previous;
                tail->Next = null;
                return true;
            }

            Vehicle* current = head->Next;
            while (current != null)
            {
                if (current->ID == id)
                {
                    current->Previous->Next = current->Next;
                    current->Next->Previous = current->Previous;
                    return true;
                }
                current = current->Next;
            }

            return false;
        }

        public unsafe Vehicle*[] GetVehicles()
        {
            int count = 0;
            Vehicle* current = head;

            // Contar la cantidad de vehículos en la lista
            while (current != null)
            {
                count++;
                current = current->Next;
            }

            // Crear el array con el tamaño adecuado
            Vehicle*[] vehicles = new Vehicle*[count];

            current = head;
            int index = 0;
            while (current != null)
            {
                vehicles[index] = current; // Copia segura del vehículo
                current = current->Next;
                index++;
            }

            return vehicles;
        }


        public string GenerateDot()
        {
            string dot = "digraph VehicleList {\n";
            dot += "rankdir=LR;\n";
            dot += "node [shape=record];\n";

            Vehicle* current = head;
            int i = 0;

            while (current != null)
            {
                string brand = new(current->Marca);
                string model = new(current->Modelo);
                string plate = new(current->Placa);
                dot += $"node{i} [label=\"ID: {current->ID}| User: {current->ID_Usuario}| Brand: {brand}| Model: {model}| Plate: {plate}\"];\n";

                if (current->Next != null)
                {
                    dot += $"node{i} -> node{i + 1};\n";
                    dot += $"node{i + 1} -> node{i} [constraint=false];\n";
                }

                current = current->Next;
                i++;
            }

            dot += "}";
            return dot;
        }
    }
}
