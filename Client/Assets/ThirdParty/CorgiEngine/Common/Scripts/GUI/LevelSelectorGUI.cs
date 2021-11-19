using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using MoreMountains.MMInterface;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Level selector GUI.
	/// </summary>
	public class LevelSelectorGUI : MonoBehaviour 
	{
		/// the panel object that contains the name of the level
		[Tooltip("the panel object that contains the name of the level")]
		public Image LevelNamePanel;
		/// the text where the level name should be displayed
		[Tooltip("the text where the level name should be displayed")]
		public Text LevelNameText;
		/// the offset to apply to the level name 
		[Tooltip("the offset to apply to the level name ")]
		public Vector2 LevelNameOffset;

    public GameObject map1;
    public GameObject map2;
    public GameObject map3;
    public GameObject mapselect;

		/// <summary>
		/// On start, disables the HUD and optionally the level name panels
		/// </summary>
		protected virtual void Start ()
	    {
	       GUIManager.Instance.SetHUDActive(false);

	       if (LevelNamePanel!=null && LevelNameText!=null)
			{
				LevelNamePanel.enabled=false;
				LevelNameText.enabled=false;
	       }
	    }

		/// <summary>
		/// Sets the name of the level to the one in parameters
		/// </summary>
		/// <param name="levelName">Level name.</param>
	    public virtual void SetLevelName(string levelName)
		{
			LevelNameText.text=levelName;
			LevelNamePanel.enabled=true;
			LevelNameText.enabled=true;
      if (levelName == "第一关：月球探索")
      {
        map1.SetActive(true);
        map2.SetActive(false);
        map3.SetActive(false);
      }else if (levelName == "第二关：无畏星球")
      {
        map1.SetActive(false);
        map2.SetActive(true);
        map3.SetActive(false);
      }
      else if (levelName == "第三关：隐藏的秘密")
      {
        map1.SetActive(false);
        map2.SetActive(false);
        map3.SetActive(true);
      }
		}

		/// <summary>
		/// Turns the name of the level off.
		/// </summary>
		public virtual void TurnOffLevelName()
		{
			LevelNamePanel.enabled=false;
			LevelNameText.enabled=false;
		}

    public virtual void ButtonPressed()
    {
      StartCoroutine (LoadFirstLevel ());
    }

    protected virtual IEnumerator LoadFirstLevel()
		{
			yield return new WaitForSeconds (1f);
			MMSceneLoadingManager.LoadScene ("StartScreen");
		}
	}


}