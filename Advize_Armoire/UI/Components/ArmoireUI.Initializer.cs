namespace Advize_Armoire;

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore;
using UnityEngine.UI;
using static StaticMembers;

public partial class ArmoireUI : MonoBehaviour
{
    internal void Initialize()
    {
        SetupPanelPosition();
        SetupTooltipFonts();
        SetupLabelFonts();
        SetupPanelSprites();
        SetupSlotButtonSprites();
        SetupCycleOutfitButtonSprites();
        SetupOutfitButtonSprites();
        SetupMiscIcons();
        SetupScrollViewSprites();
        SetupToggleHidden();
        SetupExtraneousButtonSprites();
        SetupSpriteStates();
        SetupButtonListeners();
    }

    private void SetupPanelPosition() => config.UpdateUIPanelPosition(null, null);

    private void SetupTooltipFonts()
    {
        Transform tooltip = GetComponentsInChildren<UITooltip>().First().m_tooltipPrefab.transform.GetChild(0);
        foreach (string child in new[] { "Topic", "Text" })
        {
            TextMeshProUGUI text = tooltip.Find(child).GetComponent<TextMeshProUGUI>();
            text.font = UIResources.GetFontAsset("Valheim-AveriaSerifLibre");
            text.fontMaterial = UIResources.GetMaterial("Valheim-AveriaSerifLibre - Outline");
        }
    }

