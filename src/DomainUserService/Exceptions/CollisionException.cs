namespace DomainUserService.Exceptions;

public class CollisionException : Exception
{
    public CollisionException() : base("Username or id already exist")
    {
    }
}