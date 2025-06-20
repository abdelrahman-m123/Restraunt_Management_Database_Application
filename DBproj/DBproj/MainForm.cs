using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace FoodDeliverySystem
{
    public partial class MainForm : Form
    {
        
        private readonly string connectionString = @"Data Source=ABO7EDAR;Initial Catalog=master;Integrated Security=True";
        private DataTable currentDataTable;
        private string currentTable;

        public MainForm()
        {
            InitializeComponent();
            PopulateTableList();
        }

        private void PopulateTableList()
        {
            // Add tables to the list
            cboTables.Items.Add("Customer");
            cboTables.Items.Add("Restaurant");
            cboTables.Items.Add("Restaurant_Address");  // Added
            cboTables.Items.Add("Menu");
            cboTables.Items.Add("menu_item");
            cboTables.Items.Add("order");
            cboTables.Items.Add("delivery");
            cboTables.Items.Add("Deliveryman");
            cboTables.Items.Add("Review");
            cboTables.Items.Add("payment");  // Added
            cboTables.Items.Add("Consists_of");  // Added

            // Set default selection
            if (cboTables.Items.Count > 0)
                cboTables.SelectedIndex = 0;
        }

        private void cboTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTableData();
        }

        private void LoadTableData()
        {
            currentTable = cboTables.SelectedItem.ToString();
            string query = $"SELECT * FROM {(currentTable == "order" ? "[order]" : currentTable)}";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    currentDataTable = new DataTable();
                    adapter.Fill(currentDataTable);

                    // Display data in DataGridView
                    dataGridView.DataSource = currentDataTable;

                    
                    btnAdd.Enabled = true;
                    btnEdit.Enabled = true;
                    btnDelete.Enabled = true;

                    lblStatus.Text = $"Loaded {currentDataTable.Rows.Count} records from {currentTable}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatus.Text = "Error loading data";
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (AddEditForm addForm = new AddEditForm(currentTable, connectionString, null))
            {
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    LoadTableData(); 
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                DataRowView rowView = (DataRowView)dataGridView.SelectedRows[0].DataBoundItem;

                using (AddEditForm editForm = new AddEditForm(currentTable, connectionString, rowView.Row))
                {
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadTableData();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a row to edit", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                DataRowView rowView = (DataRowView)dataGridView.SelectedRows[0].DataBoundItem;
                string idColumnName = GetPrimaryKeyColumnName(currentTable);
                int id = Convert.ToInt32(rowView.Row[idColumnName]);

                if (MessageBox.Show($"Are you sure you want to delete this {currentTable}?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    DeleteRecord(currentTable, idColumnName, id);
                    LoadTableData(); // Refresh the data
                }
            }
            else
            {
                MessageBox.Show("Please select a row to delete", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string GetPrimaryKeyColumnName(string tableName)
        {
            // Return the appropriate primary key column name based on the table
            switch (tableName)
            {
                case "Customer": return "CustomerID";
                case "Restaurant": return "RestaurantID";
                case "Restaurant_Address": return "RestaurantID";  // Added - Part of composite key
                case "Menu": return "menuID";
                case "menu_item": return "ItemID";
                case "order": return "OrderID";
                case "delivery": return "DeliveryID";
                case "Deliveryman": return "DeliverManID";
                case "Review": return "ReviewID";
                case "payment": return "PaymentID";  // Added
                case "Consists_of": return "OrderID";  // Added - Part of composite key
                default: return "ID";
            }
        }

        private void DeleteRecord(string tableName, string idColumnName, int id)
        {
            // Adjust table name for the order table
            string adjustedTableName = tableName == "order" ? "[order]" : tableName;

            string query = $"DELETE FROM {adjustedTableName} WHERE {idColumnName} = @ID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ID", id);
                    int rowsAffected = command.ExecuteNonQuery();

                    lblStatus.Text = $"Deleted {rowsAffected} record(s) from {tableName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting record: {ex.Message}", "Database Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatus.Text = "Error deleting record";
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadTableData();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadTableData();
                return;
            }

            // Create a filtered view of the data
            DataView dv = currentDataTable.DefaultView;
            string filter = BuildSearchFilter(currentDataTable, searchText);
            dv.RowFilter = filter;
            dataGridView.DataSource = dv;

            lblStatus.Text = $"Found {dv.Count} matching records";
        }

        private string BuildSearchFilter(DataTable table, string searchText)
        {
            // Build a filter expression that searches across all string columns
            string filter = "";
            foreach (DataColumn column in table.Columns)
            {
                if (column.DataType == typeof(string))
                {
                    if (filter.Length > 0)
                        filter += " OR ";

                    filter += $"{column.ColumnName} LIKE '%{searchText}%'";
                }
            }
            return filter;
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            using (ReportForm reportForm = new ReportForm(connectionString))
            {
                reportForm.ShowDialog();
            }
        }
    }
}