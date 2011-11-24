﻿/*
 * HFM.NET - Application Boot Strapper
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
using System.IO;
using System.Windows.Forms;

using Castle.Core.Logging;

using harlam357.Windows.Forms;

using HFM.Core;
using HFM.Forms;

namespace HFM
{
   internal sealed class BootStrapper
   {
      private readonly IPreferenceSet _prefs;
      private readonly ILogger _logger;

      public BootStrapper(IPreferenceSet prefs, ILogger logger)
      {
         _prefs = prefs;
         _logger = logger;
      }

      public void Strap(string[] args)
      {
         #region Parse Arguments

         ICollection<Argument> arguments;
         try
         {
            arguments = Arguments.Parse(args);
         }
         catch (FormatException ex)
         {
            // show usage dialog
            ShowStartupError(ex, null);
            return;
         }

         #endregion

         #region Process Arguments
         
         var processor = ServiceLocator.Resolve<ArgumentProcessor>();
         if (!processor.Process(arguments))
         {
            // arguments specified to exit the application
            return;
         }

         #endregion

         // Issue 180 - Restore the already running instance to the screen.
         using (var singleInstance = new SingleInstanceHelper())
         {
            #region Check Single Instance

            try
            {
               if (!singleInstance.Start())
               {
                  SingleInstanceHelper.SignalFirstInstance(args);
                  return;
               }
            }
            catch (Exception ex)
            {
               ShowStartupError(ex, "Failed to signal first instance of HFM.NET.  Please try starting HFM.NET again before reporting this issue.");
               return;
            }

            #endregion

            #region Setup Logging

            // create messages view (hooks into logging messages)
            ServiceLocator.Resolve<IMessagesView>();
            // write log header
            _logger.Info(String.Empty);
            _logger.Info(String.Format(CultureInfo.InvariantCulture, "Starting - HFM.NET v{0}", Core.Application.VersionWithRevision));
            _logger.Info(String.Empty);
            // check for Mono runtime
            if (Core.Application.IsRunningOnMono)
            {
               _logger.Info("Running on Mono...");
            }

            #endregion

            #region Setup Cache Folder

            try
            {
               ClearCacheFolder();
            }
            catch (Exception ex)
            {
               ShowStartupError(ex, "Failed to create or clear the data cache folder.");
               return;
            }

            #endregion

            #region Load Plugins

            var pluginLoader = ServiceLocator.Resolve<Core.Plugins.PluginLoader>();
            pluginLoader.Load();

            #endregion

            #region Register IPC Channel

            try
            {
               SingleInstanceHelper.RegisterIpcChannel(NewInstanceDetected);
            }
            catch (Exception ex)
            {
               ShowStartupError(ex, "Single Instance IPC channel failed to register.");
               return;
            }

            #endregion

            #region Initialize Main View

            IMainView frm;
            try
            {
               frm = ServiceLocator.Resolve<IMainView>();
               frm.Initialize(ServiceLocator.Resolve<MainPresenter>());
               //frm.WorkUnitHistoryMenuEnabled = connectionOk;
            }
            catch (Exception ex)
            {
               ShowStartupError(ex, "Primary UI failed to initialize.");
               return;
            }

            #endregion

            #region Register the Unhandled Exception Dialog

            ExceptionDialog.RegisterForUnhandledExceptions(Core.Application.NameAndVersionWithRevision,
               Environment.OSVersion.VersionString, ExceptionLogger);

            #endregion

            System.Windows.Forms.Application.Run((Form)frm);
         }
      }

      private void NewInstanceDetected(object sender, NewInstanceDetectedEventArgs e)
      {
         //var mainView = ServiceLocator.Resolve<IMainView>();
         //mainView.SecondInstanceStarted(e.Args);
      }

      private static void ExceptionLogger(Exception ex)
      {
         var logger = ServiceLocator.Resolve<ILogger>();
         logger.ErrorFormat(ex, "{0}", ex.Message);
      }

      internal static void ShowStartupError(Exception ex, string message)
      {
         ExceptionDialog.ShowErrorDialog(ex, Core.Application.NameAndVersionWithRevision, Environment.OSVersion.VersionString,
            message, Constants.GoogleGroupUrl, true);
      }
      
      /// <summary>
      /// Clears the log cache folder specified by the CacheFolder setting
      /// </summary>
      private void ClearCacheFolder()
      {
         string path = Path.Combine(_prefs.CacheDirectory, _prefs.GetPreference<string>(Preference.CacheFolder));
         var di = new DirectoryInfo(path);
         if (!di.Exists)
         {
            di.Create();
         }
         else
         {
            foreach (var fi in di.GetFiles())
            {
               try
               {
                  fi.Delete();
               }
               catch (Exception ex)
               {
                  _logger.WarnFormat(ex, "Failed to clear cache file '{0}'.", fi.Name);
               }
            }
         }
      }
   }
}
