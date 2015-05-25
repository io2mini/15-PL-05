using System;
using Common;
using Common.Components;
using Common.Configuration;
using Common.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComputationalNodeTest
{
    [TestClass]
    public class ComputationalNodeTests
    {
        [TestMethod]
        [ExpectedException(typeof (ParsingArgumentException))]
        public void WrongParametersTestExpectingParsingException()
        {
            var computationalNode = new ComputationalNode();
            var parametersLine = "-port 8080 -ala 122";
            computationalNode.CommunicationServerInfo = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.ComputationalNode)[0];
        }

        [TestMethod]
        public void ParametersTestExpectingNoException()
        {
            var computationalNode = new ComputationalNode();
            var parametersLine = "-port 8080 -address 127.0.0.1";
            computationalNode.CommunicationServerInfo = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.ComputationalNode)[0];
            Assert.AreEqual(computationalNode.CommunicationServerInfo.CommunicationServerPort, 8080);
            Assert.AreEqual(computationalNode.CommunicationServerInfo.CommunicationServerAddress, new Uri("http://127.0.0.1"));
        }

        [TestMethod]
        [ExpectedException(typeof (ConnectionException))]
        public void StartTestExpectintConnectionException()
        {
            var computationalNode = new ComputationalNode();
            computationalNode.IsWorking = true;
            computationalNode.CommunicationServerInfo = new CommunicationInfo();
            computationalNode.CommunicationServerInfo.CommunicationServerAddress = new Uri("http://127.0.0.2");
            computationalNode.CommunicationServerInfo.CommunicationServerPort = 8080;
            computationalNode.Start();
        }
    }
}