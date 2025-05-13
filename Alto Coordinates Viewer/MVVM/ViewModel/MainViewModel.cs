using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using Alto_Coordinates_Viewer.MVVM.Model;
using DevExpress.Mvvm;
using Emgu.CV.Structure;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;
namespace Alto_Coordinates_Viewer.MVVM.ViewModel
{
    class MainViewModel : Abstract.ViewBaseModel
    {
        public AsyncCommand<MouseWheelEventArgs> ZoomXmlTextViewerCommand { get; private set; }
        public AsyncCommand<TextEditor> AvalonTextEditor_LoadedCommand { get; private set; }
        public AsyncCommand SelectAltoFilesCommand { get; private set; }
        public AsyncCommand SelectImageFilesCommand { get; private set; }
        public DelegateCommand<SizeChangedEventArgs> ImageSizeChangedCommand { get; private set; }
        public DelegateCommand SaveUpdateXmlFileCommand { get; private set; }

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
            set { _selectedImageSource = value; RaisePropertiesChanged(nameof(SelectedImageSource));  }
        }


        private ObservableCollection<AltoModel> _altoCollection;
        public ObservableCollection<AltoModel> AltoCollection
        {
            get { return _altoCollection; }
            set { _altoCollection = value; RaisePropertiesChanged(nameof(AltoCollection)); }
        }

        private double _imagePixelWidth;

        public double ImagePixelWidth
        {
            get { return _imagePixelWidth; }
            set { _imagePixelWidth = value; RaisePropertiesChanged(nameof(ImagePixelWidth)); }
        }

        private double _imagePixelHeight;
        public double ImagePixelHeight
        {
            get { return _imagePixelHeight; }
            set { _imagePixelHeight = value; RaisePropertiesChanged(nameof(ImagePixelHeight)); }
        }

        private double _renderedImageWidth;

        public double RenderedImageWidth
        {
            get { return _renderedImageWidth; }
            set { _renderedImageWidth = value; RaisePropertiesChanged(nameof(RenderedImageWidth)); }
        }

        private double _renderedImageHeight;

        public double RenderedImageHeight
        {
            get { return _renderedImageHeight; }
            set { _renderedImageHeight = value; RaisePropertiesChanged(nameof(RenderedImageHeight)); }
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


        #endregion

        #region Fields
        public string GlobalXmlFilePath;
        #endregion


        public MainViewModel()
        {

            ZoomXmlTextViewerCommand = new AsyncCommand<MouseWheelEventArgs>(ZoomXmlTextViewer);
            AvalonTextEditor_LoadedCommand = new AsyncCommand<TextEditor>(AvalonTextEditor_Loaded);
            SelectAltoFilesCommand = new AsyncCommand(SelectAltoFiles);
            SelectImageFilesCommand = new AsyncCommand(SelectImageFiles);
            ImageSizeChangedCommand = new DelegateCommand<SizeChangedEventArgs>(ImageSizeChanged);
            SaveUpdateXmlFileCommand = new DelegateCommand(SaveUpdateXmlFile);

            AltoCollection = new ObservableCollection<AltoModel>();

            XmlTextFontSize = 12;
        }

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

               
                GlobalXmlFilePath = altoFilePath;
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
                string imageFilePath = GetFilePath(@"Select image files (*.tiff)", "*.tiff", "Open multiple tiff documents");
                if (imageFilePath == null) return;

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

                    ImagePixelWidth = bitmap.PixelWidth;
                    ImagePixelHeight = bitmap.PixelHeight;

                }

                ParseAlto(CodingTextControl.Text);
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
        private async Task ZoomXmlTextViewer(MouseWheelEventArgs args)
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



