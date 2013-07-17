﻿/*
 * HFM.NET - Client Connection Class
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
using System.Net.Sockets;
using System.Text;
using System.Timers;

using HFM.Core.DataTypes;

namespace HFM.Client
{
   /// <summary>
   /// Folding@Home client connection class.  Provides functionality for connecting to a Folding@Home client, sending data, receiving data, and accessing the raw data returned by the client connection.
   /// </summary>
   public class Connection : IDisposable
   {
      public static readonly bool IsRunningOnMono = Type.GetType("Mono.Runtime") != null;

      #region Constants

      /// <summary>
      /// Internal Network Stream Buffer Size
      /// </summary>
      private const int InternalBufferSize = 1024;
      /// <summary>
      /// Default TcpClient Send Buffer Size
      /// </summary>
      private const int DefaultSendBufferSize = 1024 * 8;
      /// <summary>
      /// Default TcpClient Receive Buffer Size
      /// </summary>
      private const int DefaultReceiveBufferSize = 1024 * 8;
      /// <summary>
      /// Default Connection, Send, and Receive Timeout Length
      /// </summary>
      private const int DefaultTimeoutLength = 5000;
      /// <summary>
      /// Default Socket Receive Timer Length
      /// </summary>
      private const int DefaultSocketTimerLength = 10;

      #endregion

      #region Events

      /// <summary>
      /// Occurs when a status update is generated by the Connection.  The data provided by this event is purely informational and should not be used as an indication of the state of the Connection class instance.
      /// </summary>
      public event EventHandler<StatusMessageEventArgs> StatusMessage;
      /// <summary>
      /// Occurs when the value of the Connected property has changed.
      /// </summary>
      public event EventHandler<ConnectedChangedEventArgs> ConnectedChanged;
      /// <summary>
      /// Occurs when data is sent by the Connection.
      /// </summary>
      public event EventHandler<DataLengthEventArgs> DataLengthSent;
      /// <summary>
      /// Occurs when data is received by the Connection.
      /// </summary>
      public event EventHandler<DataLengthEventArgs> DataLengthReceived;

      #endregion

      #region Fields

      private ITcpClient _tcpClient;
      private INetworkStream _stream;
      private byte[] _internalBuffer;
      private readonly ITcpClientFactory _tcpClientFactory;
      private readonly StringBuilder _readBuffer;
      private readonly Timer _timer;

      private static readonly object BufferLock = new object();

      #endregion

      #region Properties

      internal byte[] InternalBuffer
      {
         get { return _internalBuffer; }
         set { _internalBuffer = value; }
      }

      /// <summary>
      /// Gets a value indicating whether the Connection is connected to a remote host.
      /// </summary>
      public bool Connected
      {
         get { return _tcpClient.Client == null ? false : _tcpClient.Connected; }
      }

      /// <summary>
      /// Gets a value that indicates whether data is available to be read from the buffer.
      /// </summary>
      public bool DataAvailable
      {
         get { return _readBuffer.Length != 0; }
      }

      /// <summary>
      /// Gets the read buffer for the Connection.
      /// </summary>
      protected StringBuilder ReadBuffer
      {
         get
         {
            lock (BufferLock)
            {
               return _readBuffer;
            }
         }
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
      /// Gets or sets the length of time to wait for a response to a connection request (default - 5 seconds).
      /// </summary>
      public int ConnectTimeout { get; set; }

      /// <summary>
      /// Gets or sets the length of time between each network stream read attempt (default - 1ms).
      /// </summary>
      public double ReceiveLoopTime
      {
         get { return _timer.Interval; }
         set { _timer.Interval = value; }
      }

      private int _sendBufferSize = DefaultSendBufferSize;

      /// <summary>
      /// Gets or sets the size of outgoing data buffer (default - 8k).
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

      private int _receiveBufferSize = DefaultReceiveBufferSize;

      /// <summary>
      /// Gets or sets the size of incoming data buffer (default - 8k).
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

      /// <summary>
      /// Gets or sets the debug flag on the receive buffer.  When true the receive buffer is written to a log file specified by the value of the DebugBufferFileName property.
      /// </summary>
      public bool DebugReceiveBuffer { get; set; }

      /// <summary>
      /// Gets or sets the debug file name value.  When the DebugReceiveBuffer property is true the receive buffer is written to a log file specified by this file name value.
      /// </summary>
      public string DebugBufferFileName { get; set; }

      #endregion

      #region Constructor

      /// <summary>
      /// Initializes a new instance of the Connection class.
      /// </summary>
      [CoverageExclude]
      public Connection()
         : this(new TcpClientFactory())
      {
         
      }

      /// <summary>
      /// Initializes a new instance of the Connection class.
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
         _timer.AutoReset = false;
      }

      #endregion

      #region Methods

      /// <summary>
      /// Connect to a Folding@Home client server.
      /// </summary>
      /// <param name="host">Hostname or IP address.</param>
      /// <param name="port">TCP port number.</param>
      /// <exception cref="InvalidOperationException">Connection is already connected.</exception>
      /// <exception cref="ArgumentNullException"><paramref name="host"/> is null.</exception>
      /// <exception cref="TimeoutException">Connection attempt timed out.</exception>
      public void Connect(string host, int port)
      {
         Connect(host, port, String.Empty);
      }

      /// <summary>
      /// Connect to a Folding@Home client server.
      /// </summary>
      /// <param name="host">Hostname or IP address.</param>
      /// <param name="port">TCP port number.</param>
      /// <param name="password">Server password.</param>
      /// <exception cref="InvalidOperationException">Connection is already connected.</exception>
      /// <exception cref="ArgumentNullException"><paramref name="host"/> or <paramref name="password"/> is null.</exception>
      /// <exception cref="TimeoutException">Connection attempt timed out.</exception>
      public void Connect(string host, int port, string password)
      {
         // check connection status, callers should make sure no connection exists first
         if (Connected) throw new InvalidOperationException("Client is already connected.");

         if (host == null) throw new ArgumentNullException("host");
         if (password == null) throw new ArgumentNullException("password");

         if (_tcpClient != null)
         {
            _tcpClient.Dispose();
         }
         _tcpClient = CreateClient();

         IAsyncResult ar = _tcpClient.BeginConnect(host, port, null, null);
         try
         {
            if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(ConnectTimeout), false))
            {
               _tcpClient.Close();
                     /*
                      * When running on Mono, TcpClient.Close() causes
                      * asynchronous access (coming from
                      * BeginConnect-related path) to AsyncWaitHandle.
                      * This opens a race window with 'finally' block below
                      * that closes said handle.
                      *
                      * Unfortunate chain of events results in the following:

Unhandled Exception: System.ObjectDisposedException: The object was used after being disposed.
  at System.Threading.WaitHandle.CheckDisposed ()
  at System.Threading.EventWaitHandle.Set () 
  at (wrapper remoting-invoke-with-check) System.Threading.EventWaitHandle:Set ()
  at System.Net.Sockets.Socket+SocketAsyncResult.set_IsCompleted (Boolean value)
  at System.Net.Sockets.Socket+SocketAsyncResult.Complete ()
  at System.Net.Sockets.Socket+SocketAsyncResult.Complete (System.Exception e)
  at System.Net.Sockets.Socket+Worker.Connect ()
  at System.Net.Sockets.Socket+Worker.DispatcherCB (System.Net.Sockets.SocketAsyncResult sar)

                      * As (in Mono) TcpClient.Close() signals AsyncWaitHandle, we can just
                      * wait on it before proceeding.
                      *
                      */
               if (IsRunningOnMono)
                  ar.AsyncWaitHandle.WaitOne();

               throw new TimeoutException("Client connection has timed out.");
            }

            _tcpClient.EndConnect(ar);
            _stream = _tcpClient.GetStream();

            if (password.Length != 0)
            {
               // send authentication
               SendCommand("auth " + password);
            }
            if (Connected)
            {
               // send connected event
               OnConnectedChanged(new ConnectedChangedEventArgs(true)); // maybe use Connected property?
               // send status message
               OnStatusMessage(new StatusMessageEventArgs(String.Format(CultureInfo.CurrentCulture, 
                  "Connected to {0}:{1}", host, port), TraceLevel.Info));
               // start listening for messages
               // from the network stream
               _timer.Start();
            }
         }
         finally
         {
            ar.AsyncWaitHandle.Close();
         } 
      }

      private ITcpClient CreateClient()
      {
         var tcpClient = _tcpClientFactory.Create();
         tcpClient.SendBufferSize = SendBufferSize;
         tcpClient.ReceiveBufferSize = ReceiveBufferSize;
         return tcpClient;
      }

      /// <summary>
      /// Close the connection to the Folding@Home client server.
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
         // send connected event
         OnConnectedChanged(new ConnectedChangedEventArgs(false)); // maybe use Connected property?
         // send status message
         OnStatusMessage(new StatusMessageEventArgs("Connection closed.", TraceLevel.Info));
      }

      /// <summary>
      /// Send a command to the Folding@Home client server.
      /// </summary>
      /// <param name="command">Command text.  Null, empty, or whitespace strings will be ignored.</param>
      /// <exception cref="InvalidOperationException">Connection is not connected.</exception>
      /// <remarks>Callers should make sure the Connection is connected by checking the value of the Connected property.</remarks>
      public void SendCommand(string command)
      {
         // check connection status, callers should make sure they're connected first
         if (!Connected) throw new InvalidOperationException("Connection is not connected.");

         if (command == null || command.Trim().Length == 0)
         {
            OnStatusMessage(new StatusMessageEventArgs("No command text given.", TraceLevel.Warning));
            return;
         }

         if (!command.EndsWith("\n", StringComparison.Ordinal))
         {
            command += "\n";
         }
         byte[] buffer = Encoding.ASCII.GetBytes(command);

#if SEND_ASYNC
         _stream.BeginWrite(buffer, 0, buffer.Length, WriteCallback, new AsyncData(command, buffer));
#else
         try
         {
            _stream.Write(buffer, 0, buffer.Length);
            // send data sent event
            OnDataLengthSent(new DataLengthEventArgs(buffer.Length));
            // send status message
            OnStatusMessage(new StatusMessageEventArgs(String.Format(CultureInfo.CurrentCulture,
               "Sent command: {0} ({1} bytes)", CleanUpCommandText(command), buffer.Length), TraceLevel.Info));
         }
         catch (Exception ex)
         {
            OnStatusMessage(new StatusMessageEventArgs(String.Format(CultureInfo.CurrentCulture,
               "Write failed: {0}", ex.Message), TraceLevel.Error));
            Close();
         }
#endif
      }

