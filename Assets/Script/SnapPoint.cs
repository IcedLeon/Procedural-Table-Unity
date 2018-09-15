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

public class SnapPoint : MonoBehaviour
{
    public BoundingBox ParentBB;
    public Material SnapPointMaterial, SelectedMaterial;

    private Vector3 _initialPos;
    private bool _snapClicked = false;
    private GameObject _distancePlane;
    private const float _distanceThreshold = 0.75f;
    private float _snapPointToCenter;
    private Renderer _rendererComponent;

    private void Start()
    {
        //the following vector is used to understand whether moving  
        _snapPointToCenter = Vector3.Distance(transform.position, ParentBB.transform.position);

        //materials are hardcoded here, better if a prefab is instantiated
        SnapPointMaterial = Resources.Load("Material/SemiTransparent", typeof(Material)) as Material;
        SelectedMaterial = Resources.Load("Material/SemiTransparentSelected", typeof(Material)) as Material;

        if(GetComponent<Renderer>() != null)
            _rendererComponent = GetComponent<Renderer>();

        SetDefaultMaterial();

        _initialPos = transform.position;
        _distancePlane = CreateMeasuringPlane();
        SetActiveMeasuringPlane(false);
    }

    #region Material_Functions
    private void SetDefaultMaterial()
    {
        if (_rendererComponent != null && SnapPointMaterial != null)
            _rendererComponent.material = SnapPointMaterial;
    }

    private void SetSelectedMaterial()
    {
        if (_rendererComponent != null && SelectedMaterial != null)
            _rendererComponent.material = SelectedMaterial;
    }
    #endregion

    private GameObject CreateMeasuringPlane()
    {
        GameObject p = GameObject.CreatePrimitive(PrimitiveType.Plane);
        p.transform.parent = gameObject.transform;
        p.transform.position = gameObject.transform.position;
        p.GetComponent<Renderer>().enabled = false;
        return p;
    }
    private void SetActiveMeasuringPlane(bool status)
    {
        if (_distancePlane != null)
            _distancePlane.SetActive(status);
    }

    #region MouseInteractions
    private void OnMouseDown()
    {
        _snapClicked = true;
        SetActiveMeasuringPlane(true);
        GetComponent<Collider>().enabled = false;
        SetSelectedMaterial();
    }

    private void OnMouseDrag()
    {
        //create a ray from mouse click pos
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.Equals(_distancePlane))
            {
                //since we want to expand the table along the XZ plane only, let's understand which axe are we targetting
                float distX = _initialPos.x - hit.point.x;
                float distZ = _initialPos.z - hit.point.z;

                float MouseToCenter = Vector3.Distance(hit.point, ParentBB.transform.position);

                //this should get the raw direction of scaling
                Vector3 ScalingDirection = (hit.point - transform.position)/2;

                float UnitDim = ParentBB.ObjectToWrap.GetComponent<FornitureController>().GetUnitDim();

                if (Math.Abs(distX) >= Math.Abs(distZ))
                {//we are moving along the x axis
                    if (Math.Abs(distX) > _distanceThreshold)
                    {
                        Vector3 OffsetAlongX = new Vector3(ScalingDirection.x > 0 ? UnitDim / 2 : -UnitDim / 2, 0f, 0f);

                        if (MouseToCenter > _snapPointToCenter)
                            ParentBB.NotifyChange(new Vector2(1f, 0f), OffsetAlongX);
                        else
                            ParentBB.NotifyChange(new Vector2(-1f, 0f), OffsetAlongX);
                    }
                }
                else
                {
                    if (Math.Abs(distZ) > _distanceThreshold)
                    {
                        Vector3 OffsetAlongZ = new Vector3(0f, 0f, ScalingDirection.z > 0 ? UnitDim / 2 : -UnitDim / 2);

                        if (MouseToCenter > _snapPointToCenter)
                            ParentBB.NotifyChange(new Vector2(0f, 1f), OffsetAlongZ);
                        else
                            ParentBB.NotifyChange(new Vector2(0f, -1f), OffsetAlongZ);
                    }
                }
            }
        }
    }

    private void OnMouseUp()
    {
        SetActiveMeasuringPlane(false);
        _snapClicked = false;
        GetComponent<Collider>().enabled = true;
        SetDefaultMaterial();
    }
    #endregion

}