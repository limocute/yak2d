using System;
using System.Numerics;

namespace Yak2D
{
    /// <summary>
    /// A RenderStage that applies stylised effects to a texture source and render to a target
    /// Effects are: Pixellate, Edge Detection, Static Noise, Cathode Ray Tube and Old Movie Reel, 
    /// A combination of all the effects can be applied together
    /// In general, effect results are fed into the next effect
    /// The order being -> Pixellate -> Edge Detection -> Static -> Old Movie Reel -> CRT
    /// This struct contains configuration data for all effects in the stage
    /// </summary>     
    public struct StyleEffectGroupConfiguration
    {
        public PixellateConfiguration Pixellate;
        public EdgeDetectionConfiguration EdgeDetection;
        public StaticConfiguration Static;
        public OldMovieConfiguration OldMovie;
        public CrtEffectConfiguration CRT;
    }

    /// <summary>
    /// Texture Coordinates are rounded so that the image is divided up into a number of squares that each
    /// reference the same pixel, thus creating a pixellated effect
    /// The Intensity is an interpolation between the pre-transformed texture coordinates and the post transform coordiantes
    /// </summary>
    public struct PixellateConfiguration
    {
        /// <summary>
        /// Returns a pre-set effect configuration scaled between 0 and 1
        /// </summary>
        /// <param name="amount">0 to 1 intensity of effect (reflects size of 'pixels')</param>
        /// <returns></returns>
        public static PixellateConfiguration PreSet(float amount)
        {
            amount = Clamper.Clamp(amount, 0.0f, 1.0f);

            var pow = 4.0f + ((1.0f - amount) * 7.0f);

            var numDivisions = (int)Math.Pow(2.0f, pow);

            return new PixellateConfiguration
            {
                Intensity = 1.0f,
                NumXDivisions = numDivisions,
                NumYDivisions = numDivisions
            };
        }

        /// <summary>
        /// 0 to 1, representing an interpolation between unmodified texture coordinates and modified. 1.0 resulting in max pixellation
        /// </summary>
        public float Intensity { get; set; }

        /// <summary>
        /// Number of "pixel" divisions the source is divided up into horizontally
        /// </summary>
        public int NumXDivisions { get; set; }

        /// <summary>
        /// Number of "pixel" divisions the source is divided up into vertically
        /// </summary>
        public int NumYDivisions { get; set; }
    }

    /// <summary>
    /// Detect edges and produce a grayscale result
    /// </summary>
    public struct EdgeDetectionConfiguration
    {
        /// <summary>
        /// Returns a pre-set effect configuration scaled between 0 and 1 (uses Freichen)
        /// </summary>
        /// <param name="amount">0 to 1 intensity of effect</param>
        /// <returns></returns>
        public static EdgeDetectionConfiguration PreSet(float amount)
        {
            amount = Clamper.Clamp(amount, 0.0f, 1.0f);

            return new EdgeDetectionConfiguration
            {
                Intensity = amount,
                IsFreichen = true
            };
        }

        /// <summary>
        /// Interpolation between pre- effect and post-effect pixels (0 to 1)
        /// </summary>
        public float Intensity { get; set; }

        /// <summary>
        /// The standard edge detection algorithm is Sobel, set this as true to use Freichen
        /// </summary>
        public bool IsFreichen { get; set; }
    }

    /// <summary>
    /// Creates an evolving static 'noise' effect
    /// </summary>
    public struct StaticConfiguration
    {
        /// <summary>
        /// Returns a pre-set effect configuration scaled between 0 and 1 (uses 2.0 as texel scalar)
        /// </summary>
        /// <param name="amount">0 to 1 intensity of effect</param>
        /// <returns></returns>
        public static StaticConfiguration PreSet(float amount)
        {
            amount = Clamper.Clamp(amount, 0.0f, 1.0f);

            return new StaticConfiguration
            {
                IgnoreTransparent = 0,
                Intensity = amount,
                TexelScaler = 2.0f,
                TimeSpeed = 10.0f
            };
        }

        /// <summary>
        /// Interpolation between pre- effect and post-effect pixels (0 to 1)
        /// </summary>
        public float Intensity { get; set; }

        /// <summary>
        /// How quickly the static noise evolves over time
        /// </summary>
        public float TimeSpeed { get; set; }

        /// <summary>
        /// If false (0), transparent pixels will not be drawn over
        /// </summary>
        public int IgnoreTransparent { get; set; }

        /// <summary>
        /// The size of each static noise 'pixel'
        /// </summary>
        public float TexelScaler { get; set; }
    }

