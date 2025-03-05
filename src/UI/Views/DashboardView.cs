using System;
using Gtk;
using AutoGestPro.src.Utils;

namespace AutoGestPro.src.UI.Views
{
    public class DashboardView : Box
    {
        private readonly Stack mainStack;

        public DashboardView(Stack mainStack) : base(Orientation.Vertical, 10)
        {
            Margin = 20;
            this.mainStack = mainStack;
            BuildInterface();
        }

        private void BuildInterface()
        {
            // Header
            Label lblWelcome = new()
            {
                Markup = "<span font='20'>Bienvenido a AutoGest Pro</span>"
            };
            PackStart(lblWelcome, false, false, 10);

            Label lblDescription = new("Sistema integral de gestión para talleres de reparación de vehículos");
            PackStart(lblDescription, false, false, 5);

            // Statistics cards
            Box cardsBox = new(Orientation.Horizontal, 20)
            {
                Homogeneous = false
            };
            PackStart(cardsBox, false, false, 20);

            // Users card
            Frame cardUsers = CreateStatCard("Usuarios", "0", "user-info-symbolic");
            cardsBox.PackStart(cardUsers, true, true, 0);

            // Vehicles card
            Frame cardVehicles = CreateStatCard("Vehículos", "0", "transportation-symbolic");
            cardsBox.PackStart(cardVehicles, true, true, 0);

            // Spare parts card
            Frame cardSpareParts = CreateStatCard("Repuestos", "0", "emblem-system-symbolic");
            cardsBox.PackStart(cardSpareParts, true, true, 0);

            // Services card
            Frame cardServices = CreateStatCard("Servicios", "0", "preferences-system-symbolic");
            cardsBox.PackStart(cardServices, true, true, 0);

            // Quick actions
            Label lblActions = new()
            {
                Markup = "<span font='16'>Acciones rápidas</span>",
                Halign = Align.Start
            };
            PackStart(lblActions, false, false, 10);

            Box actionsBox = new(Orientation.Horizontal, 10);
            PackStart(actionsBox, false, false, 10);

            AddActionButtons(actionsBox);
        }

        private void AddActionButtons(Box actionsBox)
        {
            // New user button
            Button btnNewUser = CreateActionButton("Nuevo Usuario", "list-add-symbolic");
            btnNewUser.Clicked += (sender, e) => mainStack.VisibleChildName = "usuarios";
            actionsBox.PackStart(btnNewUser, false, false, 0);

            // New vehicle button
            Button btnNewVehicle = CreateActionButton("Nuevo Vehículo", "list-add-symbolic");
            btnNewVehicle.Clicked += (sender, e) => mainStack.VisibleChildName = "vehiculos";
            actionsBox.PackStart(btnNewVehicle, false, false, 0);

            // New service button
            Button btnNewService = CreateActionButton("Nuevo Servicio", "list-add-symbolic");
            btnNewService.Clicked += (sender, e) => mainStack.VisibleChildName = "servicios";
            actionsBox.PackStart(btnNewService, false, false, 0);

            // View reports button
            Button btnViewReports = CreateActionButton("Ver Reportes", "document-properties-symbolic");
            btnViewReports.Clicked += (sender, e) => mainStack.VisibleChildName = "reportes";
            actionsBox.PackStart(btnViewReports, false, false, 0);
        }

        private static Frame CreateStatCard(string title, string value, string icon)
        {
            Frame card = new()
            {
                WidthRequest = 150
            };

            Box box = new(Orientation.Vertical, 10)
            {
                Margin = 15
            };
            card.Add(box);

            Image img = new(icon);
            box.PackStart(img, false, false, 0);

            Label lblTitle = new(title)
            {
                Halign = Align.Start
            };
            box.PackStart(lblTitle, false, false, 0);

            Label lblValue = new($"<span font='24'>{value}</span>")
            {
                UseMarkup = true
            };
            box.PackStart(lblValue, false, false, 0);

            return card;
        }

        private static Button CreateActionButton(string label, string icon)
        {
            Button btn = new()
            {
                WidthRequest = 150,
                HeightRequest = 50
            };

            Box box = new(Orientation.Horizontal, 5);
            btn.Add(box);

            Image img = new(icon)
            {
                WidthRequest = 24
            };
            box.PackStart(img, false, false, 0);

            Label lbl = new(label);
            box.PackStart(lbl, true, true, 0);

            return btn;
        }
    }
}