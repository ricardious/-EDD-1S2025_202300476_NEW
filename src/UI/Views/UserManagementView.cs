using System;
using Gtk;
using AutoGestPro.src.Models;
using AutoGestPro.src.DataStructures;
using AutoGestPro.src.UI.Common;
using System.Runtime.InteropServices;
using AutoGestPro.src.Services;

namespace AutoGestPro.src.UI.Views
{
    public unsafe class UserManagementView : Box, IDisposable
    {
        private UserService userService;

        // UI components
        private Entry txtBuscarUsuarioId;
        private Button btnBuscarUsuario;
        private Entry txtNombres;
        private Entry txtApellidos;
        private Entry txtCorreo;
        private Entry txtContrasenia;
        private Button btnGuardarUsuario;
        private Button btnEditarUsuario;
        private Button btnEliminarUsuario;
        private TreeView trvUsuarios;
        private ListStore lsUsuarios;

        // Events
        public event EventHandler<MessageEventArgs> ShowMessage;
        public event EventHandler<EventArgs> UserDataChanged;

        // Counter for next user ID
        private int siguienteIdUsuario = 1;



        public UserManagementView(UserService userService) : base(Orientation.Vertical, 15)
        {
            this.userService = userService;
            Margin = 20;
            BuildInterface();
            CargarDatosUsuarios();
        }

        private void BuildInterface()
        {
            // Title
            Label lblTitulo = new Label();
            lblTitulo.Markup = "<span font='18'>Gestión de Usuarios</span>";
            lblTitulo.Halign = Align.Start;
            PackStart(lblTitulo, false, false, 0);

            // Panel for form and list
            Box panelBox = new Box(Orientation.Horizontal, 20);
            PackStart(panelBox, true, true, 0);

            // User form
            Box formBox = new Box(Orientation.Vertical, 10);
            panelBox.PackStart(formBox, false, false, 0);

            Frame formFrame = new Frame("Datos del Usuario");
            formBox.PackStart(formFrame, false, false, 0);

            Box formInnerBox = new Box(Orientation.Vertical, 10);
            formInnerBox.Margin = 15;
            formFrame.Add(formInnerBox);

            // Search by ID
            Box busquedaBox = new Box(Orientation.Horizontal, 5);
            formInnerBox.PackStart(busquedaBox, false, false, 0);

            Label lblBuscarId = new Label("ID del Usuario:");
            busquedaBox.PackStart(lblBuscarId, false, false, 0);

            txtBuscarUsuarioId = new Entry();
            txtBuscarUsuarioId.PlaceholderText = "Buscar por ID";
            txtBuscarUsuarioId.WidthChars = 10;
            busquedaBox.PackStart(txtBuscarUsuarioId, true, true, 0);

            btnBuscarUsuario = new Button("Buscar");
            btnBuscarUsuario.Clicked += BuscarUsuario_Clicked;
            busquedaBox.PackStart(btnBuscarUsuario, false, false, 0);

            // Form fields
            Grid formGrid = new()
            {
                RowSpacing = 10,
                ColumnSpacing = 10
            };
            formInnerBox.PackStart(formGrid, false, false, 10);

            Label lblNombres = new("Nombres:")
            {
                Halign = Align.Start
            };
            formGrid.Attach(lblNombres, 0, 0, 1, 1);

            txtNombres = new Entry
            {
                PlaceholderText = "Ingrese nombres"
            };
            formGrid.Attach(txtNombres, 1, 0, 1, 1);

            Label lblApellidos = new("Apellidos:")
            {
                Halign = Align.Start
            };
            formGrid.Attach(lblApellidos, 0, 1, 1, 1);

            txtApellidos = new Entry
            {
                PlaceholderText = "Ingrese apellidos"
            };
            formGrid.Attach(txtApellidos, 1, 1, 1, 1);

            Label lblCorreo = new("Correo:")
            {
                Halign = Align.Start
            };
            formGrid.Attach(lblCorreo, 0, 2, 1, 1);

            txtCorreo = new Entry();
            txtCorreo.PlaceholderText = "ejemplo@mail.com";
            formGrid.Attach(txtCorreo, 1, 2, 1, 1);

            Label lblContrasenia = new("Contraseña:")
            {
                Halign = Align.Start
            };
            formGrid.Attach(lblContrasenia, 0, 3, 1, 1);

            txtContrasenia = new Entry
            {
                PlaceholderText = "Contraseña",
                Visibility = false
            };
            formGrid.Attach(txtContrasenia, 1, 3, 1, 1);

            // Action buttons
            Box botonesBox = new(Orientation.Horizontal, 10)
            {
                Homogeneous = true
            };
            formInnerBox.PackStart(botonesBox, false, false, 10);

            btnGuardarUsuario = new Button("Guardar Nuevo");
            btnGuardarUsuario.StyleContext.AddClass("suggested-action");
            btnGuardarUsuario.Clicked += GuardarUsuario_Clicked;
            botonesBox.PackStart(btnGuardarUsuario, true, true, 0);

            btnEditarUsuario = new Button("Actualizar");
            btnEditarUsuario.Clicked += EditarUsuario_Clicked;
            botonesBox.PackStart(btnEditarUsuario, true, true, 0);

            btnEliminarUsuario = new Button("Eliminar");
            btnEliminarUsuario.StyleContext.AddClass("destructive-action");
            btnEliminarUsuario.Clicked += EliminarUsuario_Clicked;
            botonesBox.PackStart(btnEliminarUsuario, true, true, 0);

            // User list
            Frame listaFrame = new("Lista de Usuarios");
            panelBox.PackStart(listaFrame, true, true, 0);

            Box listaBox = new Box(Orientation.Vertical, 10)
            {
                Margin = 15
            };
            listaFrame.Add(listaBox);

            ScrolledWindow sw = new ScrolledWindow();
            sw.ShadowType = ShadowType.EtchedIn;
            sw.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            listaBox.PackStart(sw, true, true, 0);

            // TreeView for users
            lsUsuarios = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string), typeof(string));
            trvUsuarios = new TreeView(lsUsuarios)
            {
                HeadersVisible = true
            };

