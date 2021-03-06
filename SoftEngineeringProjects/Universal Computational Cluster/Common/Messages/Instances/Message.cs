﻿using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Common.Messages
{
    [Serializable()]
    public abstract class Message
    {
        /// <summary>
        /// Converts byteArray to string and removes unnecessary characters.
        /// </summary>
        /// <param name="byteArray">Message in byte form</param>
        /// <returns>Message in string form</returns>
        public static string Sanitize(byte[] byteArray)
        {
            var message = Encoding.UTF8.GetString(byteArray);
            message = message.Trim('\0');
            //message = message.Replace("\n", string.Empty);
            //message = message.Replace("\r", string.Empty);
            var rgx = new Regex(@">(\s*\n*\r*)*<");
            message = rgx.Replace(message, "><");

            string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (message.StartsWith(_byteOrderMarkUtf8))
            {
                message = message.Replace(_byteOrderMarkUtf8, "");
            }
            message = message.Replace("\0", string.Empty);
            return message;
        }

        /// <summary>
        /// metoda parsująca xml na instancję odpowiedniego message
        /// </summary>
        /// <param name="type">Typ message jaki chcemy uzyskać</param>
        /// <param name="Message">Message parsowany</param>
        /// <returns></returns>
        static public Message ParseXML(Type type,string Message)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(type);
            Message message = (Message)xmlSerializer.Deserialize(new StringReader(Message));
            return message;
        }

        public override String ToString()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(this.GetType());
            StringWriter writer = new UTF8StringWriter();
            xmlSerializer.Serialize(writer, this);
            return writer.ToString();
        }
    }
}
