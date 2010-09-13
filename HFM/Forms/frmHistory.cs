﻿/*
 * HFM.NET - Work Unit History UI Form
 * Copyright (C) 2010 Ryan Harlamert (harlam357)
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; version 2
 * of the License. See the included file GPLv2.TXT.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using HFM.Classes;
using HFM.Framework;
using HFM.Instances;
using HFM.Models;

namespace HFM.Forms
{
   public interface IHistoryView : IWin32Window
   {
      void AttachPresenter(HistoryPresenter presenter);

      void DataBindModel(IHistoryPresenterModel model);

      void SetLocation(int x, int y);

      void SetSize(int width, int height);
      
      void QueryComboRefreshList(IList<QueryParameters> queryList);
      
      int QueryComboSelectedIndex { get; set; }

      QueryParameters QueryComboSelectedValue { get; }

      bool EditButtonEnabled { get; set; }
      
      bool DeleteButtonEnabled { get; set; }
      
      void DataGridSetDataSource(IList<HistoryEntry> list);

      void DataGridSetDataSource(int totalResults, IList<HistoryEntry> list);

      void ApplySort(string sortColumnName, SortOrder sortOrder);

      void Show();

      void Close();

      void BringToFront();
      
      FormWindowState WindowState { get; set; }

      bool Visible { get; set; }
   }

   // ReSharper disable InconsistentNaming
   public partial class frmHistory : FormWrapper, IHistoryView
   // ReSharper restore InconsistentNaming
   {
      private HistoryPresenter _presenter;
   
      public frmHistory(IPreferenceSet prefs)
      {
         InitializeComponent();
         SetupDataGridView(prefs);
      }

      #region IHistoryView Members

      public void AttachPresenter(HistoryPresenter presenter)
      {
         _presenter = presenter;
      }
      
      public void DataBindModel(IHistoryPresenterModel model)
      {
         rdoPanelProduction.DataSource = model;
         rdoPanelProduction.ValueMember = "ProductionView";
         chkTop.DataBindings.Add("Checked", model, "ShowTopChecked", false, DataSourceUpdateMode.OnPropertyChanged);
         numericUpDown1.DataBindings.Add("Value", model, "ShowTopValue", false, DataSourceUpdateMode.OnPropertyChanged);
      }

      public void SetLocation(int x, int y)
      {
         Location = new Point(x, y);
      }

      public void SetSize(int width, int height)
      {
         Size = new Size(width, height);
      }
      
      public void QueryComboRefreshList(IList<QueryParameters> queryList)
      {
         if (queryList.Count == 0)
         {
            throw new ArgumentException("Query list must have at least one query.");
         }
      
         var selectedIndex = cboSortView.SelectedIndex;

         var names = new List<Choice>();
         foreach (var query in queryList)
         {
            names.Add(new Choice(query.Name, query));
         }
         cboSortView.DataSource = names;
         cboSortView.DisplayMember = "Display";
         cboSortView.ValueMember = "Value";

         if (selectedIndex >= 0 && selectedIndex < cboSortView.Items.Count)
         {
            cboSortView.SelectedIndex = selectedIndex;
         }
         else
         {
            cboSortView.SelectedIndex = 0;
         }
      }

      public int QueryComboSelectedIndex
      {
         get { return cboSortView.SelectedIndex; }
         set { cboSortView.SelectedIndex = value; }
      }
      
      public QueryParameters QueryComboSelectedValue
      {
         get { return (QueryParameters)cboSortView.SelectedValue; }
      }

      public bool EditButtonEnabled
      {
         get { return btnEdit.Enabled; }
         set { btnEdit.Enabled = value; }
      }
      
      public bool DeleteButtonEnabled
      {
         get { return btnDelete.Enabled; }
         set { btnDelete.Enabled = value; }
      }
      
      public void DataGridSetDataSource(IList<HistoryEntry> list)
      {
         DataGridSetDataSource(list.Count, list);
      }

      public void DataGridSetDataSource(int totalResults, IList<HistoryEntry> list)
      {
         txtResults.Text = totalResults.ToString();
         txtShown.Text = list.Count.ToString();
         dataGridView1.DataSource = new SortableBindingList<HistoryEntry>(list);
      }
      
      public void ApplySort(string sortColumnName, SortOrder sortOrder)
      {
         if (String.IsNullOrEmpty(sortColumnName) == false &&
             dataGridView1.Columns.Contains(sortColumnName) &&
             sortOrder.Equals(SortOrder.None) == false)
         {
            // ReSharper disable AssignNullToNotNullAttribute
            if (sortOrder.Equals(SortOrder.Ascending))
            {
               dataGridView1.Sort(dataGridView1.Columns[sortColumnName], ListSortDirection.Ascending);
               dataGridView1.SortedColumn.HeaderCell.SortGlyphDirection = sortOrder;
            }
            else if (sortOrder.Equals(SortOrder.Descending))
            {
               dataGridView1.Sort(dataGridView1.Columns[sortColumnName], ListSortDirection.Descending);
               dataGridView1.SortedColumn.HeaderCell.SortGlyphDirection = sortOrder;
            }
            // ReSharper restore AssignNullToNotNullAttribute
         }
      }

      #endregion
      
      private void cboSortView_SelectedIndexChanged(object sender, EventArgs e)
      {
         _presenter.SelectQuery(cboSortView.SelectedIndex);
      }

      private void btnNew_Click(object sender, EventArgs e)
      {
         _presenter.NewQueryClick();
      }

      private void btnEdit_Click(object sender, EventArgs e)
      {
         _presenter.EditQueryClick();
      }

      private void btnDelete_Click(object sender, EventArgs e)
      {
         _presenter.DeleteQueryClick();
      }

      private void mnuFileImportCompletedUnits_Click(object sender, EventArgs e)
      {
         _presenter.ImportCompletedUnitsClick();
      }

      private void mnuViewAutoSizeGrid_Click(object sender, EventArgs e)
      {
         //for (var i = 0; i < dataGridView1.Columns.Count; i++)
         //{
         //   dataGridView1.AutoResizeColumns();
         //}
         dataGridView1.AutoResizeColumns();
      }

      private void btnRefresh_Click(object sender, EventArgs e)
      {
         _presenter.RefreshClicked();
      }

      private void frmHistory_FormClosing(object sender, FormClosingEventArgs e)
      {
         _presenter.OnViewClosing();
      }

      private void frmHistory_FormClosed(object sender, FormClosedEventArgs e)
      {
         _presenter.Close();
      }

      //private void AutoSizeColumn(int columnIndex)
      //{
      //   var font = new Font(dataGridView1.DefaultCellStyle.Font, FontStyle.Regular);

      //   SizeF s;
      //   int width = 0;

      //   for (int i = 0; i < dataGridView1.Rows.Count; i++)
      //   {
      //      if (dataGridView1.Rows[i].Cells[columnIndex].Value != null)
      //      {
      //         string formattedString = dataGridView1.Rows[i].Cells[columnIndex].FormattedValue.ToString();
      //         s = TextRenderer.MeasureText(formattedString, font);

      //         if (width < s.Width)
      //         {
      //            width = (int)(s.Width + 3);
      //         }
      //      }
      //   }

      //   if (width >= dataGridView1.Columns[columnIndex].MinimumWidth)
      //   {
      //      dataGridView1.Columns[columnIndex].Width = width;
      //   }
      //}

      private void SetupDataGridView(IPreferenceSet prefs)
      {
         // Add Column Selector
         new DataGridViewColumnSelector(dataGridView1);
      
         var names = PlatformOps.GetQueryFieldColumnNames();
      
         dataGridView1.AutoGenerateColumns = false;
         // ReSharper disable PossibleNullReferenceException
         dataGridView1.Columns.Add(QueryFieldName.ProjectID.ToString(), names[(int)QueryFieldName.ProjectID]);
         dataGridView1.Columns[QueryFieldName.ProjectID.ToString()].DataPropertyName = QueryFieldName.ProjectID.ToString();
         dataGridView1.Columns.Add(QueryFieldName.WorkUnitName.ToString(), names[(int)QueryFieldName.WorkUnitName]);
         dataGridView1.Columns[QueryFieldName.WorkUnitName.ToString()].DataPropertyName = QueryFieldName.WorkUnitName.ToString();
         dataGridView1.Columns.Add(QueryFieldName.InstanceName.ToString(), names[(int)QueryFieldName.InstanceName]);
         dataGridView1.Columns[QueryFieldName.InstanceName.ToString()].DataPropertyName = QueryFieldName.InstanceName.ToString();
         dataGridView1.Columns.Add(QueryFieldName.InstancePath.ToString(), names[(int)QueryFieldName.InstancePath]);
         dataGridView1.Columns[QueryFieldName.InstancePath.ToString()].DataPropertyName = QueryFieldName.InstancePath.ToString();
         dataGridView1.Columns.Add(QueryFieldName.Username.ToString(), names[(int)QueryFieldName.Username]);
         dataGridView1.Columns[QueryFieldName.Username.ToString()].DataPropertyName = QueryFieldName.Username.ToString();
         dataGridView1.Columns.Add(QueryFieldName.Team.ToString(), names[(int)QueryFieldName.Team]);
         dataGridView1.Columns[QueryFieldName.Team.ToString()].DataPropertyName = QueryFieldName.Team.ToString();
         dataGridView1.Columns.Add(QueryFieldName.ClientType.ToString(), names[(int)QueryFieldName.ClientType]);
         dataGridView1.Columns[QueryFieldName.ClientType.ToString()].DataPropertyName = QueryFieldName.ClientType.ToString();
         dataGridView1.Columns.Add(QueryFieldName.Core.ToString(), names[(int)QueryFieldName.Core]);
         dataGridView1.Columns[QueryFieldName.Core.ToString()].DataPropertyName = QueryFieldName.Core.ToString();
         dataGridView1.Columns.Add(QueryFieldName.CoreVersion.ToString(), names[(int)QueryFieldName.CoreVersion]);
         dataGridView1.Columns[QueryFieldName.CoreVersion.ToString()].DataPropertyName = QueryFieldName.CoreVersion.ToString();
         dataGridView1.Columns.Add(QueryFieldName.FrameTime.ToString(), names[(int)QueryFieldName.FrameTime]);
         dataGridView1.Columns[QueryFieldName.FrameTime.ToString()].DataPropertyName = QueryFieldName.FrameTime.ToString();
         dataGridView1.Columns.Add(QueryFieldName.KFactor.ToString(), names[(int)QueryFieldName.KFactor]);
         dataGridView1.Columns[QueryFieldName.KFactor.ToString()].DataPropertyName = QueryFieldName.KFactor.ToString();
         dataGridView1.Columns.Add(QueryFieldName.PPD.ToString(), names[(int)QueryFieldName.PPD]);
         dataGridView1.Columns[QueryFieldName.PPD.ToString()].DataPropertyName = QueryFieldName.PPD.ToString();
         dataGridView1.Columns[QueryFieldName.PPD.ToString()].DefaultCellStyle = new DataGridViewCellStyle { Format = prefs.PpdFormatString };
         dataGridView1.Columns.Add(QueryFieldName.DownloadDateTime.ToString(), names[(int)QueryFieldName.DownloadDateTime]);
         dataGridView1.Columns[QueryFieldName.DownloadDateTime.ToString()].DataPropertyName = QueryFieldName.DownloadDateTime.ToString();
         dataGridView1.Columns.Add(QueryFieldName.CompletionDateTime.ToString(), names[(int)QueryFieldName.CompletionDateTime]);
         dataGridView1.Columns[QueryFieldName.CompletionDateTime.ToString()].DataPropertyName = QueryFieldName.CompletionDateTime.ToString();
         dataGridView1.Columns.Add(QueryFieldName.Credit.ToString(), names[(int)QueryFieldName.Credit]);
         dataGridView1.Columns[QueryFieldName.Credit.ToString()].DataPropertyName = QueryFieldName.Credit.ToString();
         dataGridView1.Columns.Add(QueryFieldName.Frames.ToString(), names[(int)QueryFieldName.Frames]);
         dataGridView1.Columns[QueryFieldName.Frames.ToString()].DataPropertyName = QueryFieldName.Frames.ToString();
         dataGridView1.Columns.Add(QueryFieldName.FramesCompleted.ToString(), names[(int)QueryFieldName.FramesCompleted]);
         dataGridView1.Columns[QueryFieldName.FramesCompleted.ToString()].DataPropertyName = QueryFieldName.FramesCompleted.ToString();
         dataGridView1.Columns.Add(QueryFieldName.Result.ToString(), names[(int)QueryFieldName.Result]);
         dataGridView1.Columns[QueryFieldName.Result.ToString()].DataPropertyName = QueryFieldName.Result.ToString();
         dataGridView1.Columns.Add(QueryFieldName.Atoms.ToString(), names[(int)QueryFieldName.Atoms]);
         dataGridView1.Columns[QueryFieldName.Atoms.ToString()].DataPropertyName = QueryFieldName.Atoms.ToString();
         dataGridView1.Columns.Add(QueryFieldName.ProjectRun.ToString(), names[(int)QueryFieldName.ProjectRun]);
         dataGridView1.Columns[QueryFieldName.ProjectRun.ToString()].DataPropertyName = QueryFieldName.ProjectRun.ToString();
         dataGridView1.Columns.Add(QueryFieldName.ProjectClone.ToString(), names[(int)QueryFieldName.ProjectClone]);
         dataGridView1.Columns[QueryFieldName.ProjectClone.ToString()].DataPropertyName = QueryFieldName.ProjectClone.ToString();
         dataGridView1.Columns.Add(QueryFieldName.ProjectGen.ToString(), names[(int)QueryFieldName.ProjectGen]);
         dataGridView1.Columns[QueryFieldName.ProjectGen.ToString()].DataPropertyName = QueryFieldName.ProjectGen.ToString();
         // ReSharper restore PossibleNullReferenceException
      }
      
      private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
      {
         var sf = new StringFormat { Alignment = StringAlignment.Center };
         if (e.ColumnIndex < 0 && e.RowIndex >= 0 && e.RowIndex < dataGridView1.Rows.Count)
         {
            e.PaintBackground(e.ClipBounds, true);
            e.Graphics.DrawString((e.RowIndex + 1).ToString(), Font, Brushes.Black, e.CellBounds, sf);
            e.Handled = true;
         }
      }

      /// <summary>
      /// Update Form Level Sorting Fields
      /// </summary>
      private void dataGridView1_Sorted(object sender, EventArgs e)
      {
         _presenter.SaveSortSettings(dataGridView1.SortedColumn.Name, dataGridView1.SortOrder);
      }
   }
}
