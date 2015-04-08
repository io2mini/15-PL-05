using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Components;
using Common.Communication;
using Common;
using Common.Exceptions;

namespace CommonTest
{
    [TestClass]
    public class CommunicationTests
    {
        [TestMethod]
        public void RegisterTaskManagerNoException()
        {
            CommunicationServer communicationServer = new CommunicationServer();
            String parametersLine = "-port 8080 -t 2";
            communicationServer.CommunicationInfo = ParametersParser.ReadParameters(parametersLine, SystemComponentType.CommunicationServer);
            communicationServer.IsWorking = true;
            communicationServer.InitializeIPList();
            communicationServer.Start();
            TaskManager taskManager = new TaskManager();
            taskManager.IsWorking = true;
            taskManager.CommunicationInfo = new CommunicationInfo();
            taskManager.CommunicationInfo.CommunicationServerAddress = new Uri("http://127.0.0.1");
            taskManager.CommunicationInfo.CommunicationServerPort = 8080;
            taskManager.Start();
        }

        [TestMethod]
        public void RegisterComputationalNodeNoException()
        {
            CommunicationServer communicationServer = new CommunicationServer();
            String parametersLine = "-port 8080 -t 2";
            communicationServer.CommunicationInfo = ParametersParser.ReadParameters(parametersLine, SystemComponentType.CommunicationServer);
            communicationServer.IsWorking = true;
            communicationServer.InitializeIPList();
            communicationServer.Start();
            ComputationalNode computationalNode = new ComputationalNode();
            computationalNode.IsWorking = true;
            computationalNode.CommunicationInfo = new CommunicationInfo();
            computationalNode.CommunicationInfo.CommunicationServerAddress = new Uri("http://127.0.0.1");
            computationalNode.CommunicationInfo.CommunicationServerPort = 8080;
            computationalNode.Start();
        }

        [TestMethod]
        public void RegisterManyComponentsNoException()
        {
            CommunicationServer communicationServer = new CommunicationServer();
            String parametersLine = "-port 8080 -t 2";
            communicationServer.CommunicationInfo = ParametersParser.ReadParameters(parametersLine, SystemComponentType.CommunicationServer);
            communicationServer.IsWorking = true;
            communicationServer.InitializeIPList();
            communicationServer.Start();
            ComputationalNode[] computationalNodes = new ComputationalNode[5];
            for (int i = 0; i < computationalNodes.Length; i++ )
            {
                computationalNodes[i] = new ComputationalNode();
                computationalNodes[i].IsWorking = true;
                computationalNodes[i].CommunicationInfo = new CommunicationInfo();
                computationalNodes[i].CommunicationInfo.CommunicationServerAddress = new Uri("http://127.0.0.1");
                computationalNodes[i].CommunicationInfo.CommunicationServerPort = 8080;
                computationalNodes[i].Start();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ConnectionException))]
        public void RegisterComponentExeptedConnectionException()
        {
            CommunicationServer communicationServer = new CommunicationServer();
            String parametersLine = "-port 8010 -t 2";
            communicationServer.CommunicationInfo = ParametersParser.ReadParameters(parametersLine, SystemComponentType.CommunicationServer);
            communicationServer.IsWorking = true;
            communicationServer.Start();
            ComputationalNode computationalNode = new ComputationalNode();
            computationalNode.IsWorking = true;
            computationalNode.CommunicationInfo = new CommunicationInfo();
            computationalNode.CommunicationInfo.CommunicationServerAddress = new Uri("http://127.0.0.1");
            computationalNode.CommunicationInfo.CommunicationServerPort = 8010;
            computationalNode.Start();
        }

        [TestMethod]
        public void DisconnectCommunicationServerExeptedNoException()
        {
            CommunicationServer communicationServer = new CommunicationServer();
            String parametersLine = "-port 8080 -t 2";
            communicationServer.CommunicationInfo = ParametersParser.ReadParameters(parametersLine, SystemComponentType.CommunicationServer);
            communicationServer.IsWorking = true;
            communicationServer.InitializeIPList();
            communicationServer.Start();
            TaskManager taskManager = new TaskManager();
            taskManager.IsWorking = true;
            taskManager.CommunicationInfo = new CommunicationInfo();
            taskManager.CommunicationInfo.CommunicationServerAddress = new Uri("http://127.0.0.1");
            taskManager.CommunicationInfo.CommunicationServerPort = 8080;
            taskManager.Start();
            communicationServer = null;
        }

        [TestMethod]
        public void DisconnectComponentExeptedNoException()
        {
            CommunicationServer communicationServer = new CommunicationServer();
            String parametersLine = "-port 8080 -t 2";
            communicationServer.CommunicationInfo = ParametersParser.ReadParameters(parametersLine, SystemComponentType.CommunicationServer);
            communicationServer.IsWorking = true;
            communicationServer.InitializeIPList();
            communicationServer.Start();
            TaskManager taskManager = new TaskManager();
            taskManager.IsWorking = true;
            taskManager.CommunicationInfo = new CommunicationInfo();
            taskManager.CommunicationInfo.CommunicationServerAddress = new Uri("http://127.0.0.1");
            taskManager.CommunicationInfo.CommunicationServerPort = 8080;
            taskManager.Start();
            ComputationalNode computationalNode = new ComputationalNode();
            computationalNode.IsWorking = true;
            computationalNode.CommunicationInfo = new CommunicationInfo();
            computationalNode.CommunicationInfo.CommunicationServerAddress = new Uri("http://127.0.0.1");
            computationalNode.CommunicationInfo.CommunicationServerPort = 8080;
            computationalNode.Start();
            taskManager = null;
        }

    }
}
