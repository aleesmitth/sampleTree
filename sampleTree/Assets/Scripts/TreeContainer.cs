﻿using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class TreeContainer : MonoBehaviour {
    private ITreeNode root = default(ITreeNode);
    //the key is concatenation of x position + y position of the node.
    private Dictionary<string, GameObject> nodesDictionary;
    public GameObject treeNodePrefab;
    private GameObject lastInsertedNodeBuffer = default(GameObject);
    [SerializeField]
    public Material defaultNodeMat;

    private void Awake() {
        nodesDictionary = new Dictionary<string, GameObject>();
        root = new BSTNode {Value = -1, Position = new Vector2(-GameManager.TREE_X_OFFSET, GameManager.TREE_Y_OFFSET)};
    }

    private void OnEnable() {
        EventManager.onDeleteNode += DeleteNode;
    }

    private void OnDisable() {
        EventManager.onDeleteNode -= DeleteNode;
    }


    public void Insert(int value) {
        Vector2 insertedPosition = root.Position;
        Vector2 parentPosition = Vector2.zero;
        Dictionary<string, List<Vector2>> updateBalanceNodes = new Dictionary<string, List<Vector2>>();
        try {
            root.Insert(ref root, ref value, ref insertedPosition, ref parentPosition, updateBalanceNodes);
        }
        catch (HeightTreeLimitException e) {
            Debug.Log(e.Message);
            return;
        }
        catch (PositionOccupiedByNodeException e) {
            Debug.Log(e.Message);
            return;
        }
        //getting node from the pool and initializing it
        GameObject node = NodePool.instance.Get();
        node.transform.position = insertedPosition;
        node.transform.rotation = Quaternion.identity;
        node.GetComponentInChildren<TextMeshProUGUI>().text = value.ToString();
        
        //if list of nodes isn't empty, restores the color of the previous inserted node to normal, and draws branch to parent
        if (nodesDictionary.Count > 0) {
            lastInsertedNodeBuffer.GetComponentInChildren<MeshRenderer>().material = defaultNodeMat;
            LineRenderer lineRenderer = node.GetComponent<LineRenderer>();
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, insertedPosition);
            lineRenderer.SetPosition(1, parentPosition);
        }
        
        lastInsertedNodeBuffer = node;
        AddNodeToDictionary(node, insertedPosition);
        
        ExtraVisualUpdatesForAVL(updateBalanceNodes);
    }

    private void AddNodeToDictionary(GameObject node, Vector3 insertedPosition) {
        var key = MakeNodeKey(insertedPosition);
        nodesDictionary.Add(key, node);
    }

    public void PrintTree() {
        root.PrintTree(root);
    }

    private void DeleteNode(Vector3 nodePosition, TextMeshProUGUI textMeshProUGUI) {
        Dictionary<string, List<Vector2>> updateBalanceNodes = new Dictionary<string, List<Vector2>>();
        print(nodePosition);
        //deletes the node from the tree.
        Dictionary<string, string> textNodesUpdate = new Dictionary<string, string>();
        var node = root.SearchNode(nodePosition, int.Parse(textMeshProUGUI.text), root);
        root.DeleteNode(node, textNodesUpdate, out Vector2 deletedNodePosition, updateBalanceNodes);
        Debug.Log("OVERHERE\n");
        var deletedNodeKey = MakeNodeKey(deletedNodePosition);
        //
        //updates text of modified nodes.
        foreach(KeyValuePair<string, string> entry in textNodesUpdate) {
            nodesDictionary[entry.Key].GetComponentInChildren<TextMeshProUGUI>().text = entry.Value;
        }
        

        //deletes node from the scene pooling it back.
        NodePool.instance.DestroyObject(nodesDictionary[deletedNodeKey]); 
        //deletes the node from my active nodes list.
        nodesDictionary.Remove(deletedNodeKey);
        
        //TODO if i put this method before the deletes i get more errors with overlapping nodes
        //TODO but its more evident why it's crashing, if i leave it here it gets other errors,
        //TODO like inserting null node or accesing invalid key in dictionary.
        ExtraVisualUpdatesForAVL(updateBalanceNodes);
    }

    private void ExtraVisualUpdatesForAVL(Dictionary<string,List<Vector2>> updateBalanceNodes) {
        if (updateBalanceNodes.Count == 0) return;
        
        var updatedNodesGameObjects = new List<GameObject>();
        foreach (var updatedNode in updateBalanceNodes) {
            nodesDictionary[updatedNode.Key].transform.position = updatedNode.Value[0];
            updatedNodesGameObjects.Add(nodesDictionary[updatedNode.Key]);
            LineRenderer lineRenderer = nodesDictionary[updatedNode.Key].GetComponent<LineRenderer>();
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, updatedNode.Value[0]);
            lineRenderer.SetPosition(1, updatedNode.Value[1]);
            nodesDictionary.Remove(updatedNode.Key);
        }

        foreach (var nodeGameObject in updatedNodesGameObjects) {
            nodesDictionary.Add(MakeNodeKey(nodeGameObject.transform.position), nodeGameObject);
        }

        foreach (var updatedNode in updateBalanceNodes) {
            print("el de: " + updatedNode.Key + " pasa a: " + updatedNode.Value[0]);
        }
    }

    public void PrintListOfNodes() {
        foreach (var node in nodesDictionary) {
            print(node.Value.transform.position);
        }
    }

    public bool IsPositionOccupied(Vector2 position) {
        var key = MakeNodeKey(position);
        return nodesDictionary.ContainsKey(key);
    }

    ///makes key used to store unique node position
    public static string MakeNodeKey(Vector2 position) {
        return position.x.ToString(CultureInfo.InvariantCulture) + GameManager.MAGIC_KEY + position.y;
    }

    public void PrintHeight() {
        root.PrintHeight(root);
    }
    
    public void PrintDepth() {
        root.PrintDepth(root);
    }

    public void ChangeTreeType() {
        root = root.ChangeTreeType();
        foreach (var node in nodesDictionary) {
            //deletes node from the scene pooling it back.
            NodePool.instance.DestroyObject(node.Value); 
        }
        
        nodesDictionary.Clear();
        lastInsertedNodeBuffer = null;
    }
}