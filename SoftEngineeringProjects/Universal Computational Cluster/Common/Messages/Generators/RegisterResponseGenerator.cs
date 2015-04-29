namespace Common.Messages.Generators
{
    public static class RegisterResponseGenerator
    {
        public static RegisterResponse Generate(ulong componentId, uint timeout)
        {
            var msg = new RegisterResponse
            {
                Id = componentId,
                Timeout = timeout
            };

            return msg;
        }
    }
}