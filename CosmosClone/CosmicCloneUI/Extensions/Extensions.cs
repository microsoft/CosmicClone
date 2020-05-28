using CosmosCloneCommon.Utility;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace CosmicCloneUI.Extensions
{
    public static class Extensions
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
                var text = File.ReadAllText(dialog.FileName);
                return CloneSerializer.XMLDeserialize<T>(text);
            }
            return default;
        }

        private readonly static byte[] entropy = Encoding.Unicode.GetBytes("Add some spice to the mix");

        public static string Encrypt(this string value)
        {
            var encrypted = ProtectedData.Protect(Encoding.Unicode.GetBytes(value), entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(this string value)
        {
            var decrypted = ProtectedData.Unprotect(Convert.FromBase64String(value), entropy, DataProtectionScope.CurrentUser);
            return Encoding.Unicode.GetString(decrypted);
        }
    }
}