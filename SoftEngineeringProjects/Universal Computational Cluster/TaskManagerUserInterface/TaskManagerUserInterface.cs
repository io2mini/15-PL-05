using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Communication;
using Common.Components;

namespace Common.UserInterface
{
    public class TaskManagerUserInterface
    {
        static void Main(string[] args)
        {
            TaskManager taskManager = new TaskManager();
            Console.WriteLine("Task Manager started successfully");
            String newLine;

            while (taskManager.IsWorking)
            {
                newLine = Console.ReadLine();
                taskManager.CommunicationInfo = ParametersParser.ReadParameters(newLine, SystemComponentType.TaskManager);
            }
        }
    }
}
