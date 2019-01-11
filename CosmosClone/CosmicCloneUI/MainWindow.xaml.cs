// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CosmicCloneUI.Models;
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
using System.ComponentModel;
using CosmosCloneCommon.Migrator;
using CosmosCloneCommon.Model;
using logger = CosmosCloneCommon.Utility.CloneLogger;

namespace CosmicCloneUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Page sourcePage;
        Page destinationPage;
        Page cloneOptionsPage;
        Page dataAnonymizationPage;
        Page copyCollectionPage;
        Page[] pages;

        CosmosDBHelper cosmosHelper;
        List<ScrubRule> scrubRules;

        public MainWindow()
        {
            InitializeComponent();
            InitializePages();
            cosmosHelper = new CosmosDBHelper();
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMessage = string.Format("An unhandled exception occurred: {0}", e.Exception.Message);
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
        private void InitializePages()
        {
            sourcePage = new SourcePage();
            destinationPage = new DestinationPage();
            cloneOptionsPage = new CloneOptionsPage();
            dataAnonymizationPage = new DataAnonymizationPage();
            copyCollectionPage = new CopyCollectionPage();
            //copyCollectionPage.SetStatus();

            pages = new Page[10];
            pages[0] = sourcePage;
            pages[1] = destinationPage;
            pages[2] = cloneOptionsPage;
            pages[3] = dataAnonymizationPage;
            pages[4] = copyCollectionPage;

            _mainFrame.Content = pages[0]; 
        }

        private void BtnClickPrevious(object sender, RoutedEventArgs e)
        {
            Page currentPage = (Page)_mainFrame.Content;
            _mainFrame.Navigate(getPreviousPage(currentPage));
        }

        private void BtnClickNext(object sender, RoutedEventArgs e)
        {
            Page currentPage = (Page)_mainFrame.Content;
            if(performAction(currentPage))
            {
                _mainFrame.Navigate(getNextPage(currentPage));
            }            
        }

        private void BtnClickFinish(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void NavigationHelper()
        {
            Page currentPage = (Page)_mainFrame.Content;
            int pagenum = getPageNumber(currentPage);
            if (pagenum == 0)
            {
                btn_previous.IsEnabled = false;
                btn_next.IsEnabled = true;
                btn_finish.IsEnabled = false;
            }
            else if (pagenum == 4)
            {
                btn_previous.IsEnabled = false;
                btn_next.IsEnabled = false;
                btn_finish.IsEnabled = false;//is this required
            }
            else
            {
                btn_previous.IsEnabled = true;
                btn_next.IsEnabled = true;
                btn_finish.IsEnabled = false;
            }
        }

        private Page getNextPage(Page currentPage)
        {
            return pages[getPageNumber(currentPage) + 1];
        }

        private Page getPreviousPage(Page currentPage)
        {
            return pages[getPageNumber(currentPage) - 1];
        }

        private int getPageNumber(Page page)
        {
            for(int i=0;i<pages.Length;i++)
            {
                if(pages[i] == page)
                {
                    return i;
                }
            }
            return 0;
        }

        private void MainFrameLoaded(object sender, EventArgs e)
        {
            NavigationHelper();
        }
        
        private bool performAction(Page currentPage)
        {
            if (getPageNumber(currentPage) == 0)
            {
                CloneSettings.SourceSettings = new CosmosCollectionValues()
                {
                    EndpointUrl = ((TextBox)currentPage.FindName("SourceURL")).Text.ToString(),
                    AccessKey = ((TextBox)currentPage.FindName("SourceKey")).Text.ToString(),
                    DatabaseName = ((TextBox)currentPage.FindName("SourceDB")).Text.ToString(),
                    CollectionName = ((TextBox)currentPage.FindName("SourceCollection")).Text.ToString()
                    //OfferThroughputRUs = int.Parse(sourceConfigs["OfferThroughputRUs"])
                };

                var result = cosmosHelper.TestSourceConnection_v2();
                if (result.IsSuccess == true)
                    return true;
                else
                    return false;

            }
            else if (getPageNumber(currentPage) == 1)
            {
                CloneSettings.TargetSettings = new CosmosCollectionValues()
                {
                    EndpointUrl = ((TextBox)currentPage.FindName("TargetURL")).Text,
                    AccessKey = ((TextBox)currentPage.FindName("TargetKey")).Text,
                    DatabaseName = ((TextBox)currentPage.FindName("TargetDB")).Text,
                    CollectionName = ((TextBox)currentPage.FindName("TargetCollection")).Text
                    //OfferThroughputRUs = int.Parse(sourceConfigs["OfferThroughputRUs"])
                };

                var result = cosmosHelper.TestTargetConnection_v2();
                if (result.IsSuccess == true)
                    return true;
                else
                    return false;

            }
            else if (getPageNumber(currentPage) == 2)
            {
                CloneSettings.CopyStoredProcedures = ((CheckBox)currentPage.FindName("SPs")).IsChecked.Value;
                CloneSettings.CopyUDFs = ((CheckBox)currentPage.FindName("UDFs")).IsChecked.Value;
                CloneSettings.CopyTriggers = ((CheckBox)currentPage.FindName("Triggers")).IsChecked.Value;
                CloneSettings.CopyDocuments = ((CheckBox)currentPage.FindName("Documents")).IsChecked.Value;
                CloneSettings.CopyIndexingPolicy = ((CheckBox)currentPage.FindName("IPs")).IsChecked.Value;
                CloneSettings.CopyPartitionKey = ((CheckBox)currentPage.FindName("PKs")).IsChecked.Value;

                return true;                
            }
            else if (getPageNumber(currentPage) == 3)
            {
                btn_finish.IsEnabled = false;
                scrubRules = ((DataAnonymizationPage)currentPage).getScrubRules();
                bool isValidationSuccess = ((DataAnonymizationPage)currentPage).validateInput();
                if (!isValidationSuccess) return false;
                               
                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.DoWork += worker_DoWork;
                worker.ProgressChanged += worker_ProgressChanged;
                worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                worker.RunWorkerAsync(1000);

                BackgroundWorker worker2 = new BackgroundWorker();
                worker2.WorkerReportsProgress = true;
                worker2.DoWork += worker_DoWork2;
                worker2.ProgressChanged += worker_ProgressChanged2;
                worker2.RunWorkerCompleted += worker_RunWorkerCompleted2;
                worker2.RunWorkerAsync(1000);

                var nextPage = getNextPage(currentPage);
                ((CopyCollectionPage)nextPage).setRequiredprogressBars(scrubRules);

                return true;
            }
            else if (getPageNumber(currentPage) == 4)
            {
                btn_finish.IsEnabled = false;
                ((CopyCollectionPage)currentPage).setRequiredprogressBars(scrubRules);
                return true;
            }
            else
            {
                return true;
            }
        }


        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var documentMigrator = new CosmosCloneCommon.Migrator.DocumentMigrator();
                documentMigrator.StartCopy(scrubRules).Wait();
            }
            catch(Exception ex)
            {
                logger.LogInfo("Main process exits with error");
                logger.LogError(ex);
                string excmessage = "Main process exits with error. /n" + ex.Message + "/n";
                if(ex.InnerException!=null)
                {
                    excmessage += ex.InnerException.Message;
                }
                MessageBox.Show(excmessage, "Error occurred. App closure", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //((ProgressBar)pages[3].FindName("ReadProgress")).Value = e.ProgressPercentage;            
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //MessageBox.Show("Document Collection Copied Successfully : ");
        }


        void worker_DoWork2(object sender, DoWorkEventArgs e)
        {
            long readPercentProgress = 0;
            long writePercentProgress = 0;
            long scrubPercentProgress = 0;
            int sendPercent = 0;
            while (readPercentProgress < 100 || writePercentProgress < 100 || scrubPercentProgress < 100)
            {
                if (DocumentMigrator.TotalRecordsInSource == 0 && CloneSettings.CopyDocuments==true)
                {
                    readPercentProgress = 0;
                    writePercentProgress = 0;
                   //scrubPercentProgress = 0;
                }
                else
                {
                    if(CloneSettings.CopyDocuments)
                    {
                        readPercentProgress = (DocumentMigrator.TotalRecordsRetrieved * 100) / DocumentMigrator.TotalRecordsInSource;
                        writePercentProgress = (DocumentMigrator.TotalRecordsSent * 100) / DocumentMigrator.TotalRecordsInSource;
                    }
                    
                    if(CloneSettings.ScrubbingRequired && DocumentMigrator.scrubRules!=null && DocumentMigrator.scrubRules.Count>0)
                    {
                        scrubPercentProgress = DocumentMigrator.ScrubPercentProgress;
                    }
                    else scrubPercentProgress = 100;

                }

                sendPercent = (int)scrubPercentProgress * 1000000 + (int)readPercentProgress * 1000 + (int)writePercentProgress ;
                (sender as BackgroundWorker).ReportProgress((int)sendPercent);
                Task.Delay(3000).Wait();
            }
        }
        void worker_ProgressChanged2(object sender, ProgressChangedEventArgs e)
        {
            int receivePercent = e.ProgressPercentage;

            
            int writePercent = (receivePercent % 1000);
            int readPercent = (receivePercent % 1000000) / 1000;
            int scrubPercent = receivePercent/1000000;

            ((ProgressBar)pages[4].FindName("ReadProgress")).Value = readPercent;
            ((ProgressBar)pages[4].FindName("WriteProgress")).Value = writePercent;
            ((ProgressBar)pages[4].FindName("ScrubProgress")).Value = scrubPercent;
            var statustextbox = ((TextBox)pages[4].FindName("StatusTextBlock"));
            statustextbox.Text = logger.FullLog;
            statustextbox.ScrollToEnd();
        }

        void worker_RunWorkerCompleted2(object sender, RunWorkerCompletedEventArgs e)
        {
            while(!DocumentMigrator.IsCodeMigrationComplete)
            {
                Task.Delay(5000).Wait();
            }            
            if(DocumentMigrator.IsCodeMigrationComplete)
            {
                string completeMessage = DocumentMigrator.TotalRecordsSent + " Documents Copied Successfully";
                completeMessage += "\n" + "Code Migration Complete";
                if (DocumentMigrator.scrubRules!=null && DocumentMigrator.scrubRules.Count>0)
                {
                    completeMessage += "\n" + "Scrubbing completed for rules " + DocumentMigrator.scrubRules.Count;
                }
                
                MessageBox.Show(completeMessage, "Completed", MessageBoxButton.OK, MessageBoxImage.Information);

                btn_finish.IsEnabled = true;
            }
        }

    }
}
