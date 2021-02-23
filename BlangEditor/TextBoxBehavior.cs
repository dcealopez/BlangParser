using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BlangEditor
{
    /// <summary>
    /// Custom TextBox behavior to handle triple clicks to select all the text
    /// </summary>
    public static class TextBoxBehavior
    {
        public static readonly DependencyProperty TripleClickSelectAllProperty = DependencyProperty.RegisterAttached(
            "TripleClickSelectAll", typeof(bool), typeof(TextBoxBehavior), new PropertyMetadata(false, OnPropertyChanged));

        /// <summary>
        /// Assigns the preview left mouse button down to all the text boxes
        /// </summary>
        /// <param name="d">dependency object</param>
        /// <param name="e">event args</param>
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d != null)
            {
                var enable = (bool)e.NewValue;

                if (enable)
                {
                    if (d is TextBox)
                    {
                        (d as TextBox).PreviewMouseLeftButtonDown += OnTextBoxMouseDown;
                    }
                    else if(d is DataGridCell)
                    {
                        (d as DataGridCell).PreviewMouseLeftButtonDown += OnTextBoxMouseDown;
                    }
                }
                else
                {
                    if (d is TextBox)
                    {
                        (d as TextBox).PreviewMouseLeftButtonDown -= OnTextBoxMouseDown;
                    }
                    else if (d is DataGridCell)
                    {
                        (d as DataGridCell).PreviewMouseLeftButtonDown -= OnTextBoxMouseDown;
                    }
                }
            }
        }

        /// <summary>
        /// Triple click behavior
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private static void OnTextBoxMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 3)
            {
                if (sender is TextBox)
                {
                    (sender as TextBox).SelectAll();
                }
                else if (sender is DataGridCell)
                {
                    ((sender as DataGridCell).Content as TextBox).SelectAll();
                }
            }
        }

        /// <summary>
        /// Enables or disables the triple click behavior
        /// </summary>
        /// <param name="element">dependency object</param>
        /// <param name="value">enable or disable</param>
        public static void SetTripleClickSelectAll(DependencyObject element, bool value)
        {
            element.SetValue(TripleClickSelectAllProperty, value);
        }

        /// <summary>
        /// Gets the TripleClickSelectAll property
        /// </summary>
        /// <param name="element">dependency object</param>
        /// <returns>whether or not the triple click behavior is enabled</returns>
        public static bool GetTripleClickSelectAll(DependencyObject element)
        {
            return (bool)element.GetValue(TripleClickSelectAllProperty);
        }
    }
}
