// Task 6.6: Integrate Management Views - DriverPayConfigForm for pay rate configuration
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using BusBuddy.UI.Base;
using BusBuddy.UI.Services;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Enums;
using Syncfusion.WinForms.Input;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Driver Pay Configuration Form for managing pay rates
    /// Allows updating of route pay scheme using payrates.json
    /// </summary>
    public partial class DriverPayConfigForm : SyncfusionBaseForm
    {
        private readonly IErrorHandlerService _errorHandler;
        private readonly PayRateManager _payRateManager;
        private List<PayRateDisplay> _payRates;
        private SfDataGrid _payRatesGrid;
        private SfButton _updateButton;
        private SfButton _closeButton;
        private TableLayoutPanel _mainLayout;
        private Panel _payButtonPanel;
        private Label _titleLabel;

        public DriverPayConfigForm() : this(
            ServiceContainerSingleton.Instance.GetService<IErrorHandlerService>(),
            ServiceContainerSingleton.Instance.GetService<PayRateManager>())
        {
        }

        public DriverPayConfigForm(IErrorHandlerService errorHandler, PayRateManager payRateManager)
        {
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _payRateManager = payRateManager ?? throw new ArgumentNullException(nameof(payRateManager));
            InitializeComponent();
            InitializeControls();
            LoadPayRates();
        }

        private void InitializeComponent()
        {
            this.Text = "Pay Rate Configuration";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(500, 350);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.ShowInTaskbar = false;
        }

        private void InitializeControls()
        {
            // Title label
            _titleLabel = new Label
            {
                Text = "💰 Driver Pay Rate Configuration",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(63, 81, 181),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };            // Data grid for pay rates
            _payRatesGrid = new SfDataGrid
            {
                AllowEditing = true,
                AutoSizeColumnsMode = AutoSizeColumnsMode.Fill,
                SelectionMode = GridSelectionMode.Single,
                NavigationMode = NavigationMode.Row,
                EditMode = EditMode.SingleClick,
                ShowRowHeader = false,
                AllowGrouping = false,
                AllowSorting = false,
                AllowFiltering = false,
                Dock = DockStyle.Fill
            };

            // Configure columns
            var routeTypeColumn = new Syncfusion.WinForms.DataGrid.GridTextColumn
            {
                MappingName = "RouteType",
                HeaderText = "Route Type",
                Width = 200,
                AllowEditing = false
            };

            var rateColumn = new Syncfusion.WinForms.DataGrid.GridNumericColumn
            {
                MappingName = "Rate",
                HeaderText = "Rate ($)",
                Width = 150,
                AllowEditing = true,
                NumberFormatInfo = new System.Globalization.NumberFormatInfo
                {
                    NumberDecimalDigits = 2,
                    CurrencySymbol = "$"
                },
                Format = "C"
            };

            var descriptionColumn = new Syncfusion.WinForms.DataGrid.GridTextColumn
            {
                MappingName = "Description",
                HeaderText = "Description",
                AllowEditing = false
            };

            _payRatesGrid.Columns.Add(routeTypeColumn);
            _payRatesGrid.Columns.Add(rateColumn);
            _payRatesGrid.Columns.Add(descriptionColumn);

            // Update button
            _updateButton = new SfButton
            {
                Text = "Update Route Pay Scheme",
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(63, 81, 181),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            _updateButton.Click += UpdateButton_Click;

            // Close button
            _closeButton = new SfButton
            {
                Text = "Close",
                Size = new Size(80, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            _closeButton.Click += (s, e) => this.Close();            // Button panel
            _payButtonPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom,
                Padding = new Padding(10)
            };

            var buttonLayout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                WrapContents = false
            };

            buttonLayout.Controls.Add(_closeButton);
            buttonLayout.Controls.Add(_updateButton);
            _payButtonPanel.Controls.Add(buttonLayout);

            // Main layout
            _mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));            _mainLayout.Controls.Add(_payRatesGrid, 0, 0);
            _mainLayout.Controls.Add(_payButtonPanel, 0, 1);

            // Add controls to form
            this.Controls.Add(_mainLayout);
            this.Controls.Add(_titleLabel);
        }

        private void LoadPayRates()
        {
            try
            {
                // Task 6.7: Use PayRateManager for loading pay rates
                var payRates = _payRateManager.LoadPayRates();
                
                _payRates = payRates.Select(rate => new PayRateDisplay
                {
                    RouteType = rate.RouteType,
                    Rate = rate.Rate,
                    Description = GetRateDescription(rate.RouteType)
                }).ToList();

                _payRatesGrid.DataSource = _payRates;
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Failed to load pay rates: {ex.Message}", "Pay Rate Error");

                // Fallback to default rates
                _payRates = new List<PayRateDisplay>
                {
                    new PayRateDisplay { RouteType = "CDL", Rate = 33.00m, Description = "Per trip (AM or PM)" },
                    new PayRateDisplay { RouteType = "SmallBus", Rate = 15.00m, Description = "Per trip (AM or PM)" },
                    new PayRateDisplay { RouteType = "SPED", Rate = 66.00m, Description = "Per day (both AM/PM)" }
                };
                _payRatesGrid.DataSource = _payRates;
            }
        }

        private string GetRateDescription(string routeType)
        {
            return routeType switch
            {
                "CDL" => "Per trip (AM or PM)",
                "SmallBus" => "Per trip (AM or PM)",
                "SPED" => "Per day (both AM/PM)",
                _ => "Per trip"
            };
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Task 6.7: Use PayRateManager for saving pay rates
                // Convert display objects to PayRate objects for saving
                var payRatesToSave = _payRates.Select(displayRate => new PayRate
                {
                    RouteType = displayRate.RouteType,
                    Rate = displayRate.Rate
                }).ToList();

                // Use PayRateManager to save rates
                _payRateManager.SavePayRates(payRatesToSave);

                MessageBox.Show(
                    "Pay rates updated successfully!\n\n" +
                    $"CDL Trip Rate: ${_payRates.First(r => r.RouteType == "CDL").Rate:F2}\n" +
                    $"Small Bus Trip Rate: ${_payRates.First(r => r.RouteType == "SmallBus").Rate:F2}\n" +
                    $"SPED Day Rate: ${_payRates.First(r => r.RouteType == "SPED").Rate:F2}",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Failed to update pay rates: {ex.Message}", "Update Error");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _payRatesGrid?.Dispose();                _updateButton?.Dispose();
                _closeButton?.Dispose();
                _mainLayout?.Dispose();
                _payButtonPanel?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Data model for pay rate display in the grid
    /// </summary>
    public class PayRateDisplay
    {
        public string RouteType { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
