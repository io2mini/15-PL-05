using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Exceptions;
using Common.Components;
using Common.Communication;
using Common;

namespace CommunicationServerTest
{
    [TestClass]
    public class CommunicationServerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ParsingArgumentException))]
        public void WrongParametersTestExpectingParsingException()
        {
            CommunicationServer communicationServer = new CommunicationServer();
            String parametersLine = "-port 8080 -ala 122";
            communicationServer.CommunicationInfo = ParametersParser.ReadParameters(parametersLine, SystemComponentType.CommunicationServer);

        }

        [TestMethod]
        public void ParametersTestExpectingNoException()
        {
            CommunicationServer communicationServer = new CommunicationServer();
            String parametersLine = "-port 8080 -t 2";
            communicationServer.CommunicationInfo = ParametersParser.ReadParameters(parametersLine, SystemComponentType.CommunicationServer);
            Assert.AreEqual(communicationServer.CommunicationInfo.CommunicationServerPort, 8080);
            Assert.AreEqual(communicationServer.CommunicationInfo.Time, (ulong)2);
        }
    }
}
