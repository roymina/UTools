using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILogService
{
    void Log(string message);
}

public class LogService : ILogService
{
    public void Log(string message)
    {
        Debug.Log(message);
    }


}
