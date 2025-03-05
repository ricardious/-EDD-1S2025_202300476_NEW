using System;
using Gtk;
using AutoGestPro.src.Models;
using AutoGestPro.src.UI.Common;  // Assuming you created the shared MessageEventArgs
using AutoGestPro.src.Services;

namespace AutoGestPro.src.UI.Views
{
    public unsafe class VehicleManagementView : Box
    {
        // Services
        private readonly VehicleService vehicleService;
        private readonly UserService userService;

        // UI components
        private ComboBoxText cmbUsuarioVehiculo;
        private Entry txtMarca;
        private Entry txtModelo;
        private Entry txtPlaca;
        private Button btnGuardarVehiculo;
        private TreeView trvVehiculos;
        private ListStore lsVehiculos;

        // Events
        public event EventHandler<MessageEventArgs> ShowMessage;
        public event EventHandler<EventArgs> VehicleDataChanged;

        // Counter for next vehicle ID
        private int siguienteIdVehiculo = 1;

        public VehicleManagementView(VehicleService vehicleService, UserService userService) : base(Orientation.Vertical, 15)
        {
            this.vehicleService = vehicleService;
            this.userService = userService;
            Margin = 20;

            try
            {
                BuildInterface();
                LoadVehicles();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing vehicle management: {ex.Message}");
                OnShowMessage("Error al inicializar la gestión de vehículos", MessageType.Error);
            }
        }

        private void BuildInterface()
        {
            // Title
            Label lblTitulo = new()
            {
                Markup = "<span font='18'>Gestión de Vehículos</span>",
                Halign = Align.Start
            };
            PackStart(lblTitulo, false, false, 0);

            // Panel with form and list
            Box panelBox = new(Orientation.Horizontal, 20);
            PackStart(panelBox, true, true, 0);

            // Vehicle form
            Box formBox = new(Orientation.Vertical, 10);
            panelBox.PackStart(formBox, false, false, 0);

            Frame formFrame = new("Datos del Vehículo");
            formBox.PackStart(formFrame, false, false, 0);

            Box formInnerBox = new(Orientation.Vertical, 10)
            {
                Margin = 15
            };
            formFrame.Add(formInnerBox);

            // Form fields
            Grid formGrid = new()
            {
                RowSpacing = 10,
                ColumnSpacing = 10
            };
            formInnerBox.PackStart(formGrid, false, false, 10);

            Label lblUsuario = new("Propietario:")
            {
                Halign = Align.Start
            };
            formGrid.Attach(lblUsuario, 0, 0, 1, 1);

            cmbUsuarioVehiculo = [];
            formGrid.Attach(cmbUsuarioVehiculo, 1, 0, 1, 1);

            Label lblMarca = new("Marca:")
            {
                Halign = Align.Start
            };
            formGrid.Attach(lblMarca, 0, 1, 1, 1);

            txtMarca = new Entry
            {
                PlaceholderText = "Ej: Toyota"
            };
            formGrid.Attach(txtMarca, 1, 1, 1, 1);

            Label lblModelo = new("Modelo:")
            {
                Halign = Align.Start
            };
            formGrid.Attach(lblModelo, 0, 2, 1, 1);

            txtModelo = new Entry
            {
                PlaceholderText = "Ej: Corolla"
            };
            formGrid.Attach(txtModelo, 1, 2, 1, 1);

            Label lblPlaca = new("Placa:")
            {
                Halign = Align.Start
            };
            formGrid.Attach(lblPlaca, 0, 3, 1, 1);

            txtPlaca = new Entry
            {
                PlaceholderText = "Ej: ABC123"
            };
            formGrid.Attach(txtPlaca, 1, 3, 1, 1);

            // Save button
            btnGuardarVehiculo = new Button("Guardar Vehículo");
            btnGuardarVehiculo.StyleContext.AddClass("suggested-action");
            btnGuardarVehiculo.Clicked += GuardarVehiculo_Clicked;
            formInnerBox.PackStart(btnGuardarVehiculo, false, false, 10);

            // Vehicle list
            Frame listaFrame = new("Lista de Vehículos");
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

            // TreeView for vehicles
            lsVehiculos = new ListStore(
                typeof(string),  // ID
                typeof(string),  // Owner
                typeof(string),  // Make
                typeof(string),  // Model
                typeof(string)   // License plate
            );

            trvVehiculos = new TreeView(lsVehiculos)
            {
                HeadersVisible = true
            };

            trvVehiculos.AppendColumn("ID", new CellRendererText(), "text", 0);
            trvVehiculos.AppendColumn("Propietario", new CellRendererText(), "text", 1);
            trvVehiculos.AppendColumn("Marca", new CellRendererText(), "text", 2);
            trvVehiculos.AppendColumn("Modelo", new CellRendererText(), "text", 3);
            trvVehiculos.AppendColumn("Placa", new CellRendererText(), "text", 4);

            sw.Add(trvVehiculos);

            // Load initial data
            LoadUserComboBox();
        }

