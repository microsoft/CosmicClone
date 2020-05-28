// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CosmicCloneUI.Models;
using CosmosCloneCommon.Model;
using CosmosCloneCommon.Utility;
using System;
using System.IO;
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
using Microsoft.Win32;
using System.Xml.Serialization;
using System.Globalization;

namespace CosmicCloneUI
{
    /// <summary>
    /// Interaction logic for DataAnonymizationPage.xaml
    /// </summary>
    public partial class DataAnonymizationPage : Page
    {
        int RuleIndex;

        public DataAnonymizationPage()
        {
            InitializeComponent();
            SaveRuleButton.IsEnabled = false;
            RuleIndex = 1;
        }

        private bool Validation()
        {
            return true;
        }

        private void BtnAddScrubRule(object sender, RoutedEventArgs e)
        {
            CreateScrubRule();
        }

        private void CreateScrubRule(ScrubRule scrubRule = null)
        {

            WrapPanel parentStackPanelLeft = (WrapPanel) this.FindName("WrapPanel");

            int newRuleIndex = newIndex(scrubRule);
            AddRuleWithData(parentStackPanelLeft, newRuleIndex, scrubRule);
        }

        int newIndex(ScrubRule scrubRule = null)
        {
            int newRuleIndex = 0;
            if(scrubRule == null)
            {
                newRuleIndex = RuleIndex++;
            }
            else
            {
                if (scrubRule.RuleId == 0)
                {
                    newRuleIndex = RuleIndex++;
                }
                else newRuleIndex = scrubRule.RuleId;
            }
            return newRuleIndex;
        }
        void AddRuleWithData(WrapPanel parentStackPanel, int ruleIndex, ScrubRule scrubRule = null)
        {
            StackPanel RuleHeadersp = new StackPanel();
            RuleHeadersp.Orientation = Orientation.Horizontal;

            TextBlock HeaderTB = new TextBlock();
            HeaderTB.Text = "Rule " + (ruleIndex);

            Label RuleIdLabel = new Label();
            RuleIdLabel.Content = ruleIndex;
            RuleIdLabel.Visibility = Visibility.Hidden;

            Button DeleteBtn = new Button();
            DeleteBtn.Name = "btnDelete_" + ruleIndex;
            DeleteBtn.HorizontalAlignment = HorizontalAlignment.Right;
            //DeleteBtn.Content = "Delete";
            DeleteBtn.Margin = new Thickness(265, 0, 0, 0);
            DeleteBtn.Click += DeleteBtn_Click;
            DeleteBtn.Content = new Image
            {
                Source = new BitmapImage(new Uri("/Images/closeIcon.png", UriKind.Relative)),
                VerticalAlignment = VerticalAlignment.Center
            };

            RuleHeadersp.Children.Add(HeaderTB);
            RuleHeadersp.Children.Add(DeleteBtn);

            Expander exp = new Expander();
            exp.Header = RuleHeadersp;
            exp.HorizontalAlignment = HorizontalAlignment.Left;
            exp.Margin = new Thickness(20, 30, 0, 0);
            exp.BorderThickness = new Thickness(1, 1, 1, 1);
            exp.Width = 350;
            exp.BorderBrush = Brushes.Gray;
            exp.Background = Brushes.White;
            exp.IsExpanded = true;
            exp.Name = "RuleExpander_" + ruleIndex;

            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Vertical;
            sp.Margin = new Thickness(20, 5, 0, 0);

            StackPanel filterSP = new StackPanel();
            filterSP.Orientation = Orientation.Horizontal;
            filterSP.Margin = new Thickness(0, 5, 0, 2);

            StackPanel attributeSP = new StackPanel();
            attributeSP.Orientation = Orientation.Horizontal;
            attributeSP.Margin = new Thickness(0, 5, 0, 2);

            StackPanel scrubTypeSP = new StackPanel();
            scrubTypeSP.Orientation = Orientation.Horizontal;
            scrubTypeSP.Margin = new Thickness(0, 5, 0, 2);

            StackPanel findValueSP = new StackPanel();
            findValueSP.Orientation = Orientation.Horizontal;
            findValueSP.Margin = new Thickness(0, 5, 0, 2);

            StackPanel scrubValueSP = new StackPanel();
            scrubValueSP.Orientation = Orientation.Horizontal;
            scrubValueSP.Margin = new Thickness(0, 5, 0, 2);

            TextBlock FilterLabel = new TextBlock();
            FilterLabel.VerticalAlignment = VerticalAlignment.Center;
            FilterLabel.Margin = new Thickness(10, 0, 0, 0);
            //FilterLabel.MaxWidth = 150;

            Run runFilterLabel = new Run();
            runFilterLabel.Text = "Filter Query";
            runFilterLabel.FontSize = 15;

            Run runFilterLabelHint = new Run();
            runFilterLabelHint.Text = " \nEx: c.Type = \"document\"";
            runFilterLabelHint.FontSize = 10;

            FilterLabel.Inlines.Add(runFilterLabel);
            FilterLabel.Inlines.Add(runFilterLabelHint);

            TextBox FilterTB = new TextBox();
            FilterTB.Name = "Filter" + ruleIndex;
            FilterTB.Width = 150;
            FilterTB.HorizontalContentAlignment = HorizontalAlignment.Left;
            FilterTB.VerticalAlignment = VerticalAlignment.Center;
            FilterTB.Margin = new Thickness(20, 0, 0, 0);

            TextBlock AttributeScrubLabel = new TextBlock();
            AttributeScrubLabel.VerticalAlignment = VerticalAlignment.Center;
            AttributeScrubLabel.Margin = new Thickness(10, 0, 0, 0);
            //AttributeScrubLabel.Width = 120;

            Run runAttributeScrubLabel = new Run();
            runAttributeScrubLabel.Text = "Attribute To Scrub";
            runAttributeScrubLabel.FontSize = 15;

            Run runAttributeScrubLabelHint = new Run();
            runAttributeScrubLabelHint.Text = " \nEx: c.Person.Email";
            runAttributeScrubLabelHint.FontSize = 10;

            AttributeScrubLabel.Inlines.Add(runAttributeScrubLabel);
            AttributeScrubLabel.Inlines.Add(runAttributeScrubLabelHint);

            TextBox AttributeScrubTB = new TextBox();
            AttributeScrubTB.Name = "ScrubAttribute" + ruleIndex;
            AttributeScrubTB.Width = 150;
            AttributeScrubTB.HorizontalContentAlignment = HorizontalAlignment.Left;
            AttributeScrubTB.VerticalAlignment = VerticalAlignment.Center;
            AttributeScrubTB.Margin = new Thickness(20, 0, 0, 0);

            TextBlock ScrubTypeLabel = new TextBlock();
            ScrubTypeLabel.Text = "Scrub Type";
            ScrubTypeLabel.FontSize = 15;
            ScrubTypeLabel.VerticalAlignment = VerticalAlignment.Center;
            ScrubTypeLabel.Margin = new Thickness(10, 0, 0, 0);

            ComboBox ScrubTypeCB = new ComboBox();
            ScrubTypeCB.Name = "ScrubType" + ruleIndex;
            ScrubTypeCB.Width = 150;
            ScrubTypeCB.Margin = new Thickness(20, 0, 0, 0);

            foreach (RuleType val in Enum.GetValues(typeof(RuleType)))
            {
                ScrubTypeCB.Items.Add(val.ToString());
            }

            ScrubTypeCB.SelectionChanged += new SelectionChangedEventHandler(scrubTypeComboBox_SelectedIndexChanged);

            TextBlock FindValueLabel = new TextBlock();
            FindValueLabel.Text = "Find";
            FindValueLabel.FontSize = 15;
            FindValueLabel.VerticalAlignment = VerticalAlignment.Center;
            FindValueLabel.Margin = new Thickness(10, 0, 0, 5);

            TextBox FindValueTB = new TextBox();
            FindValueTB.Name = "FindValue" + ruleIndex;
            FindValueTB.Width = 150;
            FindValueTB.HorizontalContentAlignment = HorizontalAlignment.Left;
            FindValueTB.VerticalAlignment = VerticalAlignment.Center;
            FindValueTB.Margin = new Thickness(20, 0, 0, 0);

            TextBlock ScrubValueLabel = new TextBlock();
            ScrubValueLabel.Text = "Replace with";
            ScrubValueLabel.FontSize = 15;
            ScrubValueLabel.VerticalAlignment = VerticalAlignment.Center;
            ScrubValueLabel.Margin = new Thickness(10, 0, 0, 5);

            TextBox ScrubValueTB = new TextBox();
            ScrubValueTB.Name = "ScrubValue" + ruleIndex;
            ScrubValueTB.Width = 150;
            ScrubValueTB.HorizontalContentAlignment = HorizontalAlignment.Left;
            ScrubValueTB.VerticalAlignment = VerticalAlignment.Center;
            ScrubValueTB.Margin = new Thickness(20, 0, 0, 0);

            FilterLabel.Width = 120;
            FilterTB.Width = 150;
            AttributeScrubLabel.Width = 120;
            AttributeScrubTB.Width = 150;
            ScrubTypeLabel.Width = 120;
            ScrubTypeCB.Width = 150;
            ScrubValueLabel.Width = 120;
            FindValueTB.Width = 150;
            FindValueLabel.Width = 120;

            filterSP.Children.Add(FilterLabel);
            filterSP.Children.Add(FilterTB);

            attributeSP.Children.Add(AttributeScrubLabel);
            attributeSP.Children.Add(AttributeScrubTB);

            scrubTypeSP.Children.Add(ScrubTypeLabel);
            scrubTypeSP.Children.Add(ScrubTypeCB);

            findValueSP.Children.Add(FindValueLabel);
            findValueSP.Children.Add(FindValueTB);
            findValueSP.Visibility = Visibility.Collapsed;

            scrubValueSP.Children.Add(ScrubValueLabel);
            scrubValueSP.Children.Add(ScrubValueTB);
            scrubValueSP.Children.Add(RuleIdLabel);
            scrubValueSP.Visibility = Visibility.Hidden;

            sp.Children.Add(attributeSP);
            sp.Children.Add(filterSP);
            sp.Children.Add(scrubTypeSP);
            sp.Children.Add(findValueSP);
            sp.Children.Add(scrubValueSP);

            exp.Content = sp;

            if(scrubRule!=null)
            {
                FilterTB.Text = scrubRule.FilterCondition;
                AttributeScrubTB.Text = scrubRule.PropertyName;
                if(scrubRule.Type!=null) ScrubTypeCB.SelectedIndex = (int)scrubRule.Type;
                ScrubValueTB.Text = scrubRule.UpdateValue;
                FindValueTB.Text = scrubRule.FindValue;
            }
            parentStackPanel.Children.Add(exp);
            if(!SaveRuleButton.IsEnabled)
            {
                SaveRuleButton.IsEnabled = true;
            }
        }
      