#if SEND_ASYNC      
      private struct AsyncData
      {
         private readonly string _command;
         public string Command
         {
            get { return _command; }
         }

         private readonly byte[] _buffer;
         public byte[] Buffer
         {
            get { return _buffer; }
         }

         public AsyncData(string command, byte[] buffer)
         {
            _command = command;
            _buffer = buffer;
         }
      }

      private void WriteCallback(IAsyncResult result)
      {
         var asyncData = (AsyncData)result.AsyncState;
         try
         {
            _stream.EndWrite(result);
            // send data sent event
            OnDataLengthSent(new DataLengthEventArgs(asyncData.Buffer.Length));
            // send status message
            OnStatusMessage(new StatusMessageEventArgs(String.Format(CultureInfo.CurrentCulture,
               "Sent command: {0} ({1} bytes)", CleanUpCommandText(asyncData.Command), asyncData.Buffer.Length), TraceLevel.Info));
         }
         catch (Exception ex)
         {
            OnStatusMessage(new StatusMessageEventArgs(String.Format(CultureInfo.CurrentCulture,
               "Write failed: {0}", ex.Message), TraceLevel.Error));
            Close();
         }
      }
#endif

      private static string CleanUpCommandText(string command)
      {
         Debug.Assert(command != null);
         return command.Replace("\n", String.Empty);
      }

      internal void SocketTimerElapsed(object sender, ElapsedEventArgs e)
      {
         Debug.Assert(Connected);
         try
         {
            Update();
         }
         catch (Exception ex)
         {
            //Console.WriteLine(DateTime.Now + " exception caught -- closing");
            if (!IsCancelBlockingCallSocketError(ex))
            {
               OnStatusMessage(new StatusMessageEventArgs(String.Format(CultureInfo.CurrentCulture,
                  "Update failed: {0}", ex.Message), TraceLevel.Error));
            }
            Close();
         }
         finally
         {
			if (_tcpClient.Connected)
            {
               ((Timer)sender).Start();
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
      /// Update the local data buffer with data from the remote network stream.
      /// </summary>
      protected virtual void Update()
      {
         int totalBytesRead = 0;
         do
         {
            int bytesRead = _stream.Read(_internalBuffer, 0, _internalBuffer.Length);
            if (bytesRead == 0)
            {
               throw new System.IO.IOException("The underlying socket has been closed.");
            }

            // lock so we're not appending to and reading from the buffer at the same time
            lock (BufferLock)
            {
               _readBuffer.Append(Encoding.ASCII.GetChars(_internalBuffer, 0, bytesRead));
            }

            totalBytesRead += bytesRead;
         } 
         while (_stream.DataAvailable);
         // send data received event
         OnDataLengthReceived(new DataLengthEventArgs(totalBytesRead));
         if (DebugReceiveBuffer && !String.IsNullOrEmpty(DebugBufferFileName))
         {
            try
            {
               System.IO.File.AppendAllText(DebugBufferFileName, 
                  _readBuffer.ToString().Replace("\n", Environment.NewLine).Replace("\\n", Environment.NewLine));
            }
            catch (Exception ex)
            {
               OnStatusMessage(new StatusMessageEventArgs(String.Format(CultureInfo.CurrentCulture,
                  "Debug buffer write failed: {0}", ex.Message), TraceLevel.Error));
            }
         }
      }

      /// <summary>
      /// Get the value of the local data buffer and clear that value from the local data buffer.
      /// </summary>
      /// <returns>The buffer value as a string.</returns>
      /// <remarks>If the buffer value is large this string allocation may end up on the Large Object Heap.</remarks>
      public string GetBuffer()
      {
         return GetBuffer(true);
      }

      /// <summary>
      /// Get the value of the local data buffer and optionally clear that value from the local data buffer.
      /// </summary>
      /// <param name="clear">true to clear the local data buffer.</param>
      /// <returns>The buffer value as a string.</returns>
      /// <remarks>If the buffer value is large this string allocation may end up on the Large Object Heap.</remarks>
      public string GetBuffer(bool clear)
      {
         // lock so we're not append to and reading from the buffer at the same time
         lock (BufferLock)
         {
            string value = _readBuffer.ToString();
            if (clear) _readBuffer.Clear();
            return value;
         }
      }

      /// <summary>
      /// Get the value of the local data buffer and clear that value from the local data buffer.
      /// </summary>
      /// <returns>The buffer value in an enumerable collection of up to 8000 element char arrays.</returns>
      public IEnumerable<char[]> GetBufferChunks()
      {
         return GetBufferChunks(true);
      }

      /// <summary>
      /// Get the value of the local data buffer and optionally clear that value from the local data buffer.
      /// </summary>
      /// <param name="clear">true to clear the local data buffer.</param>
      /// <returns>The buffer value in an enumerable collection of up to 8000 element char arrays.</returns>
      public IEnumerable<char[]> GetBufferChunks(bool clear)
      {
         // lock so we're not append to and reading from the buffer at the same time
         lock (BufferLock)
         {
            IEnumerable<char[]> value = _readBuffer.GetChunks();
            if (clear) _readBuffer.Clear();
            return value;
         }
      }

      /// <summary>
      /// Clear the value of the local data buffer.
      /// </summary>
      public void ClearBuffer()
      {
         // lock so we're not append to and reading from the buffer at the same time
         lock (BufferLock)
         {
            _readBuffer.Clear();
         }
      }

      /// <summary>
      /// Raise the StatusMessage event.
      /// </summary>
      /// <param name="e">Event arguments (if null the event is cancelled).</param>
      protected virtual void OnStatusMessage(StatusMessageEventArgs e)
      {
         if (e == null) return;

         Debug.WriteLine(e.Status);
         if (StatusMessage != null)
         {
            StatusMessage(this, e);
         }
      }

      private void OnConnectedChanged(ConnectedChangedEventArgs e)
      {
         if (ConnectedChanged != null)
         {
            ConnectedChanged(this, e);
         }
      }

      private void OnDataLengthSent(DataLengthEventArgs e)
      {
         if (DataLengthSent != null)
         {
            DataLengthSent(this, e);
         }
      }

      private void OnDataLengthReceived(DataLengthEventArgs e)
      {
         if (DataLengthReceived != null)
         {
            DataLengthReceived(this, e);
         }
      }

      #endregion

      #region IDisposable Members

      private bool _disposed;

      /// <summary>
      /// Releases all resources used by the Connection.
      /// </summary>
      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// Releases the unmanaged resources used by the Connection and optionally releases the managed resources.
      /// </summary>
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

      /// <summary>
      /// Releases unmanged resources used by the Connection.
      /// </summary>
      ~Connection()
      {
         Dispose(false);
      }

      #endregion
   }

   /// <summary>
   /// Provides data for status message events of a Connection. This class cannot be inherited.
   /// </summary>
   public sealed class StatusMessageEventArgs : EventArgs
   {
      /// <summary>
      /// Gets the status message text.
      /// </summary>
      public string Status { get; private set; }

      /// <summary>
      /// Gets the trace level of the status message.
      /// </summary>
      public TraceLevel Level { get; private set; }

      /// <summary>
      /// Initializes a new instance of the StatusMessageEventArgs class.
      /// </summary>
      /// <param name="status">The status message text.</param>
      /// <param name="level">The status message trace level.</param>
      /// <exception cref="ArgumentNullException"><paramref name="status"/> is null.</exception>
      public StatusMessageEventArgs(string status, TraceLevel level)
      {
         if (status == null) throw new ArgumentNullException("status");

         Status = status;
         Level = level;
      }
   }

   /// <summary>
   /// Provides data for connection status events of a Connection. This class cannot be inherited.
   /// </summary>
   public sealed class ConnectedChangedEventArgs : EventArgs
   {
      /// <summary>
      /// Gets the connection status.
      /// </summary>
      public bool Connected { get; private set; }

      internal ConnectedChangedEventArgs(bool connected)
      {
         Connected = connected;
      }
   }

   /// <summary>
   /// Provides data for data length events of a Connection. This class cannot be inherited.
   /// </summary>
   public sealed class DataLengthEventArgs : EventArgs
   {
      /// <summary>
      /// Gets the data length in bytes.
      /// </summary>
      public int DataLength { get; private set; }

      internal DataLengthEventArgs(int dataLength)
      {
         DataLength = dataLength;
      }
   }
}
