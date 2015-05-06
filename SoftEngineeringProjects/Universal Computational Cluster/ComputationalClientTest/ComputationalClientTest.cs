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
            computationalClient.Info = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.ComputationalNode);
        }

        [TestMethod]
        public void ParametersTestExpectingNoException()
        {
            var computationalClient = new ComputationalClient();
            var parametersLine = "-port 8080 -address 127.0.0.1";
            computationalClient.Info = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.ComputationalClient);
            Assert.AreEqual(computationalClient.Info.CommunicationServerPort, 8080);
            Assert.AreEqual(computationalClient.Info.CommunicationServerAddress,
                new Uri("http://127.0.0.1"));
        }

        [TestMethod]
        [ExpectedException(typeof (ConnectionException))]
        public void StartTestExpectintConnectionException()
        {
            var computationalClient = new ComputationalClient();
            computationalClient.IsWorking = true;
            computationalClient.Info = new CommunicationInfo();
            computationalClient.Info.CommunicationServerAddress = new Uri("http://127.0.0.2");
            computationalClient.Info.CommunicationServerPort = 8080;
            computationalClient.Start();
        }
    }
}