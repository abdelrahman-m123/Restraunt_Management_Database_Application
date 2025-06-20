using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace FoodDeliverySystem
{
    public partial class AddEditForm : Form
    {
        private readonly string tableName;
        private readonly string connectionString;
        private readonly DataRow editRow;
        private readonly string idColumnName;
        private bool isEditMode;
        private FlowLayoutPanel flowLayoutPanel;

        public AddEditForm(string tableName, string connectionString, DataRow editRow)
        {
            this.tableName = tableName;
            this.connectionString = connectionString;
            this.editRow = editRow;
            this.isEditMode = (editRow != null);
            this.idColumnName = GetPrimaryKeyColumnName(tableName);

            // Set form title based on mode
            this.Text = (isEditMode ? "Edit " : "Add ") + tableName;

            // Create form controls dynamically based on the table structure
            CreateFormControls();

            // Populate data if in edit mode
            if (isEditMode)
            {
                PopulateFormData();
            }
        }

        private void PopulateFormData()
        {
            foreach (Control control in flowLayoutPanel.Controls)
            {
                if (control.Tag == null)
                    continue;

                string columnName = control.Tag.ToString();

                if (!editRow.Table.Columns.Contains(columnName))
                    continue;

                object value = editRow[columnName];

                if (control is TextBox textBox)
                {
                    textBox.Text = value?.ToString();
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.SelectedValue = value;
                }
                else if (control is CheckBox checkBox)
                {
                    checkBox.Checked = (value != DBNull.Value) && Convert.ToBoolean(value);
                }
                else if (control is DateTimePicker datePicker)
                {
                    if (value != DBNull.Value)
                        datePicker.Value = Convert.ToDateTime(value);
                }
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

        private void CreateFormControls()
        {
            DataTable schemaTable = GetTableSchema();

            flowLayoutPanel = new FlowLayoutPanel();
            flowLayoutPanel.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel.WrapContents = false;
            flowLayoutPanel.AutoScroll = true;
            flowLayoutPanel.Dock = DockStyle.Fill;
            flowLayoutPanel.Padding = new Padding(10);

            foreach (DataRow row in schemaTable.Rows)
            {
                string columnName = row["COLUMN_NAME"].ToString();

                // Skip ID column only if it is a single-column identity key
                if (!isEditMode && columnName == idColumnName &&
                    !(tableName.ToLower() == "consists_of" || tableName.ToLower() == "restaurant_address"))
                {
                    continue;
                }


                Label label = new Label();
                label.Text = columnName + ":";
                label.Width = 300;
                label.Margin = new Padding(0, 10, 0, 0);

                Control control = CreateControlForColumn(row);
                control.Tag = columnName;
                control.Width = 300;

                flowLayoutPanel.Controls.Add(label);
                flowLayoutPanel.Controls.Add(control);
            }

            Panel buttonPanel = new Panel();
            buttonPanel.Height = 40;
            buttonPanel.Width = 300;

            Button btnOK = new Button();
            btnOK.Text = "OK";
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Width = 80;
            btnOK.Location = new Point(120, 10);
            btnOK.Click += BtnOK_Click;

            Button btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Width = 80;
            btnCancel.Location = new Point(210, 10);

            buttonPanel.Controls.Add(btnOK);
            buttonPanel.Controls.Add(btnCancel);

            flowLayoutPanel.Controls.Add(buttonPanel);

            this.Controls.Add(flowLayoutPanel);

            this.Size = new Size(350, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;

                    string quotedTableName = GetQuotedTableName(tableName);
                    string sql;

                    if (isEditMode)
                    {
                        sql = $"UPDATE {quotedTableName} SET ";
                        var setClauses = new System.Text.StringBuilder();

                        foreach (Control control in flowLayoutPanel.Controls)
                        {
                            if (control.Tag == null || control.Tag.ToString() == idColumnName) continue;

                            string column = control.Tag.ToString();
                            object value = GetControlValue(control);

                            // Skip NULL values for Status column to allow default constraint to work
                            if (column.ToLower() == "status" && (value == null || value == DBNull.Value))
                                continue;

                            cmd.Parameters.AddWithValue("@" + column, value ?? DBNull.Value);
                            setClauses.Append($"[{column}] = @{column}, ");
                        }

                        if (setClauses.Length > 0) setClauses.Length -= 2; // remove last comma
                        sql += setClauses.ToString();
                        sql += $" WHERE [{idColumnName}] = @{idColumnName}";

                        cmd.Parameters.AddWithValue("@" + idColumnName, editRow[idColumnName]);
                    }
                    else
                    {
                        var columns = new System.Text.StringBuilder();
                        var values = new System.Text.StringBuilder();

                        foreach (Control control in flowLayoutPanel.Controls)
                        {
                            if (control.Tag == null) continue;
                            string column = control.Tag.ToString();
                            object value = GetControlValue(control);

                            // For status column, ensure we have a value
                            if (column.ToLower() == "status" && (value == null || value == DBNull.Value))
                            {
                                if (tableName.ToLower() == "order" || tableName.ToLower() == "delivery")
                                    value = "Pending";
                                else if (tableName.ToLower() == "payment")
                                    value = "Pending";
                                else
                                    value = "Active";
                            }

                            columns.Append($"[{column}], ");
                            values.Append("@" + column + ", ");
                            cmd.Parameters.AddWithValue("@" + column, value ?? DBNull.Value);
                        }

                        if (columns.Length > 0) columns.Length -= 2;
                        if (values.Length > 0) values.Length -= 2;

                        sql = $"INSERT INTO {quotedTableName} ({columns}) VALUES ({values})";
                    }

                    // Debug output to verify SQL and parameters
                    Console.WriteLine("Executing SQL: " + sql);
                    foreach (SqlParameter p in cmd.Parameters)
                    {
                        Console.WriteLine($"{p.ParameterName} = {p.Value}");
                    }

                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving data: " + ex.Message, "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private string GetQuotedTableName(string tableName)
        {
            // List of SQL reserved keywords that need to be quoted
            string[] reservedKeywords = { "order", "table", "group", "user", "select", "from", "where", "join" , "Status", "status" };

            foreach (string keyword in reservedKeywords)
            {
                if (tableName.ToLower() == keyword)
                {
                    return $"[{tableName}]";
                }
            }

            return tableName;
        }

        private object GetControlValue(Control control)
        {
            if (control is TextBox textBox)
                return string.IsNullOrWhiteSpace(textBox.Text) ? DBNull.Value : (object)textBox.Text.Trim();
            if (control is ComboBox comboBox)
            {
                // Make sure ComboBox has a selection, especially for status fields
                if (comboBox.SelectedValue != null)
                    return comboBox.SelectedValue;
                else if (comboBox.SelectedItem != null)
                    return comboBox.SelectedItem;
                else if (control.Tag != null && control.Tag.ToString().ToLower().Contains("status"))
                {
                    // Provide default values for status fields
                    if (tableName.ToLower() == "order" || tableName.ToLower() == "delivery")
                        return "Pending";
                    else if (tableName.ToLower() == "payment")
                        return "Pending";
                    else
                        return "Active";
                }
                return DBNull.Value;
            }
            if (control is CheckBox checkBox)
                return checkBox.Checked;
            if (control is DateTimePicker datePicker)
                return datePicker.Value;

            return DBNull.Value;
        }

        // Fix for AddEditForm.cs - Update the CreateControlForColumn method
        private Control CreateControlForColumn(DataRow schemaRow)
        {
            string dataType = schemaRow["DATA_TYPE"].ToString().ToLower();
            bool isNullable = schemaRow["IS_NULLABLE"].ToString() == "YES";
            string columnName = schemaRow["COLUMN_NAME"].ToString();

            if (columnName.EndsWith("ID") && columnName != idColumnName)
            {
                ComboBox comboBox = new ComboBox();
                comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                PopulateForeignKeyDropdown(comboBox, columnName);
                return comboBox;
            }
            else if (dataType == "bit")
            {
                return new CheckBox { Text = "" };
            }
            else if (dataType == "datetime" || dataType == "date")
            {
                return new DateTimePicker { Format = DateTimePickerFormat.Short };
            }
            else if (dataType == "int" || dataType == "decimal" || dataType == "float" || dataType == "money")
            {
                return new TextBox();
            }
            else if (columnName.ToLower().Contains("status"))
            {
                ComboBox statusCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
                if (tableName == "order" || tableName == "delivery")
                {
                    statusCombo.Items.AddRange(new string[] { "Pending", "In Transit", "Delivered", "Cancelled" });
                    statusCombo.SelectedIndex = 0; // Default to "Pending"
                }
                else if (tableName == "payment")
                {
                    statusCombo.Items.AddRange(new string[] { "Pending", "Completed", "Failed", "Refunded" });
                    statusCombo.SelectedIndex = 0; // Default to "Pending"
                }
                else
                {
                    statusCombo.Items.AddRange(new string[] { "Active", "Inactive", "Pending" });
                    statusCombo.SelectedIndex = 0; // Default to "Active"
                }
                return statusCombo;
            }
            else
            {
                return new TextBox();
            }
        }

        private void PopulateForeignKeyDropdown(ComboBox comboBox, string foreignKeyColumn)
        {
            string referencedTable = "";
            string displayColumn = "";

            // Map foreign key columns to their referenced tables and appropriate display columns
            if (foreignKeyColumn == "CustomerID")
            {
                referencedTable = "Customer";
                displayColumn = "Fname";
            }
            else if (foreignKeyColumn == "RestaurantID")
            {
                referencedTable = "Restaurant";
                displayColumn = "Name";
            }
            else if (foreignKeyColumn == "MenuID")
            {
                referencedTable = "Menu";
                displayColumn = "category";
            }
            else if (foreignKeyColumn == "ItemID")
            {
                referencedTable = "menu_item";
                displayColumn = "Description";
            }
            else if (foreignKeyColumn == "OrderID")
            {
                referencedTable = "[order]";
                displayColumn = "OrderID";
            }
            else if (foreignKeyColumn == "DeliveryID")
            {
                referencedTable = "delivery";
                displayColumn = "DeliveryID";
            }
            else if (foreignKeyColumn == "DeliverManID")
            {
                referencedTable = "Deliveryman";
                displayColumn = "Name";
            }
            else if (foreignKeyColumn == "PaymentID")
            {
                referencedTable = "payment";
                displayColumn = "PaymentID";
            }

            if (string.IsNullOrEmpty(referencedTable)) return;

            // Get the proper primary key column name
            string idColumn = GetPrimaryKeyColumnName(referencedTable);

            // Properly quote the table name
            string quotedTableName = GetQuotedTableName(referencedTable);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = $"SELECT [{idColumn}] AS ID, [{displayColumn}] AS DisplayValue FROM {quotedTableName}";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        comboBox.DataSource = dt;
                        comboBox.DisplayMember = "DisplayValue";
                        comboBox.ValueMember = "ID";
                    }
                    else
                    {
                        comboBox.Items.Add("No data available");
                        comboBox.Enabled = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading {referencedTable} data: {ex.Message}", "Database Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    comboBox.Items.Add("Error loading data");
                    comboBox.Enabled = false;
                }
            }
        }

        private DataTable GetTableSchema()
        {
            DataTable schemaTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Quote the table name for the INFORMATION_SCHEMA query
                    string quotedTableName = GetQuotedTableName(this.tableName);
                    // Remove brackets for INFORMATION_SCHEMA query as it expects unquoted names
                    string normalizedTableName = this.tableName;

                    string schemaQuery = $@"
                SELECT 
                    COLUMN_NAME, 
                    DATA_TYPE, 
                    IS_NULLABLE 
                FROM 
                    INFORMATION_SCHEMA.COLUMNS 
                WHERE 
                    TABLE_NAME = '{normalizedTableName}' 
                ORDER BY 
                    ORDINAL_POSITION";

                    SqlDataAdapter adapter = new SqlDataAdapter(schemaQuery, connection);
                    adapter.Fill(schemaTable);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading table schema: {ex.Message}", "Database Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return schemaTable;
        }
    }
}