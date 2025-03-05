using System;
using System.IO;
using Gtk;
using Newtonsoft.Json.Linq;
using AutoGestPro.src.UI.Common;
using AutoGestPro.src.Services;
using System.Linq;

namespace AutoGestPro.src.UI.Views
{
    public unsafe class BulkLoadView : Box
    {
        // Private fields for services
        private readonly UserService userService;
        private readonly VehicleService vehicleService;
        private readonly SparePartsService sparePartsService;

        // UI components
        private ComboBoxText cmbEntityType;
        private Button btnSelectFile;
        private Button btnLoadData;
        private Label lblLoadStatus;
        private ScrolledWindow swPreview;
        private TextView txtPreview;

        // State
        private string selectedFilePath;

        // Event handlers
        public event EventHandler<MessageEventArgs> ShowMessage;

        public BulkLoadView(UserService userService, VehicleService vehicleService, SparePartsService sparePartsService)
            : base(Orientation.Vertical, 15)
        {
            // Store references to data structures
            this.userService = userService;
            this.vehicleService = vehicleService;
            this.sparePartsService = sparePartsService;

            // Configure view
            Margin = 20;
            BuildInterface();
        }

        private void BuildInterface()
        {
            // Title
            Label lblTitle = new()
            {
                Markup = "<span font='18'>Bulk Data Load</span>",
                Halign = Align.Start
            };
            PackStart(lblTitle, false, false, 0);

            // Entity type selection
            Box selectionBox = new(Orientation.Horizontal, 10);
            PackStart(selectionBox, false, false, 10);

            Label lblType = new("Entity type:");
            selectionBox.PackStart(lblType, false, false, 0);

            cmbEntityType = [];
            cmbEntityType.AppendText("Users");
            cmbEntityType.AppendText("Vehicles");
            cmbEntityType.AppendText("Spare Parts");
            cmbEntityType.Active = 0;
            selectionBox.PackStart(cmbEntityType, false, false, 0);

            btnSelectFile = new Button("Select JSON file");
            btnSelectFile.Clicked += SelectFile_Clicked;
            selectionBox.PackStart(btnSelectFile, false, false, 10);

            lblLoadStatus = new Label("No file selected");
            selectionBox.PackStart(lblLoadStatus, false, false, 0);

            // File preview
            Frame framePreview = new("JSON File Preview");
            PackStart(framePreview, true, true, 0);

            swPreview = new ScrolledWindow
            {
                ShadowType = ShadowType.EtchedIn
            };

            txtPreview = new TextView
            {
                Editable = false,
                WrapMode = WrapMode.Word
            };
            swPreview.Add(txtPreview);

            framePreview.Add(swPreview);

            // Load button
            Box actionsBox = new(Orientation.Horizontal, 0)
            {
                Halign = Align.End
            };
            PackStart(actionsBox, false, false, 10);

            btnLoadData = new Button("Load Data");
            btnLoadData.StyleContext.AddClass("suggested-action");
            btnLoadData.Sensitive = false;
            btnLoadData.Clicked += LoadData_Clicked;
            actionsBox.PackStart(btnLoadData, false, false, 0);
        }

        private void SelectFile_Clicked(object sender, EventArgs e)
        {
            Window parentWindow = this.Toplevel as Window;
            FileChooserDialog dialog = new("Select JSON File", parentWindow,
                FileChooserAction.Open,
                "Cancel", ResponseType.Cancel,
                "Select", ResponseType.Accept);

            dialog.Response += (s, args) =>
            {
                if ((ResponseType)args.ResponseId == ResponseType.Accept)
                {
                    selectedFilePath = dialog.Filename;
                    lblLoadStatus.Text = $"Selected file: {selectedFilePath}";

                    string json = File.ReadAllText(selectedFilePath);
                    txtPreview.Buffer.Text = json;
                    btnLoadData.Sensitive = true;
                }
                dialog.Destroy();
            };

            dialog.Run();
        }

        private void LoadData_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Show loading indicator
                lblLoadStatus.Text = "Processing...";

                // Check file size before loading
                FileInfo fileInfo = new FileInfo(selectedFilePath);
                long fileSizeInMB = fileInfo.Length / (1024 * 1024);

