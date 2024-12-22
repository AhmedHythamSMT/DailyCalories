using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace DailyCalories
{
    public partial class Form1 : Form
    {
        string connectionString = "Server=localhost;Database=FitnessDB;Uid=root;";
        private int caloriesPerMinute;

        public Form1()
        {
            InitializeComponent();
            LoadActivities();  // Load activities into the combo box on startup
            lblCaloriesBurned.Text = "Calories Burned: 0";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Additional initialization code if needed
        }

        private void LoadActivities()
        {
            // Example activities with their calorie rates

            cbActivities.Items.Add(new ActivityItem("Running", 10));
            cbActivities.Items.Add(new ActivityItem("Cycling", 12));
            cbActivities.Items.Add(new ActivityItem("Swimming", 22));
            cbActivities.Items.Add(new ActivityItem("Walking", 5));
            cbActivities.Items.Add(new ActivityItem("Cardio", 42));
            cbActivities.Items.Add(new ActivityItem("Diving", 22));
            cbActivities.DisplayMember = "Name";

        }
        private void ClearFields()
        {
            txtDuration.Clear();
            lblCaloriesBurned.Text = "Calories Burned: 0";
            cbActivities.SelectedIndex = -1; // Reset combo box selection
            txtAge.Text = "";
            txtWeight.Text ="";
        }


        private void btnCalculate_Click(object sender, EventArgs e)
        {
            if (cbActivities.SelectedItem == null || !int.TryParse(txtDuration.Text, out int duration) ||
                !int.TryParse(txtAge.Text, out int age) || !decimal.TryParse(txtWeight.Text, out decimal weight))
            {
                MessageBox.Show("Please select an activity, enter a valid duration, age, and weight.");
                return;
            }

            var selectedActivity = (ActivityItem)cbActivities.SelectedItem;
            caloriesPerMinute = selectedActivity.CaloriesPerMinute;

            // Adjust calories burned based on weight (simple example)
            int caloriesBurned = (int)(caloriesPerMinute * weight / 70) * duration; // Assuming 70kg is the base weight

            lblCaloriesBurned.Text = $"Calories Burned: {caloriesBurned}";
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string activity = cbActivities.Text.Trim();
                string durationPlayed = txtDuration.Text.Trim();
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                int caloriesBurned = int.Parse(lblCaloriesBurned.Text.Split(':')[1].Trim());
                // Capture age and weight
                int age = int.Parse(txtAge.Text.Trim());
                decimal weight = decimal.Parse(txtWeight.Text.Trim());

                // Insert into Database
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO DailyCalories (Activity, DurationPlayed, Date, CaloriesBurned, Age, Weight) VALUES (@activity, @durationplayed, @date, @calories, @age, @weight)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@activity", activity);
                    cmd.Parameters.AddWithValue("@durationplayed", durationPlayed);
                    cmd.Parameters.AddWithValue("@date", date);
                    cmd.Parameters.AddWithValue("@calories", caloriesBurned);
                    cmd.Parameters.AddWithValue("@age", age);
                    cmd.Parameters.AddWithValue("@weight", weight);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Data Saved!", "Save Successfully");

                    // Clear fields after saving
                    ClearFields();
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter valid numbers for calories, age, and weight.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }


        private void cbActivities_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            HistoryForm historyForm = new HistoryForm();
            historyForm.Show();

        }
    }


        // Define a simple class to represent an activity and its calories per minute
        public class ActivityItem
        {
            public string Name { get; set; }
            public int CaloriesPerMinute { get; set; }

            public ActivityItem(string name, int caloriesPerMinute)
            {
                Name = name;
                CaloriesPerMinute = caloriesPerMinute;
            }
        }
    }
