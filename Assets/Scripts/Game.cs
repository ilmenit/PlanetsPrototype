// need of    Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER","yes");
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using FullSerializer;

public class Game : MonoSingleton<Game>
{
    protected Game()
    {
    } // guarantee this will be always a singleton only - can't use the constructor!

    public Engine engine;
    private bool initialized = false;

    private System.Threading.Thread engineThread;
    public Definitions Definitions = null;

    public void Start()
    {
        var GUI = TextFrontend.Instance; // instantiate Text gui
    }

    new public void OnDestroy()
    {
        base.OnDestroy();
        if (engineThread != null)
            engineThread.Abort();
    }

    public void LoadDefinitions()
    {
        string path = Path.Combine(UnityEngine.Application.streamingAssetsPath, "definitions.json");
        if (!File.Exists(path))
            return;

        Definitions newDefinitions = new Definitions();

        fsData data;
        string Strdata;
        Strdata = File.ReadAllText(path);
        data = fsJsonParser.Parse(Strdata);
        fsSerializer serializer = new fsSerializer();
        serializer.TryDeserialize(data, ref newDefinitions).AssertSuccessWithoutWarnings();
        Definitions = newDefinitions;
        UnityEngine.Debug.Log("Load definitions");
    }

    public void SaveDefinitions()
    {
        UnityEngine.Debug.Log("Save definitions");
        fsData data;
        string Strdata;
        fsSerializer serializer = new fsSerializer();  //create new seralizer instance
        serializer.TrySerialize(Definitions, out data).AssertSuccessWithoutWarnings();
        Strdata = fsJsonPrinter.CompressedJson(data);
        string path = Path.Combine(UnityEngine.Application.streamingAssetsPath, "definitions.json");
        File.WriteAllText(path, Strdata);
    }

    public void Update()
    {
        if (!initialized)
        {
            initialized = true;

            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
            Application.targetFrameRate = 60;
            LoadDefinitions();

            engine = Engine.Instance;
            engine.Init();
            engineThread = Loom.Instance.RunAsync(engine.MainMultiThread);
        }
    }
}