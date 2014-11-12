﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace TheBoxSoftware.API.LiveDocumenter
{
    /// <summary>
    /// An exception that is thrown by the API when there is a licensing issue.
    /// </summary>
    public sealed class LicenceException : Exception
    {
        internal LicenceException() : base() { }
        internal LicenceException(string message) : base(message) { }
        internal LicenceException(string message, Exception innerException) : base(message, innerException) { }
        internal LicenceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
