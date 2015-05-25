using System;
using System.Linq;
using System.Text.RegularExpressions;
using Common.Exceptions;
using System.Collections.Generic;

namespace Common.Configuration
{
    public class ParametersParser
    {
        private const string TIME_PARAMETER = "-t";
        private const string BACKUP_PARAMETER = "-backup";
        private const string PORT_PARAMETER = "-port";
        private const string ADDRESS_PARAMETER = "-address";
        private static readonly char[] WHITESPACES = {' ', '\t', '\n'};

        private static readonly string ipRegex =
            @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

        public static List<CommunicationInfo> ReadParameters(string s, SystemComponentType type)
        {
            if (s == null) return null;
            var parameters = s.Split(WHITESPACES, StringSplitOptions.RemoveEmptyEntries);
            List<CommunicationInfo> cInfo = new List<CommunicationInfo>();
            switch (type)
            {
                case SystemComponentType.CommunicationServer:
                    if (parameters.Contains(BACKUP_PARAMETER))
                    {
                        type = SystemComponentType.BackupCommunicationServer;
                        if (parameters.Length != 9)
                        {
                            var message = "Wrong number of arguments passed, give -port [my_port] -backup -t [timeout] "
                                          + "-address [address] -port [port]";
                            throw new ParsingArgumentException(message);
                        }
                    }
                    else
                    {
                        if (parameters.Length != 4)
                        {
                            var message = "Wrong number of arguments passed, give -port [port] -t [timeout]";
                            throw new ParsingArgumentException(message);
                        }
                    }
                    break;
                default:
                    if (parameters.Length != 4)
                    {
                        var message = "Wrong number of arguments passed, give -address [address] -port [port]";
                        throw new ParsingArgumentException(message);
                    }
                    break;
            }
            for (var i = 0; i < parameters.Length; i++)
            {
                bool isSetUp = false;
                switch (type)
                {
                    case SystemComponentType.TaskManager:
                        ParseArgumentsForComputation(parameters, ref i, ref cInfo);
                        break;
                    case SystemComponentType.ComputationalNode:
                        ParseArgumentsForComputation(parameters, ref i, ref cInfo);
                        break;
                    case SystemComponentType.ComputationalClient:
                        ParseArgumentsForComputation(parameters, ref i, ref cInfo);
                        break;
                    case SystemComponentType.CommunicationServer:
                        ParseArgumentsForCommunicationServer(parameters, ref i, ref cInfo);
                        break;
                    case SystemComponentType.BackupCommunicationServer:
                        ParseArgumentsForBackupCommunicationServer(parameters, ref i, ref isSetUp, ref cInfo);
                        break;
                }
            }
            return cInfo;
        }

        private static void ParseArgumentsForComputation(string[] parameters, ref int i, ref List<CommunicationInfo> cInfo)
        {
            if (cInfo.Capacity == 0) cInfo.Add(new CommunicationInfo());
            if (parameters[i] == ADDRESS_PARAMETER && i < parameters.Length - 1)
            {
                try
                {
                    if (Regex.IsMatch(parameters[++i], ipRegex))
                    {
                        parameters[i] = "http://" + parameters[i] + "/";
                    }
                    cInfo[0].CommunicationServerAddress = new Uri(parameters[i]);
                }
                catch (UriFormatException e)
                {
                    var message = "Couldn't create end point adress";
                    throw new ParsingArgumentException(message, e);
                }
            }
            else if (parameters[i] == PORT_PARAMETER && i < parameters.Length - 1)
            {
                try
                {
                    var port = uint.Parse(parameters[++i]);
                    cInfo[0].CommunicationServerPort = (ushort)port;
                }
                catch (UriFormatException e)
                {
                    var message = "Couldn't create port";
                    throw new ParsingArgumentException(message, e);
                }
            }
            else
            {
                var message = string.Format("No parameter match : {0}", parameters[i]);
                throw new ParsingArgumentException(message);
            }
        }

        private static void ParseArgumentsForCommunicationServer(string[] parameters, ref int i,
            ref List<CommunicationInfo> cInfo)
        {
            if (cInfo.Capacity == 0) cInfo.Add(new CommunicationInfo());
            if (parameters[i] == TIME_PARAMETER && i < parameters.Length - 1)
            {
                try
                {
                    var time = ulong.Parse(parameters[++i]);
                    cInfo[0].Time = time;
                }
                catch (UriFormatException e)
                {
                    var message = "Couldn't create timeout";
                    throw new ParsingArgumentException(message, e);
                }
            }
            else if (parameters[i] == PORT_PARAMETER && i < parameters.Length - 1)
            {
                try
                {
                    var port = uint.Parse(parameters[++i]);
                    cInfo[0].CommunicationServerPort = (ushort) port;
                }
                catch (UriFormatException e)
                {
                    var message = "Couldn't create port";
                    throw new ParsingArgumentException(message, e);
                }
            }
            else
            {
                var message = string.Format("No parameter match : {0}", parameters[i]);
                throw new ParsingArgumentException(message);
            }
        }

        private static void ParseArgumentsForBackupCommunicationServer(string[] parameters, ref int i, ref bool portSetUp,
            ref List<CommunicationInfo> cInfo)
        {
            if (cInfo.Capacity == 0)
            {
                // first CommunicationInfo is ours
                cInfo.Add(new CommunicationInfo());
                // second CommunicationInfo ic Primary CS
                cInfo.Add(new CommunicationInfo());
            }
            if (parameters[i] == TIME_PARAMETER && i < parameters.Length - 1)
            {
                try
                {
                    var time = ulong.Parse(parameters[++i]);
                    cInfo[0].Time = time;
                }
                catch (UriFormatException e)
                {
                    var message = "Couldn't create timeout";
                    throw new ParsingArgumentException(message, e);
                }
            }
            else if (parameters[i] == ADDRESS_PARAMETER && i < parameters.Length - 1)
            {
                try
                {
                    cInfo[1].CommunicationServerAddress = new Uri(parameters[++i]);
                }
                catch (UriFormatException e)
                {
                    var message = "Couldn't create end point adress";
                    throw new ParsingArgumentException(message, e);
                }
            }
            else if (parameters[i] == PORT_PARAMETER && i < parameters.Length - 1)
            {
                try
                {
                    var port = uint.Parse(parameters[++i]);
                    if(portSetUp)
                    {
                        cInfo[1].CommunicationServerPort = (ushort)port;
                    }
                    else
                    {
                        portSetUp = true;
                        cInfo[0].CommunicationServerPort = (ushort)port;
                    }
                }
                catch (UriFormatException e)
                {
                    var message = "Couldn't create port";
                    throw new ParsingArgumentException(message, e);
                }
            }
            else if (parameters[i] == BACKUP_PARAMETER)
            {
                cInfo[0].IsBackup = true;
            }
            else
            {
                var message = string.Format("No parameter match : {0}", parameters[i]);
                throw new ParsingArgumentException(message);
            }
        }
    }
}