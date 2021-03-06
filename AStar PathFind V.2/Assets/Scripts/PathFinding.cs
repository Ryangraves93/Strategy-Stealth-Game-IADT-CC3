﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    public Transform player, target; //Player and target postion

    GridScript grid; //Grid reference
    int frameCount = 0;
    

    private void Awake()
    {
        grid = GetComponent<GridScript>(); //Assign grid as a reference to our gridscript class
    }

    public void Update()
    {
       
        if (Input.GetMouseButtonDown(0))
        {
            playerMove();
        }
        int pathSize = grid.path.Count;
        if (frameCount % 60 == 0)
        {
            frameCount = 0;
            if (pathSize != 0)
            {
                Node lastNode = grid.path[pathSize - 1];
                lastNode.worldPosition.y = player.position.y;
                player.position = lastNode.worldPosition;
                grid.path.Remove(lastNode);
            }
        }
        frameCount++;
    }

    //The PlayerMove() is used to convert a click on the screen into a grid on the map. It does this by using a raycast to see if the user has clicked on the screen, it then 
    //uses unity's ScreenPointToRay function which is built into unity, to see where on screen the user click. It stores that information in a RayCastHit variable and then
    //we assign the point to a Node class called mouseNode. Then I pass it into NodeFromWorldPoint where it converts it into a grid point, then we feed that into the
    //FindPath() function to find the distance between the player and point clicked on the map.
    public void playerMove()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Node mouseNode = grid.NodeFromWorldPoint(hit.point);//Passes in the hit to be converted 
            Debug.Log(mouseNode.worldPosition);
            FindPath(grid.NodeFromWorldPoint(player.position),mouseNode);//Start point is seeker position end point is mouse position
        }
   
    }

    //FindPath() is going to be the function you use to calculate the distance between two points on the grid. This is important though, before you use it make sure you're 
    //passing it in NODE classes and not vectors. If you need to convert a vector3 for it pass that into the gridFromWorldPoint() function on the gridscript.
    void FindPath(Node startNode, Node targetNode)
    {
        //Node startNode = grid.NodeFromWorldPoint(startPos);
        //Node targetNode = grid.NodeFromWorldPoint(targetPos);

        List<Node> openSet = new List<Node>(); //List of nodes from "OpenSet" which are nodes that have not been checked yet
        HashSet<Node> closedSet = new HashSet<Node>();//HashSet of nodes from "ClosedSet" which are nodes that have already been checked
        openSet.Add(startNode);//Adds the first node

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                //Checks the node being evalutated to see if it's fCost is lower or if the fCost is the same AND the hCost is lower
                //if so switches to that node
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }
            //Remove Current from the openset and add to the closed set
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            //If destination is found runs RetracePath function passing in both the start and target node
            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }

            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }
                //Calculates movement cost to neighbour using the gCost and GetDistance function
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;//Set new g cost
                    neighbour.hCost = GetDistance(neighbour, targetNode);//Calculates hCost with GetDistance function
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }

        }
    }

    //RetracePath() allows yous to retrace the path between one point to another, you wouldn't really use this function as it's called in FindPath(Function above) and it's
    //used to retrace the steps back from the path found.
    void RetracePath(Node startNode, Node EndNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = EndNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);//Adds current node to the path
            currentNode = currentNode.parent;//Parents node to retrace
            
        }

        //path.Reverse();//Reverses path as the path was retraced
        grid.path = path;
        //Debug.Log(grid.path[0].worldPosition);
        
     
    }

    //Calculates distance from nodes 
    int GetDistance(Node NodeA, Node NodeB)
    {
        int dstX = Mathf.Abs(NodeA.gridX - NodeB.gridX);
        int dstY = Mathf.Abs(NodeB.gridY - NodeA.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