    /// <summary>
    /// Creates the effect of an old movie reel / projector
    /// Includes: Scratch Lines in frames, Noise, Random Vertical Shifting, Dimming, Reel Rolling, Over Exposure Flicker
    /// </summary>
    public struct OldMovieConfiguration
    {
        /// <summary>
        /// Generate a Default Configurations for the Old Movie Effect
        /// </summary>
        /// <returns></returns>
        public static OldMovieConfiguration GenerateDefault()
        {
            return new OldMovieConfiguration
            {
                Intensity = 1.0f,
                Scratch = 0.02f,
                Noise = 0.1f,
                RndShiftCutOff = 1.0f,
                RndShiftScalar = 0.3f,
                Dim = 0.3f,

                ProbabilityRollStarts = 0.005f,
                ProbabilityRollEnds = 0.08f,
                RollSpeedMin = 1.92f,
                RollSpeedMax = 2.62f,
                RollAccelerationMin = 0.8f,
                RollAccelerationMax = 2.8f,
                RollShakeFactor = 0.20f,
                RollOverallScale = 0.4f,

                OverExposureProbabilityStart = 0.007f,
                OverExposureFlickerTimeMin = 9.4f,
                OverExposureFlickerTimeMax = 24.0f,
                OverExposureIntensityMin = 2.7f,
                OverExposureIntensityMax = 3.4f,
                OverExposureOscillationsMin = 2,
                OverExposureOscillationsMax = 4
            };
        }

        /// <summary>
        /// Returns a pre-set effect configuration scaled between 0 and 1
        /// </summary>
        /// <param name="amount">0 to 1 intensity of effect</param>
        /// <returns></returns>
        public static OldMovieConfiguration PreSet(float amount)
        {
            amount = Clamper.Clamp(amount, 0.0f, 1.0f);

            return new OldMovieConfiguration
            {
                Intensity = amount,
                Scratch = 0.02f * (1.8f * amount),
                Noise = 0.2f * amount,
                RndShiftCutOff = 0.5f + (amount * 2.0f),
                RndShiftScalar = 0.6f * amount,
                Dim = 1.0f * amount,

                ProbabilityRollStarts = 0.002f + (amount * 0.04f),
                ProbabilityRollEnds = 0.09f + ((1.0f - amount) * 0.09f),
                RollSpeedMin = 3.5f + (3.8f * amount),
                RollSpeedMax = 5.6f + (2.0f * amount),
                RollAccelerationMin = 2.3f + (2.2f * amount),
                RollAccelerationMax = 3.2f + (3.7f * amount),
                RollShakeFactor = 0.02f + (amount * 0.45f),
                RollOverallScale = 0.1f * (0.6f * amount),

                OverExposureProbabilityStart = 0.001f + (amount * 0.01f),
                OverExposureFlickerTimeMin = 3.6f + (12.5f * amount),
                OverExposureFlickerTimeMax = 16.0f + (16.0f * amount),
                OverExposureIntensityMin = 1.3f * (3.7f * amount),
                OverExposureIntensityMax = 2.4f * (4.7f * amount),
                OverExposureOscillationsMin = 1 + (int)(amount * 4),
                OverExposureOscillationsMax = 2 + (int)(amount * 8)
            };
        }

        /// <summary>
        /// Used for twice:
        /// 1. An interpolation between unmodified texture coordinates and modified. 1.0 resulting in max pixellation
        /// 2. Interpolation between pre- effect and post-effect pixels (0 to 1)
        /// </summary>
        public float Intensity { get; set; }

        /// <summary>
        /// Scalar for amount of scratch lines
        /// </summary>
        public float Scratch { get; set; }

        /// <summary>
        /// Scalar for Noise
        /// </summary>
        public float Noise { get; set; }

        /// <summary>
        /// Random cut off factor for shifting screen vertically at random intervals
        /// A higher value makes random shifts less likely
        /// </summary>
        public float RndShiftCutOff { get; set; }

        /// <summary>
        /// The intensity of random shift movement
        /// </summary>
        public float RndShiftScalar { get; set; }

        /// <summary>
        /// Scalar for random dimming amount
        /// </summary>
        public float Dim { get; set; }

        /// <summary>
        /// Chance a reel roll starts each frame (lower less likley)
        /// </summary>
        public float ProbabilityRollStarts { get; set; }

        /// <summary>
        /// Chance a reel roll stops each frame when rolling (lower less likley)
        /// </summary>
        public float ProbabilityRollEnds { get; set; }

