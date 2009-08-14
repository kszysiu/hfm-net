/*
 * HFM.NET - Benchmarks Form Class
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
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

using HFM.Instances;
using HFM.Proteins;
using HFM.Instrumentation;

namespace HFM.Forms
{
   public partial class frmBenchmarks : Classes.FormWrapper
   {
      #region Members
      private readonly FoldingInstanceCollection _clientInstances;
      private readonly int _initialProjectID; 
      #endregion

      #region Form Constructor / functionality
      public frmBenchmarks(FoldingInstanceCollection clientInstances, int projectID)
      {
         _clientInstances = clientInstances;
         _initialProjectID = projectID;
      
         InitializeComponent();
      }

      private void frmBenchmarks_Shown(object sender, EventArgs e)
      {
         listBox1.DataSource = ProteinBenchmarkCollection.Instance.GetBenchmarkProjects();
         int index = listBox1.Items.IndexOf(_initialProjectID);
         if (index > -1)
         {
            listBox1.SelectedIndex = index;
         }
      }
      #endregion

      #region Event Handlers
      private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
      {
         txtBenchmarks.Text = String.Empty;
         int ProjectID = (int)listBox1.SelectedItem;

         string[] lines = UpdateProteinInformation(ProjectID);

         UpdateBenchmarkText(lines);

         List<InstanceProteinBenchmark> list = ProteinBenchmarkCollection.Instance.GetProjectBenchmarks(ProjectID);

         foreach (InstanceProteinBenchmark benchmark in list)
         {
            ClientInstance instance;
            _clientInstances.InstanceCollection.TryGetValue(benchmark.OwningInstanceName, out instance);
            UpdateBenchmarkText(benchmark.ToMultiLineString(instance));
         }
      }

      private void linkDescription_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
      {
         try
         {
            Process.Start(linkDescription.Text);
         }
         catch (Exception ex)
         {
            HfmTrace.WriteToHfmConsole(ex);
            MessageBox.Show(String.Format(Properties.Resources.ProcessStartError, "Project Description"));
         }
      }

      private void btnExit_Click(object sender, EventArgs e)
      {
         Close();
      } 
      #endregion

      #region Helper Routines
      private void UpdateBenchmarkText(IEnumerable<string> benchmarkLines)
      {
         List<string> lines = new List<string>(txtBenchmarks.Lines);
         lines.AddRange(benchmarkLines);
         txtBenchmarks.Lines = lines.ToArray();
      }

      private string[] UpdateProteinInformation(int ProjectID)
      {
         List<string> lines = new List<string>(5);

         Protein protein;
         ProteinCollection.Instance.TryGetValue(ProjectID, out protein);

         if (protein != null)
         {
            txtProjectID.Text = protein.WorkUnitName;
            txtCredit.Text = protein.Credit.ToString();
            txtFrames.Text = protein.Frames.ToString();
            txtAtoms.Text = protein.NumAtoms.ToString();
            txtCore.Text = protein.Core;
            linkDescription.Text = protein.Description;
            txtPreferredDays.Text = protein.PreferredDays.ToString();
            txtMaximumDays.Text = protein.MaxDays.ToString();
            txtContact.Text = protein.Contact;
            txtServerIP.Text = protein.ServerIP;

            lines.Add(String.Format(" Project ID: {0}", protein.ProjectNumber));
            lines.Add(String.Format(" Core: {0}", protein.Core));
            lines.Add(String.Format(" Credit: {0}", protein.Credit));
            lines.Add(String.Format(" Frames: {0}", protein.Frames));
            lines.Add(String.Empty);
         }
         else
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning,
                                       String.Format("{0} could not find Project ID '{1}'.", HfmTrace.FunctionName, ProjectID));
         }

         return lines.ToArray();
      }
      #endregion
   }
}