        private void LoadVehicles()
        {
            lsVehiculos.Clear();

            Vehicle*[] vehicles = vehicleService.GetAllVehicles();
            if (vehicles == null || vehicles.Length == 0) return;

            foreach (Vehicle* vehicle in vehicles)
            {
                // Add null check before dereferencing
                if (vehicle == null) continue;

                // Find owner name
                string ownerName = "Desconocido";
                User* owner = userService.GetUserById(vehicle->ID_Usuario);

                // Safe string conversion with null checks
                if (owner != null)
                {
                    try
                    {
                        string nombres = owner->Nombres != null ? new string(owner->Nombres) : "";
                        string apellidos = owner->Apellidos != null ? new string(owner->Apellidos) : "";
                        ownerName = $"{vehicle->ID_Usuario} - {nombres} {apellidos}";
                    }
                    catch
                    {
                        ownerName = $"{vehicle->ID_Usuario} - [Error de memoria]";
                    }
                }

                // Add try-catch for string conversions
                try
                {
                    lsVehiculos.AppendValues(
                        vehicle->ID.ToString(),
                        ownerName,
                        vehicle->Marca != null ? new string(vehicle->Marca) : "",
                        vehicle->Modelo != null ? new string(vehicle->Modelo) : "",
                        vehicle->Placa != null ? new string(vehicle->Placa) : ""
                    );

                    // Update next vehicle ID
                    siguienteIdVehiculo = Math.Max(siguienteIdVehiculo, vehicle->ID + 1);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading vehicle data: {ex.Message}");
                }
            }
        }
        private void LoadUserComboBox()
        {
            cmbUsuarioVehiculo.RemoveAll();

            try
            {
                User*[] users = userService.GetAllUsers();
                if (users == null || users.Length == 0) return;

                foreach (User* user in users)
                {
                    if (user == null) continue;

                    try
                    {
                        string nombres = user->Nombres != null ? new string(user->Nombres) : "";
                        string apellidos = user->Apellidos != null ? new string(user->Apellidos) : "";
                        cmbUsuarioVehiculo.AppendText($"{user->ID} - {nombres} {apellidos}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing user data: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
            }
        }
        private void GuardarVehiculo_Clicked(object sender, EventArgs e)
        {
            try
            {
                int idUsuario = -1;
                if (cmbUsuarioVehiculo.ActiveText != null &&
                    int.TryParse(cmbUsuarioVehiculo.ActiveText.Split('-')[0].Trim(), out idUsuario))
                {
                    string marca = txtMarca.Text?.Trim() ?? string.Empty;
                    string modelo = txtModelo.Text?.Trim() ?? string.Empty;
                    string placa = txtPlaca.Text?.Trim() ?? string.Empty;

                    if (string.IsNullOrEmpty(marca) || string.IsNullOrEmpty(modelo) || string.IsNullOrEmpty(placa))
                    {
                        OnShowMessage("Todos los campos son obligatorios", MessageType.Error);
                        return;
                    }

                    // Create vehicle through service
                    int id = siguienteIdVehiculo++;
                    vehicleService.CreateVehicle(id, idUsuario, marca, modelo, placa);

                    // Update ListView using Gtk.Application.Invoke to ensure UI updates on the main thread
                    Application.Invoke(delegate
                    {
                        lsVehiculos.AppendValues(
                            id.ToString(),
                            cmbUsuarioVehiculo.ActiveText,
                            marca,
                            modelo,
                            placa
                        );

                        OnShowMessage("Vehículo registrado exitosamente", MessageType.Info);
                        ClearVehicleFields();
                        OnVehicleDataChanged();
                    });
                }
                else
                {
                    OnShowMessage("Por favor, seleccione un propietario válido", MessageType.Error);
                }
            }
            catch (Exception ex)
            {
                OnShowMessage("Error al registrar vehículo: " + ex.Message, MessageType.Error);
            }
        }

        private void ClearVehicleFields()
        {
            cmbUsuarioVehiculo.Active = -1;
            txtMarca.Text = string.Empty;
            txtModelo.Text = string.Empty;
            txtPlaca.Text = string.Empty;
        }

        protected virtual void OnShowMessage(string message, MessageType messageType)
        {
            ShowMessage?.Invoke(this, new MessageEventArgs(message, messageType));
        }

        protected virtual void OnVehicleDataChanged()
        {
            VehicleDataChanged?.Invoke(this, EventArgs.Empty);
        }

        // Method for updating the view when user data changes
        public void RefreshUserData()
        {
            LoadUserComboBox();

        }

        // Method for refreshing all data
        public void RefreshData()
        {
            LoadUserComboBox();
            LoadVehicles();
        }
    }
}