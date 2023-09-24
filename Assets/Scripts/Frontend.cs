using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[Flags]
public enum Dirty
{
    None = 0,
    Map = 1,
    PlayerData = 2,
    RightPanel = 4,
    Options = 8,
    ClearAll = 0xFFFF
}

// The casts to object in the below code are an unfortunate necessity due to
// C#'s restriction against a where T : Enum constraint. (There are ways around
// this, but they're outside the scope of this simple illustration.)
public static class FlagsHelper
{
    static readonly object _lock = new object();

    public static bool IsSet<T>(T flags, T flag) where T : struct
    {
        lock (_lock)
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            return (flagsValue & flagValue) != 0;
        }
    }

    public static void Set<T>(ref T flags, T flag) where T : struct
    {
        lock (_lock)
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue | flagValue);
        }
    }

    public static void Unset<T>(ref T flags, T flag) where T : struct
    {
        lock (_lock)
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue & (~flagValue));
        }
    }
}


public abstract class Frontend : MonoBehaviour
{
    public Dirty DirtyBits = Dirty.None;

    static Frontend _instance;
    private static object _lock = new object();

    public static Frontend Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    Type t = MethodBase.GetCurrentMethod().DeclaringType;

                    _instance = FindObjectOfType(t) as Frontend;

                    if (FindObjectsOfType(t).Length > 1)
                    {
                        Debug.LogError("[Singleton] Something went really wrong " +
                                       " - there should never be more than 1 singleton!" +
                                       " Reopenning the scene might fix it.");
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent(t) as Frontend;
                        singleton.name = "(singleton) " + t.ToString();

                        //                        DontDestroyOnLoad(singleton);

                        Debug.Log("[Singleton] An instance of " + t +
                                  " is needed in the scene, so '" + singleton +
                                  "' was created with DontDestroyOnLoad.");
                    }
                    else
                    {
                        Debug.Log("[Singleton] Using instance already created: " +
                                  _instance.gameObject.name);
                    }
                }
                return _instance;
            }
        }
    }

    public void SetDirty(Dirty dirt)
    {
        //        Log.WhoAmI();
        FlagsHelper.Set(ref DirtyBits, dirt);
    }

    public void Init()
    {
        _instance = this;
        InitEx();
    }
    protected abstract void InitEx();

    #region State handlers
    public abstract void EnteringMap();
    public abstract void ExitingMap();

    public abstract void EnteringBattle();
    public abstract void ExitingBattle();

    public abstract void EnteringResearch();
    public abstract void ExitingResearch();

    public abstract void EnteringResearchConfirmation();
    public abstract void ExitingResearchConfirmation();

    public abstract void EnteringFleet();
    public abstract void ExitingFleet();

    public abstract void EnteringMenu();
    public abstract void ExitingMenu();

    public abstract void EnteringCellInfo();
    public abstract void ExitingCellInfo();

    public abstract void EnteringBuildingOnTerrain();
    public abstract void ExitingBuildingOnTerrain();

    public abstract void EnteringSelectingSupportForBuilding();
    public abstract void ExitingSelectingSupportForBuilding();


    #endregion

    #region Function that cause engine thread to wait
    public abstract IEnumerator ShowAttack(Unit attacker, Unit defender);
    public abstract IEnumerator ShowDamage(Unit unit, int damage);
    #endregion
}
