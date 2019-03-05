using System;
using System.Collections.Generic;
using System.Text;

namespace PlataformaPDCOnline.Internals.excepciones
{
    class MyODBCException : Exception
    {
        public MyODBCException()
        {
        }

        public MyODBCException(string message)
            : base(message)
        {
        }
    }
}
