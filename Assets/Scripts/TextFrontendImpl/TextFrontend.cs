using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public enum RightPanelState
{
    None,
    CellInfo,
    Research,
    Production
}


public class TextFrontend : Frontend
{
    public Terminal Terminal;
    public delegate void ShowViewHandler();
    public ShowViewHandler ShowView;

    TextMainView TextMainView;
    TextBattleView TextBattleView;

    public void Start()
    {
        Log.WhoAmI();
        Init();
    }

    public override void EnteringMap()
    {
        Log.WhoAmI();
        ShowView = TextMainView.ShowMapView;
        TextMainView.ClickableMap.gameObject.SetActive(true);
        TextMainView.ClickableFleetButton.gameObject.SetActive(true);
        TextMainView.ClickableResearchButton.gameObject.SetActive(true);
        TextMainView.ClickableTurnButton.gameObject.SetActive(true);
        TextMainView.ClickableBattleButton.gameObject.SetActive(true);
        Frontend.Instance.SetDirty(Dirty.ClearAll);
    }

    public override void ExitingMap()
    {
        Log.WhoAmI();
        TextMainView.ClickableMap.gameObject.SetActive(false);
        TextMainView.ClickableFleetButton.gameObject.SetActive(false);
        TextMainView.ClickableResearchButton.gameObject.SetActive(false);
        TextMainView.ClickableTurnButton.gameObject.SetActive(false);
        TextMainView.ClickableBattleButton.gameObject.SetActive(false);
    }

    public override void EnteringBattle()
    {
        Log.WhoAmI();
        ShowView = TextBattleView.ShowBattleView;
        Frontend.Instance.SetDirty(Dirty.ClearAll);
    }

    public override void ExitingBattle()
    {
        Log.WhoAmI();
    }

    public override void EnteringResearch()
    {
        Log.WhoAmI();
        TextMainView.RightPanel.gameObject.SetActive(true);
        TextMainView.PanelList.SetOnClick(TextMainView.ResearchListClickHandler);
        TextMainView.ShowResearchPanel();
    }

    public override void ExitingResearch()
    {
        Log.WhoAmI();
        TextMainView.PanelList.RemoveOnClick(TextMainView.ResearchListClickHandler);
        TextMainView.RightPanel.gameObject.SetActive(false);
    }

    public override void EnteringResearchConfirmation()
    {
        Log.WhoAmI();
        TextMainView.RightPanel.gameObject.SetActive(true);
        TextMainView.PanelList.SetOnClick(TextMainView.ResearchConfirmationClickHandler);
        TextMainView.PanelList.SetPosition(0, 2);
        TextMainView.ShowResearchConfirmationPanel();
    }

    public override void ExitingResearchConfirmation()
    {
        Log.WhoAmI();
        TextMainView.PanelList.RemoveOnClick(TextMainView.ResearchConfirmationClickHandler);
        TextMainView.PanelList.SetPosition(0, 2);
        TextMainView.RightPanel.gameObject.SetActive(false);
    }



    public override void EnteringBuildingOnTerrain()
    {
        Log.WhoAmI();
        TextMainView.ClickableMap.gameObject.SetActive(true);
        TextMainView.RightPanel.gameObject.SetActive(true);
        TextMainView.PanelList.SetOnClick(TextMainView.BuildingOnTerrainListClickHandler);
        TextMainView.ShowBuildingOnTerrainList();
    }

    public override void ExitingBuildingOnTerrain()
    {
        Log.WhoAmI();
        TextMainView.ClickableMap.gameObject.SetActive(false);
        TextMainView.PanelList.RemoveOnClick(TextMainView.BuildingOnTerrainListClickHandler);
        TextMainView.RightPanel.gameObject.SetActive(false);
    }


    public override void EnteringSelectingSupportForBuilding()
    {
        Log.WhoAmI();
        TextMainView.ClickableMap.gameObject.SetActive(true);
        TextMainView.RightPanel.gameObject.SetActive(true);
        TextMainView.PanelList.SetOnClick(TextMainView.SupplyListClickHandler);
        TextMainView.ShowSupplyForBuilding();
    }