        /// <summary>
        /// Min reel roll texture coordinte speed. Speed is chosen randomly in interval
        /// </summary>
        public float RollSpeedMin { get; set; }

        /// <summary>
        /// Max reel roll texture coordinte speed. Speed is chosen randomly in interval
        /// </summary>
        public float RollSpeedMax { get; set; }

        /// <summary>
        /// Min reel roll texture coordinte acceleration. Acceleration is chosen randomly in interval
        /// </summary>
        public float RollAccelerationMin { get; set; }

        /// <summary>
        /// Max reel roll texture coordinte acceleration. Acceleration is chosen randomly in interval
        /// </summary>
        public float RollAccelerationMax { get; set; }

        /// <summary>
        /// Linear scaler to modify overal roll movement
        /// </summary>
        public float RollOverallScale { get; set; }

        /// <summary>
        /// Scales the amount of random shaking in the texture coordinate shifting of the reel rolls
        /// </summary>
        public float RollShakeFactor { get; set; }

        /// <summary>
        /// Chance over exposure flicker starts each frame (lower less likely)
        /// </summary>
        public float OverExposureProbabilityStart { get; set; }

        /// <summary>
        /// Minimum time for over exposure flicker duration. Duration chosen randomly in range
        /// </summary>
        public float OverExposureFlickerTimeMin { get; set; }

        /// <summary>
        /// Maximum time for over exposure flicker duration. Duration chosen randomly in range
        /// </summary>
        public float OverExposureFlickerTimeMax { get; set; }

        /// <summary>
        /// Minimum Over Exposure Intensity. Intensity chosen randomly in range 
        /// </summary>
        public float OverExposureIntensityMin { get; set; }

        /// <summary>
        /// Maximum Over Exposure Intensity. Intensity chosen randomly in range 
        /// </summary>
        public float OverExposureIntensityMax { get; set; }

        /// <summary>
        /// Minimum number of over exposure oscillations per flicker. Number of chosen randomly in range
        /// </summary>
        public float OverExposureOscillationsMin { get; set; }

        /// <summary>
        /// Maximum number of over exposure oscillations per flicker. Number of chosen randomly in range
        /// </summary>
        public float OverExposureOscillationsMax { get; set; }
    }

    /// <summary>
    /// Mimicks the effect of a Cathode Ray Tube Screen
    /// Particularly effective when combined with CRT Mesh shaped render surface (MeshRender stage)
    /// </summary>
    public struct CrtEffectConfiguration
    {
        /// <summary>
        /// Returns a pre-set effect configuration scaled between 0 and 1
        /// </summary>
        /// <param name="amount">0 to 1 intensity of effect (reflects size of 'pixels')</param>
        /// <param name="aspect">Aspect ratio (width / height)</param>
        /// <returns></returns>
        public static CrtEffectConfiguration PreSet(float amount, float aspect)
        {
            amount = Clamper.Clamp(amount, 0.0f, 1.0f);

            var filtersHorizontally = 96;

            return new CrtEffectConfiguration
            {
                NumRgbFiltersHorizontally = filtersHorizontally,
                NumRgbFiltersVertically = (int)(filtersHorizontally * (1.0f / aspect)),
                RgbPixelFilterAmount = amount,
                RgbPixelFilterIntensity = amount,
                SimpleScanlinesIntensity = amount

            };
        }

        /// <summary>
        /// Scalar related to how aggressively the pixels are filtered through the RGB 'sub pixels'
        /// </summary>
        public float RgbPixelFilterIntensity { get; set; }

        /// <summary>
        /// Interpolation between pre- and post-Rgb Filter effect pixels (0 to 1)
        /// </summary>
        public float RgbPixelFilterAmount { get; set; }

        /// <summary>
        /// Number of virtual CRT 3 component pixels horizontally
        /// </summary>
        public int NumRgbFiltersHorizontally { get; set; }

        /// <summary>
        /// Number of virtual CRT 3 component pixels vertically
        /// </summary>
        public int NumRgbFiltersVertically { get; set; }

        /// <summary>
        /// Interpolation between pre- and post-scan line effect pixels (0 to 1)
        /// </summary>
        public float SimpleScanlinesIntensity { get; set; }
    }

    static class Clamper
    {
        public static float Clamp(float value, float min, float max)
        {
            var clamped = value;
            if (clamped < min)
            {
                clamped = min;
            }
            if (clamped > max)
            {
                clamped = max;
            }
            return clamped;
        }
    }
}