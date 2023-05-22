/*
    Author: Ian Rodriguez, Justin Cardoso
    Script Description:
        [[[TO DO]]]
    
    TO DO:
        - include illegal placement check against currently existing occupied points (bitmaps)
        - script description
        - comment out Update() code once LLM can make placement requests
        - out of bounds request: inform user
        - change name of PlaceCube() so that it is appropriate to be used with other objects
*/
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CubePlacer : MonoBehaviour
{
    private Grid _grid;
    [SerializeField]
    private GameObject _block;
    private GameObject _objectContainer;
    [SerializeField]
    private bool _pointAndClickDebug = true;

    [SerializeField]
    private Material _baseBlockMat;

    // Start is called before the first frame update
    void Start()
    {
        _grid = GameObject.FindObjectOfType<Grid>();
        if(_grid == null)
        {
            Debug.LogError("CubePlacer::Start - Grid component is null");
        }
        _objectContainer = GameObject.FindGameObjectWithTag("ObjectContainer");
        if(_objectContainer == null)
        {
            Debug.LogError("CubePlacer::Start - Object Container is null");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //[[[for display purposes ONLY]]]
        if (_pointAndClickDebug)
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hitInfo;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hitInfo))
                {
                    Debug.Log("Ray hit something!");
                    PlaceObject(hitInfo.point);
                }
            }
        }
    }

    public void PlaceCube(ObjectData objData)
    {
        //checking if pos is illegal position
        if(!objData.success)
        {
            Debug.Log("OBJECT IS UNSUCCESSFUL.");
            //[[[AI ERROR MESSAGE]]]
            return;
        }
        if(objData.position.x > 49 || objData.position.x < 0 
            || objData.position.y > 49 || objData.position.y < 0
            || objData.position.z > 49 || objData.position.z < 0)
        {
            Debug.Log("OBJECT IS OUT OF BOUNDS.");
            //[[[AI ERROR MESSAGE]]]
            return;
        }
        //[[[INCLUDE IF-CHECK FOR TRYING TO PLACE AT OCCUPIED SPACE]]]

        //instantiate block
        GameObject block = GameObject.Instantiate(_block, objData.position, Quaternion.identity);

        //desired material
        Renderer newBlockRend = block.GetComponent<Renderer>();
        newBlockRend.material = new Material(_baseBlockMat);
        newBlockRend.material.color = objData.color;

        //desired position
        Vector3 adjustedPos = _grid.GetNearestPointOnGrid(objData.position);
        block.transform.position = adjustedPos;
        FindAndCreateParent(block);
    }

    private void PlaceObject(Vector3 pos)
    {
        //checking if pos is illegal position
        if (pos.x > 49 || pos.y > 49 || pos.z > 49) //out of bounds
        {
            Debug.LogError("CubePlacer::PlaceObject - this spot is out of bounds!");
            return;
        }

        //placing
        Vector3 adjustedPos = _grid.GetNearestPointOnGrid(pos);
        GameObject block = GameObject.Instantiate(_block, pos, Quaternion.identity);
        block.transform.position = adjustedPos;
        FindAndCreateParent(block);
    }

    private void FindAndCreateParent(GameObject obj)
    {
        GameObject parent;
        string parentName = obj.name + "s";

        parent = GameObject.Find(parentName);

        //creating parent if already does not exist
        if (parent == null)
        {
            parent = new GameObject(parentName);
            parent.transform.SetParent(_objectContainer.transform); //putting new parent under "Placed_Objects"
        }

        obj.transform.SetParent(parent.transform);
    }
}
