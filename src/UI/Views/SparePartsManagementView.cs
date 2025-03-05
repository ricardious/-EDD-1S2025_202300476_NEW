using System;
using Gtk;
using AutoGestPro.src.Models;
using AutoGestPro.src.DataStructures;
using AutoGestPro.src.Services;
using System.Runtime.InteropServices;
using AutoGestPro.src.UI.Common;

namespace AutoGestPro.src.UI.Views
{
    public unsafe class SparePartsManagementView : Box, IDisposable
    {


        // Services
        private readonly SparePartsService sparePartsService;

        // UI components
        private Entry txtNombreRepuesto;
        private Entry txtDetallesRepuesto;
        private Entry txtCostoRepuesto;
        private Button btnGuardarRepuesto;
        private TreeView trvRepuestos;
        private ListStore lsRepuestos;

        private bool disposed = false;

        // Events
        public event EventHandler<MessageEventArgs> ShowMessage;
        public event EventHandler<EventArgs> SparePartDataChanged;

        // Counter for next spare part ID
        private int siguienteIdRepuesto = 1;

        public SparePartsManagementView(SparePartsService sparePartsService) : base(Orientation.Vertical, 15)
        {
            try
            {
                this.sparePartsService = sparePartsService;
                Margin = 20;
                BuildInterface();
                LoadSpareParts();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing SparePartsManagementView: {ex.Message}");
                // Still show the basic UI, but inform the user
                Label errorLabel = new Label("Error loading spare parts. Please try again later.");
                PackStart(errorLabel, true, true, 10);
            }
        }

        private void BuildInterface()
        {
            // Title
            Label lblTitulo = new()
            {
                Markup = "<span font='18'>Gestión de Repuestos</span>",
                Halign = Align.Start
            };
            PackStart(lblTitulo, false, false, 0);

            // Panel with form and list
            Box panelBox = new(Orientation.Horizontal, 20);
            PackStart(panelBox, true, true, 0);

            // Spare part form
            Box formBox = new(Orientation.Vertical, 10);
            panelBox.PackStart(formBox, false, false, 0);

            Frame formFrame = new("Datos del Repuesto");
            formBox.PackStart(formFrame, false, false, 0);

            Box formInnerBox = new(Orientation.Vertical, 10);
            formInnerBox.Margin = 15;
            formFrame.Add(formInnerBox);

            // Form fields
            Grid formGrid = new()
            {
                RowSpacing = 10,
                ColumnSpacing = 10
            };
            formInnerBox.PackStart(formGrid, false, false, 10);

            Label lblNombre = new("Nombre:")
            {
                Halign = Align.Start
            };
            formGrid.Attach(lblNombre, 0, 0, 1, 1);

            txtNombreRepuesto = new Entry
            {
                PlaceholderText = "Ej: Filtro de aceite"
            };
            formGrid.Attach(txtNombreRepuesto, 1, 0, 1, 1);

            Label lblDetalles = new("Detalles:")
            {
                Halign = Align.Start
            };
            formGrid.Attach(lblDetalles, 0, 1, 1, 1);

            txtDetallesRepuesto = new Entry
            {
                PlaceholderText = "Descripción del repuesto"
            };
            formGrid.Attach(txtDetallesRepuesto, 1, 1, 1, 1);

            Label lblCosto = new("Costo ($):")
            {
                Halign = Align.Start
            };
            formGrid.Attach(lblCosto, 0, 2, 1, 1);

            txtCostoRepuesto = new Entry
            {
                PlaceholderText = "Ej: 25.50"
            };
            formGrid.Attach(txtCostoRepuesto, 1, 2, 1, 1);

            // Save button
            btnGuardarRepuesto = new Button("Guardar Repuesto");
            btnGuardarRepuesto.StyleContext.AddClass("suggested-action");
            btnGuardarRepuesto.Clicked += GuardarRepuesto_Clicked;
            formInnerBox.PackStart(btnGuardarRepuesto, false, false, 10);

            // Spare parts list
            Frame listaFrame = new("Lista de Repuestos");
            panelBox.PackStart(listaFrame, true, true, 0);

            Box listaBox = new(Orientation.Vertical, 10)
            {
                Margin = 15
            };
            listaFrame.Add(listaBox);

            ScrolledWindow sw = new()
            {
                ShadowType = ShadowType.EtchedIn
            };
            sw.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            listaBox.PackStart(sw, true, true, 0);

            // TreeView for spare parts
            lsRepuestos = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string));
            trvRepuestos = new TreeView(lsRepuestos)
            {
                HeadersVisible = true
            };

            trvRepuestos.AppendColumn("ID", new CellRendererText(), "text", 0);
            trvRepuestos.AppendColumn("Nombre", new CellRendererText(), "text", 1);
            trvRepuestos.AppendColumn("Detalles", new CellRendererText(), "text", 2);
            trvRepuestos.AppendColumn("Costo", new CellRendererText(), "text", 3);

