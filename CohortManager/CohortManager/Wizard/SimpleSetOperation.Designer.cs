﻿namespace CohortManager.Wizard
{
    partial class SimpleSetOperation
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
            this.pbSetOperation = new System.Windows.Forms.PictureBox();
            this.ddSetOperation = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbSetOperation)).BeginInit();
            this.SuspendLayout();
            // 
            // pbSetOperation
            // 
            this.pbSetOperation.Location = new System.Drawing.Point(3, 3);
            this.pbSetOperation.Name = "pbSetOperation";
            this.pbSetOperation.Size = new System.Drawing.Size(20, 20);
            this.pbSetOperation.TabIndex = 0;
            this.pbSetOperation.TabStop = false;
            // 
            // ddSetOperation
            // 
            this.ddSetOperation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddSetOperation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddSetOperation.FormattingEnabled = true;
            this.ddSetOperation.Location = new System.Drawing.Point(29, 3);
            this.ddSetOperation.Name = "ddSetOperation";
            this.ddSetOperation.Size = new System.Drawing.Size(296, 21);
            this.ddSetOperation.TabIndex = 1;
            this.ddSetOperation.SelectedIndexChanged += new System.EventHandler(this.ddSetOperation_SelectedIndexChanged);
            // 
            // SimpleSetOperation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ddSetOperation);
            this.Controls.Add(this.pbSetOperation);
            this.Name = "SimpleSetOperation";
            this.Size = new System.Drawing.Size(328, 30);
            ((System.ComponentModel.ISupportInitialize)(this.pbSetOperation)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbSetOperation;
        private System.Windows.Forms.ComboBox ddSetOperation;
    }
}
