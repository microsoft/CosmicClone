using CosmosCloneCommon.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            if (TestConnection())
            {
                var connectionIcon = (Image)this.FindName("ConnectionIcon");
                ConnectionIcon.Source = new BitmapImage(new Uri("/Images/success.png", UriKind.Relative));
            }
            else
            {
                var connectionIcon = (Image)this.FindName("ConnectionIcon");
                ConnectionIcon.Source = new BitmapImage(new Uri("/Images/fail.png", UriKind.Relative));
            }
        }

        private bool TestConnection()
        {
            CloneSettings.SourceSettings = new CosmosCollectionValues()
            {
                EndpointUrl = SourceURL.Text.ToString(),
                AccessKey = SourceKey.Text.ToString(),
                DatabaseName = SourceDB.Text.ToString(),
                CollectionName = SourceCollection.Text.ToString()
            };

            var result = cosmosHelper.TestSourceConnection_v2();
            if (result.IsSuccess == true)
                return true;
            else
                return false;
        }
    }
}
