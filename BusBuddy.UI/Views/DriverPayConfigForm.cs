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
        private List<PayRate> _payRates;
        private SfDataGrid _payRatesGrid;        private SfButton _updateButton;
        private SfButton _closeButton;
        private TableLayoutPanel _mainLayout;
        private Panel _payButtonPanel;
        private Label _titleLabel;

        public DriverPayConfigForm() : this(new ErrorHandlerService())
        {
        }

        public DriverPayConfigForm(IErrorHandlerService errorHandler)
        {
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
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
                var jsonPath = Path.Combine(Application.StartupPath, "Resources", "payrates.json");
                
                if (File.Exists(jsonPath))
                {
                    var json = File.ReadAllText(jsonPath);
                    var rates = JsonSerializer.Deserialize<Dictionary<string, decimal>>(json);
                    
                    _payRates = new List<PayRate>
                    {
                        new PayRate 
                        { 
                            RouteType = "CDL", 
                            Rate = rates.ContainsKey("CDLTripRate") ? rates["CDLTripRate"] : 33.00m,
                            Description = "Per trip (AM or PM)"
                        },
                        new PayRate 
                        { 
                            RouteType = "SmallBus", 
                            Rate = rates.ContainsKey("SmallBusTripRate") ? rates["SmallBusTripRate"] : 15.00m,
                            Description = "Per trip (AM or PM)"
                        },
                        new PayRate 
                        { 
                            RouteType = "SPED", 
                            Rate = rates.ContainsKey("SPEDDayRate") ? rates["SPEDDayRate"] : 66.00m,
                            Description = "Per day (both AM/PM)"
                        }
                    };
                }
                else
                {
                    // Default rates if file doesn't exist
                    _payRates = new List<PayRate>
                    {
                        new PayRate { RouteType = "CDL", Rate = 33.00m, Description = "Per trip (AM or PM)" },
                        new PayRate { RouteType = "SmallBus", Rate = 15.00m, Description = "Per trip (AM or PM)" },
                        new PayRate { RouteType = "SPED", Rate = 66.00m, Description = "Per day (both AM/PM)" }
                    };
                }

                _payRatesGrid.DataSource = _payRates;
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Failed to load pay rates: {ex.Message}", "Pay Rate Error");
                
                // Fallback to default rates
                _payRates = new List<PayRate>
                {
                    new PayRate { RouteType = "CDL", Rate = 33.00m, Description = "Per trip (AM or PM)" },
                    new PayRate { RouteType = "SmallBus", Rate = 15.00m, Description = "Per trip (AM or PM)" },
                    new PayRate { RouteType = "SPED", Rate = 66.00m, Description = "Per day (both AM/PM)" }
                };
                _payRatesGrid.DataSource = _payRates;
            }
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate rates
                foreach (var rate in _payRates)
                {
                    if (rate.Rate <= 0)
                    {
                        _errorHandler.HandleError($"Rate for {rate.RouteType} must be greater than zero.", "Invalid Rate");
                        return;
                    }
                }

                // Create rates dictionary
                var rates = new Dictionary<string, decimal>
                {
                    { "CDLTripRate", _payRates.First(r => r.RouteType == "CDL").Rate },
                    { "SmallBusTripRate", _payRates.First(r => r.RouteType == "SmallBus").Rate },
                    { "SPEDDayRate", _payRates.First(r => r.RouteType == "SPED").Rate }
                };

                // Serialize to JSON with formatting
                var json = JsonSerializer.Serialize(rates, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });

                // Ensure directory exists
                var resourcesPath = Path.Combine(Application.StartupPath, "Resources");
                if (!Directory.Exists(resourcesPath))
                {
                    Directory.CreateDirectory(resourcesPath);
                }

                // Write to file
                var jsonPath = Path.Combine(resourcesPath, "payrates.json");
                File.WriteAllText(jsonPath, json);

                MessageBox.Show(
                    "Pay rates updated successfully!\n\n" +
                    $"CDL Trip Rate: ${rates["CDLTripRate"]:F2}\n" +
                    $"Small Bus Trip Rate: ${rates["SmallBusTripRate"]:F2}\n" +
                    $"SPED Day Rate: ${rates["SPEDDayRate"]:F2}",
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
    /// Data model for pay rate configuration
    /// </summary>
    public class PayRate
    {
        public string RouteType { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
