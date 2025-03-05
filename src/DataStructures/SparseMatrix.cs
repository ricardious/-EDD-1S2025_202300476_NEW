using System;
using System.Runtime.InteropServices;
using AutoGestPro.src.Utils;

namespace AutoGestPro.src.DataStructures
{
    public unsafe struct MatrixNode
    {
        public int VehicleId;
        public int SparePartId;
        public fixed char Details[100]; // Fixed-size char array instead of string
        
        public MatrixNode* Right;
        public MatrixNode* Down;
        
        public MatrixNode(int vehicleId, int sparePartId, string details)
        {
            VehicleId = vehicleId;
            SparePartId = sparePartId;
            fixed (char* ptr = Details) { StringUtils.CopyToFixedBuffer(ptr, details, 100); }
            Right = null;
            Down = null;
        }
    }
    
    public unsafe struct HeaderNode(int id)
    {
        public int Id = id;
        public HeaderNode* Next = null;
        public MatrixNode* Access = null;
    }

    public unsafe class HeaderList
    {
        private HeaderNode* first;
        
        public HeaderList()
        {
            first = null;
        }
        
        public HeaderNode* Search(int id)
        {
            HeaderNode* current = first;
            while (current != null)
            {
                if (current->Id == id)
                {
                    return current;
                }
                current = current->Next;
            }
            return null;
        }
        
        public void Insert(HeaderNode* newNode)
        {
            if (first == null)
            {
                first = newNode;
                return;
            }
            
            if (newNode->Id < first->Id)
            {
                newNode->Next = first;
                first = newNode;
                return;
            }
            
            HeaderNode* current = first;
            while (current->Next != null && current->Next->Id < newNode->Id)
            {
                current = current->Next;
            }
            
            newNode->Next = current->Next;
            current->Next = newNode;
        }
        
        public HeaderNode* GetFirst()
        {
            return first;
        }
    }
    
    public unsafe class SparseMatrix
    {
        private HeaderList rows;
        private HeaderList columns;
        
        public SparseMatrix()
        {
            rows = new HeaderList();
            columns = new HeaderList();
        }
        
        public void Insert(int vehicleId, int sparePartId, string details)
        {
            // Create new node
            MatrixNode* newNode = (MatrixNode*)Marshal.AllocHGlobal(sizeof(MatrixNode));
            *newNode = new MatrixNode(vehicleId, sparePartId, details);
            
            // Search or create header for the row (vehicle)
            HeaderNode* currentRow = rows.Search(vehicleId);
            if (currentRow == null)
            {
                HeaderNode* newHeaderNode = (HeaderNode*)Marshal.AllocHGlobal(sizeof(HeaderNode));
                *newHeaderNode = new HeaderNode(vehicleId);
                rows.Insert(newHeaderNode);
                currentRow = newHeaderNode;
            }
            
            // Search or create header for the column (spare part)
            HeaderNode* currentColumn = columns.Search(sparePartId);
            if (currentColumn == null)
            {
                HeaderNode* newHeaderNode = (HeaderNode*)Marshal.AllocHGlobal(sizeof(HeaderNode));
                *newHeaderNode = new HeaderNode(sparePartId);
                columns.Insert(newHeaderNode);
                currentColumn = newHeaderNode;
            }
            
            // Insert into row
            if (currentRow->Access == null)
            {
                currentRow->Access = newNode;
            }
            else
            {
                MatrixNode* current = currentRow->Access;
                MatrixNode* previous = null;
                
                while (current != null && current->SparePartId < sparePartId)
                {
                    previous = current;
                    current = current->Right;
                }
                
                if (current != null && current->SparePartId == sparePartId)
                {
                    // Already exists, update
                    StringUtils.CopyToFixedBuffer(current->Details, details, 100);
                    return;
                }
                
                if (previous == null)
                {
                    newNode->Right = currentRow->Access;
                    currentRow->Access = newNode;
                }
                else
                {
                    newNode->Right = previous->Right;
                    previous->Right = newNode;
                }
            }
            
            // Insert into column
            if (currentColumn->Access == null)
            {
                currentColumn->Access = newNode;
            }
            else
            {
                MatrixNode* current = currentColumn->Access;
                MatrixNode* previous = null;
                
                while (current != null && current->VehicleId < vehicleId)
                {
                    previous = current;
                    current = current->Down;
                }
                
                if (previous == null)
                {
                    newNode->Down = currentColumn->Access;
                    currentColumn->Access = newNode;
                }
                else
                {
                    newNode->Down = previous->Down;
                    previous->Down = newNode;
                }
            }
        }
    }
}
