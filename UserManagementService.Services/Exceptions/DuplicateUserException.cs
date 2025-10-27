namespace UserManagementService.Services.Exceptions
{
    public class DuplicateUserException : Exception
    {
        public DuplicateUserException() : base()
        {
        }

        public DuplicateUserException(string message) : base(message)
        {
        }
    }
}