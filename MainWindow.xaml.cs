using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace TextObscuringApp
{
    public partial class MainWindow : Window
    {
        private string originalText;
        private Dictionary<string, string> obscuredWords;

        public MainWindow()
        {
            InitializeComponent();
            AdjustRowHeights();
        }

        private void ObscureButton_Click(object sender, RoutedEventArgs e)
        {
            originalText = InputTextBox.Text;
            if (!double.TryParse(WordProportionTextBox.Text, out double wordProportion) ||
                !double.TryParse(WordPartProportionTextBox.Text, out double wordPartProportion) ||
                wordProportion < 0 || wordProportion > 100 || wordPartProportion < 0 || wordPartProportion > 100)
            {
                MessageBox.Show("Please enter valid percentages between 0 and 100.");
                return;
            }

            bool showFirstLetter = ShowFirstLetterCheckBox.IsChecked == true;
            obscuredWords = ObscureText(originalText, wordProportion / 100, wordPartProportion / 100, showFirstLetter);
            ObscuredTextBox.Text = GenerateObscuredText(originalText, obscuredWords);
        }

        private Dictionary<string, string> ObscureText(string text, double wordProportion, double wordPartProportion, bool showFirstLetter)
        {
            var words = text.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var random = new Random();
            int wordsToObscure = (int)(words.Length * wordProportion);
            var selectedWords = words.OrderBy(_ => random.Next()).Take(wordsToObscure).Distinct();
            var obscuredWords = new Dictionary<string, string>();

            foreach (var word in selectedWords)
            {
                int charsToObscure = (int)(word.Length * wordPartProportion);
                var chars = word.ToCharArray();
                if (showFirstLetter && charsToObscure >= word.Length)
                {
                    charsToObscure = word.Length - 1;
                }
                for (int i = 0; i < charsToObscure; i++)
                {
                    int index = showFirstLetter ? random.Next(1, word.Length) : random.Next(word.Length);
                    chars[index] = '_';
                }
                obscuredWords[word] = new string(chars);
            }

            return obscuredWords;
        }

        private string GenerateObscuredText(string text, Dictionary<string, string> obscuredWords)
        {
            var regex = new Regex(@"\b\w+\b");
            return regex.Replace(text, match =>
            {
                var word = match.Value;
                return obscuredWords.ContainsKey(word) ? obscuredWords[word] : word;
            });
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var userInput = ObscuredTextBox.Text;
            var (mistakes, markedText) = CalculateMistakesAndMarkText(originalText, userInput);

            ResultTextBlock.Text = $"You made {mistakes} mistakes.";
            MistakesTextBlock.Inlines.Clear();
            MistakesTextBlock.Inlines.AddRange(markedText);
        }

        private (int, List<Inline>) CalculateMistakesAndMarkText(string original, string userInput)
        {
            var originalWords = original.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var userWords = userInput.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int mistakes = 0;
            var inlines = new List<Inline>();

            int minLength = Math.Min(originalWords.Length, userWords.Length);
            for (int i = 0; i < minLength; i++)
            {
                if (!originalWords[i].Equals(userWords[i], StringComparison.OrdinalIgnoreCase))
                {
                    mistakes++;
                    var incorrectWord = new Run(userWords[i]) { Foreground = Brushes.Red, TextDecorations = TextDecorations.Strikethrough };
                    var correctWord = new Run($" ({originalWords[i]}) ") { Foreground = Brushes.Green };
                    inlines.Add(incorrectWord);
                    inlines.Add(correctWord);
                }
                else
                {
                    inlines.Add(new Run(userWords[i] + " "));
                }
            }

            if (originalWords.Length > minLength)
            {
                for (int i = minLength; i < originalWords.Length; i++)
                {
                    var correctWord = new Run(originalWords[i] + " ") { Foreground = Brushes.Green };
                    inlines.Add(correctWord);
                    mistakes++;
                }
            }

            if (userWords.Length > minLength)
            {
                for (int i = minLength; i < userWords.Length; i++)
                {
                    var incorrectWord = new Run(userWords[i]) { Foreground = Brushes.Red, TextDecorations = TextDecorations.Strikethrough };
                    inlines.Add(incorrectWord);
                    mistakes++;
                }
            }

            return (mistakes, inlines);
        }

        private void ToggleVisibilityButton_Click(object sender, RoutedEventArgs e)
        {
            if (InputTextBox.Visibility == Visibility.Visible)
            {
                InputTextBox.Visibility = Visibility.Collapsed;
                ToggleVisibilityButton.Content = "Show Original Text";
            }
            else
            {
                InputTextBox.Visibility = Visibility.Visible;
                ToggleVisibilityButton.Content = "Hide Original Text";
            }
            AdjustRowHeights();
        }

        private void AdjustRowHeights()
        {
            if (InputTextBox.Visibility == Visibility.Visible)
            {
                // Assuming InputTextBox is your control and myGrid is the parent Grid
                int rowIndex = (int)InputTextBox.GetValue(Grid.RowProperty); // Get the row index
                RowDefinition rowDefinition = myGrid.RowDefinitions[rowIndex]; // Access the corresponding row definition
                rowDefinition.Height = new GridLength(1, GridUnitType.Star); // Modify the height

                int rowIndex2 = (int)ObscuredTextBox.GetValue(Grid.RowProperty); // Get the row index
                RowDefinition rowDefinition2 = myGrid.RowDefinitions[rowIndex]; // Access the corresponding row definition
                rowDefinition.Height = new GridLength(1, GridUnitType.Star); // Modify the height

            }
            else
            {
                int rowIndex = (int)InputTextBox.GetValue(Grid.RowProperty); // Get the row index
                RowDefinition rowDefinition = myGrid.RowDefinitions[rowIndex]; // Access the corresponding row definition
                rowDefinition.Height = new GridLength(0); // Modify the height

                int rowIndex2 = (int)ObscuredTextBox.GetValue(Grid.RowProperty); // Get the row index
                RowDefinition rowDefinition2 = myGrid.RowDefinitions[rowIndex]; // Access the corresponding row definition
                rowDefinition.Height = new GridLength(1, GridUnitType.Star); // Modify the height
            }

            /*
             *             if (InputTextBox.Visibility == Visibility.Visible)
            {
                ((RowDefinition)InputTextBox.Parent.GetValue(Grid.RowDefinitionsProperty)).Height = new GridLength(1, GridUnitType.Star);
                ((RowDefinition)ObscuredTextBox.Parent.GetValue(Grid.RowDefinitionsProperty)).Height = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                ((RowDefinition)InputTextBox.Parent.GetValue(Grid.RowDefinitionsProperty)).Height = new GridLength(0);
                ((RowDefinition)ObscuredTextBox.Parent.GetValue(Grid.RowDefinitionsProperty)).Height = new GridLength(1, GridUnitType.Star);
            }
             */
        }
    }
}



