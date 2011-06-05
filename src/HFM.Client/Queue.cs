﻿/*
 * HFM.NET - Queue Data Class
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

using System.Collections.Generic;
using System.ComponentModel;

using Newtonsoft.Json.Linq;

namespace HFM.Client
{
   public class Queue : Message, IList<QueueUnit>
   {
      private readonly List<QueueUnit> _units;

      private Queue()
      {
         _units = new List<QueueUnit>();
      }

      /// <summary>
      /// Create a Queue object from the given Message.
      /// </summary>
      /// <param name="message">Message object containing JSON value and meta-data.</param>
      public static Queue Parse(Message message)
      {
         var jsonArray = JArray.Parse(message.Value);
         var queue = new Queue();
         foreach (var token in jsonArray)
         {
            if (!token.HasValues)
            {
               continue;
            }

            var queueUnit = new QueueUnit();
            foreach (var prop in JObject.Parse(token.ToString()).Properties())
            {
               FahClient.SetObjectProperty(queueUnit, TypeDescriptor.GetProperties(queueUnit), prop);
            }
            queue.Add(queueUnit);
         }
         queue.SetMessageValues(message);
         return queue;
      }

      #region IList<QueueUnit> Members

      /// <summary>
      /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
      /// </summary>
      /// <returns>
      /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
      /// </returns>
      /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
      public int IndexOf(QueueUnit item)
      {
         return _units.IndexOf(item);
      }

      /// <summary>
      /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the specified index.
      /// </summary>
      /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param><param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
      public void Insert(int index, QueueUnit item)
      {
         _units.Insert(index, item);
      }

      /// <summary>
      /// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
      /// </summary>
      /// <param name="index">The zero-based index of the item to remove.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
      public void RemoveAt(int index)
      {
         _units.RemoveAt(index);
      }

      /// <summary>
      /// Gets or sets the element at the specified index.
      /// </summary>
      /// <returns>
      /// The element at the specified index.
      /// </returns>
      /// <param name="index">The zero-based index of the element to get or set.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
      public QueueUnit this[int index]
      {
         get { return _units[index]; }
         set { _units[index] = value; }
      }

      #endregion

      #region ICollection<QueueUnit> Members

      /// <summary>
      /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
      /// </summary>
      /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
      public void Add(QueueUnit item)
      {
         _units.Add(item);
      }

      /// <summary>
      /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
      /// </summary>
      /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
      public void Clear()
      {
         _units.Clear();
      }

      /// <summary>
      /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
      /// </summary>
      /// <returns>
      /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
      /// </returns>
      /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
      public bool Contains(QueueUnit item)
      {
         return _units.Contains(item);
      }

      /// <summary>
      /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
      /// </summary>
      /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.-or-Type <paramref name="T"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
      public void CopyTo(QueueUnit[] array, int arrayIndex)
      {
         _units.CopyTo(array, arrayIndex);
      }

      /// <summary>
      /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
      /// </summary>
      /// <returns>
      /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
      /// </returns>
      public int Count
      {
         get { return _units.Count; }
      }

      /// <summary>
      /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
      /// </summary>
      /// <returns>
      /// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
      /// </returns>
      bool ICollection<QueueUnit>.IsReadOnly
      {
         get { return ((ICollection<QueueUnit>)_units).IsReadOnly; }
      }

      /// <summary>
      /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
      /// </summary>
      /// <returns>
      /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
      /// </returns>
      /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
      public bool Remove(QueueUnit item)
      {
         return _units.Remove(item);
      }

      #endregion

      #region IEnumerable<QueueUnit> Members

      /// <summary>
      /// Returns an enumerator that iterates through the collection.
      /// </summary>
      /// <returns>
      /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
      /// </returns>
      /// <filterpriority>1</filterpriority>
      public IEnumerator<QueueUnit> GetEnumerator()
      {
         return _units.GetEnumerator();
      }

      #endregion

      #region IEnumerable Members

      /// <summary>
      /// Returns an enumerator that iterates through a collection.
      /// </summary>
      /// <returns>
      /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
      /// </returns>
      /// <filterpriority>2</filterpriority>
      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      #endregion
   }

   public class QueueUnit
   {
      internal QueueUnit()
      {
         
      }

      #region Properties

      [MessageProperty("id")]
      public int ID { get; set; }

      // SHOULD be enum type (looks like same value in Slot.Status)
      [MessageProperty("state")]
      public string State { get; set; }

      [MessageProperty("project")]
      public int Project { get; set; }

      [MessageProperty("run")]
      public int Run { get; set; }

      [MessageProperty("clone")]
      public int Clone { get; set; }

      [MessageProperty("gen")]
      public int Gen { get; set; }

      [MessageProperty("core")]
      public string Core { get; set; }

      [MessageProperty("unit")]
      public string Unit { get; set; }

      [MessageProperty("percentdone")]
      public string PercentDone { get; set; }

      [MessageProperty("totalframes")]
      public int TotalFrames { get; set; }

      [MessageProperty("framesdone")]
      public int FramesDone { get; set; }

      // SHOULD be DateTime type (wait for v7.1.25 - has ISO formatted values)
      [MessageProperty("assigned")]
      public string Assigned { get; set; }

      // SHOULD be DateTime type (wait for v7.1.25 - has ISO formatted values)
      [MessageProperty("timeout")]
      public string Timeout { get; set; }

      // SHOULD be DateTime type (wait for v7.1.25 - has ISO formatted values)
      [MessageProperty("deadline")]
      public string Deadline { get; set; }

      // could be IP Address type
      [MessageProperty("ws")]
      public string WorkServer { get; set; }

      // could be IP Address type
      [MessageProperty("cs")]
      public string CollectionServer { get; set; }

      [MessageProperty("waitingon")]
      public string WaitingOn { get; set; }

      [MessageProperty("attempts")]
      public int Attempts { get; set; }

      [MessageProperty("nextattempt")]
      public string NextAttempt { get; set; }

      [MessageProperty("slot")]
      public int Slot { get; set; }

      // SHOULD be TimeSpan type
      [MessageProperty("eta")]
      public string ETA { get; set; }

      [MessageProperty("ppd")]
      public double PPD { get; set; }

      // SHOULD be TimeSpan type
      [MessageProperty("tpf")]
      public string TPF { get; set; }

      [MessageProperty("basecredit")]
      public double BaseCredit { get; set; }

      [MessageProperty("creditestimate")]
      public double CreditEstimate { get; set; }

      #endregion
   }
}
