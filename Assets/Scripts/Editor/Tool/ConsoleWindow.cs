using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ConsoleWindow : EditorWindow {
    [MenuItem("Window/GOConsole/ConsoleWindow")]
    public static void CreateConsoleWindow()
    {
        EditorWindow window = EditorWindow.GetWindow<ConsoleWindow>();
        window.Show();
    }

    GUIStyle logStyle = new GUIStyle();
    GUIStyle toolBarStyle;
    Texture2D logIconSml;
    Texture2D warnIconSml;
    Texture2D errorIconSml;
    Texture2D logActiveIconSml;
    Texture2D warnActiveIconSml;
    Texture2D errorActiveIconSml;
    Texture2D logIcon;
    Texture2D warnIcon;
    Texture2D errorIcon;
    Texture2D logBgOdd;
    Texture2D logBgEven;
    Texture2D logBgSelected;

    GUIContent logContent = new GUIContent();
    GUIContent warnContent = new GUIContent();
    GUIContent errorContent = new GUIContent();

    bool clear = false;
    bool collapse = false;
    bool clearOnPlay = false;
    bool errorPause = false;
    bool log = false;
    bool logWarning = false;
    bool logError = false;

    string searchStr = "All";

    Vector2 upScrollView;

    Rect resizeRect = new Rect(0, 0, 0, 20);
    Rect rect = new Rect(0, 0, 0, 0);
    bool resizing = false;    
    float ratio = 0.5f;
    const int MinBoxSize = 28;

    private List<EditorLog> logs = new List<EditorLog>();
    private int[] logLengths = { 0, 0, 0 };
    private EditorLog selectedLog;

    public void OnEnable()
    {
        logIcon = EditorGUIUtility.Load("console.infoicon") as Texture2D;
        logActiveIconSml = EditorGUIUtility.Load("console.infoicon.sml") as Texture2D;
        logIconSml = EditorGUIUtility.Load("console.infoicon.sml") as Texture2D;
        logContent.image = logIconSml;
        logContent.text = "100";

        warnIcon = EditorGUIUtility.Load("console.warnicon") as Texture2D;
        warnActiveIconSml = EditorGUIUtility.Load("console.warnicon.sml") as Texture2D;
        warnIconSml = EditorGUIUtility.Load("console.warnicon.inactive.sml") as Texture2D;
        warnContent.image = warnIconSml;
        warnContent.text = "100";

        errorIcon = EditorGUIUtility.Load("console.erroricon") as Texture2D;
        errorActiveIconSml = EditorGUIUtility.Load("console.erroricon.sml") as Texture2D;
        errorIconSml = EditorGUIUtility.Load("console.erroricon.inactive.sml") as Texture2D;
        errorContent.image = errorIconSml;
        errorContent.text = "100";

        logBgOdd = EditorGUIUtility.Load("builtin skins/darkskin/images/cn entrybackodd.png") as Texture2D;
        logBgEven = EditorGUIUtility.Load("builtin skins/darkskin/images/cnentrybackeven.png") as Texture2D;
        logBgSelected = EditorGUIUtility.Load("builtin skins/darkskin/images/menuitemhover.png") as Texture2D;

        Application.logMessageReceived += LogMessageReceived;
    }

    public void OnDisable()
    {
        Application.logMessageReceived -= LogMessageReceived;
    }

    private void LogMessageReceived(string condition, string stackTrace, LogType type)
    {
        EditorLog log = new EditorLog(false, condition, stackTrace, type);
        logs.Add(log);
    }

    public void OnGUI()
    {
        // prepare style and resize rect  
        toolBarStyle = EditorStyles.toolbarButton;
        GUIStyle box = GUI.skin.GetStyle("CN Box");
        logStyle.normal.textColor = Color.white;
        EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeVertical);

        // draw toolbar 
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(EditorStyles.toolbar.fixedHeight), GUILayout.ExpandWidth(true));
        {
            clear = GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50));
            collapse = GUILayout.Toggle(collapse, "Collapse", EditorStyles.toolbarButton, GUILayout.Width(60));
            clearOnPlay = GUILayout.Toggle(clearOnPlay, "Clear On Play", EditorStyles.toolbarButton, GUILayout.Width(90));
            errorPause = GUILayout.Toggle(errorPause, "Error Pause", EditorStyles.toolbarButton, GUILayout.Width(70));

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal(toolBarStyle);
            {
                GUIStyle searchStyle = GUI.skin.GetStyle("ToolbarSeachTextField");
                searchStr = GUILayout.TextField(searchStr, searchStyle, GUILayout.Width(200));
                if(GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
                {

                }
            }
            GUILayout.EndHorizontal();
            log = GUILayout.Toggle(log, logContent, EditorStyles.toolbarButton, GUILayout.Width(toolBarStyle.CalcSize(logContent).x));
            logWarning = GUILayout.Toggle(logWarning, warnContent, EditorStyles.toolbarButton, GUILayout.Width(toolBarStyle.CalcSize(warnContent).x));
            logError = GUILayout.Toggle(logError, errorContent, EditorStyles.toolbarButton, GUILayout.Width(toolBarStyle.CalcSize(errorContent).x));

        }
        EditorGUILayout.EndHorizontal();

        // draw content
        Rect tmp = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
        {
            if ((Event.current.type != EventType.Layout) && (Event.current.type != EventType.used))
            {                
                rect = tmp;
                resizeRect.width = rect.width;
                resizeRect.y = Mathf.Clamp(rect.height * ratio, MinBoxSize, rect.height - MinBoxSize);
                ratio = (resizeRect.y) / rect.height;
            }

            EditorGUILayout.BeginVertical(GUILayout.Height(rect.height * ratio));
            //GUILayout.Box("", box, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            upScrollView = GUILayout.BeginScrollView(upScrollView);
            DragLogs();
            GUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            GUILayout.Box("", box, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();

        // whether to resize content
        if (Event.current.type == EventType.MouseDown && resizeRect.Contains(Event.current.mousePosition))
        {
            resizing = true;
        }
        else if (resizing && Event.current.type == EventType.MouseUp)
        {
            resizing = false;
        }
        // repaint window after drag
        if (Event.current.type == EventType.MouseDrag && resizing)
        {
            resizeRect.y = Event.current.mousePosition.y - rect.y;
            ratio = resizeRect.y / rect.height;
            Repaint();
        }
    }

    void DragLogs()
    {
        for (int i = 0; i < logs.Count; i++)
        {
            logStyle.normal.background = i % 2 == 0 ? logBgOdd : logBgEven;
            logStyle.normal.background = logs[i].isSelected ? logBgSelected : logStyle.normal.background;
            if (GUILayout.Button(new GUIContent(logs[i].info, logIcon), logStyle, GUILayout.ExpandWidth(true), GUILayout.Height(32)))
            {
                //logStyle.normal.background = logBgSelected;
                if(selectedLog != null)
                {
                    selectedLog.isSelected = false;
                }
                selectedLog = logs[i];
                selectedLog.isSelected = true;
                if(Event.current.clickCount >= 2)
                {
                    
                }
            }
            Repaint();
        }
    }

    public class EditorLog
    {
        public LogType logType = LogType.Log;
        public UnityEngine.Object context;
        public string format = "";
        public string info = "";
        public bool isSelected = false;
        public string message;
        public LogType type;
        //public object[] args;

        public EditorLog(bool isSelected, string info, string message, LogType type)
        {
            this.isSelected = isSelected;
            this.info = info;
            this.message = message;
            this.type = type;
        }
    }
}
