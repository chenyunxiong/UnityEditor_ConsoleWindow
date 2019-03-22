using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//[InitializeOnLoad]
public class GOConsoleLog : ILogHandler {

    public static ILogHandler DefaultHandler { get; private set; }

    public static List<GOLog> Logs
    {
        get
        {
            return logs;
        }

        set
        {
            logs = value;
        }
    }

    private static GOConsoleLog handler;
    private static bool useGOConsole = false;
    private static List<GOLog> logs = new List<GOLog>();

    static GOConsoleLog()
    {
        //DefaultHandler = Debug.logger.logHandler;
        //handler = new GOConsoleLog();
        //Debug.logger.logHandler = handler;
        //Debug.logger.logEnabled = false;
        handler = new GOConsoleLog();
        Debug.logger.logHandler = handler;
    }

    public void LogException(Exception exception, UnityEngine.Object context)
    {
        if (!useGOConsole)
        {
            DefaultHandler.LogException(exception, context);
        }
        else
        {
            GOLog log = new GOLog(LogType.Exception, context, "", "error");
            Logs.Add(log);
        }
    }

    public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
    {
        if (!useGOConsole)
        {
            DefaultHandler.LogFormat(logType, context, format, args);
        }
        else
        {
            GOLog log = new GOLog(logType, context, format, args);
            Logs.Add(log);
        }
    }

    const string menuTitle = "Window/GOConsole/UseGOConsole";

    [MenuItem(menuTitle)]
    public static void ToggleSimulationMode()
    {
        useGOConsole = !useGOConsole;
    }

    [MenuItem(menuTitle, true)]
    public static bool ToggleSimulationModeValidate()
    {
        Menu.SetChecked(menuTitle, useGOConsole);
        return true;
    }
}

public class GOLog {
    public LogType logType = LogType.Log;
    public UnityEngine.Object context;
    public string format = "";
    public string info = "";
    public bool isSelected = false;
    //public object[] args;

    public GOLog(LogType logType, UnityEngine.Object context, string format, params object[] args)
    {
        this.isSelected = false;
        this.logType = logType;
        this.info = Convert.ToString(args[0]);
    }
}
