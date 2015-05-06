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
            taskManager.Info = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.TaskManager);
        }

        [TestMethod]
        public void ParametersTestExpectingNoException()
        {
            var taskManager = new TaskManager();
            var parametersLine = "-port 8080 -address 127.0.0.1";
            taskManager.Info = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.TaskManager);
            Assert.AreEqual(taskManager.Info.CommunicationServerPort, 8080);
            Assert.AreEqual(taskManager.Info.CommunicationServerAddress, new Uri("http://127.0.0.1"));
        }

        [TestMethod]
        [ExpectedException(typeof (ConnectionException))]
        public void StartTestExpectintConnectionException()
        {
            var taskManager = new TaskManager();
            taskManager.IsWorking = true;
            taskManager.Info = new CommunicationInfo();
            taskManager.Info.CommunicationServerAddress = new Uri("http://127.0.0.2");
            taskManager.Info.CommunicationServerPort = 8080;
            taskManager.Start();
        }
    }
}