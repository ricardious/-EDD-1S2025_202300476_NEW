using System;
using Gtk;
using AutoGestPro.src.Models;
using AutoGestPro.src.DataStructures;
using System.Runtime.InteropServices;
using AutoGestPro.src.UI.Common;  // Assuming you created the shared MessageEventArgs
using AutoGestPro.src.Services;

namespace AutoGestPro.src.UI.Views
{
    public unsafe class InvoiceManagementView : Box, IDisposable
    {
        // Services
        private readonly InvoiceService invoiceService;
        private readonly ServiceQueueService serviceQueueService;
        private readonly VehicleService vehicleService;

        // UI components
        private TreeView trvFacturas;
        private ListStore lsFacturas;
        private Button btnCancelarFactura;

        // Event handler reference for proper unsubscription
        private EventHandler selectionChangedHandler;
        private EventHandler btnCancelarFacturaClickedHandler;

        // Events
        public event EventHandler<MessageEventArgs> ShowMessage;
        public event EventHandler<EventArgs> InvoiceDataChanged;

        // Add disposed flag
        private bool disposed = false;

        public InvoiceManagementView(InvoiceService invoiceService, ServiceQueueService serviceQueueService, VehicleService vehicleService) : base(Orientation.Vertical, 15)
        {
            try
            {
                this.invoiceService = invoiceService;
                this.serviceQueueService = serviceQueueService;
                this.vehicleService = vehicleService;
                Margin = 20;
                BuildInterface();
                LoadInvoices();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing InvoiceManagementView: {ex.Message}");
                OnShowMessage("Error al inicializar la vista de facturas", MessageType.Error);
            }
        }

