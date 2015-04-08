﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Exceptions;
using Common.Components;
using Common.Communication;
using Common;

namespace ComputationalNodeTest
{
    [TestClass]
    public class ComputationalNodeTests
    {
        [TestMethod]
        [ExpectedException(typeof(ParsingArgumentException))]
        public void WrongParametersTestExpectingParsingException()
        {
            ComputationalNode computationalNode = new ComputationalNode();
            String parametersLine = "-port 8080 -ala 122";
            computationalNode.CommunicationInfo = ParametersParser.ReadParameters(parametersLine, SystemComponentType.ComputationalNode);

        }

        [TestMethod]
        public void ParametersTestExpectingNoException()
        {
            ComputationalNode computationalNode = new ComputationalNode();
            String parametersLine = "-port 8080 -address 127.0.0.1";
            computationalNode.CommunicationInfo = ParametersParser.ReadParameters(parametersLine, SystemComponentType.ComputationalNode);
            Assert.AreEqual(computationalNode.CommunicationInfo.CommunicationServerPort, 8080);
            Assert.AreEqual(computationalNode.CommunicationInfo.CommunicationServerAddress, new Uri("http://127.0.0.1"));
        }
    }
}