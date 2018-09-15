using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * README
 * Every computation is here done in world coordinates. 
 * In order to correctly compute transformations, scaling operation indepentenly from scale,
 * position and rotation, every geometric calculation should be translated to LOCAL positions, not WORLD.
 */
public class BoundingBox : MonoBehaviour {

    public GameObject ObjectToWrap;
    public bool BoundChildren = false;

    private bool _isVisible = true;
    private GameObject _boundingBoxGO;
    private List<GameObject> SnapPointList;
    private Bounds _goBound;
    private Renderer rend;
    private Vector3[] SnapsPos = {  new Vector3(1f, 1f, 1f),
                                    new Vector3(1f, 1f, -1f),
                                    new Vector3(-1f, 1f, -1f),
                                    new Vector3(-1f, 1f, 1f),
                                    new Vector3(1f, -1f, 1f),
                                    new Vector3(1f, -1f, -1f),
                                    new Vector3(-1f, -1f, 1f),
                                    new Vector3(-1f, -1f, -1f)};    //this defines an unitary cube used as multi versor collection

    Bounds NewWrappingBounds()
    {
        Bounds b = new Bounds(gameObject.transform.position, Vector3.zero);

        if(rend != null)
            b.Encapsulate(rend.bounds);

        if (BoundChildren)
            foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>())
            {
                b.Encapsulate(r.bounds);
            }

        return b;
    }

    void ComputeSnapPoints()
    {
        int SnappingPoints = SnapsPos.Length;
        CreateSnapList();

        for (int i = 0; i < SnappingPoints; i++)
        {
            //creating the snapping point
            GameObject snappingCube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            //give it the snap script
            snappingCube.name = "Snap_Handle_" + i;
            snappingCube.AddComponent<SnapPoint>().ParentBB = this;

            //scaling
            snappingCube.transform.localScale = 0.1f * Vector3.one;

            //repositioning
            Vector3 versor = SnapsPos[i % SnapsPos.Length];

            //this is required to spawn snap points onto the floor for negative Y components
            float yPos = (_goBound.center.y + _goBound.center.y * versor.y ) /2;
            if (yPos < 0)
                yPos = 0;
            
            snappingCube.transform.position = new Vector3(  _goBound.center.x + _goBound.extents.x * versor.x,
                                                            yPos,
                                                            _goBound.center.z + _goBound.extents.z * versor.z);
            
            //let this snap have a parent
            snappingCube.transform.parent = _boundingBoxGO.transform;

            SnapPointList.Add(snappingCube);
        }
    }

    private void CreateSnapList()
    {
        if (SnapPointList != null)
        {
            if(SnapPointList.Count>0)
                foreach (GameObject sp in SnapPointList)
                {
                    Destroy(sp);
                }
        }
        SnapPointList = new List<GameObject>();
    }

    // Use this for initialization
    void Start () {
        if (ObjectToWrap == null)
            ObjectToWrap = gameObject.transform.parent.gameObject;

        rend = GetComponent<Renderer>();
        NewBoundingBox();
    }

    private void SwitchVisibilitySnappingPoints()
    {
        _isVisible = !_isVisible;

        foreach (GameObject sp in SnapPointList)
        {
            sp.SetActive(_isVisible);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log(!_isVisible ? "Showing BoundingBox" : "Hiding BoundingBox" );
            SwitchVisibilitySnappingPoints();
        }
    }

    public void NewBoundingBox()
    {
        _boundingBoxGO = new GameObject
        {
            name = "BoundingBox_" + gameObject.name
        };
        _boundingBoxGO.transform.parent = ObjectToWrap.transform;

        // must be hidded at start, call it by button click from menu gui
        CreateSnappingPoints();
        SwitchVisibilitySnappingPoints();
    }

    public void DestroyBoundingBox()
    {
        if(_boundingBoxGO!=null)
            Destroy(_boundingBoxGO);
    }

    public void CreateSnappingPoints()
    {
        _goBound = NewWrappingBounds();
        ComputeSnapPoints();
    }

    public void NotifyChange(Vector2 ScalingAxis, Vector3 ScalingDirection)
    {
        ObjectToWrap.GetComponent<TableController>().UpdateDimensions(ScalingAxis, ScalingDirection);
    }

}
