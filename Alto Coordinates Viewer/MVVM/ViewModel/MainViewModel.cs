using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using Alto_Coordinates_Viewer.MVVM.Model;
using Alto_Coordinates_Viewer.Services;
using DevExpress.Mvvm;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Utils;
namespace Alto_Coordinates_Viewer.MVVM.ViewModel
{
    class MainViewModel : Abstract.ViewBaseModel
    {
        public DelegateCommand<MouseWheelEventArgs> ZoomXmlTextViewerCommand { get; private set; }
        public DelegateCommand<MouseWheelEventArgs> ZoomImageViewerCommand { get; private set; }
        public DelegateCommand AutoHighlightStringCommand { get; private set; }
        public DelegateCommand<TextEditor> AvalonTextEditor_LoadedCommand { get; private set; }
        public AsyncCommand SelectAltoFilesCommand { get; private set; }
        public AsyncCommand SelectImageFilesCommand { get; private set; }
        public DelegateCommand SaveUpdateXmlFileCommand { get; private set; }
        public DelegateCommand XmlTextChangedCommand { get; private set; }

        public DelegateCommand BoxTicknessUiChangedCommand { get; private set; }


        #region Properties

        private double _xmlTextFontSize;
        public double XmlTextFontSize
        {
            get { return _xmlTextFontSize; }
            set { _xmlTextFontSize = value; RaisePropertiesChanged(nameof(XmlTextFontSize)); }
        }

        private TextDocument _CodingTxtDocument;
        public TextDocument CodingTxtDocument
        {
            get { return _CodingTxtDocument; }
            set { _CodingTxtDocument = value; RaisePropertiesChanged(nameof(CodingTxtDocument)); }
        }


        private BitmapSource _selectedImageSource;

        public BitmapSource SelectedImageSource
        {
            get { return _selectedImageSource; }
            set { _selectedImageSource = value; RaisePropertiesChanged(nameof(SelectedImageSource)); }
        }


        private ObservableCollection<AltoModel> _altoCollection;
        public ObservableCollection<AltoModel> AltoCollection
        {
            get { return _altoCollection; }
            set { _altoCollection = value; RaisePropertiesChanged(nameof(AltoCollection)); }
        }


        private double _originalImagePixelWidth;
        public double OriginalImagePixelWidth
        {
            get { return _originalImagePixelWidth; }
            set { _originalImagePixelWidth = value; RaisePropertiesChanged(nameof(OriginalImagePixelWidth)); }
        }

        private double _originalImagePixelHeight;
        public double OriginalImagePixelHeight
        {
            get { return _originalImagePixelHeight; }
            set { _originalImagePixelHeight = value; RaisePropertiesChanged(nameof(OriginalImagePixelHeight)); }
        }

        private TextEditor _codingTextControl;

        public TextEditor CodingTextControl
        {
            get { return _codingTextControl; }
            set { _codingTextControl = value; RaisePropertiesChanged(nameof(CodingTextControl)); }
        }

        private double _DefaultZoom;
        public double DefaultZoom
        {
            get { return _DefaultZoom; }
            set { _DefaultZoom = value; RaisePropertiesChanged(nameof(DefaultZoom)); }
        }
        private double _ImageViewerScale;

        public double ImageViewerScale
        {
            get { return _ImageViewerScale; }
            set { _ImageViewerScale = value; RaisePropertiesChanged(nameof(ImageViewerScale)); }
        }


        private double _ImageHeight;
        public double ImageHeight
        {
            get { return _ImageHeight; }
            set { _ImageHeight = value; RaisePropertiesChanged(); CanvasHeight = _ImageHeight; }
        }

        private double _ImageWidth;

        public double ImageWidth
        {
            get { return _ImageWidth; }
            set { _ImageWidth = value; RaisePropertiesChanged(); CanvasWidth = _ImageWidth; }
        }

        private double _CanvasHeight;

        public double CanvasHeight
        {
            get { return _CanvasHeight; }
            set { _CanvasHeight = value; RaisePropertiesChanged(); }
        }

        private double _CanvasWidth;

        public double CanvasWidth
        {
            get { return _CanvasWidth; }
            set { _CanvasWidth = value; RaisePropertiesChanged(); }
        }

        private int _boxTicknessUi;

        public int BoxTicknessUi
        {
            get { return _boxTicknessUi; }
            set { _boxTicknessUi = value; RaisePropertiesChanged(nameof(BoxTicknessUi)); }
        }

        private string _xmlFileName;

