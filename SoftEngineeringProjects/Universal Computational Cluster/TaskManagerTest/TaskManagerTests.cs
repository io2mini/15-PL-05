using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Components;
using Common.Communication;
using Common;
using Common.Exceptions;

namespace TaskManagerTest
{
    [TestClass]
    public class TaskManagerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ParsingArgumentException))]
        public void WrongParametersTestExpectingParsingException()
        {
          TaskManager taskManager = new TaskManager();
          String parametersLine = "-port 8080 -ala 122";
          taskManager.CommunicationInfo = ParametersParser.ReadParameters(parametersLine, SystemComponentType.TaskManager);

        }

        [TestMethod]
        public void ParametersTestExpectingNoException()
        {
           TaskManager taskManager = new TaskManager();
           String parametersLine = "-port 8080 -address 127.0.0.1";
           taskManager.CommunicationInfo = ParametersParser.ReadParameters(parametersLine, SystemComponentType.TaskManager);
           Assert.AreEqual(taskManager.CommunicationInfo.CommunicationServerPort, 8080);
           Assert.AreEqual(taskManager.CommunicationInfo.CommunicationServerAddress, new Uri("http://127.0.0.1"));
        }

        [TestMethod]
        [ExpectedException(typeof(ConnectionException))]
        public void StartTestExpectintConnectionException()
        {
            TaskManager taskManager = new TaskManager();
            taskManager.IsWorking = true;
            taskManager.CommunicationInfo = new CommunicationInfo();
            taskManager.CommunicationInfo.CommunicationServerAddress = new Uri("http://127.0.0.2");
            taskManager.CommunicationInfo.CommunicationServerPort = 8080;
            taskManager.Start();
        }
    }
}
