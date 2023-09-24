using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Terminal : TerminalRect, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    // When Size is (0,0) on Awake(), then you have to manually call Resize
    public bool PointerFollowsPrinting = false;
    public bool MouseActive = true;

    // if set then clicks go through all the Terminal UI layers to trigger events
    public Transform CharTemplate;
    public Transform BackgroundTemplate;
    public RectTransform Pointer;
    public Transform BackgroundGrid;
    public Transform CharGrid;

    private Vector2Int cursorPos = new Vector2Int();
    private Color cursorColor = Color.white;

    [HideInInspector]
    public Color DefaultTextColor;

    [HideInInspector]
    public Color DefaultBackgroundColor;

    private Color cursorBackground;
    private char filler;
    private char[,] consoleData;
    private Color[,] consoleColors;
    private Color[,] consoleBackgrounds;
    private Vector2 CellSize;

    private bool MouseIsOver = false;
    private Vector3 previousMousePosition = Vector3.back;
    private bool initialized = false;

    // Use this for initialization
    public void Awake()
    {
        Init();
        if (Size.x > 0 && Size.y > 0)
            Resize(Size);
    }

    public new void Init()
    {
        base.Init();
        base.Terminal = this;

        CellSize = BackgroundTemplate.GetComponent<RectTransform>().sizeDelta;
        BackgroundGrid.GetComponent<GridLayoutGroup>().cellSize = CellSize;
        CharGrid.GetComponent<GridLayoutGroup>().cellSize = CellSize;
        
        Pointer.sizeDelta = CellSize;

        // get default colors
        DefaultBackgroundColor = BackgroundTemplate.GetComponent<Image>().color;
        DefaultTextColor = CharTemplate.GetComponent<Text>().color;

        // move templates to grid
        CharTemplate.SetParent(CharGrid);
        BackgroundTemplate.SetParent(BackgroundGrid);
        // make sure that the object are in proper order
        BackgroundGrid.transform.SetSiblingIndex(0);
        CharGrid.transform.SetSiblingIndex(1);
        Pointer.transform.SetSiblingIndex(2);

        initialized = true;
    }

    /// <summary>
    /// Keep in mind Unity UI Vertex cap of 65535 per canvas
    /// </summary>
    public void Resize(int x, int y)
    {
        Resize(new Vector2Int(x, y));
    }

    public void Resize(Vector2Int newSize)
    {
        if (!initialized)
            Init();
        Size = newSize;
        consoleData = new char[Size.y, Size.x];
        consoleColors = new Color[Size.y, Size.x];
        consoleBackgrounds = new Color[Size.y, Size.x];

        var backgroundGridLayout = BackgroundGrid.GetComponent<GridLayoutGroup>();
        backgroundGridLayout.constraintCount = Size.x;

        var charGridLayout = CharGrid.GetComponent<GridLayoutGroup>();
        charGridLayout.constraintCount = Size.x;

        // destroy old grid objects
        while (BackgroundGrid.childCount > newSize.x * newSize.y)
        {
            GameObject obj = BackgroundGrid.GetChild(0).gameObject;
            GameObject.DestroyImmediate(obj);
        }

        while (CharGrid.childCount > newSize.x * newSize.y)
        {
            GameObject obj = CharGrid.GetChild(0).gameObject;
            GameObject.DestroyImmediate(obj);
        }

        // add up to needed
        while (BackgroundGrid.childCount < newSize.x * newSize.y)
        {
            GameObject newText = GameObject.Instantiate(BackgroundGrid.GetChild(0).gameObject);
            newText.transform.SetParent(BackgroundGrid);
        }
        while (CharGrid.childCount < newSize.x * newSize.y)
        {
            GameObject newText = GameObject.Instantiate(CharGrid.GetChild(0).gameObject);
            newText.transform.SetParent(CharGrid);
        }

        Clear();
        Refresh();

        foreach (var child in transform.parent.GetComponentsInChildren<RectTransform>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(child);
        }

        GetComponent<RectTransform>().sizeDelta = BackgroundGrid.GetComponent<RectTransform>().sizeDelta;
    }

    public void SetFiller(char filler = '.')
    {
        this.filler = filler;
    }

    public void Clear(char filler = ' ')
    {
        SetFiller(filler);
        SetColor(DefaultTextColor, DefaultBackgroundColor);
        for (int y = 0; y < Size.y; ++y)
        {
            for (int x = 0; x < Size.x; ++x)
            {
                consoleData[y, x] = this.filler;
                consoleColors[y, x] = DefaultTextColor;
                consoleBackgrounds[y, x] = DefaultBackgroundColor;
            }
        }
        MoveCursor(0, 0);
    }

    public Vector2Int GetCursorPosition()
    {
        return cursorPos;
    }

    public void MoveCursor(Vector2Int newPos)
    {
        cursorPos = newPos;
    }

    public void MoveCursor(int x, int y)
    {
        MoveCursor(new Vector2Int(x, y));
    }

    public void SetColor(Color color)
    {
        this.cursorColor = color;
    }

    public void SetBackground(Color background)
    {
        this.cursorBackground = background;
    }

    public void SetColor(Color color, Color background)
    {
        SetColor(color);
        SetBackground(background);
    }

    public void DisplayUIElements()
    {
        // Display children
        var previousColor = cursorColor;
        var previousBackground = cursorBackground;
        Vector2Int previousCursorPosition = GetCursorPosition();
        GetComponent<TerminalRect>().DisplayChildTerminalRects();
        MoveCursor(previousCursorPosition);
        cursorColor = previousColor;
        cursorBackground = previousBackground;
    }

    public void Refresh()
    {
        DisplayUIElements();

        // Refresh screen

        for (int y = 0; y < Size.y; ++y)
        {
            for (int x = 0; x < Size.x; ++x)
            {
                // getchild of BackGround, which is Char
                Transform cellTransform = BackgroundGrid.GetChild(x + y * Size.x);
                cellTransform.GetComponent<Image>().color = consoleBackgrounds[y, x];
                Transform textTranfsofrm = CharGrid.GetChild(x + y * Size.x);
                Text text = textTranfsofrm.GetComponent<Text>();
                text.color = consoleColors[y, x];
                text.text = consoleData[y, x].ToString();
            }
        }
        if (PointerFollowsPrinting)
            SetPointerPosition(cursorPos);
    }

    public char GetChar(int x, int y)
    {
        if (InObject(x,y))
            return consoleData[y, x];
        return '\0';
    }

    public Color GetColor(int x, int y)
    {
        if (InObject(x, y))
            return consoleColors[y, x];
        return Color.black;
    }

    public void DrawRect(Vector2Int pos, Vector2Int size)
    {
        for (int y = pos.y; y < pos.y + size.y; ++y)
        {
            Terminal.PrintAt(pos.x, y, "", false, size.x);
        }
    }

    public void Print(string toPrint, bool wrap = true, int definedLength = -1)
    {
        PrintAt(cursorPos.x, cursorPos.y, toPrint, wrap, definedLength);
    }

    public void PrintAt(Vector2Int pos, string toPrint, bool wrap = true, int definedLength = -1)
    {
        PrintAt(pos.x, pos.y, toPrint, wrap, definedLength);
    }

    public void PrintAt(int x, int y, string toPrint, bool wrap = true, int definedLength = -1)
    {
        cursorPos.x = x;
        cursorPos.y = y;
        toPrint = toPrint.Replace(System.Environment.NewLine, "\n");

        if (definedLength == -1)
            definedLength = toPrint.Length;

        for (int printed = 0; printed < definedLength; ++printed)
        {
            char character;
            if (printed < toPrint.Length)
                character = toPrint[printed];
            else
                character = filler;

            if (character == '\n' || character == '\r')
            {
                if (wrap)
                {
                    GoToNextLine();
                }
                else
                {
                    cursorPos.x = x;
                    ++cursorPos.y;
                }
                continue;
            }
            if (wrap && cursorPos.x == Size.x) // right border reached
            {
                GoToNextLine();
            }

            if (InObject(cursorPos.x, cursorPos.y))
            {
                consoleData[cursorPos.y, cursorPos.x] = character;
                consoleColors[cursorPos.y, cursorPos.x] = cursorColor;
                consoleBackgrounds[cursorPos.y, cursorPos.x] = cursorBackground;
            }
            ++cursorPos.x;
        }
        // check if we are ending out the console after printing last char
        if (wrap && cursorPos.x == Size.x) // right border reached
            GoToNextLine();
        MoveCursor(cursorPos.x, cursorPos.y);
    }

    private void GoToNextLine()
    {
        // Debug.Log("GoToNextLine");
        ++cursorPos.y;
        cursorPos.x = 0;
        while (cursorPos.y >= Size.y)
        {
            MoveContentUp();
            --cursorPos.y;
        }
    }

    private void MoveContentUp()
    {
        // Debug.Log("MoveContentUp");
        for (int copyY = 1; copyY < Size.y; ++copyY)
        {
            for (int copyX = 0; copyX < Size.x; ++copyX)
            {
                consoleData[copyY - 1, copyX] = consoleData[copyY, copyX];
                consoleColors[copyY - 1, copyX] = consoleColors[copyY, copyX];
                consoleBackgrounds[copyY - 1, copyX] = consoleBackgrounds[copyY, copyX];
            }
        }
        for (int copyX = 0; copyX < Size.x; ++copyX)
        {
            consoleData[Size.y - 1, copyX] = filler;
            consoleColors[Size.y - 1, copyX] = DefaultTextColor;
            consoleBackgrounds[Size.y - 1, copyX] = DefaultBackgroundColor;
        }
    }

    public void SetPointerPosition(Vector2Int pos)
    {
        Pointer.localPosition = new Vector3(pos.x * CellSize.x, -pos.y * CellSize.y, 0);
    }

    // Update is called once per frame
    public void Update()
    {
        if (MouseActive && MouseIsOver)
        {
            var pos = Input.mousePosition;
            if (previousMousePosition != pos)
            {
                int GridPosX;
                int GridPosY;
                MouseToTerminalPosition(pos, out GridPosX, out GridPosY);
                if (InObject(GridPosX,GridPosY))
                    SetPointerPosition(new Vector2Int(GridPosX, GridPosY));
                previousMousePosition = pos;
            }
        }
    }

    private void MouseToTerminalPosition(Vector2 position, out int GridPosX, out int GridPosY)
    {
        Vector2 localPoint;
        var rectangle = BackgroundGrid.GetComponent<RectTransform>();
        GridPosX = -1;
        GridPosY = -1;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectangle, position, null, out localPoint))
            return;

        float xpos = localPoint.x;
        float ypos = -localPoint.y;

        //Debug.Log("XPos: " + xpos + ", YPos: " + ypos);

        GridPosX = (int)(xpos / CellSize.x);
        GridPosY = (int)(ypos / CellSize.y);
    }

    public void OnPointerClick(PointerEventData pointerData)
    {
        if (!MouseActive)
            return;

        if (onClick == null)
            return;

        int GridPosX;
        int GridPosY;
        MouseToTerminalPosition(pointerData.position, out GridPosX, out GridPosY);
        // Log.Print("OnPointerClick: X=" + GridPosX.ToString() + ",Y=" + GridPosY.ToString());

        var rects = GetComponentsInChildren<TerminalRect>(false);

        foreach (var rect in rects)
        {
            if (!rect.gameObject.activeInHierarchy)
                continue;

            if (rect.InObject(GridPosX, GridPosY) && rect.enabled)
            {
                rect.onClick.Invoke(rect, new Vector2Int(GridPosX, GridPosY) - rect.Position);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!MouseActive)
            return;
        MouseIsOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!MouseActive)
            return;
        MouseIsOver = false;
    }
}