        public string XmlFileName
        {
            get { return _xmlFileName; }
            set { _xmlFileName = value; RaisePropertiesChanged(nameof(XmlFileName)); }
        }

        private string _imageFileName;

        public string ImageFileName
        {
            get { return _imageFileName; }
            set { _imageFileName = value; RaisePropertiesChanged(nameof(ImageFileName)); }
        }


        private bool _isAutoHighlightString;

        public bool IsAutoHighlightString
        {
            get { return _isAutoHighlightString; }
            set { _isAutoHighlightString = value; RaisePropertiesChanged(nameof(IsAutoHighlightString)); }
        }



        #endregion

        #region Fields
        public string GlobalXmlFilePath;

        #endregion


        public MainViewModel()
        {

            ZoomXmlTextViewerCommand = new DelegateCommand<MouseWheelEventArgs>(ZoomXmlTextViewer);
            ZoomImageViewerCommand = new DelegateCommand<MouseWheelEventArgs>(ZoomImageViewer);
            AutoHighlightStringCommand = new DelegateCommand(AutoHighlightString);
            AvalonTextEditor_LoadedCommand = new DelegateCommand<TextEditor>(AvalonTextEditor_Loaded);
            SelectAltoFilesCommand = new AsyncCommand(SelectAltoFiles);
            SelectImageFilesCommand = new AsyncCommand(SelectImageFiles);
            SaveUpdateXmlFileCommand = new DelegateCommand(SaveUpdateXmlFile);
            XmlTextChangedCommand = new DelegateCommand(XmlTextChanged);

            BoxTicknessUiChangedCommand = new DelegateCommand(BoxTicknessUiChanged);

            AltoCollection = new ObservableCollection<AltoModel>();

            XmlTextFontSize = 12;
            BoxTicknessUi = 5;
            DefaultZoom = 0.5;
            ImageViewerScale = DefaultZoom;

            IsAutoHighlightString = false;

            ImageFileName = "...";
            XmlFileName = "...";
        }




