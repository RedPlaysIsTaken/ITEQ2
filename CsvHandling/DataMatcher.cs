using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using ITEQ2;
using ITEQ2.CsvHandling;
using Microsoft.Win32;

public class DataMatcher
{
    public static List<UnifiedModel> MatchAndMerge(List<FucModel> fucData, List<ITEQModel> iteqData) // Creates a list with the merged data from the two CSV files
    {
        List<UnifiedModel> mergedRecords = new(); // Create new list to store the merged data

        foreach (var iteqRow in iteqData) // Loop through each row in ITEQ data csv file
        {
            string matchNumber = iteqRow.GgLabel; // Use the GG-LABEL number to match the rows in the two CSV files

            var fucMatch = fucData.FirstOrDefault(f => ExtractNumber(f.PC) == matchNumber); // Use the ExtractNumber method to extract the number from the PC column, and match the number with the GG-LABEL number

            if (fucMatch != null)
            {
                var unified = new UnifiedModel // Merge the two Csv files
                {
                    Column = iteqRow.Column,
                    GgLabel = iteqRow.GgLabel,
                    Type = iteqRow.Type,
                    Make = iteqRow.Make,
                    Model = iteqRow.Model,
                    SerialNo = iteqRow.SerialNo,
                    SecurityId = iteqRow.SecurityId,
                    User = iteqRow.User,
                    Site = iteqRow.Site,
                    Status = iteqRow.Status,
                    PurchaseDate = iteqRow.PurchaseDate,
                    Received = iteqRow.Received,
                    ShortComment = iteqRow.ShortComment,
                    PC = fucMatch.PC,
                    FucUser = fucMatch.USER,
                    Username = fucMatch.Username,
                    Date = fucMatch.DATE,
                    ReportDate = fucMatch.ReportDATE,
                    PCLocation = fucMatch.PCLocation,
                    EmplMailAdresse = fucMatch.Empl_Mailadresse
                };

                mergedRecords.Add(unified);
            }
        }

        //string savePath = ShowSaveFileDialog(); // Ask user where to save the file
        //if (!string.IsNullOrEmpty(savePath))
        //{
        //    SaveToCsv(mergedRecords, savePath);
        //    System.Diagnostics.Debug.WriteLine($"CSV file saved to: {savePath}");
        //    MessageBox.Show($"CSV file saved to: {savePath}", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        //}

        return mergedRecords;
    }

    private static string ExtractNumber(string pcValue) // Extracts the number from the PC column
    {
        if (string.IsNullOrEmpty(pcValue)) // Check if the PC column is empty
        {
            return string.Empty;
        }
        else
        {
            // Use regex to extract the numeric part from "PC1031" or "PCL1031" to get "1031"
            Match match = Regex.Match(pcValue, @"\d+");
            return match.Success ? match.Value : string.Empty;
        }
    }

    public static void SaveToCsv(List<UnifiedModel> mergedRecords, string filePath) // Save the merged data to a CSV file
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ","
        };

        try
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, config);
            csv.WriteRecords(mergedRecords);

            System.Diagnostics.Debug.WriteLine($"File successfully saved to: {filePath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving CSV: {ex.Message}");
            MessageBox.Show($"Error saving CSV: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public static void LoadData(List<UnifiedModel> unifiedData, MainWindow mainWindow)
    {
        if (unifiedData == null || !unifiedData.Any())
        {
            MessageBox.Show("No data available.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Pass data to MainWindow and update ListView
        mainWindow.LoadData(unifiedData);
    }

    //public static string ShowSaveFileDialog() // Choose how to save the merged data
    //{
    //    SaveFileDialog saveFileDialog = new SaveFileDialog
    //    {
    //        Filter = "CSV files (*.csv)|*.csv",
    //        Title = "Save Merged Data",
    //        FileName = "MergedOutput.csv"
    //    };

    //    return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : string.Empty;
    //}
}

