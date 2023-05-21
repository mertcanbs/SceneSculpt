using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.Display;

namespace SceneSculpt
{
    internal sealed class SceneSculptForm : Form
    {
        private const int imageViewHeight = 512;
        private const int imageViewWidth = 512;
        private TextArea promptBox = new TextArea { Wrap = true };
        private Button goButton = new Button { Text = "Go" };
        private Button applyAsBackgroundButton = new Button { Text = "Apply as Background" };
        private Button captureFromViewportButton = new Button { Text = "Capture from Viewport" };
        private Button importImageButton = new Button { Text = "Import Image" };
        private Button exportImageButton = new Button { Text = "Export Image" };
        private Button toggleConfigButton = new Button { Text = "Config..." };
        private Spinner spinner = new Spinner();
        private Panel spinnerPanel;
        private Bitmap currentImage;
        private DropDown modeDropDown = new DropDown
        {
            Items = { "Regenerate", "Iterate" },
            SelectedIndex = 0
        };

        private ImageView imageView = new ImageView
        {
            Size = new Size(imageViewWidth, imageViewHeight),
        };

        private TableCell configCell;
        private Slider stepsSlider;
        private Label stepsSliderLabel;
        private Slider cfgScaleSlider;
        private Label cfgScaleSliderLabel;
        private Slider promptWeightSlider;
        private Label promptWeightSliderLabel;
        private Slider imageStrengthSlider;
        private Label imageStrengthSliderLabel;
        private DropDown clipGuidancePresetDropDown;
        private DropDown samplerDropDown;
        private DropDown stylePresetDropDown;

        private bool isGenerating = false;

