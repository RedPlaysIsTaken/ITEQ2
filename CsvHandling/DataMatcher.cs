using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using CsvHelper;
using CsvHelper.Configuration;
using ITEQ2.CsvHandling;
using Microsoft.Win32;

public class DataMatcher
{
    public static List<UnifiedModel> MatchAndMerge(List<FucModel> fucData, List<ITEQModel> iteqData)
    {
        List<UnifiedModel> mergedRecords = new();

        foreach (var iteqRow in iteqData)
        {
            string matchNumber = iteqRow.GgLabel; // The GG-LABEL number

            // Find the matching row in FucModel where PC contains this number
            var fucMatch = fucData.FirstOrDefault(f => ExtractNumber(f.PC) == matchNumber);

            if (fucMatch != null)
            {
                // Merge data into the unified model
                var unified = new UnifiedModel
                {
                    GgLabel = iteqRow.GgLabel,
                    User = iteqRow.User,
                    Type = iteqRow.Type,
                    Make = iteqRow.Make,
                    Model = iteqRow.Model,
                    SerialNo = iteqRow.SerialNo,
                    SecurityId = iteqRow.SecurityId,
                    Site = iteqRow.Site,
                    Status = iteqRow.Status,
                    PurchaseDate = iteqRow.PurchaseDate,
                    Received = iteqRow.Received,
                    ShortComment = iteqRow.ShortComment,

                    PC = fucMatch.PC,
                    Username = fucMatch.Username,
                    Date = fucMatch.DATE,
                    ReportDate = fucMatch.ReportDATE,
                    PCLocation = fucMatch.PCLocation,
                    EmplMailAdresse = fucMatch.Empl_Mailadresse
                };

                mergedRecords.Add(unified);
            }
        }

        // Ask user where to save the file
        string savePath = ShowSaveFileDialog();
        if (!string.IsNullOrEmpty(savePath))
        {
            SaveToCsv(mergedRecords, savePath);
            System.Diagnostics.Debug.WriteLine($"CSV file saved to: {savePath}");
            MessageBox.Show($"CSV file saved to: {savePath}", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        return mergedRecords;
    }

    private static string ExtractNumber(string pcValue)
    {
        if (string.IsNullOrEmpty(pcValue))
            return string.Empty;

        // Use regex to extract the numeric part from "PC1031" or "PCL1031"
        Match match = Regex.Match(pcValue, @"\d+");
        return match.Success ? match.Value : string.Empty;
    }

    public static void SaveToCsv(List<UnifiedModel> mergedRecords, string filePath)
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

    public static string ShowSaveFileDialog()
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            Title = "Save Merged Data",
            FileName = "MergedOutput.csv"
        };

        return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : string.Empty;
    }
}