            // Add selection handling
            trvRepuestos.Selection.Changed += (sender, e) =>
            {
                if (trvRepuestos.Selection.GetSelected(out TreeIter iter))
                {
                    // Fill form with selected spare part data
                    string id = (string)lsRepuestos.GetValue(iter, 0);
                    txtNombreRepuesto.Text = (string)lsRepuestos.GetValue(iter, 1);
                    txtDetallesRepuesto.Text = (string)lsRepuestos.GetValue(iter, 2);

                    // Extract just the numeric value from the cost (remove currency symbol)
                    string costStr = (string)lsRepuestos.GetValue(iter, 3);
                    txtCostoRepuesto.Text = decimal.Parse(costStr.Replace("Q", "").Trim()).ToString();
                }
            };

            sw.Add(trvRepuestos);
        }

        private void LoadSpareParts()
        {
            try
            {
                lsRepuestos.Clear();

                SparePart*[] spareParts = sparePartsService.GetSpareParts();
                if (spareParts == null || spareParts.Length == 0) return;

                foreach (SparePart* part in spareParts)
                {
                    if (part == null) continue;

                    try
                    {
                        lsRepuestos.AppendValues(
                            part->ID.ToString(),
                            part->Repuesto != null ? new string(part->Repuesto) : "Sin nombre",
                            part->Detalles != null ? new string(part->Detalles) : "Sin detalles",
                            part->Costo.ToString("C")
                        );

                        // Update next spare part ID
                        siguienteIdRepuesto = Math.Max(siguienteIdRepuesto, part->ID + 1);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading spare part: {ex.Message}");
                        // Continue processing other parts instead of crashing
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error in LoadSpareParts: {ex.Message}");
                OnShowMessage("Error al cargar los repuestos", MessageType.Error);
            }
        }

        private void GuardarRepuesto_Clicked(object sender, EventArgs e)
        {
            if (disposed) return; // Don't process events after disposal

            try
            {
                // Validate inputs
                string nombre = txtNombreRepuesto.Text?.Trim() ?? string.Empty;
                string detalles = txtDetallesRepuesto.Text?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(detalles))
                {
                    OnShowMessage("Nombre y detalles son campos obligatorios", MessageType.Error);
                    return;
                }

                if (!double.TryParse(txtCostoRepuesto.Text, out double costo) || costo <= 0)
                {
                    OnShowMessage("Por favor, ingrese un costo válido", MessageType.Error);
                    return;
                }

                // Create new spare part - wrapped in try-catch specifically for this operation
                int id = siguienteIdRepuesto++;
                try
                {
                    sparePartsService.CreateSparePart(id, nombre, detalles, costo);
                }
                catch (Exception ex)
                {
                    siguienteIdRepuesto--; // Revert ID increment
                    throw new Exception($"Error al crear repuesto: {ex.Message}", ex);
                }

                // Update ListView using Gtk.Application.Invoke to ensure UI updates on the main thread
                Application.Invoke(delegate
                {
                    lsRepuestos.AppendValues(
                        id.ToString(),
                        nombre,
                        detalles,
                        costo.ToString("C")
                    );

                    OnShowMessage("Repuesto registrado exitosamente", MessageType.Info);
                    LimpiarCamposRepuesto();
                    OnSparePartDataChanged();
                });
            }
            catch (Exception ex)
            {
                OnShowMessage("Error al registrar repuesto: " + ex.Message, MessageType.Error);
                Console.WriteLine($"Error in GuardarRepuesto_Clicked: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void LimpiarCamposRepuesto()
        {
            txtNombreRepuesto.Text = string.Empty;
            txtDetallesRepuesto.Text = string.Empty;
            txtCostoRepuesto.Text = string.Empty;
        }

        protected virtual void OnShowMessage(string message, MessageType messageType)
        {
            ShowMessage?.Invoke(this, new MessageEventArgs(message, messageType));
        }

        protected virtual void OnSparePartDataChanged()
        {
            SparePartDataChanged?.Invoke(this, EventArgs.Empty);
        }

        // Method for refreshing data
        public void RefreshData()
        {
            if (disposed) return;

            try
            {
                LoadSpareParts();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing spare parts data: {ex.Message}");
                OnShowMessage("Error al actualizar los datos de repuestos", MessageType.Error);
            }
        }

        // Track whether Dispose has been called


        // Implement IDisposable pattern
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
                    // Clean up managed resources
                    txtNombreRepuesto?.Dispose();
                    txtDetallesRepuesto?.Dispose();
                    txtCostoRepuesto?.Dispose();
                    btnGuardarRepuesto?.Dispose();
                    trvRepuestos?.Dispose();
                    lsRepuestos?.Dispose();

                    // Unsubscribe from events to prevent memory leaks
                    btnGuardarRepuesto.Clicked -= GuardarRepuesto_Clicked;

                    // Clean up other event handlers too
                    ShowMessage = null;
                    SparePartDataChanged = null;
                }

                // Clean up unmanaged resources if any

                disposed = true;
            }
        }

        // Add finalizer since we're using unsafe code
        ~SparePartsManagementView()
        {
            Dispose(false);
        }
    }
}