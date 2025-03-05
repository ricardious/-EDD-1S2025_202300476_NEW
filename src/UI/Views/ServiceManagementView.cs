using System;
using Gtk;
using AutoGestPro.src.Models;
using AutoGestPro.src.DataStructures;
using System.Runtime.InteropServices;
using AutoGestPro.src.UI.Common;
using AutoGestPro.src.Services;  // Assuming you created the shared MessageEventArgs

namespace AutoGestPro.src.UI.Views
{
    public unsafe class ServiceManagementView : Box, IDisposable
    {
        // Services
        private readonly ServiceQueueService serviceQueueService;
        private readonly VehicleService vehicleService;
        private readonly SparePartsService sparePartsService;
        private readonly InvoiceService invoiceService;
        private readonly SparseMatrix logMatrix;

        // UI components
        private ComboBoxText cmbVehiculoServicio;
        private ComboBoxText cmbRepuestoServicio;
        private Entry txtDetallesServicio;
        private Entry txtCostoServicio;
        private Button btnGenerarServicio;
        private TreeView trvServicios;
        private ListStore lsServicios;

        // Events
        public event EventHandler<MessageEventArgs> ShowMessage;
        public event EventHandler<EventArgs> ServiceDataChanged;
        public event EventHandler<EventArgs> InvoiceCreated;

        // Counter for next IDs
        private int siguienteIdServicio = 1;
        private int siguienteIdFactura = 1;

        private bool disposed = false;
        public ServiceManagementView(
            ServiceQueueService serviceQueueService,
            VehicleService vehicleService,
            SparePartsService sparePartsService,
            InvoiceService invoiceService,
            SparseMatrix logMatrix,
            int startServiceId = 1,
            int startInvoiceId = 1) : base(Orientation.Vertical, 15)
        {
            this.serviceQueueService = serviceQueueService;
            this.vehicleService = vehicleService;
            this.sparePartsService = sparePartsService;
            this.invoiceService = invoiceService;
            this.logMatrix = logMatrix;
            this.siguienteIdServicio = startServiceId;
            this.siguienteIdFactura = startInvoiceId;

            Margin = 20;
            BuildInterface();
            LoadPendingServices();
        }

        private void BuildInterface()
        {
            // Title
            Label lblTitulo = new Label();
            lblTitulo.Markup = "<span font='18'>Generación de Servicios</span>";
            lblTitulo.Halign = Align.Start;
            PackStart(lblTitulo, false, false, 0);

            // Panel with form and list
            Box panelBox = new Box(Orientation.Horizontal, 20);
            PackStart(panelBox, true, true, 0);

            // Service form
            Box formBox = new Box(Orientation.Vertical, 10);
            panelBox.PackStart(formBox, false, false, 0);

            Frame formFrame = new Frame("Datos del Servicio");
            formBox.PackStart(formFrame, false, false, 0);

            Box formInnerBox = new Box(Orientation.Vertical, 10);
            formInnerBox.Margin = 15;
            formFrame.Add(formInnerBox);

            // Form fields
            Grid formGrid = new Grid();
            formGrid.RowSpacing = 10;
            formGrid.ColumnSpacing = 10;
            formInnerBox.PackStart(formGrid, false, false, 0);

            Label lblVehiculo = new Label("Vehículo:");
            lblVehiculo.Halign = Align.Start;
            formGrid.Attach(lblVehiculo, 0, 0, 1, 1);

            cmbVehiculoServicio = new ComboBoxText();
            formGrid.Attach(cmbVehiculoServicio, 1, 0, 1, 1);

            Label lblRepuesto = new Label("Repuesto:");
            lblRepuesto.Halign = Align.Start;
            formGrid.Attach(lblRepuesto, 0, 1, 1, 1);

            cmbRepuestoServicio = new ComboBoxText();
            formGrid.Attach(cmbRepuestoServicio, 1, 1, 1, 1);

            Label lblDetalles = new Label("Detalles del servicio:");
            lblDetalles.Halign = Align.Start;
            formGrid.Attach(lblDetalles, 0, 2, 1, 1);

            txtDetallesServicio = new Entry();
            txtDetallesServicio.PlaceholderText = "Describa el servicio";
            formGrid.Attach(txtDetallesServicio, 1, 2, 1, 1);

            Label lblCosto = new Label("Costo ($):");
            lblCosto.Halign = Align.Start;
            formGrid.Attach(lblCosto, 0, 3, 1, 1);

            txtCostoServicio = new Entry();
            txtCostoServicio.PlaceholderText = "Ej: 50.00";
            formGrid.Attach(txtCostoServicio, 1, 3, 1, 1);

            // Generate service button
            btnGenerarServicio = new Button("Generar Servicio");
            btnGenerarServicio.StyleContext.AddClass("suggested-action");
            btnGenerarServicio.Clicked += GenerarServicio_Clicked;
            formInnerBox.PackStart(btnGenerarServicio, false, false, 10);

            // Services list
            Frame listaFrame = new Frame("Lista de Servicios Pendientes");
            panelBox.PackStart(listaFrame, true, true, 0);

            Box listaBox = new Box(Orientation.Vertical, 10);
            listaBox.Margin = 15;
            listaFrame.Add(listaBox);

            ScrolledWindow sw = new ScrolledWindow();
            sw.ShadowType = ShadowType.EtchedIn;
            sw.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            listaBox.PackStart(sw, true, true, 0);

            // TreeView for services
            lsServicios = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string), typeof(string));
            trvServicios = new TreeView(lsServicios);
            trvServicios.HeadersVisible = true;

