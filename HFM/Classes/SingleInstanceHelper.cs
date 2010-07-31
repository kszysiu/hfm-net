/*
 * HFM.NET - Single Instance Helper Class
 * Copyright (C) 2010 Ryan Harlamert (harlam357)
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

/*
 * Class based primarily on code from: http://www.codeproject.com/KB/threads/SingleInstancingWithIpc.aspx
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;

using HFM.Forms;
using HFM.Framework;

namespace HFM.Classes
{
   public static class SingleInstanceHelper
   {
      private static Mutex _mutex;
      
      private const string ObjectName = "SingleInstanceProxy";
      private static readonly string MutexName = String.Format(CultureInfo.InvariantCulture, "Global\\{0}", PlatformOps.AssemblyGuid);

      public static bool Start()
      {
         bool onlyInstance;
         _mutex = new Mutex(true, MutexName, out onlyInstance);
         return onlyInstance;
      }
      
      public static void RegisterIpcChannel(frmMain frm)
      {
         IChannel ipcChannel = new IpcServerChannel(PlatformOps.AssemblyGuid);
         ChannelServices.RegisterChannel(ipcChannel, false);

         IpcObject obj = new IpcObject(frm.SecondInstanceStarted);
         RemotingServices.Marshal(obj, ObjectName);
      }
      
      public static void SignalFirstInstance(string[] args)
      {
         string objectUri = String.Format(CultureInfo.InvariantCulture, "ipc://{0}/{1}", PlatformOps.AssemblyGuid, ObjectName);

         IChannel ipcChannel = new IpcClientChannel();
         ChannelServices.RegisterChannel(ipcChannel, false);

         IpcObject obj = (IpcObject)Activator.GetObject(typeof(IpcObject), objectUri);
         obj.SignalNewInstance(args);
      }

      public static void Stop()
      {
         _mutex.ReleaseMutex();
      }
   }

   public delegate void NewInstanceHandler(string[] args);

   public class IpcObject : MarshalByRefObject
   {
      [SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
      public event NewInstanceHandler NewInstance;

      public IpcObject(NewInstanceHandler handler)
      {
         NewInstance += handler;
      }

      public void SignalNewInstance(string[] args)
      {
         NewInstance(args);
      }

      // Make sure the object exists "forever"
      public override object InitializeLifetimeService()
      {
         return null;
      }
   }
}
