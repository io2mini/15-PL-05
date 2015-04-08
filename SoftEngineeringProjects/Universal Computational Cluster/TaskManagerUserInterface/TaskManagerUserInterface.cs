using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Communication;
using Common.Components;
using Common.Exceptions;

namespace Common.UserInterface
{
    public class TaskManagerUserInterface
    {
        static void Main(string[] args)
        {
            TaskManager taskManager = new TaskManager();
            Console.WriteLine("Task Manager started successfully");
            String newLine;
            bool hasReadData = false;
            while (taskManager.IsWorking && !hasReadData)
            {
                newLine = Console.ReadLine();
                try
                {
                    taskManager.CommunicationInfo = ParametersParser.ReadParameters(newLine, SystemComponentType.TaskManager);
                    hasReadData = true;
                }
                catch(ParsingArgumentException)
                {
                    Console.WriteLine("Wrong Arguments");
                    continue;
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
