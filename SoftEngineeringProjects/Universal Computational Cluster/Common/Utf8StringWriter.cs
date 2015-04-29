using System.IO;
using System.Text;

namespace Common
{
    public class UTF8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}