    private void SetupLabelFonts()
    {
        foreach (TextMeshProUGUI label in GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true))
        {
            label.font = UIResources.GetFontAsset("Valheim-AveriaSerifLibre");
            label.fontMaterial = UIResources.GetMaterial("Valheim-AveriaSerifLibre - Outline");
            label.fontStyle = FontStyles.Normal;
            label.fontWeight = FontWeight.Regular;
            label.fontFeatures = [OTL_FeatureTag.kern];
        }
    }

    private void SetupPanelSprites()
    {
        panelBorder.sprite = UIResources.GetSprite("panel_interior_bkg_128");
        panelBackgrounds.ForEach(bg => bg.sprite = UIResources.GetSprite("panel_bkg_128"));
    }

    private void SetupSlotButtonSprites()
    {
        foreach (Button button in slotButtons)
        {
            button.GetComponent<Image>().sprite = UIResources.GetSprite("panel_bkg");
            button.transform.Find("LabelBG").GetComponent<Image>().sprite = UIResources.GetSprite("button_disabled");
        }
    }

    private void SetupCycleOutfitButtonSprites()
    {
        foreach (Button button in cycleOutfitButtons)
            button.GetComponent<Image>().sprite = UIResources.GetSprite("button");

        outfitNumBG.GetComponent<Image>().sprite = UIResources.GetSprite("button_disabled");
    }

    private void SetupOutfitButtonSprites()
    {
        SetupOutfitButton(saveOutfitButton, "file_local");
        SetupOutfitButton(loadOutfitButton, "winddirection");
        SetupOutfitButton(deleteOutfitButton, "mapicon_checked");
    }

    private void SetupOutfitButton(Button button, string iconSprite)
    {
        button.GetComponent<Image>().sprite = UIResources.GetSprite("button");
        button.transform.GetChild(0).GetComponent<Image>().sprite = UIResources.GetSprite(iconSprite);
        button.transform.GetChild(1).GetComponent<Image>().sprite = UIResources.GetSprite("button_disabled");
    }

    private void SetupMiscIcons()
    {
        childIcons.ForEach(i => i.sprite = UIResources.GetSprite("mapicon_checked"));
        hintKeyBKG.ForEach(i => i.sprite = UIResources.GetSprite("key_base"));
    }

    private void SetupScrollViewSprites()
    {
        SetupScrollButton(currentAppearanceButton, "item_background_sunken", "selection_frame");
        SetupScrollButton(hideSlotButton, "item_background_sunken", "selection_frame", "mapicon_hildir", "mapicon_checked");

        verticalScrollbar.sprite = UIResources.GetSprite("panel_interior_bkg_128");
        verticalScrollbar.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = UIResources.GetSprite("crafting_panel_bkg");

        armoireSlot.GetComponent<Image>().sprite = UIResources.GetSprite("item_background_sunken");
        armoireSlot.transform.GetChild(1).GetComponent<Image>().sprite = UIResources.GetSprite("selection_frame");
    }

    private void SetupScrollButton(Button button, string bgSprite, string frameSprite, string iconSprite = null, string iconChildSprite = null)
    {
        button.GetComponent<Image>().sprite = UIResources.GetSprite(bgSprite);
        button.transform.GetChild(1).GetComponent<Image>().sprite = UIResources.GetSprite(frameSprite);

        if (iconSprite != null)
        {
            Transform icon = button.transform.GetChild(0);
            icon.GetComponent<Image>().sprite = UIResources.GetSprite(iconSprite);
            if (iconChildSprite != null)
                icon.GetChild(0).GetComponent<Image>().sprite = UIResources.GetSprite(iconChildSprite);
        }
    }

    private void SetupToggleHidden()
    {
        Transform toggleIcon = toggleHidden.transform.GetChild(2);
        toggleIcon.GetComponent<Image>().sprite = UIResources.GetSprite("button_small");
        toggleIcon.GetChild(0).GetComponent<Image>().sprite = UIResources.GetSprite("checkbox_marker_filtered");

        toggleHidden.isOn = config.ShowAllAppearances;
        _acquiredAppearanceText = toggleHidden.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    private void SetupExtraneousButtonSprites()
    {
        outfitsButton.GetComponent<Image>().sprite = UIResources.GetSprite("button");
        cancelButton.GetComponent<Image>().sprite = UIResources.GetSprite("button");
    }

    private void SetupSpriteStates()
    {
        SpriteState spriteState = new()
        {
            highlightedSprite = UIResources.GetSprite("button_highlight"),
            pressedSprite = UIResources.GetSprite("button_pressed"),
            selectedSprite = UIResources.GetSprite("button_highlight"),
            disabledSprite = UIResources.GetSprite("button_disabled")
        };

        foreach (Button b in (List<Button>)[.. cycleOutfitButtons, outfitsButton, cancelButton, saveOutfitButton, loadOutfitButton, deleteOutfitButton])
            b.spriteState = spriteState;
    }

    internal void SetupButtonListeners()
    {
        SetupSlotPanelButtonListeners();
        SetupScrollViewButtonListeners();
        SetupToggleHiddenListener();
        SetupExtraneousButtonListeners();
        SetupOutfitPanelButtonListeners();
    }

    private void SetupSlotPanelButtonListeners()
    {
        for (int i = 0; i < slotButtons.Count; i++)
        {
            Button button = slotButtons[i];
            SlotButtonListener listener = new(this, i);
            button.onClick.AddListener(listener.OnClick);

            ArmoireInputHandler inputHandler = button.GetComponent<ArmoireInputHandler>();
            inputHandler.onRightClick.AddListener(listener.OnRightClick);
            inputHandler.onSelectChange.AddListener(listener.OnSelectChange);
        }
    }

    private void SetupScrollViewButtonListeners()
    {
        SetupScrollViewButton(currentAppearanceButton, HideScrollView);
        SetupScrollViewButton(hideSlotButton, HideSlotButtonClick);
    }

    private void SetupScrollViewButton(Button button, UnityAction clickAction)
    {
        button.onClick.AddListener(clickAction);
        ScrollViewButtonListener listener = new(this, button.gameObject);
        button.GetComponent<ArmoireInputHandler>().onSelectChange.AddListener(listener.OnSelectChange);
    }

    private void SetupToggleHiddenListener() => toggleHidden.onValueChanged.AddListener(ToggleHiddenClicked);

    private void SetupExtraneousButtonListeners()
    {
        cancelButton.onClick.AddListener(CancelButtonClicked);
        outfitsButton.onClick.AddListener(ShowOutfitsPanel);
    }

    private void SetupOutfitPanelButtonListeners()
    {
        int direction = -1;
        foreach (Button button in cycleOutfitButtons)
        {
            button.onClick.AddListener(direction < 0 ? CycleOutfitPrevious : CycleOutfitNext);
            direction = Math.Abs(direction);
        }

        saveOutfitButton.onClick.AddListener(SaveOutfitButtonClicked);
        loadOutfitButton.onClick.AddListener(LoadOutfitButtonClicked);
        deleteOutfitButton.onClick.AddListener(DeleteOutfitButtonClicked);
    }

    private void CycleOutfitPrevious() => CycleOutfitButtonClicked(-1);
    private void CycleOutfitNext() => CycleOutfitButtonClicked(1);

    private sealed class SlotButtonListener(ArmoireUI ui, int slotIndex)
    {
        internal void OnClick() => ui.AppearanceSlotButtonClicked(slotIndex);
        internal void OnRightClick() => ui.AppearanceSlotButtonRightClicked(slotIndex);
        internal void OnSelectChange(bool isSelected) => ui.UpdateAppearanceSlotButtonSelection(isSelected, slotIndex);
    }

    private sealed class ScrollViewButtonListener(ArmoireUI ui, GameObject buttonObject, int buttonIndex = -1, int variant = 0)
    {
        internal void OnClick() => ui.UnlockedAppearanceOnClick(buttonIndex, variant);
        internal void OnSelectChange(bool isSelected) => ui.UpdateScrollViewButtonSelection(isSelected, buttonObject);
    }
}
