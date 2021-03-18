using BlangParser;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

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
        /// Track unsaved changes to the file
        /// </summary>
        public bool UnsavedChanges;

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
        /// OnKeyDown handler for the string's text box, to allow
        /// entering new lines by pressing Shift+Enter
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (Key.Return == e.Key && 0 < (ModifierKeys.Shift & e.KeyboardDevice.Modifiers))
            {
                var currentCaretIndex = (sender as TextBox).CaretIndex;

                (sender as TextBox).Text = (sender as TextBox).Text.Insert(currentCaretIndex, "\n");
                (sender as TextBox).CaretIndex = currentCaretIndex + 1;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Open menu item on click event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void OpenItem_Click(object sender, ExecutedRoutedEventArgs e)
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

                // Create a backup of this file first
                try
                {
                    File.Copy(openFileDialog.FileName, openFileDialog.FileName + ".bak", true);
                }
                catch (Exception)
                {
                    MessageBox.Show("IO Error.",
                        "An error occured while creating the backup file.",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
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
                BlangStringsDataGrid.CellEditEnding += CellEditEnding;
                Title = $"{WindowTitle} - {openFileDialog.FileName}";
                CurrentFilePath = openFileDialog.FileName;
                SaveMenuItem.IsEnabled = true;
                SaveToMenuItem.IsEnabled = true;
                CloseMenuItem.IsEnabled = true;
            }
        }

        private void CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (UnsavedChanges)
            {
                return;
            }

            var blangString = (e.EditingElement.DataContext as BlangString);
            var editingTextBox = e.EditingElement as TextBox;
            var newValue = editingTextBox.Text;

            if (blangString.Text != null && blangString.Text.Equals(newValue))
            {
                return;
            }

            Title = $"*{Title}";
            UnsavedChanges = true;
        }

        /// <summary>
        /// Close menu item on click event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void CloseItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            if (UnsavedChanges)
            {
                if (MessageBox.Show("There are unsaved changes, are you sure you want to close the current file?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }
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
        private void SaveItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            if (!ValidateIdentifierNames())
            {
                MessageBox.Show("There are entries with empty identifier names. Please set an identifier name for these entries or delete them.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            try
            {
                CurrentBlangFile.WriteTo(CurrentFilePath);
                Title = $"{WindowTitle} - {CurrentFilePath}";
                UnsavedChanges = false;
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
        private void SaveAsItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            if (!ValidateIdentifierNames())
            {
                MessageBox.Show("There are entries with empty identifier names. Please set an identifier name for these entries or delete them.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

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
                    UnsavedChanges = false;
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
        private void ExitItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            if (CurrentBlangFile != null && UnsavedChanges)
            {
                if (MessageBox.Show("There are unsaved changes, are you sure you want to exit?",
                    "Exit",
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
            MessageBox.Show("BlangEditor v1.2\nby proteh.\n\nGithub: https://github.com/dcealopez/",
                "About BlangEditor",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Handle the key down event on the DataGrid to customize
        /// the Enter key actions
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void BlangStringsDataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            var originalSourceUIElement = e.OriginalSource as UIElement;

            // Escape key handling
            if (dataGrid != null && dataGrid.SelectedIndex >= 0 && e.Key == Key.Escape)
            {
                dataGrid.CancelEdit();
                dataGrid.CommitEdit(DataGridEditingUnit.Row, false);
                (dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.SelectedIndex) as DataGridRow).Focus();
                e.Handled = true;
                return;
            }

            // Enter key handling
            if (dataGrid != null && dataGrid.SelectedIndex >= 0 && e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                if (!(dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.SelectedIndex) as DataGridRow).IsEditing)
                {
                    dataGrid.BeginEdit();
                    e.Handled = true;
                    return;
                }
                else
                {
                    if ((dataGrid.SelectedIndex + 1) == dataGrid.Items.Count - 1)
                    {
                        dataGrid.CommitEdit(DataGridEditingUnit.Row, false);
                    }
                    else
                    {
                        originalSourceUIElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                        dataGrid.CommitEdit();
                        (dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.SelectedIndex++) as DataGridRow).Focus();
                    }

                    e.Handled = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Window closing event handler
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (CurrentBlangFile != null && UnsavedChanges)
            {
                if (MessageBox.Show("There are unsaved changes, are you sure you want to exit?",
                    "Exit",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            App.Current.Shutdown(0);
        }

        /// <summary>
        /// Checks for null or empty identifier names
        /// </summary>
        /// <returns></returns>
        public bool ValidateIdentifierNames()
        {
            foreach (var blangString in CurrentBlangFile.Strings)
            {
                if (string.IsNullOrEmpty(blangString.Identifier) || string.IsNullOrWhiteSpace(blangString.Identifier))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
