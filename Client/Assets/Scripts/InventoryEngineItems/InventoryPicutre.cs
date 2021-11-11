
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using MoreMountains.InventoryEngine;
using MoreMountains.CorgiEngine;

public class InventoryPicutre : InventoryItem 
{
  public new bool Usable = true;
  public override bool Use()
  {
    base.Use();
    return true;
  }
}