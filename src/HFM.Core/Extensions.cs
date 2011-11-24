﻿/*
 * HFM.NET - Core Extension Methods
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
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using HFM.Core.DataTypes;

namespace HFM.Core
{
   public static class Extensions
   {
      #region ClientSettings

      public static string CachedFahLogFileName(this ClientSettings settings)
      {
         return String.Format(CultureInfo.InvariantCulture, "{0}-{1}", settings.Name, Default.FahLogFileName);
      }

      public static string CachedUnitInfoFileName(this ClientSettings settings)
      {
         return String.Format(CultureInfo.InvariantCulture, "{0}-{1}", settings.Name, Default.UnitInfoFileName);
      }

      public static string CachedQueueFileName(this ClientSettings settings)
      {
         return String.Format(CultureInfo.InvariantCulture, "{0}-{1}", settings.Name, Default.QueueFileName);
      }

      #endregion

      /// <summary>
      /// Get the totals for all slots.
      /// </summary>
      /// <returns>The totals for all slots.</returns>
      public static InstanceTotals GetSlotTotals(this IEnumerable<SlotModel> slots)
      {
         var totals = new InstanceTotals();

         // If no Instance Collection, return initialized totals.
         // Added this check because this function is now being passed a copy of the client 
         // slots references using GetCurrentInstanceArray() and not the collection 
         // directly, since the "live" collection can change at any time.
         // 4/17/10 - GetCurrentInstanceArray() no longer returns null when there are no clients
         //           it returns an empty collection.  However, leaving this check for now.
         if (slots == null)
         {
            return totals;
         }

         totals.TotalClients = slots.Count();

         foreach (SlotModel slot in slots)
         {
            totals.PPD += slot.PPD;
            totals.UPD += slot.UPD;
            totals.TotalRunCompletedUnits += slot.TotalRunCompletedUnits;
            totals.TotalRunFailedUnits += slot.TotalRunFailedUnits;
            totals.TotalClientCompletedUnits += slot.TotalClientCompletedUnits;

            if (slot.ProductionValuesOk)
            {
               totals.WorkingClients++;
            }
         }

         return totals;
      }

      /// <summary>
      /// Find Clients with Duplicate UserIDs or Project (R/C/G)
      /// </summary>
      public static void FindDuplicates(this IEnumerable<SlotModel> slots) // Issue 19
      {
         FindDuplicateUserId(slots);
         FindDuplicateProjects(slots);
      }

      private static void FindDuplicateUserId(IEnumerable<SlotModel> slots)
      {
         var duplicates = (from x in slots
                           group x by x.UserAndMachineId into g
                           let count = g.Count()
                           where count > 1 && g.First().UserIdUnknown == false
                           select g.Key);

         foreach (SlotModel instance in slots)
         {
            instance.UserIdIsDuplicate = duplicates.Contains(instance.UserAndMachineId);
         }
      }

      private static void FindDuplicateProjects(IEnumerable<SlotModel> slots)
      {
         var duplicates = (from x in slots
                           group x by x.UnitInfoLogic.UnitInfoData.ProjectRunCloneGen() into g
                           let count = g.Count()
                           where count > 1 && g.First().UnitInfoLogic.UnitInfoData.ProjectIsUnknown() == false
                           select g.Key);

         foreach (SlotModel instance in slots)
         {
            instance.ProjectIsDuplicate = duplicates.Contains(instance.UnitInfoLogic.UnitInfoData.ProjectRunCloneGen());
         }
      }
   }
}
