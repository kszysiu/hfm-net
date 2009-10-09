﻿/*
 * HFM.NET - Queue Parser Class
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

using System.Collections.Generic;

using HFM.Proteins;

namespace HFM.Instances
{
   internal static class QueueParser
   {
      /// <summary>
      /// Parse Queue Information Info UnitInfo
      /// </summary>
      /// <param name="entry">QueueEntry to Parse</param>
      /// <param name="parsedUnitInfo">UnitInfo to Populate</param>
      /// <param name="ClientIsOnVirtualMachine">Client on VM (Times as UTC) Flag</param>
      internal static void ParseQueueEntry(QueueEntry entry, UnitInfo parsedUnitInfo, bool ClientIsOnVirtualMachine)
      {
         if (entry.EntryStatus.Equals(QueueEntryStatus.Finished) ||
             entry.EntryStatus.Equals(QueueEntryStatus.FoldingNow) ||
             entry.EntryStatus.Equals(QueueEntryStatus.Queued) ||
             entry.EntryStatus.Equals(QueueEntryStatus.ReadyForUpload) ||
             entry.EntryStatus.Equals(QueueEntryStatus.FetchingFromServer))
         {
            /* Tag (Could be read here or through the unitinfo.txt file) */
            parsedUnitInfo.ProteinTag = entry.WorkUnitTag;

            /* DownloadTime (Could be read here or through the unitinfo.txt file) */
            if (ClientIsOnVirtualMachine)
            {
               parsedUnitInfo.DownloadTime = entry.BeginTimeUtc;
            }
            else
            {
               parsedUnitInfo.DownloadTime = entry.BeginTimeLocal;
            }

            /* DueTime (Could be read here or through the unitinfo.txt file) */
            if (ClientIsOnVirtualMachine)
            {
               parsedUnitInfo.DueTime = entry.DueTimeUtc;
            }
            else
            {
               parsedUnitInfo.DueTime = entry.DueTimeLocal;
            }

            /* FinishedTime */
            if (entry.EntryStatus.Equals(QueueEntryStatus.Finished))
            {
               if (ClientIsOnVirtualMachine)
               {
                  parsedUnitInfo.FinishedTime = entry.EndTimeUtc;
               }
               else
               {
                  parsedUnitInfo.FinishedTime = entry.EndTimeLocal;
               }
            }

            /* FoldingID and Team from Queue Entry */
            parsedUnitInfo.FoldingID = entry.FoldingID;
            parsedUnitInfo.Team = (int)entry.TeamNumber;

            /* Project (R/C/G) Match */
            List<int> ProjectID = new List<int>(4);
      
            ProjectID.Add(entry.ProjectID);
            ProjectID.Add(entry.ProjectRun);
            ProjectID.Add(entry.ProjectClone);
            ProjectID.Add(entry.ProjectGen);
            
            ClientInstance.DoProjectIDMatch(parsedUnitInfo, ProjectID);
         }
      }
   }
}
