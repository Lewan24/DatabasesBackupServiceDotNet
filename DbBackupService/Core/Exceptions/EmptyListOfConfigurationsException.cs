using System;

namespace Core.Exceptions;

public class EmptyListOfConfigurationsException : Exception
{
    public EmptyListOfConfigurationsException(string message) : base(message){}
}