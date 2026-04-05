using System;
using System.Collections.Generic;

namespace m0.Bootstrap
{
    public enum LogLevelEnum
    {
        Trace = 5,
        Debug = 4,
        Info = 3,
        Warning = 2,
        Error = 1,
        Fatal = 0,
        Off = -1
    }

    public class CommandLineParameters
    {
        public bool DoHelp;
        public bool NoBackup;
        public bool NoAutostart;
        public string Autostart;
        public string RunUser;
        public string CreateUser;
        public LogLevelEnum LogLevel = LogLevelEnum.Warning;

        public IList<string> RawArguments = new List<string>();
        public IList<string> UsedArguments = new List<string>();

        public static CommandLineParameters Parse(string[] args)
        {
            CommandLineParameters result = new CommandLineParameters();

            if (args == null)
                return result;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                result.RawArguments.Add(arg);

                if (arg == "-h" || arg == "--help")
                {
                    result.DoHelp = true;
                    result.UsedArguments.Add(arg);
                }
                else if (arg == "-n" || arg == "--no-autostart")
                {
                    result.NoAutostart = true;
                    result.UsedArguments.Add(arg);
                }
                else if (arg == "-nb" || arg == "--no-backup")
                {
                    result.NoBackup = true;
                    result.UsedArguments.Add(arg);
                }
                else if (arg == "-a" || arg == "--autostart")
                {
                    if (i + 1 >= args.Length)
                        throw new ArgumentException("Missing value for parameter " + arg + ".");

                    string autostartDirectory = args[++i];
                    result.RawArguments.Add(autostartDirectory);
                    result.UsedArguments.Add(arg);
                    result.UsedArguments.Add(autostartDirectory);
                    result.Autostart = autostartDirectory;
                }
                else if (arg == "-l" || arg == "--log-level")
                {
                    if (i + 1 >= args.Length)
                        throw new ArgumentException("Missing value for parameter " + arg + ".");

                    string logLevelValue = args[++i];
                    result.RawArguments.Add(logLevelValue);
                    result.UsedArguments.Add(arg);
                    result.UsedArguments.Add(logLevelValue);
                    result.LogLevel = ParseLogLevel(logLevelValue);
                }
                else if (arg == "-u" || arg == "--user")
                {
                    if (i + 1 >= args.Length)
                        throw new ArgumentException("Missing value for parameter " + arg + ".");

                    string userName = args[++i];
                    result.RawArguments.Add(userName);
                    result.UsedArguments.Add(arg);
                    result.UsedArguments.Add(userName);
                    result.RunUser = userName;
                }
                else if (arg == "-c" || arg == "--create-user")
                {
                    if (i + 1 >= args.Length)
                        throw new ArgumentException("Missing value for parameter " + arg + ".");

                    string userName = args[++i];
                    result.RawArguments.Add(userName);
                    result.UsedArguments.Add(arg);
                    result.UsedArguments.Add(userName);
                    result.CreateUser = userName;
                }
                else
                {
                    throw new ArgumentException("Unknown parameter: " + arg + ".");
                }
            }

            return result;
        }

        public int GetM0LogLevel()
        {
            return (int)LogLevel;
        }

        public static string GetHelpText(string appName)
        {
            return
                appName + " command line options:" + Environment.NewLine +
                "  -h, --help" + Environment.NewLine +
                "      Show this help message." + Environment.NewLine +
                "  -n, --no-autostart" + Environment.NewLine +
                "      Disable autostart execution." + Environment.NewLine +
                "  -nb, --no-backup" + Environment.NewLine +
                "      Turns off automatic *.m0?.backup file creation when saving *.m0? stores." + Environment.NewLine +
                "  -a, --autostart \"directory\"" + Environment.NewLine +
                "      Set autostart directory name (relative to m0 location)." + Environment.NewLine +
                "  -u, --user <user name>" + Environment.NewLine +
                "      Run -zero with given user context." + Environment.NewLine +
                "  -c, --create-user <user name>" + Environment.NewLine +
                "      Create new user with the name \"user name\". " + Environment.NewLine +
                "      If the \"user name\" exist, the whole \"user name\" environment is re-created from scratch." + Environment.NewLine +
                "      You can use this option for resetting the user environment." + Environment.NewLine +
                "  -l, --log-level <trace|debug|info|warning|error|fatal|off>" + Environment.NewLine +
                "      Set logging verbosity.";
        }

        private static LogLevelEnum ParseLogLevel(string value)
        {
            if (string.Equals(value, "trace", StringComparison.OrdinalIgnoreCase))
                return LogLevelEnum.Trace;
            if (string.Equals(value, "debug", StringComparison.OrdinalIgnoreCase))
                return LogLevelEnum.Debug;
            if (string.Equals(value, "info", StringComparison.OrdinalIgnoreCase))
                return LogLevelEnum.Info;
            if (string.Equals(value, "warning", StringComparison.OrdinalIgnoreCase))
                return LogLevelEnum.Warning;
            if (string.Equals(value, "error", StringComparison.OrdinalIgnoreCase))
                return LogLevelEnum.Error;
            if (string.Equals(value, "fatal", StringComparison.OrdinalIgnoreCase))
                return LogLevelEnum.Fatal;
            if (string.Equals(value, "off", StringComparison.OrdinalIgnoreCase))
                return LogLevelEnum.Off;

            throw new ArgumentException(
                "Unknown log level: " + value + ". Allowed values: trace, debug, info, warning, error, fatal, off.");
        }

        public void CommandLineExecute_BeforeInitialisation()
        {            
            if (CreateUser != null)
                CommandLineExecution.CreateUser_BeforeInitialisation(CreateUser);

            if (RunUser != null)
                CommandLineExecution.RunUser_BeforeInitialisation(RunUser);
            else
                CommandLineExecution.RunUser_BeforeInitialisation("root");
        }

        public void CommandLineExecute_AfterInitialisation()
        {        
            if (CreateUser != null)
                CommandLineExecution.CreateUser(CreateUser);

            if (RunUser != null)
                CommandLineExecution.RunUser(RunUser, false);
            else
                CommandLineExecution.RunUser("root", true);
        }
    }
}
