
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using MoreMountains.InventoryEngine;
using MoreMountains.CorgiEngine;

public class InventoryPicutre : InventoryItem 
{
  public override bool IsUsable {  get { return true;  } }
  public override bool Use()
  {
    base.Use();
    this.Drop();
    GameObject InventoryCanvas = GameObject.Find("/UICamera/InventoryCanvas");
    InventoryInputManager script = InventoryCanvas.GetComponent<InventoryInputManager>();
    script.ToggleInventory();
    PropMgr.Instance.PicObj = GameObject.Find(ItemID);
    return true;
  }
}