using BlangParser;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BlangEditor
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The collection view for the BlangFile object
        /// </summary>
        public ICollectionView BlangStringsView;

        /// <summary>
        /// Currently open Blang file
        /// </summary>
        public BlangFile CurrentBlangFile;

        /// <summary>
        /// Currently open Blang file path
        /// </summary>
        public string CurrentFilePath;

        /// <summary>
        /// Window title to append to (for file paths)
        /// </summary>
        public const string WindowTitle = "BlangEditor";

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Filter TextBox on text changed event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BlangStringsView == null)
            {
                return;
            }

            BlangStringsView.Refresh();
        }

        /// <summary>
        /// Open menu item on click event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void OpenItem_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = "All Files (.*)|*.*",
                FilterIndex = 1,
                Multiselect = false,
                Title = "Open a Blang file for viewing or editing"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (CurrentBlangFile != null)
                {
                    if (MessageBox.Show("Are you sure you want to close the current file to open a new one?",
                        "Make sure you save your changes",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                try
                {
                    CurrentBlangFile = BlangFile.Parse(openFileDialog.FileName);
                }
                catch (Exception)
                {
                    MessageBox.Show("Unsupported file format.",
                        "Error while opening file",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }

                var blangStringsViewSource = new CollectionViewSource()
                {
                    Source = CurrentBlangFile.Strings
                };

                BlangStringsView = blangStringsViewSource.View;

                BlangStringsView.Filter += (obj) =>
                {
                    if (obj == null)
                    {
                        return false;
                    }

                    if (string.IsNullOrEmpty(FilterText.Text) || string.IsNullOrWhiteSpace(FilterText.Text))
                    {
                        return true;
                    }

                    if (((BlangString)obj).Identifier.ToLowerInvariant().Contains(FilterText.Text.ToLowerInvariant()) ||
                        ((BlangString)obj).Text.ToLowerInvariant().Contains(FilterText.Text.ToLowerInvariant()))
                    {
                        return true;
                    }

                    return false;
                };

                BlangStringsDataGrid.ItemsSource = BlangStringsView;
                Title = $"{WindowTitle} - {openFileDialog.FileName}";
                CurrentFilePath = openFileDialog.FileName;
                SaveMenuItem.IsEnabled = true;
                SaveToMenuItem.IsEnabled = true;
                CloseMenuItem.IsEnabled = true;
            }
        }

        /// <summary>
        /// Close menu item on click event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void CloseItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to close the current file?",
                "Make sure you save your changes",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            BlangStringsDataGrid.ItemsSource = null;
            BlangStringsView = null;
            CurrentBlangFile = null;
            CurrentFilePath = string.Empty;
            SaveMenuItem.IsEnabled = false;
            SaveToMenuItem.IsEnabled = false;
            CloseMenuItem.IsEnabled = false;
            Title = WindowTitle;
        }

        /// <summary>
        /// Save menu item on click event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void SaveItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CurrentBlangFile.WriteTo(CurrentFilePath);
            }
            catch (Exception)
            {
                MessageBox.Show("An error occured while saving the file.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Save As menu item on click event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void SaveAsItem_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog()
            {
                Filter = "All Files (.*)|*.*",
                FilterIndex = 1,
                Title = "Save To ..."
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    CurrentBlangFile.WriteTo(saveFileDialog.FileName);
                    Title = $"{WindowTitle} - {saveFileDialog.FileName}";
                    CurrentFilePath = saveFileDialog.FileName;
                }
                catch (Exception)
                {
                    MessageBox.Show("An error occured while saving the file.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Exit menu item on click event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBlangFile != null)
            {
                if (MessageBox.Show("You have a file open, are you sure you want to exit?",
                    "Make sure you save your changes",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }
            }

            App.Current.Shutdown(0);
        }

        /// <summary>
        /// About menu item on click event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("BlangEditor v1.0\nby proteh.\n\nGithub: https://github.com/dcealopez/",
                "About BlangEditor",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
