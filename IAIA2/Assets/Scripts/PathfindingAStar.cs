using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingAStar : MonoBehaviour
{
    private Transform targetPosition;

    public Grid grid;
    private float minDistanceToWaypoint = 25f;
    private float currentDistanceToWaypoint = 0f;

    private void Awake()
    {
        //grid = GetComponent<Grid>();
    }

    public void Pathfinding(Nodo initialPos, Nodo finalPos, ref List<Nodo> path)
    {
        FindPath(initialPos, finalPos,  ref path);
    }

    void FindPath(Nodo a_StartPos, Nodo a_TargetPos, ref List<Nodo> path)
    {
        Nodo startNode = a_StartPos;
        Nodo targetNode = a_TargetPos;

        List<Nodo> OpenList = new List<Nodo>();
        HashSet<Nodo> ClosedList = new HashSet<Nodo>();

        OpenList.Add(startNode);

        while (OpenList.Count > 0)
        {
            Nodo CurrentNode = OpenList[0];

            for (int i = 1; i < OpenList.Count; i++)
                if (OpenList[i].FCost < CurrentNode.FCost || OpenList[i].FCost == CurrentNode.FCost && OpenList[i].hCost < CurrentNode.hCost)
                    CurrentNode = OpenList[i];

            OpenList.Remove(CurrentNode);
            ClosedList.Add(CurrentNode);

            if (CurrentNode == targetNode)
                GetFinalPath(startNode, targetNode, ref path);
                    
            foreach (Nodo NeighborNode in grid.GetNeighbouringNodes(CurrentNode))
            {
                if (NeighborNode.IsWall || ClosedList.Contains(NeighborNode))
                    continue;
                float MoveCost = CurrentNode.gCost + NeighborNode.influencia + GetDistance(CurrentNode, NeighborNode);

                if (MoveCost < NeighborNode.gCost || !OpenList.Contains(NeighborNode))
                {
                    NeighborNode.gCost = MoveCost;
                    NeighborNode.hCost = GetDistance(NeighborNode, targetNode);
                    NeighborNode.Parent = CurrentNode;

                    if (!OpenList.Contains(NeighborNode))
                    {
                        OpenList.Add(NeighborNode);
                    }
                }
            }
        }
    }

    void GetFinalPath(Nodo a_StartingNode, Nodo a_EndNode, ref List<Nodo> path)
    {
        List<Nodo> FinalPath = new List<Nodo>();
        Nodo CurrentNode = a_EndNode;

        while (CurrentNode != a_StartingNode)
        {
            FinalPath.Add(CurrentNode);
            CurrentNode = CurrentNode.Parent;
        }

        FinalPath.Reverse();

        foreach (Nodo nodo in FinalPath)
            nodo.visited = false;

        path = FinalPath;
    }

    int GetDistance(Nodo a_nodeA, Nodo a_nodeB)
    {
        int x = Mathf.Abs(a_nodeA.gridX - a_nodeB.gridX);
        int y = Mathf.Abs(a_nodeA.gridY - a_nodeB.gridY);

        return x + y;
    }

    void FindPathPlayer(Nodo a_StartPos, Nodo a_TargetPos, ref List<Nodo> path)
    {
        Nodo startNode = a_StartPos;
        Nodo targetNode = a_TargetPos;

        List<Nodo> OpenList = new List<Nodo>();
        HashSet<Nodo> ClosedList = new HashSet<Nodo>();

        OpenList.Add(startNode);

        while (OpenList.Count > 0)
        {
            Nodo CurrentNode = OpenList[0];

            for (int i = 1; i < OpenList.Count; i++)
                if (OpenList[i].FCost < CurrentNode.FCost || OpenList[i].FCost == CurrentNode.FCost && OpenList[i].hCost < CurrentNode.hCost)
                    CurrentNode = OpenList[i];

            OpenList.Remove(CurrentNode);
            ClosedList.Add(CurrentNode);

            if (CurrentNode == targetNode)
                GetFinalPath(startNode, targetNode, ref path);

            foreach (Nodo NeighborNode in grid.GetNeighbouringNodes(CurrentNode))
            {
                if (NeighborNode.IsWall || ClosedList.Contains(NeighborNode))
                    continue;
                float MoveCost = CurrentNode.gCost + GetDistance(CurrentNode, NeighborNode);

                if (MoveCost < NeighborNode.gCost || !OpenList.Contains(NeighborNode))
                {
                    NeighborNode.gCost = MoveCost;
                    NeighborNode.hCost = GetDistance(NeighborNode, targetNode);
                    NeighborNode.Parent = CurrentNode;

                    if (!OpenList.Contains(NeighborNode))
                    {
                        OpenList.Add(NeighborNode);
                    }
                }
            }
        }
    }

    public void PathfindingPlayer(Nodo initialPos, Nodo finalPos, ref List<Nodo> path)
    {
        FindPathPlayer(initialPos, finalPos, ref path);
    }
}
