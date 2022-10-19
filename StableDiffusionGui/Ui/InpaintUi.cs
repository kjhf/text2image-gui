﻿using System.Drawing;
using System.IO;
using StableDiffusionGui.Data;
using StableDiffusionGui.Forms;
using StableDiffusionGui.Io;
using StableDiffusionGui.Main;
using StableDiffusionGui.MiscUtils;
using Paths = StableDiffusionGui.Io.Paths;

namespace StableDiffusionGui.Ui
{
    internal static class InpaintingUtils
    {
        public static string MaskedImagePath
        { get { return Path.Combine(Paths.GetSessionDataPath(), "masked.png"); } }

        private static Image _currentMask;

        public static Image CurrentMask
        {
            get => _currentMask;
            set
            {
                _currentMask = value;
                Program.MainForm.UpdateInpaintUi();
            }
        }

        public static int CurrentBlurValue = -1;

        public static bool PrepareInpaintingIfEnabled(TtiSettings settings)
        {
            bool img2img = !string.IsNullOrWhiteSpace(settings.Params["initImg"]);
            bool inpaint = settings.Params["inpainting"] == "masked";

            if (img2img && inpaint)
            {
                PrepareInpainting(settings.Params["initImg"], Parser.GetSize(settings.Params["res"]));
                return true;
            }

            return false;
        }

        public static void PrepareInpainting(string initImgPath, Size targetSize)
        {
            var img = ImgUtils.ResizeImage(IoUtils.GetImage(initImgPath), targetSize.Width, targetSize.Height);

            if (CurrentMask == null)
            {
                DrawForm maskForm = new DrawForm(img);
                maskForm.ShowDialog();
                CurrentMask = maskForm.Mask;
            }

            if (CurrentMask == null)
            {
                TextToImage.Cancel("Inpainting is enabled, but no mask was used!");
                return;
            }

            if (CurrentMask.Size != img.Size)
                CurrentMask = ImgUtils.ResizeImage(CurrentMask, img.Size);

            var maskedOverlay = ImgUtils.AlphaMask(ImgUtils.MagickImgFromImage(img), ImgUtils.MagickImgFromImage(CurrentMask), true);
            maskedOverlay.Write(MaskedImagePath);
        }

        public static void DeleteMaskedImage()
        {
            IoUtils.TryDeleteIfExists(MaskedImagePath);
        }
    }
}
