using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;

public class FileToConsole : MonoBehaviour {

    public const string outFile = @"D://Projet/Creajeux/TicTacTow/Console.out.log";

    StreamReader m_LogStream;
    TextWriter m_Writer;
	// Use this for initialization
	void Start () {

        try
        {
            File.Delete(outFile);
        }
        catch (Exception e) { }
        FileStream fsin = new FileStream(outFile,
                                           FileMode.CreateNew,
                                           FileAccess.ReadWrite,
                                           FileShare.ReadWrite);
        FileStream fsout = new FileStream(outFile,
                                           FileMode.Open,
                                           FileAccess.ReadWrite,
                                           FileShare.ReadWrite);

        m_Writer = new StreamWriter(fsin);
        Console.SetOut(m_Writer);
        Console.SetError(m_Writer);
        m_LogStream = new StreamReader(fsout);
	}
	
	// Update is called once per frame
	void Update () {
        try
        {
            List<string> buffer = new List<string>();
            string line;
            // Read and display lines from the file until the end of 
            // the file is reached.
            m_Writer.Flush();
            while ((line = m_LogStream.ReadLine()) != null)
            {
                buffer.Add(line);

            }
            foreach (string bufentry in buffer)
            {
                Debug.Log(bufentry);
            }
        }
        catch (Exception e)
        {
            // Let the user know what went wrong.
            Debug.LogError("The file could not be read:");

        }
        
	}
}
