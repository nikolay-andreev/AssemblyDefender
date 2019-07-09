using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Common.Xml
{
    public static class XmlUtils
    {
        #region Read

        public static void CheckAttributesAreEmpty(this XmlReader reader)
        {
            if (reader.HasAttributes)
            {
                throw new XmlException(string.Format(SR.XmlAttributeNotValid, reader.Name));
            }
        }

        public static void CheckElementsAreEmpty(this XmlReader reader)
        {
            if (!reader.IsEmptyElement)
            {
                while (reader.Read() && reader.IsStartElement())
                {
                    throw new XmlException(string.Format(SR.XmlElementNotValid, reader.Name));
                }
            }
        }

        public static void CheckAttributesAndElementsAreEmpty(this XmlReader reader)
        {
            CheckAttributesAreEmpty(reader);
            CheckElementsAreEmpty(reader);
        }

        #region Boolean

        public static bool GetBoolValue(this XmlReader reader)
        {
            return ToBool(reader.Value, reader);
        }

        public static bool ReadBool(this XmlReader reader)
        {
            return ToBool(reader.ReadString(), reader);
        }

        private static bool ToBool(string s, XmlReader reader)
        {
            try
            {
                return bool.Parse(s);
            }
            catch (FormatException ex)
            {
                throw new XmlException(string.Format(SR.XmlNodeValueNotValid, s, reader.Name), ex);
            }
        }

        #endregion

        #region Int16

        public static short GetInt16Value(this XmlReader reader)
        {
            return ToInt16(reader.Value, reader);
        }

        public static short ReadInt16(this XmlReader reader)
        {
            return ToInt16(reader.ReadString(), reader);
        }

        private static short ToInt16(string s, XmlReader reader)
        {
            try
            {
                return short.Parse(s);
            }
            catch (FormatException ex)
            {
                throw new XmlException(string.Format(SR.XmlNodeValueNotValid, s, reader.Name), ex);
            }
        }

        #endregion

        #region Int32

        public static int GetInt32Value(this XmlReader reader)
        {
            return ToInt32(reader.Value, reader);
        }

        public static int ReadInt32(this XmlReader reader)
        {
            return ToInt32(reader.ReadString(), reader);
        }

        private static int ToInt32(string s, XmlReader reader)
        {
            try
            {
                return int.Parse(s);
            }
            catch (FormatException ex)
            {
                throw new XmlException(string.Format(SR.XmlNodeValueNotValid, s, reader.Name), ex);
            }
        }

        #endregion

        #region Int64

        public static long GetInt64Value(this XmlReader reader)
        {
            return ToInt64(reader.Value, reader);
        }

        public static long ReadInt64(this XmlReader reader)
        {
            return ToInt64(reader.ReadString(), reader);
        }

        private static long ToInt64(string s, XmlReader reader)
        {
            try
            {
                return long.Parse(s);
            }
            catch (FormatException ex)
            {
                throw new XmlException(string.Format(SR.XmlNodeValueNotValid, s, reader.Name), ex);
            }
        }

        #endregion

        #region Guid

        public static Guid GetGuidValue(this XmlReader reader)
        {
            return ToGuid(reader.Value, reader);
        }

        public static Guid ReadGuid(this XmlReader reader)
        {
            return ToGuid(reader.ReadString(), reader);
        }

        private static Guid ToGuid(string s, XmlReader reader)
        {
            try
            {
                return new Guid(s);
            }
            catch (FormatException ex)
            {
                throw new XmlException(string.Format(SR.XmlNodeValueNotValid, s, reader.Name), ex);
            }
        }

        #endregion

        #region Version

        public static Version GetVersionValue(this XmlReader reader)
        {
            return ToVersion(reader.Value, reader);
        }

        public static Version ReadVersion(this XmlReader reader)
        {
            return ToVersion(reader.ReadString(), reader);
        }

        private static Version ToVersion(string s, XmlReader reader)
        {
            try
            {
                return new Version(s);
            }
            catch (FormatException ex)
            {
                throw new XmlException(string.Format(SR.XmlNodeValueNotValid, s, reader.Name), ex);
            }
        }

        #endregion

        #region Byte array

        public static byte[] GetByteArrayValue(this XmlReader reader)
        {
            return ToByteArray(reader.Value, reader);
        }

        public static byte[] ReadByteArray(this XmlReader reader)
        {
            return ToByteArray(reader.ReadString(), reader);
        }

        private static byte[] ToByteArray(string s, XmlReader reader)
        {
            try
            {
                if (string.IsNullOrEmpty(s))
                    return BufferUtils.EmptyArray;

                return Convert.FromBase64String(s);
            }
            catch (FormatException ex)
            {
                throw new XmlException(string.Format(SR.XmlNodeValueNotValid, s, reader.Name), ex);
            }
        }

        #endregion

        #endregion

        #region Write

        public static void WriteEmptyElement(this XmlWriter writer, string name)
        {
            writer.WriteStartElement(name);
            writer.WriteEndElement();
        }

        #region Boolean

        public static void WriteAttribute(this XmlWriter writer, string name, bool value)
        {
            writer.WriteStartAttribute(name);
            writer.WriteValue(value);
            writer.WriteEndAttribute();
        }

        public static void WriteElement(this XmlWriter writer, string name, bool value)
        {
            writer.WriteStartElement(name);
            writer.WriteValue(value);
            writer.WriteEndElement();
        }

        #endregion

        #region Int16

        public static void WriteAttribute(this XmlWriter writer, string name, short value)
        {
            writer.WriteStartAttribute(name);
            writer.WriteValue(value);
            writer.WriteEndAttribute();
        }

        public static void WriteElement(this XmlWriter writer, string name, short value)
        {
            writer.WriteStartElement(name);
            writer.WriteValue(value);
            writer.WriteEndElement();
        }

        #endregion

        #region Int32

        public static void WriteAttribute(this XmlWriter writer, string name, int value)
        {
            writer.WriteStartAttribute(name);
            writer.WriteValue(value);
            writer.WriteEndAttribute();
        }

        public static void WriteElement(this XmlWriter writer, string name, int value)
        {
            writer.WriteStartElement(name);
            writer.WriteValue(value);
            writer.WriteEndElement();
        }

        #endregion

        #region Int64

        public static void WriteAttribute(this XmlWriter writer, string name, long value)
        {
            writer.WriteStartAttribute(name);
            writer.WriteValue(value);
            writer.WriteEndAttribute();
        }

        public static void WriteElement(this XmlWriter writer, string name, long value)
        {
            writer.WriteStartElement(name);
            writer.WriteValue(value);
            writer.WriteEndElement();
        }

        #endregion

        #region String

        public static void WriteAttribute(this XmlWriter writer, string name, string value)
        {
            writer.WriteStartAttribute(name);
            writer.WriteValue(value);
            writer.WriteEndAttribute();
        }

        public static void WriteElement(this XmlWriter writer, string name, string value)
        {
            writer.WriteStartElement(name);
            writer.WriteValue(value);
            writer.WriteEndElement();
        }

        #endregion

        #region Guid

        public static void WriteAttribute(this XmlWriter writer, string name, Guid value)
        {
            writer.WriteStartAttribute(name);
            writer.WriteValue(ToString(value));
            writer.WriteEndAttribute();
        }

        public static void WriteElement(this XmlWriter writer, string name, Guid value)
        {
            writer.WriteStartElement(name);
            writer.WriteValue(ToString(value));
            writer.WriteEndElement();
        }

        private static string ToString(Guid value)
        {
            return value.ToString("B").ToUpper();
        }

        #endregion

        #region Version

        public static void WriteAttribute(this XmlWriter writer, string name, Version value)
        {
            writer.WriteStartAttribute(name);
            writer.WriteValue(ToString(value));
            writer.WriteEndAttribute();
        }

        public static void WriteElement(this XmlWriter writer, string name, Version value)
        {
            writer.WriteStartElement(name);
            writer.WriteValue(ToString(value));
            writer.WriteEndElement();
        }

        private static string ToString(Version value)
        {
            return value.ToString();
        }

        #endregion

        #region Byte array

        public static void WriteAttribute(this XmlWriter writer, string name, byte[] value)
        {
            writer.WriteStartAttribute(name);
            writer.WriteValue(ToString(value));
            writer.WriteEndAttribute();
        }

        public static void WriteElement(this XmlWriter writer, string name, byte[] value)
        {
            writer.WriteStartElement(name);
            writer.WriteValue(ToString(value));
            writer.WriteEndElement();
        }

        private static string ToString(byte[] value)
        {
            if (value == null || value.Length == 0)
                return string.Empty;

            return Convert.ToBase64String(value);
        }

        #endregion

        #endregion
    }
}
