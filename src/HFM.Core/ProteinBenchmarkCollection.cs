﻿/*
 * HFM.NET - Benchmark Collection Class
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
using System.Diagnostics;

using HFM.Core.DataTypes;

namespace HFM.Core
{
   public interface IProteinBenchmarkCollection : ICollection<ProteinBenchmark>
   {
      /// <summary>
      /// List of BenchmarkClient objects.
      /// </summary>
      IEnumerable<BenchmarkClient> BenchmarkClients { get; }

      void UpdateData(UnitInfo unit, int startingFrame, int endingFrame);

      /// <summary>
      /// Gets the ProteinBenchmark based on the UnitInfo owner and project data.
      /// </summary>
      /// <param name="unitInfo">The UnitInfo containing owner and project data.</param>
      /// <exception cref="T:System.ArgumentNullException"><paramref name="unitInfo"/> is null.</exception>
      ProteinBenchmark GetBenchmark(UnitInfo unitInfo);

      /// <summary>
      /// Removes all the elements from the ProteinBenchmarkCollection that match the benchmarkClient.
      /// </summary>
      /// <param name="benchmarkClient">The BenchmarkClient to remove from the ProteinBenchmarkCollection.</param>
      /// <exception cref="T:System.ArgumentNullException"><paramref name="benchmarkClient"/> is null.</exception>
      /// <exception cref="T:System.ArgumentException"><paramref name="benchmarkClient"/> represents all clients.</exception>
      void RemoveAll(BenchmarkClient benchmarkClient);

      /// <summary>
      /// Removes all the elements from the ProteinBenchmarkCollection that match the benchmarkClient and projectId.
      /// </summary>
      /// <param name="benchmarkClient">The BenchmarkClient to remove from the ProteinBenchmarkCollection.</param>
      /// <param name="projectId">The Folding@Home project number.</param>
      /// <exception cref="T:System.ArgumentNullException"><paramref name="benchmarkClient"/> is null.</exception>
      void RemoveAll(BenchmarkClient benchmarkClient, int projectId);

      /// <summary>
      /// Determines whether the ProteinBenchmarkCollection contains a specific value.
      /// </summary>
      /// <returns>
      /// true if <paramref name="benchmarkClient"/> is found in the ProteinBenchmarkCollection; otherwise, false.
      /// </returns>
      /// <param name="benchmarkClient">The BenchmarkClient to locate in the ProteinBenchmarkCollection.</param>
      /// <exception cref="T:System.ArgumentNullException"><paramref name="benchmarkClient"/> is null.</exception>
      bool Contains(BenchmarkClient benchmarkClient);

      /// <summary>
      /// Gets a list of benchmark project numbers.
      /// </summary>
      /// <param name="benchmarkClient">The BenchmarkClient to locate in the ProteinBenchmarkCollection.</param>
      /// <exception cref="T:System.ArgumentNullException"><paramref name="benchmarkClient"/> is null.</exception>
      IEnumerable<int> GetBenchmarkProjects(BenchmarkClient benchmarkClient);

      /// <summary>
      /// Gets a list of ProteinBenchmark objects.
      /// </summary>
      /// <param name="benchmarkClient">The BenchmarkClient to locate in the ProteinBenchmarkCollection.</param>
      /// <exception cref="T:System.ArgumentNullException"><paramref name="benchmarkClient"/> is null.</exception>
      IEnumerable<ProteinBenchmark> GetBenchmarks(BenchmarkClient benchmarkClient);

      /// <summary>
      /// Gets a list of ProteinBenchmark objects.
      /// </summary>
      /// <param name="benchmarkClient">The BenchmarkClient to locate in the ProteinBenchmarkCollection.</param>
      /// <param name="projectId">The Folding@Home project number.</param>
      /// <exception cref="T:System.ArgumentNullException"><paramref name="benchmarkClient"/> is null.</exception>
      IEnumerable<ProteinBenchmark> GetBenchmarks(BenchmarkClient benchmarkClient, int projectId);

      /// <summary>
      /// Updates the owner name of all the elements in ProteinBenchmarkCollection that match the benchmarkClient and projectId.
      /// </summary>
      /// <param name="benchmarkClient">The BenchmarkClient to locate in the ProteinBenchmarkCollection.</param>
      /// <param name="name">The new benchmark owner name.</param>
      /// <exception cref="T:System.ArgumentNullException"><paramref name="benchmarkClient"/> is null or <paramref name="name"/> is null.</exception>
      void UpdateOwnerName(BenchmarkClient benchmarkClient, string name);

      /// <summary>
      /// Updates the owner path of all the elements in ProteinBenchmarkCollection that match the benchmarkClient and projectId.
      /// </summary>
      /// <param name="benchmarkClient">The BenchmarkClient to locate in the ProteinBenchmarkCollection.</param>
      /// <param name="path">The new benchmark owner path.</param>
      /// <exception cref="T:System.ArgumentNullException"><paramref name="benchmarkClient"/> is null or <paramref name="path"/> is null.</exception>
      void UpdateOwnerPath(BenchmarkClient benchmarkClient, string path);

      /// <summary>
      /// Updates the minimum frame time of all the elements in ProteinBenchmarkCollection that match the benchmarkClient and projectId.
      /// </summary>
      /// <param name="benchmarkClient">The BenchmarkClient to locate in the ProteinBenchmarkCollection.</param>
      /// <param name="projectId">The Folding@Home project number.</param>
      /// <exception cref="T:System.ArgumentNullException"><paramref name="benchmarkClient"/> is null.</exception>
      void UpdateMinimumFrameTime(BenchmarkClient benchmarkClient, int projectId);

      #region ICollection<ProteinBenchmark> Members

      // Override Default Interface Documentation

      /// <summary>
      /// Adds a ProteinBenchmark to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
      /// </summary>
      /// <param name="item">The ProteinBenchmark to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
      /// <exception cref="T:System.ArgumentNullException"><paramref name="item"/> is null.</exception>
      /// <exception cref="T:System.ArgumentException">The <paramref name="item"/> already exists in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</exception>
      new void Add(ProteinBenchmark item);

      #endregion

      #region DataContainer<T>

      void Read();

      List<ProteinBenchmark> Read(string filePath, Plugins.IFileSerializer<List<ProteinBenchmark>> serializer);

      void Write();

      void Write(string filePath, Plugins.IFileSerializer<List<ProteinBenchmark>> serializer);

      #endregion
   }

   public sealed class ProteinBenchmarkCollection : DataContainer<List<ProteinBenchmark>>, IProteinBenchmarkCollection
   {
      #region Properties

      public override Plugins.IFileSerializer<List<ProteinBenchmark>> DefaultSerializer
      {
         get { return new Serializers.ProtoBufFileSerializer<List<ProteinBenchmark>>(); }
      }

      public IEnumerable<BenchmarkClient> BenchmarkClients
      {
         get
         {
            var benchmarkClients = new List<BenchmarkClient> { new BenchmarkClient() };
            foreach (var benchmark in Data)
            {
               if (!benchmarkClients.Contains(benchmark.Client))
               {
                  benchmarkClients.Add(benchmark.Client);
               }
            }

            benchmarkClients.Sort();
            return benchmarkClients.AsReadOnly();
         }
      }

      #endregion

      #region Constructor

      public ProteinBenchmarkCollection()
         : this(null)
      {
         
      } 

      public ProteinBenchmarkCollection(IPreferenceSet prefs)
      {
         if (prefs != null && !String.IsNullOrEmpty(prefs.ApplicationDataFolderPath))
         {
            FileName = System.IO.Path.Combine(prefs.ApplicationDataFolderPath, Constants.BenchmarkCacheFileName);
         }
      }

      #endregion

      #region Implementation

      #region UpdateData

      public void UpdateData(UnitInfo unit, int startingFrame, int endingFrame)
      {
         Debug.Assert(unit != null);

         // project is not known, don't add to benchmark data
         if (unit.ProjectIsUnknown()) return;

         // no progress has been made so stub out
         if (startingFrame > endingFrame) return;

         ProteinBenchmark findBenchmark = GetBenchmark(unit);
         if (findBenchmark == null)
         {
            var newBenchmark = new ProteinBenchmark
                               {
                                  OwningSlotName = unit.OwningSlotName,
                                  OwningSlotPath = unit.OwningSlotPath,
                                  ProjectID = unit.ProjectID
                               };

            if (UpdateFrames(unit, startingFrame, endingFrame, newBenchmark))
            {
               Data.Add(newBenchmark);
            }
         }
         else
         {
            UpdateFrames(unit, startingFrame, endingFrame, findBenchmark);
         }
      }

      private bool UpdateFrames(UnitInfo unit, int startingFrame, int endingFrame, ProteinBenchmark benchmark)
      {
         bool result = false;

         for (int i = startingFrame; i <= endingFrame; i++)
         {
            UnitFrame frame = unit.GetUnitFrame(i);
            if (frame != null)
            {
               if (benchmark.SetFrameTime(frame.FrameDuration))
               {
                  result = true;
               }
            }
            else
            {
               Logger.DebugFormat("({0}) FrameID '{1}' not found for Project {2}", unit.OwningSlotName, i, unit.ProjectID);
            }
         }

         return result;
      }

      #endregion

      public ProteinBenchmark GetBenchmark(UnitInfo unitInfo)
      {
         if (unitInfo == null) throw new ArgumentNullException("unitInfo");

         return Data.Find(benchmark => benchmark.Equals(unitInfo));
      }

      public void RemoveAll(BenchmarkClient benchmarkClient)
      {
         if (benchmarkClient == null) throw new ArgumentNullException("benchmarkClient");
         if (benchmarkClient.AllClients) throw new ArgumentException("Cannot remove all clients.");

         Data.RemoveAll(benchmark => benchmark.Client.Equals(benchmarkClient));
         Write();
      }

      public void RemoveAll(BenchmarkClient benchmarkClient, int projectId)
      {
         if (benchmarkClient == null) throw new ArgumentNullException("benchmarkClient");

         Data.RemoveAll(benchmark =>
                         {
                            if (benchmarkClient.AllClients)
                            {
                               return benchmark.ProjectID.Equals(projectId);
                            }
                            if (benchmark.Client.Equals(benchmarkClient))
                            {
                               return benchmark.ProjectID.Equals(projectId);
                            }
                            return false;
                         });
         Write();
      }

      public bool Contains(BenchmarkClient benchmarkClient)
      {
         if (benchmarkClient == null) throw new ArgumentNullException("benchmarkClient");

         return Data.Find(benchmark =>
                           {
                              if (benchmarkClient.AllClients)
                              {
                                 return true;
                              }
                              if (benchmark.Client.Equals(benchmarkClient))
                              {
                                 return true;
                              }
                              return false;
                           }) != null;
      }

      public IEnumerable<int> GetBenchmarkProjects(BenchmarkClient benchmarkClient)
      {
         if (benchmarkClient == null) throw new ArgumentNullException("benchmarkClient");

         var projects = new List<int>();
         foreach (var benchmark in Data)
         {
            if (projects.Contains(benchmark.ProjectID))
            {
               continue;
            }

            if (benchmarkClient.AllClients)
            {
               projects.Add(benchmark.ProjectID);
            }
            else
            {
               if (benchmark.Client.Equals(benchmarkClient))
               {
                  projects.Add(benchmark.ProjectID);
               }
            }
         }

         projects.Sort();
         return projects.AsReadOnly();
      }

      public IEnumerable<ProteinBenchmark> GetBenchmarks(BenchmarkClient benchmarkClient)
      {
         if (benchmarkClient == null) throw new ArgumentNullException("benchmarkClient");

         var list = Data.FindAll(benchmark =>
                                  {
                                     if (benchmarkClient.AllClients)
                                     {
                                        return true;
                                     }
                                     return benchmark.Client.Equals(benchmarkClient);
                                  });

         return list.AsReadOnly();
      }

      public IEnumerable<ProteinBenchmark> GetBenchmarks(BenchmarkClient benchmarkClient, int projectId)
      {
         if (benchmarkClient == null) throw new ArgumentNullException("benchmarkClient");

         var list = Data.FindAll(benchmark =>
                                  {
                                     if (benchmarkClient.AllClients)
                                     {
                                        return benchmark.ProjectID.Equals(projectId);
                                     }
                                     if (benchmark.Client.Equals(benchmarkClient))
                                     {
                                        return benchmark.ProjectID.Equals(projectId);
                                     }
                                     return false;
                                  });

         return list.AsReadOnly();
      }

      public void UpdateOwnerName(BenchmarkClient benchmarkClient, string name)
      {
         if (benchmarkClient == null) throw new ArgumentNullException("benchmarkClient");
         if (name == null) throw new ArgumentNullException("name");
         
         // Core library - should have a valid client name 
         Debug.Assert(Validate.ClientName(name));

         IEnumerable<ProteinBenchmark> benchmarks = GetBenchmarks(benchmarkClient);
         foreach (ProteinBenchmark benchmark in benchmarks)
         {
            benchmark.OwningSlotName = name;
         }
         Write();
      }

      public void UpdateOwnerPath(BenchmarkClient benchmarkClient, string path)
      {
         if (benchmarkClient == null) throw new ArgumentNullException("benchmarkClient");
         if (path == null) throw new ArgumentNullException("path");

         IEnumerable<ProteinBenchmark> benchmarks = GetBenchmarks(benchmarkClient);
         foreach (ProteinBenchmark benchmark in benchmarks)
         {
            benchmark.OwningSlotPath = path;
         }
         Write();
      }

      public void UpdateMinimumFrameTime(BenchmarkClient benchmarkClient, int projectId)
      {
         if (benchmarkClient == null) throw new ArgumentNullException("benchmarkClient");

         IEnumerable<ProteinBenchmark> benchmarks = GetBenchmarks(benchmarkClient, projectId);
         foreach (ProteinBenchmark benchmark in benchmarks)
         {
            benchmark.UpdateMinimumFrameTime();
         }
         Write();
      }
      
      #endregion

      #region ICollection<ProteinBenchmark> Members

      public void Add(ProteinBenchmark item)
      {
         if (item == null) throw new ArgumentNullException("item");
         if (Contains(item)) throw new ArgumentException("The benchmark already exists.", "item");

         Data.Add(item);
      }

      [CoverageExclude]
      public void Clear()
      {
         Data.Clear();
      }

      public bool Contains(ProteinBenchmark item)
      {
         return item != null && Data.Contains(item);
      }

      [CoverageExclude]
      void ICollection<ProteinBenchmark>.CopyTo(ProteinBenchmark[] array, int arrayIndex)
      {
         Data.CopyTo(array, arrayIndex);
      }

      public int Count
      {
         [CoverageExclude]
         get { return Data.Count; }
      }

      bool ICollection<ProteinBenchmark>.IsReadOnly
      {
         [CoverageExclude]
         get { return false; }
      }

      public bool Remove(ProteinBenchmark item)
      {
         return item != null && Data.Remove(item);
      }

      #endregion

      #region IEnumerable<ProteinBenchmark> Members

      [CoverageExclude]
      public IEnumerator<ProteinBenchmark> GetEnumerator()
      {
         return Data.GetEnumerator();
      }

      #endregion

      #region IEnumerable Members

      [CoverageExclude]
      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      #endregion
   }
}
