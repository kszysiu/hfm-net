﻿/*
 * HFM.NET - Client Data Type Enumerations
 * Copyright (C) 2009-2012 Ryan Harlamert (harlam357)
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

namespace HFM.Client.DataTypes
{
   public enum FahClientType
   {
      Unknown,
      Normal,
      Advanced,
      BigAdv,
      Beta
   }

   // ReSharper disable InconsistentNaming

   public enum FahClientSubType
   {
      Unknown = 0,
      Uniprocessor = 1,
      SMP = 2,
      GPU = 3,
   }

   // ReSharper restore InconsistentNaming

   public enum FahSlotStatus
   {
      Unknown,
      Paused,
      Running,
      Finishing,
      Ready
   }

   public enum FahUnitStatus
   {
      Unknown,
      Running,
      Download,
      Send,
      Ready
   }

   public enum MaxPacketSize
   {
      Unknown,
      Small,
      Normal,
      Big
   }

   public enum CorePriority
   {
      Unknown,
      Idle,
      Low
   }
}
