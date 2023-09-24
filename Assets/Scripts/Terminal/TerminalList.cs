using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TerminalList : TerminalRect
{
    public new void Init()
    {
        base.Init();
    }

    static new public TerminalList Instantiate()
    {
        var newGO = new GameObject("TerminalList");
        var newComponent = newGO.AddComponent<TerminalList>();
        newComponent.Init();
        newComponent.transform.SetParent(newComponent.Terminal.transform);
        return newComponent;
    }

    public new TerminalList SetPosition(Vector2Int position)
    {
        base.SetPosition(position);
        return this;
    }

    public new TerminalList SetPosition(int x, int y)
    {
        return SetPosition(new Vector2Int(x, y));
    }

    public new TerminalList SetOnClick(UnityAction<TerminalRect, Vector2Int> handler)
    {
        onClick.AddListener(handler);
        return this;
    }


    public void Add(TerminalRect obj)
    {
        obj.SetPosition(new Vector2Int(0,Size.y));
        obj.transform.SetParent(transform);

        // currently size of the list is set on Add, returning size dynamically is not supported
        int newX;
        if (obj.Size.x > Size.x)
            newX = obj.Size.x;
        else
            newX = Size.x;
        Size = new Vector2Int(newX , Size.y + obj.Size.y);
    }

    public override void Clear()
    {
        var children = GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child == transform)
                continue;

            child.gameObject.SetActive(false);
            GameObject.Destroy(child.gameObject);
        }
        base.Clear();
        Size = new Vector2Int(0,0);
    }
}