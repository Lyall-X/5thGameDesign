using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using MoreMountains.Tools;
using UnityEngine;

public class PropMgr : MMSingleton<PropMgr>
{
    private List<Prop> propsList;
    protected override void Awake()
    {
        base.Awake();
        Init();
    }
    public void Init()
    {
        propsList = FindObjectsOfType<Prop>().ToList();
        for (int i = 0; i < propsList.Count; i++)
        {
            //TODO 初始化道具的获取状态
            propsList[i].Init();
        }
    }

    // Update is called once per frame

    public List<int> CheckProposBeFound(Vector3 targetPos,float wight,float hight,Rect rect)
    {
        List<int> propIdList = new List<int>();
        for (int i = 0; i < propsList.Count; i++)
        {
            if (propsList[i].CheckBeFound(targetPos,wight,hight,rect))
            {
                propIdList.Add(propsList[i].id);
            }
        }
        
        return propIdList;
    }
}
