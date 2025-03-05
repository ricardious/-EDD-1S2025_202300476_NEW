using System;
using System.IO;
using System.Diagnostics;
using Gtk;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using AutoGestPro.src.Models;
using AutoGestPro.src.DataStructures;
using AutoGestPro.src.UI.Common;  // Assuming you created the shared MessageEventArgs

namespace AutoGestPro.src.UI.Views
{
    public unsafe class ReportsView : Box
    {
        // Data structures
        private readonly UserList userList;
        private readonly VehicleList vehicleList;
        private readonly SparePartsList sparePartsList;
        private readonly ServiceQueue serviceQueue;
        private readonly InvoiceStack invoiceStack;
        private readonly SparseMatrix logMatrix;

        // UI components
        private ComboBoxText cmbTipoReporte;
        private Button btnGenerarReporte;
        private Image imgReporte;

        // Events
        public event EventHandler<MessageEventArgs> ShowMessage;

        public ReportsView(
            UserList userList,
            VehicleList vehicleList,
            SparePartsList sparePartsList,
            ServiceQueue serviceQueue,
            InvoiceStack invoiceStack,
            SparseMatrix logMatrix) : base(Orientation.Vertical, 15)
        {
            this.userList = userList;
            this.vehicleList = vehicleList;
            this.sparePartsList = sparePartsList;
            this.serviceQueue = serviceQueue;
            this.invoiceStack = invoiceStack;
            this.logMatrix = logMatrix;

            Margin = 20;
            BuildInterface();
        }

        private void BuildInterface()
        {
            // Title
            Label lblTitulo = new Label();
            lblTitulo.Markup = "<span font='18'>Reportes y Gráficos</span>";
            lblTitulo.Halign = Align.Start;
            PackStart(lblTitulo, false, false, 0);

            // Report type selector
            Label lblTipoReporte = new Label("Seleccione el tipo de reporte:");
            PackStart(lblTipoReporte, false, false, 10);

            cmbTipoReporte = new ComboBoxText();
            cmbTipoReporte.AppendText("Usuarios");
            cmbTipoReporte.AppendText("Vehículos");
            cmbTipoReporte.AppendText("Repuestos");
            cmbTipoReporte.AppendText("Servicios");
            cmbTipoReporte.AppendText("Facturación");
            cmbTipoReporte.AppendText("Bitácora");
            cmbTipoReporte.AppendText("Top 5 vehículos con más servicios");
            cmbTipoReporte.AppendText("Top 5 vehículos más antiguos");
            cmbTipoReporte.Active = 0;
            PackStart(cmbTipoReporte, false, false, 0);

            // Generate report button
            btnGenerarReporte = new Button("Generar Reporte");
            btnGenerarReporte.StyleContext.AddClass("suggested-action");
            btnGenerarReporte.Clicked += GenerarReporte_Clicked;
            PackStart(btnGenerarReporte, false, false, 20);

            // Area to display the report
            Frame frameReporte = new Frame("Reporte generado");
            PackStart(frameReporte, true, true, 0);

            ScrolledWindow sw = new ScrolledWindow();
            sw.ShadowType = ShadowType.EtchedIn;
            sw.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            frameReporte.Add(sw);

            imgReporte = new Image();
            sw.Add(imgReporte);

            // Add help text
            Label lblHelp = new Label("Seleccione un tipo de reporte y haga clic en 'Generar Reporte' para visualizar la estructura de datos.");
            lblHelp.LineWrap = true;
            lblHelp.Justify = Justification.Center;
            lblHelp.StyleContext.AddClass("dim-label");
            PackStart(lblHelp, false, false, 10);
        }

