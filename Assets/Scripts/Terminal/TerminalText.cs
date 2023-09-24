using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Single line text - line-breaks are not supported
/// </summary>
public class TerminalText : TerminalRect
{
    public string Text = "";
    public Color Color;
    public Color BackgroundColor;

    public override Vector2Int Size
    {
        get {
            return new Vector2Int(Text.Length, 1);
        }
        set { }
    }

    public new void Init()
    {
        base.Init();
        Color = Color.white;
        BackgroundColor = Color.black;
    }

    static new public TerminalText Instantiate()
    {
        var newGO = new GameObject("TerminalText");
        var newComponent = newGO.AddComponent<TerminalText>();
        newGO.transform.SetParent(newComponent.Terminal.transform);
        newComponent.Init();
        return newComponent;
    }

    public TerminalText SetText(string text)
    {
        Text = text;
        return this;
    }

    public new TerminalText SetPosition(Vector2Int pos)
    {
        base.SetPosition(pos);
        return this;
    }

    public new TerminalText SetPosition(int x, int y)
    {
        return SetPosition(new Vector2Int(x,y));
    }

    public TerminalText SetColor(Color color, Color background)
    {
        SetColor(color);
        SetBackground(background);
        return this;
    }

    public TerminalText SetColor(Color color)
    {
        Color = color;
        return this;
    }

    public TerminalText SetBackground(Color background)
    {
        BackgroundColor = background;
        return this;
    }

    public new TerminalText SetOnClick(UnityAction<TerminalRect, Vector2Int> handler)
    {
        if (onClick == null)
            onClick = new EventTerminalRectClicked();

        onClick.AddListener(handler);
        return this;
    }

    public override void Show()
    {
        Terminal.SetColor(Color, BackgroundColor);
        Terminal.PrintAt(Position.x, Position.y, Text, false);
    }
}