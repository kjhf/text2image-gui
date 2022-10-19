using System;

namespace StableDiffusionGui.Main
{
    public static class Enums
    {
        public static class Models
        {
            public enum Format { Fp16, Fp32 }
        }

        public static class StableDiffusion
        {
            public enum Sampler { K_Euler_A, K_Euler, K_Lms, Ddim, Plms, K_Heun, K_Dpm_2, K_Dpm_2_A }
        }

        public static class Dreambooth
        {
            public enum TrainPreset { VeryHighQuality, HighQuality, MedQuality, LowQuality }
        }

        public static class Cuda
        {
            public enum Device { Automatic, Cpu }
        }
    }
}
