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

        // Get specific value
        public string GetValue(int vehicleId, int sparePartId)
        {
            HeaderNode* rowHeader = rows.Search(vehicleId);
            if (rowHeader == null) return null;

            MatrixNode* current = rowHeader->Access;
            while (current != null)
            {
                if (current->SparePartId == sparePartId)
                {
                    // Convert fixed buffer to string
                    return new string(current->Details);
                }
                current = current->Right;
            }

            return null; // Not found
        }
        // Get all values in the matrix
        public string[,] GetAllValues()
        {
            // First, determine the dimensions
            int maxVehicleId = 0;
            int maxSparePartId = 0;

            HeaderNode* currentRow = rows.GetFirst();
            while (currentRow != null)
            {
                if (currentRow->Id > maxVehicleId)
                    maxVehicleId = currentRow->Id;

                currentRow = currentRow->Next;
            }

            HeaderNode* currentColumn = columns.GetFirst();
            while (currentColumn != null)
            {
                if (currentColumn->Id > maxSparePartId)
                    maxSparePartId = currentColumn->Id;

                currentColumn = currentColumn->Next;
            }

            // Create the result matrix
            string[,] result = new string[maxVehicleId + 1, maxSparePartId + 1];

            // Fill with values
            currentRow = rows.GetFirst();
            while (currentRow != null)
            {
                MatrixNode* node = currentRow->Access;
                while (node != null)
                {
                    result[node->VehicleId, node->SparePartId] = new string(node->Details);
                    node = node->Right;
                }
                currentRow = currentRow->Next;
            }

            return result;
        }

        // Generate a formatted log/report
        public string GenerateServiceLog()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== BITÁCORA DE SERVICIOS ===");
            sb.AppendLine();

            HeaderNode* currentRow = rows.GetFirst();
            while (currentRow != null)
            {
                int vehicleId = currentRow->Id;
                sb.AppendLine($"Vehículo ID: {vehicleId}");

                MatrixNode* node = currentRow->Access;
                while (node != null)
                {
                    sb.AppendLine($"  - Repuesto ID: {node->SparePartId}");
                    sb.AppendLine($"    Detalles: {new string(node->Details)}");

                    node = node->Right;
                }

                sb.AppendLine();
                currentRow = currentRow->Next;
            }

            return sb.ToString();
        }

        // Generate DOT representation for Graphviz visualization
        public string GenerateDot()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("digraph SparseMatrix {");
            sb.AppendLine("  node [shape=box];");
            sb.AppendLine("  rankdir=LR;");

            // Header nodes styling
            sb.AppendLine("  node [style=filled, fillcolor=lightblue];");

            // Row headers
            HeaderNode* rowHeader = rows.GetFirst();
            while (rowHeader != null)
            {
                sb.AppendLine($"  row_{rowHeader->Id} [label=\"Vehículo {rowHeader->Id}\"];");

                if (rowHeader->Next != null)
                    sb.AppendLine($"  row_{rowHeader->Id} -> row_{rowHeader->Next->Id} [constraint=false];");

                rowHeader = rowHeader->Next;
            }

            // Column headers
            HeaderNode* colHeader = columns.GetFirst();
            while (colHeader != null)
            {
                sb.AppendLine($"  col_{colHeader->Id} [label=\"Repuesto {colHeader->Id}\"];");

                if (colHeader->Next != null)
                    sb.AppendLine($"  col_{colHeader->Id} -> col_{colHeader->Next->Id} [constraint=false];");

                colHeader = colHeader->Next;
            }

            // Matrix nodes
            sb.AppendLine("  node [style=filled, fillcolor=lightgreen];");
            rowHeader = rows.GetFirst();
            while (rowHeader != null)
            {
                MatrixNode* current = rowHeader->Access;

                // Link row header to first node
                if (current != null)
                    sb.AppendLine($"  row_{rowHeader->Id} -> node_{current->VehicleId}_{current->SparePartId};");

                while (current != null)
                {
                    string details = new string(current->Details);
                    if (details.Length > 20) details = details.Substring(0, 17) + "...";

                    sb.AppendLine($"  node_{current->VehicleId}_{current->SparePartId} [label=\"{details}\"];");

                    // Horizontal links
                    if (current->Right != null)
                        sb.AppendLine($"  node_{current->VehicleId}_{current->SparePartId} -> node_{current->Right->VehicleId}_{current->Right->SparePartId};");

                    current = current->Right;
                }

                rowHeader = rowHeader->Next;
            }

            // Link column headers to first nodes in columns
            colHeader = columns.GetFirst();
            while (colHeader != null)
            {
                MatrixNode* current = colHeader->Access;

                if (current != null)
                    sb.AppendLine($"  col_{colHeader->Id} -> node_{current->VehicleId}_{current->SparePartId};");

                while (current != null)
                {
                    // Vertical links
                    if (current->Down != null)
                        sb.AppendLine($"  node_{current->VehicleId}_{current->SparePartId} -> node_{current->Down->VehicleId}_{current->Down->SparePartId} [constraint=false, style=dashed];");

                    current = current->Down;
                }

                colHeader = colHeader->Next;
            }

            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
