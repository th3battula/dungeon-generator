using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ObjectPool {
    private GameObject pooledObjectPrefab;
    private Transform parent;
    private int sizeOfPool;
    private bool canExpand, startEnabled;

    [HideInInspector]
    Dictionary<string, PooledObject> poolDict;

    public ObjectPool (GameObject pooledObject, int size, bool canExpand = true, bool startEnabled = true, Transform parent = null) {
        this.parent = parent;
        this.pooledObjectPrefab = pooledObject;
        this.sizeOfPool = size;
        this.canExpand = canExpand;
        this.startEnabled = startEnabled;

        InitializePool();
    }

    void InitializePool () {
        if (this.poolDict != null) this.poolDict.Clear();
        this.poolDict = new Dictionary<string, PooledObject>();

        for (int i = 0; i < this.sizeOfPool; i++) {
            GameObject obj = GameObject.Instantiate (this.pooledObjectPrefab);
            if (this.parent != null) {
                obj.transform.SetParent(this.parent);
            }
            obj.name = i.ToString();
            obj.SetActive(startEnabled);
            string id = System.Guid.NewGuid().ToString();
            PooledObject pooledObj = new PooledObject(id, obj);
            this.poolDict.Add(id, pooledObj);
        }

    }

    public void ClearPool () {
        foreach (PooledObject obj in this.poolDict.Values) {
            Object.Destroy(obj.gameObject);
        }
        this.poolDict.Clear();
    }

    public void DisableAll () {
        foreach (PooledObject pooledObj in this.poolDict.Values) {
            GameObject obj = pooledObj.gameObject;
            if (obj.activeInHierarchy) {
                obj.SetActive(false);
            }
        }
    }

    public void Disable (string id) {
        PooledObject obj;
        this.poolDict.TryGetValue(id, out obj);

        if (obj != null) {
            obj.SetActive(false);
        }
    }

    public void Disable (List<GameObject> objs) {
        foreach (GameObject obj in objs) {
            if (obj.activeInHierarchy) {
                obj.SetActive(false);
            }
        }
    }

    public void Disable (GameObject obj) {
        obj.SetActive(false);
    }

    public bool RemoveFromPool (GameObject obj, bool shouldDestroy = true) {
        KeyValuePair<string, PooledObject> kvp = this.poolDict.Where(z => z.Value.gameObject == obj).First();
        if (kvp.Key != null) {
            this.poolDict.Remove(kvp.Key);
            if (shouldDestroy) Object.Destroy(obj);
            this.sizeOfPool--;
            return true;
        }
        return false;
    }

    public bool RemoveFromPool (string id, bool shouldDestroy = true) {
        if (this.poolDict.ContainsKey(id)) {
            PooledObject pooledObj;
            this.poolDict.TryGetValue(id, out pooledObj);
            this.poolDict.Remove(id);
            if (shouldDestroy && pooledObj != null) {
                Object.Destroy(pooledObj.gameObject);
            }
            this.sizeOfPool--;
            return true;
        }
        return false;
    }

    public PooledObject AddObjectToPool () {
        if (canExpand) {
            string id = System.Guid.NewGuid().ToString();
            GameObject obj = GameObject.Instantiate (this.pooledObjectPrefab);
            if (this.parent != null) {
                obj.transform.SetParent(this.parent);
            }
            obj.SetActive(startEnabled);
            PooledObject pooledObj = new PooledObject(id, obj);
            this.poolDict.Add(System.Guid.NewGuid().ToString(), pooledObj);
            this.sizeOfPool++;
            return pooledObj;
        }
        return null;
    }

    public PooledObject GetObjectFromPoolByName (string name) {
        KeyValuePair<string, PooledObject> kvp = this.poolDict.Where(z => z.Value.gameObject.name == name).FirstOrDefault();
        if (kvp.Key != null) {
            return kvp.Value;
        }
        return null;
    }

    public PooledObject GetObjectFromPoolById (string id) {
        KeyValuePair<string, PooledObject> kvp = this.poolDict.Where(z => z.Value.id == id).FirstOrDefault();
        if (kvp.Key != null) {
            return kvp.Value;
        }
        return null;
    }

    public PooledObject GetObjectFromPoolByPosition (Vector3 position) {
        KeyValuePair<string, PooledObject> kvp = this.poolDict.Where(z => z.Value.gameObject.transform.position == position).FirstOrDefault();
        if (kvp.Key != null) {
            return kvp.Value;
        }
        return null;
    }

    public PooledObject this[string id] {
        get {
            PooledObject val;
            this.poolDict.TryGetValue(id, out val);

            return val;
        }
    }

    public PooledObject GetObjectFromPool {
        get {
            IEnumerable<KeyValuePair<string, PooledObject>> enumerableKvp = this.poolDict.Where(z => !z.Value.gameObject.activeInHierarchy);
            KeyValuePair<string, PooledObject> kvp = enumerableKvp.FirstOrDefault();
            if (kvp.Key != null) {
                return kvp.Value;
            }

            if (this.canExpand) {
                return AddObjectToPool();
            }

            return null;
        }
    }

    public Dictionary<string, PooledObject> GetObjectPool {
        get {
            return this.poolDict;
        }
    }

    public bool CanExpand {
        get {
            return this.canExpand;
        }

        set {
            this.canExpand = value;
        }
    }

    public bool StartEnabled {
        get {
            return this.startEnabled;
        }

        set {
            this.startEnabled = value;
        }
    }

    [System.Serializable]
    public class PooledObject {
        [SerializeField] string _id;
        GameObject _gameObject;

        public PooledObject (string id, GameObject obj) {
            _id = id;
            _gameObject = obj;
        }

        public void SetActive(bool active) {
            _gameObject.SetActive(active);
        }

        public override string ToString() {
            return JsonUtility.ToJson(this);
        }

        public string id {
            get {
                return _id;
            }
        }

        public bool activeInHierarchy {
            get {
                return _gameObject.activeInHierarchy;
            }
        }

        public GameObject gameObject {
            get {
                return _gameObject;
            }
        }

        public string name {
            get {
                return _gameObject.name;
            }

            set {
                _gameObject.name = value;
            }
        }

        public Vector3 position {
            get {
                return _gameObject.transform.position;
            }

            set {
                _gameObject.transform.position = value;
            }
        }

        public Vector3 scale {
            get {
                return _gameObject.transform.localScale;
            }

            set {
                _gameObject.transform.localScale = value;
            }
        }

        public Transform transform {
            get {
                return _gameObject.transform;
            }
        }
    }
}