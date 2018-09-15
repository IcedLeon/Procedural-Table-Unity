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

/*
 * This should be inherited from a superclass
 * or using some kind of interface, 
 * designed to define "Forniture" 
 * or any proper super class
 */
public class TableController : FornitureController
{
    public GameObject TableGameObject;
    public GameObject Chair, TableLeg;
    public bool ScaleKeepingCenter = true;
    public int xDim = 1;
    public int zDim = 1;

    private float UnitDim = 1f; //this is used for offsets
    private BoundingBox _boundingBox;
    private readonly int LegsNumber = 4;


    //here we are considering two kind of features: fixed and variable number of fashion elements
    private GameObject[] TableLegs;     //here we are supposing to build a table with a number of fixed legs (4)
    private List<GameObject> TableChairs;   //here we are supposing to have a number of chairs proportional to the table dimensions

    // Use this for initialization
    void Start()
    {
        SaveUnitsDimensions();

        if (Chair == null || TableLeg == null)
        {
            Debug.Log("Can't build the table: missing" +
                ((Chair == null) ? " Chair" : "") +
                ((TableLeg == null) ? " TableLeg" : ""
                + " in script vars"));
            return;
        }

        // build the table
        CreateBoundingBox();
        Build();
    }

    private void CreateBoundingBox()
    {
        if (_boundingBox == null)
            _boundingBox = TableGameObject.gameObject.GetComponent<BoundingBox>();
        else
        {
            _boundingBox.DestroyBoundingBox();
            _boundingBox.NewBoundingBox();
        }
    }

    #region Superclass Methods

    public override void Build()
    {
        if (Chair != null || TableLeg != null)
        {
            if (xDim >= 1 && zDim >= 1)
            {
                //getting meshes dimensions
                Bounds b = new Bounds(gameObject.transform.position, Vector3.zero);
                b.Encapsulate(TableGameObject.GetComponent<Renderer>().bounds);

                /*
                //if children have to be considered
                //in this case, the only table top is enough
                foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>())
                {
                    b.Encapsulate(r.bounds);
                }*/

                CreateLegs(b);
                SpawnChairs(b);
            }
        }
    }

    public override void Demolish()
    {
        //clear previous res
        if (TableLegs != null && TableChairs != null && _boundingBox != null)
        {
            //Deleting legs
            foreach (GameObject leg in TableLegs)
            {
                Destroy(leg);
            }

            //Deleting chairs
            foreach (GameObject chair in TableChairs)
            {
                Destroy(chair);
            }
            TableChairs.Clear();

            //Deleting boundingBox
            _boundingBox.DestroyBoundingBox();
        }
        else
        {
            Debug.LogError("Can't delete previous accessories");
        }
    }

    public override void UpdateDimensions(Vector2 AlongAxis, Vector3 ScalingOffset)
    {
        Vector3 StartingScale = transform.localScale;
        //this if is require to understand if a change occurred or not
        if (AlongAxis.x != 0 || AlongAxis.y != 0)
        {
            if (AlongAxis.x > 0)
            {
                transform.localScale += new Vector3(1f, 0, 0);
            }
            else if (AlongAxis.y > 0)
            {
                transform.localScale += new Vector3(0, 0, 1f);
            }
            else if (AlongAxis.x < 0 && StartingScale.x > 1)
            {
                transform.localScale -= new Vector3(1f, 0, 0);
            }
            else if (AlongAxis.y < 0 && StartingScale.z > 1)
            {
                transform.localScale -= new Vector3(0, 0, 1f);
            }

            //if click didn't change the object, nothing to do
            if (StartingScale == transform.localScale)
                return;

            if (!ScaleKeepingCenter)
            {
                transform.position += ScalingOffset;
            }

            Demolish();

            //else
            SaveUnitsDimensions();
            print("New dimensions are xDim: " + xDim + ", zDim" + zDim);

            //reinstance the table and fix BoundingBox
            CreateBoundingBox();
            Build();

        }
    }

    private void SaveUnitsDimensions()
    {
        xDim = (int)transform.localScale.x;
        zDim = (int)transform.localScale.z;
    }

    #endregion

    private void SpawnChairs(Bounds b)
    {
        if (Chair == null)
            return;

        TableChairs = new List<GameObject>();

        for (int i = 0; i < xDim; i++)
        {
            float ChairPosX = b.center.x - b.extents.x + (i * UnitDim) + UnitDim/2;
            float ChairPosZ1 = b.center.z + b.extents.z;
            float ChairPosZ2 = b.center.z - b.extents.z;

            NewChair(new Vector3(ChairPosX, 0f, ChairPosZ1), new Vector3(0f, 180f, 0));
            NewChair(new Vector3(ChairPosX, 0f, ChairPosZ2), Vector3.zero);
        }

        for (int i = 0; i < zDim; i++)
        {
            float ChairPosZ = b.center.z - b.extents.z + i + UnitDim/2;
            float ChairPosX1 = b.center.x + b.extents.x;
            float ChairPosX2 = b.center.x - b.extents.x;

            NewChair(new Vector3(ChairPosX1, 0f, ChairPosZ), new Vector3(0f, -90f, 0));
            NewChair(new Vector3(ChairPosX2, 0f, ChairPosZ), new Vector3(0f, 90f, 0));
        }


    }
    private void NewChair(Vector3 chairPos, Vector3 angle)
    {
        GameObject newChair = Instantiate(Chair, chairPos, Quaternion.Euler(angle));

        TableChairs.Add(newChair);
        newChair.transform.parent = gameObject.transform;
        newChair.name = "Chair_" + TableChairs.Count;
    }
    private void CreateLegs(Bounds b)
    {
        if (TableLeg == null)
            return;
        Vector2[] VersorPosition = { new Vector2(1f, 1f), new Vector2(1f, -1f), new Vector2(-1f, 1f), new Vector2(-1f, -1f) };

        // compute ChairsNumber
        float offsetX = 0.053f;
        float offsetZ = 0.053f;

        TableLegs = new GameObject[LegsNumber];

        float LegPosX = b.center.x + b.extents.x - offsetX;
        float LegPosZ = b.center.z + b.extents.z - offsetZ;

        //find legs position
        for (int i = 0; i < LegsNumber && i < VersorPosition.Length; i++)
        {
            Vector3 versorPos = VersorPosition[i];

            Debug.Log("b.center.x " + b.center.x + " b.extents.x " + b.extents.x + " versorPos.x " + versorPos.x);
            //position on XZ plane is calculated depending on variables dimensions of table
            float xPos = b.center.x + (b.extents.x - offsetX) * versorPos.x;
            float ZPos = b.center.z + (b.extents.z - offsetZ) * versorPos.y;
            //vertical positioning is imposed by prefab
            float YPos = TableLeg.transform.position.y;

            GameObject newLeg = Instantiate(TableLeg, new Vector3(xPos, YPos, ZPos), Quaternion.identity);
            newLeg.transform.parent = gameObject.transform;
            TableLegs[i] = newLeg;
        }
    }

    public override float GetUnitDim()
    {
        return UnitDim;
    }
}