        #region File Selection
        /// <summary>
        /// Select ALTO XML file
        /// Load the file in text viewer
        /// </summary>
        /// <returns></returns>
        private async Task SelectAltoFiles()
        {
            try
            {
                string altoFilePath = GetFilePath(@"Select alto files (*.xml)", "*.xml", "Open multiple xml documents");
                if (altoFilePath == null) { return; }


                string textContent = "";
                // Load xml file in textviewer
                using (FileStream fs = new FileStream(altoFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    using (StreamReader reader = FileReader.OpenStream(fs, Encoding.UTF8))
                    {
                        textContent = await reader.ReadToEndAsync();
                        CodingTxtDocument = new TextDocument(textContent);
                    }
                }

                GlobalXmlFilePath = altoFilePath;
                XmlFileName = Path.GetFileName(altoFilePath);
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        /// <summary>
        /// Select Image file and load the image
        /// </summary>
        /// <returns></returns>
        private async Task SelectImageFiles()
        {
            try
            {
                //Select image file
                List<string> ImageExtensionCollection = new List<string> { "jpg", "jpeg", "png", "tif", "tiff" };

                string filterExtensions = string.Join(";", ImageExtensionCollection.Select(ext => $"*.{ext}"));
                string filter = $"Image files ({filterExtensions})|{filterExtensions}";

                string imageFilePath = GetFilePath(filter, filterExtensions, "Open image file");
                if (imageFilePath == null) return;

                ImageFileName = Path.GetFileName(imageFilePath);

                #region Validation
                string imagename = Path.GetFileNameWithoutExtension(imageFilePath);
                string altoname = Path.GetFileNameWithoutExtension(GlobalXmlFilePath);

                if ($"ALTO_{imagename}" != altoname)
                {
                    InformationMessage("Image name and ALTO name must be the same", "Not Match");
                    return;
                }
                #endregion

                byte[] imageData;
                int bufferSize = 4096;
                using (FileStream fs = new FileStream(imageFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, bufferSize * 3, useAsync: true))
                {
                    imageData = new byte[fs.Length];
                    await fs.ReadAsync(imageData, 0, (int)fs.Length);
                }

                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // Load image into memory to improve performance
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze(); // Freeze to make it cross-thread accessible
                    SelectedImageSource = bitmap;

                    ImageWidth = bitmap.PixelWidth;
                    ImageHeight = bitmap.PixelHeight;

                }

                ParseAlto(CodingTextControl.Text);
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }
        #endregion


        #region EventToCommand 

        /// <summary>
        /// Load xml text editor attach with scroll event
        /// Change Single Tag Color 
        /// </summary>
        /// <param name="xamlInterfaceControlElement"></param>
        /// <returns></returns>
        private void AvalonTextEditor_Loaded(TextEditor xamlInterfaceControlElement)
        {
            try
            {
                CodingTextControl = xamlInterfaceControlElement;

                // Attach the custom syntax highlighter
                CodingTextControl.TextArea.TextView.LineTransformers.Clear(); // optional: clear any existing
                CodingTextControl.TextArea.TextView.LineTransformers.Add(new TagChangeColor());
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        /// <summary>
        /// TextChanged event handler for the XML text editor.
        /// This will be update the bounding box when the xml text changed.
        /// </summary>
        private void XmlTextChanged()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(CodingTextControl.Text))
                {
                    ParseAlto(CodingTextControl.Text);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }


        /// <summary>
        /// Highlights the entire <String ... />; tag under the caret and updates the selection
        /// </summary>
        /// <param name="e"></param>
        private void AutoHighlightString()
        {
            try
            {
                SelectedStringChanged highlight = new SelectedStringChanged();
                
                if (CodingTextControl?.Document == null) { return; }

                if (IsAutoHighlightString == false) 
                {
                    highlight.AvalonTextSelectionChanged(CodingTextControl, AltoCollection); 
                    return; 
                }

                // Delay until AvalonEdit completes its built-in selection handling
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                {
                    string fullText = CodingTextControl.Text;
                    int selectionStart = CodingTextControl.SelectionStart;

                    // Regex to match <String ... />
                    Regex stringRegex = new Regex(@"<String\s+[^>]*?/>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    foreach (Match match in stringRegex.Matches(fullText))
                    {
                        // If the caret or selection start is inside the tag
                        if (selectionStart >= match.Index && selectionStart <= match.Index + match.Length)
                        {
                            CodingTextControl.Select(match.Index, match.Length);
                            CodingTextControl.ScrollToLine(CodingTextControl.Document.GetLineByOffset(match.Index).LineNumber);

                            highlight.AvalonTextSelectionChanged(CodingTextControl, AltoCollection);

                            return;
                        }
                    }
                }), DispatcherPriority.ApplicationIdle); // Ensures this runs after AvalonEdit finalizes selection
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }


        /// <summary>
        /// Zoom Xml Text Control
        /// Ctrl+MouseWheel
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private void ZoomXmlTextViewer(MouseWheelEventArgs args)
        {
            try
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (CodingTxtDocument != null)
                    {
                        if (args.Delta > 0)
                        {
                            XmlTextFontSize += 0.5;
                        }
                        else
                        {
                            XmlTextFontSize -= 0.5;
                        }

                        args.Handled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }


        private void ZoomImageViewer(MouseWheelEventArgs args)
        {
            try
            {
                if (args == null)
                {
                    return;
                }

                int delta = args.Delta;
                double increment = 0;

                if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Control)
                {
                    increment = delta > 0 ? 0.1 : -0.1;
                }
                else
                {
                    increment = delta > 0 ? 0.01 : -0.01;
                }

                double tempScale = ImageViewerScale + increment;

                // Larger value

                if (tempScale < 0.01)
                {
                    return;
                }

                ImageViewerScale = tempScale;
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        private void BoxTicknessUiChanged()
        {
            foreach (var box in AltoCollection)
            {
                box.BoxTickness = BoxTicknessUi;
            }
        }

        #endregion


        #region Create Bounding box to highlight the ALTO coordinates
        /// <summary>
        /// Parse the coordinates of the strings from the ALTO XML file.
        /// Parse the Page to get the image size.
        /// </summary>
        /// <param name="altoPath"></param>
        public void ParseAlto(string xmlText)
        {
            try
            {
                XDocument xml = XDocument.Parse(xmlText);
                var ns = xml.Root.GetDefaultNamespace();

                var strings = xml.Descendants(ns + "String");
                var pageElement = xml.Descendants(ns + "Page").FirstOrDefault();

                AltoCollection.Clear();

                //gets the string coordinates from the XML
                foreach (var sElement in strings)
                {

                    if (double.TryParse(sElement.Attribute("HPOS")?.Value, out double hpos) &&
                       double.TryParse(sElement.Attribute("VPOS")?.Value, out double vpos) &&
                       double.TryParse(sElement.Attribute("WIDTH")?.Value, out double width) &&
                       double.TryParse(sElement.Attribute("HEIGHT")?.Value, out double height))
                    {
                        AltoCollection.Add(new AltoModel
                        {
                            X = hpos,
                            Y = vpos,
                            Width = width,
                            Height = height,
                        });
                    }

                }

                if (pageElement != null)
                {
                    OriginalImagePixelWidth = double.Parse(pageElement.Attribute("WIDTH").Value);
                    OriginalImagePixelHeight = double.Parse(pageElement.Attribute("HEIGHT").Value);


                    // Refresh red box scaling
                    UpdateAltoScales(ImageWidth, ImageHeight);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }

        }

        /// <summary>
        /// Updates the scaled coordinates of each ALTO box to match the rendered image size.
        /// This ensures that the red overlay rectangles align correctly on the image.
        /// </summary>
        private void UpdateAltoScales(double actualWidth, double actualHeight)
        {
            try
            {
                //gets the original ALTO image size and the current displayed image size
                double scaleX = actualWidth / OriginalImagePixelWidth;
                double scaleY = actualHeight / OriginalImagePixelHeight;

                //uppdate the scaled position and size for each ALTO box
                foreach (var box in AltoCollection)
                {
                    //position and size scaled proportionally to match the image's rendered size
                    box.ScaledX = box.X * scaleX;
                    box.ScaledY = box.Y * scaleY;
                    box.ScaledWidth = box.Width * scaleX;
                    box.ScaledHeight = box.Height * scaleY;
                    box.ColorBox = Brushes.Red;
                    box.BoxTickness = BoxTicknessUi;
                    box.OpacityBackground = 100;
                    box.BackgroundBoxColor = Brushes.Transparent;
                }

            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }

        }

        #endregion


        #region Save and Update Xml File

        /// <summary>
        /// Update and save Xml text editor
        /// Use Crtl+S to save the changes
        /// </summary>
        private void SaveUpdateXmlFile()
        {
            try
            {
                if (CodingTxtDocument == null) return;

                // Get the updated XML text from AvalonEdit
                string updatedXml = CodingTxtDocument.Text;

                ReloadandUpadateXmlTextViewer(updatedXml);

                ParseAlto(CodingTextControl.Text);
                InformationMessage("Save Xml successful", "Success");
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        /// <summary>
        /// Reload Xml text Editor and xml file
        /// </summary>
        /// <param name="isEnableBrowserScroll"></param>
        private void ReloadandUpadateXmlTextViewer(string updatexml)
        {
            try
            {
                // Save the updated XML content
                File.WriteAllText(GlobalXmlFilePath, updatexml, Encoding.UTF8);

                // Update AvalonEdit text editor
                if (CodingTextControl != null)
                {
                    CodingTextControl.Text = updatexml;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }

        }





        #endregion


    }



    #region dump

    //private T FindParent<T>(DependencyObject child) where T : DependencyObject
    //{
    //    DependencyObject parent = VisualTreeHelper.GetParent(child);
    //    while (parent != null && !(parent is T))
    //    {
    //        parent = VisualTreeHelper.GetParent(parent);
    //    }
    //    return parent as T;
    //}

    //var editor = e.Source as TextEditor;
    //if (editor == null)
    //   { return; }

    //var textView = editor.TextArea.TextView;
    //textView.EnsureVisualLines();

    //var position = textView.GetPosition(e.GetPosition(textView));
    //if (position == null) { return; }

    //int offset = editor.Document.GetOffset(position.Value.Location);
    //string fullText = editor.Document.Text;

    //Regex stringRegex = new Regex(@"<String\s+[^>]*?/>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    //foreach (Match match in stringRegex.Matches(fullText))
    //{
    //    if (offset >= match.Index && offset <= match.Index + match.Length)
    //    {
    //        editor.Select(match.Index, match.Length);
    //        break;
    //    }

    //}
    //if (parameter is MouseButtonEventArgs e)
    //{
    //    // Get TextEditor from CommandParameter
    //    var editor = e.Source as TextEditor ?? e.OriginalSource as TextEditor;
    //    if (editor == null && e.Source is DependencyObject source)
    //    {
    //        editor = FindParent<TextEditor>(source);
    //    }

    //    if (editor == null)
    //    { return; }

    //    var textView = editor.TextArea.TextView;
    //    textView.EnsureVisualLines(); // Ensure visuals are valid

    //    var mousePosition = e.GetPosition(textView);
    //    var logicalPos = textView.GetPosition(mousePosition);
    //    if (logicalPos == null)
    //       { return; }

    //    int offset = editor.Document.GetOffset(logicalPos.Value.Location);
    //    string fullText = editor.Document.Text;

    //    if (offset < 0 || offset > fullText.Length)
    //       { return; }

    //    Regex stringRegex = new Regex(@"<String\s+[^>]*?/>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    //    foreach (Match match in stringRegex.Matches(fullText))
    //    {
    //        if (offset >= match.Index && offset <= match.Index + match.Length)
    //        {
    //            editor.Select(match.Index, match.Length);
    //            editor.ScrollToLine(editor.Document.GetLineByOffset(match.Index).LineNumber);
    //            break;
    //        }
    //    }
    //}

    //protected override void ColorizeLine(DocumentLine line)
    //{
    //    string text = CurrentContext.Document.GetText(line);


    //    // Match links inside the text (e.g., http://...)
    //    MatchCollection linkMatches = sLinkRegex.Matches(text);
    //    foreach (Match match in linkMatches)
    //    {
    //        if (match.Success)
    //        {
    //            int startOffset = line.Offset + match.Index;
    //            int endOffset = startOffset + match.Length;
    //            ChangeLinePart(startOffset, endOffset, (visualElement) =>
    //            {
    //                visualElement.TextRunProperties.SetForegroundBrush(new SolidColorBrush(Color.FromRgb(255, 192, 0))); // Yellowish-orange color
    //            });
    //        }
    //    }

    //    // 1. XML Tags
    //    foreach (Match match in sTagRegex.Matches(text))
    //    {
    //        int start = line.Offset + match.Index;
    //        int end = start + match.Length;
    //        ChangeLinePart(start, end, (visualElement) =>
    //        {
    //            visualElement.TextRunProperties.SetForegroundBrush(new SolidColorBrush(Color.FromRgb(86, 156, 214))); // #569CD6
    //        });
    //    }

    //    // 2. Attribute Names
    //    foreach (Match match in sAttributeRegex.Matches(text))
    //    {
    //        int start = line.Offset + match.Index;
    //        int end = start + match.Length;
    //        ChangeLinePart(start, end, (visualElement) =>
    //        {
    //            visualElement.TextRunProperties.SetForegroundBrush(new SolidColorBrush(Color.FromRgb(156, 220, 254))); // #9CDCFE
    //        });
    //    }

    //    // 3. Attribute Equal Signs + Quotes
    //    foreach (Match match in sEqualQuoteRegex.Matches(text))
    //    {
    //        int start = line.Offset + match.Index;
    //        int end = start + match.Length;
    //        ChangeLinePart(start, end, (visualElement) =>
    //        {
    //            visualElement.TextRunProperties.SetForegroundBrush(new SolidColorBrush(Color.FromRgb(255, 128, 128))); // Light reddish color
    //        });
    //    }

    //    // 4. Attribute Values
    //    foreach (Match match in sAttributeValueRegex.Matches(text))
    //    {
    //        int start = line.Offset + match.Index;
    //        int end = start + match.Length;
    //        ChangeLinePart(start, end, (visualElement) =>
    //        {
    //            visualElement.TextRunProperties.SetForegroundBrush(new SolidColorBrush(Color.FromRgb(206, 145, 120))); // #CE9178
    //        });
    //    }

    //    // 5. Text content between tags
    //    foreach (Match match in sTextContentRegex.Matches(text))
    //    {
    //        if (!string.IsNullOrWhiteSpace(match.Groups[1].Value))
    //        {
    //            int start = line.Offset + match.Index + 1;
    //            int end = start + match.Groups[1].Length;
    //            ChangeLinePart(start, end, (visualElement) =>
    //            {
    //                visualElement.TextRunProperties.SetForegroundBrush(new SolidColorBrush(Color.FromRgb(212, 212, 212))); // #D4D4D4
    //            });
    //        }
    //    }

    //    // Match the tag endings (>, />, ?>)
    //    MatchCollection tagEndMatches = sTagEndRegex.Matches(text);
    //    foreach (Match match in tagEndMatches)
    //    {
    //        if (match.Success)
    //        {
    //            int startOffset = line.Offset + match.Index;
    //            int endOffset = startOffset + match.Length;
    //            ChangeLinePart(startOffset, endOffset, (visualElement) =>
    //            {
    //                visualElement.TextRunProperties.SetForegroundBrush(new SolidColorBrush( Color.FromRgb(86, 156, 214))); // Same blue as for < and <? 
    //            });
    //        }
    //    }



    //}
    //}

    #endregion
}
