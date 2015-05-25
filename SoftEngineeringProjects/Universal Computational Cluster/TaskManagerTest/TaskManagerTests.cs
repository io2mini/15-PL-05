using System;
using Common;
using Common.Components;
using Common.Configuration;
using Common.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskManagerTest
{
    [TestClass]
    public class TaskManagerTests
    {
        [TestMethod]
        [ExpectedException(typeof (ParsingArgumentException))]
        public void WrongParametersTestExpectingParsingException()
        {
            var taskManager = new TaskManager();
            var parametersLine = "-port 8080 -ala 122";
            taskManager.CommunicationServerInfo = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.TaskManager)[0];
        }

        [TestMethod]
        public void ParametersTestExpectingNoException()
        {
            var taskManager = new TaskManager();
            var parametersLine = "-port 8080 -address 127.0.0.1";
            taskManager.CommunicationServerInfo = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.TaskManager)[0];
            Assert.AreEqual(taskManager.CommunicationServerInfo.CommunicationServerPort, 8080);
            Assert.AreEqual(taskManager.CommunicationServerInfo.CommunicationServerAddress, new Uri("http://127.0.0.1"));
        }

        [TestMethod]
        [ExpectedException(typeof (ConnectionException))]
        public void StartTestExpectintConnectionException()
        {
            var taskManager = new TaskManager();
            taskManager.IsWorking = true;
            taskManager.CommunicationServerInfo = new CommunicationInfo();
            taskManager.CommunicationServerInfo.CommunicationServerAddress = new Uri("http://127.0.0.2");
            taskManager.CommunicationServerInfo.CommunicationServerPort = 8080;
            taskManager.Start();
        }
    }
}