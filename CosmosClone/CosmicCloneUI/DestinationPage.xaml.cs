// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CosmosCloneCommon.Utility;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CosmicCloneUI.Extensions;

namespace CosmicCloneUI
{
    /// <summary>
    /// Interaction logic for DestinationPage.xaml
    /// </summary>
    public partial class DestinationPage : Page
    {
        CosmosDBHelper cosmosHelper;
        public DestinationPage()
        {
            InitializeComponent();
            cosmosHelper = new CosmosDBHelper();
        }

        private void BtnTestTarget(object sender, RoutedEventArgs e)
        {
            TestDestinationConnection();
        }

        public bool TestDestinationConnection()
        {
            ConnectionTestMsg.Text = "";
            CloneSettings.TargetSettings = new CosmosCollectionValues()
            {
                EndpointUrl = TargetURL.Text.ToString(),
                AccessKey = TargetKey.Text.ToString(),
                DatabaseName = TargetDB.Text.ToString(),
                CollectionName = TargetCollection.Text.ToString()
            };

            var result = cosmosHelper.TestTargetConnection();
            if (result.IsSuccess)
            {
                var connectionIcon = (Image)this.FindName("ConnectionIcon");
                ConnectionIcon.Source = new BitmapImage(new Uri("/Images/success.png", UriKind.Relative));
                ConnectionTestMsg.Text = "Validation Passed";
            }
            else
            {
                var connectionIcon = (Image)this.FindName("ConnectionIcon");
                ConnectionIcon.Source = new BitmapImage(new Uri("/Images/fail.png", UriKind.Relative));
                ConnectionTestMsg.Text = result.Message;
            }
            return result.IsSuccess;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        { 
            var settings = new CosmosCollectionValues
            {
                EndpointUrl = TargetURL.Text.Encrypt(),
                AccessKey = TargetKey.Text.Encrypt(),
                DatabaseName = TargetDB.Text.Encrypt(),
                CollectionName = TargetCollection.Text.Encrypt()
            };
            
            new SaveFileDialog().SaveFile(Environment.SpecialFolder.MyDocuments, "Target", settings);
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();

            try
            {
                var settings = dialog.LoadFile<CosmosCollectionValues>(Environment.SpecialFolder.MyDocuments, "Target");

                if (settings != null)
                {
                    TargetURL.Text = settings.EndpointUrl.Decrypt();
                    TargetKey.Text = settings.AccessKey.Decrypt();
                    TargetDB.Text = settings.DatabaseName.Decrypt();
                    TargetCollection.Text = settings.CollectionName.Decrypt();
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"Unable to load Target from file: {dialog.FileName}", $"Failed to load Target", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
