using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public static ObservableCollection<EquipmentObject> MatchAndMerge(ObservableCollection<FucModel> fucData, ObservableCollection<ITEQModel> iteqData) // Creates a list with the merged data from the two CSV files
    {
        ObservableCollection<EquipmentObject> mergedRecords = new(); // Create new list to store the merged data

        // first match everything that matches between the iteq and the fuc
        foreach (var iteqRow in iteqData) // Loop through each row in ITEQ data csv file
        {
            string matchNumber = iteqRow.GgLabel; // Use the GG-LABEL number to match the rows in the two CSV files

            var fucMatch = fucData.FirstOrDefault(f => ExtractNumber(f.PC) == matchNumber); // Use the ExtractNumber method to extract the number from the PC column, and match the number with the GG-LABEL number

            var unified = new EquipmentObject // Merge the two Csv files where possible
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

                // if fuc match not found fill with blank values instead
                PC = fucMatch?.PC ?? "",
                FucUser = fucMatch?.USER ?? "",
                Username = fucMatch?.Username ?? "",
                Date = fucMatch?.DATE,
                ReportDate = fucMatch?.ReportDATE,
                PCLocation = fucMatch?.PCLocation ?? "",
                EmplMailAdresse = fucMatch?.Empl_Mailadresse ?? ""
            };
            mergedRecords.Add(unified);
        }

        // second, try to add al fucmodel records that were not mathced with iteqdata
        foreach (var fucRow in fucData)
        {
            string matchNumber = ExtractNumber(fucRow.PC);
            bool alreadyMatched = mergedRecords.Any(m => m.GgLabel == matchNumber);

            if (!alreadyMatched)
            {
                var unified = new EquipmentObject
                {
                    Column = "",
                    GgLabel = matchNumber,
                    Type = "",
                    Make = "",
                    Model = "",
                    SerialNo = "",
                    SecurityId = "",
                    User = "",
                    Site = "",
                    Status = "",
                    PurchaseDate = null,
                    Received = null,
                    ShortComment = "",

                    // Use values from fucRow
                    PC = fucRow.PC,
                    FucUser = fucRow.USER,
                    Username = fucRow.Username,
                    Date = fucRow.DATE,
                    ReportDate = fucRow.ReportDATE,
                    PCLocation = fucRow.PCLocation,
                    EmplMailAdresse = fucRow.Empl_Mailadresse
                };
                mergedRecords.Add(unified);
            }
        }
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
    public static void LoadData(ObservableCollection<EquipmentObject> CompleteItemCollection, MainWindow mainWindow)
    {
        if (CompleteItemCollection == null || !CompleteItemCollection.Any())
        {
            MessageBox.Show("No data available.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Pass data to MainWindow and update ListView
        mainWindow.LoadData(CompleteItemCollection);
    }

    public static string ShowSaveFileDialog() // Choose how to save the merged data
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

