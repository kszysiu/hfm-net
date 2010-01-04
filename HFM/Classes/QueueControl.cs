﻿/*
 * HFM.NET - Queue Control
 * Copyright (C) 2009 Ryan Harlamert (harlam357)
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
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.
 */

using System;
using System.Globalization;
using System.Windows.Forms;

using HFM.Framework;
using HFM.Queue;
using HFM.Proteins;

namespace HFM.Classes
{
   public sealed partial class QueueControl : UserControl
   {
      private enum QueueControlRows
      {
         IndexCombo = 0,
         Blank1,
         Status,
         Credit,
         BeginDate,
         EndDate,
         SpeedFactor,
         PerfFraction,
         MegaFlops,
         Server,
         AvgDownload,
         AvgUpload,
         CpuType,
         OS,
         Memory,
         GpuMemory,
         Benchmark,
         SmpCores,
         CoresToUse,
         UserId,
         MachineId
      }
   
      public event EventHandler<QueueIndexChangedEventArgs> QueueIndexChanged;
   
      private QueueReader _qr = null;
      private ClientType _ClientType = ClientType.Unknown;
      private bool _ClientIsOnVirtualMachine;
      
      private const int DefaultRowHeight = 23;
   
      public QueueControl()
      {
         InitializeComponent();
      }
      
      private void OnQueueIndexChanged(QueueIndexChangedEventArgs e)
      {
         if (QueueIndexChanged != null)
         {
            QueueIndexChanged(this, e);
         }
      }
      
      [CLSCompliant(false)]
      public void SetQueue(QueueReader qr)
      {
         SetQueue(qr, ClientType.Unknown, false);
      }

      [CLSCompliant(false)]
      public void SetQueue(QueueReader qr, ClientType type, bool vm)
      {
         if (qr != null && qr.QueueReadOk)
         {
            _qr = qr;
            _ClientType = type;
            _ClientIsOnVirtualMachine = vm;
            
            cboQueueIndex.SelectedIndexChanged -= cboQueueIndex_SelectedIndexChanged;
            cboQueueIndex.DataSource = _qr.EntryNameCollection;
            cboQueueIndex.SelectedIndex = -1;
            cboQueueIndex.SelectedIndexChanged += cboQueueIndex_SelectedIndexChanged;
            
            cboQueueIndex.SelectedIndex = (int)_qr.CurrentIndex;
         }
         else
         {
            _qr = null;
            _ClientType = ClientType.Unknown;
            _ClientIsOnVirtualMachine = false;
            SetControlsVisible(false);
         }
      }

