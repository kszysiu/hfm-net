﻿/*
 * HFM.NET - Client Instance Factory
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
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

using HFM.Framework;
using HFM.Instrumentation;

namespace HFM.Instances
{
   public class ClientInstanceFactory
   {
      /// <summary>
      /// PreferenceSet Interface
      /// </summary>
      private readonly IPreferenceSet _prefs;

      /// <summary>
      /// Protein Collection Interface
      /// </summary>
      private readonly IProteinCollection _proteinCollection;

      /// <summary>
      /// Protein Collection Interface
      /// </summary>
      private readonly IProteinBenchmarkContainer _benchmarkContainer;

      public ClientInstanceFactory(IPreferenceSet prefs, IProteinCollection proteinCollection, IProteinBenchmarkContainer benchmarkContainer)
      {
         _prefs = prefs;
         _proteinCollection = proteinCollection;
         _benchmarkContainer = benchmarkContainer;
      }
   
      public ReadOnlyCollection<ClientInstance> HandleImportResults(ICollection<ClientInstanceSettings> results)
      {
         if (results == null) throw new ArgumentNullException("results");
      
         var clientInstances = new List<ClientInstance>(results.Count);
         foreach (var settings in results)
         {
            if (String.IsNullOrEmpty(settings.ImportError) == false)
            {
               HfmTrace.WriteToHfmConsole(TraceLevel.Error, settings.ImportError);
               continue;
            }
            var clientInstance = CreateWrapper(settings);
            if (clientInstance != null)
            {
               clientInstances.Add(clientInstance);
            }
         }

         return clientInstances.AsReadOnly();
      }

      private ClientInstance CreateWrapper(ClientInstanceSettings settings)
      {
         try
         {
            return Create(settings);
         }
         catch (InvalidOperationException ex)
         {
            HfmTrace.WriteToHfmConsole(ex);
            return null;
         }
      }
      
      public ClientInstance Create(ClientInstanceSettings settings)
      {
         if (settings == null) throw new ArgumentNullException("settings");
         if (settings.InstanceNameEmpty)
         {
            throw new InvalidOperationException("No Instance Name is given.  Will not create Client Instance.");
         }
         
         if (settings.PathEmpty)
         {
            throw new InvalidOperationException("No Instance Path is given.  Will not create Client Instance.");
         }
      
         string preCleanInstanceName = settings.InstanceName;
         ICollection<string> cleanupWarnings = settings.CleanupSettings();
         if (settings.InstanceNameError)
         {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, 
               "Instance Name '{0}' is not valid after cleaning.  Will not create Client Instance.", preCleanInstanceName));
         }
         
         if (cleanupWarnings.Count != 0)
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, "------------------------");
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, "Client Settings Warnings");
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, "------------------------");
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, String.Format(CultureInfo.CurrentCulture,
                                                           "Instance Name: {0}", settings.InstanceName));

            foreach (var error in cleanupWarnings)
            {
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, error);
            }
         }

         return new ClientInstance(_prefs, _proteinCollection, _benchmarkContainer, settings);
      }
   }
}
