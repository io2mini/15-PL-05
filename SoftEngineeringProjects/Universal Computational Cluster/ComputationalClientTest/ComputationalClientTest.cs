using System;
using Common;
using Common.Components;
using Common.Configuration;
using Common.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComputationalClientTest
{
    [TestClass]
    public class ComputationalClientTest
    {
        [TestMethod]
        [ExpectedException(typeof (ParsingArgumentException))]
        public void WrongParametersTestExpectingParsingException()
        {
            var computationalClient = new ComputationalClient();
            var parametersLine = "-port 8080 -ala 122";
            computationalClient.CommunicationServerInfo = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.ComputationalNode)[0];
        }

        [TestMethod]
        public void ParametersTestExpectingNoException()
        {
            var computationalClient = new ComputationalClient();
            var parametersLine = "-port 8080 -address 127.0.0.1";
            computationalClient.CommunicationServerInfo = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.ComputationalClient)[0];
            Assert.AreEqual(computationalClient.CommunicationServerInfo.CommunicationServerPort, 8080);
            Assert.AreEqual(computationalClient.CommunicationServerInfo.CommunicationServerAddress,
                new Uri("http://127.0.0.1"));
        }

        [TestMethod]
        [ExpectedException(typeof (ConnectionException))]
        public void StartTestExpectintConnectionException()
        {
            var computationalClient = new ComputationalClient();
            computationalClient.IsWorking = true;
            computationalClient.CommunicationServerInfo = new CommunicationInfo();
            computationalClient.CommunicationServerInfo.CommunicationServerAddress = new Uri("http://127.0.0.2");
            computationalClient.CommunicationServerInfo.CommunicationServerPort = 8080;
            computationalClient.Start();
        }
    }
}