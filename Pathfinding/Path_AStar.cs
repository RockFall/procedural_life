using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using System.Linq;

public class Path_AStar
{
    Queue<Tile> path;

    public Path_AStar (World world, Tile startTile, Tile endTile) {

        if (world.tileGraph == null) {
            world.tileGraph = new Path_TileGraph(world);
        }

        // A dictionay of all valid, walkable nodes
        Dictionary<Tile, Path_Node<Tile>> nodes = world.tileGraph.nodes;

        // Make sure our start/end are in the list of nodes
        if (nodes.ContainsKey(startTile) == false) {
            Debug.LogError("Path_AStar: Starting tile isn't in the list of nodes!");
            return;
        }
        if (nodes.ContainsKey(endTile) == false) {
            Debug.LogError("Path_AStar: Ending tile isn't in the list of nodes!");
            return;
        }

        Path_Node<Tile> start = nodes[startTile];
        Path_Node<Tile> goal = nodes[endTile];

        // Mostly following this pseudocode:
        // https://en.wikipedia.org/wiki/A*_search_algorithm

        List<Path_Node<Tile>> ClosedSet = new List<Path_Node<Tile>>();

        /*
        List<Path_Node<Tile>> OpenSet = new List<Path_Node<Tile>>();
        OpenSet.Add(start);
        */

        SimplePriorityQueue< Path_Node <Tile>> OpenSet = new SimplePriorityQueue<Path_Node<Tile>> ();
        OpenSet.Enqueue(start, 0);

        // Dictionary of the pair Tile/Cheapest next tile.
        Dictionary<Path_Node<Tile>, Path_Node<Tile>> Came_From = new Dictionary<Path_Node<Tile>, Path_Node<Tile>>();

        // g_score[n] is the cost of the cheapest path from start to n currently known.
        Dictionary<Path_Node<Tile>, float> g_score = new Dictionary<Path_Node<Tile>, float>();
        foreach(Path_Node<Tile> node in nodes.Values) {
            g_score[node] = Mathf.Infinity; // Default value of infinity
        }
        g_score[start] = 0;

        // f_score[node] represents our current best guess as to
        // how short a path from start to finish can be if it goes through node.
        Dictionary<Path_Node<Tile>, float> f_score = new Dictionary<Path_Node<Tile>, float>();
        foreach (Path_Node<Tile> node in nodes.Values) {
            f_score[node] = Mathf.Infinity; // Default value of infinity
        }
        f_score[start] = heuristic_cost_estimate(start, goal);

        while (OpenSet.Count > 0) {
            Path_Node<Tile> current = OpenSet.Dequeue();
            if (current == goal) {
                // Goal achieved
                Repath(Came_From, current);
                return;
            }

            ClosedSet.Add(current);

            foreach (Path_Edge<Tile> neighbour in current.edges) {
                Path_Node<Tile> neighbourNode = neighbour.node;
                if (ClosedSet.Contains(neighbourNode)) {
                    continue; // Ignore this, already completed neighbour
                }
                float movement_cost_to_neighbour = DistBetween(current, neighbourNode) * neighbourNode.data.movementCost;
                float tentative_g_score = g_score[current] + movement_cost_to_neighbour;
                f_score[current] = g_score[current] + heuristic_cost_estimate(neighbourNode, goal);

                if (OpenSet.Contains(neighbourNode) && tentative_g_score >= g_score[current]) {
                    continue;
                }

                Came_From[neighbourNode] = current;
                g_score[neighbourNode] = tentative_g_score;
                f_score[neighbourNode] = g_score[neighbourNode] + heuristic_cost_estimate(neighbourNode, goal);

                if (OpenSet.Contains(neighbourNode) == false) {
                    OpenSet.Enqueue(neighbourNode, f_score[neighbourNode]);
                } else {
                    OpenSet.UpdatePriority(neighbourNode, f_score[neighbourNode]);
                }
            } // End of foreach neighbour
        } // End of While

        // If we got to here, there is no avaible path from start to goal
        return;
    }

    float heuristic_cost_estimate(Path_Node<Tile> start, Path_Node<Tile> goal) {
        return Mathf.Sqrt(Mathf.Pow(start.data.X - goal.data.X, 2) + Mathf.Pow(start.data.Y - goal.data.Y, 2));
    }

    float DistBetween(Path_Node<Tile> a, Path_Node<Tile> b) {

        // Hori/Vert neighbours have a distance of 1.
        // Diag neighbours have a distance of 1.41421356237.

        if (Mathf.Abs(a.data.X - b.data.X) + Mathf.Abs(a.data.Y - b.data.Y) == 1)
            return 1f;
        if ((Mathf.Abs(a.data.X - b.data.X) == 1 && Mathf.Abs(a.data.Y - b.data.Y) == 1))
            return 1.41421356237f;
        else {
            //Debug.LogError("Path_AStar:: DistBetween -- Not a neighbour node");
            return Mathf.Sqrt(Mathf.Pow(a.data.X - b.data.X, 2) + Mathf.Pow(a.data.Y - b.data.Y, 2));
        }
    }

    public Tile Dequeue() {
        return path.Dequeue();
    }

    void Repath(Dictionary<Path_Node<Tile>, Path_Node<Tile>> Came_From, Path_Node<Tile> current) {
        // At this point, current is the goal, we will walk backwards through the Came_From map
        // until we reach the starting node.

        Queue<Tile> total_path = new Queue<Tile>();
        total_path.Enqueue(current.data); // Add goal to the queue

        while (Came_From.ContainsKey(current)) {
            current = Came_From[current];
            total_path.Enqueue(current.data); // Add every node in the path to queue
        }

        path = new Queue<Tile>(total_path.Reverse());
    }

    public int Lenght() {
        if (path == null)
            return 0;
        return path.Count;
    }
}