        private void scrubTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //check comobox status based on it change visibility of scrubvalue stack panel
            var cbox = (ComboBox)sender;
            var scrubType = cbox.SelectedValue.ToString();
            if (scrubType == RuleType.SingleValue.ToString() || scrubType == RuleType.PartialMaskFromLeft.ToString() || scrubType == RuleType.PartialMaskFromRight.ToString())
            {
                var parentPanel = (StackPanel)cbox.Parent;
                var gpPanel = (StackPanel)parentPanel.Parent;
                gpPanel.Children[3].Visibility = Visibility.Collapsed;
                gpPanel.Children[4].Visibility = Visibility.Visible;
            }
            else if (scrubType == RuleType.FindAndReplace.ToString())
            {
                var parentPanel = (StackPanel)cbox.Parent;
                var gpPanel = (StackPanel)parentPanel.Parent;

                gpPanel.Children[3].Visibility = Visibility.Visible;
                gpPanel.Children[4].Visibility = Visibility.Visible;
            }
            else
            {
                var parentPanel = (StackPanel)cbox.Parent;
                var gpPanel = (StackPanel)parentPanel.Parent;
                gpPanel.Children[3].Visibility = Visibility.Collapsed;
                gpPanel.Children[4].Visibility = Visibility.Hidden;
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var btnDelete = (Button)sender;
            string expname = "RuleExpander_" + btnDelete.Name.Substring(btnDelete.Name.IndexOf('_')+1);

            WrapPanel wrapPanel = (WrapPanel)this.FindName("WrapPanel");
            foreach (UIElement SPUI in wrapPanel.Children)
            {
                Expander exp = (Expander)SPUI;
                if (exp.Name == expname)
                {
                    wrapPanel.Children.Remove(SPUI);
                    break;
                }
            }

            var rules = getScrubRules();
            //Delete all scrub rules
            wrapPanel.Children.RemoveRange(0, wrapPanel.Children.Count);

            //Re initialize rule index
            this.RuleIndex = 1;

            foreach (var rule in rules)
            {
                rule.RuleId = 0;//reset Ids so they are re assigned
                CreateScrubRule(rule);
            }           
        }

