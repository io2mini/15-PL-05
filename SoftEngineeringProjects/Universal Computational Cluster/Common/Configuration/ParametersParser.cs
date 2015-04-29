using System;
using System.Linq;
using System.Text.RegularExpressions;
using Common.Exceptions;

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

        public static CommunicationInfo ReadParameters(string s, SystemComponentType type)
        {
            if (s == null) return null;
            var parameters = s.Split(WHITESPACES, StringSplitOptions.RemoveEmptyEntries);
            var cInfo = new CommunicationInfo();
            switch (type)
            {
                case SystemComponentType.CommunicationServer:
                    if (parameters.Contains(BACKUP_PARAMETER))
                    {
                        type = SystemComponentType.BackupCommunicationServer;
                        if (parameters.Length != 7)
                        {
                            var message = "Wrong number of arguments passed, give -address [address] -port [port] -backup"
                                          + "-t [timeout]";
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
                        ParseArgumentsForBackupCommunicationServer(parameters, ref i, ref cInfo);
                        break;
                }
            }
            return cInfo;
        }

        private static void ParseArgumentsForComputation(string[] parameters, ref int i, ref CommunicationInfo cInfo)
        {
            if (parameters[i] == ADDRESS_PARAMETER && i < parameters.Length - 1)
            {
                try
                {
                    if (Regex.IsMatch(parameters[++i], ipRegex))
                    {
                        parameters[i] = "http://" + parameters[i] + "/";
                    }
                    cInfo.CommunicationServerAddress = new Uri(parameters[i]);
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
                    cInfo.CommunicationServerPort = (ushort) port;
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
            ref CommunicationInfo cInfo)
        {
            if (parameters[i] == TIME_PARAMETER && i < parameters.Length - 1)
            {
                try
                {
                    var time = ulong.Parse(parameters[++i]);
                    cInfo.Time = time;
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
                    cInfo.CommunicationServerPort = (ushort) port;
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

        private static void ParseArgumentsForBackupCommunicationServer(string[] parameters, ref int i,
            ref CommunicationInfo cInfo)
        {
            if (parameters[i] == TIME_PARAMETER && i < parameters.Length - 1)
            {
                try
                {
                    var time = ulong.Parse(parameters[++i]);
                    cInfo.Time = time;
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
                    cInfo.CommunicationServerAddress = new Uri(parameters[++i]);
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
                    cInfo.CommunicationServerPort = (ushort) port;
                }
                catch (UriFormatException e)
                {
                    var message = "Couldn't create port";
                    throw new ParsingArgumentException(message, e);
                }
            }
            else if (parameters[i] == BACKUP_PARAMETER)
            {
                cInfo.IsBackup = true;
            }
            else
            {
                var message = string.Format("No parameter match : {0}", parameters[i]);
                throw new ParsingArgumentException(message);
            }
        }
    }
}