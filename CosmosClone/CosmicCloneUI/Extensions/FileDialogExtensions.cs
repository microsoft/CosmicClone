using CosmosCloneCommon.Utility;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using static System.Windows.MessageBoxButton;
using static System.Windows.MessageBoxImage;

namespace CosmicCloneUI.Extensions
{
    public static class FileDialogExtensions
    {

        public static void SaveFile<T>(this SaveFileDialog dialog, Environment.SpecialFolder folder, string fileName, T data)
        {
            dialog.InitialDirectory = Environment.GetFolderPath(folder);
            dialog.Filter = "XML file (*.xml)|*.xml";
            dialog.Title = $"Save {fileName}";
            dialog.FileName = $"{fileName}_{DateTime.Now.ToString("MM-dd-yyyy-HHmmss", CultureInfo.InvariantCulture)}";

            if (dialog.ShowDialog() == true)
            {
                var xmlText = CloneSerializer.XMLSerialize(data);
                File.WriteAllText(dialog.FileName, xmlText);
            }
        }

        public static T LoadFile<T>(this OpenFileDialog dialog, Environment.SpecialFolder folder, string fileName)
        {
            dialog.InitialDirectory = Environment.GetFolderPath(folder);
            dialog.Filter = "XML file (*.xml)|*.xml";
            dialog.Title = $"Load {fileName}";

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    return CloneSerializer.XMLDeserialize<T>(File.ReadAllText(dialog.FileName));
                }
                catch (Exception)
                {
                    MessageBox.Show($"Unable to load {fileName} from file: {dialog.FileName}", $"Failed to load {fileName}", OK, Warning);
                    return default;
                }
            }
            return default;
        }
    }
}