      private void cboQueueIndex_SelectedIndexChanged(object sender, EventArgs e)
      {
         if ((_qr != null && _qr.QueueReadOk) == false) return;
      
         if (cboQueueIndex.SelectedIndex > -1)
         {
            SetControlsVisible(true);
         
            QueueEntry entry = _qr.GetQueueEntry((uint)cboQueueIndex.SelectedIndex);
            txtStatus.Text = entry.EntryStatus.ToString();
            txtCredit.Text = ProteinCollection.Instance.ContainsKey(entry.ProjectID) ? ProteinCollection.Instance[entry.ProjectID].Credit.ToString(CultureInfo.CurrentCulture) : "0";
            if (_ClientIsOnVirtualMachine)
            {
               txtBeginDate.Text = String.Format("{0} {1}", entry.BeginTimeUtc.ToShortDateString(), entry.BeginTimeUtc.ToShortTimeString());
            }
            else
            {
               txtBeginDate.Text = String.Format("{0} {1}", entry.BeginTimeLocal.ToShortDateString(), entry.BeginTimeLocal.ToShortTimeString());
            }
            if (entry.EndTimeUtc.Equals(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)))
            {
               txtEndDate.Text = "(Not Completed)";
            }
            else
            {
               if (_ClientIsOnVirtualMachine)
               {
                  txtEndDate.Text = String.Format("{0} {1}", entry.EndTimeUtc.ToShortDateString(), entry.EndTimeUtc.ToShortTimeString());
               }
               else
               {
                  txtEndDate.Text = String.Format("{0} {1}", entry.EndTimeLocal.ToShortDateString(), entry.EndTimeLocal.ToShortTimeString());
               }
               
            }
            txtSpeedFactor.Text = String.Format(CultureInfo.CurrentCulture, "{0} x min speed", entry.SpeedFactor);
            txtPerformanceFraction.Text = String.Format(CultureInfo.CurrentCulture, "{0} (u={1})", _qr.PerformanceFraction, _qr.PerformanceFractionUnitWeight);
            txtMegaFlops.Text = String.Format(CultureInfo.CurrentCulture, "{0:f}", entry.MegaFlops);
            txtServer.Text = entry.ServerIP;
            txtAverageDownloadRate.Text = String.Format(CultureInfo.CurrentCulture, "{0} KB/s (u={1})", _qr.DownloadRateAverage, _qr.DownloadRateUnitWeight);
            txtAverageUploadRate.Text = String.Format(CultureInfo.CurrentCulture, "{0} KB/s (u={1})", _qr.UploadRateAverage, _qr.UploadRateUnitWeight);
            txtCpuType.Text = entry.CpuString;
            txtOsType.Text = entry.OsString;
            txtMemory.Text = entry.Memory.ToString(CultureInfo.CurrentCulture);
            txtGpuMemory.Text = entry.GpuMemory.ToString(CultureInfo.CurrentCulture);
            txtBenchmark.Text = entry.Benchmark.ToString(CultureInfo.CurrentCulture);
            txtSmpCores.Text = entry.NumberOfSmpCores.ToString(CultureInfo.CurrentCulture);
            txtCoresToUse.Text = entry.UseCores.ToString(CultureInfo.CurrentCulture);
            txtUserID.Text = entry.UserID;
            txtMachineID.Text = entry.MachineID.ToString(CultureInfo.CurrentCulture);
            
            #region Test TextBox Code (commented)
            //txtStatus.Text = "Status";
            //txtCredit.Text = "Credit";
            //txtBeginDate.Text = "BeginDate";
            //txtEndDate.Text = "EndDate";
            //txtSpeedFactor.Text = "SpeedFactor";
            //txtPerformanceFraction.Text = "PerfFraction";
            //txtMegaFlops.Text = "MegaFlops";
            //txtServer.Text = "Server";
            //txtAverageDownloadRate.Text = "AvgDownload";
            //txtAverageUploadRate.Text = "AvgUpload";
            //txtCpuType.Text = "CpuType";
            //txtOsType.Text = "OsType";
            //txtMemory.Text = "Memory";
            //txtGpuMemory.Text = "GpuMemory";
            //txtBenchmark.Text = "Benchmark";
            //txtSmpCores.Text = "SmpCores";
            //txtCoresToUse.Text = "CoresToUse";
            //txtUserID.Text = "UserID";
            //txtMachineID.Text = "MachineID";
            #endregion
            
            OnQueueIndexChanged(new QueueIndexChangedEventArgs(cboQueueIndex.SelectedIndex));
         }
         else
         {
            // hide controls and display queue not available message
            SetControlsVisible(false);

            OnQueueIndexChanged(new QueueIndexChangedEventArgs(-1));
         }
      }
      
