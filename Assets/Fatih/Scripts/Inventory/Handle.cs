using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Handle : MonoBehaviour
{
    public static Handle instance;

    public int index;

    private MeshRenderer meshrenderer;
    private MeshFilter mesh;

    private PlayerInputs playerInputs;
    
    [Header("UI Settings")]
    [SerializeField] private Transform ui_parent;
    [SerializeField] private Texture newTexture;
    [SerializeField] private Texture oldTexture;

    // UI Zamanlayıcısını takip etmek için değişken
    private Coroutine uiHideCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // BU SATIRLARI AÇTIM (Yoksa SetHandlePrefab hata verir)
        mesh = GetComponent<MeshFilter>();
        meshrenderer = GetComponent<MeshRenderer>();

        playerInputs = InputManager.instance.playerInputs;

        playerInputs.Interaction.Num1.performed += _ => SelectSlot(0);
        playerInputs.Interaction.Num2.performed += _ => SelectSlot(1);
        playerInputs.Interaction.Num3.performed += _ => SelectSlot(2);
        playerInputs.Interaction.Num4.performed += _ => SelectSlot(3);

        playerInputs.Interaction.Scroll.performed += OnScroll;
        playerInputs.Interaction.Drop.performed += _ => OnDropItem();

        // Oyun başladığında UI kapalı başlasın
        if (ui_parent != null)
        {
            ui_parent.gameObject.SetActive(false);
        }
    }

    public void SetHandlePrefab()
    {
        //animasyonu oynat
        //seçili slotun sizeını büyüt ve rengini değiştir
        ShowUIWithTimer();

        // Envanter slotu boşsa veya prefab yoksa görseli temizle
        /*if (InventoryController.instance.player_inventory.slots.Count <= index || 
            InventoryController.instance.player_inventory.slots[index].prefab == null)
        {
            if (mesh != null) mesh.mesh = null;
            if (meshrenderer != null) meshrenderer.material = null;
            return;
        }

        GameObject prefab = InventoryController.instance.player_inventory.slots[index].prefab;

        if (mesh != null) mesh.mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
        if (meshrenderer != null) meshrenderer.material = prefab.GetComponent<MeshRenderer>().sharedMaterial;*/
    }

    #region Inputs

    public void SelectSlot(int value)
    {
        index = value;
        SetHandlePrefab();
        SelectedSlotUI();

        // Slot değişince UI'ı göster ve sayacı başlat
    }

    private void OnScroll(InputAction.CallbackContext ctx)
    {
        Vector2 scroll = ctx.ReadValue<Vector2>();

        if (scroll.y > 0)
        {
            index++;
            if (index > InventoryController.instance.player_inventory.slots.Count - 1)
                index = 0;
        }
        else if (scroll.y < 0)
        {
            index--;
            if (index < 0)
                index = InventoryController.instance.player_inventory.slots.Count - 1;
        }
        SetHandlePrefab();
        SelectedSlotUI();

        // Scroll yapınca UI'ı göster ve sayacı başlat
    }

    void OnDropItem()
    {
        InventoryController.instance.DropItem(index);
    }

    void SelectedSlotUI()
    {
        if (InventoryController.instance == null || InventoryController.instance.T_slots == null) return;

        for (int i = 0; i < 4; i++)
        {
            InventoryController.instance.T_slots[i].GetComponent<RawImage>().texture = oldTexture;
        }
        InventoryController.instance.T_slots[index].GetComponent<RawImage>().texture = newTexture;
    }

    #endregion

    #region UI Timer Logic (Yeni Eklenen Kısım)

    private void ShowUIWithTimer()
    {
        // 1. UI'ı aktif et
        if (ui_parent != null)
        {
            ui_parent.gameObject.SetActive(true);
        }

        // 2. Eğer çalışan bir sayaç varsa durdur (Süreyi başa sarmak için)
        if (uiHideCoroutine != null)
        {
            StopCoroutine(uiHideCoroutine);
        }

        // 3. Yeni bir 4 saniyelik sayaç başlat
        uiHideCoroutine = StartCoroutine(HideUIAfterDelay());
    }

    private IEnumerator HideUIAfterDelay()
    {
        // 4 saniye bekle
        yield return new WaitForSeconds(4f);

        // Süre dolunca UI'ı kapat
        if (ui_parent != null)
        {
            ui_parent.gameObject.SetActive(false);
        }

        uiHideCoroutine = null;
    }

    #endregion
}