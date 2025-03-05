using System;
using Gtk;
using AutoGestPro.src.Models;
using AutoGestPro.src.Services;
using System.IO;

/// <summary>
/// Represents the login window of the AutoGestPro application.
/// This window handles user authentication and provides a graphical interface for login.
/// </summary>
/// <remarks>
/// The window includes:
/// - Username and password input fields
/// - Login button
/// - Error message display
/// - Company logo
/// - Custom styled UI elements using CSS
/// </remarks>
namespace AutoGestPro.src.UI.Windows
{
    public class LoginWindow : Window
    {
        // UI Elements
        private Entry txtUsername;
        private Entry txtPassword;
        private Button btnLogin;
        private Label lblError;
        private AuthenticationService authService;

        /// <summary>
        /// Initializes the login window and its components.
        /// </summary>
        public LoginWindow() : base(WindowType.Toplevel)
        {
            // Initialize authentication service
            authService = new AuthenticationService();
            
            // Configure window settings
            SetupWindow();
            
            // Build the UI elements
            BuildInterface();
            
            // Show all UI components
            ShowAll();
            
            // Explicitly hide error message after ShowAll()
            lblError.Hide();
        }

        /// <summary>
        /// Configures the main properties of the login window.
        /// </summary>
        private void SetupWindow()
        {
            Title = "AutoGestPro - Acceso";
            DefaultWidth = 400;
            DefaultHeight = 500;
            WindowPosition = WindowPosition.Center;
            Resizable = false;
            
            // Close event to terminate the application
            DeleteEvent += (sender, e) => Application.Quit();
        }

        /// <summary>
        /// Builds the user interface components for the login form.
        /// </summary>
        private void BuildInterface()
        {
            // Main container with margin
            var mainBox = new Box(Orientation.Vertical, 0)
            {
                Margin = 30
            };

            // Centering container
            var centeringBox = new Box(Orientation.Vertical, 0)
            {
                Valign = Align.Center,
                Vexpand = true
            };
            mainBox.PackStart(centeringBox, true, true, 0);
            
            // Application logo
            var logo = new Image();
            try
            {
                logo.Pixbuf = new Gdk.Pixbuf("logo.png", 120, 120);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar el logo: {ex.Message}");
                logo = new Image(Stock.MissingImage, IconSize.Dialog); // Fallback image
            }
            logo.Halign = Align.Center;
            logo.MarginBottom = 20;
            centeringBox.PackStart(logo, false, false, 0);

            // Application title
            var logoTitle = new Label("AutoGestPro")
            {
                Halign = Align.Center,
                MarginBottom = 40
            };

            // Load CSS styles for UI elements
            var cssProvider = new CssProvider();
            cssProvider.LoadFromData(@"
                .logo-title { 
                    color: #8e44ad; 
                    font-size: 24px; 
                    font-weight: bold; 
                }
                
                button.login-btn { 
                    background-image: linear-gradient(to bottom, #9b59b6, #8e44ad, #6c3483);
                    color: white; 
                    border-radius: 20px; 
                    border: none;
                    transition: all 0.3s ease;
                }
                
                button.login-btn:hover { 
                    background-image: linear-gradient(to bottom, #6c3483, #5b2c6f, #4a235a);
                    box-shadow: 0 4px 12px rgba(142, 68, 173, 0.5);
                   
                }
                
                .error-message { 
                    color: #e74c3c; 
                    font-weight: bold;
                }
            ");
            
            logoTitle.StyleContext.AddProvider(cssProvider, StyleProviderPriority.Application);
            logoTitle.StyleContext.AddClass("logo-title");
            
            centeringBox.PackStart(logoTitle, false, false, 0);
            
            // Login form container
            var formBox = new Box(Orientation.Vertical, 10);

            // Username field
            var lblUsername = new Label("Usuario")
            {
                Halign = Align.Start
            };
            formBox.PackStart(lblUsername, false, false, 0);

            txtUsername = new Entry
            {
                MarginBottom = 15,
                HeightRequest = 40
            };
            formBox.PackStart(txtUsername, false, false, 0);

            // Password field
            var lblPassword = new Label("Contraseña")
            {
                Halign = Align.Start
            };
            formBox.PackStart(lblPassword, false, false, 0);

            txtPassword = new Entry
            {
                Visibility = false,
                MarginBottom = 25,
                HeightRequest = 40
            };
            formBox.PackStart(txtPassword, false, false, 0);
            
            // Error message label (hidden by default)
            lblError = new Label("Usuario o contraseña incorrectos");
            lblError.Visible = false;
            lblError.NoShowAll = true; // Prevents ShowAll() from displaying it
            lblError.Halign = Align.Center;
            lblError.MarginBottom = 15;
            
            // Apply error message style
            lblError.StyleContext.AddProvider(cssProvider, StyleProviderPriority.Application);
            lblError.StyleContext.AddClass("error-message");
            
            formBox.PackStart(lblError, false, false, 0);
            
            // Login button container
            var buttonBox = new Box(Orientation.Horizontal, 0);

            btnLogin = new Button("Ingresar")
            {
                HeightRequest = 40,
                WidthRequest = 200,
                Halign = Align.Center
            };
            btnLogin.Clicked += OnLoginClicked;
            
            // Apply button style
            btnLogin.StyleContext.AddProvider(cssProvider, StyleProviderPriority.Application);
            btnLogin.StyleContext.AddClass("login-btn");
            
            buttonBox.PackStart(btnLogin, true, true, 0);
            buttonBox.Halign = Align.Center;
            
            formBox.PackStart(buttonBox, false, false, 0);
            
            centeringBox.PackStart(formBox, false, false, 0);
            
            // Add main container to window
            Add(mainBox);
        }

        /// <summary>
        /// Handles the login button click event.
        /// </summary>
        private void OnLoginClicked(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            // Authenticate user
            bool isAuthenticated = AuthenticationService.Authenticate(username, password);

            if (isAuthenticated)
            {
                // Proceed to the main application window
                var mainWindow = new MainWindow();
                mainWindow.Show();
                Hide();
            }
            else
            {
                // Show error message and clear password field
                lblError.Show();
                txtPassword.Text = string.Empty;
                txtPassword.GrabFocus();
            }
        }
    }
}