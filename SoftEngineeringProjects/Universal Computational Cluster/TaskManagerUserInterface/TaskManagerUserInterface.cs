using System;
using Common.Components;
using Common.Configuration;
using Common.Exceptions;

namespace Common.UserInterface
{
    public class TaskManagerUserInterface
    {
        private static void Main(string[] args)
        {
            var taskManager = new TaskManager();
            Console.WriteLine("Task Manager started successfully");
            string newLine;
            var hasReadData = false;
            while (taskManager.IsWorking && !hasReadData)
            {
                newLine = Console.ReadLine();
                try
                {
                    taskManager.CommunicationInfo = ParametersParser.ReadParameters(newLine,
                        SystemComponentType.TaskManager);
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