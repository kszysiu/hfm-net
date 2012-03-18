﻿/*
 * HFM.NET - MessageCache Class
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

using HFM.Client.DataTypes;

namespace HFM.Client
{
   /// <summary>
   /// Folding@Home client message cache class.  Provides functionality for parsing, storing, and accessing the raw JSON messages returned by the Folding@Home client.
   /// </summary>
   public class MessageCache : Connection
   {
      #region Events

      /// <summary>
      /// Occurs when an updated message is received.
      /// </summary>
      public event EventHandler<MessageUpdatedEventArgs> MessageUpdated;
      /// <summary>
      /// Occurs when the local data buffer update is finished.
      /// </summary>
      public event EventHandler UpdateFinished;
      
      #endregion

      #region Constants

      private const string LineFeed = "\n";
      private const string CarriageReturnLineFeed = "\r\n";
      private const string PyonHeader = "PyON 1 ";
      private const string PyonFooter = "---";

      #endregion

      #region Fields

      private readonly StringBuilder _readBuffer;
      private readonly Dictionary<string, JsonMessage> _messages;

      #endregion

      #region Constructor

      /// <summary>
      /// Initializes a new instance of the MessageCache class.
      /// </summary>
      [CoverageExclude]
      public MessageCache()
         : this(new TcpClientFactory())
      {

      }

      /// <summary>
      /// Initializes a new instance of the MessageCache class.
      /// </summary>
      internal MessageCache(ITcpClientFactory tcpClientFactory)
         : base(tcpClientFactory)
      {
         _readBuffer = new StringBuilder();
         _messages = new Dictionary<string, JsonMessage>();
      }

      #endregion

      #region Methods

      /// <summary>
      /// Get a Folding@Home client message as a JSON value.  Returns null if the requested message is not available in the cache.
      /// </summary>
      /// <param name="key">JSON message key.</param>
      public JsonMessage GetJsonMessage(string key)
      {
         return _messages.ContainsKey(key) ? _messages[key] : null;
      }

      /// <summary>
      /// Update the local data buffer with data from the remote network stream.
      /// </summary>
      protected override void Update()
      {
         // first, update the connection's buffer
         base.Update();

         // get the connection buffer and clear the connection buffer
         _readBuffer.Append(GetBuffer());
         string bufferValue = _readBuffer.ToString();
         _readBuffer.Clear();

         JsonMessage json;
         while ((json = GetNextJsonMessage(ref bufferValue)) != null)
         {
            UpdateMessageCache(json);
            OnMessageUpdated(new MessageUpdatedEventArgs(json.Key));
         }
         _readBuffer.Append(bufferValue);
         // send update finished event
         OnUpdateFinished(EventArgs.Empty);
      }

      /// <summary>
      /// Parse first message from the data buffer and remove it from the buffer value.  The remaining buffer value is returned to the caller.
      /// </summary>
      /// <param name="buffer">Data buffer value.</param>
      /// <returns>JsonMessage or null if no message is available in the buffer.</returns>
      internal static JsonMessage GetNextJsonMessage(ref string buffer)
      {
         if (buffer == null) return null;

         // find the header
         int messageIndex = buffer.IndexOf(PyonHeader, StringComparison.Ordinal);
         if (messageIndex < 0)
         {
            return null;
         }
         // set starting message index
         messageIndex += PyonHeader.Length;

         // find the first CrLf or Lf character after the header
         int startIndex = FindStartIndex(buffer, messageIndex);
         if (startIndex < 0) return null;

         // find the footer
         int endIndex = FindEndIndex(buffer, startIndex);
         if (endIndex < 0)
         {
            return null;
         }

         // create the message and set received time stamp
         var message = new JsonMessage { Received = DateTime.UtcNow };
         // get the message name
         message.Key = buffer.Substring(messageIndex, startIndex - messageIndex);

         // get the PyON message
         string pyon = buffer.Substring(startIndex, endIndex - startIndex);
         // replace PyON values with JSON values
         message.Value = pyon.Replace(": None", ": null");

         // set the index so we know where to trim the string (end plus footer length)
         int nextStartIndex = endIndex + PyonFooter.Length;
         // if more buffer is available set it and return, otherwise set the buffer empty
         buffer = nextStartIndex < buffer.Length ? buffer.Substring(nextStartIndex) : String.Empty;

         return message;
      }

      private static int FindStartIndex(string buffer, int messageIndex)
      {
         int index = buffer.IndexOf(CarriageReturnLineFeed, messageIndex, StringComparison.Ordinal);
         return index >= 0 ? index : buffer.IndexOf(LineFeed, messageIndex, StringComparison.Ordinal);
      }

      private static int FindEndIndex(string buffer, int startIndex)
      {
         int index = buffer.IndexOf(String.Concat(CarriageReturnLineFeed, PyonFooter, CarriageReturnLineFeed), startIndex, StringComparison.Ordinal);
         if (index >= 0)
         {
            return index;
         }

         index = buffer.IndexOf(String.Concat(LineFeed, PyonFooter, LineFeed), startIndex, StringComparison.Ordinal);
         if (index >= 0)
         {
            return index;
         }

         //index = buffer.IndexOf(PyonFooter, startIndex, StringComparison.Ordinal);
         //if (index >= 0)
         //{
         //   return index;
         //}

         return -1;
      }

      private void UpdateMessageCache(JsonMessage message)
      {
         _messages[message.Key] = message;
         OnStatusMessage(new StatusMessageEventArgs(String.Format(CultureInfo.CurrentCulture,
            "Received message: {0} ({1} bytes)", message.Key, message.Value.Length), TraceLevel.Info));
      }

      /// <summary>
      /// Raise the MessageUpdated event.
      /// </summary>
      /// <param name="e">Event arguments (if null the event is cancelled).</param>
      protected virtual void OnMessageUpdated(MessageUpdatedEventArgs e)
      {
         if (e == null) return;

         if (MessageUpdated != null)
         {
            MessageUpdated(this, e);
         }
      }

      private void OnUpdateFinished(EventArgs e)
      {
         if (UpdateFinished != null)
         {
            UpdateFinished(this, e);
         }
      }

      #endregion
   }

   /// <summary>
   /// Provides data for message updated events of a MessageCache connection.
   /// </summary>
   public class MessageUpdatedEventArgs : EventArgs
   {
      /// <summary>
      /// Gets the message key that was updated.
      /// </summary>
      public string Key { get; private set; }
      /// <summary>
      /// Gets the message data type that was updated (FahClient connections only).
      /// </summary>
      public Type DataType { get; internal set; }

      /// <summary>
      /// Initializes a new instance of the MessageUpdatedEventArgs class.
      /// </summary>
      public MessageUpdatedEventArgs(string key)
      {
         Key = key;
      }
   }

   /// <summary>
   /// Folding@Home client message keys for JSON messages.
   /// </summary>
   public static class JsonMessageKey
   {
      /// <summary>
      /// Heartbeat Message Key.
      /// </summary>
      public const string Heartbeat = "heartbeat";
      /// <summary>
      /// Info Message Key.
      /// </summary>
      public const string Info = "info";
      /// <summary>
      /// Options Message Key.
      /// </summary>
      public const string Options = "options";
      /// <summary>
      /// Simulation Info Message Key.
      /// </summary>
      /// <remarks>This message is in response to a command that takes a slot id argument.</remarks>
      public const string SimulationInfo = "simulation-info";
      /// <summary>
      /// Slot Info Message Key.
      /// </summary>
      public const string SlotInfo = "slots";
      /// <summary>
      /// Slot Options Message Key.
      /// </summary>
      /// <remarks>This message is in response to a command that takes a slot id argument.</remarks>
      public const string SlotOptions = "slot-options";
      /// <summary>
      /// Queue Info Message Key.
      /// </summary>
      public const string QueueInfo = "units";

      /// <summary>
      /// Log Restart Message Key.
      /// </summary>
      public const string LogRestart = "log-restart";
      /// <summary>
      /// Log Update Message Key.
      /// </summary>
      public const string LogUpdate = "log-update";
   }
}