            trvServicios.AppendColumn("ID", new CellRendererText(), "text", 0);
            trvServicios.AppendColumn("Vehículo", new CellRendererText(), "text", 1);
            trvServicios.AppendColumn("Repuesto", new CellRendererText(), "text", 2);
            trvServicios.AppendColumn("Detalles", new CellRendererText(), "text", 3);
            trvServicios.AppendColumn("Costo", new CellRendererText(), "text", 4);

            sw.Add(trvServicios);

            // Load initial data
            LoadVehicleComboBox();
            LoadSparePartsComboBox();
        }

        private void LoadPendingServices()
        {
            try
            {
                lsServicios.Clear();

                Service*[] services = serviceQueueService.GetAllServicePointers();
                if (services == null || services.Length == 0) return;

                foreach (Service* service in services)
                {
                    if (service == null) continue;

                    try
                    {
                        Vehicle* vehiculo = vehicleService.GetVehicleById(service->Id_Vehiculo);
                        SparePart* repuesto = sparePartsService.Search(service->Id_Repuesto);

                        if (vehiculo != null && repuesto != null)
                        {
                            string placa = SafeGetString(vehiculo->Placa);
                            string nombreRepuesto = SafeGetString(repuesto->Repuesto);
                            string detalles = SafeGetString(service->Detalles);

                            lsServicios.AppendValues(
                                service->ID.ToString(),
                                placa,
                                nombreRepuesto,
                                detalles,
                                (service->Costo + repuesto->Costo).ToString("C")
                            );

                            // Update next service ID
                            siguienteIdServicio = Math.Max(siguienteIdServicio, service->ID + 1);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing service record: {ex.Message}");
                        // Continue with next service
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading pending services: {ex.Message}");
                OnShowMessage("Error al cargar servicios pendientes", MessageType.Error);
            }
        }

        private void LoadVehicleComboBox()
        {
            cmbVehiculoServicio.RemoveAll();

            Vehicle*[] vehicles = vehicleService.GetAllVehicles();
            if (vehicles == null) return;

            foreach (Vehicle* vehicle in vehicles)
            {
                string marca = new(vehicle->Marca);
                string modelo = new(vehicle->Modelo);
                string placa = new(vehicle->Placa);

                cmbVehiculoServicio.AppendText($"{vehicle->ID} - {placa} ({marca} {modelo})");
            }
        }

        private void LoadSparePartsComboBox()
        {
            cmbRepuestoServicio.RemoveAll();

            SparePart*[] spareParts = sparePartsService.GetSpareParts();
            if (spareParts == null) return;

            foreach (SparePart* part in spareParts)
            {
                string nombre = new(part->Repuesto);
                cmbRepuestoServicio.AppendText($"{part->ID} - {nombre} (${part->Costo:F2})");
            }
        }

        private void GenerarServicio_Clicked(object sender, EventArgs e)
        {
            if (disposed) return;

            int idVehiculo = -1;
            int idRepuesto = -1;

            try
            {
                // Input validation with better error handling
                if (cmbVehiculoServicio.Active == -1 ||
                    string.IsNullOrEmpty(cmbVehiculoServicio.ActiveText) ||
                    !int.TryParse(cmbVehiculoServicio.ActiveText.Split('-')[0].Trim(), out idVehiculo))
                {
                    OnShowMessage("Por favor, seleccione un vehículo válido", MessageType.Error);
                    return;
                }

                if (cmbRepuestoServicio.Active == -1 ||
                    string.IsNullOrEmpty(cmbRepuestoServicio.ActiveText) ||
                    !int.TryParse(cmbRepuestoServicio.ActiveText.Split('-')[0].Trim(), out idRepuesto))
                {
                    OnShowMessage("Por favor, seleccione un repuesto válido", MessageType.Error);
                    return;
                }

                string detalles = txtDetallesServicio.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(detalles))
                {
                    OnShowMessage("Por favor, ingrese los detalles del servicio", MessageType.Error);
                    return;
                }

                if (!decimal.TryParse(txtCostoServicio.Text, out decimal costo) || costo < 0)
                {
                    OnShowMessage("Por favor, ingrese un costo válido", MessageType.Error);
                    return;
                }

                // Retrieve vehicle and spare part with null checks
                Vehicle* vehiculo = vehicleService.GetVehicleById(idVehiculo);
                SparePart* repuesto = sparePartsService.Search(idRepuesto);

                if (vehiculo == null || repuesto == null)
                {
                    OnShowMessage("Vehículo o repuesto no encontrados", MessageType.Error);
                    return;
                }

                // Create new service with proper exception handling
                int idServicio = siguienteIdServicio++;
                Service* servicio = null;

                try
                {
                    servicio = (Service*)Marshal.AllocHGlobal(sizeof(Service));
                    *servicio = new Service(idServicio, idVehiculo, idRepuesto, detalles, (double)costo);
                }
                catch (Exception ex)
                {
                    siguienteIdServicio--; // Revert ID increment
                    if (servicio != null) Marshal.FreeHGlobal((IntPtr)servicio);
                    throw new Exception($"Error al crear el servicio: {ex.Message}", ex);
                }

                // Add to queue
                serviceQueueService.Enqueue(servicio);

                // Update ListView safely
                string placaVehiculo = SafeGetString(vehiculo->Placa);
                string nombreRepuesto = SafeGetString(repuesto->Repuesto);

                // Use Application.Invoke for UI updates
                Application.Invoke(delegate
                {
                    lsServicios.AppendValues(
                        idServicio.ToString(),
                        placaVehiculo,
                        nombreRepuesto,
                        detalles,
                        ((double)costo + repuesto->Costo).ToString("C")
                    );

                    OnShowMessage("Servicio registrado exitosamente", MessageType.Info);

                    // Generate invoice automatically
                    GenerarFactura(servicio, repuesto, (double)costo);

                    // Clear form
                    ClearServiceFields();
                    OnServiceDataChanged();
                });
            }
            catch (Exception ex)
            {
                OnShowMessage("Error al generar servicio: " + ex.Message, MessageType.Error);
                Console.WriteLine($"Error in GenerarServicio_Clicked: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void GenerarFactura(Service* servicio, SparePart* repuesto, double costoServicio)
        {
            try
            {
                int idFactura = siguienteIdFactura++;
                Invoice* factura = null;

                try
                {
                    factura = (Invoice*)Marshal.AllocHGlobal(sizeof(Invoice));
                    *factura = new Invoice(idFactura, servicio->ID, costoServicio + repuesto->Costo);
                }
                catch (Exception)
                {
                    siguienteIdFactura--; // Revert ID increment
                    if (factura != null) Marshal.FreeHGlobal((IntPtr)factura);
                    throw;
                }

                invoiceService.Push(factura);

                // Insert in log matrix with null check
                if (servicio->Detalles != null)
                {
                    logMatrix.Insert(servicio->Id_Vehiculo, repuesto->ID, SafeGetString(servicio->Detalles));
                }
                else
                {
                    logMatrix.Insert(servicio->Id_Vehiculo, repuesto->ID, "Sin detalles");
                }

                OnInvoiceCreated();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating invoice: {ex.Message}");
                OnShowMessage("Error al generar factura", MessageType.Error);
            }
        }

        private void ClearServiceFields()
        {
            cmbVehiculoServicio.Active = -1;
            cmbRepuestoServicio.Active = -1;
            txtDetallesServicio.Text = string.Empty;
            txtCostoServicio.Text = string.Empty;
        }

        protected virtual void OnShowMessage(string message, MessageType messageType)
        {
            ShowMessage?.Invoke(this, new MessageEventArgs(message, messageType));
        }

        protected virtual void OnServiceDataChanged()
        {
            ServiceDataChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnInvoiceCreated()
        {
            InvoiceCreated?.Invoke(this, EventArgs.Empty);
        }

        // Methods to refresh data when other entities change
        public void RefreshVehicleData()
        {

            LoadVehicleComboBox();

        }

        public void RefreshSparePartData()
        {


            LoadSparePartsComboBox();

        }

        private string SafeGetString(char* ptr)
        {
            if (ptr == null) return string.Empty;

            try
            {
                return new string(ptr);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting string: {ex.Message}");
                return "[Error]";
            }
        }

        // Method for refreshing all data
        public void RefreshData()
        {
            if (disposed) return;

            try
            {
                LoadVehicleComboBox();
                LoadSparePartsComboBox();
                LoadPendingServices();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing data: {ex.Message}");
                OnShowMessage("Error al actualizar datos", MessageType.Error);
            }
        }

        // Add at the end of the class
        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected new virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                try
                {
                    if (disposing)
                    {
                        // Dispose managed resources
                        cmbVehiculoServicio?.Dispose();
                        cmbRepuestoServicio?.Dispose();
                        txtDetallesServicio?.Dispose();
                        txtCostoServicio?.Dispose();

                        // Remove event handlers
                        if (btnGenerarServicio != null)
                        {
                            btnGenerarServicio.Clicked -= GenerarServicio_Clicked;
                            btnGenerarServicio.Dispose();
                        }

                        trvServicios?.Dispose();
                        lsServicios?.Dispose();

                        // Clear event handlers
                        ShowMessage = null;
                        ServiceDataChanged = null;
                        InvoiceCreated = null;
                    }

                    // Additional unmanaged resource cleanup if needed
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during disposal: {ex.Message}");
                }

                disposed = true;
            }
        }

        ~ServiceManagementView()
        {
            Dispose(false);
        }
    }
}