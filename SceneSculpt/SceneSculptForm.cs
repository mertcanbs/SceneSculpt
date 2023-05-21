using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Eto.Drawing;
using Eto.Forms;
using Rhino;

namespace SceneSculpt
{
    internal sealed class SceneSculptForm : Form
    {
        private const int imageViewHeight = 500;
        private const int imageViewWidth = 500;
        private TextBox promptBox = new TextBox();
        private Button goButton = new Button { Text = "Go" };
        private Button applyAsBackgroundButton = new Button { Text = "Apply as Background" };
        private Button importImageButton = new Button { Text = "Import Image" };
        private Spinner spinner = new Spinner();
        private Panel spinnerPanel;
        private Bitmap currentImage;
        private DropDown modeDropDown = new DropDown
        {
            Items = { "Regenerate", "Iterate" },
            SelectedIndex = 0
        };

        private ImageView imageView = new ImageView()
        {
            Size = new Size(imageViewWidth, imageViewHeight),
        };

        private bool isGenerating = false;

        public SceneSculptForm()
        {
            Title = "SceneSculpt";
            MinimumSize = new Size(500, 500);

            goButton.Click += OnGoClicked;
            applyAsBackgroundButton.Click += OnApplyAsBackgroundClicked;
            importImageButton.Click += OnImportImageClicked;

            spinnerPanel = new Panel
            {
                Size = new Size(imageViewWidth, imageViewHeight),
                Content = new StackLayout
                {
                    Items = { spinner },
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center
                },
            };

            imageView.Visible = false;
            spinnerPanel.Visible = false;
            applyAsBackgroundButton.Visible = false;

            Content = new StackLayout
            {
                Padding = 20,
                Items =
                {
                    spinnerPanel,
                    imageView,
                    promptBox,
                    modeDropDown,
                    goButton,
                    applyAsBackgroundButton,
                    importImageButton
                }
            };
        }

        private void OnApplyAsBackgroundClicked(object sender, EventArgs e)
        {
            var views = RhinoDoc.ActiveDoc.Views.GetViewList(true, true);
            var viewNames = views.Select(v => v.MainViewport.Name).ToList();
            var selectedViewName = (string)
                Rhino.UI.Dialogs.ShowListBox("Views", "Select a view", viewNames);
            if (selectedViewName == null)
                return;

            // TODO: Save in a temp path or the project path
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "background.png"
            );
            currentImage.Save(path, ImageFormat.Png);

            var selectedView = views.FirstOrDefault(v => v.MainViewport.Name == selectedViewName);
            if (selectedView == null)
                return;

            selectedView.MainViewport.SetWallpaper(path, false);
        }

        ~SceneSculptForm()
        {
            goButton.Click -= OnGoClicked;
            applyAsBackgroundButton.Click -= OnApplyAsBackgroundClicked;
            importImageButton.Click -= OnImportImageClicked;
        }

        private async void OnGoClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(promptBox.Text) || isGenerating)
            {
                return;
            }

            isGenerating = true;
            spinnerPanel.Visible = true;
            spinner.Enabled = true;
            imageView.Visible = false;
            applyAsBackgroundButton.Visible = false;

            try
            {
                string base64 = "";
                if (modeDropDown.SelectedIndex == 0)
                {
                    base64 = await StableDiffusionAPIClient.TextToImage(promptBox.Text);
                }
                else
                {
                    base64 = await StableDiffusionAPIClient.ImageToImage(
                        promptBox.Text,
                        currentImage.ToByteArray(ImageFormat.Png)
                    );
                }

                if (!string.IsNullOrEmpty(base64))
                {
                    var image = Convert.FromBase64String(base64);
                    var bitmap = new Bitmap(image);
                    imageView.Image = bitmap;
                    currentImage = bitmap;
                    imageView.Visible = true;
                    applyAsBackgroundButton.Visible = true;
                }
                else
                {
                    RhinoApp.WriteLine("No image received by SceneSculptForm");
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine(ex.Message);
            }
            finally
            {
                isGenerating = false;
                spinnerPanel.Visible = false;
                spinner.Enabled = false;
            }
        }

        private void OnImportImageClicked(object sender, EventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filters.Add(
                new FileFilter("Image Files", new string[] { ".png", ".jpg", ".jpeg", ".bmp" })
            );
            fileDialog.MultiSelect = false;
            fileDialog.CheckFileExists = true;
            fileDialog.Title = "Import Image";
            DialogResult result = fileDialog.ShowDialog(this);

            if (result != DialogResult.Ok)
                return;

            try
            {
                var bitmap = new System.Drawing.Bitmap(fileDialog.FileName);
                var cropped = bitmap.ResizeAndCrop(new System.Drawing.Rectangle(0, 0, 512, 512));
								using (Stream stream = new MemoryStream()) {
									cropped.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
									stream.Position = 0;
									currentImage = new Bitmap(stream);
									imageView.Image = currentImage;
									imageView.Visible = true;
									applyAsBackgroundButton.Visible = true;
								}
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine(ex.Message);
            }
        }
    }
}