        private void BuildInterface()
        {
            // Title
            Label lblTitulo = new()
            {
                Markup = "<span font='18'>Facturación</span>",
                Halign = Align.Start
            };
            PackStart(lblTitulo, false, false, 0);

            // List of invoices
            Label lblFacturas = new("Facturas generadas:");
            PackStart(lblFacturas, false, false, 10);

            ScrolledWindow sw = new ScrolledWindow
            {
                ShadowType = ShadowType.EtchedIn
            };
            sw.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            PackStart(sw, true, true, 0);

            // TreeView for invoices
            lsFacturas = new ListStore(typeof(string), typeof(string), typeof(string));
            trvFacturas = new TreeView(lsFacturas)
            {
                HeadersVisible = true
            };

            trvFacturas.AppendColumn("ID", new CellRendererText(), "text", 0);
            trvFacturas.AppendColumn("ID Servicio", new CellRendererText(), "text", 1);
            trvFacturas.AppendColumn("Total", new CellRendererText(), "text", 2);

            // Store handler reference for proper cleanup
            selectionChangedHandler = (sender, e) =>
            {
                try
                {
                    if (!disposed && trvFacturas != null && trvFacturas.Selection.GetSelected(out TreeIter iter))
                    {
                        // Get values safely
                        string id = SafeGetValue(lsFacturas, iter, 0);
                        string serviceId = SafeGetValue(lsFacturas, iter, 1);
                        string total = SafeGetValue(lsFacturas, iter, 2);
                        Console.WriteLine($"Selected invoice: {id}, Service: {serviceId}, Total: {total}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in selection: {ex.Message}");
                }
            };

            trvFacturas.Selection.Changed += selectionChangedHandler;
            // Add this after setting up the TreeView in BuildInterface method

            // Handle double clicks on rows
            trvFacturas.ButtonPressEvent += (sender, args) =>
            {
                // Check for double-click
                if (args.Event.Type == Gdk.EventType.TwoButtonPress &&
                    args.Event.Button == 1) // Left button
                {
                    try
                    {
                        if (trvFacturas.Selection.GetSelected(out TreeIter iter))
                        {
                            // Get values safely
                            string id = SafeGetValue(lsFacturas, iter, 0);
                            string serviceId = SafeGetValue(lsFacturas, iter, 1);
                            string total = SafeGetValue(lsFacturas, iter, 2);

                            // Open invoice detail window
                            ShowInvoiceDetailDialog(id, serviceId, total);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error handling double-click: {ex.Message}");
                    }
                }
            };
            sw.Add(trvFacturas);

            // Button to cancel invoice
            btnCancelarFactura = new Button("Cancelar Última Factura");
            btnCancelarFactura.StyleContext.AddClass("destructive-action");

            // Store handler reference for proper cleanup
            btnCancelarFacturaClickedHandler = CancelarFactura_Clicked;
            btnCancelarFactura.Clicked += CancelarFactura_Clicked;
            btnCancelarFactura.Halign = Align.Center;
            btnCancelarFactura.MarginTop = 10;
            PackStart(btnCancelarFactura, false, false, 10);

            // Add some info text
            Label lblInfo = new("Las facturas se generan automáticamente al crear un nuevo servicio.")
            {
                Halign = Align.Start
            };
            lblInfo.StyleContext.AddClass("dim-label");
            PackStart(lblInfo, false, false, 20);
        }


        private void ShowInvoiceDetailDialog(string invoiceId, string serviceId, string totalAmount)
        {
            try
            {
                Window parentWindow = this.Toplevel as Window;
                // Create a new dialog window
                Dialog dialog = new Dialog(
                    "Factura #" + invoiceId,
                    parentWindow,
                    DialogFlags.Modal | DialogFlags.DestroyWithParent,
                    "Cerrar", ResponseType.Close
                );

                // Set size and make it look nice
                dialog.SetDefaultSize(350, 400);
                dialog.ContentArea.MarginStart = 20;
                dialog.ContentArea.MarginEnd = 20;
                dialog.ContentArea.MarginTop = 20;
                dialog.ContentArea.MarginBottom = 10;

                // Create the content box with invoice details
                Box contentBox = new Box(Orientation.Vertical, 10);
                dialog.ContentArea.Add(contentBox);

                // Add logo or header
                Label headerLabel = new Label
                {
                    Markup = "<span font='16' weight='bold'>Auto Gest Pro</span>",
                    Halign = Align.Center
                };
                contentBox.PackStart(headerLabel, false, false, 5);

                Label subheaderLabel = new Label
                {
                    Text = "Sistema de Talleres",
                    Halign = Align.Center
                };
                contentBox.PackStart(subheaderLabel, false, false, 0);

                // Add separator
                Separator sep1 = new Separator(Orientation.Horizontal);
                contentBox.PackStart(sep1, false, false, 10);

                // Invoice details - header
                Label detailsHeader = new Label
                {
                    Markup = "<span font='12' weight='bold'>Detalles de la Factura</span>",
                    Halign = Align.Start
                };
                contentBox.PackStart(detailsHeader, false, false, 5);

                // Invoice date
                string currentDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                // Create a grid for details
                Grid detailsGrid = new Grid
                {
                    RowSpacing = 5,
                    ColumnSpacing = 10,
                    Halign = Align.Fill,
                    ColumnHomogeneous = false
                };
                contentBox.PackStart(detailsGrid, false, false, 5);

                // Row 0: Invoice ID
                Label lblInvoiceId = new Label("No. Factura:") { Halign = Align.Start };
                Label valInvoiceId = new Label(invoiceId) { Halign = Align.Start };
                detailsGrid.Attach(lblInvoiceId, 0, 0, 1, 1);
                detailsGrid.Attach(valInvoiceId, 1, 0, 1, 1);

                // Row 1: Service ID
                Label lblServiceId = new Label("No. Servicio:") { Halign = Align.Start };
                Label valServiceId = new Label(serviceId) { Halign = Align.Start };
                detailsGrid.Attach(lblServiceId, 0, 1, 1, 1);
                detailsGrid.Attach(valServiceId, 1, 1, 1, 1);

                // Row 2: Date
                Label lblDate = new Label("Fecha:") { Halign = Align.Start };
                Label valDate = new Label(currentDate) { Halign = Align.Start };
                detailsGrid.Attach(lblDate, 0, 2, 1, 1);
                detailsGrid.Attach(valDate, 1, 2, 1, 1);

                // Add separator
                Separator sep2 = new Separator(Orientation.Horizontal);
                contentBox.PackStart(sep2, false, false, 10);

                // Try to fetch more details about the service/order
                try
                {
                    int serviceIdNum = int.Parse(serviceId);

                    // Actually retrieve the service using your service object
                    Service* service = serviceQueueService.Search(serviceIdNum);

                    if (service != null)
                    {
                        // If available, add vehicle details
                        // Actually retrieve the vehicle using your vehicle service
                        Vehicle* vehicle = vehicleService.GetVehicleById(service->Id_Vehiculo);

                        if (vehicle != null)
                        {
                            string placa = SafeGetString(vehicle->Placa);
                            string marca = SafeGetString(vehicle->Marca);
                            string modelo = SafeGetString(vehicle->Modelo);

                            Label lblVehicle = new Label
                            {
                                Markup = "<span weight='bold'>Vehículo:</span>",
                                Halign = Align.Start
                            };
                            contentBox.PackStart(lblVehicle, false, false, 5);

                            Label lblVehicleDetails = new Label
                            {
                                Text = $"Placa: {placa}\nMarca: {marca}\nModelo: {modelo}",
                                Halign = Align.Start
                            };
                            contentBox.PackStart(lblVehicleDetails, false, false, 5);
                        }

                        // Add service details if available
                        string serviceDetails = SafeGetString(service->Detalles);
                        if (!string.IsNullOrEmpty(serviceDetails))
                        {
                            Label lblServiceDetails = new Label
                            {
                                Markup = "<span weight='bold'>Detalle del Servicio:</span>",
                                Halign = Align.Start
                            };
                            contentBox.PackStart(lblServiceDetails, false, false, 5);

                            Label valServiceDetails = new Label
                            {
                                Text = serviceDetails,
                                Halign = Align.Start,
                                Wrap = true
                            };
                            contentBox.PackStart(valServiceDetails, false, false, 5);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching service details: {ex.Message}");
                    // Continue with basic invoice display
                }

                // Another separator
                Separator sep3 = new Separator(Orientation.Horizontal);
                contentBox.PackStart(sep3, false, false, 10);

                // Total amount (in bold)
                Box totalBox = new Box(Orientation.Horizontal, 0);
                totalBox.PackEnd(new Label
                {
                    Markup = $"<span font='14' weight='bold'>Total: {totalAmount}</span>",
                    Halign = Align.End
                }, false, false, 0);
                contentBox.PackStart(totalBox, false, false, 10);

                // Show thank you message
                Label thanksLabel = new Label
                {
                    Text = "¡Gracias por su preferencia!",
                    Halign = Align.Center
                };
                contentBox.PackStart(thanksLabel, false, false, 20);

                // Show the dialog
                dialog.ShowAll();
                dialog.Run();
                dialog.Destroy();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error displaying invoice detail: {ex.Message}");
                OnShowMessage("Error al mostrar detalles de factura", MessageType.Error);
            }
        }

        // Helper method for safe string extraction from pointers
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
                return string.Empty;
            }
        }

        private void LoadInvoices()
        {
            try
            {
                if (disposed || lsFacturas == null) return;

                lsFacturas.Clear();

                Invoice*[] invoices = invoiceService.GetAllInvoices();
                if (invoices == null) return;

                foreach (Invoice* invoice in invoices)
                {
                    if (invoice == null) continue;

                    try
                    {
                        // Application.Invoke ensures UI updates happen on the UI thread
                        Application.Invoke(delegate
                        {
                            lsFacturas.AppendValues(
                                invoice->ID.ToString(),
                                invoice->ID_Orden.ToString(),
                                invoice->Total.ToString("C")
                            );
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding invoice to list: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading invoices: {ex.Message}");
                OnShowMessage("Error al cargar las facturas", MessageType.Error);
            }
        }

        private void CancelarFactura_Clicked(object sender, EventArgs e)
        {
            if (disposed) return;

            try
            {
                Invoice* factura = invoiceService.Pop();
                if (factura != null)
                {
                    OnShowMessage($"Factura #{factura->ID} cancelada con éxito", MessageType.Info);

                    // Update ListView by removing the last row (since we're using a stack)
                    Application.Invoke(delegate
                    {
                        if (lsFacturas != null && lsFacturas.IterNChildren() > 0)
                        {
                            TreeIter iter;
                            if (lsFacturas.GetIterFirst(out iter))
                            {
                                // Move to the last row
                                int lastIndex = lsFacturas.IterNChildren() - 1;
                                for (int i = 0; i < lastIndex; i++)
                                {
                                    if (!lsFacturas.IterNext(ref iter))
                                        break;
                                }
                                lsFacturas.Remove(ref iter);
                            }
                        }

                        OnInvoiceDataChanged();
                    });
                }
                else
                {
                    OnShowMessage("No hay facturas pendientes", MessageType.Info);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error canceling invoice: {ex.Message}");
                OnShowMessage("Error al cancelar la factura", MessageType.Error);
            }
        }

        protected virtual void OnShowMessage(string message, MessageType messageType)
        {
            try
            {
                ShowMessage?.Invoke(this, new MessageEventArgs(message, messageType));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing message: {ex.Message}");
            }
        }

        protected virtual void OnInvoiceDataChanged()
        {
            try
            {
                InvoiceDataChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notifying data changed: {ex.Message}");
            }
        }

        // Method to refresh data
        public void RefreshData()
        {
            if (disposed) return;

            try
            {
                LoadInvoices();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing data: {ex.Message}");
            }
        }

        // Method to add a new invoice programmatically (used when service creates an invoice)
        public void AddInvoice(int id, int orderId, double total)
        {
            if (disposed) return;

            try
            {
                Application.Invoke(delegate
                {
                    lsFacturas?.AppendValues(
                        id.ToString(),
                        orderId.ToString(),
                        total.ToString("C")
                    );
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding invoice: {ex.Message}");
            }
        }

        private string SafeGetValue(ListStore store, TreeIter iter, int column)
        {
            try
            {
                if (store == null) return string.Empty;
                object value = store.GetValue(iter, column);
                return value?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected new virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    try
                    {
                        // Unsubscribe from events
                        if (trvFacturas != null && trvFacturas.Selection != null && selectionChangedHandler != null)
                        {
                            trvFacturas.Selection.Changed -= selectionChangedHandler;
                        }

                        if (btnCancelarFactura != null && btnCancelarFacturaClickedHandler != null)
                        {
                            btnCancelarFactura.Clicked -= btnCancelarFacturaClickedHandler;
                        }

                        // Dispose managed resources
                        trvFacturas?.Dispose();
                        lsFacturas?.Dispose();
                        btnCancelarFactura?.Dispose();

                        // Clear event handlers
                        ShowMessage = null;
                        InvoiceDataChanged = null;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during disposal: {ex.Message}");
                    }
                }

                disposed = true;
            }

            base.Dispose(disposing);
        }

        ~InvoiceManagementView()
        {
            Dispose(false);
        }
    }
}