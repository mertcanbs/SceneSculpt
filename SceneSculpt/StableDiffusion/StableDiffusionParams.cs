using System.Collections.Generic;

namespace SceneSculpt
{
    public class StableDiffusionParams
    {
        public const int MIN_STEPS = 10;
        public const int MAX_STEPS = 150;
        public const int MIN_CFG_SCALE = 0;
        public const int MAX_CFG_SCALE = 35;
        public const double MIN_PROMPT_WEIGHT = 0;
        public const double MAX_PROMPT_WEIGHT = 1;
				public const double MIN_IMAGE_STRENGTH = 0;
				public const double MAX_IMAGE_STRENGTH = 1;

        public static readonly List<string> CLIP_GUIDANCE_PRESETS = new List<string>
        {
            "FAST_BLUE",
            "FAST_GREEN",
            "NONE",
            "SIMPLE",
            "SLOW",
            "SLOWER",
            "SLOWEST"
        };

        public static readonly List<string> SAMPLERS = new List<string>
        {
            "",
            "DDIM",
            "DDPM",
            "K_DPMPP_2M",
            "K_DPMPP_2S_ANCESTRAL",
            "K_DPM_2",
            "K_DPM_2_ANCESTRAL",
            "K_EULER",
            "K_EULER_ANCESTRAL",
            "K_HEUN",
            "K_LMS"
        };

        public static readonly List<string> STYLE_PRESETS = new List<string>
        {
            "3d-model",
            "analog-film",
            "anime",
            "cinematic",
            "comic-book",
            "digital-art",
            "enhance",
            "fantasy-art",
            "isometric",
            "line-art",
            "low-poly",
            "modeling-compound",
            "neon-punk",
            "origami",
            "photographic",
            "pixel-art",
            "tile-texture"
        };

        public string Prompt { get; set; } = "";
        public int Steps { get; set; } = 50;
        public int Height { get; set; } = 512;
        public int Width { get; set; } = 512;
        public int CfgScale { get; set; } = 7;
        public double PromptWeight { get; set; } = 1;
        public double ImageStrength { get; set; } = 0.35;
        public string ClipGuidancePreset { get; set; } = "NONE";
        public string Sampler { get; set; } = "";
        public string StylePreset { get; set; } = "photographic";
    }
}
