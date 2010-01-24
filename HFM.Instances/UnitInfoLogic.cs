﻿/*
 * HFM.NET - Unit Info Logic Class
 * Copyright (C) 2009-2010 Ryan Harlamert (harlam357)
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

using HFM.Framework;
using HFM.Instrumentation;

namespace HFM.Instances
{
   public class UnitInfoLogic : IUnitInfoLogic
   {
      /// <summary>
      /// Preferences Interface
      /// </summary>
      private readonly IPreferenceSet _Prefs;
      /// <summary>
      /// Protein Collection Interface
      /// </summary>
      private readonly IProteinCollection _proteinCollection;
      
      private UnitInfo _unitInfo;
      /// <summary>
      /// Unit Info Data Class
      /// </summary>
      public UnitInfo UnitInfoData
      {
         get { return _unitInfo; }
         set { _unitInfo = value; }
      }

      #region Owner Data Properties
      /// <summary>
      /// Name of the Client Instance that owns this UnitInfo
      /// </summary>
      public string OwningInstanceName
      {
         get { return _unitInfo.OwningInstanceName; }
         set { _unitInfo.OwningInstanceName = value; }
      }

      /// <summary>
      /// Path of the Client Instance that owns this UnitInfo
      /// </summary>
      public string OwningInstancePath
      {
         get { return _unitInfo.OwningInstancePath; }
         set { _unitInfo.OwningInstancePath = value; }
      }
      #endregion

      #region Retrieval Time Property
      /// <summary>
      /// Local time the logs used to generate this UnitInfo were retrieved
      /// </summary>
      public DateTime UnitRetrievalTime
      {
         get { return _unitInfo.UnitRetrievalTime; }
         set { _unitInfo.UnitRetrievalTime = value; }
      }
      #endregion

      #region Folding ID and Team Properties
      /// <summary>
      /// The Folding ID (Username) attached to this work unit
      /// </summary>
      public string FoldingID
      {
         get { return _unitInfo.FoldingID; }
         set { _unitInfo.FoldingID = value; }
      }

      /// <summary>
      /// The Team number attached to this work unit
      /// </summary>
      public Int32 Team
      {
         get { return _unitInfo.Team; }
         set { _unitInfo.Team = value; }
      }
      #endregion

      #region Constructors
      public UnitInfoLogic(IPreferenceSet Prefs, IProteinCollection proteinCollection, UnitInfo unitInfo)
      {
         _Prefs = Prefs;
         _proteinCollection = proteinCollection;
         _unitInfo = unitInfo;
         
         // This constructor is used when restoring a UnitInfo upon Load fro the UnitInfoContainer.
         // Since the ProjectID setter will not be called, we need to force the CurrentProtein 
         // property to be set.
         SetCurrentProtein();
      }

      public UnitInfoLogic(IPreferenceSet Prefs, IProteinCollection proteinCollection, UnitInfo unitInfo, 
                              string ownerName, string ownerPath, DateTime unitRetrievalTime)
         : this(Prefs, proteinCollection, unitInfo, ownerName, ownerPath, unitRetrievalTime, Constants.FoldingIDDefault, Constants.TeamDefault)
      {

      }
      
      public UnitInfoLogic(IPreferenceSet Prefs, IProteinCollection proteinCollection, UnitInfo unitInfo, 
                              string ownerName, string ownerPath, DateTime unitRetrievalTime, string foldingID, int team)
      {
         _Prefs = Prefs;
         _proteinCollection = proteinCollection;
         _unitInfo = unitInfo;

         OwningInstanceName = ownerName;
         OwningInstancePath = ownerPath;
         UnitRetrievalTime = unitRetrievalTime;
         FoldingID = foldingID;
         Team = team;

         Init();
      }

      private void Init()
      {
         TypeOfClient = ClientType.Unknown;
         DownloadTime = DateTime.MinValue;
         DueTime = DateTime.MinValue;
         UnitStartTimeStamp = TimeSpan.Zero;
         FinishedTime = DateTime.MinValue;
         CoreVersion = String.Empty;
         ProjectID = 0;
         ProjectRun = 0;
         ProjectClone = 0;
         ProjectGen = 0;
         ProteinName = String.Empty;
         ProteinTag = String.Empty;
         UnitResult = WorkUnitResult.Unknown;

         CurrentProtein = _proteinCollection.GetNewProtein();

         RawFramesComplete = 0;
         RawFramesTotal = 0;
      }
      #endregion

      #region Unit Level Members
      /// <summary>
      /// Client Type for this work unit
      /// </summary>
      public ClientType TypeOfClient
      {
         get { return _unitInfo.TypeOfClient; }
         set { _unitInfo.TypeOfClient = value; }
      }

      /// <summary>
      /// Date/time the unit was downloaded
      /// </summary>
      public DateTime DownloadTime
      {
         get { return _unitInfo.DownloadTime; }
         set { _unitInfo.DownloadTime = value; }
      }

      /// <summary>
      /// Flag specifying if Download Time is Unknown
      /// </summary>
      public bool DownloadTimeUnknown
      {
         get { return DownloadTime.Equals(DateTime.MinValue); }
      }

      /// <summary>
      /// Work Unit Preferred Deadline
      /// </summary>
      public DateTime PreferredDeadline
      {
         get
         {
            if (DownloadTimeUnknown == false && CurrentProtein.IsUnknown == false)
            {
               return DownloadTime.AddDays(CurrentProtein.PreferredDays);
            }

            return DateTime.MinValue;
         }
      }

      /// <summary>
      /// Flag specifying if Preferred Deadline is Unknown
      /// </summary>
      public bool PreferredDeadlineUnknown
      {
         get { return PreferredDeadline.Equals(DateTime.MinValue); }
      }

      /// <summary>
      /// Work Unit Preferred Deadline
      /// </summary>
      public DateTime FinalDeadline
      {
         get
         {
            if (DownloadTimeUnknown == false && CurrentProtein.IsUnknown == false)
            {
               return DownloadTime.AddDays(CurrentProtein.MaxDays);
            }

            return DateTime.MinValue;
         }
      }

      /// <summary>
      /// Flag specifying if Final Deadline is Unknown
      /// </summary>
      public bool FinalDeadlineUnknown
      {
         get { return FinalDeadline.Equals(DateTime.MinValue); }
      }

      /// <summary>
      /// Date/time the unit is due (preferred deadline)
      /// </summary>
      public DateTime DueTime
      {
         get { return _unitInfo.DueTime; }
         set { _unitInfo.DueTime = value; }
      }

      /// <summary>
      /// Flag specifying if Due Time is Unknown
      /// </summary>
      public bool DueTimeUnknown
      {
         get { return DueTime.Equals(DateTime.MinValue); }
      }

      /// <summary>
      /// Unit Start Time Stamp (Time Stamp from First Parsable Line in LogLines)
      /// </summary>
      /// <remarks>Used to Determine Status when a LogLine Time Stamp is not available - See ClientInstance.HandleReturnedStatus</remarks>
      public TimeSpan UnitStartTimeStamp
      {
         get { return _unitInfo.UnitStartTimeStamp; }
         set { _unitInfo.UnitStartTimeStamp = value; }
      }

      /// <summary>
      /// Date/time the unit finished
      /// </summary>
      public DateTime FinishedTime
      {
         get { return _unitInfo.FinishedTime; }
         set { _unitInfo.FinishedTime = value; }
      }

      /// <summary>
      /// Core Version Number
      /// </summary>
      public string CoreVersion
      {
         get { return _unitInfo.CoreVersion; }
         set { _unitInfo.CoreVersion = value; }
      }

      /// <summary>
      /// Project ID Number
      /// </summary>
      public Int32 ProjectID
      {
         get { return _unitInfo.ProjectID; }
         set
         {
            _unitInfo.ProjectID = value;
            if (_unitInfo.ProjectID == 0)
            {
               CurrentProtein = _proteinCollection.GetNewProtein();
            }
            else
            {
               CurrentProtein = _proteinCollection.GetProtein(ProjectID);
            }
         }
      }

      /// <summary>
      /// Project ID (Run)
      /// </summary>
      public Int32 ProjectRun
      {
         get { return _unitInfo.ProjectRun; }
         set { _unitInfo.ProjectRun = value; }
      }

      /// <summary>
      /// Project ID (Clone)
      /// </summary>
      public Int32 ProjectClone
      {
         get { return _unitInfo.ProjectClone; }
         set { _unitInfo.ProjectClone = value; }
      }

      /// <summary>
      /// Project ID (Gen)
      /// </summary>
      public Int32 ProjectGen
      {
         get { return _unitInfo.ProjectGen; }
         set { _unitInfo.ProjectGen = value; }
      }

      /// <summary>
      /// Returns true if Project (R/C/G) has not been identified
      /// </summary>
      public bool ProjectIsUnknown
      {
         get
         {
            return ProjectID == 0 &&
                   ProjectRun == 0 &&
                   ProjectClone == 0 &&
                   ProjectGen == 0;
         }
      }

      /// <summary>
      /// Formatted Project (Run, Clone, Gen) Information
      /// </summary>
      public string ProjectRunCloneGen
      {
         get
         {
            return String.Format("P{0} (R{1}, C{2}, G{3})", ProjectID,
                                                            ProjectRun,
                                                            ProjectClone,
                                                            ProjectGen);
         }
      }

      /// <summary>
      /// Name of the unit
      /// </summary>
      public String ProteinName
      {
         get { return _unitInfo.ProteinName; }
         set { _unitInfo.ProteinName = value; }
      }

      /// <summary>
      /// Tag string as read from the UnitInfo.txt file
      /// </summary>
      public string ProteinTag
      {
         get { return _unitInfo.ProteinTag; }
         set { _unitInfo.ProteinTag = value; }
      }

      /// <summary>
      /// Flag specifying if Protein Tag value is Unknown
      /// </summary>
      public bool ProteinTagUnknown
      {
         get { return ProteinTag.Length == 0; }
      }

      /// <summary>
      /// The Result of this Work Unit
      /// </summary>
      public WorkUnitResult UnitResult
      {
         get { return _unitInfo.UnitResult; }
         set { _unitInfo.UnitResult = value; }
      }

      private IProtein _CurrentProtein;
      /// <summary>
      /// Class member containing info on the currently running protein
      /// </summary>
      public IProtein CurrentProtein
      {
         get { return _CurrentProtein; }
         protected set
         {
            if (value == null)
            {
               throw new ArgumentException("The given value cannot be null.");
            }

            // Get a Reference to the Old Protein
            IProtein OldProtein = CurrentProtein;
            // Set the New Protein
            _CurrentProtein = value;
            // The Current Protein controls the length of the Unit Frame
            // Data Array.  Resize if the frame values are not the same.
            // We don't just want to clear the Array as there may be valid
            // Unit Frame Data already stored within it.
            if (OldProtein == null)
            {
               ClearUnitFrameData();
            }
            else if (OldProtein.Frames != CurrentProtein.Frames)
            {
               ResizeUnitFrameData();
            }
         }
      }
      #endregion

      #region Frames/Percent Completed Unit Level Members
      /// <summary>
      /// Raw number of steps complete
      /// </summary>
      public Int32 RawFramesComplete
      {
         get { return _unitInfo.RawFramesComplete; }
         set { _unitInfo.RawFramesComplete = value; }
      }

      /// <summary>
      /// Raw total number of steps
      /// </summary>
      public Int32 RawFramesTotal
      {
         get { return _unitInfo.RawFramesTotal; }
         set { _unitInfo.RawFramesTotal = value; }
      }

      /// <summary>
      /// Frame progress of the unit
      /// </summary>
      public Int32 FramesComplete
      {
         get
         {
            // Report Frame Progress even if CurrentProtein.IsUnknown - 11/22/09
            Int32 RawScaleFactor = RawFramesTotal / CurrentProtein.Frames;
            if (RawScaleFactor > 0)
            {
               return RawFramesComplete / RawScaleFactor;
            }

            return 0;
         }
      }

      /// <summary>
      /// Current progress (percentage) of the unit
      /// </summary>
      public Int32 PercentComplete
      {
         get
         {
            // Report Percent Progress even if CurrentProtein.IsUnknown - 11/22/09
            return FramesComplete * 100 / CurrentProtein.Frames;
         }
      }
      #endregion

      #region Production Value Properties
      /// <summary>
      /// Time per frame (TPF) of the unit
      /// </summary>
      public TimeSpan TimePerFrame
      {
         get
         {
            if (RawTimePerSection != 0)
            {
               return TimeSpan.FromSeconds(RawTimePerSection);
            }

            return InstanceProvider.GetInstance<IProteinBenchmarkContainer>().GetBenchmarkAverageFrameTime(this);
         }
      }

      /// <summary>
      /// Units per day (UPD) rating for this unit
      /// </summary>
      public Double UPD
      {
         get
         {
            if (CurrentProtein.IsUnknown == false)
            {
               return CurrentProtein.GetUPD(TimePerFrame);
            }

            return 0;
         }
      }

      /// <summary>
      /// Points per day (PPD) rating for this unit
      /// </summary>
      public Double PPD
      {
         get
         {
            if (CurrentProtein.IsUnknown == false)
            {
               // Issue 125
               if (_Prefs.GetPreference<bool>(Preference.CalculateBonus))
               {
                  return CurrentProtein.GetPPD(TimePerFrame, EFT);
               }

               return CurrentProtein.GetPPD(TimePerFrame);
            }

            return 0;
         }
      }

      /// <summary>
      /// Esimated time of arrival (ETA) for this unit
      /// </summary>
      public TimeSpan ETA
      {
         get
         {
            return new TimeSpan((CurrentProtein.Frames - FramesComplete) * TimePerFrame.Ticks);
         }
      }

      /// <summary>
      /// Esimated Finishing Time for this unit
      /// </summary>
      public TimeSpan EFT
      {
         get
         {
            if (DownloadTime.Equals(DateTime.MinValue) == false)
            {
               if (FinishedTime.Equals(DateTime.MinValue))
               {
                  return UnitRetrievalTime.Add(ETA).Subtract(DownloadTime);
               }

               return FinishedTime.Subtract(DownloadTime);
            }

            return TimeSpan.Zero;
         }
      }
      #endregion

      #region CurrentProtein Pass-Through Properties/Methods
      public string WorkUnitName
      {
         get
         {
            return CurrentProtein.WorkUnitName;
         }
      }

      public double NumAtoms
      {
         get
         {
            return CurrentProtein.NumAtoms;
         }
      }

      public double Credit
      {
         get
         {
            return CurrentProtein.Credit;
         }
      }

      /// <summary>
      /// Get the Credit of the Unit (including bonus)
      /// </summary>
      [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
      public double GetBonusCredit()
      {
         return CurrentProtein.GetBonusCredit(EFT);
      }

      public double Frames
      {
         get
         {
            return CurrentProtein.Frames;
         }
      }

      public string Core
      {
         get
         {
            return CurrentProtein.Core;
         }
      }
      #endregion

      #region Frame (UnitFrame) Data Variables
      /// <summary>
      /// Number of Frames Observed on this Unit
      /// </summary>
      public Int32 FramesObserved
      {
         get { return _unitInfo.FramesObserved; }
         set { _unitInfo.FramesObserved = value; }
      }

      /// <summary>
      /// Last Observed Frame on this Unit
      /// </summary>
      public IUnitFrame CurrentFrame
      {
         get { return _unitInfo.CurrentFrame; }
      }

      /// <summary>
      /// Timestamp from the last completed frame
      /// </summary>
      public TimeSpan TimeOfLastFrame
      {
         get
         {
            if (CurrentFrame != null)
            {
               return CurrentFrame.TimeOfFrame;
            }

            return TimeSpan.Zero;
         }
      }

      /// <summary>
      /// Last Frame ID based on UnitFrame Data
      /// </summary>
      public Int32 LastUnitFrameID
      {
         get
         {
            // This Property is handled differently than TimeOfLastFrame
            // above because we want the most recent FrameID even if the
            // CurrentFrame Property returns null.
            for (int i = CurrentProtein.Frames; i >= 0; i--)
            {
               if (UnitFrames[i] != null)
               {
                  return UnitFrames[i].FrameID;
               }
            }

            return 0;
         }
      }

      /// <summary>
      /// Frame Data for this Unit
      /// </summary>
      [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
      public IUnitFrame[] UnitFrames
      {
         get { return _unitInfo.UnitFrames; }
      }
      #endregion

      #region Frame Data Methods
      /// <summary>
      /// Set the Current Work Unit Frame
      /// </summary>
      public void SetCurrentFrame(ILogLine logLine, DateTimeStyles style)
      {
         // Check for FrameData
         IFrameData frame = logLine.LineData as IFrameData;
         if (frame == null)
         {
            // If not found, clear the LineType and get out
            logLine.LineType = LogLineType.Unknown;
            return;
         }

         // Parse TimeStamp
         DateTime timeStamp = DateTime.ParseExact(frame.TimeStampString, "HH:mm:ss",
                              DateTimeFormatInfo.InvariantInfo, style);

         // Set Raw Frame Values                                       
         RawFramesComplete = frame.RawFramesComplete;
         RawFramesTotal = frame.RawFramesTotal;
         // Create new UnitFrame
         UnitFrame unitFrame = new UnitFrame(frame.FrameID, timeStamp.TimeOfDay);

         if (UnitFrames[frame.FrameID] == null)
         {
            // increment observed count
            FramesObserved++;

            _unitInfo.CurrentFrameConcrete = unitFrame;
            UnitFrames[CurrentFrame.FrameID] = CurrentFrame;

            CurrentFrame.FrameDuration = TimeSpan.Zero;
            if (CurrentFrame.FrameID > 0 && UnitFrames[CurrentFrame.FrameID - 1] != null && FramesObserved > 1)
            {
               CurrentFrame.FrameDuration = GetDelta(CurrentFrame.TimeOfFrame, UnitFrames[CurrentFrame.FrameID - 1].TimeOfFrame);
            }
         }
         else
         {
            // FrameID already exists, clear the LineType
            logLine.LineType = LogLineType.Unknown;
         }
      }

      /// <summary>
      /// Clear the Observed Count, Current Frame Pointer, and the UnitFrames Array
      /// </summary>
      public void ClearUnitFrameData()
      {
         ClearCurrentFrame();
         // Now based on CurrentProtein.Frames - not 101 hard code - 11/22/09
         _unitInfo.UnitFramesConcrete = new UnitFrame[CurrentProtein.Frames + 1];
      }

      /// <summary>
      /// Clear the Observed Count and Current Frame Pointer
      /// </summary>
      public void ClearCurrentFrame()
      {
         FramesObserved = 0;
         _unitInfo.CurrentFrameConcrete = null;
      }

      /// <summary>
      /// Resize the Unit Frame Data based on the Current Protein
      /// </summary>
      private void ResizeUnitFrameData()
      {
         if (_unitInfo.UnitFrames != null)
         {
            // Get Reference to Previous Array
            UnitFrame[] OldUnitFrames = _unitInfo.UnitFramesConcrete;
            // Create New Array based on Current Protein Frame Count
            _unitInfo.UnitFramesConcrete = new UnitFrame[CurrentProtein.Frames + 1];

            // We want to copy all the data from the old array to the new array.
            // But we need to be sure that all the data will fit.  Probably won't
            // run into this situation, but it's defensive programming.
            int CopyLength = OldUnitFrames.Length;
            if (_unitInfo.UnitFramesConcrete.Length < OldUnitFrames.Length)
            {
               CopyLength = _unitInfo.UnitFramesConcrete.Length;
            }

            Array.Copy(OldUnitFrames, 0, _unitInfo.UnitFramesConcrete, 0, CopyLength);
         }
      }
      #endregion

      #region Unit Production Variations
      /// <summary>
      /// Average frame time since unit download
      /// </summary>
      public Int32 RawTimePerUnitDownload
      {
         get
         {
            if (FramesObserved > 0)
            {
               // Make sure CurrentFramePercent is greater than 0 to avoid DivideByZeroException - Issue 34
               if (DownloadTime.Equals(DateTime.MinValue) == false && CurrentFrame.FrameID > 0)
               {
                  // Use UnitRetrievalTime (sourced from ClientInstance.LastRetrievalTime) as basis for
                  // TimeSinceUnitDownload.  This removes the use of the "floating" value DateTime.Now
                  // as a basis for the calculation. - Issue 92
                  TimeSpan TimeSinceUnitDownload = UnitRetrievalTime.Subtract(DownloadTime);
                  return (Convert.ToInt32(TimeSinceUnitDownload.TotalSeconds) / CurrentFrame.FrameID);
               }

               return 0;
            }

            return 0;
         }
      }

      /// <summary>
      /// Average frame time since unit download
      /// </summary>
      public TimeSpan TimePerUnitDownload
      {
         get
         {
            return TimeSpan.FromSeconds(RawTimePerUnitDownload);
         }
      }

      /// <summary>
      /// PPD based on average frame time since unit download
      /// </summary>
      public double PPDPerUnitDownload
      {
         get
         {
            if (CurrentProtein.IsUnknown == false)
            {
               // Issue 125
               if (_Prefs.GetPreference<bool>(Preference.CalculateBonus))
               {
                  return CurrentProtein.GetPPD(TimePerUnitDownload, EFT);
               }

               return CurrentProtein.GetPPD(TimePerUnitDownload);
            }

            return 0;
         }
      }

      /// <summary>
      /// Average frame time over all sections
      /// </summary>
      public Int32 RawTimePerAllSections
      {
         get
         {
            if (FramesObserved > 0)
            {
               return GetDurationInSeconds(FramesObserved);
            }

            return 0;
         }
      }

      /// <summary>
      /// Average frame time over all sections
      /// </summary>
      public TimeSpan TimePerAllSections
      {
         get
         {
            return TimeSpan.FromSeconds(RawTimePerAllSections);
         }
      }

      /// <summary>
      /// PPD based on average frame time over all sections
      /// </summary>
      public double PPDPerAllSections
      {
         get
         {
            if (CurrentProtein.IsUnknown == false)
            {
               // Issue 125
               if (_Prefs.GetPreference<bool>(Preference.CalculateBonus))
               {
                  return CurrentProtein.GetPPD(TimePerAllSections, EFT);
               }

               return CurrentProtein.GetPPD(TimePerAllSections);
            }

            return 0;
         }
      }

      /// <summary>
      /// Average frame time over the last three sections
      /// </summary>
      public Int32 RawTimePerThreeSections
      {
         get
         {
            // time is valid for 3 "sets" ago
            if (FramesObserved > 3)
            {
               return GetDurationInSeconds(3);
            }

            return 0;
         }
      }

      /// <summary>
      /// Average frame time over the last three sections
      /// </summary>
      public TimeSpan TimePerThreeSections
      {
         get
         {
            return TimeSpan.FromSeconds(RawTimePerThreeSections);
         }
      }

      /// <summary>
      /// PPD based on average frame time over the last three sections
      /// </summary>
      public double PPDPerThreeSections
      {
         get
         {
            if (CurrentProtein.IsUnknown == false)
            {
               // Issue 125
               if (_Prefs.GetPreference<bool>(Preference.CalculateBonus))
               {
                  return CurrentProtein.GetPPD(TimePerThreeSections, EFT);
               }

               return CurrentProtein.GetPPD(TimePerThreeSections);
            }

            return 0;
         }
      }

      /// <summary>
      /// Frame time of the last section
      /// </summary>
      public Int32 RawTimePerLastSection
      {
         get
         {
            // time is valid for 1 "set" ago
            if (FramesObserved > 1)
            {
               return Convert.ToInt32(CurrentFrame.FrameDuration.TotalSeconds);
            }

            return 0;
         }
      }

      /// <summary>
      /// Frame time of the last section
      /// </summary>
      public TimeSpan TimePerLastSection
      {
         get
         {
            // time is valid for 1 "set" ago
            if (FramesObserved > 1)
            {
               return CurrentFrame.FrameDuration;
            }

            return TimeSpan.Zero;
         }
      }

      /// <summary>
      /// PPD based on frame time of the last section
      /// </summary>
      public double PPDPerLastSection
      {
         get
         {
            if (CurrentProtein.IsUnknown == false)
            {
               // Issue 125
               if (_Prefs.GetPreference<bool>(Preference.CalculateBonus))
               {
                  return CurrentProtein.GetPPD(TimePerLastSection, EFT);
               }

               return CurrentProtein.GetPPD(TimePerLastSection);
            }

            return 0;
         }
      }

      /// <summary>
      /// Frame Time per section based on current PPD calculation setting (readonly)
      /// </summary>
      public Int32 RawTimePerSection
      {
         get
         {
            switch (_Prefs.GetPreference<PpdCalculationType>(Preference.PpdCalculation))
            {
               case PpdCalculationType.LastFrame:
                  return RawTimePerLastSection;
               case PpdCalculationType.LastThreeFrames:
                  return RawTimePerThreeSections;
               case PpdCalculationType.AllFrames:
                  return RawTimePerAllSections;
               case PpdCalculationType.EffectiveRate:
                  return RawTimePerUnitDownload;
            }

            return 0;
         }
      }
      #endregion

      #region Calculate Production Variations
      /// <summary>
      /// Get the average duration over the specified number of most recent frames
      /// </summary>
      /// <param name="numberOfFrames">Number of most recent frames</param>
      private int GetDurationInSeconds(int numberOfFrames)
      {
         // No Frames have been captured yet, just return 0.
         if (CurrentFrame == null || UnitFrames == null)
         {
            return 0;
         }

         if (numberOfFrames < 1) //TODO: possibly add an upper bound check here
         {
            throw new ArgumentOutOfRangeException("numberOfFrames", "Argument 'numberOfFrames' must be greater than 0.");
         }

         int AverageSeconds = 0;

         try
         {
            int frameNumber = CurrentFrame.FrameID;

            // Make sure we only add frame durations greater than a Zero TimeSpan
            // The first frame will always have a Zero TimeSpan for frame duration
            // we don't want to take this frame into account when calculating 'AllFrames' - Issue 23
            TimeSpan TotalTime = TimeSpan.Zero;
            int countFrames = 0;

            for (int i = 0; i < numberOfFrames; i++)
            {
               if (UnitFrames[frameNumber].FrameDuration > TimeSpan.Zero)
               {
                  TotalTime = TotalTime.Add(UnitFrames[frameNumber].FrameDuration);
                  countFrames++;
               }
               frameNumber--;
            }

            if (countFrames > 0)
            {
               AverageSeconds = Convert.ToInt32(TotalTime.TotalSeconds) / countFrames;
            }
         }
         catch (NullReferenceException ex)
         {
            AverageSeconds = 0;
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, ex);
         }

         return AverageSeconds;
      }

      /// <summary>
      /// Get Time Delta between given frames
      /// </summary>
      /// <param name="timeLastFrame">Time of last frame</param>
      /// <param name="timeCompareFrame">Time of a previous frame to compare</param>
      private static TimeSpan GetDelta(TimeSpan timeLastFrame, TimeSpan timeCompareFrame)
      {
         TimeSpan tDelta;

         // check for rollover back to 00:00:00 timeLastFrame will be less than previous timeCompareFrame reading
         if (timeLastFrame < timeCompareFrame)
         {
            // get time before rollover
            tDelta = TimeSpan.FromDays(1).Subtract(timeCompareFrame);
            // add time from latest reading
            tDelta = tDelta.Add(timeLastFrame);
         }
         else
         {
            tDelta = timeLastFrame.Subtract(timeCompareFrame);
         }

         return tDelta;
      }
      #endregion

      #region Project to Protein Matching
      /// <summary>
      /// Attempts to set the Protein based on the given Project data.
      /// </summary>
      /// <param name="match">Regex Match containing Project values</param>
      public void DoProjectIDMatch(Match match)
      {
         IList<int> ProjectRCG = new List<int>(4);

         ProjectRCG.Add(Int32.Parse(match.Result("${ProjectNumber}")));
         ProjectRCG.Add(Int32.Parse(match.Result("${Run}")));
         ProjectRCG.Add(Int32.Parse(match.Result("${Clone}")));
         ProjectRCG.Add(Int32.Parse(match.Result("${Gen}")));

         DoProjectIDMatch(ProjectRCG);
      }

      /// <summary>
      /// Attempts to set the Protein based on the given Project data.
      /// </summary>
      /// <param name="ProjectRCG">List of Project (R/C/G) values</param>
      public void DoProjectIDMatch(IList<int> ProjectRCG)
      {
         Debug.Assert(ProjectRCG.Count == 4);

         ProjectID = ProjectRCG[0];
         ProjectRun = ProjectRCG[1];
         ProjectClone = ProjectRCG[2];
         ProjectGen = ProjectRCG[3];

         SetCurrentProtein();
         TypeOfClient = GetClientTypeFromProtein(CurrentProtein);
      }
      
      public void SetCurrentProtein()
      {
         CurrentProtein = _proteinCollection.GetProtein(ProjectID);
      }
      #endregion

      #region Static Helpers
      /// <summary>
      /// Determine the client type based on the current protein core
      /// </summary>
      /// <param name="CurrentProtein">Current Instance Protein</param>
      /// <returns>Client Type</returns>
      private static ClientType GetClientTypeFromProtein(IProtein CurrentProtein)
      {
         switch (CurrentProtein.Core.ToUpperInvariant())
         {
            case "GROMACS":
            case "DGROMACS":
            case "GBGROMACS":
            case "AMBER":
            //case "QMD":
            case "GROMACS33":
            case "GROST":
            case "GROSIMT":
            case "DGROMACSB":
            case "DGROMACSC":
            case "GRO-A4":
            //case "TINKER":
            /*** ProtoMol Only */
            case "PROTOMOL":
               /*******************/
               return ClientType.Standard;
            case "GRO-SMP":
            case "GROCVS":
            case "GRO-A3":
               return ClientType.SMP;
            case "GROGPU2":
            case "GROGPU2-MT":
            case "ATI-DEV":
            case "NVIDIA-DEV":
               return ClientType.GPU;
            default:
               return ClientType.Unknown;
         }
      }
      #endregion
   }
}
