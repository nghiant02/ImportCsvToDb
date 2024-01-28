using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
        public List<int> Years { get; set; } = new List<int>();

        public MainWindow()
        {
            InitializeComponent();
            dbContext = new HighSchoolScoresContext();
            Scores = new List<Score>();
            dgScores.ItemsSource = Scores;

            // Bind the ComboBox to the Years property
            cbYear.ItemsSource = Years;

            // Đăng ký sự kiện SelectionChanged cho ComboBox
            cbYear.SelectionChanged += CbYear_SelectionChanged;
        }

        private void CbYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Lọc danh sách Scores theo năm đã chọn
            int? selectedYear = cbYear.SelectedItem as int?;

            if (selectedYear.HasValue)
            {
                var filteredScores = Scores.Where(score => score.Year == selectedYear.Value).ToList();

                // Sắp xếp danh sách theo năm (hoặc theo một trường khác nếu cần)
                filteredScores = filteredScores.OrderBy(score => score.Year).ToList();

                // Gán lại danh sách đã lọc và sắp xếp cho DataGrid
                dgScores.ItemsSource = filteredScores;
                dgScores.Items.Refresh();
            }
        }

        private void LoadDataFromCsv(string filePath)
        {
            try
            {
                // Read all lines from the CSV file
                string[] lines = File.ReadAllLines(filePath);

                // Clear existing data
                Scores.Clear();
                Years.Clear();

                // Assuming the CSV has a header row, skip the first line
                for (int i = 1; i < lines.Length; i++)
                {
                    // Split the line into columns using comma as the delimiter
                    string[] columns = lines[i].Split(',');

                    // Extract the year using int.Parse
                    int year = int.Parse(columns[6]);

                    // Add the year to the Years list if it's not already there
                    if (!Years.Contains(year))
                    {
                        Years.Add(year);
                    }

                    // Create a new Score object and populate its properties
                    Score score = new Score
                    {
                        SBD = int.Parse(columns[0]),
                        Toan = ParseNullableDouble(columns[1]),
                        Van = ParseNullableDouble(columns[2]),
                        Ly = ParseNullableDouble(columns[3]),
                        Sinh = ParseNullableDouble(columns[4]),
                        Ngoai_ngu = ParseNullableDouble(columns[5]),
                        Year = int.Parse(columns[6]),
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
                dgScores.ItemsSource = Scores;
                dgScores.Items.Refresh();

                // Set the ItemsSource of the ComboBox to the Years list
                cbYear.ItemsSource = Years;
                cbYear.Items.Refresh();
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

            // Clear the DataGrid
            dgScores.ItemsSource = null;
            dgScores.Items.Refresh();

            // Clear the ComboBox
            cbYear.ItemsSource = null;
            cbYear.Items.Refresh();
        }
    }
}