        private void GenerarReporte_Clicked(object sender, EventArgs e)
        {
            try
            {
                string tipoReporte = cmbTipoReporte.ActiveText;
                OnShowMessage($"Generando reporte de {tipoReporte}...", MessageType.Info);

                string dotContent = null;
                switch (tipoReporte)
                {
                    case "Usuarios":
                        dotContent = userList.GenerateDot();
                        break;

                    case "Vehículos":
                        dotContent = vehicleList.GenerateDot();
                        break;

                    case "Repuestos":
                        dotContent = sparePartsList.GenerateDot();
                        break;

                    case "Servicios":
                        dotContent = serviceQueue.GenerateDot();
                        break;

                    case "Facturación":
                        dotContent = invoiceStack.GenerateDot();
                        break;

                    case "Bitácora":
                        dotContent = logMatrix.GenerateDot();
                        break;

                    case "Top 5 vehículos con más servicios":
                        dotContent = GenerateTopVehiclesWithMostServices();
                        break;

                    case "Top 5 vehículos más antiguos":
                        dotContent = GenerateOldestVehicles();
                        break;
                }

                if (string.IsNullOrEmpty(dotContent))
                {
                    OnShowMessage("No se pudo generar el contenido del reporte", MessageType.Error);
                    return;
                }

                // Create the reports directory in the project folder
                string projectDir = AppDomain.CurrentDomain.BaseDirectory;
                string reportsDir = System.IO.Path.Combine(projectDir, "Reports");
                Directory.CreateDirectory(reportsDir);

                // Save files with timestamp
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string reportName = tipoReporte.Replace(" ", "_");
                string dotFilePath = System.IO.Path.Combine(reportsDir, $"{reportName}_{timestamp}.dot");
                string outputPath = System.IO.Path.Combine(reportsDir, $"{reportName}_{timestamp}.png");

                // Write DOT file
                File.WriteAllText(dotFilePath, dotContent);
                Console.WriteLine($"DOT file saved to: {dotFilePath}");
                OnShowMessage($"Archivo DOT guardado en: {dotFilePath}", MessageType.Info);

                // Check if Graphviz is installed
                if (!IsGraphvizInstalled())
                {
                    OnShowMessage("Graphviz no está instalado. Por favor, instale Graphviz con: sudo apt-get install graphviz", MessageType.Error);
                    return;
                }

                try
                {
                    // Generate image using Graphviz
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "dot",
                        Arguments = $"-Tpng \"{dotFilePath}\" -o \"{outputPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process process = Process.Start(startInfo))
                    {
                        if (process == null)
                        {
                            OnShowMessage("No se pudo iniciar el proceso de Graphviz", MessageType.Error);
                            return;
                        }

                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (process.ExitCode == 0)
                        {
                            // Load the image
                            if (File.Exists(outputPath))
                            {
                                try
                                {
                                    Application.Invoke(delegate
                                    {
                                        try
                                        {
                                            imgReporte.Pixbuf = new Gdk.Pixbuf(outputPath);
                                            OnShowMessage($"Reporte generado exitosamente en: {outputPath}", MessageType.Info);
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Error loading image: {ex.Message}");
                                            OnShowMessage($"Error al cargar la imagen: {ex.Message}", MessageType.Error);
                                        }
                                    });
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error loading image: {ex.Message}");
                                    OnShowMessage($"El reporte fue guardado en {outputPath}, pero no se pudo mostrar en pantalla", MessageType.Warning);
                                }
                            }
                            else
                            {
                                OnShowMessage($"Error: El archivo de imagen no fue creado en: {outputPath}", MessageType.Error);
                            }
                        }
                        else
                        {
                            OnShowMessage($"Error al generar la imagen: {error}", MessageType.Error);
                            Console.WriteLine($"Process error: {error}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnShowMessage($"Error al ejecutar Graphviz: {ex.Message}", MessageType.Error);
                    Console.WriteLine($"Graphviz execution error: {ex.Message}\n{ex.StackTrace}");
                }
            }
            catch (Exception ex)
            {
                OnShowMessage("Error al generar reporte: " + ex.Message, MessageType.Error);
                Console.WriteLine($"Report generation error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Generate report for the top 5 vehicles with most services
        private string GenerateTopVehiclesWithMostServices()
        {
            try
            {
                Dictionary<int, int> vehicleServiceCount = new Dictionary<int, int>();

                // Get all services from the queue
                Service*[] services = serviceQueue.GetAllServicePointers();
                if (services == null || services.Length == 0)
                {
                    return "digraph { node [shape=box]; \"No hay servicios registrados\" }";
                }

                // Count services per vehicle
                foreach (var service in services)
                {
                    if (service == null) continue;

                    int vehicleId = service->Id_Vehiculo;
                    if (vehicleServiceCount.ContainsKey(vehicleId))
                    {
                        vehicleServiceCount[vehicleId]++;
                    }
                    else
                    {
                        vehicleServiceCount[vehicleId] = 1;
                    }
                }

                // Get top 5 vehicles with most services
                var topVehicles = vehicleServiceCount
                    .OrderByDescending(kv => kv.Value)
                    .Take(5)
                    .ToArray();

                if (topVehicles.Length == 0)
                {
                    return "digraph { node [shape=box]; \"No hay datos para generar el reporte\" }";
                }

                // Generate DOT content
                StringBuilder dot = new StringBuilder();
                dot.AppendLine("digraph TopVehicles {");
                dot.AppendLine("  rankdir=TB;");
                dot.AppendLine("  node [shape=record, style=filled, fillcolor=lightblue];");
                dot.AppendLine("  graph [fontsize=20 labelloc=\"t\" label=\"Top 5 Vehículos con Más Servicios\"];");

                // Add a header node
                dot.AppendLine("  header [label=\"Top 5 Vehículos con Más Servicios\", shape=plaintext];");

                // Create nodes for each vehicle
                for (int i = 0; i < topVehicles.Length; i++)
                {
                    int vehicleId = topVehicles[i].Key;
                    int serviceCount = topVehicles[i].Value;

                    // Try to get vehicle information
                    Vehicle* vehicle = vehicleList.Search(vehicleId);
                    string vehicleInfo = "Información no disponible";

                    if (vehicle != null)
                    {
                        vehicleInfo = $"ID: {vehicleId}\\nPlaca: {new string(vehicle->Placa)}\\nMarca: {new string(vehicle->Marca)}\\nModelo: {new string(vehicle->Modelo)}";
                    }

                    dot.AppendLine($"  vehicle{i} [label=\"{vehicleInfo}\\nTotal Servicios: {serviceCount}\"];");

                    // Add rank to maintain order
                    if (i > 0)
                    {
                        dot.AppendLine($"  vehicle{i - 1} -> vehicle{i} [weight=1];");
                    }
                    else
                    {
                        dot.AppendLine($"  header -> vehicle{i} [weight=1];");
                    }
                }

                dot.AppendLine("}");
                return dot.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating top vehicles report: {ex.Message}");
                return "digraph { node [shape=box]; \"Error al generar reporte: " + ex.Message.Replace("\"", "\\\"") + "\" }";
            }
        }

        // Generate report for the top 5 oldest vehicles
        private string GenerateOldestVehicles()
        {
            try
            {
                // Get all vehicles
                Vehicle*[] vehicles = vehicleList.GetVehicles();
                if (vehicles == null || vehicles.Length == 0)
                {
                    return "digraph { node [shape=box]; \"No hay vehículos registrados\" }";
                }

                // Filter out null vehicles and create a new array
                int validCount = 0;
                foreach (Vehicle* v in vehicles)
                {
                    if (v != null) validCount++;
                }

                Vehicle*[] filteredVehicles = new Vehicle*[validCount];
                int index = 0;
                foreach (Vehicle* v in vehicles)
                {
                    if (v != null)
                    {
                        filteredVehicles[index++] = v;
                    }
                }

                // Sort by ID (bubble sort as a simple example)
                for (int i = 0; i < filteredVehicles.Length - 1; i++)
                {
                    for (int j = 0; j < filteredVehicles.Length - i - 1; j++)
                    {
                        if (filteredVehicles[j]->ID > filteredVehicles[j + 1]->ID)
                        {
                            // Swap
                            Vehicle* temp = filteredVehicles[j];
                            filteredVehicles[j] = filteredVehicles[j + 1];
                            filteredVehicles[j + 1] = temp;
                        }
                    }
                }

                // Take top 5 (or fewer if there aren't 5)
                int takeCount = Math.Min(5, filteredVehicles.Length);
                Vehicle*[] oldestVehicles = new Vehicle*[takeCount];
                for (int i = 0; i < takeCount; i++)
                {
                    oldestVehicles[i] = filteredVehicles[i];
                }

                if (oldestVehicles.Length == 0)
                {
                    return "digraph { node [shape=box]; \"No hay datos para generar el reporte\" }";
                }

                // Generate DOT content - This part remains the same
                StringBuilder dot = new StringBuilder();
                dot.AppendLine("digraph OldestVehicles {");
                dot.AppendLine("  rankdir=TB;");
                dot.AppendLine("  node [shape=record, style=filled, fillcolor=lightblue];");
                dot.AppendLine("  graph [fontsize=20 labelloc=\"t\" label=\"Top 5 Vehículos Más Antiguos\"];");

                // Add a header node
                dot.AppendLine("  header [label=\"Top 5 Vehículos Más Antiguos\", shape=plaintext];");

                // Create nodes for each vehicle
                for (int i = 0; i < oldestVehicles.Length; i++)
                {
                    Vehicle* vehicle = oldestVehicles[i];

                    string placa = new string(vehicle->Placa);
                    string marca = new string(vehicle->Marca);
                    string modelo = new string(vehicle->Modelo);

                    dot.AppendLine($"  vehicle{i} [label=\"ID: {vehicle->ID}\\nPlaca: {placa}\\nMarca: {marca}\\nModelo: {modelo}\"];");

                    // Add rank to maintain order
                    if (i > 0)
                    {
                        dot.AppendLine($"  vehicle{i - 1} -> vehicle{i} [weight=1];");
                    }
                    else
                    {
                        dot.AppendLine($"  header -> vehicle{i} [weight=1];");
                    }
                }

                dot.AppendLine("}");
                return dot.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating oldest vehicles report: {ex.Message}");
                return "digraph { node [shape=box]; \"Error al generar reporte: " + ex.Message.Replace("\"", "\\\"") + "\" }";
            }
        }

        private bool IsGraphvizInstalled()
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = "dot",
                        Arguments = "-V",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    process.Start();
                    // Graphviz typically outputs version to stderr
                    string output = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    // Log the output for debugging
                    Console.WriteLine($"Graphviz check output: {output}");

                    return output.Contains("graphviz") || process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Graphviz check failed: {ex.Message}");
                return false;
            }
        }


        protected virtual void OnShowMessage(string message, MessageType messageType)
        {
            ShowMessage?.Invoke(this, new MessageEventArgs(message, messageType));
        }

        // Method to save the current report image to a file
        public void SaveCurrentReport()
        {
            if (imgReporte.Pixbuf != null)
            {
                try
                {
                    FileChooserDialog dialog = new FileChooserDialog(
                        "Guardar Reporte",
                        null,
                        FileChooserAction.Save,
                        "Cancelar", ResponseType.Cancel,
                        "Guardar", ResponseType.Accept);

                    FileFilter filter = new FileFilter();
                    filter.AddPattern("*.png");
                    filter.Name = "Archivos de imagen (*.png)";
                    dialog.AddFilter(filter);

                    dialog.Response += (sender, args) =>
                    {
                        if ((ResponseType)args.ResponseId == ResponseType.Accept)
                        {
                            string filename = dialog.Filename;
                            if (!filename.EndsWith(".png"))
                                filename += ".png";

                            imgReporte.Pixbuf.Save(filename, "png");
                            OnShowMessage("Reporte guardado exitosamente", MessageType.Info);
                        }
                        dialog.Destroy();
                    };

                    dialog.Run();
                }
                catch (Exception ex)
                {
                    OnShowMessage("Error al guardar reporte: " + ex.Message, MessageType.Error);
                }
            }
            else
            {
                OnShowMessage("No hay reporte para guardar. Genere un reporte primero.", MessageType.Warning);
            }
        }
    }
}