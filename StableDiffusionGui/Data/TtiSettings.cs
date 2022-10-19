﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace StableDiffusionGui.Data
{
    public enum Implementation
    { StableDiffusion, StableDiffusionOptimized }

    public class TtiSettings
    {
        public Implementation Implementation { get; set; } = Implementation.StableDiffusion;
        public string[] Prompts { get; set; } = new string[] { "" };
        public int Iterations { get; set; } = 1;
        public Dictionary<string, string> Params { get; set; } = new Dictionary<string, string>();

        public int GetTargetImgCount()
        {
            int count = 0;

            try
            {
                foreach (string prompt in Prompts)
                {
                    for (int i = 0; i < Iterations; i++)
                    {
                        foreach (string scale in Params["scales"].Replace(" ", "").Split(","))
                        {
                            foreach (string strength in Params["initStrengths"].Replace(" ", "").Split(","))
                            {
                                count++;

                                if (string.IsNullOrWhiteSpace(Params["initImg"]))
                                    break;
                            }
                        }
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public override string ToString()
        {
            string init = System.IO.File.Exists(Params["initImg"]) ? " - With Image" : "";
            string emb = System.IO.File.Exists(Params["embedding"]) ? " - With Concept" : "";
            string extraPrompts = Prompts.Length > 1 ? $" (+{Prompts.Length - 1})" : "";
            return $"\"{Prompts.FirstOrDefault().Trunc(75)}\"{extraPrompts} - {Iterations} Images - {Params["steps"]} Steps - Seed {Params["seed"]} - {Params["res"]} - Sampler {Params["sampler"]}{init}{emb}";
        }
    }
}
