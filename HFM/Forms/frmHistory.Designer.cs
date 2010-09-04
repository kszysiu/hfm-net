﻿namespace HFM.Forms
{
   partial class frmHistory
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

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
         System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
         System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmHistory));
         this.tableLayoutGridView = new System.Windows.Forms.TableLayoutPanel();
         this.dataGridView1 = new System.Windows.Forms.DataGridView();
         this.panel1 = new System.Windows.Forms.Panel();
         this.grpResults = new HFM.Classes.GroupBoxWrapper();
         this.rdoPanelProduction = new harlam357.Windows.Forms.RadioPanel();
         this.rdoStandard = new HFM.Classes.RadioButtonWrapper();
         this.rdoBonusFrame = new HFM.Classes.RadioButtonWrapper();
         this.rdoBonusDownload = new HFM.Classes.RadioButtonWrapper();
         this.lblPpdCalc = new HFM.Classes.LabelWrapper();
         this.txtShown = new HFM.Classes.TextBoxWrapper();
         this.lblView = new HFM.Classes.LabelWrapper();
         this.txtResults = new HFM.Classes.TextBoxWrapper();
         this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
         this.lblResults = new HFM.Classes.LabelWrapper();
         this.chkTop = new HFM.Classes.CheckBoxWrapper();
         this.btnRefresh = new HFM.Classes.ButtonWrapper();
         this.panel2 = new System.Windows.Forms.Panel();
         this.grpDataView = new HFM.Classes.GroupBoxWrapper();
         this.btnNew = new HFM.Classes.ButtonWrapper();
         this.cboSortView = new HFM.Classes.ComboBoxWrapper();
         this.btnDelete = new HFM.Classes.ButtonWrapper();
         this.btnEdit = new HFM.Classes.ButtonWrapper();
         this.menuStrip1 = new System.Windows.Forms.MenuStrip();
         this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuFileImportCompletedUnits = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuView = new System.Windows.Forms.ToolStripMenuItem();
         this.mnuViewAutoSizeGrid = new System.Windows.Forms.ToolStripMenuItem();
         this.tableLayoutGridView.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
         this.panel1.SuspendLayout();
         this.grpResults.SuspendLayout();
         this.rdoPanelProduction.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
         this.panel2.SuspendLayout();
         this.grpDataView.SuspendLayout();
         this.menuStrip1.SuspendLayout();
         this.SuspendLayout();
         // 
         // tableLayoutGridView
         // 
         this.tableLayoutGridView.ColumnCount = 2;
         this.tableLayoutGridView.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 337F));
         this.tableLayoutGridView.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.tableLayoutGridView.Controls.Add(this.dataGridView1, 0, 1);
         this.tableLayoutGridView.Controls.Add(this.panel1, 1, 0);
         this.tableLayoutGridView.Controls.Add(this.panel2, 0, 0);
         this.tableLayoutGridView.Dock = System.Windows.Forms.DockStyle.Fill;
         this.tableLayoutGridView.Location = new System.Drawing.Point(0, 24);
         this.tableLayoutGridView.Name = "tableLayoutGridView";
         this.tableLayoutGridView.RowCount = 2;
         this.tableLayoutGridView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
         this.tableLayoutGridView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.tableLayoutGridView.Size = new System.Drawing.Size(830, 542);
         this.tableLayoutGridView.TabIndex = 0;
         // 
         // dataGridView1
         // 
         this.dataGridView1.AllowUserToAddRows = false;
         this.dataGridView1.AllowUserToDeleteRows = false;
         this.dataGridView1.AllowUserToOrderColumns = true;
         this.dataGridView1.AllowUserToResizeRows = false;
         dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
         this.dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
         this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
         dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
         dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
         dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
         dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
         dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
         dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
         this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
         this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
         this.tableLayoutGridView.SetColumnSpan(this.dataGridView1, 2);
         dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
         dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
         dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
         dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
         dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
         dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
         this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle3;
         this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.dataGridView1.Location = new System.Drawing.Point(3, 78);
         this.dataGridView1.MultiSelect = false;
         this.dataGridView1.Name = "dataGridView1";
         this.dataGridView1.ReadOnly = true;
         this.dataGridView1.RowTemplate.Height = 18;
         this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
         this.dataGridView1.ShowCellToolTips = false;
         this.dataGridView1.Size = new System.Drawing.Size(824, 461);
         this.dataGridView1.TabIndex = 0;
         this.dataGridView1.Sorted += new System.EventHandler(this.dataGridView1_Sorted);
         this.dataGridView1.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dataGridView1_CellPainting);
         // 
         // panel1
         // 
         this.panel1.Controls.Add(this.grpResults);
         this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.panel1.Location = new System.Drawing.Point(340, 3);
         this.panel1.Name = "panel1";
         this.panel1.Size = new System.Drawing.Size(487, 69);
         this.panel1.TabIndex = 1;
         // 
         // grpResults
         // 
         this.grpResults.Controls.Add(this.rdoPanelProduction);
         this.grpResults.Controls.Add(this.lblPpdCalc);
         this.grpResults.Controls.Add(this.txtShown);
         this.grpResults.Controls.Add(this.lblView);
         this.grpResults.Controls.Add(this.txtResults);
         this.grpResults.Controls.Add(this.numericUpDown1);
         this.grpResults.Controls.Add(this.lblResults);
         this.grpResults.Controls.Add(this.chkTop);
         this.grpResults.Controls.Add(this.btnRefresh);
         this.grpResults.Dock = System.Windows.Forms.DockStyle.Fill;
         this.grpResults.Location = new System.Drawing.Point(0, 0);
         this.grpResults.Name = "grpResults";
         this.grpResults.Size = new System.Drawing.Size(487, 69);
         this.grpResults.TabIndex = 7;
         this.grpResults.TabStop = false;
         this.grpResults.Text = "Results";
         // 
         // rdoPanelProduction
         // 
         this.rdoPanelProduction.Controls.Add(this.rdoStandard);
         this.rdoPanelProduction.Controls.Add(this.rdoBonusFrame);
         this.rdoPanelProduction.Controls.Add(this.rdoBonusDownload);
         this.rdoPanelProduction.Location = new System.Drawing.Point(127, 16);
         this.rdoPanelProduction.Name = "rdoPanelProduction";
         this.rdoPanelProduction.Size = new System.Drawing.Size(342, 24);
         this.rdoPanelProduction.TabIndex = 13;
         this.rdoPanelProduction.ValueMember = null;
         // 
         // rdoStandard
         // 
         this.rdoStandard.AutoSize = true;
         this.rdoStandard.Location = new System.Drawing.Point(272, 3);
         this.rdoStandard.Name = "rdoStandard";
         this.rdoStandard.Size = new System.Drawing.Size(68, 17);
         this.rdoStandard.TabIndex = 9;
         this.rdoStandard.TabStop = true;
         this.rdoStandard.Tag = "2";
         this.rdoStandard.Text = "Standard";
         this.rdoStandard.UseVisualStyleBackColor = true;
         // 
         // rdoBonusFrame
         // 
         this.rdoBonusFrame.AutoSize = true;
         this.rdoBonusFrame.Location = new System.Drawing.Point(147, 3);
         this.rdoBonusFrame.Name = "rdoBonusFrame";
         this.rdoBonusFrame.Size = new System.Drawing.Size(119, 17);
         this.rdoBonusFrame.TabIndex = 10;
         this.rdoBonusFrame.TabStop = true;
         this.rdoBonusFrame.Tag = "1";
         this.rdoBonusFrame.Text = "Bonus (Frame Time)";
         this.rdoBonusFrame.UseVisualStyleBackColor = true;
         // 
         // rdoBonusDownload
         // 
         this.rdoBonusDownload.AutoSize = true;
         this.rdoBonusDownload.Location = new System.Drawing.Point(3, 3);
         this.rdoBonusDownload.Name = "rdoBonusDownload";
         this.rdoBonusDownload.Size = new System.Drawing.Size(138, 17);
         this.rdoBonusDownload.TabIndex = 11;
         this.rdoBonusDownload.TabStop = true;
         this.rdoBonusDownload.Tag = "0";
         this.rdoBonusDownload.Text = "Bonus (Download Time)";
         this.rdoBonusDownload.UseVisualStyleBackColor = true;
         // 
         // lblPpdCalc
         // 
         this.lblPpdCalc.AutoSize = true;
         this.lblPpdCalc.Location = new System.Drawing.Point(6, 20);
         this.lblPpdCalc.Name = "lblPpdCalc";
         this.lblPpdCalc.Size = new System.Drawing.Size(119, 13);
         this.lblPpdCalc.TabIndex = 12;
         this.lblPpdCalc.Text = "PPD/Credit Calculation:";
         // 
         // txtShown
         // 
         this.txtShown.Location = new System.Drawing.Point(180, 44);
         this.txtShown.Name = "txtShown";
         this.txtShown.ReadOnly = true;
         this.txtShown.Size = new System.Drawing.Size(65, 20);
         this.txtShown.TabIndex = 8;
         // 
         // lblView
         // 
         this.lblView.AutoSize = true;
         this.lblView.Location = new System.Drawing.Point(131, 47);
         this.lblView.Name = "lblView";
         this.lblView.Size = new System.Drawing.Size(43, 13);
         this.lblView.TabIndex = 7;
         this.lblView.Text = "Shown:";
         // 
         // txtResults
         // 
         this.txtResults.Location = new System.Drawing.Point(60, 44);
         this.txtResults.Name = "txtResults";
         this.txtResults.ReadOnly = true;
         this.txtResults.Size = new System.Drawing.Size(65, 20);
         this.txtResults.TabIndex = 6;
         // 
         // numericUpDown1
         // 
         this.numericUpDown1.Location = new System.Drawing.Point(344, 44);
         this.numericUpDown1.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
         this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
         this.numericUpDown1.Name = "numericUpDown1";
         this.numericUpDown1.Size = new System.Drawing.Size(46, 20);
         this.numericUpDown1.TabIndex = 2;
         this.numericUpDown1.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
         // 
         // lblResults
         // 
         this.lblResults.AutoSize = true;
         this.lblResults.Location = new System.Drawing.Point(6, 47);
         this.lblResults.Name = "lblResults";
         this.lblResults.Size = new System.Drawing.Size(48, 13);
         this.lblResults.TabIndex = 0;
         this.lblResults.Text = "Results: ";
         // 
         // chkTop
         // 
         this.chkTop.AutoSize = true;
         this.chkTop.Location = new System.Drawing.Point(263, 46);
         this.chkTop.Name = "chkTop";
         this.chkTop.Size = new System.Drawing.Size(75, 17);
         this.chkTop.TabIndex = 1;
         this.chkTop.Text = "Show Top";
         this.chkTop.UseVisualStyleBackColor = true;
         // 
         // btnRefresh
         // 
         this.btnRefresh.Location = new System.Drawing.Point(408, 42);
         this.btnRefresh.Name = "btnRefresh";
         this.btnRefresh.Size = new System.Drawing.Size(70, 23);
         this.btnRefresh.TabIndex = 5;
         this.btnRefresh.Text = "Refresh";
         this.btnRefresh.UseVisualStyleBackColor = true;
         this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
         // 
         // panel2
         // 
         this.panel2.Controls.Add(this.grpDataView);
         this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
         this.panel2.Location = new System.Drawing.Point(3, 3);
         this.panel2.Name = "panel2";
         this.panel2.Size = new System.Drawing.Size(331, 69);
         this.panel2.TabIndex = 2;
         // 
         // grpDataView
         // 
         this.grpDataView.Controls.Add(this.btnNew);
         this.grpDataView.Controls.Add(this.cboSortView);
         this.grpDataView.Controls.Add(this.btnDelete);
         this.grpDataView.Controls.Add(this.btnEdit);
         this.grpDataView.Dock = System.Windows.Forms.DockStyle.Fill;
         this.grpDataView.Location = new System.Drawing.Point(0, 0);
         this.grpDataView.Name = "grpDataView";
         this.grpDataView.Size = new System.Drawing.Size(331, 69);
         this.grpDataView.TabIndex = 5;
         this.grpDataView.TabStop = false;
         this.grpDataView.Text = "Data View";
         // 
         // btnNew
         // 
         this.btnNew.Location = new System.Drawing.Point(19, 15);
         this.btnNew.Name = "btnNew";
         this.btnNew.Size = new System.Drawing.Size(95, 23);
         this.btnNew.TabIndex = 2;
         this.btnNew.Text = "New";
         this.btnNew.UseVisualStyleBackColor = true;
         this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
         // 
         // cboSortView
         // 
         this.cboSortView.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cboSortView.FormattingEnabled = true;
         this.cboSortView.Location = new System.Drawing.Point(19, 44);
         this.cboSortView.Name = "cboSortView";
         this.cboSortView.Size = new System.Drawing.Size(297, 21);
         this.cboSortView.TabIndex = 0;
         this.cboSortView.SelectedIndexChanged += new System.EventHandler(this.cboSortView_SelectedIndexChanged);
         // 
         // btnDelete
         // 
         this.btnDelete.Location = new System.Drawing.Point(221, 15);
         this.btnDelete.Name = "btnDelete";
         this.btnDelete.Size = new System.Drawing.Size(95, 23);
         this.btnDelete.TabIndex = 3;
         this.btnDelete.Text = "Delete";
         this.btnDelete.UseVisualStyleBackColor = true;
         this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
         // 
         // btnEdit
         // 
         this.btnEdit.Location = new System.Drawing.Point(120, 15);
         this.btnEdit.Name = "btnEdit";
         this.btnEdit.Size = new System.Drawing.Size(95, 23);
         this.btnEdit.TabIndex = 4;
         this.btnEdit.Text = "Edit";
         this.btnEdit.UseVisualStyleBackColor = true;
         this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
         // 
         // menuStrip1
         // 
         this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.mnuView});
         this.menuStrip1.Location = new System.Drawing.Point(0, 0);
         this.menuStrip1.Name = "menuStrip1";
         this.menuStrip1.Size = new System.Drawing.Size(830, 24);
         this.menuStrip1.TabIndex = 1;
         this.menuStrip1.Text = "menuStrip1";
         // 
         // mnuFile
         // 
         this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFileImportCompletedUnits});
         this.mnuFile.Name = "mnuFile";
         this.mnuFile.Size = new System.Drawing.Size(35, 20);
         this.mnuFile.Text = "&File";
         // 
         // mnuFileImportCompletedUnits
         // 
         this.mnuFileImportCompletedUnits.Name = "mnuFileImportCompletedUnits";
         this.mnuFileImportCompletedUnits.Size = new System.Drawing.Size(215, 22);
         this.mnuFileImportCompletedUnits.Text = "Import &CompletedUnits.csv";
         this.mnuFileImportCompletedUnits.Click += new System.EventHandler(this.mnuFileImportCompletedUnits_Click);
         // 
         // mnuView
         // 
         this.mnuView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuViewAutoSizeGrid});
         this.mnuView.Name = "mnuView";
         this.mnuView.Size = new System.Drawing.Size(41, 20);
         this.mnuView.Text = "&View";
         // 
         // mnuViewAutoSizeGrid
         // 
         this.mnuViewAutoSizeGrid.Name = "mnuViewAutoSizeGrid";
         this.mnuViewAutoSizeGrid.Size = new System.Drawing.Size(195, 22);
         this.mnuViewAutoSizeGrid.Text = "Auto Size &Grid Columns";
         this.mnuViewAutoSizeGrid.Click += new System.EventHandler(this.mnuViewAutoSizeGrid_Click);
         // 
         // frmHistory
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(830, 566);
         this.Controls.Add(this.tableLayoutGridView);
         this.Controls.Add(this.menuStrip1);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.MinimumSize = new System.Drawing.Size(800, 600);
         this.Name = "frmHistory";
         this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
         this.Text = "Work Unit History";
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmHistory_FormClosing);
         this.tableLayoutGridView.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
         this.panel1.ResumeLayout(false);
         this.grpResults.ResumeLayout(false);
         this.grpResults.PerformLayout();
         this.rdoPanelProduction.ResumeLayout(false);
         this.rdoPanelProduction.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
         this.panel2.ResumeLayout(false);
         this.grpDataView.ResumeLayout(false);
         this.menuStrip1.ResumeLayout(false);
         this.menuStrip1.PerformLayout();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.TableLayoutPanel tableLayoutGridView;
      private System.Windows.Forms.DataGridView dataGridView1;
      private System.Windows.Forms.Panel panel1;
      private HFM.Classes.ComboBoxWrapper cboSortView;
      private HFM.Classes.ButtonWrapper btnDelete;
      private HFM.Classes.ButtonWrapper btnNew;
      private System.Windows.Forms.MenuStrip menuStrip1;
      private System.Windows.Forms.ToolStripMenuItem mnuFile;
      private System.Windows.Forms.ToolStripMenuItem mnuFileImportCompletedUnits;
      private System.Windows.Forms.ToolStripMenuItem mnuView;
      private System.Windows.Forms.ToolStripMenuItem mnuViewAutoSizeGrid;
      private System.Windows.Forms.Panel panel2;
      private HFM.Classes.LabelWrapper lblResults;
      private HFM.Classes.ButtonWrapper btnEdit;
      private HFM.Classes.ButtonWrapper btnRefresh;
      private System.Windows.Forms.NumericUpDown numericUpDown1;
      private HFM.Classes.CheckBoxWrapper chkTop;
      private HFM.Classes.GroupBoxWrapper grpDataView;
      private HFM.Classes.GroupBoxWrapper grpResults;
      private HFM.Classes.RadioButtonWrapper rdoBonusDownload;
      private HFM.Classes.RadioButtonWrapper rdoBonusFrame;
      private HFM.Classes.RadioButtonWrapper rdoStandard;
      private HFM.Classes.TextBoxWrapper txtShown;
      private HFM.Classes.LabelWrapper lblView;
      private HFM.Classes.TextBoxWrapper txtResults;
      private HFM.Classes.LabelWrapper lblPpdCalc;
      private harlam357.Windows.Forms.RadioPanel rdoPanelProduction;
   }
}
