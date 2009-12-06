﻿/*
 * HFM.NET - Queue Reader Class Tests
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
using System.Globalization;

using NUnit.Framework;

using HFM.Instances;

namespace HFM.Instances.Tests
{
   [TestFixture]
   public class QueueReaderTests
   {
      [Test, Category("SMP")]
      public void WinSmpQueueTest_624R3()
      {
         QueueReader queue = new QueueReader();
         queue.ReadQueue("..\\..\\TestFiles\\WinSMP 6.24R3 queue.dat");
         Assert.AreEqual(600, queue.Version);
         Assert.AreEqual(9, queue.CurrentIndex);
         Assert.AreEqual(0.7884074f, queue.PerformanceFraction);
         Assert.AreEqual(4, queue.PerformanceFractionUnitWeight);
         Assert.AreEqual(232.941f, queue.DownloadRateAverage);
         Assert.AreEqual(4, queue.DownloadRateUnitWeight);
         Assert.AreEqual(179.357f, queue.UploadRateAverage);
         Assert.AreEqual(4, queue.UploadRateUnitWeight);
         Assert.AreEqual(0, queue.ResultsSent);
         
         QueueEntry entry8 = queue.GetQueueEntry(8);
         Assert.AreEqual(QueueEntryStatus.Finished, entry8.EntryStatus);
         Assert.AreEqual(4.46, entry8.SpeedFactor);
         Assert.AreEqual("171.64.65.64", entry8.ServerIP);
         Assert.AreEqual(8080, entry8.ServerPort);
         Assert.AreEqual(2653, entry8.ProjectID);
         Assert.AreEqual(3, entry8.ProjectRun);
         Assert.AreEqual(71, entry8.ProjectClone);
         Assert.AreEqual(119, entry8.ProjectGen);
         Assert.AreEqual("P2653 (R3, C71, G119)", entry8.ProjectRunCloneGen);
         Assert.AreEqual(0, entry8.Benchmark);
         Assert.AreEqual(500, entry8.Misc1a);
         Assert.AreEqual(200, entry8.Misc1b);
         Assert.AreEqual(new DateTime(2009, 9, 11, 13, 34, 24, DateTimeKind.Local), entry8.ProjectIssuedLocal);
         Assert.AreEqual(new DateTime(2009, 9, 11, 13, 34, 56, DateTimeKind.Local), entry8.BeginTimeLocal);
         Assert.AreEqual(new DateTime(2009, 9, 12, 11, 7, 37, DateTimeKind.Local), entry8.EndTimeLocal);
         Assert.AreEqual(new DateTime(2009, 9, 15, 13, 34, 56, DateTimeKind.Local), entry8.DueTimeLocal);
         Assert.AreEqual(4, entry8.ExpirationInDays);
         //preferred
         Assert.AreEqual("http://www.stanford.edu/~pande/Win32/x86/Core_a1.fah", entry8.CoreDownloadUrl.AbsoluteUri);
         Assert.AreEqual("a1", entry8.CoreNumber);
         //core name
         Assert.AreEqual(1, entry8.CpuType);
         Assert.AreEqual(687, entry8.CpuSpecies);
         Assert.AreEqual("Pentium II/III", entry8.CpuString);
         Assert.AreEqual(1, entry8.OsType);
         Assert.AreEqual(8, entry8.OsSpecies);
         Assert.AreEqual("WinXP", entry8.OsString);
         Assert.AreEqual(2, entry8.NumberOfSmpCores);
         Assert.AreEqual(2, entry8.UseCores);
         Assert.AreEqual("P2653R3C71G119", entry8.WorkUnitTag);
         Assert.AreEqual(1061857101, entry8.Flops);
         Assert.AreEqual(1061.857101, entry8.MegaFlops);
         Assert.AreEqual("1061.857101", entry8.MegaFlops.ToString(CultureInfo.InvariantCulture));
         Assert.AreEqual(1534, entry8.Memory);
         Assert.AreEqual(0, entry8.GpuMemory);
         Assert.AreEqual(true, entry8.AssignmentInfoPresent);
         Assert.AreEqual(new DateTime(2009, 9, 11, 13, 34, 20, DateTimeKind.Local), entry8.AssignmentTimeStampLocal);
         Assert.AreEqual("B9645213", entry8.AssignmentInfoChecksum);
         Assert.AreEqual("171.67.108.25", entry8.CollectionServerIP);
         Assert.AreEqual(524286976, entry8.PacketSizeLimit);
         Assert.AreEqual("harlam357", entry8.FoldingID);
         Assert.AreEqual("32", entry8.Team);
         Assert.AreEqual("2A73A923B9D2FA7A", entry8.ID);
         Assert.AreEqual("7AFAD2B923A97329", entry8.UserID);
         Assert.AreEqual(1, entry8.MachineID);
         Assert.AreEqual(2432236, entry8.WuDataFileSize);
         Assert.AreEqual("Folding@Home", entry8.WorkUnitType);
      }

      [Test, Category("SMP")]
      public void LinuxSmpQueueTest_624()
      {
         QueueReader queue = new QueueReader();
         queue.ReadQueue("..\\..\\TestFiles\\Linux SMP 6.24 queue.dat");
         Assert.AreEqual(600, queue.Version);
         Assert.AreEqual(4, queue.CurrentIndex);
         Assert.AreEqual(0.724012256f, queue.PerformanceFraction);
         Assert.AreEqual(4, queue.PerformanceFractionUnitWeight);
         Assert.AreEqual(300.335f, queue.DownloadRateAverage);
         Assert.AreEqual(4, queue.DownloadRateUnitWeight);
         Assert.AreEqual(184.095f, queue.UploadRateAverage);
         Assert.AreEqual(4, queue.UploadRateUnitWeight);
         Assert.AreEqual(0, queue.ResultsSent);

         QueueEntry entry4 = queue.GetQueueEntry(4);
         Assert.AreEqual(QueueEntryStatus.FoldingNow, entry4.EntryStatus);
         Assert.AreEqual(0, entry4.SpeedFactor);
         Assert.AreEqual("171.64.65.56", entry4.ServerIP);
         Assert.AreEqual(8080, entry4.ServerPort);
         Assert.AreEqual(2677, entry4.ProjectID);
         Assert.AreEqual(33, entry4.ProjectRun);
         Assert.AreEqual(19, entry4.ProjectClone);
         Assert.AreEqual(44, entry4.ProjectGen);
         Assert.AreEqual("P2677 (R33, C19, G44)", entry4.ProjectRunCloneGen);
         Assert.AreEqual(0, entry4.Benchmark);
         Assert.AreEqual(500, entry4.Misc1a);
         Assert.AreEqual(200, entry4.Misc1b);
         Assert.AreEqual(new DateTime(2009, 9, 13, 21, 36, 34, DateTimeKind.Local), entry4.ProjectIssuedLocal);
         Assert.AreEqual(new DateTime(2009, 9, 13, 21, 38, 21, DateTimeKind.Local), entry4.BeginTimeLocal);
         /*** Entry Not Completed ***/
         Assert.AreEqual(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), entry4.EndTimeUtc); //Utc
         Assert.AreEqual(new DateTime(2009, 9, 16, 21, 38, 21, DateTimeKind.Local), entry4.DueTimeLocal);
         Assert.AreEqual(3, entry4.ExpirationInDays);
         //preferred
         Assert.AreEqual("http://www.stanford.edu/~pande/Linux/AMD64/Core_a2.fah", entry4.CoreDownloadUrl.AbsoluteUri);
         Assert.AreEqual("a2", entry4.CoreNumber);
         //core name
         Assert.AreEqual(16, entry4.CpuType);
         Assert.AreEqual(0, entry4.CpuSpecies);
         Assert.AreEqual("AMD64", entry4.CpuString);
         Assert.AreEqual(4, entry4.OsType);
         Assert.AreEqual(0, entry4.OsSpecies);
         Assert.AreEqual("Linux", entry4.OsString);
         Assert.AreEqual(2, entry4.NumberOfSmpCores);
         Assert.AreEqual(2, entry4.UseCores);
         Assert.AreEqual("P2677R33C19G44", entry4.WorkUnitTag);
         Assert.AreEqual(1060722910, entry4.Flops);
         Assert.AreEqual(1060.722910, entry4.MegaFlops);
         Assert.AreEqual("1060.72291", entry4.MegaFlops.ToString(CultureInfo.InvariantCulture));
         Assert.AreEqual(685, entry4.Memory);
         Assert.AreEqual(0, entry4.GpuMemory);
         Assert.AreEqual(true, entry4.AssignmentInfoPresent);
         Assert.AreEqual(new DateTime(2009, 9, 13, 21, 36, 31, DateTimeKind.Local), entry4.AssignmentTimeStampLocal);
         Assert.AreEqual("B9196E68", entry4.AssignmentInfoChecksum);
         Assert.AreEqual("171.67.108.25", entry4.CollectionServerIP);
         Assert.AreEqual(524286976, entry4.PacketSizeLimit);
         Assert.AreEqual("harlam357", entry4.FoldingID);
         Assert.AreEqual("32", entry4.Team);
         Assert.AreEqual("9B1ED93B634D9D3D", entry4.ID);
         Assert.AreEqual("3D9D4D633BD91E9A", entry4.UserID);
         Assert.AreEqual(1, entry4.MachineID);
         Assert.AreEqual(4838584, entry4.WuDataFileSize);
         Assert.AreEqual("Folding@Home", entry4.WorkUnitType);
      }

      [Test, Category("SMP")]
      public void LinuxSmpWaitingUploadQueueTest_624()
      {
         QueueReader queue = new QueueReader();
         queue.ReadQueue("..\\..\\TestFiles\\Linux SMP 6.24 Waiting Upload queue.dat");
         Assert.AreEqual(600, queue.Version);
         Assert.AreEqual(7, queue.CurrentIndex);
         Assert.AreEqual(0.7182704f, queue.PerformanceFraction);
         Assert.AreEqual(4, queue.PerformanceFractionUnitWeight);
         Assert.AreEqual(273.766f, queue.DownloadRateAverage);
         Assert.AreEqual(4, queue.DownloadRateUnitWeight);
         Assert.AreEqual(183.91f, queue.UploadRateAverage);
         Assert.AreEqual(4, queue.UploadRateUnitWeight);
         Assert.AreEqual(0, queue.ResultsSent);

         QueueEntry entry6 = queue.GetQueueEntry(6);
         Assert.AreEqual(QueueEntryStatus.ReadyForUpload, entry6.EntryStatus);
         Assert.AreEqual(3.97, entry6.SpeedFactor);
         Assert.AreEqual("171.64.65.56", entry6.ServerIP);
         Assert.AreEqual(8080, entry6.ServerPort);
         Assert.AreEqual(2669, entry6.ProjectID);
         Assert.AreEqual(2, entry6.ProjectRun);
         Assert.AreEqual(112, entry6.ProjectClone);
         Assert.AreEqual(164, entry6.ProjectGen);
         Assert.AreEqual("P2669 (R2, C112, G164)", entry6.ProjectRunCloneGen);
         Assert.AreEqual(0, entry6.Benchmark);
         Assert.AreEqual(500, entry6.Misc1a);
         Assert.AreEqual(200, entry6.Misc1b);
         Assert.AreEqual(new DateTime(2009, 9, 25, 10, 29, 46, DateTimeKind.Local), entry6.ProjectIssuedLocal);
         Assert.AreEqual(new DateTime(2009, 9, 25, 10, 30, 32, DateTimeKind.Local), entry6.BeginTimeLocal);
         Assert.AreEqual(new DateTime(2009, 9, 26, 9, 38, 6, DateTimeKind.Local), entry6.EndTimeUtc);
         Assert.AreEqual(new DateTime(2009, 9, 28, 10, 30, 32, DateTimeKind.Local), entry6.DueTimeLocal);
         Assert.AreEqual(3, entry6.ExpirationInDays);
         //preferred
         Assert.AreEqual("http://www.stanford.edu/~pande/Linux/AMD64/Core_a2.fah", entry6.CoreDownloadUrl.AbsoluteUri);
         Assert.AreEqual("a2", entry6.CoreNumber);
         //core name
         Assert.AreEqual(16, entry6.CpuType);
         Assert.AreEqual(0, entry6.CpuSpecies);
         Assert.AreEqual("AMD64", entry6.CpuString);
         Assert.AreEqual(4, entry6.OsType);
         Assert.AreEqual(0, entry6.OsSpecies);
         Assert.AreEqual("Linux", entry6.OsString);
         Assert.AreEqual(2, entry6.NumberOfSmpCores);
         Assert.AreEqual(2, entry6.UseCores);
         Assert.AreEqual("P2669R2C112G164", entry6.WorkUnitTag);
         Assert.AreEqual(1060500841, entry6.Flops);
         Assert.AreEqual(1060.500841, entry6.MegaFlops);
         Assert.AreEqual("1060.500841", entry6.MegaFlops.ToString(CultureInfo.InvariantCulture));
         Assert.AreEqual(685, entry6.Memory);
         Assert.AreEqual(0, entry6.GpuMemory);
         Assert.AreEqual(true, entry6.AssignmentInfoPresent);
         Assert.AreEqual(new DateTime(2009, 9, 25, 10, 29, 42, DateTimeKind.Local), entry6.AssignmentTimeStampLocal);
         Assert.AreEqual("B91698A1", entry6.AssignmentInfoChecksum);
         Assert.AreEqual("171.67.108.25", entry6.CollectionServerIP);
         Assert.AreEqual(524286976, entry6.PacketSizeLimit);
         Assert.AreEqual("harlam357", entry6.FoldingID);
         Assert.AreEqual("32", entry6.Team);
         Assert.AreEqual("21EE6D43B3860603", entry6.ID);
         Assert.AreEqual("30686B3436DEE20", entry6.UserID);
         Assert.AreEqual(1, entry6.MachineID);
         Assert.AreEqual(4836605, entry6.WuDataFileSize);
         Assert.AreEqual("Folding@Home", entry6.WorkUnitType);
      }

      [Test, Category("GPU")]
      public void WinGpuQueueTest_623()
      {
         QueueReader queue = new QueueReader();
         queue.ReadQueue("..\\..\\TestFiles\\GPU2 6.23 queue.dat");
         Assert.AreEqual(600, queue.Version);
         Assert.AreEqual(8, queue.CurrentIndex);
         Assert.AreEqual(0.990233958f, queue.PerformanceFraction);
         Assert.AreEqual(4, queue.PerformanceFractionUnitWeight);
         Assert.AreEqual(64.48f, queue.DownloadRateAverage);
         Assert.AreEqual(4, queue.DownloadRateUnitWeight);
         Assert.AreEqual(99.712f, queue.UploadRateAverage);
         Assert.AreEqual(4, queue.UploadRateUnitWeight);
         Assert.AreEqual(305567834, queue.ResultsSent); // This number makes no sense

         QueueEntry entry7 = queue.GetQueueEntry(7);
         Assert.AreEqual(QueueEntryStatus.Finished, entry7.EntryStatus);
         Assert.AreEqual(178.51, entry7.SpeedFactor);
         Assert.AreEqual("171.64.65.106", entry7.ServerIP);
         Assert.AreEqual(8080, entry7.ServerPort);
         Assert.AreEqual(5790, entry7.ProjectID);
         Assert.AreEqual(5, entry7.ProjectRun);
         Assert.AreEqual(360, entry7.ProjectClone);
         Assert.AreEqual(1, entry7.ProjectGen);
         Assert.AreEqual("P5790 (R5, C360, G1)", entry7.ProjectRunCloneGen);
         Assert.AreEqual(0, entry7.Benchmark);
         Assert.AreEqual(500, entry7.Misc1a);
         Assert.AreEqual(200, entry7.Misc1b);
         Assert.AreEqual(new DateTime(2009, 9, 13, 19, 45, 30, DateTimeKind.Local), entry7.ProjectIssuedLocal);
         Assert.AreEqual(new DateTime(2009, 9, 13, 19, 45, 30, DateTimeKind.Local), entry7.BeginTimeLocal);
         Assert.AreEqual(new DateTime(2009, 9, 13, 21, 46, 30, DateTimeKind.Local), entry7.EndTimeLocal);
         Assert.AreEqual(new DateTime(2009, 9, 28, 19, 45, 30, DateTimeKind.Local), entry7.DueTimeLocal);
         Assert.AreEqual(15, entry7.ExpirationInDays);
         //preferred
         Assert.AreEqual("http://www.stanford.edu/~pande/Win32/x86/NVIDIA/G80/Core_11.fah", entry7.CoreDownloadUrl.AbsoluteUri);
         Assert.AreEqual("11", entry7.CoreNumber);
         //core name
         Assert.AreEqual(1, entry7.CpuType);
         Assert.AreEqual(687, entry7.CpuSpecies);
         Assert.AreEqual("Pentium II/III", entry7.CpuString);
         Assert.AreEqual(1, entry7.OsType);
         Assert.AreEqual(0, entry7.OsSpecies);
         Assert.AreEqual("Windows", entry7.OsString);
         Assert.AreEqual(0, entry7.NumberOfSmpCores);
         Assert.AreEqual(0, entry7.UseCores);
         Assert.AreEqual("P5790R5C360G1", entry7.WorkUnitTag);
         Assert.AreEqual(1065171903, entry7.Flops);
         Assert.AreEqual(1065.171903, entry7.MegaFlops);
         Assert.AreEqual("1065.171903", entry7.MegaFlops.ToString(CultureInfo.InvariantCulture));
         Assert.AreEqual(4094, entry7.Memory);
         Assert.AreEqual(258, entry7.GpuMemory);
         Assert.AreEqual(true, entry7.AssignmentInfoPresent);
         Assert.AreEqual(new DateTime(2009, 9, 13, 19, 45, 26, DateTimeKind.Local), entry7.AssignmentTimeStampLocal);
         Assert.AreEqual("B9194833", entry7.AssignmentInfoChecksum);
         Assert.AreEqual("171.67.108.25", entry7.CollectionServerIP);
         Assert.AreEqual(524286976, entry7.PacketSizeLimit);
         Assert.AreEqual("harlam357", entry7.FoldingID);
         Assert.AreEqual("32", entry7.Team);
         Assert.AreEqual("492A106C0885F10C", entry7.ID);
         Assert.AreEqual("CF185086C102A47", entry7.UserID);
         Assert.AreEqual(2, entry7.MachineID);
         Assert.AreEqual(67869, entry7.WuDataFileSize);
         Assert.AreEqual("Folding@Home", entry7.WorkUnitType);
      }
      
      [Test, Category("Standard")]
      public void StandardPpcQueueTest_6xx()
      {
         QueueReader queue = new QueueReader();
         queue.ReadQueue("..\\..\\TestFiles\\Standard PPC 6.xx queue.dat");
         Assert.AreEqual(600, queue.Version);
         Assert.AreEqual(2, queue.CurrentIndex);
         Assert.AreEqual(0.907946765f, queue.PerformanceFraction);
         Assert.AreEqual(1, queue.PerformanceFractionUnitWeight);
         Assert.AreEqual(998.251f, queue.DownloadRateAverage);
         Assert.AreEqual(2, queue.DownloadRateUnitWeight);
         Assert.AreEqual(3375.984f, queue.UploadRateAverage);
         Assert.AreEqual(1, queue.UploadRateUnitWeight);
         Assert.AreEqual(0, queue.ResultsSent);
         
         QueueEntry entry0 = queue.GetQueueEntry(0);
         Assert.AreEqual(QueueEntryStatus.Empty, entry0.EntryStatus);

         QueueEntry entry1 = queue.GetQueueEntry(1);
         Assert.AreEqual(QueueEntryStatus.Finished, entry1.EntryStatus);
         Assert.AreEqual(10.86, entry1.SpeedFactor);
         Assert.AreEqual("171.64.65.65", entry1.ServerIP);
         Assert.AreEqual(8080, entry1.ServerPort);
         Assert.AreEqual(2611, entry1.ProjectID);
         Assert.AreEqual(1, entry1.ProjectRun);
         Assert.AreEqual(518, entry1.ProjectClone);
         Assert.AreEqual(185, entry1.ProjectGen);
         Assert.AreEqual("P2611 (R1, C518, G185)", entry1.ProjectRunCloneGen);
         Assert.AreEqual(0, entry1.Benchmark);
         Assert.AreEqual(500, entry1.Misc1a);
         Assert.AreEqual(400, entry1.Misc1b);
         Assert.AreEqual(new DateTime(2009, 9, 25, 15, 20, 04, DateTimeKind.Local), entry1.ProjectIssuedLocal);
         Assert.AreEqual(new DateTime(2009, 9, 25, 15, 20, 12, DateTimeKind.Local), entry1.BeginTimeLocal);
         Assert.AreEqual(new DateTime(2009, 9, 29, 7, 42, 28, DateTimeKind.Local), entry1.EndTimeLocal);
         Assert.AreEqual(new DateTime(2009, 11, 4, 14, 20, 12, DateTimeKind.Local), entry1.DueTimeLocal);
         Assert.AreEqual(40, entry1.ExpirationInDays);
         //preferred
         Assert.AreEqual("http://www.stanford.edu/~pande/OSX/PowerPC/Core_78.fah", entry1.CoreDownloadUrl.AbsoluteUri);
         Assert.AreEqual("78", entry1.CoreNumber);
         //core name
         Assert.AreEqual(2, entry1.CpuType);
         Assert.AreEqual(0, entry1.CpuSpecies);
         Assert.AreEqual("PowerPC", entry1.CpuString);
         Assert.AreEqual(3, entry1.OsType);
         Assert.AreEqual(0, entry1.OsSpecies);
         Assert.AreEqual("OSX", entry1.OsString);
         Assert.AreEqual(0, entry1.NumberOfSmpCores);
         Assert.AreEqual(0, entry1.UseCores);
         Assert.AreEqual("P2611R1C518G185", entry1.WorkUnitTag);
         Assert.AreEqual(0, entry1.Flops);
         Assert.AreEqual(0, entry1.MegaFlops);
         Assert.AreEqual("0", entry1.MegaFlops.ToString(CultureInfo.InvariantCulture));
         Assert.AreEqual(2048, entry1.Memory);
         Assert.AreEqual(0, entry1.GpuMemory);
         Assert.AreEqual(true, entry1.AssignmentInfoPresent);
         Assert.AreEqual(new DateTime(2009, 9, 25, 15, 20, 00, DateTimeKind.Local), entry1.AssignmentTimeStampLocal);
         Assert.AreEqual("B916E4CE", entry1.AssignmentInfoChecksum);
         Assert.AreEqual("171.67.108.25", entry1.CollectionServerIP);
         Assert.AreEqual(524286976, entry1.PacketSizeLimit);
         Assert.AreEqual("surferseth", entry1.FoldingID);
         Assert.AreEqual("32", entry1.Team);
         Assert.AreEqual("990D49073792355C", entry1.ID);
         Assert.AreEqual("5C35923707490D98", entry1.UserID);
         Assert.AreEqual(1, entry1.MachineID);
         Assert.AreEqual(3872181, entry1.WuDataFileSize);
         Assert.AreEqual("Folding@Home", entry1.WorkUnitType);

         QueueEntry entry2 = queue.GetQueueEntry(2);
         Assert.AreEqual(QueueEntryStatus.FoldingNow, entry2.EntryStatus);
         Assert.AreEqual(0, entry2.SpeedFactor);
         Assert.AreEqual("171.64.65.65", entry2.ServerIP);
         Assert.AreEqual(8080, entry2.ServerPort);
         Assert.AreEqual(2613, entry2.ProjectID);
         Assert.AreEqual(32, entry2.ProjectRun);
         Assert.AreEqual(0, entry2.ProjectClone);
         Assert.AreEqual(169, entry2.ProjectGen);
         Assert.AreEqual("P2613 (R32, C0, G169)", entry2.ProjectRunCloneGen);
         Assert.AreEqual(0, entry2.Benchmark);
         Assert.AreEqual(500, entry2.Misc1a);
         Assert.AreEqual(400, entry2.Misc1b);
         Assert.AreEqual(new DateTime(2009, 9, 29, 7, 45, 57, DateTimeKind.Local), entry2.ProjectIssuedLocal);
         Assert.AreEqual(new DateTime(2009, 9, 29, 7, 46, 15, DateTimeKind.Local), entry2.BeginTimeLocal);
         /*** Entry Not Completed ***/
         Assert.AreEqual(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), entry2.EndTimeUtc); //Utc
         Assert.AreEqual(new DateTime(2009, 11, 8, 6, 46, 15, DateTimeKind.Local), entry2.DueTimeLocal);
         Assert.AreEqual(40, entry2.ExpirationInDays);
         //preferred
         Assert.AreEqual("http://www.stanford.edu/~pande/OSX/PowerPC/Core_78.fah", entry2.CoreDownloadUrl.AbsoluteUri);
         Assert.AreEqual("78", entry2.CoreNumber);
         //core name
         Assert.AreEqual(2, entry2.CpuType);
         Assert.AreEqual(0, entry2.CpuSpecies);
         Assert.AreEqual("PowerPC", entry2.CpuString);
         Assert.AreEqual(3, entry2.OsType);
         Assert.AreEqual(0, entry2.OsSpecies);
         Assert.AreEqual("OSX", entry2.OsString);
         Assert.AreEqual(0, entry2.NumberOfSmpCores);
         Assert.AreEqual(0, entry2.UseCores);
         Assert.AreEqual("P2613R32C0G169", entry2.WorkUnitTag);
         Assert.AreEqual(1063808819, entry2.Flops);
         Assert.AreEqual(1063.808819, entry2.MegaFlops);
         Assert.AreEqual("1063.808819", entry2.MegaFlops.ToString(CultureInfo.InvariantCulture));
         Assert.AreEqual(2048, entry2.Memory);
         Assert.AreEqual(0, entry2.GpuMemory);
         Assert.AreEqual(true, entry2.AssignmentInfoPresent);
         Assert.AreEqual(new DateTime(2009, 9, 29, 7, 45, 53, DateTimeKind.Local), entry2.AssignmentTimeStampLocal);
         Assert.AreEqual("B90DB8BF", entry2.AssignmentInfoChecksum);
         Assert.AreEqual("171.67.108.25", entry2.CollectionServerIP);
         Assert.AreEqual(524286976, entry2.PacketSizeLimit);
         Assert.AreEqual("surferseth", entry2.FoldingID);
         Assert.AreEqual("32", entry2.Team);
         Assert.AreEqual("990D49073792355C", entry2.ID);
         Assert.AreEqual("5C35923707490D98", entry2.UserID);
         Assert.AreEqual(1, entry2.MachineID);
         Assert.AreEqual(9256122, entry2.WuDataFileSize);
         Assert.AreEqual("Folding@Home", entry2.WorkUnitType);
      }

      [Test, Category("Standard")]
      public void StandardPpcQueueTest_501()
      {
         QueueReader queue = new QueueReader();
         queue.ReadQueue("..\\..\\TestFiles\\Standard PPC 5.01 queue.dat");
         Assert.AreEqual(501, queue.Version);
         Assert.AreEqual(2, queue.CurrentIndex);
         Assert.AreEqual(0.823216438f, queue.PerformanceFraction);
         Assert.AreEqual(4, queue.PerformanceFractionUnitWeight);
         Assert.AreEqual(200.309f, queue.DownloadRateAverage);
         Assert.AreEqual(4, queue.DownloadRateUnitWeight);
         Assert.AreEqual(97.206f, queue.UploadRateAverage);
         Assert.AreEqual(4, queue.UploadRateUnitWeight);
         Assert.AreEqual(0, queue.ResultsSent);

         QueueEntry entry1 = queue.GetQueueEntry(1);
         Assert.AreEqual(QueueEntryStatus.Finished, entry1.EntryStatus);
         Assert.AreEqual(8.44, entry1.SpeedFactor);
         Assert.AreEqual("171.64.65.58", entry1.ServerIP);
         Assert.AreEqual(8080, entry1.ServerPort);
         Assert.AreEqual(3046, entry1.ProjectID);
         Assert.AreEqual(0, entry1.ProjectRun);
         Assert.AreEqual(505, entry1.ProjectClone);
         Assert.AreEqual(74, entry1.ProjectGen);
         Assert.AreEqual("P3046 (R0, C505, G74)", entry1.ProjectRunCloneGen);
         Assert.AreEqual(357, entry1.Benchmark);
         Assert.AreEqual(500, entry1.Misc1a);
         Assert.AreEqual(200, entry1.Misc1b);
         Assert.AreEqual(new DateTime(2009, 08, 31, 10, 07, 52, DateTimeKind.Local), entry1.ProjectIssuedLocal);
         Assert.AreEqual(new DateTime(2009, 08, 31, 10, 06, 31, DateTimeKind.Local), entry1.BeginTimeLocal);
         Assert.AreEqual(new DateTime(2009, 09, 08, 05, 52, 45, DateTimeKind.Local), entry1.EndTimeLocal);
         Assert.AreEqual(new DateTime(2009, 11, 05, 09, 06, 31, DateTimeKind.Local), entry1.DueTimeLocal);
         Assert.AreEqual(66, entry1.ExpirationInDays);
         //preferred
         Assert.AreEqual("http://www.stanford.edu/~pande/OSX/PowerPC/Core_78.fah", entry1.CoreDownloadUrl.AbsoluteUri);
         Assert.AreEqual("78", entry1.CoreNumber);
         //core name
         Assert.AreEqual(2, entry1.CpuType);
         Assert.AreEqual(0, entry1.CpuSpecies);
         Assert.AreEqual("PowerPC", entry1.CpuString);
         Assert.AreEqual(3, entry1.OsType);
         Assert.AreEqual(0, entry1.OsSpecies);
         Assert.AreEqual("OSX", entry1.OsString);
         Assert.AreEqual(0, entry1.NumberOfSmpCores);
         Assert.AreEqual(0, entry1.UseCores);
         Assert.AreEqual("P3046R0C505G74", entry1.WorkUnitTag);
         Assert.AreEqual(0, entry1.Flops);
         Assert.AreEqual(0, entry1.MegaFlops);
         Assert.AreEqual("0", entry1.MegaFlops.ToString(CultureInfo.InvariantCulture));
         Assert.AreEqual(0, entry1.Memory);
         Assert.AreEqual(0, entry1.GpuMemory);
         Assert.AreEqual(true, entry1.AssignmentInfoPresent);
         Assert.AreEqual(new DateTime(2009, 08, 31, 10, 07, 49, DateTimeKind.Local), entry1.AssignmentTimeStampLocal);
         Assert.AreEqual("B977A500", entry1.AssignmentInfoChecksum);
         Assert.AreEqual("171.67.108.17", entry1.CollectionServerIP);
         Assert.AreEqual(5241856, entry1.PacketSizeLimit);
         Assert.AreEqual("susato", entry1.FoldingID);
         Assert.AreEqual("1971", entry1.Team);
         Assert.AreEqual("A43758419B21B433", entry1.ID);
         Assert.AreEqual("33B4219B415837A3", entry1.UserID);
         Assert.AreEqual(1, entry1.MachineID);
         Assert.AreEqual(283826, entry1.WuDataFileSize);
         Assert.AreEqual("Folding@Home", entry1.WorkUnitType);
      }

      [Test, Category("Standard")]
      public void Standardx86QueueTest_6xx()
      {
         QueueReader queue = new QueueReader();
         queue.ReadQueue("..\\..\\TestFiles\\Standard x86 6.xx queue.dat");
         Assert.AreEqual(600, queue.Version);
         Assert.AreEqual(1, queue.CurrentIndex);
         Assert.AreEqual(0.0f, queue.PerformanceFraction);
         Assert.AreEqual(0, queue.PerformanceFractionUnitWeight);
         Assert.AreEqual(424.698f, queue.DownloadRateAverage);
         Assert.AreEqual(1, queue.DownloadRateUnitWeight);
         Assert.AreEqual(0.0f, queue.UploadRateAverage);
         Assert.AreEqual(0, queue.UploadRateUnitWeight);
         Assert.AreEqual(0, queue.ResultsSent);

         QueueEntry entry0 = queue.GetQueueEntry(0);
         Assert.AreEqual(QueueEntryStatus.Empty, entry0.EntryStatus);

         QueueEntry entry1 = queue.GetQueueEntry(1);
         Assert.AreEqual(QueueEntryStatus.FoldingNow, entry1.EntryStatus);
         Assert.AreEqual(0.0, entry1.SpeedFactor);
         Assert.AreEqual("171.65.103.162", entry1.ServerIP);
         Assert.AreEqual(8080, entry1.ServerPort);
         Assert.AreEqual(2498, entry1.ProjectID);
         Assert.AreEqual(204, entry1.ProjectRun);
         Assert.AreEqual(8, entry1.ProjectClone);
         Assert.AreEqual(16, entry1.ProjectGen);
         Assert.AreEqual("P2498 (R204, C8, G16)", entry1.ProjectRunCloneGen);
         Assert.AreEqual(0, entry1.Benchmark);
         Assert.AreEqual(500, entry1.Misc1a);
         Assert.AreEqual(400, entry1.Misc1b);
         // This ProjectIssuedLocal Value makes no sense
         Assert.AreEqual(new DateTime(2009, 10, 25, 17, 20, 57, DateTimeKind.Local), entry1.ProjectIssuedLocal);
         Assert.AreEqual(new DateTime(2009, 10, 25, 11, 47, 16, DateTimeKind.Local), entry1.BeginTimeLocal);
         /*** Entry Not Completed ***/
         Assert.AreEqual(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), entry1.EndTimeUtc); //Utc
         Assert.AreEqual(new DateTime(2010, 1, 26, 10, 47, 16, DateTimeKind.Local), entry1.DueTimeLocal);
         Assert.AreEqual(93, entry1.ExpirationInDays);
         //preferred
         Assert.AreEqual("http://www.stanford.edu/~pande/Win32/x86/Core_78.fah", entry1.CoreDownloadUrl.AbsoluteUri);
         Assert.AreEqual("78", entry1.CoreNumber);
         //core name
         Assert.AreEqual(1, entry1.CpuType);
         Assert.AreEqual(2000, entry1.CpuSpecies);
         Assert.AreEqual("AMD x86", entry1.CpuString);
         Assert.AreEqual(1, entry1.OsType);
         Assert.AreEqual(8, entry1.OsSpecies);
         Assert.AreEqual("WinXP", entry1.OsString);
         Assert.AreEqual(0, entry1.NumberOfSmpCores);
         Assert.AreEqual(0, entry1.UseCores);
         Assert.AreEqual("P2498R204C8G16", entry1.WorkUnitTag);
         Assert.AreEqual(0, entry1.Flops);
         Assert.AreEqual(0, entry1.MegaFlops);
         Assert.AreEqual("0", entry1.MegaFlops.ToString(CultureInfo.InvariantCulture));
         Assert.AreEqual(1022, entry1.Memory);
         Assert.AreEqual(0, entry1.GpuMemory);
         Assert.AreEqual(true, entry1.AssignmentInfoPresent);
         Assert.AreEqual(new DateTime(2009, 10, 25, 11, 46, 56, DateTimeKind.Local), entry1.AssignmentTimeStampLocal);
         Assert.AreEqual("B92F1DDD", entry1.AssignmentInfoChecksum);
         Assert.AreEqual("171.65.103.100", entry1.CollectionServerIP);
         Assert.AreEqual(524286976, entry1.PacketSizeLimit);
         Assert.AreEqual("harlam357", entry1.FoldingID);
         Assert.AreEqual("32", entry1.Team);
         Assert.AreEqual("CAE6B725063F6B3F", entry1.ID);
         Assert.AreEqual("3F6B3F0625B7E6BA", entry1.UserID);
         Assert.AreEqual(16, entry1.MachineID);
         Assert.AreEqual(2972889, entry1.WuDataFileSize);
         Assert.AreEqual("Folding@Home", entry1.WorkUnitType);
      }
      
      [Test]
      public void UserIDCalculationTest()
      {
         byte[] b = QueueEntry.HexToData("99D3CF222E1FA00");
         Array.Reverse(b);
         string UserID = QueueEntry.GetUserIDFromUserAndMachineID(b, 16);
         Assert.AreEqual("99D3CF222E1F9F0", UserID);
      }
   }
}
