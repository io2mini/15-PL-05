using Common;
using Common.Components;
using Common.Configuration;
using Common.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommunicationServerTest
{
    [TestClass]
    public class CommunicationServerTests
    {
        [TestMethod]
        [ExpectedException(typeof (ParsingArgumentException))]
        public void WrongParametersTestExpectingParsingException()
        {
            var communicationServer = new CommunicationServer();
            var parametersLine = "-port 8080 -ala 122";
            communicationServer.CommunicationServerInfo = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.CommunicationServer)[0];
        }

        [TestMethod]
        public void ParametersTestExpectingNoException()
        {
            var communicationServer = new CommunicationServer();
            var parametersLine = "-port 8080 -t 2";
            communicationServer.CommunicationServerInfo = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.CommunicationServer)[0];
            Assert.AreEqual(communicationServer.CommunicationServerInfo.CommunicationServerPort, 8080);
            Assert.AreEqual(communicationServer.CommunicationServerInfo.Time, (ulong) 2);
        }

        [TestMethod]
        public void InicializeTestExpectiongNoException()
        {
            var communicationServer = new CommunicationServer();
            var parametersLine = "-port 8080 -t 2";
            communicationServer.CommunicationServerInfo = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.CommunicationServer)[0];
            communicationServer.IsWorking = true;
            communicationServer.InitializeIpList();
            communicationServer.Start();
        }
    }
}