        // TODO: Definitely split this up into functional bits
        public SceneSculptForm()
        {
            Title = "SceneSculpt";
            Size = new Size(-1, -1);
            AutoSize = true;
            Resizable = false;

            goButton.Click += OnGoClicked;
            applyAsBackgroundButton.Click += OnApplyAsBackgroundClicked;
            captureFromViewportButton.Click += OnCaptureFromViewportClicked;
            importImageButton.Click += OnImportImageClicked;
            exportImageButton.Click += OnExportImageClicked;
            toggleConfigButton.Click += OnToggleConfigClicked;

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

            stepsSlider = new Slider
            {
                MinValue = StableDiffusionParams.MIN_STEPS,
                MaxValue = StableDiffusionParams.MAX_STEPS,
                Value = 50,
                Orientation = Orientation.Horizontal,
                SnapToTick = true,
            };

            stepsSlider.ValueChanged += OnStepsSliderValueChanged;
            stepsSliderLabel = new Label { Text = $"Steps: {stepsSlider.Value}" };

            cfgScaleSlider = new Slider
            {
                MinValue = StableDiffusionParams.MIN_CFG_SCALE,
                MaxValue = StableDiffusionParams.MAX_CFG_SCALE,
                Value = 7,
                Orientation = Orientation.Horizontal,
                SnapToTick = true
            };

            cfgScaleSlider.ValueChanged += OnCfgScaleSliderValueChanged;
            cfgScaleSliderLabel = new Label { Text = $"Cfg Scale: {cfgScaleSlider.Value}" };

            promptWeightSlider = new Slider
            {
                MinValue = (int)StableDiffusionParams.MIN_PROMPT_WEIGHT * 100,
                MaxValue = (int)StableDiffusionParams.MAX_PROMPT_WEIGHT * 100,
                Value = 100,
                Orientation = Orientation.Horizontal,
                SnapToTick = true
            };

            promptWeightSlider.ValueChanged += OnPromptWeightSliderValueChanged;
            promptWeightSliderLabel = new Label
            {
                Text = $"Prompt Weight: {(double)promptWeightSlider.Value / 100}"
            };

            imageStrengthSlider = new Slider
            {
                MinValue = (int)StableDiffusionParams.MIN_IMAGE_STRENGTH * 100,
                MaxValue = (int)StableDiffusionParams.MAX_IMAGE_STRENGTH * 100,
                Value = 88,
                Orientation = Orientation.Horizontal,
                SnapToTick = true
            };

            imageStrengthSlider.ValueChanged += OnImageStrengthSliderValueChanged;
            imageStrengthSliderLabel = new Label
            {
                Text = $"Image Strength: {(double)imageStrengthSlider.Value / 100}"
            };

            clipGuidancePresetDropDown = new DropDown
            {
                DataStore = StableDiffusionParams.CLIP_GUIDANCE_PRESETS,
                SelectedValue = "NONE",
            };

            samplerDropDown = new DropDown
            {
                DataStore = StableDiffusionParams.SAMPLERS,
                SelectedValue = "",
            };

            stylePresetDropDown = new DropDown
            {
                DataStore = StableDiffusionParams.STYLE_PRESETS,
                SelectedValue = "photographic",
            };

            configCell = new TableCell
            {
                ScaleWidth = false,
                Control = new StackLayout
                {
                    Visible = false,
                    MinimumSize = new Size(200, -1),
                    Padding = 20,
                    Spacing = 10,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Items =
                    {
                        stepsSliderLabel,
                        stepsSlider,
                        cfgScaleSliderLabel,
                        cfgScaleSlider,
                        new Label { Text = "Clip Guidance" },
                        clipGuidancePresetDropDown,
                        new Label { Text = "Sampler" },
                        samplerDropDown,
                        new Label { Text = "Style" },
                        stylePresetDropDown,
                        imageStrengthSliderLabel,
                        imageStrengthSlider,
                        promptWeightSliderLabel,
                        promptWeightSlider,
                    }
                },
            };

            imageView.Visible = false;
            spinnerPanel.Visible = true;
            spinner.Visible = false;
            applyAsBackgroundButton.Visible = false;

            Content = new TableLayout
            {
                Size = new Size(-1, -1),
                Padding = 20,
                Rows =
                {
                    new TableRow
                    {
                        Cells =
                        {
                            new TableCell
                            {
                                ScaleWidth = false,
                                Control = new StackLayout { Items = { spinnerPanel, imageView } }
                            },
                            new TableCell
                            {
                                ScaleWidth = false,
                                Control = new StackLayout
                                {
                                    MinimumSize = new Size(200, -1),
                                    Padding = 20,
                                    Spacing = 10,
                                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                    Items =
                                    {
                                        promptBox,
                                        modeDropDown,
                                        goButton,
                                        applyAsBackgroundButton,
                                        importImageButton,
                                        exportImageButton,
                                        captureFromViewportButton,
                                        toggleConfigButton
                                    }
                                }
                            },
                            configCell
                        }
                    }
                }
            };
        }

        ~SceneSculptForm()
        {
            goButton.Click -= OnGoClicked;
            applyAsBackgroundButton.Click -= OnApplyAsBackgroundClicked;
            importImageButton.Click -= OnImportImageClicked;
            exportImageButton.Click -= OnExportImageClicked;
            captureFromViewportButton.Click -= OnCaptureFromViewportClicked;
            toggleConfigButton.Click -= OnToggleConfigClicked;
            stepsSlider.ValueChanged -= OnStepsSliderValueChanged;
            cfgScaleSlider.ValueChanged -= OnCfgScaleSliderValueChanged;
            promptWeightSlider.ValueChanged -= OnPromptWeightSliderValueChanged;
            imageStrengthSlider.ValueChanged -= OnImageStrengthSliderValueChanged;
            currentImage?.Dispose();
        }

        private void OnExportImageClicked(object sender, EventArgs e)
        {
            var fileDialog = new SaveFileDialog();
            fileDialog.Filters.Add(new FileFilter("PNG", ".png"));
            var result = fileDialog.ShowDialog(this);
            if (result != DialogResult.Ok)
                return;

            var path = fileDialog.FileName;
            currentImage.Save(path, ImageFormat.Png);
        }

        private void OnImageStrengthSliderValueChanged(object sender, EventArgs e)
        {
            imageStrengthSliderLabel.Text =
                $"Image Strength: {(double)imageStrengthSlider.Value / 100}";
        }