    public override void ExitingSelectingSupportForBuilding()
    {
        Log.WhoAmI();
        TextMainView.ClickableMap.gameObject.SetActive(false);
        TextMainView.PanelList.RemoveOnClick(TextMainView.SupplyListClickHandler);
        TextMainView.RightPanel.gameObject.SetActive(false);
    }


    public override void EnteringFleet()
    {
        Log.WhoAmI();
        TextMainView.RightPanel.gameObject.SetActive(true);
        TextMainView.PanelList.SetOnClick(TextMainView.FleetListClickHandler);
        TextMainView.ShowFleetPanel();
    }

    public override void ExitingFleet()
    {
        Log.WhoAmI();
        TextMainView.PanelList.RemoveOnClick(TextMainView.FleetListClickHandler);
        TextMainView.RightPanel.gameObject.SetActive(false);
    }


    public override void EnteringMenu()
    {
        Log.WhoAmI();
    }

    public override void ExitingMenu()
    {
        Log.WhoAmI();
    }


    public override void EnteringCellInfo()
    {
        Log.WhoAmI();
        TextMainView.ClickableMap.gameObject.SetActive(true);
        TextMainView.PanelList.SetOnClick(TextMainView.CellInfoListClickHandler);
        TextMainView.RightPanel.gameObject.SetActive(true);
        TextMainView.PanelList.SetPosition(0, 5);
        TextMainView.ShowCellInfo();
    }

    public override void ExitingCellInfo()
    {
        Log.WhoAmI();
        TextMainView.ClickableMap.gameObject.SetActive(false);
        TextMainView.PanelList.RemoveOnClick(TextMainView.CellInfoListClickHandler);
        TextMainView.RightPanel.gameObject.SetActive(false);
        TextMainView.PanelList.SetPosition(0, 2);
        TextMainView.RightPanelHeader.BackgroundColor = Terminal.DefaultBackgroundColor;
    }



    public void Update()
    {
        bool acquiredLock = false;

        // Threading rule
        // If something to clean in Frontent, clean it
        // If something to clean in model, don't add additional dirt

        try
        {
            Monitor.TryEnter(Engine.Instance.threadLock, 0, ref acquiredLock);
            if (acquiredLock)
            {

                // Code that accesses resources that are protected by the lock.
                // there is nothing to display
                if (DirtyBits == Dirty.None)
                    return;

                // if active player is not gui player, then ignore what happened (?)
                if (Model.Instance.ActivePlayer != Model.Instance.GuiPlayer)
                {
                    DirtyBits = Dirty.None;
                    return;
                }

                if (DirtyBits == Dirty.ClearAll)
                    Terminal.Clear();

                ShowView?.Invoke();

                Terminal.Refresh();
                DirtyBits = Dirty.None;
            }
        }
        finally
        {
            if (acquiredLock)
            {
                Monitor.Exit(Engine.Instance.threadLock);
            }
        }
    }

    protected override void InitEx()
    {
        Log.WhoAmI();
        
        TextMainView = gameObject.AddComponent<TextMainView>();
        TextMainView.Terminal = Terminal;
        TextMainView.Init();

        TextBattleView = gameObject.AddComponent<TextBattleView>();
        TextBattleView.Terminal = Terminal;
        TextBattleView.Init();

        Terminal.Resize(new Vector2Int(40, 24));
        Terminal.DefaultBackgroundColor = Color.black;
        Terminal.Clear();
    }

    public override IEnumerator ShowAttack(Unit attacker, Unit defender)
    {
        for (int x=0;x<10;++x)
        {
            Terminal.PrintAt(x, 22, "*");
            Terminal.Refresh();
            yield return new WaitForSeconds(.05f);
        }
    }

    public override IEnumerator ShowDamage(Unit unit, int damage)
    {
        yield return 0;
    }


    public IEnumerator Pause()
    {
        Debug.Log("Pause start");
        yield return new WaitForSeconds(3);
        Debug.Log("Pause end");
    }
}