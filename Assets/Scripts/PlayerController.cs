using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject twoStepQuad;
    [SerializeField] private GameObject oneStepQuad;
    [SerializeField] public GameObject intersectionVFX;

    private GameCubeNode hoveredNode = new GameCubeNode();

    //private bool CanNodeBeReachead(GameCubeNode node) //, List<GameCubeNode> reachableNodes
    //{
    //    foreach (GameCubeNode reachableNode in reachableNodes)
    //    {
    //        if (reachableNode == node)
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    public GameCubeNode HandleMouseInput(CameraController playerCamera, MapController mapController, int actionsAmount)
    {
        hoveredNode = null;

        Ray ray = playerCamera.GetGameCamera().ScreenPointToRay(Input.mousePosition);
        // Vector3 intersectPos = Vector3.zero;


        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (mapController.IntersectCube(ray, out hoveredNode))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    return hoveredNode;
                }
            }


        }

        if (hoveredNode != null)
        {
            intersectionVFX.transform.position = hoveredNode.transform.position;
        }

        return null;
    }

    public void RenderWalkableNodes(bool isMoving, MapController mapController, int actionsAmount, List<GameCubeNode> reachableNodes)
    {
        if (!isMoving)
        {
            List<Matrix4x4> matricesTwoStep = new List<Matrix4x4>();
            List<Matrix4x4> matricesOneStep = new List<Matrix4x4>();
            foreach (GameCubeNode node in reachableNodes)
            {
                if (node.reachableValue > 7)
                {
                    Vector3 pos = node.transform.position;
                    pos.y += 1.2f;
                    matricesTwoStep.Add(Matrix4x4.TRS(pos, twoStepQuad.transform.rotation, twoStepQuad.transform.lossyScale));
                }
                else
                {
                    Vector3 pos = node.transform.position;
                    pos.y += 1.2f;
                    matricesOneStep.Add(Matrix4x4.TRS(pos, oneStepQuad.transform.rotation, oneStepQuad.transform.lossyScale));
                }
            }
            if (actionsAmount > 1)
            {
                Mesh meshTwoStep = twoStepQuad.GetComponent<MeshFilter>().mesh;
                Material matTwoStep = twoStepQuad.GetComponent<Renderer>().material;
                Graphics.DrawMeshInstanced(meshTwoStep, 0, matTwoStep, matricesTwoStep);
            }
            if (actionsAmount > 0)
            {
                Mesh meshOneStep = oneStepQuad.GetComponent<MeshFilter>().mesh;
                Material matOneStep = oneStepQuad.GetComponent<Renderer>().material;
                Graphics.DrawMeshInstanced(meshOneStep, 0, matOneStep, matricesOneStep);
            }
        }
    }
}
