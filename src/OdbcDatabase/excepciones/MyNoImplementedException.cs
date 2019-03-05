using System;

namespace OdbcDatabase.excepciones
{
    public class MyNoImplementedException : Exception
    {
        public MyNoImplementedException()
        {
        }

        public MyNoImplementedException(string message)
            : base(message)
        {
        }
    }
}
