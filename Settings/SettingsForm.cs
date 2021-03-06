﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WordLight.Settings
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            Properties.SelectedObject = AddinSettings.Instance;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

		private void btnReset_Click(object sender, EventArgs e)
		{
			AddinSettings.Instance.ResetToDefaults();
			Properties.Refresh();
		}
    }
}
