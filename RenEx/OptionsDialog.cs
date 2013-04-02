﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using libWyvernzora.IO;
using libWyvernzora.Utilities;

namespace RenEx
{
    public partial class OptionsDialog : Form
    {
        public OptionsDialog(Int32 mode = -1, String arg = null)
        {
            // WinForms default call
            InitializeComponent();

            // Attach Handlers
            Closing += (@s, e) =>
                {
                    // Apply all pending changes
                    if (ApplyExtConfig()) e.Cancel = true;

                    // Save Config File
                    MainForm.SaveConfiguration();
                };
            AttachExtensionAnalysisHandlers();
            AttachRuleTemplateHandlers();

            // Populate UI
            UpdateExtConfigList(Configuration.Instance.CurrentExtensionSettings);
            //ShowExtensionSetting(Configuration.Instance.GetCurrentExtensionSettings());

            if (mode >= 0)
                tabControl1.SelectTab(mode);

            if (arg != null)
            {
                // TODO Apply argument

            }
        }

        #region Extension Analysis Tab

        private void AttachExtensionAnalysisHandlers()
        {
            // Enable/Disable Validators
            radExtLenVal.CheckedChanged += (@s, e) =>
                { nudExtLenMin.Enabled = nudExtLenMax.Enabled = radExtLenVal.Checked; };
            radExtListVal.CheckedChanged += (@s, e) =>
                { txtExtList.Enabled = radExtListVal.Checked; };

            // Preset List
            lbExtPresets.SelectedIndexChanged += (@s, e) =>
                {
                    if (lbExtPresets.SelectedItem != null)
                        ShowExtensionSetting((Configuration.ExtensionConfig)lbExtPresets.SelectedItem);

                    gbExtOptions.Enabled = lbExtPresets.SelectedItem != null;
                    btnExtRemove.Enabled =
                        !Configuration.StringComparer.Equals(lbExtPresets.SelectedItem.ToString(), "Default");
                };

            // Apply Changes
            txtExtName.Leave += (@s, e) => ApplyExtConfig();
            txtExtList.Leave += (@s, e) => ApplyExtConfig();
            radExtLenVal.Leave += (@s, e) => ApplyExtConfig();
            radExtListVal.Leave += (@s, e) => ApplyExtConfig();
            nudExtMaxExt.Leave += (@s, e) => ApplyExtConfig();
            nudExtLenMin.Leave += (@s, e) => ApplyExtConfig();
            nudExtLenMax.Leave += (@s, e) => ApplyExtConfig();

            // Add/Remove
            btnExtAdd.Click += (@s, e) =>
                {
                    String nname = "New Preset #" +
                                   libWyvernzora.Core.DirectIntConv.ToHexString(Configuration.Random.Next(), 8)
                                                .Substring(2);
                    
                    Configuration.ExtensionConfig config = Configuration.ExtensionConfig.Default;
                    config.Name = nname;
                    Configuration.Instance.ExtensionConfigs.Add(nname, config);
                    UpdateExtConfigList(nname);
                    txtExtName.Select();
                    txtExtName.SelectAll();
                };
            btnExtRemove.Click += (@s, e) =>
                {
                    Configuration.ExtensionConfig config = lbExtPresets.SelectedItem as Configuration.ExtensionConfig;
                    if (config == null) return;

                    Configuration.Instance.ExtensionConfigs.Remove(config.Name);
                    if (Configuration.StringComparer.Equals(Configuration.Instance.CurrentExtensionSettings, config.Name))
                        Configuration.Instance.CurrentExtensionSettings = "Default";
                    UpdateExtConfigList(Configuration.Instance.CurrentExtensionSettings);
                };
        }

        private void UpdateExtConfigList(String selection = null)
        {
             
            using (new ActionLock(lbExtPresets.BeginUpdate, lbExtPresets.EndUpdate))
            {
                lbExtPresets.Items.Clear();
                foreach (var k in Configuration.Instance.ExtensionConfigs.Values)
                {
                    lbExtPresets.Items.Add(k);
                    if (selection != null && Configuration.StringComparer.Equals(k.Name, selection))
                        lbExtPresets.SelectedItem = k;
                }
            }
        }
        
        private void ShowExtensionSetting(Configuration.ExtensionConfig config)
        {

            txtExtName.Enabled = !Configuration.StringComparer.Equals(config.Name, "Default");
            txtExtName.Text = config.Name;
            nudExtMaxExt.Value = config.MaximumExtensions;

            if (config.Validator is FileExtLengthValidator)
            {
                var validator = (FileExtLengthValidator) config.Validator;
                radExtLenVal.Checked = true;
                nudExtLenMin.Value = validator.MinimumLength;
                nudExtLenMax.Value = validator.MaximumLength;

                // Set defaults
                txtExtList.Clear();
            }
            else
            {
                var validator = (FileExtListValidator) config.Validator;
                radExtListVal.Checked = true;
                txtExtList.Text = String.Join(";", validator.Extensions);

                // Set defaults
                nudExtLenMin.Value = 0;
                nudExtLenMax.Value = 4;
            }
        }

        private Configuration.ExtensionConfig GetConfigurationFromUi()
        {
            Configuration.ExtensionConfig config = new Configuration.ExtensionConfig();

            config.Name = txtExtName.Text;
            config.MaximumExtensions = (Int32) nudExtMaxExt.Value;

            if (radExtLenVal.Checked)
                config.Validator = new FileExtLengthValidator((Int32) nudExtLenMin.Value, (Int32) nudExtLenMax.Value);
            else
                config.Validator = new FileExtListValidator(from ext in txtExtList.Text.Split(';') select ext.Trim(' '));

            return config;
        }

        private Boolean ApplyExtConfig()
        {
            Configuration.ExtensionConfig oldc = (Configuration.ExtensionConfig) lbExtPresets.SelectedItem;
            Configuration.ExtensionConfig newc = GetConfigurationFromUi();

            // Validate Data
            if (String.IsNullOrWhiteSpace(newc.Name))
            {
                error.SetError(txtExtName, "Preset name cannot be empty or all whitespaces!");
                return true;
            }

            // Check duplicates
            if (!Configuration.StringComparer.Equals(oldc.Name, newc.Name)
                && Configuration.Instance.ExtensionConfigs.ContainsKey(newc.Name))
            {
                DialogResult dr = MessageBox.Show(
                    "There is already a preset with exactly the same name.\nDo you want to overwrite it?",
                    "Overwrite?!", MessageBoxButtons.YesNoCancel);
                if (dr == DialogResult.No) return false;
                else if (dr == DialogResult.Cancel) return true;
            }

            Configuration.Instance.ExtensionConfigs.Remove(oldc.Name);
            Configuration.Instance.ExtensionConfigs.Add(newc.Name, newc);
            UpdateExtConfigList(newc.Name);
            return false;
        }

        #endregion

        #region Rule Templates Tab

        public void AttachRuleTemplateHandlers()
        {
            
        }

        private void UpdateRuleTemplateList(String selection = null)
        {
            
        }

        #endregion
    }
}