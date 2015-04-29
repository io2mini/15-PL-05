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
            computationalClient.CommunicationInfo = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.ComputationalNode);
        }

        [TestMethod]
        public void ParametersTestExpectingNoException()
        {
            var computationalClient = new ComputationalClient();
            var parametersLine = "-port 8080 -address 127.0.0.1";
            computationalClient.CommunicationInfo = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.ComputationalClient);
            Assert.AreEqual(computationalClient.CommunicationInfo.CommunicationServerPort, 8080);
            Assert.AreEqual(computationalClient.CommunicationInfo.CommunicationServerAddress,
                new Uri("http://127.0.0.1"));
        }

        [TestMethod]
        [ExpectedException(typeof (ConnectionException))]
        public void StartTestExpectintConnectionException()
        {
            var computationalClient = new ComputationalClient();
            computationalClient.IsWorking = true;
            computationalClient.CommunicationInfo = new CommunicationInfo();
            computationalClient.CommunicationInfo.CommunicationServerAddress = new Uri("http://127.0.0.2");
            computationalClient.CommunicationInfo.CommunicationServerPort = 8080;
            computationalClient.Start();
        }
    }
}