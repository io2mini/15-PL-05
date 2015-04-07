﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Components
{
    public class TaskManager : SystemComponent
    {
        public TaskManager() : base() 
        {
            deviceType = SystemComponentType.TaskManager;
            solvableProblems = new string[] { "DVRP" };
            pararellThreads = 1;
        }
    }
}
