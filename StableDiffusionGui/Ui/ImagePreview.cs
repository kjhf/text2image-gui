﻿using StableDiffusionGui.Data;
using StableDiffusionGui.Io;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StableDiffusionGui.Ui
{
    internal static class ImagePreview
    {
        public enum ImgShowMode { DontShow, ShowFirst, ShowLast }

        public static string CurrentImagePath { get { return _currentImages.Length > 0 ? _currentImages[_currIndex] : ""; } }
        public static ImageMetadata CurrentImageMetadata { get { return IoUtils.GetImageMetadata(CurrentImagePath); } }

        private static string[] _currentImages = new string[0];
        private static int _currIndex = -1;

        public static DateTime TimeOfLastImageViewerInteraction;

        /// <returns> The amount of images shown </returns>
        public static int SetImages(string imagesDir, ImgShowMode showMode, int amount = -1, string pattern = "*.png", bool recursive = false)
        {
            List<FileInfo> imgPaths = IoUtils.GetFileInfosSorted(imagesDir, recursive, pattern).OrderBy(x => x.CreationTime).Reverse().ToList(); // Find images and sort by date, newest to oldest

            if (amount > 0)
                imgPaths = imgPaths.Take(amount).ToList();

            SetImages(imgPaths.Select(x => x.FullName).ToList(), showMode);

            return imgPaths.Count;
        }

        public static void SetImages(List<string> imagePaths, ImgShowMode showMode, bool ignoreTimeout = false)
        {
            if (Enumerable.SequenceEqual(imagePaths, _currentImages))
                return;

            _currentImages = imagePaths.ToArray();

            if (showMode == ImgShowMode.DontShow)
                return;

            if (ignoreTimeout || (DateTime.Now - TimeOfLastImageViewerInteraction).TotalSeconds >= 3)
            {
                if (showMode == ImgShowMode.ShowFirst)
                    _currIndex = 0;

                if (showMode == ImgShowMode.ShowLast)
                    _currIndex = _currentImages.Length - 1;
            }

            Show();
        }

        public static void Show()
        {
            if (_currIndex < 0 || _currIndex >= _currentImages.Length)
            {
                Clear();
                return;
            }

            Program.MainForm.PictBoxImgViewer.Text = "";
            Program.MainForm.PictBoxImgViewer.Image = IoUtils.GetImage(_currentImages[_currIndex]);
            ImagePopup.UpdateSlideshow(Program.MainForm.PictBoxImgViewer.Image);

            var meta = CurrentImageMetadata;

            List<string> infos = new List<string>();

            if (meta.Seed >= 0)
                infos.Add($"Seed {meta.Seed}");

            if (meta.Scale >= 0)
                infos.Add($"Scale {meta.Scale.ToStringDot()}");

            if (!meta.GeneratedResolution.IsEmpty)
                infos.Add($"{meta.GeneratedResolution.Width}x{meta.GeneratedResolution.Height}");

            if (meta.InitStrength > 0.0001f)
                infos.Add($"Strength {meta.InitStrength.ToStringDot()}");

            if (!string.IsNullOrWhiteSpace(meta.Sampler))
                infos.Add($"{meta.Sampler}");

            Program.MainForm.OutputImgLabel.Text = $"Image {_currIndex + 1}/{_currentImages.Length} {(infos.Count > 0 ? $" - {string.Join(" - ", infos)}" : "")}";
        }

        public static void Clear()
        {
            Program.MainForm.PictBoxImgViewer.Text = "";
            Program.MainForm.PictBoxImgViewer.Image = null;
            Program.MainForm.OutputImgLabel.Text = "No images to show.";
        }

        public static void Move(bool previous = false)
        {
            TimeOfLastImageViewerInteraction = DateTime.Now;

            if (!previous)
            {
                _currIndex++;

                if (_currIndex >= _currentImages.Length)
                    _currIndex = 0;
            }
            else
            {
                _currIndex--;

                if (_currIndex < 0)
                    _currIndex = _currentImages.Length - 1;
            }

            Show();
        }

        public static void OpenCurrent ()
        {
            Process.Start(CurrentImagePath);
        }

        public static void OpenFolderOfCurrent()
        {
            Process.Start("explorer", $@"/select, {CurrentImagePath.Wrap()}");
        }

        public static void DeleteCurrent()
        {
            IoUtils.TryDeleteIfExists(CurrentImagePath);
            _currentImages = _currentImages.Where(x => File.Exists(x)).ToArray();
            Move(true);
        }

        public static void DeleteAll()
        {
            var parentDirs = _currentImages.Select(x => x.GetParentDirOfFile());

            _currentImages.ToList().ForEach(x => IoUtils.TryDeleteIfExists(x));
            _currentImages = _currentImages.Where(x => File.Exists(x)).ToArray();

            if (_currentImages.Length > 0)
                Move(true);
            else
                Clear();

            parentDirs.Where(dir => IoUtils.GetAmountOfFiles(dir, true) == 0).ToList().ForEach(dir => IoUtils.TryDeleteIfExists(dir)); // Delete dir if it's now empty
        }
    }
}
