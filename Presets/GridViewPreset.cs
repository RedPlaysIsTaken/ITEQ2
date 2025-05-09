using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace ITEQ2.Presets
{
    public class GridViewPreset
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<double> ColumnWidths { get; set; } = new();
        public List<string> ColumnOrder { get; set; } = new();
    }

    public static class GridViewPresetManager
    {
        private static readonly string PresetFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ITEQ2/GridViewPresets");

        public static void SavePreset(string name, GridView gridView, string description = "")
        {
            Directory.CreateDirectory(PresetFolder);

            var preset = new GridViewPreset { Name = name , Description = description };

            foreach (var column in gridView.Columns)
            {
                preset.ColumnWidths.Add(column.Width);
                preset.ColumnOrder.Add(column.Header?.ToString());
            }

            var json = JsonSerializer.Serialize(preset);
            File.WriteAllText(Path.Combine(PresetFolder, name + ".json"), json);
        }

        public static void LoadPreset(string name, GridView gridView)
        {
            string path = Path.Combine(PresetFolder, name + ".json");
            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);
            var preset = JsonSerializer.Deserialize<GridViewPreset>(json);

            if (preset.ColumnOrder.Count > 0)
            {
                var currentColumns = gridView.Columns.ToList();
                var orderedColumns = new List<GridViewColumn>();

                foreach (var header in preset.ColumnOrder)
                {
                    var col = currentColumns.FirstOrDefault(c => c.Header?.ToString() == header);
                    if (col != null)
                        orderedColumns.Add(col);
                }

                gridView.Columns.Clear();
                foreach (var col in orderedColumns)
                    gridView.Columns.Add(col);
            }

            for (int i = 0; i < preset.ColumnWidths.Count && i < gridView.Columns.Count; i++)
            {
                gridView.Columns[i].Width = preset.ColumnWidths[i];
            }
        }

        public static List<GridViewPreset> GetAvailablePresets()
        {
            Directory.CreateDirectory(PresetFolder);
            var files = Directory.GetFiles(PresetFolder, "*.json");
            var presets = new List<GridViewPreset>();

            foreach (var file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var preset = JsonSerializer.Deserialize<GridViewPreset>(json);
                    if (preset != null)
                    {
                        presets.Add(preset);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to load preset: {file}. Error: {ex.Message}");
                }
            }
            return presets;
        }
    }
    public class GridViewPresetsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<GridViewPreset> GridViewPresets { get; set; } = new();

        public void LoadPresets()
        {
            GridViewPresets.Clear();
            foreach (var preset in GridViewPresetManager.GetAvailablePresets())
            {
                GridViewPresets.Add(preset);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
