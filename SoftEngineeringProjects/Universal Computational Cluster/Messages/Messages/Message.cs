﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace Common
{
    [System.SerializableAttribute()]
    public abstract class Message
    {
        public String GetMessage()
        {
            return null;
        }
    }
}
