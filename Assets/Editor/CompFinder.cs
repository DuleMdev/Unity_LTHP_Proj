using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CompFinder : EditorWindow {

	static public List<string> result; // A keresés eredménye
	static public List<string> result2; // A keresés eredménye
	static public List<GameObject> result2GameObject;
	static public string selectedComponentName; // Milyen nevű komponenseket kell kilistázni

	Vector2 scroolPos1;
	Vector2 scroolPos2;
	int width = 0;
	bool showUnityEngine;

	[MenuItem ("Tools/CompFinder")]
	public static void  ShowWindow () {
		EditorWindow.GetWindow(typeof(CompFinder));
	}

	List<GameObject> GetGameObject() {
		GameObject[] gameObjectArray = GameObject.FindObjectsOfType<GameObject>();

		List<GameObject> gameObjectList = new List<GameObject> ();

		foreach (GameObject item in gameObjectArray) {
			if (item.transform.parent == null)
				gameObjectList.AddRange(GetGameObjectAndChild(item));
		}

		return gameObjectList;
	}

	// A metódus egy gameObject-hez csatolt összes gyerek GameObject-ját visszaadja
	List<GameObject> GetGameObjectAndChild(GameObject go) {
		List<GameObject> gameObjectList = new List<GameObject> ();
		Transform[] transforms = go.GetComponentsInChildren<Transform> (true);

		foreach (Transform item in transforms) {
			gameObjectList.Add(item.gameObject);	
		}

		return gameObjectList;
	}

	// Kigyűjtjük a színen található componenseket
	void CollectAllComponentsName() {
		// Ez sajnos csak az engedélyezett objektumokat adja vissza
		//GameObject[] gameObjectArray = GameObject.FindObjectsOfType<GameObject>();
		List<GameObject> gameObjectArray = GetGameObject();

		// result.Clear();
		result = new List<string> ();
        foreach (GameObject gameObject in gameObjectArray)
            foreach (Component item in gameObject.GetComponents<Component>())
            {
                if (item != null)
                {
                    string componentName = item.GetType().ToString();

                    if (!showUnityEngine && componentName.StartsWith("UnityEngine.")) continue;

                    if (!result.Contains(componentName))
                        result.Add(componentName);
                }
            }

		result.Sort ();
	}

	// A megadott gameObject-nek az elhelyezkedését adja vissza
	public static string GetGameObjectHierarchy(GameObject go) {
		string path = "";
		Transform transform = go.transform;
		
		do {
			path = "/" + transform.name + path;
			transform = transform.parent;
		} while (transform != null);
		
		return path;
	}

	void CollectComponentsByName(string name) {
		//GameObject[] gameObjectArray = GameObject.FindObjectsOfType<GameObject>();
		List<GameObject> gameObjectArray = GetGameObject();

		result2 = new List<string>();
		result2GameObject = new List<GameObject> ();
		foreach (GameObject gameObject in gameObjectArray)
		foreach (Component item in gameObject.GetComponents<Component>()) {
			if (item != null && item.GetType().ToString() == name) {
				string hierarchy = GetGameObjectHierarchy(gameObject);
				int pos = InsertPos (hierarchy);

				result2.Insert (pos, hierarchy);
				result2GameObject.Insert (pos, gameObject);
			}
		}
	}

	int InsertPos(string s) {
		int pos = 0;

		while (pos < result2.Count) {
			if (string.Compare(result2[pos], s) < 1 )
				pos++;
			else 
				break;
		}

		return pos;
	}

	void OnProjectChange() {
		Debug.Log("OnProjectChange");
	}
	
	// Ha a Hierarhiába megváltoztatják a kiválasztott gameObject-et, akkor frissítjük a tartalmat
	void OnSelectionChange() {
		//Debug.Log("OnSelectionChange");
		Repaint();
	}

	void OnGUI () {
		CollectAllComponentsName ();

		GUIStyle alignRight = new GUIStyle (GUI.skin.label);
		alignRight.alignment = TextAnchor.UpperRight;
		GUIStyle whiteText = new GUIStyle (GUI.skin.label);
		whiteText.normal.textColor = Color.white;
		whiteText.fontStyle = FontStyle.Bold;

		Rect r = EditorGUILayout.BeginHorizontal (); {

			if (r.size.x > 0)
				width = (int)(r.size.x / 2);
			//Debug.Log ("Width : " + width.ToString ());

			scroolPos1 = EditorGUILayout.BeginScrollView (scroolPos1, "box", GUILayout.MaxWidth (width)); {
				GUILayout.BeginHorizontal (); {

					GUILayout.BeginVertical (GUILayout.MaxWidth(1)); 
					for (int i = 0; i < result.Count; i++) {
						GUILayout.Label ((i + 1).ToString() + ".", alignRight, GUILayout.ExpandWidth (true));	
					}
					GUILayout.EndVertical ();
	
					GUILayout.BeginVertical (GUILayout.MaxWidth(1000)); 
					foreach (var item in result) {
						if (GUILayout.Button (item, (selectedComponentName == item) ? whiteText : "label")) {
							selectedComponentName = item;
						}
					}
					GUILayout.EndVertical ();
				}
				GUILayout.EndHorizontal ();
			}
			EditorGUILayout.EndScrollView ();

			if (!result.Contains(selectedComponentName))
				selectedComponentName = "";

			CollectComponentsByName (selectedComponentName);

			scroolPos2 = EditorGUILayout.BeginScrollView (scroolPos2, "box", GUILayout.MaxWidth (width)); {
				GUILayout.BeginHorizontal (); {
		
					GUILayout.BeginVertical (GUILayout.MaxWidth(1)); 
					for (int i = 0; i < result2.Count; i++) {
						GUILayout.Label ((i + 1).ToString () + ".", alignRight, GUILayout.ExpandWidth (true));	
					}
					GUILayout.EndVertical ();


					GUILayout.BeginVertical (GUILayout.MaxWidth(1000));
					for (int i = 0; i < result2.Count; i++) {
						if (GUILayout.Button (result2 [i], (Selection.activeObject == result2GameObject[i]) ? whiteText : "label")) {
							Selection.activeObject = result2GameObject[i];
						}
					}
					GUILayout.EndVertical ();
				}
				GUILayout.EndHorizontal ();
			}
			EditorGUILayout.EndScrollView ();

		}
		EditorGUILayout.EndHorizontal ();

		// Jobb Click egy üres területen és felbukkanó menü lesz az eredménye
		Event currentEvent = Event.current;
		//if (Input.GetMouseButtonDown(0))
		if (currentEvent.type == EventType.ContextClick)
		{
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Show UnityEngine"), showUnityEngine, (object o) => {
				showUnityEngine = !showUnityEngine;
			}, "");
			menu.ShowAsContext();
			currentEvent.Use();
		}
	}
}
