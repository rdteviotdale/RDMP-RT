﻿namespace Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
{
    partial class ArgumentValueTextUI
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbText = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tbText
            // 
            this.tbText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbText.Location = new System.Drawing.Point(3, 5);
            this.tbText.Name = "tbText";
            this.tbText.Size = new System.Drawing.Size(541, 20);
            this.tbText.TabIndex = 0;
            this.tbText.TextChanged += new System.EventHandler(this.tbText_TextChanged);
            // 
            // ArgumentValueTextUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbText);
            this.Name = "ArgumentValueTextUI";
            this.Size = new System.Drawing.Size(547, 28);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbText;


    }
}
