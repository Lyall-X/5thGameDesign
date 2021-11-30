using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using MoreMountains.Tools;
using UnityEngine;

public class PropMgr : MMSingleton<PropMgr>
{
    private List<Prop> propsList;
    private List<GameObject> itemList;
    public GameObject PicObj = null;

    protected override void Awake()
    {
        base.Awake();
        propsList = new List<Prop>();
        itemList = new List<GameObject>();
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

    public GameObject CheckProposBeFound(Vector3 targetPos,float wight,float hight,Rect rect)
    {
        List<Prop> propIdList = new List<Prop>();
        for (int i = 0; i < propsList.Count; i++)
        {
            if (propsList[i].CheckBeFound(targetPos,wight,hight,rect))
            {
                propIdList.Add(propsList[i]);
                break;
            }
        }
        if (propIdList.Count > 0)
        {
            GameObject parent = new GameObject(itemList.Count.ToString());
            parent.transform.position = new Vector3(-9999,-9999,0);
            foreach(Prop item in propIdList)
            {
              GameObject obj = GameObject.Instantiate(item.gameObject);
              obj.gameObject.transform.parent = parent.transform;
              obj.gameObject.transform.localPosition = Vector3.zero;
              break;
            }
            itemList.Add(parent);
            return parent;
        }
        
        return null;
    }
}
