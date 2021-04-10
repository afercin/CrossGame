using System;
using System.Runtime.Serialization;

namespace Cross_Game
{
    [Serializable]
    internal class InternetConnectionException : Exception
    {
        public InternetConnectionException()
        {
        }

        public InternetConnectionException(string message) : base(message)
        {
        }
    }
}