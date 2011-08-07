﻿/*
 * HFM.NET - Slot Options Data Class Tests
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
using System.IO;

using NUnit.Framework;

using HFM.Client.DataTypes;

namespace HFM.Client.Tests.DataTypes
{
   [TestFixture]
   public class SlotOptionsTests
   {
      [Test]
      public void ParseTest1()
      {
         string message = File.ReadAllText("..\\..\\..\\TestFiles\\Client_v7_1\\slot-options.txt");
         var slotOptions = SlotOptions.Parse(MessageCache.GetNextJsonMessage(ref message));
         Assert.AreEqual("normal", slotOptions.ClientType);
         Assert.AreEqual(ClientTypeEnum.Normal, slotOptions.ClientTypeEnum);
         Assert.AreEqual("SMP", slotOptions.ClientSubType);
         Assert.AreEqual(ClientSubTypeEnum.SMP, slotOptions.ClientSubTypeEnum);
         Assert.AreEqual(0, slotOptions.MachineId);
         Assert.AreEqual("normal", slotOptions.MaxPacketSize);
         Assert.AreEqual(MaxPacketSizeEnum.Normal, slotOptions.MaxPacketSizeEnum);
         Assert.AreEqual("idle", slotOptions.CorePriority);
         Assert.AreEqual(CorePriorityEnum.Idle, slotOptions.CorePriorityEnum);
         Assert.AreEqual(99, slotOptions.NextUnitPercentage);
         Assert.AreEqual(0, slotOptions.MaxUnits);
         Assert.AreEqual(15, slotOptions.Checkpoint);
         Assert.AreEqual(true, slotOptions.PauseOnStart);
         Assert.AreEqual(null, slotOptions.GpuVendorId);
         Assert.AreEqual(null, slotOptions.GpuDeviceId);
      }

      [Test]
      [ExpectedException(typeof(ArgumentNullException))]
      public void ParseNullArgumentTest()
      {
         SlotOptions.Parse(null);
      }
   }
}
