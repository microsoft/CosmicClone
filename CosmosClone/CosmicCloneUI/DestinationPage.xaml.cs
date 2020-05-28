// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CosmosCloneCommon.Utility;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CosmicCloneUI.Extensions;
using static System.Environment.SpecialFolder;
using static CosmosCloneCommon.Utility.CloneSettings;

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

         private void SaveButton_Click(object sender, RoutedEventArgs e) => new SaveFileDialog().SaveFile(MyDocuments, "Target", SourceSettings);

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = new OpenFileDialog().LoadFile<CosmosCollectionValues>(MyDocuments, "Target");

            if (settings != null)
            {
                TargetURL.Text = settings.EndpointUrl;
                TargetKey.Text = settings.AccessKey;
                TargetDB.Text =  settings.DatabaseName;
                TargetCollection.Text = settings.CollectionName;
            }
        }
    }
}
