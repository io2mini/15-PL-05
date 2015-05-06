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
            computationalNode.Info = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.ComputationalNode);
        }

        [TestMethod]
        public void ParametersTestExpectingNoException()
        {
            var computationalNode = new ComputationalNode();
            var parametersLine = "-port 8080 -address 127.0.0.1";
            computationalNode.Info = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.ComputationalNode);
            Assert.AreEqual(computationalNode.Info.CommunicationServerPort, 8080);
            Assert.AreEqual(computationalNode.Info.CommunicationServerAddress, new Uri("http://127.0.0.1"));
        }

        [TestMethod]
        [ExpectedException(typeof (ConnectionException))]
        public void StartTestExpectintConnectionException()
        {
            var computationalNode = new ComputationalNode();
            computationalNode.IsWorking = true;
            computationalNode.Info = new CommunicationInfo();
            computationalNode.Info.CommunicationServerAddress = new Uri("http://127.0.0.2");
            computationalNode.Info.CommunicationServerPort = 8080;
            computationalNode.Start();
        }
    }
}