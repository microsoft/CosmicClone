using CosmicCloneUI.Extensions;
using CosmosCloneCommon.Utility;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static System.Environment.SpecialFolder;
using static CosmosCloneCommon.Utility.CloneSettings;

namespace CosmicCloneUI
{
    /// <summary>
    /// Interaction logic for SourcePage.xaml
    /// </summary>
    public partial class SourcePage : Page
    {
        CosmosDBHelper cosmosHelper;
        public SourcePage()
        {
            InitializeComponent();
            cosmosHelper = new CosmosDBHelper();
        }

        private void BtnTestSource(object sender, RoutedEventArgs e)
        {
            TestSourceConnection();
        }

        public bool TestSourceConnection()
        {
            ConnectionTestMsg.Text = "";
            CloneSettings.SourceSettings = new CosmosCollectionValues()
            {
                EndpointUrl = SourceURL.Text.ToString(),
                AccessKey = SourceKey.Text.ToString(),
                DatabaseName = SourceDB.Text.ToString(),
                CollectionName = SourceCollection.Text.ToString()
            };

            var result = cosmosHelper.TestSourceConnection();
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

        private void SaveButton_Click(object sender, RoutedEventArgs e) => new SaveFileDialog().SaveFile(MyDocuments, "Source", SourceSettings);

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = new OpenFileDialog().LoadFile<CosmosCollectionValues>(MyDocuments, "Source");

            if (settings != null)
            {
                SourceURL.Text = settings.EndpointUrl;
                SourceKey.Text = settings.AccessKey;
                SourceDB.Text = settings.DatabaseName;
                SourceCollection.Text = settings.CollectionName;
            }
        }
    }
}
