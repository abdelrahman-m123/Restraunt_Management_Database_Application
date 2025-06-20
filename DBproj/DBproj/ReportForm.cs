using System;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;

namespace FoodDeliverySystem
{
    public partial class ReportForm : Form
    {
        private string connectionString;
        private TextBox txtCustomerID;
        private TextBox txtRestaurantID;
        private Button btnGenerate;
        private Panel reportPanel;
        private void InitializeInputControls()
        {
            // Input panel for better organization
            Panel inputPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(inputPanel);

            // Customer ID input
            txtCustomerID = new TextBox { Location = new Point(120, 20), Width = 100 };
            inputPanel.Controls.Add(new Label { Text = "Customer ID:", Location = new Point(20, 20) });
            inputPanel.Controls.Add(txtCustomerID);

            // Restaurant ID input
            txtRestaurantID = new TextBox { Location = new Point(120, 50), Width = 100 };
            inputPanel.Controls.Add(new Label { Text = "Restaurant ID:", Location = new Point(20, 50) });
            inputPanel.Controls.Add(txtRestaurantID);

            // Generate button
            btnGenerate = new Button
            {
                Text = "Generate Report",
                Location = new Point(250, 35),
                Size = new Size(120, 30)
            };
            btnGenerate.Click += GenerateReport_Click;
            inputPanel.Controls.Add(btnGenerate);
        }

        public ReportForm(string connectionString)
        {

            this.connectionString = connectionString;

            
            reportPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            this.Controls.Add(reportPanel);

            InitializeInputControls();
        }

        private void GenerateReport_Click(object sender, EventArgs e)
        {
            reportPanel.Controls.Clear();
            int yPosition = 20;

            // Customer reports
            if (int.TryParse(txtCustomerID.Text, out int customerId))
            {
                AddReportItem("Customer Order Count:", GetOrderCountForCustomer(customerId).ToString(), ref yPosition);
                AddReportItem("Total Spent by Customer:", GetTotalSpentByCustomer(customerId).ToString("C"), ref yPosition);
            }

            // Restaurant reports
            if (int.TryParse(txtRestaurantID.Text, out int restaurantId))
            {
                AddReportItem("Average Restaurant Rating:", GetAverageRatingForRestaurant(restaurantId).ToString("0.00"), ref yPosition);
            }

            if (yPosition == 20) // No valid inputs
            {
                AddReportItem("Notice:", "Please enter valid ID(s)", ref yPosition);
            }
        }

        private void AddReportItem(string label, string value, ref int yPos)
        {
            var lbl = new Label
            {
                Text = label,
                AutoSize = true,
                Location = new Point(20, yPos),
                Font = new Font(this.Font, FontStyle.Bold)
            };
            reportPanel.Controls.Add(lbl);

            var val = new Label
            {
                Text = value,
                AutoSize = true,
                Location = new Point(200, yPos)
            };
            reportPanel.Controls.Add(val);

            yPos += 30;
        }

        private int GetOrderCountForCustomer(int customerId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT dbo.GetOrderCountForCustomer(@CustomerID)", connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);
                    return (int)command.ExecuteScalar();
                }
            }
        }

        private decimal GetTotalSpentByCustomer(int customerId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT dbo.GetTotalSpentByCustomer(@CustomerID)", connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);
                    return (decimal)command.ExecuteScalar();
                }
            }
        }

        private double GetAverageRatingForRestaurant(int restaurantId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT dbo.GetAverageRatingForRestaurant(@RestaurantID)", connection))
                {
                    command.Parameters.AddWithValue("@RestaurantID", restaurantId);
                    object result = command.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToDouble(result);
                }
            }
        }
    }
}