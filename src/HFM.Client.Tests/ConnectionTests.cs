﻿/*
 * HFM.NET - Client Connection Class Tests
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
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using NUnit.Framework;
using Rhino.Mocks;

namespace HFM.Client.Tests
{
   [TestFixture]
   public class ConnectionTests
   {
      private ITcpClient _tcpClient;
      private INetworkStream _stream;

      private ITcpClientFactory CreateClientFactory()
      {
         var tcpClientFactory = MockRepository.GenerateStub<ITcpClientFactory>();
         _tcpClient = MockRepository.GenerateMock<ITcpClient>();
         _stream = MockRepository.GenerateMock<INetworkStream>();
         tcpClientFactory.Stub(x => x.Create()).Return(_tcpClient);
         return tcpClientFactory;
      }

      [Test]
      public void TimeAndBufferValueTest1()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            Assert.AreEqual(2000, connection.ConnectTimeout);
            Assert.AreEqual(1, connection.ReceiveLoopTime);
            Assert.AreEqual(1024 * 8, connection.SendBufferSize);
            Assert.AreEqual(1024 * 8, connection.ReceiveBufferSize);
         }
      }

      [Test]
      public void TimeAndBufferValueTest2()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            connection.ConnectTimeout = 3000;
            Assert.AreEqual(3000, connection.ConnectTimeout);
            connection.ReceiveLoopTime = 1500;
            Assert.AreEqual(1500, connection.ReceiveLoopTime);

            _tcpClient.Expect(x => x.SendBufferSize = 2048);
            _tcpClient.Expect(x => x.ReceiveBufferSize = 2048);

            connection.SendBufferSize = 2048;
            Assert.AreEqual(2048, connection.SendBufferSize);
            connection.ReceiveBufferSize = 2048;
            Assert.AreEqual(2048, connection.ReceiveBufferSize);
         }

         _tcpClient.VerifyAllExpectations();
      }

      [Test]
      public void ConnectedTest1()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            Assert.IsFalse(connection.Connected);
         }
      }

      [Test]
      public void ConnectedTest2()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            _tcpClient.Expect(x => x.Client).Return(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
            _tcpClient.Expect(x => x.Connected).Return(false);
            Assert.IsFalse(connection.Connected);
         }

         _tcpClient.VerifyAllExpectations();
      }

      [Test]
      public void ConnectedTest3()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            _tcpClient.Expect(x => x.Client).Return(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
            _tcpClient.Expect(x => x.Connected).Return(true);
            Assert.IsTrue(connection.Connected);
         }

         _tcpClient.VerifyAllExpectations();
      }

      [Test]
      public void UpdateEnabledTest1()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            Connect(connection);

            Assert.IsTrue(connection.UpdateEnabled);
            connection.UpdateEnabled = false;
            Assert.IsFalse(connection.UpdateEnabled);
         }
      }

      [Test]
      public void ConnectTest1()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            SetupSuccessfulConnectionExpectations(_tcpClient, _stream);

            bool connectedChangedFired = false;
            bool statusMessageFired = false;
            connection.ConnectedChanged += (sender, args) => connectedChangedFired = true;
            connection.StatusMessage += (sender, args) => statusMessageFired = true;
            // set to 5 minutes so the update loop never gets a chance to fire
            connection.ReceiveLoopTime = 300000;
            connection.Connect("server", 10000, "password");

            Assert.IsTrue(connectedChangedFired);
            Assert.IsTrue(statusMessageFired);
            Assert.IsTrue(connection.UpdateEnabled);
         }

         _tcpClient.VerifyAllExpectations();
         _stream.VerifyAllExpectations();
      }

      [Test]
      public void ConnectTest1NoPassword()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            SetupSuccessfulConnectionExpectations(_tcpClient, _stream, false);

            bool connectedChangedFired = false;
            bool statusMessageFired = false;
            connection.ConnectedChanged += (sender, args) => connectedChangedFired = true;
            connection.StatusMessage += (sender, args) => statusMessageFired = true;
            // set to 5 minutes so the update loop never gets a chance to fire
            connection.ReceiveLoopTime = 300000;
            connection.Connect("server", 10000);

            Assert.IsTrue(connectedChangedFired);
            Assert.IsTrue(statusMessageFired);
            Assert.IsTrue(connection.UpdateEnabled);
         }

         _tcpClient.VerifyAllExpectations();
         _stream.VerifyAllExpectations();
      }

      private static void SetupSuccessfulConnectionExpectations(ITcpClient tcpClient, INetworkStream stream, bool withPassword = true)
      {
         // client not connected
         tcpClient.Expect(x => x.Client).Return(null);

         // setup connect expectations
         var asyncResult = MockRepository.GenerateStub<IAsyncResult>();
         tcpClient.Expect(x => x.BeginConnect("server", 10000, null, null)).Return(asyncResult);
         asyncResult.Stub(x => x.AsyncWaitHandle).Return(new EventWaitHandle(true, EventResetMode.ManualReset));
         tcpClient.Expect(x => x.EndConnect(asyncResult));
         tcpClient.Expect(x => x.GetStream()).Return(stream);

         // setup Connected property expectations
         tcpClient.Expect(x => x.Client).Return(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)).Repeat.AtLeastOnce();
         tcpClient.Expect(x => x.Connected).Return(true).Repeat.AtLeastOnce();

         if (withPassword)
         {
            // setup SendCommand() expectation
            var buffer = Encoding.ASCII.GetBytes("auth password\n");
            stream.Expect(x => x.Write(buffer, 0, buffer.Length));
         }
      }

      private void Connect(Connection connection)
      {
         Connect(connection, _tcpClient, _stream);
      }

      internal static void Connect(Connection connection, ITcpClient tcpClient, INetworkStream stream)
      {
         SetupSuccessfulConnectionExpectations(tcpClient, stream);
         // set to 5 minutes so the update loop never gets a chance to fire
         connection.ReceiveLoopTime = 300000;
         connection.Connect("server", 10000, "password");
      }

      [Test]
      public void ConnectTest2()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            // client not connected
            _tcpClient.Expect(x => x.Client).Return(null);

            // setup connect expectations
            var asyncResult = MockRepository.GenerateStub<IAsyncResult>();
            _tcpClient.Expect(x => x.BeginConnect("server", 10000, null, null)).Return(asyncResult);
            asyncResult.Stub(x => x.AsyncWaitHandle).Return(new EventWaitHandle(false, EventResetMode.ManualReset));
            _tcpClient.Expect(x => x.Close());

            // set to 1/100 of a second so the test doesn't take long
            connection.ConnectTimeout = 10;
            try
            {
               connection.Connect("server", 10000, String.Empty);
               Assert.Fail("Connection attempt did not timeout as expected");
            }
            catch (TimeoutException)
            { }
            Assert.IsFalse(connection.UpdateEnabled);
         }

         _tcpClient.VerifyAllExpectations();
      }

      [Test]
      [ExpectedException(typeof(InvalidOperationException))]
      public void ConnectTest3()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            Connect(connection);

            Assert.IsTrue(connection.Connected);
            // cannot connect again if a connection is already established
            connection.Connect("server", 10000, "password");
         }
      }

      [Test]
      [ExpectedException(typeof(ArgumentNullException))]
      public void ConnectTest4()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            connection.Connect(null, 10000, String.Empty);
         }
      }

      [Test]
      [ExpectedException(typeof(ArgumentNullException))]
      public void ConnectTest5()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            connection.Connect("server", 10000, null);
         }
      }

      [Test]
      public void CloseTest()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            Connect(connection);

            _stream.Expect(x => x.Close());
            _tcpClient.Expect(x => x.Close());

            bool statusMessageFired = false;
            connection.StatusMessage += (sender, args) => statusMessageFired = true;
            connection.Close();

            Assert.IsTrue(statusMessageFired);
         }

         _tcpClient.VerifyAllExpectations();
         _stream.VerifyAllExpectations();
      }

      [Test]
      [ExpectedException(typeof(InvalidOperationException))]
      public void SendCommandTest1()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            connection.SendCommand("command");
         }
      }

      [Test]
      public void SendCommandTest2()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            Connect(connection);

            bool dataLengthSentFired = false;
            bool statusMessageFired = false;
            connection.DataLengthSent += (sender, args) => dataLengthSentFired = true;
            connection.StatusMessage += (sender, args) => statusMessageFired = true;
            var buffer = Encoding.ASCII.GetBytes("command\n");
            _stream.Expect(x => x.Write(buffer, 0, buffer.Length));
            connection.SendCommand("command");

            Assert.IsTrue(dataLengthSentFired);
            Assert.IsTrue(statusMessageFired);
         }

         _tcpClient.VerifyAllExpectations();
         _stream.VerifyAllExpectations();
      }

      [Test]
      public void SendCommandTest3()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            Connect(connection);

            bool dataLengthSentFired = false;
            bool statusMessageFired = false;
            bool connectedChangedFired = false;
            connection.DataLengthSent += (sender, args) => dataLengthSentFired = true;
            connection.StatusMessage += (sender, args) => statusMessageFired = true;
            connection.ConnectedChanged += (sender, args) => connectedChangedFired = true;
            var buffer = Encoding.ASCII.GetBytes("command\n");
            _stream.Expect(x => x.Write(buffer, 0, buffer.Length)).Throw(new IOException("Write failed."));
            connection.SendCommand("command");

            Assert.IsFalse(dataLengthSentFired);
            Assert.IsTrue(statusMessageFired);
            Assert.IsTrue(connectedChangedFired);
         }

         _tcpClient.VerifyAllExpectations();
         _stream.VerifyAllExpectations();
      }

      #region SendCommand - Null, Empty, & Whitespace Tests

      [Test]
      public void SendCommandNullTest()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            Connect(connection);

            bool statusMessageFired = false;
            connection.StatusMessage += (sender, args) => statusMessageFired = true;
            connection.SendCommand(null);

            Assert.IsTrue(statusMessageFired);
         }

         _tcpClient.VerifyAllExpectations();
         _stream.VerifyAllExpectations();
      }

      [Test]
      public void SendCommandEmptyTest()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            Connect(connection);

            bool statusMessageFired = false;
            connection.StatusMessage += (sender, args) => statusMessageFired = true;
            connection.SendCommand(String.Empty);

            Assert.IsTrue(statusMessageFired);
         }

         _tcpClient.VerifyAllExpectations();
         _stream.VerifyAllExpectations();
      }

      [Test]
      public void SendCommandWhitespaceTest()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            Connect(connection);

            bool statusMessageFired = false;
            connection.StatusMessage += (sender, args) => statusMessageFired = true;
            connection.SendCommand("   ");

            Assert.IsTrue(statusMessageFired);
         }

         _tcpClient.VerifyAllExpectations();
         _stream.VerifyAllExpectations();
      }

      #endregion

      [Test]
      public void SocketTimerElapsedTest1()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            Connect(connection);

            bool dataLengthReceivedFired = false;
            connection.DataLengthReceived += (sender, args) => dataLengthReceivedFired = true;
            var buffer = connection.InternalBuffer;
            _stream.Expect(x => x.Read(buffer, 0, buffer.Length)).Do(
               new Func<byte[], int, int, int>(FillBufferWithTestData));

            connection.SocketTimerElapsed(null, null);

            Assert.IsTrue(dataLengthReceivedFired);
            // check GetBuffer() and DataAvailable
            Assert.IsTrue(connection.DataAvailable);
            string connectionBuffer = connection.GetBuffer(false);
            Assert.IsFalse(String.IsNullOrEmpty(connectionBuffer));
            Assert.IsTrue(connection.DataAvailable);
            connectionBuffer = connection.GetBuffer();
            Assert.IsFalse(String.IsNullOrEmpty(connectionBuffer));
            Assert.IsFalse(connection.DataAvailable);
         }

         _tcpClient.VerifyAllExpectations();
         _stream.VerifyAllExpectations();
      }

      [Test]
      public void SocketTimerElapsedTest2()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            Connect(connection);

            var buffer = connection.InternalBuffer;
            _stream.Expect(x => x.Read(buffer, 0, buffer.Length)).Do(
               new Func<byte[], int, int, int>(FillBufferWithTestData));

            connection.SocketTimerElapsed(null, null);

            // check ClearBuffer() and DataAvailable
            Assert.IsTrue(connection.DataAvailable);
            connection.ClearBuffer();
            Assert.IsFalse(connection.DataAvailable);
         }

         _tcpClient.VerifyAllExpectations();
         _stream.VerifyAllExpectations();
      }

      [Test]
      public void SocketTimerElapsedTest3()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            Connect(connection);

            var buffer = connection.InternalBuffer;
            // when Read returns 0 the connection has been lost and the
            // implementation throws an IOException
            _stream.Expect(x => x.Read(buffer, 0, buffer.Length)).Return(0);
            // as a result the connection is closed, set expectations
            _stream.Expect(x => x.Close());
            _tcpClient.Expect(x => x.Close());

            bool statusMessageFired = false;
            connection.StatusMessage += (sender, args) => statusMessageFired = true;
            connection.SocketTimerElapsed(null, null);

            Assert.IsTrue(statusMessageFired);
         }

         _tcpClient.VerifyAllExpectations();
         _stream.VerifyAllExpectations();
      }

      [Test]
      public void SocketTimerElapsedTest4()
      {
         using (var connection = new Connection(CreateClientFactory()))
         {
            Connect(connection);

            var buffer = connection.InternalBuffer;
            // when we call Close() ourselves then the Read() method called by
            // Update() will throw a WSACancelBlockingCall error code as the 
            // inner exception of an IOException.  in this case Close() does 
            // not need called again since it was called through the API.
            _stream.Expect(x => x.Read(buffer, 0, buffer.Length)).Throw(new IOException(String.Empty, new SocketException(10004)));

            connection.SocketTimerElapsed(null, null);
         }

         _tcpClient.VerifyAllExpectations();
         _stream.VerifyAllExpectations();
      }

      internal static int FillBufferWithTestData(byte[] buffer, int offset, int size)
      {
         string message = File.ReadAllText("..\\..\\..\\TestFiles\\Client_v7_1\\units.txt");
         var messageBytes = Encoding.ASCII.GetBytes(message);

         for (int i = 0; i < messageBytes.Length; i++)
         {
            buffer[i] = messageBytes[i];
         }
         return messageBytes.Length;
      }
   }
}
