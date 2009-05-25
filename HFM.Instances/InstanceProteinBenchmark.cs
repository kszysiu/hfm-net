/*
 * HFM.NET - Benchmark Data Class
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
using HFM.Proteins;
using Debug=HFM.Instrumentation.Debug;

namespace HFM.Instances
{
   [Serializable]
   public class InstanceProteinBenchmark
   {
      private const Int32 MaxFrames = 300;
      
      #region Owner Data Properties
      /// <summary>
      /// Name of the Client Instance that owns this UnitInfo
      /// </summary>
      private string _OwningInstanceName;
      /// <summary>
      /// Name of the Client Instance that owns this UnitInfo
      /// </summary>
      public string OwningInstanceName
      {
         get { return _OwningInstanceName; }
         set { _OwningInstanceName = value; }
      }

      /// <summary>
      /// Path of the Client Instance that owns this UnitInfo
      /// </summary>
      private string _OwningInstancePath;
      /// <summary>
      /// Path of the Client Instance that owns this UnitInfo
      /// </summary>
      public string OwningInstancePath
      {
         get { return _OwningInstancePath; }
         set { _OwningInstancePath = value; }
      }
      #endregion
   
      private readonly Int32 _ProjectID;
      public Int32 ProjectID
      {
         get { return _ProjectID; }
      }
   
      private TimeSpan _MinimumFrameTime;
      public TimeSpan MinimumFrameTime
      {
         get 
         { 
            if (_MinimumFrameTime == TimeSpan.MaxValue)
            {
               return TimeSpan.Zero;
            }
            return _MinimumFrameTime;
         }
      }
      
      public TimeSpan AverageFrameTime
      {
         get 
         { 
            if (_FrameTimes.Count > 0)
            {
               int totalSeconds = 0;
               foreach (TimeSpan time in _FrameTimes)
               {
                  totalSeconds += Convert.ToInt32(time.TotalSeconds);
               }

               return TimeSpan.FromSeconds(totalSeconds / _FrameTimes.Count);
            }
            
            return TimeSpan.Zero;
         }
      }

      private readonly Queue<TimeSpan> _FrameTimes;
      public Queue<TimeSpan> FrameTimes
      {
         get { return _FrameTimes; }
      }

      public void SetFrameTime(TimeSpan frameTime)
      {
         if (frameTime > TimeSpan.Zero)
         {
            if (frameTime < _MinimumFrameTime || _MinimumFrameTime.Equals(TimeSpan.Zero))
            {
               _MinimumFrameTime = frameTime;
            }

            // Dequeue once we have the Maximum number of frame times
            if (_FrameTimes.Count == MaxFrames)
            {
               _FrameTimes.Dequeue();
            }
            _FrameTimes.Enqueue(frameTime);
         }
      }
   
      public InstanceProteinBenchmark(string ownerName, string ownerPath, Int32 proteinID)
      {
         _OwningInstanceName = ownerName;
         _OwningInstancePath = ownerPath;
         _ProjectID = proteinID;
         _MinimumFrameTime = TimeSpan.Zero;
         _FrameTimes = new Queue<TimeSpan>(MaxFrames);
      }
      
      public string[] ToMultiLineString(ClientInstance instance)
      {
         Protein protein;
         ProteinCollection.Instance.TryGetValue(_ProjectID, out protein);
         
         List<string> output = new List<string>(10);
 
         if (protein != null)
         {
            output.Add(String.Empty);
            output.Add(String.Format(" Name: {0}", _OwningInstanceName));
            output.Add(String.Format(" Path: {0}", _OwningInstancePath));
            output.Add(String.Format(" Number of Frames Observed: {0}", _FrameTimes.Count));
            output.Add(String.Empty);
            output.Add(String.Format(" Min. Time / Frame : {0} - {1} PPD", _MinimumFrameTime, Math.Round(protein.GetPPD(_MinimumFrameTime), 1)));
            output.Add(String.Format(" Avg. Time / Frame : {0} - {1} PPD", AverageFrameTime, Math.Round(protein.GetPPD(AverageFrameTime), 1)));
            
            TimeSpan currentFrameTime = TimeSpan.Zero;
            TimeSpan threeFrameTime = TimeSpan.Zero;
            TimeSpan allFrameTime = TimeSpan.Zero;
            TimeSpan effectFrameTime = TimeSpan.Zero;
            if (instance != null && instance.CurrentUnitInfo.ProjectID == _ProjectID)
            {
               currentFrameTime = TimeSpan.FromSeconds(instance.CurrentUnitInfo.RawTimePerLastSection);
               threeFrameTime = TimeSpan.FromSeconds(instance.CurrentUnitInfo.RawTimePerThreeSections);
               allFrameTime = TimeSpan.FromSeconds(instance.CurrentUnitInfo.RawTimePerAllSections);
               effectFrameTime = TimeSpan.FromSeconds(instance.CurrentUnitInfo.RawTimePerUnitDownload);
            }
            output.Add(String.Format(" Cur. Time / Frame : {0} - {1} PPD", currentFrameTime, Math.Round(protein.GetPPD(currentFrameTime), 1)));
            output.Add(String.Format(" R3F. Time / Frame : {0} - {1} PPD", threeFrameTime, Math.Round(protein.GetPPD(threeFrameTime), 1)));
            output.Add(String.Format(" All  Time / Frame : {0} - {1} PPD", allFrameTime, Math.Round(protein.GetPPD(allFrameTime), 1)));
            output.Add(String.Format(" Eff. Time / Frame : {0} - {1} PPD", effectFrameTime, Math.Round(protein.GetPPD(effectFrameTime), 1)));
            output.Add(String.Empty);
         }
         else
         {
            Debug.WriteToHfmConsole(TraceLevel.Warning,
                                    String.Format("{0} could not find Project ID '{1}'.", Debug.FunctionName, _ProjectID));
         }
         
         return output.ToArray();
      }
   }
}
