﻿/*
 * HFM.NET - FahClient Class
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

using HFM.Client.DataTypes;

namespace HFM.Client
{
   public class FahClient : MessageCache
   {
      /// <summary>
      /// Create a FahClient.
      /// </summary>
      public FahClient()
         : this(new TcpClientFactory())
      {

      }

      /// <summary>
      /// Create a FahClient.
      /// </summary>
      public FahClient(ITcpClientFactory tcpClientFactory)
         : base(tcpClientFactory)
      {

      }

      public T GetMessage<T>() where T : TypedMessage
      {
         if (typeof(T).Equals(typeof(Heartbeat)))
         {
            var jsonMessage = GetJsonMessage(JsonMessageKey.Heartbeat);
            if (jsonMessage != null)
            {
               return Heartbeat.Parse(jsonMessage) as T;
            }
         }
         else if (typeof(T).Equals(typeof(Info)))
         {
            var jsonMessage = GetJsonMessage(JsonMessageKey.Info);
            if (jsonMessage != null)
            {
               return Info.Parse(jsonMessage) as T;
            }
         }
         else if (typeof(T).Equals(typeof(Options)))
         {
            var jsonMessage = GetJsonMessage(JsonMessageKey.Options);
            if (jsonMessage != null)
            {
               return Options.Parse(jsonMessage) as T;
            }
         }
         else if (typeof(T).Equals(typeof(Queue)))
         {
            var jsonMessage = GetJsonMessage(JsonMessageKey.QueueInfo);
            if (jsonMessage != null)
            {
               return Queue.Parse(jsonMessage) as T;
            }
         }
         else if (typeof(T).Equals(typeof(Slots)))
         {
            var jsonMessage = GetJsonMessage(JsonMessageKey.SlotInfo);
            if (jsonMessage != null)
            {
               return Slots.Parse(jsonMessage) as T;
            }
         }

         return null;
      }

      protected override void OnMessageUpdated(MessageUpdatedEventArgs e)
      {
         e.Type = MapKeyToType(e.Key);
         base.OnMessageUpdated(e);
      }

      private static Type MapKeyToType(string key)
      {
         switch (key)
         {
            case JsonMessageKey.Heartbeat:
               return typeof(Heartbeat);
            case JsonMessageKey.Info:
               return typeof(Info);
            case JsonMessageKey.Options:
               return typeof(Options);
            case JsonMessageKey.QueueInfo:
               return typeof(Queue);
            case JsonMessageKey.SlotInfo:
               return typeof(Slots);
         }

         return null;
      }
   }
}