      private void SetControlsVisible(bool visible)
      {
         if (visible == false)
         {
            cboQueueIndex.DataSource = null;
            cboQueueIndex.Items.Clear();
            cboQueueIndex.Items.Add("No Queue Data");
            cboQueueIndex.SelectedIndex = 0;
         }

         txtStatus.Visible = visible;
         txtCredit.Visible = visible;
         txtBeginDate.Visible = visible;
         txtEndDate.Visible = visible;
         txtSpeedFactor.Visible = visible;
         txtPerformanceFraction.Visible = visible;
         txtMegaFlops.Visible = visible;
         txtServer.Visible = visible;
         txtAverageDownloadRate.Visible = visible;
         txtAverageUploadRate.Visible = visible;
         txtCpuType.Visible = visible;
         txtOsType.Visible = visible;
         txtMemory.Visible = visible;
         txtGpuMemory.Visible = visible;
         txtBenchmark.Visible = visible;
         txtSmpCores.Visible = visible;
         txtCoresToUse.Visible = visible;
         txtUserID.Visible = visible;
         txtMachineID.Visible = visible;
         
         if (visible == false)
         {
            tableLayoutPanel1.RowStyles[(int)QueueControlRows.GpuMemory].Height = 0;
            tableLayoutPanel1.RowStyles[(int)QueueControlRows.Benchmark].Height = DefaultRowHeight;
            tableLayoutPanel1.RowStyles[(int)QueueControlRows.SmpCores].Height = 0;
            tableLayoutPanel1.RowStyles[(int)QueueControlRows.CoresToUse].Height = 0;
         }
         else
         {
            switch (_ClientType)
            {
               case ClientType.Unknown:
               case ClientType.Standard:
                  // Set Rows to Zero Height and Hide Labels First
                  txtGpuMemory.Visible = false;
                  tableLayoutPanel1.RowStyles[(int)QueueControlRows.GpuMemory].Height = 0;
                  txtSmpCores.Visible = false;
                  tableLayoutPanel1.RowStyles[(int)QueueControlRows.SmpCores].Height = 0;
                  txtCoresToUse.Visible = false;
                  tableLayoutPanel1.RowStyles[(int)QueueControlRows.CoresToUse].Height = 0;
                  // Then Show the Client Specific Queue Row(s)
                  txtBenchmark.Visible = true;
                  tableLayoutPanel1.RowStyles[(int)QueueControlRows.Benchmark].Height = DefaultRowHeight;
                  break;
               case ClientType.GPU:
                  // Set Rows to Zero Height and Hide Labels First
                  txtBenchmark.Visible = false;
                  tableLayoutPanel1.RowStyles[(int)QueueControlRows.Benchmark].Height = 0;
                  txtSmpCores.Visible = false;
                  tableLayoutPanel1.RowStyles[(int)QueueControlRows.SmpCores].Height = 0;
                  txtCoresToUse.Visible = false;
                  tableLayoutPanel1.RowStyles[(int)QueueControlRows.CoresToUse].Height = 0;
                  // Then Show the Client Specific Queue Row(s)
                  txtGpuMemory.Visible = true;
                  tableLayoutPanel1.RowStyles[(int)QueueControlRows.GpuMemory].Height = DefaultRowHeight;
                  break;
               case ClientType.SMP:
                  // Set Rows to Zero Height and Hide Labels First
                  txtGpuMemory.Visible = false;
                  tableLayoutPanel1.RowStyles[(int)QueueControlRows.GpuMemory].Height = 0;
                  txtBenchmark.Visible = false;
                  tableLayoutPanel1.RowStyles[(int)QueueControlRows.Benchmark].Height = 0;
                  // Then Show the Client Specific Queue Row(s)
                  txtCoresToUse.Visible = true;
                  tableLayoutPanel1.RowStyles[(int)QueueControlRows.SmpCores].Height = DefaultRowHeight;
                  txtSmpCores.Visible = true;
                  tableLayoutPanel1.RowStyles[(int)QueueControlRows.CoresToUse].Height = DefaultRowHeight;
                  break;
            }
         }
      }
   }
   
   public class QueueIndexChangedEventArgs : EventArgs
   {
      private readonly int _index;
      
      public int Index
      {
         get { return _index; }
      }
      
      public QueueIndexChangedEventArgs(int index)
      {
         _index = index;
      }
   }
}
