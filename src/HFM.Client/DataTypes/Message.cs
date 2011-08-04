﻿/*
 * HFM.NET - Message Data Class
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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

using Newtonsoft.Json.Linq;

namespace HFM.Client.DataTypes
{
   public abstract class TypedMessage : Message
   {
      
   }

   public class JsonMessage : Message
   {
      internal JsonMessage()
      {
         
      }

      /// <summary>
      /// Message Value
      /// </summary>
      public string Value { get; set; }

      /// <summary>
      /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
      /// </summary>
      /// <returns>
      /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
      /// </returns>
      /// <filterpriority>2</filterpriority>
      public override string ToString()
      {
         var sb = new StringBuilder();
         sb.Append(base.ToString());
         sb.AppendLine(String.Format(CultureInfo.CurrentCulture, " - Length: {0}", Value.Length));
         sb.AppendLine(Value);
         sb.AppendLine();
         return sb.ToString();
      }
   }

   public abstract class Message
   {
      /// <summary>
      /// Message Key
      /// </summary>
      public string Key { get; set; }

      /// <summary>
      /// Received Time Stamp
      /// </summary>
      public DateTime Received { get; set; }

      internal void SetMessageValues(Message message)
      {
         Key = message.Key;
         Received = message.Received;
      }

      /// <summary>
      /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
      /// </summary>
      /// <returns>
      /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
      /// </returns>
      /// <filterpriority>2</filterpriority>
      public override string ToString()
      {
         return String.Format(CultureInfo.CurrentCulture, "Message Key: {0} - Received at: {1}", Key, Received);
      }
   }

   [AttributeUsage(AttributeTargets.Property)]
   public sealed class MessagePropertyAttribute : Attribute
   {
      private readonly string _name;

      public string Name
      {
         get { return _name; }
      }

      private readonly Type _converterType;

      public Type ConverterType
      {
         get { return _converterType; }
      }

      public MessagePropertyAttribute(string name)
      {
         _name = name;
      }

      public MessagePropertyAttribute(string name, Type converterType)
      {
         _name = name;
         _converterType = converterType;
      }
   }

   internal class MessagePropertySetter
   {
      private readonly object _message;
      private readonly PropertyDescriptorCollection _properties;

      internal MessagePropertySetter(object message)
      {
         _message = message;
         _properties = TypeDescriptor.GetProperties(message);
      }

      internal void SetProperty(JProperty jProperty)
      {
         if (jProperty == null) return;

         var properties = GetProperties(jProperty.Name);
         if (properties.Count() == 0)
         {
            return;
         }

         if (jProperty.Value.Type.Equals(JTokenType.String))
         {
            foreach (var classProperty in properties)
            {
               SetPropertyValue(classProperty, (string)jProperty);
            }
         }
         else if (jProperty.Value.Type.Equals(JTokenType.Integer))
         {
            foreach (var classProperty in properties)
            {
               classProperty.SetValue(_message, (int)jProperty);
            }
         }
         else if (jProperty.Value.Type.Equals(JTokenType.Object))
         {
            var propertyValue = GetPropertyValue(jProperty.Name);
            if (propertyValue != null)
            {
               var propertySetter = new MessagePropertySetter(propertyValue);
               foreach (var prop in JObject.Parse(jProperty.Value.ToString()).Properties())
               {
                  propertySetter.SetProperty(prop);
               }
            }
         }
      }

      internal void SetProperty(string key, string value)
      {
         if (key == null) return;

         var properties = GetProperties(key);
         if (properties.Count() == 0)
         {
            return;
         }

         foreach (var classProperty in properties)
         {
            SetPropertyValue(classProperty, value);
         }
      }

      private void SetPropertyValue(PropertyDescriptor classProperty, string value)
      {
         try
         {
            IConversionProvider conversionProvider = GetConversionProvider(classProperty);
            if (conversionProvider != null)
            {
               classProperty.SetValue(_message, conversionProvider.Convert(value));
            }
            else
            {
               classProperty.SetValue(_message, Convert.ChangeType(value, classProperty.PropertyType, CultureInfo.InvariantCulture));
            }
         }
         catch (FormatException)
         {
            // TODO: Log or make a list of these conversion errors
         }
      }

      internal object GetPropertyValue(string key)
      {
         var classProperty = GetProperties(key).FirstOrDefault();
         return classProperty == null ? null : classProperty.GetValue(_message);
      }

      private IEnumerable<PropertyDescriptor> GetProperties(string key)
      {
         return (from PropertyDescriptor classProperty in _properties
                 let messageProperty = (MessagePropertyAttribute)classProperty.Attributes[typeof(MessagePropertyAttribute)]
                 where messageProperty != null
                 where messageProperty.Name == key
                 select classProperty);
      }

      private static IConversionProvider GetConversionProvider(MemberDescriptor classProperty)
      {
         Type converterType = ((MessagePropertyAttribute)classProperty.Attributes[typeof(MessagePropertyAttribute)]).ConverterType;
         if (converterType != null && converterType.GetInterface("IConversionProvider") != null)
         {
            return (IConversionProvider)Activator.CreateInstance(converterType);
         }
         return null;
      }
   }

   public interface IConversionProvider
   {
      object Convert(string input);
   }
}
