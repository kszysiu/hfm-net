/*
 * HFM.NET - Protein Class
 * Copyright (C) 2006 David Rawling
 * Copyright (C) 2009-2011 Ryan Harlamert (harlam357)
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
using System.Globalization;

namespace HFM.Framework.DataTypes
{
   public interface IProtein
   {
      /// <summary>
      /// Project Number
      /// </summary>
      int ProjectNumber { get; }

      /// <summary>
      /// Server IP Address
      /// </summary>
      String ServerIP { get; }

      /// <summary>
      /// Work Unit Name
      /// </summary>
      String WorkUnitName { get; }

      /// <summary>
      /// Number of Atoms
      /// </summary>
      int NumAtoms { get; }

      /// <summary>
      /// Deadline - Preferred Days
      /// </summary>
      double PreferredDays { get; }

      /// <summary>
      /// Deadline - Maximum Days
      /// </summary>
      double MaxDays { get; }

      /// <summary>
      /// Work Unit Credit
      /// </summary>
      double Credit { get; }

      /// <summary>
      /// Number of Frames
      /// </summary>
      int Frames { get; }

      /// <summary>
      /// Core Identification String
      /// </summary>
      String Core { get; }

      /// <summary>
      /// Project Description (usually a URL)
      /// </summary>
      String Description { get; }

      /// <summary>
      /// Project Research Contact
      /// </summary>
      String Contact { get; }

      /// <summary>
      /// Bonus (K) Factor
      /// </summary>
      double KFactor { get; }

      /// <summary>
      /// Flag Denoting if Project Number is Unknown
      /// </summary>
      bool IsUnknown { get; }

      /// <summary>
      /// Get Points Per Day based on given Frame Time
      /// </summary>
      /// <param name="frameTime">Frame Time</param>
      double GetPPD(TimeSpan frameTime);

      /// <summary>
      /// Get Points Per Day based on given Frame Time
      /// </summary>
      /// <param name="frameTime">Frame Time</param>
      /// <param name="estTimeOfUnit">Estimated Time of the Unit</param>
      double GetPPD(TimeSpan frameTime, TimeSpan estTimeOfUnit);

      /// <summary>
      /// Get Units Per Day based on given Frame Time
      /// </summary>
      /// <param name="frameTime">Frame Time</param>
      double GetUPD(TimeSpan frameTime);

      /// <summary>
      /// Get the Credit of the Unit (including bonus)
      /// </summary>
      /// <param name="estTimeOfUnit">Estimated Time of the Unit</param>
      double GetBonusCredit(TimeSpan estTimeOfUnit);

      /// <summary>
      /// Get the Bonus Multiplier
      /// </summary>
      /// <param name="estTimeOfUnit">Estimated Time of the Unit</param>
      double GetBonusMultiplier(TimeSpan estTimeOfUnit);

      /// <summary>
      /// Get all Production Values
      /// </summary>
      /// <param name="frameTime">Frame Time</param>
      /// <param name="eftByDownloadTime">Estimated Time of the Unit (by Download Time)</param>
      /// <param name="eftByFrameTime">Estimated Time of the Unit (by Frame Time)</param>
      ProductionValues GetProductionValues(TimeSpan frameTime, TimeSpan eftByDownloadTime, TimeSpan eftByFrameTime);
   }

   public class Protein : IProtein
   {
      public Protein()
      {
         ServerIP = "0.0.0.0";
         WorkUnitName = "Unknown";
         Frames = 100;
         Core = "Unknown";
         Description = "Unassigned Description";
         Contact = "Unknown";
      }

      /// <summary>
      /// Project Number
      /// </summary>
      public int ProjectNumber { get; set; }

      /// <summary>
      /// Server IP Address
      /// </summary>
      public string ServerIP { get; set; }

      /// <summary>
      /// Work Unit Name
      /// </summary>
      public string WorkUnitName { get; set; }

      /// <summary>
      /// Number of Atoms
      /// </summary>
      public int NumAtoms { get; set; }

      /// <summary>
      /// Deadline - Preferred Days
      /// </summary>
      public double PreferredDays { get; set; }

      /// <summary>
      /// Deadline - Maximum Days
      /// </summary>
      public double MaxDays { get; set; }

      /// <summary>
      /// Work Unit Credit
      /// </summary>
      public double Credit { get; set; }

      /// <summary>
      /// Number of Frames
      /// </summary>
      public int Frames { get; set; }

      /// <summary>
      /// Core Identification string
      /// </summary>
      public string Core { get; set; }

      /// <summary>
      /// Project Description (usually a URL)
      /// </summary>
      public string Description { get; set; }

      /// <summary>
      /// Project Research Contact
      /// </summary>
      public string Contact { get; set; }

      /// <summary>
      /// Bonus (K) Factor
      /// </summary>
      public double KFactor { get; set; }

      /// <summary>
      /// Flag Denoting if Project Number is Unknown
      /// </summary>
      public bool IsUnknown
      {
         get { return ProjectNumber == 0; }
      }
      
      public bool Valid
      {
         get
         {
            return (ProjectNumber > 0 &&
                    PreferredDays > 0 &&
                    MaxDays > 0 &&
                    Credit > 0 &&
                    Frames > 0 &&
                    KFactor >= 0);
         }
      }

      /// <summary>
      /// Get Points Per Day based on given Frame Time
      /// </summary>
      /// <param name="frameTime">Frame Time</param>
      public double GetPPD(TimeSpan frameTime)
      {
         return GetPPD(frameTime, TimeSpan.Zero);
      }

      /// <summary>
      /// Get Points Per Day based on given Frame Time
      /// </summary>
      /// <param name="frameTime">Frame Time</param>
      /// <param name="estTimeOfUnit">Estimated Time of the Unit</param>
      public double GetPPD(TimeSpan frameTime, TimeSpan estTimeOfUnit)
      {
         if (frameTime.IsZero()) return 0;

         double basePPD = GetUPD(frameTime) * Credit;
         double bonusMulti = GetBonusMultiplier(estTimeOfUnit);
         double bonusPPD = Math.Round((basePPD * bonusMulti), Default.MaxDecimalPlaces);
         
         return bonusPPD;
      }

      /// <summary>
      /// Get Units Per Day based on given Frame Time
      /// </summary>
      /// <param name="frameTime">Frame Time</param>
      public double GetUPD(TimeSpan frameTime)
      {
         return frameTime.IsZero() ? 0.0 : 86400 / (frameTime.TotalSeconds * Frames);
      }
      
      /// <summary>
      /// Get the Credit of the Unit (including bonus)
      /// </summary>
      /// <param name="estTimeOfUnit">Estimated Time of the Unit</param>
      public double GetBonusCredit(TimeSpan estTimeOfUnit)
      {
         double bonusMulti = GetBonusMultiplier(estTimeOfUnit);
         return Math.Round((Credit * bonusMulti), 0);
      }

      /// <summary>
      /// Get the Bonus Multiplier
      /// </summary>
      /// <param name="estTimeOfUnit">Estimated Time of the Unit</param>
      public double GetBonusMultiplier(TimeSpan estTimeOfUnit)
      {
         // Make sure the given TimeSpan is not negative
         if (KFactor > 0 && estTimeOfUnit.CompareTo(TimeSpan.Zero) > 0)
         {
            if (estTimeOfUnit <= TimeSpan.FromDays(PreferredDays))
            {
               return Math.Sqrt((MaxDays * KFactor) / estTimeOfUnit.TotalDays);
            }
         }
         
         return 1;
      }

      /// <summary>
      /// Get all Production Values
      /// </summary>
      /// <param name="frameTime">Frame Time</param>
      /// <param name="eftByDownloadTime">Estimated Time of the Unit (by Download Time)</param>
      /// <param name="eftByFrameTime">Estimated Time of the Unit (by Frame Time)</param>
      public ProductionValues GetProductionValues(TimeSpan frameTime, TimeSpan eftByDownloadTime, TimeSpan eftByFrameTime)
      {
         var value = new ProductionValues
                     {
                        TimePerFrame = frameTime,
                        BaseCredit = Credit,
                        BasePPD = GetPPD(frameTime),
                        PreferredTime = TimeSpan.FromDays(PreferredDays),
                        MaximumTime = TimeSpan.FromDays(MaxDays),
                        KFactor = KFactor,
                        EftByDownloadTime = eftByDownloadTime,
                        DownloadTimeBonusMulti = GetBonusMultiplier(eftByDownloadTime),
                        DownloadTimeBonusCredit = GetBonusCredit(eftByDownloadTime),
                        DownloadTimeBonusPPD = GetPPD(frameTime, eftByDownloadTime),
                        EftByFrameTime = eftByFrameTime,
                        FrameTimeBonusMulti = GetBonusMultiplier(eftByFrameTime),
                        FrameTimeBonusCredit = GetBonusCredit(eftByFrameTime),
                        FrameTimeBonusPPD = GetPPD(frameTime, eftByFrameTime)
                     };
         return value;
      }
      
      /// <summary>
      /// Determine the Client Type based on the FAH Core Name
      /// </summary>
      /// <param name="coreName">FAH Core Name (from psummary)</param>
      public static ClientType GetClientTypeFromCore(string coreName)
      {
         // make this method more forgiving - rwh 9/6/10
         if (String.IsNullOrEmpty(coreName))
         {
            return ClientType.Unknown;
         }

         switch (coreName.ToUpperInvariant())
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
            case "GRO-A5":
               return ClientType.SMP;
            case "GROGPU2":
            case "GROGPU2-MT":
            case "OPENMMGPU":
            case "ATI-DEV":
            case "NVIDIA-DEV":
               return ClientType.GPU;
            default:
               return ClientType.Unknown;
         }
      }
   }
   
   public struct ProductionValues
   {
      public TimeSpan TimePerFrame { get; set; }

      public double BaseCredit { get; set; }

      public double BasePPD { get; set; }

      public TimeSpan PreferredTime { get; set; }

      public TimeSpan MaximumTime { get; set; }

      public double KFactor { get; set; }

      public TimeSpan EftByDownloadTime { get; set; }

      public double DownloadTimeBonusMulti { get; set; }

      public double DownloadTimeBonusCredit { get; set; }

      public double DownloadTimeBonusPPD { get; set; }
      
      public TimeSpan EftByFrameTime { get; set; }

      public double FrameTimeBonusMulti { get; set; }

      public double FrameTimeBonusCredit { get; set; }

      public double FrameTimeBonusPPD { get; set; }
      
      public IEnumerable<string> ToMultiLineString()
      {
         return new[]
         {
            String.Format(CultureInfo.CurrentCulture, " - Base Credit--------- : {0}", BaseCredit),
            String.Format(CultureInfo.CurrentCulture, " - Base PPD ----------- : {0}", BasePPD),
            String.Format(CultureInfo.CurrentCulture, " - Preferred Time ----- : {0}", PreferredTime),
            String.Format(CultureInfo.CurrentCulture, " - Maximum Time ------- : {0}", MaximumTime),
            String.Format(CultureInfo.CurrentCulture, " - KFactor ------------ : {0}", KFactor),
            String.Format(CultureInfo.CurrentCulture, " + - by Download Time - + {0}", String.Empty),
            String.Format(CultureInfo.CurrentCulture, " - --- WU Time -------- : {0}", EftByDownloadTime),
            String.Format(CultureInfo.CurrentCulture, " - --- Bonus Multiplier : {0}", DownloadTimeBonusMulti),
            String.Format(CultureInfo.CurrentCulture, " - --- Bonus Credit --- : {0}", DownloadTimeBonusCredit),
            String.Format(CultureInfo.CurrentCulture, " - --- Bonus PPD ------ : {0}", DownloadTimeBonusPPD),
            String.Format(CultureInfo.CurrentCulture, " + - by Frame Time ---- + {0}", String.Empty),
            String.Format(CultureInfo.CurrentCulture, " - --- WU Time -------- : {0}", EftByFrameTime),
            String.Format(CultureInfo.CurrentCulture, " - --- Bonus Multiplier : {0}", FrameTimeBonusMulti),
            String.Format(CultureInfo.CurrentCulture, " - --- Bonus Credit --- : {0}", FrameTimeBonusCredit),
            String.Format(CultureInfo.CurrentCulture, " - --- Bonus PPD ------ : {0}", FrameTimeBonusPPD)
         };
      }
   }
}
