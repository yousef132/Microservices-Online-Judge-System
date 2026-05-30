namespace Community.API.Common.Exceptions;

public class ForbiddenAccessException(string message = "User is not authorized to perform this action.")
    : Exception(message)
{ }