        private void OnPromptWeightSliderValueChanged(object sender, EventArgs e)
        {
            promptWeightSliderLabel.Text =
                $"Prompt Weight: {(double)promptWeightSlider.Value / 100}";
        }

        private void OnStepsSliderValueChanged(object sender, EventArgs e)
        {
            stepsSliderLabel.Text = $"Steps: {stepsSlider.Value}";
        }

        private void OnCfgScaleSliderValueChanged(object sender, EventArgs e)
        {
            cfgScaleSliderLabel.Text = $"Cfg Scale: {cfgScaleSlider.Value}";
        }

        private void OnToggleConfigClicked(object sender, EventArgs e)
        {
            configCell.Control.Visible = !configCell.Control.Visible;
        }

        private void SetCurrentImage(Bitmap bitmap)
        {
            currentImage?.Dispose();
            currentImage = bitmap;
            imageView.Image = currentImage;
            imageView.Visible = true;
            spinnerPanel.Visible = false;
            applyAsBackgroundButton.Visible = true;
        }

        private void OnCaptureFromViewportClicked(object sender, EventArgs e)
        {
            var selectedView = GetViewSelection();
            if (selectedView == null)
                return;

            using (var bitmap = selectedView.CaptureToBitmap(new System.Drawing.Size(512, 512)))
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                SetCurrentImage(new Bitmap(stream));
            }
        }

        private void OnApplyAsBackgroundClicked(object sender, EventArgs e)
        {
            var selectedView = GetViewSelection();
            if (selectedView == null)
                return;

            // TODO: Save in project path?
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "background.png"
            );
            currentImage.Save(path, ImageFormat.Png);
            selectedView.MainViewport.SetWallpaper(path, false);
        }

        private RhinoView GetViewSelection()
        {
            var views = RhinoDoc.ActiveDoc.Views.GetViewList(true, false);
            if (views.Length == 1)
                return views[0];
            var viewNames = views.Select(v => v.MainViewport.Name).ToList();
            var selectedViewName = (string)
                Rhino.UI.Dialogs.ShowListBox("Views", "Select a view", viewNames);
            if (selectedViewName == null)
                return null;

            return views.FirstOrDefault(v => v.MainViewport.Name == selectedViewName);
        }

        private async void OnGoClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(promptBox.Text) || isGenerating)
            {
                return;
            }

            isGenerating = true;
            spinnerPanel.Visible = true;
            spinner.Visible = true;
            spinner.Enabled = true;
            imageView.Visible = false;
            applyAsBackgroundButton.Visible = false;

            try
            {
                string base64 = "";
                var parameters = new StableDiffusionParams
                {
                    Prompt = promptBox.Text,
                    CfgScale = cfgScaleSlider.Value,
                    Steps = stepsSlider.Value,
                    PromptWeight = (double)promptWeightSlider.Value / 100,
                    ImageStrength = (double)imageStrengthSlider.Value / 100,
                    ClipGuidancePreset = (string)clipGuidancePresetDropDown.SelectedValue,
                    StylePreset = (string)stylePresetDropDown.SelectedValue,
                    Sampler = (string)samplerDropDown.SelectedValue,
                };
                // TODO: This is gross, make it type safe with a proper enum
                if (modeDropDown.SelectedIndex == 0)
                {
                    base64 = await StableDiffusionAPIClient.TextToImage(parameters);
                }
                else
                {
                    base64 = await StableDiffusionAPIClient.ImageToImage(
                        parameters,
                        currentImage.ToByteArray(ImageFormat.Png)
                    );
                }

                if (!string.IsNullOrEmpty(base64))
                {
                    var image = Convert.FromBase64String(base64);
                    var bitmap = new Bitmap(image);
                    SetCurrentImage(bitmap);
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
                using (var bitmap = new System.Drawing.Bitmap(fileDialog.FileName))
                using (
                    var cropped = bitmap.ResizeAndCrop(new System.Drawing.Rectangle(0, 0, 512, 512))
                )
                using (Stream stream = new MemoryStream())
                {
                    cropped.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Position = 0;
                    SetCurrentImage(new Bitmap(stream));
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine(ex.Message);
            }
        }
    }
}
