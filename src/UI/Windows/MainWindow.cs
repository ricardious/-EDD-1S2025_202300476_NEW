using System;
using System.IO;
using Gtk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using AutoGestPro.src.Models;
using AutoGestPro.src.DataStructures;
using AutoGestPro.src.UI.Components;
using AutoGestPro.src.Utils;
using AutoGestPro.src.UI.Views;
using AutoGestPro.src.Services;
using AutoGestPro.src.Services.Interfaces;
using AutoGestPro.src.UI.Common;



namespace AutoGestPro.src.UI.Windows
{
    public unsafe class MainWindow : Window
    {
        // Data structures
        private readonly UserList userList;
        private readonly VehicleList vehicleList;
        private readonly SparePartsList sparePartsList;
        private readonly ServiceQueue serviceQueue;
        private readonly InvoiceStack invoiceStack;
        private readonly SparseMatrix logMatrix;

        // Services
        private readonly UserService userService;
        private readonly VehicleService vehicleService;
        private readonly SparePartsService sparePartsService;
        private readonly ServiceQueueService serviceQueueService;
        private readonly InvoiceService invoiceService;

        // UI components
        private readonly HeaderBar headerBar;
        private readonly Stack mainStack;
        private readonly Sidebar sidebar;

        // Views
        private DashboardView dashboardView;
        private BulkLoadView bulkLoadView;
        private UserManagementView userManagementView;
        private VehicleManagementView vehicleManagementView;
        private SparePartsManagementView sparePartsManagementView;
        private ServiceManagementView serviceManagementView;
        private ReportsView reportsView;
        private InvoiceManagementView invoiceManagementView;


        public MainWindow() : base("AutoGest Pro - Sistema de Gestión para Talleres")
        {
            // Initialize data structures
            userList = new UserList();
            vehicleList = new VehicleList();
            sparePartsList = new SparePartsList();
            serviceQueue = new ServiceQueue();
            invoiceStack = new InvoiceStack();
            logMatrix = new SparseMatrix();

            // Initialize services
            userService = new UserService(userList);
            vehicleService = new VehicleService(vehicleList);
            sparePartsService = new SparePartsService(sparePartsList);
            serviceQueueService = new ServiceQueueService(serviceQueue);
            invoiceService = new InvoiceService(invoiceStack);

            // Configure main Window
            SetDefaultSize(800, 600);
            WindowPosition = WindowPosition.Center;
            Resizable = true;

            // Create modern header bar
            headerBar = new HeaderBar
            {
                ShowCloseButton = true,
                Title = "AutoGest Pro",
                Subtitle = "Sistema de Gestión para Talleres"
            };
            Titlebar = headerBar;

            // Set up main layout
            Box mainBox = new(Orientation.Horizontal, 0);
            Add(mainBox);

            // Create sidebar
            sidebar = [];
            mainBox.PackStart(sidebar, false, false, 0);

            // Main panel with stack for navigation
            mainStack = new Stack
            {
                TransitionType = StackTransitionType.SlideLeftRight
            };

            // Initialize and add views to the stack
            InitializeViews();

            // Add stack to main container
            Box contentBox = new(Orientation.Vertical, 0);
            contentBox.PackStart(mainStack, true, true, 0);

            mainBox.PackStart(contentBox, true, true, 0);

            // Set up navigation events
            sidebar.SidebarItemSelected += (sender, args) =>
            {
                mainStack.VisibleChildName = args.ItemName;
            };

            // Show elements
            ShowAll();

            // Connect close signals
            DeleteEvent += (o, args) =>
            {
                Application.Quit();
                args.RetVal = true;
            };
        }

        private void InitializeViews()
        {
            // Create and add views to the stack
            dashboardView = new DashboardView(mainStack);

            mainStack.AddTitled(dashboardView, "dashboard", "Dashboard");

            bulkLoadView = new BulkLoadView(userService, vehicleService, sparePartsService);
            bulkLoadView.ShowMessage += DisplayMessage;
            mainStack.AddTitled(bulkLoadView, "bulkload", "Bulk Data Load");

            userManagementView = new UserManagementView(userService);
            userManagementView.ShowMessage += DisplayMessage;
            userService.DataChanged += (sender, e) => userManagementView.RefreshData();
            mainStack.AddTitled(userManagementView, "usermanagement", "User Management");

            vehicleManagementView = new VehicleManagementView(vehicleService, userService);
            vehicleManagementView.ShowMessage += DisplayMessage;
            vehicleService.DataChanged += (sender, e) =>
            {
                vehicleManagementView.RefreshData();
                serviceManagementView?.RefreshVehicleData();
            };
            mainStack.AddTitled(vehicleManagementView, "vehiclemanagement", "Vehicle Management");

            sparePartsManagementView = new SparePartsManagementView(sparePartsService);
            sparePartsManagementView.ShowMessage += DisplayMessage;
            sparePartsService.DataChanged += (sender, e) =>
            {
                sparePartsManagementView.RefreshData();
                serviceManagementView?.RefreshSparePartData();
            };
            mainStack.AddTitled(sparePartsManagementView, "sparepartsmanagement", "Spare Parts Management");

            serviceManagementView = new ServiceManagementView(serviceQueueService, vehicleService, sparePartsService, invoiceService, logMatrix);
            serviceManagementView.ShowMessage += DisplayMessage;
            serviceQueueService.DataChanged += (sender, e) => serviceManagementView.RefreshData();
            mainStack.AddTitled(serviceManagementView, "servicemanagement", "Service Management");

            invoiceManagementView = new InvoiceManagementView(invoiceService, serviceQueueService, vehicleService);
            invoiceManagementView.ShowMessage += DisplayMessage;
            invoiceService.DataChanged += (sender, e) =>
            {
                Console.WriteLine("MainWindow: Invoice data changed, refreshing view");
                Application.Invoke(delegate
                {
                    invoiceManagementView.RefreshData();
                });
            };
            mainStack.AddTitled(invoiceManagementView, "invoicemanagement", "Invoice Management");
            serviceManagementView.InvoiceCreated += (sender, e) =>
                {
                    Console.WriteLine("MainWindow: Invoice created event received");
                    Application.Invoke(delegate
                    {
                        invoiceManagementView.RefreshData();
                    });
                };

            reportsView = new ReportsView(userList, vehicleList, sparePartsList, serviceQueue, invoiceStack, logMatrix);
            reportsView.ShowMessage += DisplayMessage;
            mainStack.AddTitled(reportsView, "reports", "Reports and Charts");
        }

    private void DisplayMessage(object sender, MessageEventArgs args)
        {
            Application.Invoke(delegate {
                try {
                    // Create and show dialog - use args.MessageType directly since it's already Gtk.MessageType
                    using (MessageDialog dialog = new MessageDialog(
                        this,
                        DialogFlags.Modal,
                        args.MessageType,
                        ButtonsType.Ok,
                        args.Message))
                    {
                        dialog.Run();
                        dialog.Destroy();
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error displaying message: {ex.Message}");
                }
            });
        }

    }

}