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
            _mainFrame.Navigate(GetPreviousPage(currentPage));
        }

        private void BtnClickNext(object sender, RoutedEventArgs e)
        {
            Page currentPage = (Page)_mainFrame.Content;
            if(PerformAction(currentPage))
            {
                _mainFrame.Navigate(GetNextPage(currentPage));
            }            
        }

        private void BtnClickFinish(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void NavigationHelper()
        {
            Page currentPage = (Page)_mainFrame.Content;
            int pagenum = GetPageNumber(currentPage);
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

        private Page GetNextPage(Page currentPage)
        {
            return pages[GetPageNumber(currentPage) + 1];
        }

        private Page GetPreviousPage(Page currentPage)
        {
            return pages[GetPageNumber(currentPage) - 1];
        }

        private int GetPageNumber(Page page)
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
        
        private bool PerformAction(Page currentPage)
        {
            if (GetPageNumber(currentPage) == 0)
            {
                var result = ((SourcePage)currentPage).TestSourceConnection();
                return result;
            }
            else if (GetPageNumber(currentPage) == 1)
            {
                var result = ((DestinationPage)currentPage).TestDestinationConnection();
                return result;
            }
            else if (GetPageNumber(currentPage) == 2)
            {
                CloneSettings.CopyStoredProcedures = ((CheckBox)currentPage.FindName("SPs")).IsChecked.Value;
                CloneSettings.CopyUDFs = ((CheckBox)currentPage.FindName("UDFs")).IsChecked.Value;
                CloneSettings.CopyTriggers = ((CheckBox)currentPage.FindName("CosmosTriggers")).IsChecked.Value;
                CloneSettings.CopyDocuments = ((CheckBox)currentPage.FindName("Documents")).IsChecked.Value;
                CloneSettings.CopyIndexingPolicy = ((CheckBox)currentPage.FindName("IPs")).IsChecked.Value;
                CloneSettings.CopyPartitionKey = ((CheckBox)currentPage.FindName("PKs")).IsChecked.Value;

                return true;                
            }
            else if (GetPageNumber(currentPage) == 3)
            {
                btn_finish.IsEnabled = false;
                scrubRules = ((DataAnonymizationPage)currentPage).getScrubRules();
                bool isValidationSuccess = ((DataAnonymizationPage)currentPage).validateInput();
                if (!isValidationSuccess) return false;

                BackgroundWorker worker = new BackgroundWorker
                {
                    WorkerReportsProgress = true
                };
                worker.DoWork += Worker_DoWork;
                worker.ProgressChanged += Worker_ProgressChanged;
                worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                worker.RunWorkerAsync(1000);

                BackgroundWorker worker2 = new BackgroundWorker
                {
                    WorkerReportsProgress = true
                };
                worker2.DoWork += Worker_DoWork2;
                worker2.ProgressChanged += Worker_ProgressChanged2;
                worker2.RunWorkerCompleted += Worker_RunWorkerCompleted2;
                worker2.RunWorkerAsync(1000);

                var nextPage = GetNextPage(currentPage);
                ((CopyCollectionPage)nextPage).setRequiredprogressBars(scrubRules);

                return true;
            }
            else if (GetPageNumber(currentPage) == 4)
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


        void Worker_DoWork(object sender, DoWorkEventArgs e)
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

        void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //((ProgressBar)pages[3].FindName("ReadProgress")).Value = e.ProgressPercentage;            
        }

        void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //MessageBox.Show("Document Collection Copied Successfully : ");
        }


        void Worker_DoWork2(object sender, DoWorkEventArgs e)
        {
            Task.Delay(3000).Wait();
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
                    else
                    {
                        readPercentProgress = 100;
                        writePercentProgress = 100;
                    }

                    if (CloneSettings.ScrubbingRequired && DocumentMigrator.scrubRules != null && DocumentMigrator.scrubRules.Count > 0)
                    {
                        scrubPercentProgress = DocumentMigrator.ScrubPercentProgress;
                      
                    }                    
                    else
                    {
                        if (DocumentMigrator.scrubRules == null || DocumentMigrator.scrubRules.Count == 0) scrubPercentProgress = 100;
                        else scrubPercentProgress = 0;
                    }

                }

                sendPercent = (int)scrubPercentProgress * 1000000 + (int)readPercentProgress * 1000 + (int)writePercentProgress ;
                (sender as BackgroundWorker).ReportProgress((int)sendPercent);
                Task.Delay(3000).Wait();
            }
        }
        void Worker_ProgressChanged2(object sender, ProgressChangedEventArgs e)
        {
            int receivePercent = e.ProgressPercentage;


            int writePercent = (receivePercent % 1000);
            int readPercent = (receivePercent % 1000000) / 1000;
            int scrubPercent = receivePercent / 1000000;

            ((ProgressBar)pages[4].FindName("ReadProgress")).Value = readPercent;
            ((ProgressBar)pages[4].FindName("WriteProgress")).Value = writePercent;
            ((ProgressBar)pages[4].FindName("ScrubProgress")).Value = scrubPercent;

            var statustextbox = ((TextBox)pages[4].FindName("StatusTextBlock"));
            statustextbox.Text = logger.FullLog;
            statustextbox.ScrollToEnd();
        }

        void Worker_RunWorkerCompleted2(object sender, RunWorkerCompletedEventArgs e)
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
