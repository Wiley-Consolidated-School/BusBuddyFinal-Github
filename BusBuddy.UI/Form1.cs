using BusBuddy.Models;
using BusBuddy.Business;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BusBuddy
{
    public partial class Form1 : Form
    {
        private readonly IVehicleService? _vehicleService;
        private readonly Dashboard? _dashboard;

        // Parameterless constructor for design-time and simple startup
        public Form1() : this(null, null)
        {
        }

        public Form1(IVehicleService? vehicleService, Dashboard? dashboard)
        {
            _vehicleService = vehicleService;
            _dashboard = dashboard;
            InitializeComponent();
            SetupDataGridView();
            if (_vehicleService != null)
            {
                LoadVehicleDataAsync();
            }
        }

        private void SetupDataGridView()
        {
            vehicleGridView.AutoGenerateColumns = false;
            vehicleGridView.ReadOnly = true;
            vehicleGridView.Columns.Clear();

            vehicleGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VehicleID",
                HeaderText = "ID",
                Width = 50
            });
            vehicleGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "BusNumber",
                HeaderText = "Bus Number",
                Width = 100
            });
            vehicleGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Year",
                HeaderText = "Year",
                Width = 70
            });
            vehicleGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Make",
                HeaderText = "Make",
                Width = 100
            });
            vehicleGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Model",
                HeaderText = "Model",
                Width = 100
            });
            vehicleGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SeatingCapacity",
                HeaderText = "Capacity",
                Width = 70
            });
            vehicleGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Inspection Due",
                Width = 120
            });
            vehicleGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Total Mileage",
                Width = 100
            });
            vehicleGridView.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colEdit",
                Text = "Edit",
                UseColumnTextForButtonValue = true,
                Width = 60
            });
            vehicleGridView.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colDelete",
                Text = "Delete",
                UseColumnTextForButtonValue = true,
                Width = 60
            });

            vehicleGridView.CellClick += vehicleGridView_CellClick;
        }

        private async void LoadVehicleDataAsync()
        {
            if (_vehicleService == null) return;
            
            try
            {
                var vehicles = await _vehicleService.GetAllVehiclesAsync();
                vehicleGridView.Rows.Clear();

                foreach (var vehicle in vehicles)
                {
                    var isDue = await _vehicleService.IsVehicleDueForInspectionAsync(vehicle.VehicleID);
                    var mileage = await _vehicleService.GetTotalMileageForVehicleAsync(vehicle.VehicleID);
                    vehicleGridView.Rows.Add(
                        vehicle.VehicleID,
                        vehicle.BusNumber,
                        vehicle.Year,
                        vehicle.Make,
                        vehicle.Model,
                        vehicle.SeatingCapacity,
                        isDue ? "Yes" : "No",
                        mileage,
                        "Edit",
                        "Delete"
                    );
                }

                lblTotalVehicles.Text = $"Total Vehicles: {vehicles.Count()}";
                int dueForInspection = vehicleGridView.Rows.Cast<DataGridViewRow>()
                    .Count(row => row.Cells[6].Value.ToString() == "Yes");
                lblVehiclesDueInspection.Text = $"Vehicles Due for Inspection: {dueForInspection}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vehicles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadVehicleDataAsync();
        }

        private async void btnAddVehicle_Click(object sender, EventArgs e)
        {
            if (_vehicleService == null) 
            {
                MessageBox.Show("Vehicle service not available", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var vehicleForm = new VehicleForm();
            if (vehicleForm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var vehicle = vehicleForm.Vehicle;
                    await _vehicleService.AddVehicleAsync(vehicle);
                    LoadVehicleDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving vehicle: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void vehicleGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _vehicleService == null) return;

            var vehicleId = Convert.ToInt32(vehicleGridView.Rows[e.RowIndex].Cells[0].Value);

            try
            {
                if (e.ColumnIndex == vehicleGridView.Columns["colEdit"].Index)
                {
                    var vehicles = await _vehicleService.GetAllVehiclesAsync();
                    var vehicle = vehicles.FirstOrDefault(v => v.VehicleID == vehicleId);
                    if (vehicle == null)
                    {
                        MessageBox.Show("Vehicle not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var vehicleForm = new VehicleForm(vehicle);
                    if (vehicleForm.ShowDialog() == DialogResult.OK)
                    {
                        await _vehicleService.UpdateVehicleAsync(vehicleForm.Vehicle);
                        LoadVehicleDataAsync();
                    }
                }
                else if (e.ColumnIndex == vehicleGridView.Columns["colDelete"].Index)
                {
                    var result = MessageBox.Show("Are you sure you want to delete this vehicle?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        await _vehicleService.DeleteVehicleAsync(vehicleId);
                        LoadVehicleDataAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Missing event handlers referenced in Designer
        private void btnThisMonth_Click(object sender, EventArgs e)
        {
            // TODO: Implement this month filter
        }

        private void btnLast30Days_Click(object sender, EventArgs e)
        {
            // TODO: Implement last 30 days filter
        }

        private void btnLast7Days_Click(object sender, EventArgs e)
        {
            // TODO: Implement last 7 days filter
        }

        private void btnCustomDate_Click(object sender, EventArgs e)
        {
            // TODO: Implement custom date filter
        }

        private void btnOkCustomDate_Click(object sender, EventArgs e)
        {
            // TODO: Implement custom date OK handler
        }
    }
}
