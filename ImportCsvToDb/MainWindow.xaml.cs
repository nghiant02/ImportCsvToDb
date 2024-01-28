using System.IO;
using System.Windows;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Repository.Models;

namespace ImportCsvToDb
{
    public partial class MainWindow : Window
    {
        private readonly HighSchoolScoresContext dbContext;
        public List<Score> Scores { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            dbContext = new HighSchoolScoresContext();
            Scores = new List<Score>();
            dgScores.ItemsSource = Scores; 
        }

        private void LoadDataFromCsv(string filePath)
        {
            try
            {
                // Read all lines from the CSV file
                string[] lines = File.ReadAllLines(filePath);

                // Assuming the CSV has a header row, skip the first line
                for (int i = 1; i < lines.Length; i++)
                {
                    // Split the line into columns using comma as the delimiter
                    string[] columns = lines[i].Split(',');

                    // Create a new Score object and populate its properties
                    Score score = new Score
                    {
                        SBD = int.Parse(columns[0]),
                        Toan = ParseNullableDouble(columns[1]),
                        Van = ParseNullableDouble(columns[2]),
                        Ly = ParseNullableDouble(columns[3]),
                        Sinh = ParseNullableDouble(columns[4]),
                        Ngoai_ngu = ParseNullableDouble(columns[5]),
                        Year = ParseNonNullableInt(columns[6]),
                        Hoa = ParseNullableDouble(columns[7]),
                        Lich_su = ParseNullableDouble(columns[8]),
                        Dia_ly = ParseNullableDouble(columns[9]),
                        GDCD = ParseNullableDouble(columns[10]),
                        MaTinh = ParseNonNullableInt(columns[11])
                    };

                    // Add the score to the Scores list
                    Scores.Add(score);
                }

                // Refresh the DataGrid to display the loaded data
                dgScores.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading CSV file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private double? ParseNullableDouble(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return double.Parse(value);
        }

        private int ParseNonNullableInt(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0; 
            }

            return int.Parse(value);
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV Files (*.csv)|*.csv|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                txtFile.Text = filePath;

                LoadDataFromCsv(filePath);
            }
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Disable tracking to improve performance during bulk insert
                dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                foreach (var score in Scores)
                {
                    // Check if the record with the same primary key already exists
                    var existingScore = dbContext.Scores.Find(score.SBD);

                    if (existingScore == null)
                    {
                        // Record doesn't exist, insert
                        dbContext.Entry(score).State = EntityState.Added;
                    }
                    else
                    {
                        // Record exists, update
                        dbContext.Entry(existingScore).CurrentValues.SetValues(score);
                    }
                }

                dbContext.SaveChanges();

                // Enable tracking back if needed
                dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

                MessageBox.Show("Import successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            Scores.Clear(); 
            txtFile.Clear();
            dgScores.Items.Refresh(); 
        }
    }
}
