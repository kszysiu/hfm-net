﻿/*
 * HFM.NET - Client Connection Class
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
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace HFM.Client
{
   public class Connection : IDisposable
   {
      #region Constants

      /// <summary>
      /// Internal Network Stream Buffer Size
      /// </summary>
      private const int InternalBufferSize = 1024;
      /// <summary>
      /// Default TcpClient Send Buffer Size
      /// </summary>
      private const int DefaultSendBufferSize = 8196;
      /// <summary>
      /// Default TcpClient Receive Buffer Size
      /// </summary>
      private const int DefaultReceiveBufferSize = 32768;
      /// <summary>
      /// Default Connection, Send, and Receive Timeout Length
      /// </summary>
      private const int DefaultTimeoutLength = 2000;
      /// <summary>
      /// Default Socket Receive Timer Length
      /// </summary>
      private const int DefaultSocketTimerLength = 100;

      #endregion

      #region Events

      /// <summary>
      /// Fired when a status event occurs.
      /// </summary>
      public event EventHandler<StatusMessageEventArgs> StatusMessage;

      #endregion

      #region Fields

      private ITcpClient _tcpClient;
      private INetworkStream _stream;
      private readonly ITcpClientFactory _tcpClientFactory;
      private readonly byte[] _internalBuffer;
      private readonly StringBuilder _readBuffer;
      private readonly Timer _timer;

      /// <summary>
      /// Don't allow Update() to be called more than once.
      /// </summary>
      private bool _updating;

      private static readonly object BufferLock = new object();

      #endregion

      #region Properties

      internal byte[] InternalBuffer
      {
         get { return _internalBuffer; }
      }

      /// <summary>
      /// Gets a value indicating whether the Connection is connected to a remote host.
      /// </summary>
      public bool Connected
      {
         get { return _tcpClient.Client == null ? false : _tcpClient.Connected; }
      }

      /// <summary>
      /// Gets a value that indicates whether data is available to be read.
      /// </summary>
      public bool DataAvailable
      {
         get { return _readBuffer.Length != 0; }
      }

      /// <summary>
      /// Gets or sets a value indicating whether the Connection should process updates.
      /// </summary>
      public bool UpdateEnabled
      {
         get { return _timer.Enabled; }
         set { _timer.Enabled = value; }
      }

      /// <summary>
      /// Length of time to wait for Connection response (default - 2 seconds).
      /// </summary>
      public int ConnectTimeout { get; set; }

      /// <summary>
      /// Length of time between each network stream read attempt (default - 100ms).
      /// </summary>
      public double ReceiveLoopTime
      {
         get { return _timer.Interval; }
         set { _timer.Interval = value; }
      }

      //private int _sendTimeout = DefaultTimeoutLength;

      ///// <summary>
      ///// Length of time to wait for a command to be sent (default - 2 seconds).
      ///// </summary>
      //public int SendTimeout
      //{
      //   get { return _sendTimeout; }
      //   set
      //   {
      //      _tcpClient.SendTimeout = value;
      //      _sendTimeout = value;
      //   }
      //}

      private int _sendBufferSize = DefaultSendBufferSize;

      /// <summary>
      /// Size of outgoing data buffer (default - 8k).
      /// </summary>
      public int SendBufferSize
      {
         get { return _sendBufferSize; }
         set
         {
            _tcpClient.SendBufferSize = value;
            _sendBufferSize = value;
         }
      }

      //private int _receiveTimeout = DefaultTimeoutLength;

      ///// <summary>
      ///// Length of time to wait for a response to be received (default - 2 seconds).
      ///// </summary>
      //public int ReceiveTimeout
      //{
      //   get { return _receiveTimeout; }
      //   set
      //   {
      //      _tcpClient.ReceiveTimeout = value;
      //      _receiveTimeout = value;
      //   }
      //}

      private int _receiveBufferSize = DefaultReceiveBufferSize;

      /// <summary>
      /// Size of incoming data buffer (default - 32k).
      /// </summary>
      public int ReceiveBufferSize
      {
         get { return _receiveBufferSize; }
         set
         {
            _tcpClient.ReceiveBufferSize = value;
            _receiveBufferSize = value;
         }
      }

      #endregion

      #region Constructor

      /// <summary>
      /// Create a Server Connection.
      /// </summary>
      public Connection()
         : this(new TcpClientFactory())
      {
         
      }

      /// <summary>
      /// Create a Server Connection.
      /// </summary>
      internal Connection(ITcpClientFactory tcpClientFactory)
      {
         ConnectTimeout = DefaultTimeoutLength;

         _tcpClientFactory = tcpClientFactory;
         _tcpClient = CreateClient();
         _internalBuffer = new byte[InternalBufferSize];
         _readBuffer = new StringBuilder();
         _timer = new Timer(DefaultSocketTimerLength);
         _timer.Elapsed += SocketTimerElapsed;
      }

      #endregion

      #region Methods
      
      /// <summary>
      /// Connect to a Server.
      /// </summary>
      /// <param name="host">Hostname or IP Address</param>
      /// <param name="port">TCP Port</param>
      /// <param name="password">Client Access Password</param>
      /// <exception cref="InvalidOperationException">Throws if the client is already connected.</exception>
      /// <exception cref="ArgumentNullException">Throws if either 'host' or 'password' argument is null.</exception>
      /// <exception cref="TimeoutException">Throws if connection attempt times out.</exception>
      public void Connect(string host, int port, string password)
      {
         // check connection status, callers should make sure no connection exists first
         if (Connected) throw new InvalidOperationException("Client is already connected.");

         if (host == null) throw new ArgumentNullException("host");
         if (password == null) throw new ArgumentNullException("password");

         _tcpClient = CreateClient();

         IAsyncResult ar = _tcpClient.BeginConnect(host, port, null, null);
         try
         {
            if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(ConnectTimeout), false))
            {
               _tcpClient.Close();
               throw new TimeoutException("Client connection has timed out.");
            }

            _tcpClient.EndConnect(ar);
            _stream = _tcpClient.GetStream();

            // send authentication
            SendCommand("auth " + password);
            // send status message
            OnStatusMessage(new StatusMessageEventArgs(String.Format(CultureInfo.CurrentCulture,
               "Connected to {0} ({1})", host, port), TraceLevel.Info));
            // start listening for messages
            // from the network stream
            _timer.Start();
         }
         finally
         {
            ar.AsyncWaitHandle.Close();
         } 
      }

      private ITcpClient CreateClient()
      {
         var tcpClient = _tcpClientFactory.Create();
         //tcpClient.SendTimeout = SendTimeout;
         tcpClient.SendBufferSize = SendBufferSize;
         //tcpClient.ReceiveTimeout = ReceiveTimeout;
         tcpClient.ReceiveBufferSize = ReceiveBufferSize;
         return tcpClient;
      }

      /// <summary>
      /// Close the Connection to the Server.
      /// </summary>
      public void Close()
      {
         // stop the timer
         _timer.Stop();
         // close the network stream
         if (_stream != null)
         {
            _stream.Close();
         }
         // remove reference to the network stream
         _stream = null;
         // close the actual connection
         _tcpClient.Close();
         // send status message
         OnStatusMessage(new StatusMessageEventArgs("Connection closed.", TraceLevel.Info));
      }

      /// <summary>
      /// Send a Command to the Server.
      /// </summary>
      /// <param name="command">Command Text.  Null, empty, or whitespace strings will be ignored.</param>
      /// <exception cref="InvalidOperationException">Throws if client is not connected.</exception>
      /// <remarks>Callers should make sure they're connected by checking the Connected property.</remarks>
      public void SendCommand(string command)
      {
         // check connection status, callers should make sure they're connected first
         if (!Connected) throw new InvalidOperationException("Client is not connected.");

         if (command.IsNullOrWhiteSpace())
         {
            OnStatusMessage(new StatusMessageEventArgs("No command text given.", TraceLevel.Warning));
            return;
         }

         if (!command.EndsWith("\n", StringComparison.Ordinal))
         {
            command += "\n";
         }
         var buffer = Encoding.ASCII.GetBytes(command);
         _stream.BeginWrite(buffer, 0, buffer.Length, null, null);
         // send status message
         OnStatusMessage(new StatusMessageEventArgs(String.Format(CultureInfo.CurrentCulture,
            "Sent command: {0}", command), TraceLevel.Info));
      }

      internal void SocketTimerElapsed(object sender, ElapsedEventArgs e)
      {
         Debug.Assert(Connected);

         if (!_updating)
         {
            try
            {
               Update();
            }
            catch (Exception ex)
            {
               if (!IsCancelBlockingCallSocketError(ex))
               {
                  OnStatusMessage(new StatusMessageEventArgs(String.Format(CultureInfo.CurrentCulture,
                     "Update failed: {0}", ex.Message), TraceLevel.Error));
                  Close();
               }
            }
         }
      }

      private static bool IsCancelBlockingCallSocketError(Exception ex)
      {
         var ioEx = ex as System.IO.IOException;
         if (ioEx != null)
         {
            var socketEx = ioEx.InnerException as SocketException;
            // code 10004 is WSACancelBlockingCall
            if (socketEx != null && socketEx.ErrorCode == 10004)
            {
               return true;
            }
         }
         return false;
      }

      /// <summary>
      /// Update the Data Buffer.
      /// </summary>
      protected virtual void Update()
      {
         // lock so we're not appending to and reading 
         // from the buffer at the same time
         lock (BufferLock)
         {
            try
            {
               _updating = true;

               do
               {
                  if (_stream.Read(_internalBuffer, 0, _internalBuffer.Length) == 0)
                  {
                     throw new System.IO.IOException("The underlying socket has been closed.");
                  }
                  _readBuffer.Append(Encoding.ASCII.GetString(_internalBuffer));
               } 
               while (_stream.DataAvailable);
            }
            finally
            {
               _updating = false;
            }
         }
      }

      /// <summary>
      /// Get the buffer value.
      /// </summary>
      /// <remarks>Automatically clears the Connection's buffer.</remarks>
      public string GetBuffer()
      {
         return GetBuffer(true);
      }

      /// <summary>
      /// Get the buffer value.
      /// </summary>
      /// <param name="clear">True to clear the Connection's buffer.</param>
      public string GetBuffer(bool clear)
      {
         // lock so we're not append to and reading 
         // from the buffer at the same time
         lock (BufferLock)
         {
            string value = _readBuffer.ToString();
            if (clear) _readBuffer.Clear();
            return value;
         }
      }

      /// <summary>
      /// Clear the buffer value.
      /// </summary>
      public void ClearBuffer()
      {
         // lock so we're not append to and reading 
         // from the buffer at the same time
         lock (BufferLock)
         {
            _readBuffer.Clear();
         }
      }

      /// <summary>
      /// Raise the StatusMessage Event.
      /// </summary>
      /// <param name="e">Event Arguments (if null the event is cancelled)</param>
      protected virtual void OnStatusMessage(StatusMessageEventArgs e)
      {
         if (e == null) return;

         Debug.WriteLine(e.Status);
         if (StatusMessage != null)
         {
            StatusMessage(this, e);
         }
      }

      #endregion

      #region IDisposable Members

      private bool _disposed;

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      protected virtual void Dispose(bool disposing)
      {
         if (!_disposed)
         {
            if (disposing)
            {
               // close connection
               Close();
               // dispose of timer
               _timer.Dispose();
            }
         }

         _disposed = true;
      }

      ~Connection()
      {
         Dispose(false);
      }

      #endregion
   }

   public class StatusMessageEventArgs : EventArgs
   {
      public string Status { get; private set; }

      public TraceLevel Level { get; private set; }

      internal StatusMessageEventArgs(string status, TraceLevel level)
      {
         if (status == null) throw new ArgumentNullException("status");

         Status = CleanUpStatusMessage(status);
         Level = level;
      }

      private static string CleanUpStatusMessage(string status)
      {
         Debug.Assert(status != null);

         int newLine = status.IndexOf(Environment.NewLine, StringComparison.OrdinalIgnoreCase);
         if (newLine > -1)
         {
            return status.Substring(0, newLine);
         }
         int lineFeed = status.IndexOf("\n", StringComparison.OrdinalIgnoreCase);
         if (lineFeed > -1)
         {
            return status.Substring(0, lineFeed);
         }
         return status;
      }
   }
}
