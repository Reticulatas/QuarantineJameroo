using UnityEngine;
using System.Collections;

public class BehaviourSingleton<T> : MonoBehaviour where T : class {

    public static T obj { get { return _obj as T; } }
    private static object _obj;

	// Use this for initialization
	public virtual void Awake()
	{
	    if (_obj != null)
	        Debug.LogError("Assigning singleton twice: " + typeof (T).Name);
        _obj = this;
	}
}
