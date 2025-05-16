using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using Alto_Coordinates_Viewer.MVVM.Model;
using ICSharpCode.AvalonEdit;

namespace Alto_Coordinates_Viewer.Services
{
    class SelectedStringChanged : Abstract.ViewBaseModel
    {
        /// <summary>
        /// Highlight the selected <String> in the text editor
        /// And update the color of the corresponding ALTO boxes
        /// </summary>
        public void AvalonTextSelectionChanged(TextEditor CodingTextControl, ObservableCollection<AltoModel> AltoCollection)
        {
            try
            {
                string selectedText = CodingTextControl.TextArea.Selection.GetText()?.Trim();

                Regex stringRegex = new Regex(@"<String ID=""(.*?)"" HPOS=""(.*?)"" VPOS=""(.*?)"" WIDTH=""(.*?)"" HEIGHT=""(.*?)"" WC=""(.*?)"" CONTENT=""(.*?)""", RegexOptions.Compiled);
                Match match = stringRegex.Match(selectedText);

                if (match.Success)
                {
                    // Extract coordinates of selected string
                    double selectedX = double.Parse(match.Groups[2].Value);
                    double selectedY = double.Parse(match.Groups[3].Value);
                    double selectedWidth = double.Parse(match.Groups[4].Value);
                    double selectedHeight = double.Parse(match.Groups[5].Value);

                    foreach (var word in AltoCollection)
                    {
                        // Checking whether its position and size match the coordinates of the <String>
                        // Floating-point comparison with a small tolerance (0.1)
                        if (Math.Abs(word.X - selectedX) < 0.1 &&
                            Math.Abs(word.Y - selectedY) < 0.1 &&
                            Math.Abs(word.Width - selectedWidth) < 0.1 &&
                            Math.Abs(word.Height - selectedHeight) < 0.1)
                        {
                            word.ColorBox = Brushes.Green;
                            word.OpacityBackground = 0.3;
                            word.BackgroundBoxColor = Brushes.Green;
                        }
                        else
                        {
                            word.ColorBox = Brushes.Red;
                            word.OpacityBackground = 100;
                            word.BackgroundBoxColor = Brushes.Transparent;
                        }
                    }
                }
                else
                {
                    // No <String> selected, reset all to Red
                    foreach (var word in AltoCollection)
                    {
                        word.ColorBox = Brushes.Red;
                        word.OpacityBackground = 100;
                        word.BackgroundBoxColor = Brushes.Transparent;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }
    }
}