                if (fileSizeInMB > 10) // Warn for files larger than 10MB
                {
                    using (MessageDialog messageDialog = new MessageDialog(
                        this.Toplevel as Window,
                        DialogFlags.Modal,
                        MessageType.Warning,
                        ButtonsType.YesNo,
                        $"The selected file is {fileSizeInMB}MB which may use significant memory. Continue?"))
                    {
                        ResponseType response = (ResponseType)messageDialog.Run();
                        messageDialog.Destroy();

                        if (response != ResponseType.Yes)
                            return;
                    }
                }

                // Validate JSON structure based on selected entity type
                string entityType = cmbEntityType.ActiveText;
                if (!ValidateJsonStructure(selectedFilePath, entityType))
                {
                    OnShowMessage($"The file does not have the correct structure for {entityType}.", MessageType.Error);
                    return;
                }

                // Process documents based on entity type
                switch (entityType)
                {
                    case "Users":
                        LoadUsers(selectedFilePath);
                        break;

                    case "Vehicles":
                        LoadVehicles(selectedFilePath);
                        break;

                    case "Spare Parts":
                        LoadSpareParts(selectedFilePath);
                        break;
                }
            }
            catch (Exception ex)
            {
                OnShowMessage($"Error loading data: {ex.Message}", MessageType.Error);
            }
        }

        private bool ValidateJsonStructure(string jsonFilePath, string entityType)
        {
            try
            {
                using (StreamReader streamReader = new StreamReader(jsonFilePath))
                using (Newtonsoft.Json.JsonTextReader reader = new Newtonsoft.Json.JsonTextReader(reader: streamReader))
                {
                    // Check if file starts with an array
                    if (!reader.Read() || reader.TokenType != Newtonsoft.Json.JsonToken.StartArray)
                    {
                        OnShowMessage("Invalid JSON format: File must start with an array '['", MessageType.Error);
                        return false;
                    }

                    // Try to read the first object
                    if (!reader.Read() || reader.TokenType != Newtonsoft.Json.JsonToken.StartObject)
                    {
                        OnShowMessage("Invalid JSON format: No objects found in array", MessageType.Error);
                        return false;
                    }

                    // Load the first object to check its structure
                    JObject firstItem = JObject.Load(reader);

                    // Check required fields based on entity type
                    switch (entityType)
                    {
                        case "Users":
                            return ValidateUserFields(firstItem);
                        case "Vehicles":
                            return ValidateVehicleFields(firstItem);
                        case "Spare Parts":
                            return ValidateSparePartFields(firstItem);
                        default:
                            return false;
                    }
                }
            }
            catch (Exception ex)
            {
                OnShowMessage($"Error validating JSON structure: {ex.Message}", MessageType.Error);
                return false;
            }
        }

        private bool ValidateUserFields(JObject item)
        {
            string[] requiredFields = { "ID", "Nombres", "Apellidos", "Correo", "Contrasenia" };
            return ValidateRequiredFields(item, requiredFields);
        }

        private bool ValidateVehicleFields(JObject item)
        {
            string[] requiredFields = { "ID", "ID_Usuario", "Marca", "Modelo", "Placa" };
            return ValidateRequiredFields(item, requiredFields);
        }

        private bool ValidateSparePartFields(JObject item)
        {
            string[] requiredFields = { "ID", "Repuesto", "Detalles", "Costo" };
            return ValidateRequiredFields(item, requiredFields);
        }

        private bool ValidateRequiredFields(JObject item, string[] requiredFields)
        {
            foreach (string field in requiredFields)
            {
                if (!item.ContainsKey(field))
                {
                    OnShowMessage($"Missing required field: '{field}'", MessageType.Error);
                    return false;
                }
            }
            return true;
        }
        private void LoadUsers(string jsonFilePath)
        {
            try
            {
                using (StreamReader streamReader = new StreamReader(jsonFilePath))
                using (Newtonsoft.Json.JsonTextReader reader = new Newtonsoft.Json.JsonTextReader(streamReader))
                {
                    reader.Read(); // Read opening array token

                    int processedCount = 0;
                    int batchSize = 100;
                    int batchCount = 0;

                    while (reader.Read())
                    {
                        if (reader.TokenType == Newtonsoft.Json.JsonToken.StartObject)
                        {
                            JObject obj = JObject.Load(reader);

                            int id = (int)obj["ID"];
                            string firstName = (string)obj["Nombres"];
                            string lastName = (string)obj["Apellidos"];
                            string email = (string)obj["Correo"];
                            string password = (string)obj["Contrasenia"];

                            if (userService.GetUserById(id) == null)
                            {
                                userService.CreateUser(id, firstName, lastName, email, password);
                            }

                            processedCount++;
                            batchCount++;

                            if (batchCount >= batchSize)
                            {
                                Application.Invoke((s, e) =>
                                {
                                    lblLoadStatus.Text = $"Processed {processedCount} users...";
                                    while (Application.EventsPending())
                                        Application.RunIteration();
                                });

                                batchCount = 0;
                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                            }

                            obj = null;
                        }
                    }

                    OnShowMessage($"Users loaded successfully: {processedCount} items", MessageType.Info);
                }
            }
            catch (Exception ex)
            {
                OnShowMessage($"Error loading users: {ex.Message}", MessageType.Error);
            }
        }

        private void LoadVehicles(string jsonFilePath)
        {
            try
            {
                // Stream the vehicles file like you do with users and spare parts
                using (StreamReader streamReader = new StreamReader(jsonFilePath))
                using (Newtonsoft.Json.JsonTextReader reader = new Newtonsoft.Json.JsonTextReader(streamReader))
                {
                    reader.Read(); // Read opening array token

                    int processedCount = 0;
                    int batchSize = 100;
                    int batchCount = 0;

                    while (reader.Read())
                    {
                        if (reader.TokenType == Newtonsoft.Json.JsonToken.StartObject)
                        {
                            JObject obj = JObject.Load(reader);

                            int id = (int)obj["ID"];
                            int userId = (int)obj["ID_Usuario"];
                            string brand = (string)obj["Marca"];
                            string model = (string)obj["Modelo"];
                            string plate = (string)obj["Placa"];

                            if (vehicleService.GetVehicleById(id) == null)
                            {
                                vehicleService.CreateVehicle(id, userId, brand, model, plate);
                            }

                            processedCount++;
                            batchCount++;

                            if (batchCount >= batchSize)
                            {
                                Application.Invoke((s, e) =>
                                {
                                    lblLoadStatus.Text = $"Processed {processedCount} vehicles...";
                                    while (Application.EventsPending())
                                        Application.RunIteration();
                                });

                                batchCount = 0;
                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                            }

                            obj = null;
                        }
                    }

                    OnShowMessage($"Vehicles loaded successfully: {processedCount} items", MessageType.Info);
                }
            }
            catch (Exception ex)
            {
                OnShowMessage($"Error loading vehicles: {ex.Message}", MessageType.Error);
            }
        }
        private void LoadSpareParts(string jsonFilePath)
        {
            try
            {
                // Don't load the full JSON into memory - stream it
                using (StreamReader streamReader = new StreamReader(selectedFilePath))
                using (Newtonsoft.Json.JsonTextReader reader = new Newtonsoft.Json.JsonTextReader(streamReader))
                {
                    // Read the opening array token
                    reader.Read();

                    int processedCount = 0;
                    int batchSize = 100;
                    int batchCount = 0;

                    // Process each JSON object individually
                    while (reader.Read())
                    {
                        if (reader.TokenType == Newtonsoft.Json.JsonToken.StartObject)
                        {
                            JObject obj = JObject.Load(reader);

                            int id = (int)obj["ID"];
                            string name = (string)obj["Repuesto"];
                            string category = (string)obj["Detalles"];
                            double price = (double)obj["Costo"];

                            if (sparePartsService.Search(id) == null)
                            {
                                sparePartsService.CreateSparePart(id, name, category, price);
                            }

                            processedCount++;
                            batchCount++;

                            // Update UI periodically and force garbage collection
                            if (batchCount >= batchSize)
                            {
                                Application.Invoke((s, e) =>
                                {
                                    lblLoadStatus.Text = $"Processed {processedCount} spare parts...";
                                    // Process GTK events to keep UI responsive
                                    while (Application.EventsPending())
                                        Application.RunIteration();
                                });

                                batchCount = 0;
                                // Force garbage collection to reclaim memory
                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                            }

                            // Clear object reference to help garbage collection
                            obj = null;
                        }
                    }

                    OnShowMessage($"Spare parts loaded successfully: {processedCount} items", MessageType.Info);
                }
            }
            catch (Exception ex)
            {
                OnShowMessage($"Error loading spare parts: {ex.Message}", MessageType.Error);
            }
        }

        protected virtual void OnShowMessage(string message, MessageType messageType)
        {
            ShowMessage?.Invoke(this, new MessageEventArgs(message, messageType));
        }
    }

}