        public List<ScrubRule> getScrubRules()
        {
            //List<ScrubRule> sb = new List<ScrubRule>();
            TextBox filterCondition = (TextBox) this.FindName("FilterCondition");
            //sb.filterQuery = filterCondition.Text;
            List<ScrubRule> srList = new List<ScrubRule>();

            WrapPanel wrapPanel = (WrapPanel) this.FindName("WrapPanel");
            foreach (UIElement SPUI in wrapPanel.Children)
            {
                Expander exp = (Expander) SPUI;
                StackPanel lrsp = (StackPanel) exp.Content;
                UIElementCollection uiElementsSP = lrsp.Children;

                ScrubRule sr = new ScrubRule();

                foreach (UIElement uiElementSP in uiElementsSP)
                {
                    StackPanel tempSP = (StackPanel) uiElementSP;
                    UIElementCollection uiElements = tempSP.Children;

                    foreach (UIElement uiElement in uiElements)
                    {
                        if (uiElement.GetType().Name == "Label")
                        {
                            var ruleIdLabel = (Label) uiElement;
                            int ruleId;
                            if (int.TryParse(ruleIdLabel.Content.ToString(), out ruleId))
                            {
                                sr.RuleId = ruleId;
                            }
                            else sr.RuleId = 0;
                        }

                        if (uiElement.GetType().Name == "TextBox")
                        {
                            TextBox tb = (TextBox) uiElement;

                            if (tb.Name.StartsWith("Filter"))
                            {
                                sr.FilterCondition = tb.Text.Trim();
                            }
                            else if (tb.Name.StartsWith("ScrubAttribute"))
                            {
                                sr.PropertyName = tb.Text.Trim();
                            }
                            else if (tb.Name.StartsWith("ScrubValue"))
                            {
                                sr.UpdateValue = tb.Text.Trim();
                            }
                            else if (tb.Name.StartsWith("FindValue"))
                            {
                                sr.FindValue = tb.Text.Trim();
                            }
                        }

                        if (uiElement.GetType().Name == "ComboBox")
                        {
                            ComboBox cb = (ComboBox) uiElement;
                            if (cb.Name.StartsWith("ScrubType"))
                            {
                                //sr.Type = (RuleType) Enum.Parse(typeof(RuleType), cb.Text);
                                RuleType rType;

                                if (Enum.TryParse<RuleType>(cb.Text, out rType))
                                {
                                    sr.Type = rType;
                                }
                                else
                                {
                                    sr.Type = null;
                                }
                            }
                        }
                    }

                }

                srList.Add(sr);
            }

            return srList;
        }