        /// <summary>
        /// Load xml text editor attach with scroll event
        /// Change Single Tag Color 
        /// </summary>
        /// <param name="xamlInterfaceControlElement"></param>
        /// <returns></returns>
        private async Task AvalonTextEditor_Loaded(TextEditor xamlInterfaceControlElement)
        {
            try
            {
                CodingTextControl = xamlInterfaceControlElement;

                // Attach text changed event to trigger red box updates on-the-fly
                CodingTextControl.TextChanged += (s, e) =>
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
                };

                //CodingTextControl.SyntaxHighlighting.GetNamedColor("XmlTag").Foreground = new SimpleHighlightingBrush(Colors.White);
                //CodingTextControl.TextArea.TextView.LineTransformers.Add(new TagChangeColor());
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        /// <summary>
        /// Triggered when the image is resized (e.g., when the window or control size changes).
        /// Updates the rendered image dimensions and rescales all overlay rectangles accordingly.
        /// </summary>
        private void ImageSizeChanged(SizeChangedEventArgs e)
        {
            try
            {
                // Ensure the event source is a valid visual element (usually the <Image> control)
                if (e?.Source is FrameworkElement image)
                {
                    //gets the current rendered width and height of the image (after stretch)
                    RenderedImageWidth = e.NewSize.Width;
                    RenderedImageHeight = e.NewSize.Height;

                    // Update scaling of ALTO boxes to match the new rendered image size
                    UpdateAltoScales(RenderedImageWidth, RenderedImageHeight);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }

        }

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
                    UpdateAltoScales(RenderedImageWidth, RenderedImageHeight);
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

    #region make darkmode foregound color text
    internal class TagChangeColor : DocumentColorizingTransformer
    {

        // Match URLs (adjust the pattern to match more types of URLs)
        private readonly Regex sLinkRegex = new Regex(@"https?:\/\/[^\s""<>]+", RegexOptions.Compiled);

        // Match XML tags
        private readonly Regex sTagRegex = new Regex(@"<\??/?[\w:.-]+", RegexOptions.Compiled);

        // Match XML tags with attributes endtag
        private readonly Regex sTagEndRegex = new Regex(@"(\?>|/>|>)", RegexOptions.Compiled);

        // Match attribute names (before = )
        private readonly Regex sAttributeRegex = new Regex(@"[\w:.-]+(?=\s*=)", RegexOptions.Compiled);

        // Match attribute values (inside quotes)
        private readonly Regex sAttributeValueRegex = new Regex("\"([^\"]*)\"", RegexOptions.Compiled);

        // Match the ="
        private readonly Regex sEqualQuoteRegex = new Regex(@"=\s*[""']", RegexOptions.Compiled);

        // Match text content inside tags
        private readonly Regex sTextContentRegex = new Regex(@">(.*?)<", RegexOptions.Compiled);

        protected override void ColorizeLine(DocumentLine line)
        {
            string text = CurrentContext.Document.GetText(line);

            // Tag names
            foreach (Match match in sTagRegex.Matches(text))
                SetColor(line, match, Color.FromRgb(86, 156, 214));

            // Tag endings
            foreach (Match match in sTagEndRegex.Matches(text))
                SetColor(line, match, Color.FromRgb(86, 156, 214));

            // Attribute names
            foreach (Match match in sAttributeRegex.Matches(text))
            {
                SetColor(line, match, Color.FromRgb(156, 220, 254));
            }

            // Attribute values (inside quotes)
            foreach (Match match in sAttributeValueRegex.Matches(text))
            {

                // Check if the attribute value is a URL
                if (sLinkRegex.IsMatch(match.Value))
                {
                    Console.WriteLine("Found URL: " + match.Value); // Debugging the matched URL
                    SetColor(line, match, Color.FromRgb(255, 192, 0)); // Yellowish-Orange
                }
                else
                {
                    SetColor(line, match, Color.FromRgb(206, 145, 120));
                }
            }

            // Equal-Quote signs (=")
            foreach (Match match in sEqualQuoteRegex.Matches(text))
                SetColor(line, match, Color.FromRgb(206, 145, 120));

            // Text inside tags (content)
            foreach (Match match in sTextContentRegex.Matches(text))
                SetColor(line, match, Color.FromRgb(212, 212, 212));
        }

        // Helper to set the color
        private void SetColor(DocumentLine line, Match match, Color color)
        {
            if (match.Success)
            {
                int start = line.Offset + match.Index;
                int end = start + match.Length;
                ChangeLinePart(start, end, (visualElement) =>
                {
                    visualElement.TextRunProperties.SetForegroundBrush(new SolidColorBrush(color));
                    visualElement.TextRunProperties.SetTextDecorations(null); // no underline
                });
            }
        }


        #region dump
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

        #endregion
        #endregion
    }
}
