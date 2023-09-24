using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EventTerminalRectClicked : UnityEvent<TerminalRect, Vector2Int>
{
}

public class TerminalRect : MonoBehaviour
{
    public EventTerminalRectClicked onClick;
    public bool CleanOnDisable;

    private Terminal terminal;

    public Terminal Terminal
    {
        get
        {
            if (terminal == null)
                terminal = FindObjectOfType<Terminal>();
            return terminal;
        }
        set
        {
            terminal = value;
        }
    }

    public Vector2Int LocalPosition;
    public virtual Vector2Int Size { get; set; }

    public TerminalRect TerminalParent
    {
        get
        {
            if (transform.parent)
                return transform.parent.GetComponent<TerminalRect>();
            else
                return null;
        }
    }

    public TerminalRect SetPosition(Vector2Int position)
    {
        LocalPosition = position;
        return this;
    }

    public TerminalRect SetPosition(int x, int y)
    {
        return SetPosition(new Vector2Int(x, y));
    }

    public TerminalRect SetSize(Vector2Int size)
    {
        Size = size;
        return this;
    }

    public TerminalRect SetSize(int x, int y)
    {
        return SetSize(new Vector2Int(x, y));
    }

    public TerminalRect SetOnClick(UnityAction<TerminalRect, Vector2Int> handler)
    {
        onClick.AddListener( handler );
        return this;
    }

    public TerminalRect RemoveOnClick(UnityAction<TerminalRect, Vector2Int> handler)
    {
        onClick.RemoveListener(handler);
        return this;
    }

    public void Init()
    {
        onClick = new EventTerminalRectClicked();
        CleanOnDisable = true;
    }

    static public TerminalRect Instantiate()
    {
        var newGO = new GameObject("TerminalRect");
        var newComponent = newGO.AddComponent<TerminalRect>();
        newComponent.Init();
        newComponent.transform.SetParent(newComponent.Terminal.transform);
        return newComponent;
    }


    public void Start()
    {
        Terminal = GetComponentInParent<Terminal>();
    }

    public void DisplayChildTerminalRects()
    {
        Show();
        foreach (Transform child in transform)
        {
            var termRect = child.GetComponent<TerminalRect>();

            if (termRect != null && termRect.gameObject.activeInHierarchy)
                termRect.DisplayChildTerminalRects();
        }
    }

    public virtual void Show()
    {

    }

    public Vector2Int Position
    {
        get
        {
            if (TerminalParent != null)
                return TerminalParent.Position + LocalPosition;
            else
                return LocalPosition;
        }

        set
        {
            if (TerminalParent != null)
                LocalPosition = value - TerminalParent.Position;
            else
                LocalPosition = value;
        }
    }

    public bool InLocalObject(int x, int y)
    {
        return x >= 0
            && x < Size.x
            && y >= 0
            && y < Size.y;
    }

    public bool InLocalObject(Vector2Int pos)
    {
        return InObject(pos.x, pos.y);
    }

    public bool InObject(int x, int y)
    {
        return x >= Position.x &&
            x < (Position.x + Size.x) &&
            y >= Position.y &&
            y < (Position.y + Size.y);
    }

    public bool InObject(Vector2Int pos)
    {
        return InObject(pos.x, pos.y);
    }

    public virtual void Clear()
    {
        Terminal.SetColor(Terminal.DefaultTextColor, Terminal.DefaultBackgroundColor);
        Terminal.DrawRect(Position, Size);
    }

    public void OnDisable()
    {
        if (CleanOnDisable)
            Clear();
    }

    // calls handler with local position withing this ConsoleObject
    public void ClickHandler(Vector2Int position)
    {
        if (onClick == null)
            return;
        
        if (InObject(position))
        {
            onClick.Invoke(this, position - Position);
        }
    }
}