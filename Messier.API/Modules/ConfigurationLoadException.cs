using System;
using System.Runtime.Serialization;

namespace Messier.Base.Modules
{
    [Serializable]
    public class ConfigurationLoadException : Exception
    {
        public ConfigurationLoadException()
        {
        }

        public ConfigurationLoadException(string message) : base(message)
        {
        }

        public ConfigurationLoadException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConfigurationLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}