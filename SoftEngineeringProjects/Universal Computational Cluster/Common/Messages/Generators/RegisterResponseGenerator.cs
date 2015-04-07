using System.CodeDom.Compiler;

namespace Common.Messages.Generators
{
    public static class RegisterResponseGenerator
    {
        public static RegisterResponse Generate(ulong componentId, uint timeout, )
        {
            RegisterResponse msg = new RegisterResponse();

            msg.Id = componentId;
            msg.Timeout = timeout;

            return msg;
        }
    }
}