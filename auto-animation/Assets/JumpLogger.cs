using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class JumpLogFile {
    public string filename;
    public string logFolder = "C:\\Users\\Oberon\\Documents\\Thesis\\auto-animation\\logs\\";
    public string[] columns;
    public string delimiter = ";";

    public void StartLog() {
        using (TextWriter file = File.CreateText(logFolder + filename)) {
            file.WriteLine("sep=" + delimiter);
            file.WriteLine(string.Join(delimiter, columns));
        }
    }
    
    public void AddRow(List<string> data) {
        using (TextWriter file = File.AppendText(logFolder + filename)) {
            file.WriteLine(string.Join(delimiter, data.ToArray()));
        }
    }
}

[System.Serializable]
public class JumpLogger {
    public JumpLogFile[] files;
    
    public void StartAll() {
        foreach (JumpLogFile f in files) {
            f.StartLog();
        }
    }

    public IEnumerable<JumpLogFile> GetFile(string filename) {
        IEnumerable<JumpLogFile> query =
            from f in files
            where f.filename == filename
            select f;
        return query;
    }
}
