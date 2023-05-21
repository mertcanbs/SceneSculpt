using System;
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
        private static readonly string apiKey =
            "sk-0F4jX4w6EdApKD4JrB6nXxT2cQlpU07GCgZlzOFG2iUVQQm9";

        public static async Task<string> TextToImage(StableDiffusionParams parameters)
        {
            var body = new Dictionary<string, object>
            {
                { "height", parameters.Height },
                { "width", parameters.Width },
                { "cfg_scale", parameters.CfgScale },
                { "steps", parameters.Steps },
                { "clip_guidance_preset", parameters.ClipGuidancePreset },
                { "style_preset", parameters.StylePreset },
                {
                    "text_prompts",
                    new List<object>
                    {
                        new Dictionary<string, object>
                        {
                            { "text", parameters.Prompt },
                            { "weight", parameters.PromptWeight },
                        },
                    }
                },
            };

            if (!string.IsNullOrEmpty(parameters.Sampler))
            {
                body.Add("sampler", parameters.Sampler);
            }

            var request = new RestRequest($"/generation/{engineId}/text-to-image", Method.Post);
            request.AddBody(body, ContentType.Json);
            request.AddHeader("Authorization", $"Bearer {apiKey}");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                try
                {
                    var serializedResponse = JsonSerializer.Deserialize<ImageResponse>(
                        response.Content
                    );
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

        public static async Task<string> ImageToImage(StableDiffusionParams parameters, byte[] imageBytes)
        {
            var request = new RestRequest($"/generation/{engineId}/image-to-image", Method.Post);
            request.AddFile("init_image", imageBytes, "image.png", "image/png");
            request.AddParameter("text_prompts[0][text]", parameters.Prompt);
            request.AddParameter("text_prompts[0][weight]", parameters.PromptWeight);
            request.AddParameter("image_strength", parameters.ImageStrength);
            request.AddParameter("init_image_mode", "IMAGE_STRENGTH");
            request.AddParameter("cfg_scale", parameters.CfgScale);
            request.AddParameter("clip_guidance_preset", parameters.ClipGuidancePreset);
            request.AddParameter("steps", parameters.Steps);
            request.AddParameter("samples", 1);
            if (!string.IsNullOrEmpty(parameters.Sampler))
            {
                request.AddParameter("sampler", parameters.Sampler);
            }
            request.AddHeader("Authorization", $"Bearer {apiKey}");
            request.AddHeader("Content-Type", "multipart/form-data");
            request.AddHeader("Accept", "application/json");
            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                try
                {
                    var serializedResponse = JsonSerializer.Deserialize<ImageResponse>(
                        response.Content
                    );
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
