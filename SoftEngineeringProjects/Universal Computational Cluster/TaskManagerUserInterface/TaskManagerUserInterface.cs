using System;
using Common.Components;
using Common.Configuration;
using Common.Exceptions;
using Common.Properties;

namespace Common.UserInterface
{
    public class TaskManagerUserInterface
    {
        private static void Main(string[] args)
        {
            var taskManager = new TaskManager();
            Console.WriteLine(Resources.TaskManagerUserInterface_Main_Task_Manager_started_successfully);
            string newLine;
            var hasReadData = false;
            while (taskManager.IsWorking && !hasReadData)
            {
                newLine = Console.ReadLine();
                try
                {
                    taskManager.CommunicationServerInfo = ParametersParser.ReadParameters(newLine,
                        SystemComponentType.TaskManager)[0];
                    hasReadData = true;
                }
                catch (ParsingArgumentException)
                {
                    Console.WriteLine("Wrong Arguments");
                }
            }

            taskManager.Start();
            while (taskManager.IsWorking)
            {
            }
            Console.WriteLine("Task Manager ended successfully");
        }
    }
}