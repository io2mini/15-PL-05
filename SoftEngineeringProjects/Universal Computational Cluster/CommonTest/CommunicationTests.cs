using System;
using Common;
using Common.Components;
using Common.Configuration;
using Common.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonTest
{
    [TestClass]
    public class CommunicationTests
    {
        [TestMethod]
        public void RegisterTaskManagerNoException()
        {
            var communicationServer = new CommunicationServer();
            var parametersLine = "-port 8080 -t 2";
            communicationServer.CommunicationServerInfo = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.CommunicationServer)[0];
            communicationServer.IsWorking = true;
            communicationServer.InitializeIpList();
            communicationServer.Start();
            var taskManager = new TaskManager
            {
                IsWorking = true,
                CommunicationServerInfo = new CommunicationInfo
                {
                    CommunicationServerAddress = new Uri("http://127.0.0.1"),
                    CommunicationServerPort = 8080
                }
            };
            taskManager.Start();
        }

        [TestMethod]
        public void RegisterComputationalNodeNoException()
        {
            var communicationServer = new CommunicationServer();
            var parametersLine = "-port 8080 -t 2";
            communicationServer.CommunicationServerInfo = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.CommunicationServer)[0];
            communicationServer.IsWorking = true;
            communicationServer.InitializeIpList();
            communicationServer.Start();
            var computationalNode = new ComputationalNode
            {
                IsWorking = true,
                CommunicationServerInfo = new CommunicationInfo
                {
                    CommunicationServerAddress = new Uri("http://127.0.0.1"),
                    CommunicationServerPort = 8080
                }
            };
            computationalNode.Start();
        }

        [TestMethod]
        public void RegisterManyComponentsNoException()
        {
            var communicationServer = new CommunicationServer();
            var parametersLine = "-port 8080 -t 2";
            communicationServer.CommunicationServerInfo = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.CommunicationServer)[0];
            communicationServer.IsWorking = true;
            communicationServer.InitializeIpList();
            communicationServer.Start();
            var computationalNodes = new ComputationalNode[5];
            for (var i = 0; i < computationalNodes.Length; i++)
            {
                computationalNodes[i] = new ComputationalNode
                {
                    IsWorking = true,
                    CommunicationServerInfo = new CommunicationInfo
                    {
                        CommunicationServerAddress = new Uri("http://127.0.0.1"),
                        CommunicationServerPort = 8080
                    }
                };
                computationalNodes[i].Start();
            }
        }

        [TestMethod]
        [ExpectedException(typeof (ConnectionException))]
        public void RegisterComponentExeptedConnectionException()
        {
            var communicationServer = new CommunicationServer();
            var parametersLine = "-port 8010 -t 2";
            communicationServer.CommunicationServerInfo = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.CommunicationServer)[0];
            communicationServer.IsWorking = true;
            communicationServer.Start();
            var computationalNode = new ComputationalNode
            {
                IsWorking = true,
                CommunicationServerInfo = new CommunicationInfo
                {
                    CommunicationServerAddress = new Uri("http://127.0.0.1"),
                    CommunicationServerPort = 8010
                }
            };
            computationalNode.Start();
        }

        [TestMethod]
        public void DisconnectCommunicationServerExeptedNoException()
        {
            var communicationServer = new CommunicationServer();
            var parametersLine = "-port 8080 -t 2";
            communicationServer.CommunicationServerInfo = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.CommunicationServer)[0];
            communicationServer.IsWorking = true;
            communicationServer.InitializeIpList();
            communicationServer.Start();
            var taskManager = new TaskManager
            {
                IsWorking = true,
                CommunicationServerInfo = new CommunicationInfo
                {
                    CommunicationServerAddress = new Uri("http://127.0.0.1"),
                    CommunicationServerPort = 8080
                }
            };
            taskManager.Start();
            communicationServer.Stop();
        }

        [TestMethod]
        public void DisconnectComponentExeptedNoException()
        {
            var communicationServer = new CommunicationServer();
            var parametersLine = "-port 8080 -t 2";
            communicationServer.CommunicationServerInfo = ParametersParser.ReadParameters(parametersLine,
                SystemComponentType.CommunicationServer)[0];
            communicationServer.IsWorking = true;
            communicationServer.InitializeIpList();
            communicationServer.Start();
            var taskManager = new TaskManager
            {
                IsWorking = true,
                CommunicationServerInfo = new CommunicationInfo
                {
                    CommunicationServerAddress = new Uri("http://127.0.0.1"),
                    CommunicationServerPort = 8080
                }
            };
            taskManager.Start();
            var computationalNode = new ComputationalNode
            {
                IsWorking = true,
                CommunicationServerInfo = new CommunicationInfo
                {
                    CommunicationServerAddress = new Uri("http://127.0.0.1"),
                    CommunicationServerPort = 8080
                }
            };
            computationalNode.Start();
            taskManager = null;
        }
    }
}