// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
using CosmosCloneCommon.Utility;
using logger = CosmosCloneCommon.Utility.CloneLogger;
using CosmosCloneCommon.Model;

namespace CosmicCloneUI
{
    /// <summary>
    /// Interaction logic for CopyCollectionPage.xaml
    /// </summary>
    public partial class CopyCollectionPage : Page
    {
        public CopyCollectionPage()
        {
            InitializeComponent();

        }

        public void SetStatus()
        {
            //StatusTextBlock.Text = CloneLogger.GetFullLog();
            
        }

        public void setRequiredprogressBars(List<ScrubRule> scrubRules)
        {
            if(CloneSettings.CopyDocuments==false)
            {
                CollectionReadStackPanel.Visibility = Visibility.Hidden;
                CollectionWriteStackPanel.Visibility = Visibility.Hidden;
            }
            if(CloneSettings.ScrubbingRequired)
            {
                if(scrubRules == null || scrubRules.Count<=0)
                {
                    ScrubStackPanel.Visibility = Visibility.Hidden;
                }
            }
        }

        private void ShowStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if(StatusTextBlock.Visibility == Visibility.Visible)
            {
                StatusTextBlock.Visibility = Visibility.Hidden;
                StatusTextBlockBorder.Visibility = Visibility.Hidden;
                ShowStatusButton.Content = "Show status";
            }
            else if(StatusTextBlock.Visibility == Visibility.Hidden)
            {
                StatusTextBlock.Visibility = Visibility.Visible;
                StatusTextBlockBorder.Visibility = Visibility.Visible;
                ShowStatusButton.Content = "Hide status";
            }
            
        }
    }
}
