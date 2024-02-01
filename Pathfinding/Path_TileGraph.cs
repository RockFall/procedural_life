using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_TileGraph
{
    // This class constructs a simple path-finding compatible graph of our world.
    // Each tile is a node and each walkable connection between tiles is an edge.

    public Dictionary<Tile,Path_Node<Tile>> nodes;

    public Path_TileGraph(World world) {
        // We don't create nodes for non-floor tiles nither unwalkable (i.e. walls) ones. <<-- IMPORTANT

        nodes = new Dictionary<Tile, Path_Node<Tile>>();

        // Loop through all tiles of the world, creating a node for each one.
        for (int x = 0; x < world.Width; x++) {
            for (int y = 0; y < world.Height; y++) {
                // Get Tile
                Tile t = world.GetTileAt(x, y);
                //if (t.movementCost > 0) {   // Tiles with movementCost < 0 are unwalkable or empty
                    // Create node
                    Path_Node<Tile> n = new Path_Node<Tile>();
                    n.data = t;
                    // Add Tile/node pair to dictionary
                    nodes.Add(t, n);
                //}
            }
        }

        int edgeCount = 0; // For debug only

        // Loop through all nodes again creating edges for neighbours
        foreach (Tile t in nodes.Keys) {
            Path_Node<Tile> node = nodes[t];

            List<Path_Edge<Tile>> edges = new List<Path_Edge<Tile>>();

            // Get a list of neighbours for this tile.
            Tile[] neighbours = t.GetNeighbours(true); // NOTE: Some of the array spots could be null.

            //  If neighbour is walkable, create an edge.
            for (int i = 0; i < neighbours.Length; i++) {
                if (neighbours[i] != null && neighbours[i].movementCost > 0) {
                    // Neighbours exists and is walkable

                    if (IsCuttingCorner(t, neighbours[i])) {
                        continue;   // Skip to the next neighbour
                    }
                    //Creates edge
                    Path_Edge<Tile> edge = new Path_Edge<Tile>();
                    edge.cost = neighbours[i].movementCost;
                    edge.node = nodes[neighbours[i]];

                    //Add the edge to temporary list
                    edges.Add(edge);
                    edgeCount++; // Remove after debug
                    
                }
            }

            // Store edges in the node
            node.edges = edges.ToArray();
        }

        //Debug.Log(nodes.Count + " nodes and " + edgeCount + " edges created!");
    }

    bool IsCuttingCorner(Tile curr, Tile neigh) {
        if (Mathf.Abs(curr.X - neigh.X) == 1 && Mathf.Abs(curr.Y - neigh.Y) == 1) {
            // We are diagonals
            int dX = neigh.X - curr.X;
            int dY = neigh.Y - curr.Y;
            
            if (curr.world.GetTileAt(curr.X + dX, curr.Y).movementCost == 0) {
                // Cutting edge
                return true;
            }
            if (curr.world.GetTileAt(curr.X, curr.Y + dY).movementCost == 0) {
                // Cutting edge
                return true;
            }
        }
        return false;
    }
}