        public bool validateInput()
        {
            var rules = getScrubRules();
            var orderedRules = rules.OrderBy(o => o.RuleId).ToList();
            bool isValidationSuccess;
            var validationMessages = new List<string>();

            foreach (var rule in orderedRules)
            {
                if(rule.Type == null)
                {
                    validationMessages.Add($"Rule:{rule.RuleId} - Please select a valid anonymization type");
                }
                if(string.IsNullOrEmpty(rule.PropertyName))
                {
                    validationMessages.Add($"Rule:{rule.RuleId} - Attribute name is empty");
                }
                else if (!rule.PropertyName.StartsWith("c."))
                {
                    validationMessages.Add($"Rule:{rule.RuleId} - Attribute name starts improperly. Sample c.EntityType");
                }
                else if (rule.PropertyName.EndsWith("."))
                {
                    validationMessages.Add($"Rule:{rule.RuleId} - Attribute name Ends improperly. Sample c.EntityType");
                }
                if (!string.IsNullOrEmpty(rule.FilterCondition))
                {
                    if (!rule.FilterCondition.StartsWith("c."))
                    {
                        validationMessages.Add($"Rule:{rule.RuleId} - Filter condition starts improperly. Sample c.EntityType=\"document\" ");
                    }
                }
                if (rule.Type == RuleType.FindAndReplace && string.IsNullOrEmpty(rule.FindValue))
                {
                    validationMessages.Add($"Rule:{rule.RuleId} - Find is empty");
                }
            }
            if(validationMessages.Count > 0)
            {
                isValidationSuccess = false;
                string message="";
                foreach(var msg in validationMessages)
                {
                    message += (msg + System.Environment.NewLine);
                }                
                MessageBox.Show(message, "Input validation",MessageBoxButton.OK,MessageBoxImage.Error);
            }
            else isValidationSuccess = true;
            return isValidationSuccess;
        }



