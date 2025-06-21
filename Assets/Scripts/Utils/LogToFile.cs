using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

namespace Utils
{
    /// <summary>
    /// Unity의 Debug.Log를 파일로 저장하는 시스템입니다.
    /// </summary>
    public class LogToFile : MonoBehaviour
    {
        [Header("로그 설정")]
        [SerializeField] private bool enableFileLogging = true;
        [SerializeField] private string logFileName = "game_log.txt";
        [SerializeField] private bool includeTimestamp = true;
        [SerializeField] private bool includeLogLevel = true;
        
        private string logFilePath;
        private StreamWriter logWriter;
        private bool isInitialized = false;
        
        // 싱글톤 패턴
        public static LogToFile Instance { get; private set; }
        
        private void Awake()
        {
            Activate();
        }
        
        /// <summary>
        /// 로깅 시스템을 초기화합니다.
        /// </summary>
        private void InitializeLogging()
        {
            if (!enableFileLogging) return;
            
            try
            {
                // 프로젝트 디렉토리 내의 Logs/Gamelogs 폴더에 저장
                string projectPath = Application.dataPath;
                string projectDirectory = Directory.GetParent(projectPath).FullName;
                string logDirectory = Path.Combine(projectDirectory, "Logs", "Gamelogs");
                Directory.CreateDirectory(logDirectory);
                
                // 파일명에 타임스탬프 추가
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string fileName = $"{timestamp}_{logFileName}";
                logFilePath = Path.Combine(logDirectory, fileName);
                
                // StreamWriter 초기화
                logWriter = new StreamWriter(logFilePath, true);
                logWriter.AutoFlush = true;
                
                isInitialized = true;
                
                // 초기화 로그
                string initMessage = $"[{GetTimestamp()}] [SYSTEM] LogToFile initialized. Log file: {logFilePath}";
                Debug.Log(initMessage);
                WriteToFile(initMessage);
                
                // Application.logMessageReceived 이벤트 등록
                Application.logMessageReceived += HandleLog;
            }
            catch (Exception e)
            {
                Debug.LogError($"<color=red>[LogToFile] Failed to initialize logging: {e.Message}</color>");
            }
        }
        
        /// <summary>
        /// Unity의 로그 메시지를 처리합니다.
        /// </summary>
        /// <param name="logString">로그 메시지</param>
        /// <param name="stackTrace">스택 트레이스</param>
        /// <param name="type">로그 타입</param>
        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (!isInitialized || !enableFileLogging) return;
            
            try
            {
                string logLevel = includeLogLevel ? $"[{type}]" : "";
                string timestamp = includeTimestamp ? $"[{GetTimestamp()}]" : "";
                string message = $"{timestamp} {logLevel} {logString}";
                
                WriteToFile(message);
                
                // 에러나 예외의 경우 스택 트레이스도 추가
                if (type == LogType.Error || type == LogType.Exception)
                {
                    WriteToFile($"[{GetTimestamp()}] [STACK_TRACE] {stackTrace}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"<color=red>[LogToFile] Failed to write log: {e.Message}</color>");
            }
        }
        
        /// <summary>
        /// 파일에 메시지를 직접 씁니다.
        /// </summary>
        /// <param name="message">쓸 메시지</param>
        public void WriteToFile(string message)
        {
            if (!isInitialized || logWriter == null) return;
            
            try
            {
                logWriter.WriteLine(message);
                logWriter.Flush();
            }
            catch (Exception e)
            {
                Debug.LogError($"<color=red>[LogToFile] Failed to write to file: {e.Message}</color>");
            }
        }
        
        /// <summary>
        /// 현재 타임스탬프를 반환합니다.
        /// </summary>
        /// <returns>타임스탬프 문자열</returns>
        private string GetTimestamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
        
        /// <summary>
        /// 로그 파일 경로를 반환합니다.
        /// </summary>
        /// <returns>로그 파일 경로</returns>
        public string GetLogFilePath()
        {
            return logFilePath;
        }
        
        /// <summary>
        /// 로그 파일을 열어서 보여줍니다.
        /// </summary>
        public void OpenLogFile()
        {
            if (string.IsNullOrEmpty(logFilePath) || !File.Exists(logFilePath))
            {
                Debug.LogWarning("<color=yellow>[LogToFile] Log file does not exist.</color>");
                return;
            }
            
            try
            {
                // Windows에서 파일 탐색기로 열기
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{logFilePath}\"");
            }
            catch (Exception e)
            {
                Debug.LogError($"<color=red>[LogToFile] Failed to open log file: {e.Message}</color>");
            }
        }
        
        /// <summary>
        /// 로그 파일의 내용을 읽어서 반환합니다.
        /// </summary>
        /// <returns>로그 파일 내용</returns>
        public string ReadLogFile()
        {
            if (string.IsNullOrEmpty(logFilePath) || !File.Exists(logFilePath))
            {
                return "Log file does not exist.";
            }
            
            try
            {
                return File.ReadAllText(logFilePath);
            }
            catch (Exception e)
            {
                return $"Failed to read log file: {e.Message}";
            }
        }
        
        /// <summary>
        /// 로그 파일을 삭제합니다.
        /// </summary>
        public void ClearLogFile()
        {
            if (string.IsNullOrEmpty(logFilePath) || !File.Exists(logFilePath))
            {
                Debug.LogWarning("<color=yellow>[LogToFile] Log file does not exist.</color>");
                return;
            }
            
            try
            {
                File.Delete(logFilePath);
                Debug.Log("<color=green>[LogToFile] Log file cleared.</color>");
            }
            catch (Exception e)
            {
                Debug.LogError($"<color=red>[LogToFile] Failed to clear log file: {e.Message}</color>");
            }
        }
        
        private void OnDestroy()
        {
            Deactivate();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            // 앱이 일시정지될 때 로그 파일 플러시
            if (logWriter != null)
            {
                logWriter.Flush();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            // 앱이 포커스를 잃을 때 로그 파일 플러시
            if (!hasFocus && logWriter != null)
            {
                logWriter.Flush();
            }
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 활성화 함수
        /// </summary>
        public virtual void Activate()
        {
            // 싱글톤 설정
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeLogging();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 비활성화 함수
        /// </summary>
        public virtual void Deactivate()
        {
            // 로그 이벤트 해제
            Application.logMessageReceived -= HandleLog;
            
            // 파일 스트림 정리
            if (logWriter != null)
            {
                logWriter.Close();
                logWriter.Dispose();
                logWriter = null;
            }
            
            // 상태 초기화
            isInitialized = false;
            
            // 싱글톤 참조 정리
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
} 