            trvUsuarios.AppendColumn("ID", new CellRendererText(), "text", 0);
            trvUsuarios.AppendColumn("Nombres", new CellRendererText(), "text", 1);
            trvUsuarios.AppendColumn("Apellidos", new CellRendererText(), "text", 2);
            trvUsuarios.AppendColumn("Correo", new CellRendererText(), "text", 3);

            // Selection in TreeView
            trvUsuarios.Selection.Changed += (sender, e) =>
            {
                if (trvUsuarios.Selection.GetSelected(out TreeIter iter))
                {
                    txtBuscarUsuarioId.Text = (string)lsUsuarios.GetValue(iter, 0);
                    txtNombres.Text = (string)lsUsuarios.GetValue(iter, 1);
                    txtApellidos.Text = (string)lsUsuarios.GetValue(iter, 2);
                    txtCorreo.Text = (string)lsUsuarios.GetValue(iter, 3);
                    txtContrasenia.Text = (string)lsUsuarios.GetValue(iter, 4);
                }
            };

            sw.Add(trvUsuarios);
        }

        private void CargarDatosUsuarios()
        {
            lsUsuarios.Clear();

            User*[] usuarios = userService.GetAllUsers();
            foreach (User* usuario in usuarios)
            {
                lsUsuarios.AppendValues(
                    usuario->ID.ToString(),
                    new string(usuario->Nombres),
                    new string(usuario->Apellidos),
                    new string(usuario->Correo),
                    new string(usuario->Contrasenia)
                );

                // Update next user ID
                siguienteIdUsuario = Math.Max(siguienteIdUsuario, usuario->ID + 1);
            }
        }

        private void BuscarUsuario_Clicked(object sender, EventArgs e)
        {
            int idUsuario = -1;
            if (int.TryParse(txtBuscarUsuarioId.Text, out idUsuario))
            {
                User* usuario = userService.GetUserById(idUsuario);
                if (usuario != null)
                {
                    txtNombres.Text = new string(usuario->Nombres);
                    txtApellidos.Text = new string(usuario->Apellidos);
                    txtCorreo.Text = new string(usuario->Correo);
                    txtContrasenia.Text = new string(usuario->Contrasenia);
                }
                else
                {
                    OnShowMessage("Usuario no encontrado", MessageType.Error);
                }
            }
            else
            {
                OnShowMessage("Por favor, ingrese un ID válido", MessageType.Error);
            }
        }

        private void GuardarUsuario_Clicked(object sender, EventArgs e)
        {
            try
            {
                int id = siguienteIdUsuario++;
                string nombres = txtNombres.Text;
                string apellidos = txtApellidos.Text;
                string correo = txtCorreo.Text;
                string contrasenia = txtContrasenia.Text;

                if (string.IsNullOrEmpty(nombres) || string.IsNullOrEmpty(apellidos) ||
                   string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasenia))
                {
                    OnShowMessage("Todos los campos son obligatorios", MessageType.Error);
                    return;
                }

                User* newUser = userService.CreateUser(id, nombres, apellidos, correo, contrasenia);

                if (newUser != null)
                {

                    //lsUsuarios.AppendValues([id.ToString(), nombres, apellidos, correo, contrasenia]);
                    OnShowMessage("Usuario registrado exitosamente", MessageType.Info);

                    LimpiarCamposUsuario();

                }
                else
                {
                    OnShowMessage("Error al registrar usuario", MessageType.Error);
                }
            }
            catch (Exception ex)
            {
                OnShowMessage("Error al registrar usuario: " + ex.Message, MessageType.Error);
            }
        }

        private void EditarUsuario_Clicked(object sender, EventArgs e)
        {
            int idUsuario = -1;
            if (int.TryParse(txtBuscarUsuarioId.Text, out idUsuario))
            {
                User* usuario = userService.GetUserById(idUsuario);
                if (usuario != null)
                {
                    string nombres = txtNombres.Text;
                    string apellidos = txtApellidos.Text;
                    string correo = txtCorreo.Text;
                    string contrasenia = txtContrasenia.Text;

                    if (string.IsNullOrEmpty(nombres) || string.IsNullOrEmpty(apellidos) ||
                       string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasenia))
                    {
                        OnShowMessage("Todos los campos son obligatorios", MessageType.Error);
                        return;
                    }

                    // Crear un usuario actualizado 
                    User* updatedUser = (User*)Marshal.AllocHGlobal(sizeof(User));
                    *updatedUser = new User(idUsuario, nombres, apellidos, correo, contrasenia);

                    // Actualizar usando el servicio
                    bool success = userService.UpdateUser(idUsuario, updatedUser);

                    // Liberar la memoria del objeto temporal creado para actualizar
                    Marshal.FreeHGlobal((IntPtr)updatedUser);

                    if (success)
                    {
                        // Actualizar TreeView manualmente
                        lsUsuarios.Foreach((model, path, iter) =>
                        {
                            string id = (string)model.GetValue(iter, 0);
                            if (id == idUsuario.ToString())
                            {
                                model.SetValue(iter, 1, nombres);
                                model.SetValue(iter, 2, apellidos);
                                model.SetValue(iter, 3, correo);
                                model.SetValue(iter, 4, contrasenia);
                            }
                            return false;
                        });

                        OnShowMessage("Usuario actualizado exitosamente", MessageType.Info);
                    }
                    else
                    {
                        OnShowMessage("Error al actualizar usuario", MessageType.Error);
                    }
                }
                else
                {
                    OnShowMessage("Usuario no encontrado", MessageType.Error);
                }
            }
            else
            {
                OnShowMessage("Por favor, ingrese un ID válido", MessageType.Error);
            }
        }


        private void EliminarUsuario_Clicked(object sender, EventArgs e)
        {
            int idUsuario = -1;
            if (int.TryParse(txtBuscarUsuarioId.Text, out idUsuario))
            {
                if (userService.GetUserById(idUsuario) != null)
                {
                    try
                    {
                        // El servicio debe encargarse de liberar la memoria
                        bool success = userService.DeleteUser(idUsuario);

                        if (success)
                        {
                            // Actualizar TreeView
                            TreeIter iter;
                            if (lsUsuarios.GetIterFirst(out iter))
                            {
                                do
                                {
                                    string id = (string)lsUsuarios.GetValue(iter, 0);
                                    if (id == idUsuario.ToString())
                                    {
                                        lsUsuarios.Remove(ref iter);
                                        break;
                                    }
                                } while (lsUsuarios.IterNext(ref iter));
                            }

                            OnShowMessage("Usuario eliminado exitosamente", MessageType.Info);
                            LimpiarCamposUsuario();
                        }
                        else
                        {
                            OnShowMessage("Error al eliminar usuario", MessageType.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        OnShowMessage($"Error al eliminar usuario: {ex.Message}", MessageType.Error);
                    }
                }
                else
                {
                    OnShowMessage("Usuario no encontrado", MessageType.Error);
                }
            }
            else
            {
                OnShowMessage("Por favor, ingrese un ID válido", MessageType.Error);
            }
        }

        private void LimpiarCamposUsuario()
        {
            txtBuscarUsuarioId.Text = string.Empty;
            txtNombres.Text = string.Empty;
            txtApellidos.Text = string.Empty;
            txtCorreo.Text = string.Empty;
            txtContrasenia.Text = string.Empty;
        }

        protected virtual void OnShowMessage(string message, MessageType messageType)
        {
            ShowMessage?.Invoke(this, new MessageEventArgs(message, messageType));
        }

        protected virtual void OnUserDataChanged()
        {
            UserDataChanged?.Invoke(this, EventArgs.Empty);
        }

        // Method to refresh the view data
        public void RefreshData()
        {
            CargarDatosUsuarios();
        }

        ~UserManagementView()
        {
            Dispose(disposing: false);
        }
    }

}