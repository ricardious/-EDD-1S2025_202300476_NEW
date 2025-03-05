using System;
using AutoGestPro.src.Models;

namespace AutoGestPro.src.DataStructures
{
    public unsafe class SparePartsList
    {
        private SparePart* head;

        public SparePartsList()
        {
            head = null;
        }

        public void Insert(SparePart* newSparePart)
        {
            if (head == null)
            {
                head = newSparePart;
                newSparePart->Next = newSparePart; // Points to itself (circular)
                return;
            }

            SparePart* last = head;
            while (last->Next != head)
            {
                last = last->Next;
            }

            last->Next = newSparePart;
            newSparePart->Next = head; // Points to head to form the circle
        }

        public SparePart* Search(int id)
        {
            if (head == null)
                return null;

            if (head->ID == id)
                return head;

            SparePart* current = head->Next;

            while (current != head)
            {
                if (current->ID == id)
                {
                    return current;
                }
                current = current->Next;
            }

            return null;
        }

        public bool Delete(int id)
        {
            if (head == null)
                return false;

            if (head->ID == id)
            {
                if (head->Next == head)
                {
                    head = null;
                }
                else
                {
                    SparePart* last = head;
                    while (last->Next != head)
                    {
                        last = last->Next;
                    }

                    head = head->Next;
                    last->Next = head;
                }
                return true;
            }

            SparePart* current = head;
            while (current->Next != head)
            {
                if (current->Next->ID == id)
                {
                    current->Next = current->Next->Next;
                    return true;
                }
                current = current->Next;
            }

            return false;
        }

        public unsafe SparePart*[] GetSpareParts()
        {
            if (head == null)
                return [];

            int count = GetSize();
            SparePart*[] spareParts = new SparePart*[count];

            SparePart* current = head;
            int index = 0;

            do
            {
                spareParts[index] = current; // Pointer to the current node
                current = current->Next;
                index++;
            } while (current != head);

            return spareParts;
        }


        public string GenerateDot()
        {
            if (head == null)
                return "digraph SparePartsList { }";

            string dot = "digraph SparePartsList {\n";
            dot += "rankdir=LR;\n";
            dot += "node [shape=record];\n";

            SparePart* current = head;
            int i = 0;

            do
            {
                string name = new(current->Repuesto);

                dot += $"node{i} [label=\"ID: {current->ID}| Spare Part: {name}| Cost: {current->Costo:C}\"];\n";

                int nextIdx = (i + 1) % (GetSize());
                dot += $"node{i} -> node{nextIdx};\n";

                current = current->Next;
                i++;
            } while (current != head);

            dot += "}";
            return dot;
        }

        private int GetSize()
        {
            if (head == null)
                return 0;

            int count = 1;
            SparePart* current = head->Next;

            while (current != head)
            {
                count++;
                current = current->Next;
            }

            return count;
        }
    }
}