        private void SaveRuleButton_Click(object sender, RoutedEventArgs e)
        {
            var rules = this.getScrubRules();

            if (rules == null || rules.Count == 0)
            {
                MessageBox.Show("No Rules found. Please add/load anonymization rules before Save", "No rules Found", MessageBoxButton.OK, MessageBoxImage.Warning);                
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.Filter = "XML file (*.xml)|*.xml";
            saveFileDialog.Title = "CosmicClone save AnonymizationRules";
            saveFileDialog.FileName = "AnonymizationRules_"+ DateTime.Now.ToString("MM-dd-yyyy-HHmmss",CultureInfo.InvariantCulture);            
            
            if (saveFileDialog.ShowDialog() == true)
            {                
                var xmlText = CloneSerializer.XMLSerialize(rules);
                File.WriteAllText(saveFileDialog.FileName, xmlText);
            }                
        }

        private void LoadRuleButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Filter = "XML file (*.xml)|*.xml";
            openFileDialog.Title = "CosmicClone Load AnonymizationRules";
            if (openFileDialog.ShowDialog() == true)
            {
                string xmlText = File.ReadAllText(openFileDialog.FileName);
                var rules = CloneSerializer.XMLDeserialize<List<ScrubRule>>(xmlText);
                if (rules == null && rules.Count == 0)
                {
                    MessageBox.Show("No rules to Load in file : "+openFileDialog.FileName , "No rules Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var orderedRules = rules.OrderBy(o => o.RuleId).ToList();
                
                //Delete all scrub rules
                WrapPanel wrapPanel = (WrapPanel)this.FindName("WrapPanel");
                wrapPanel.Children.Clear();

                //Re initialize rule index
                this.RuleIndex = 1;

                foreach (var rule in orderedRules)
                {
                    CreateScrubRule(rule);
                }
            }
            //txtEditor.Text = File.ReadAllText(openFileDialog.FileName);
            //handle unable to load exception
        }

        private void ValidateRuleButton_Click(object sender, RoutedEventArgs e)
        {
            var rules = getScrubRules();
            if (rules == null || rules.Count==0)
            {
                MessageBox.Show("No Rules found. Please add/load anonymization rules before Validation", "Data validation", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            bool isValidationSuccess = this.validateInput();
            if (isValidationSuccess)
            {
                MessageBox.Show("Rules Validated Successfully", "Data validation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
