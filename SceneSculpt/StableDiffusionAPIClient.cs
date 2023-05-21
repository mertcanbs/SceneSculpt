using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using RestSharp;
using Rhino;

namespace SceneSculpt
{
	public static class StableDiffusionAPIClient
	{
		private static readonly string engineId = "stable-diffusion-xl-beta-v2-2-2";
		private static readonly RestClient client = new RestClient($"https://api.stability.ai/v1");
		private static readonly string apiKey = ""; 

		// Config
		private static int height = 512;
		private static int width = 512;
		private static int cfgScale = 7;
		private static double promptWeight = 1;
		private static string stylePreset = "photographic";

		public static async Task<string> TextToImage(string text)
		{
			var body = JsonSerializer.Serialize(new TextToImageRequest(text));
			var request = new RestRequest($"/generation/{engineId}/text-to-image", Method.Post).AddBody(body, ContentType.Json);
			request.AddHeader("Authorization", $"Bearer {apiKey}");
			request.AddHeader("Content-Type", "application/json");
			request.AddHeader("Accept", "application/json");
			var response = await client.ExecuteAsync(request);

			if (response.IsSuccessful)
			{
				try
				{
					RhinoApp.WriteLine("received image");
					File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "response.json"), response.Content);
					RhinoApp.WriteLine("file written");
					var serializedResponse = JsonSerializer.Deserialize<ImageResponse>(response.Content);
					RhinoApp.WriteLine("deserialized");
					return serializedResponse.Artifacts[0].Base64;
				}
				catch (Exception e)
				{
					RhinoApp.WriteLine(e.ToString());
					return null;
				}
			}
			else
			{
				RhinoApp.WriteLine(response.Content);
				return null;
			}
		}

		public static async Task<string> ImageToImage(string text, byte[] imageBytes)
		{
			var body = JsonSerializer.Serialize(new ImageToImageRequest(text));
			var request = new RestRequest($"/generation/{engineId}/image-to-image", Method.Post);
			// request.AddStringBody(body, DataFormat.Json);
			request.AddParameter("text_prompts[0][text]", text);
			request.AddParameter("text_prompts[0][weight]", promptWeight);
			request.AddHeader("Authorization", $"Bearer {apiKey}");
			request.AddHeader("Content-Type", "multipart/form-data");
			request.AddHeader("Accept", "application/json");
			request.AddFile("init_image", imageBytes, "image.png", "image/png");
			var response = await client.ExecuteAsync(request);

			if (response.IsSuccessful)
			{
				try
				{
					RhinoApp.WriteLine("received image");
					File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "response.json"), response.Content);
					var serializedResponse = JsonSerializer.Deserialize<ImageResponse>(response.Content);
					RhinoApp.WriteLine(serializedResponse.Artifacts[0].Base64.Length.ToString());
					return serializedResponse.Artifacts[0].Base64;
				}
				catch (Exception e)
				{
					RhinoApp.WriteLine(e.ToString());
					return null;
				}
			}
			else
			{
				RhinoApp.WriteLine(response.Content);
				return null;
			}
		}

		private class ImageToImageRequest
		{
			[JsonPropertyName("image_strength")]
			public double ImageStrength { get; set; } = 0.35;

			[JsonPropertyName("init_image_mode")]
			public string InitImageMode { get; set; } = "IMAGE_STRENGTH";

			[JsonPropertyName("text_prompts[0][text]")]
			public string TextPrompt { get; set; }

			[JsonPropertyName("text_prompts[0][weight]")]
			public double TextPromptWeight { get; set; } = promptWeight;

			[JsonPropertyName("cfg_scale")]
			public int CfgScale { get; set; } = cfgScale;

			[JsonPropertyName("clip_guidance_preset")]
			public string ClipGuidancePreset { get; set; } = "FAST_BLUE";

			[JsonPropertyName("sampler")]
			public string Sampler { get; set; } = "K_DPM_2_ANCESTRAL";

			[JsonPropertyName("samples")]
			public int Samples { get; set; } = 1;

			[JsonPropertyName("steps")]
			public int Steps { get; set; } = 20;

			public ImageToImageRequest(string text)
			{
				TextPrompt = text;
			}
		}

		private class TextPrompt
		{
			[JsonPropertyName("text")]
			public string Text { get; set; }

			[JsonPropertyName("weight")]
			public double Weight { get; set; } = promptWeight;

			public TextPrompt(string text)
			{
				Text = text;
			}
		}

		private class TextToImageRequest
		{
			[JsonPropertyName("height")]
			public int Height { get; set; } = height;

			[JsonPropertyName("width")]
			public int Width { get; set; } = width;

			[JsonPropertyName("cfg_scale")]
			public int CfgScale { get; set; } = cfgScale;

			[JsonPropertyName("text_prompts")]
			public List<TextPrompt> TextPrompts { get; set; }

			[JsonPropertyName("style_preset")]
			public string StylePreset { get; set; } = stylePreset;

			public TextToImageRequest(string text)
			{
				TextPrompts = new List<TextPrompt> { new TextPrompt(text) };
			}
		}

		private class ImageResponse
		{
			[JsonPropertyName("artifacts")]
			public List<ImageResponseArtifact> Artifacts { get; set; }
		}

		private class ImageResponseArtifact
		{
			[JsonPropertyName("base64")]
			public string Base64 { get; set; }

			[JsonPropertyName("seed")]
			public Int64 Seed { get; set; }

			[JsonPropertyName("finishReason")]
			public string FinishReason { get; set; }
		}
	}
}

