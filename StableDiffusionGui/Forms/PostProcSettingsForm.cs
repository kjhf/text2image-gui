using StableDiffusionGui.Installation;
using StableDiffusionGui.Io;
using StableDiffusionGui.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace StableDiffusionGui.Forms
{
    public partial class PostProcSettingsForm : Form
    {
        public enum UpscaleOption { Disabled, X2, X3, X4 }
        public enum FaceRestoreOption { Disabled, Gfpgan, CodeFormer }

        public Dictionary<string, string> UiStrings = new Dictionary<string, string>();

        public PostProcSettingsForm()
        {
            UiStrings.Add(nameof(UpscaleOption.X2), "2x");
            UiStrings.Add(nameof(UpscaleOption.X3), "3x");
            UiStrings.Add(nameof(UpscaleOption.X4), "4x");
            UiStrings.Add(nameof(FaceRestoreOption.Gfpgan), "GFPGAN");
            UiStrings.Add(nameof(FaceRestoreOption.CodeFormer), "CodeFormer");

            InitializeComponent();
        }

        private void PostProcSettingsForm_Load(object sender, EventArgs e)
        {

        }

        private void PostProcSettingsForm_Shown(object sender, EventArgs e)
        {
            comboxUpscale.FillFromEnum<UpscaleOption>(UiStrings);
            comboxFaceRestoration.FillFromEnum<FaceRestoreOption>(UiStrings);

            LoadSettings();

            if (!InstallationStatus.HasSdUpscalers())
            {
                var dialogResult = UiUtils.ShowMessageBox("Upscalers are not installed.\nDo you want to open the installer to install them (Up to 1 GB of disk space required)?", "Error", MessageBoxButtons.YesNo);

                if (dialogResult == DialogResult.Yes)
                    new InstallerForm().ShowDialog();

                Close();
            }
        }

        private void PostProcSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        void LoadSettings()
        {
            ConfigParser.LoadComboxIndex(comboxUpscale);
            ConfigParser.LoadComboxIndex(comboxFaceRestoration);
            ConfigParser.LoadGuiElement(sliderFaceRestoreStrength, ConfigParser.SaveValueAs.Divided, 20f); sliderFaceRestoreStrength_Scroll(null, null);
            ConfigParser.LoadGuiElement(sliderCodeformerFidelity, ConfigParser.SaveValueAs.Divided, 20f); sliderCodeformerFidelity_Scroll(null, null);
        }

        void SaveSettings()
        {
            ConfigParser.SaveComboxIndex(comboxUpscale);
            ConfigParser.SaveComboxIndex(comboxFaceRestoration);
            ConfigParser.SaveGuiElement(sliderFaceRestoreStrength, ConfigParser.SaveValueAs.Divided, 20f);
            ConfigParser.SaveGuiElement(sliderCodeformerFidelity, ConfigParser.SaveValueAs.Divided, 20f);
        }

        private void sliderFaceRestoreStrength_Scroll(object sender, ScrollEventArgs e)
        {
            float strength = sliderFaceRestoreStrength.Value / 20f;
            PostProcUi.CurrentGfpganStrength = strength;
            labelFaceRestoreStrength.Text = strength.ToString();
        }

        private void comboxFaceRestoration_SelectedIndexChanged(object sender, EventArgs e)
        {
            panelFaceRestorationStrength.Visible = (FaceRestoreOption)comboxFaceRestoration.SelectedIndex != FaceRestoreOption.Disabled;
            panelCodeformerFidelity.Visible = (FaceRestoreOption)comboxFaceRestoration.SelectedIndex == FaceRestoreOption.CodeFormer;
        }

        private void sliderCodeformerFidelity_Scroll(object sender, ScrollEventArgs e)
        {
            float strength = sliderCodeformerFidelity.Value / 20f;
            PostProcUi.CurrentCfFidelity = strength;
            labelCodeformerFidelity.Text = strength.ToString();
        }
    }
}
