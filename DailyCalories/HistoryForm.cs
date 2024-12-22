using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;



namespace DailyCalories
{
    public partial class HistoryForm : Form
    {
        string connString = "server=localhost;user id=root;database=FitnessDB;sslmode=none;Pooling=false;";


        public HistoryForm()
        {
            InitializeComponent();

            // No need to attach event handlers here if you're doing it in the Designer
            // Optionally attach event handlers if not done via designer
            this.Load += new EventHandler(HistoryForm_Load);
            this.dataGridViewHistory.Click += new EventHandler(dataGridViewHistory_Click);



        }

        private void HistoryForm_Load(object sender, EventArgs e)
        {
            if (dataGridViewHistory != null)
            {
                FillDGV(""); // Load data when the form loads
                txtId.ReadOnly = true;
                txtDate.ReadOnly = true;
                txtCalories.ReadOnly = true;
                txtDuration.ReadOnly = true;
                txtActivity.ReadOnly = true;
                txtWeight.ReadOnly = true;
                txtAge.ReadOnly = true;

                dataGridViewHistory.Columns["Id"].Visible = false; // Hiding the ID column

            }
            else
            {
                MessageBox.Show("dataGridViewHistory is not initialized.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            



        }

        public void FillDGV(string searchValue)
{
    try
    {
        using (MySqlConnection conn = new MySqlConnection(connString))
        {
            conn.Open();
            string sql = "SELECT * FROM dailycalories";

            // Only add search filter if searchValue is not empty
            if (!string.IsNullOrEmpty(searchValue))
            {
                sql += " WHERE CONCAT(Activity, DurationPlayed, CaloriesBurned, Date, Age, Weight) LIKE @searchValue";
                    }
            
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                if (!string.IsNullOrEmpty(searchValue))
                {
                    cmd.Parameters.AddWithValue("@searchValue", "%" + searchValue + "%");
                }

                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable table = new DataTable();
                adapter.Fill(table);

                // Check if the table is empty
                if (table.Rows.Count == 0)
                {
                    MessageBox.Show("No data available.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Proceed with DataGridView setup only if table has data
                    dataGridViewHistory.RowTemplate.Height = 50;
                    dataGridViewHistory.AllowUserToAddRows = false;
                    dataGridViewHistory.DataSource = table;
                    dataGridViewHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

        private void dataGridViewHistory_Click(object sender, EventArgs e)
        {
            if (dataGridViewHistory.CurrentRow != null && dataGridViewHistory.CurrentRow.Index >= 0)
            {
                dataGridViewHistory.Sort(dataGridViewHistory.Columns["Date"], ListSortDirection.Ascending);
                dataGridViewHistory.Sort(dataGridViewHistory.Columns["CaloriesBurned"], ListSortDirection.Ascending);

                txtId.Text = dataGridViewHistory.CurrentRow.Cells["Id"].Value.ToString();
                txtActivity.Text = dataGridViewHistory.CurrentRow.Cells["Activity"].Value.ToString();
                txtDuration.Text = dataGridViewHistory.CurrentRow.Cells["DurationPlayed"].Value.ToString();
                txtAge.Text = dataGridViewHistory.CurrentRow.Cells["Age"].Value.ToString();
                txtWeight.Text = dataGridViewHistory.CurrentRow.Cells["Weight"].Value.ToString();

                // Handling date assignment safely
                object dateValue = dataGridViewHistory.CurrentRow.Cells["Date"].Value;

                // Check for DBNull to avoid exceptions
                if (dateValue != DBNull.Value)
                {
                    if (dateValue is DateTime)
                    {
                        DateTime date = (DateTime)dateValue;
                        txtDate.Text = date.ToString("yyyy-MM-dd"); // Formatting the date if its a valid DateTime
                    }
                    else
                    {
                        MessageBox.Show("Unexpected date format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtDate.Text = string.Empty; // Clear in case of unexpected format
                    }
                }
                else
                {
                    txtDate.Text = string.Empty; // Clear if the date is NULL
                }


                txtCalories.Text = dataGridViewHistory.CurrentRow.Cells["CaloriesBurned"].Value.ToString();
                dataGridViewHistory.CurrentRow.Cells["CaloriesBurned"].ToolTipText = $"You have burn {txtCalories.Text} Calorie";
                dataGridViewHistory.CurrentRow.Cells["Activity"].ToolTipText = $"You go {txtActivity.Text} on {txtDate.Text}";
                dataGridViewHistory.CurrentRow.Cells["Date"].ToolTipText = $"You go {txtActivity.Text} on {txtDate.Text}";
            }

        }
        private void dataGridViewHistory_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridViewHistory.Columns[e.ColumnIndex].Name.Equals("CaloriesBurned") && e.Value != null)
            {
                int caloriesBurned = (int)e.Value;
                if (caloriesBurned > 50)
                {
                    e.CellStyle.BackColor = Color.LightGreen; // Highlight high-calorie burns
                    dataGridViewHistory.CurrentRow.Cells["CaloriesBurned"].ToolTipText = $"NO PAIN = NO GAIN!";
                }
                if (caloriesBurned > 100)
                {
                    e.CellStyle.BackColor = Color.SpringGreen; // Highlight high-calorie burns
                    dataGridViewHistory.CurrentRow.Cells["CaloriesBurned"].ToolTipText = $"KEEP GOING!";
                }
                if (caloriesBurned > 200 )
                {
                    e.CellStyle.BackColor = Color.GreenYellow; // Highlight high-calorie burns
                    dataGridViewHistory.CurrentRow.Cells["CaloriesBurned"].ToolTipText = $"DON'T GIVE UP!";
                }
                if (caloriesBurned > 400)
                {
                    e.CellStyle.BackColor = Color.PaleVioletRed; // Highlight high-calorie burns
                    dataGridViewHistory.CurrentRow.Cells["CaloriesBurned"].ToolTipText = $"You Have done very well. Keep Burning";
                }
            }
        }


        public void ExecuteMyQuery(MySqlCommand cmd, string msg)
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                conn.Open();
                if (cmd.ExecuteNonQuery() == 1)
                {
                    MessageBox.Show(msg);
                    FillDGV("");
                }
                else
                {
                    MessageBox.Show("Query Not Executed");
                }
            }
        }


        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Confirm deletion
            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this entry?",
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connString))
                    {
                        conn.Open();
                        string sql = "DELETE FROM dailycalories WHERE Id = @id";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", txtId.Text);
                            ExecuteMyQuery(cmd, "Entry Deleted Successfully");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            FillDGV(txtSearch.Text);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            // Optionally confirm with the user before closing
            if (MessageBox.Show("Are you sure you want to exit?", "Confirm Exit", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void ExportToCSV()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.FileName = "Exported_Data.csv";
                saveFileDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                saveFileDialog.DefaultExt = "csv";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
                        {
                            // Loop through the columns and write the header
                            for (int i = 0; i < dataGridViewHistory.Columns.Count; i++)
                            {
                                sw.Write(CleanCSVField(dataGridViewHistory.Columns[i].HeaderText));
                                if (i < dataGridViewHistory.Columns.Count - 1)
                                    sw.Write(",");
                            }
                            sw.WriteLine();

                            // Loop through the rows and write the data
                            foreach (DataGridViewRow row in dataGridViewHistory.Rows)
                            {
                                for (int i = 0; i < row.Cells.Count; i++)
                                {
                                    sw.Write(CleanCSVField(row.Cells[i].Value?.ToString() ?? string.Empty)); // Handle potential null values
                                    if (i < row.Cells.Count - 1)
                                        sw.Write(",");
                                }
                                sw.WriteLine();
                            }
                        }
                        MessageBox.Show("Export completed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting to CSV: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // This method cleans data to be compatible with CSV format
        private string CleanCSVField(string field)
        {
            // If the field contains a comma or quotes, we need to wrap it in quotes and escape any inner quotes.
            if (field.Contains(",") || field.Contains("\""))
            {
                field = "\"" + field.Replace("\"", "\"\"") + "\""; // Escape double quotes
            }
            return field;
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportToCSV();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            txtId.Text = "";
            txtActivity.Text = "";
            txtDuration.Text = "";
            txtCalories.Text = "";
            txtDate.Text = "";
            txtAge.Text = "";
            txtWeight.Text = "";


        }

        private void HistoryForm_Load_1(object sender, EventArgs e)
        {

        }
    }
}
    
