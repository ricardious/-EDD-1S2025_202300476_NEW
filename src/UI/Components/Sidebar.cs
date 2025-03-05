using System;
using Gtk;

namespace AutoGestPro.src.UI.Components
{
    /// <summary>
    /// Sidebar navigation component for the main application
    /// </summary>
    public class Sidebar : Box
    {
        // Event for navigation
        public event EventHandler<SidebarItemSelectedEventArgs> SidebarItemSelected;
        
        // UI elements
        private readonly ListBox listBox;
        
        public Sidebar() : base(Orientation.Vertical, 0)
        {
            // Set styling
            StyleContext.AddClass("sidebar");
            
            // Create header
            Label header = new Label {
                Markup = "<span font='14'>AutoGestPro</span>",
                Halign = Align.Start,
                MarginTop = 15,
                MarginStart = 15,
                MarginBottom = 15
            };
            PackStart(header, false, false, 0);
            
            // Create list for menu items
            listBox = new ListBox {
                SelectionMode = SelectionMode.Single
            };
            listBox.StyleContext.AddClass("sidebar-list");
            
            // Add navigation items
            AddItem("dashboard", "Dashboard", "view-grid-symbolic");
            AddItem("bulkload", "Carga Masiva", "document-open-symbolic");
            AddItem("usermanagement", "Usuarios", "contact-new-symbolic");
            AddItem("vehiclemanagement", "Vehículos", "drive-harddisk-symbolic");
            AddItem("sparepartsmanagement", "Repuestos", "applications-engineering-symbolic");
            AddItem("servicemanagement", "Servicios", "emblem-system-symbolic");
            AddItem("invoicemanagement", "Facturas", "emblem-documents-symbolic");
            AddItem("reports", "Reportes", "accessories-calculator-symbolic");
            
            // Handle selection
            listBox.RowSelected += (sender, e) => {
                if (e.Row != null)
                {
                    string itemName = (string)e.Row.Data["item_name"];
                    SidebarItemSelected?.Invoke(this, new SidebarItemSelectedEventArgs(itemName));
                }
            };
            
            PackStart(listBox, true, true, 0);
            
            // Setup CSS
            var cssProvider = new CssProvider();
            cssProvider.LoadFromData(@"
                .sidebar {
                    background-color: #2c3e50;
                    color: #ecf0f1;
                    border-right: 1px solid #34495e;
                }
                .sidebar-list {
                    background-color: transparent;
                }
                .sidebar-list row {
                    padding: 12px 16px;
                    border-bottom: 1px solid rgba(255,255,255,0.1);
                }
                .sidebar-list row:selected {
                    background-color: #3498db;
                }
                .sidebar-list label {
                    color: #ecf0f1;
                }
            ");
            
            StyleContext.AddProvider(cssProvider, StyleProviderPriority.Application);
            
            // Set initial width
            WidthRequest = 200;
        }
        
        private void AddItem(string name, string label, string iconName)
        {
            Box itemBox = new(Orientation.Horizontal, 10);
            
            Image icon = new()
            {
                IconName = iconName,
                IconSize = (int)IconSize.Menu
            };
            itemBox.PackStart(icon, false, false, 0);
            
            Label itemLabel = new(label) {
                Halign = Align.Start
            };
            itemBox.PackStart(itemLabel, true, true, 0);
            
            ListBoxRow row = [itemBox];
            row.Data["item_name"] = name;
            
            listBox.Add(row);
        }
    }
    
    public class SidebarItemSelectedEventArgs(string itemName) : EventArgs
    {
        public string ItemName { get; private set; } = itemName;
    }
}